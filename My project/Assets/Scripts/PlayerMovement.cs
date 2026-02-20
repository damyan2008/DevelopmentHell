using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Charged Jump")]
    [Tooltip("Minimum jump height (world units) when you tap/release quickly.")]
    [SerializeField] private float minJumpHeight = 1.5f;

    [Tooltip("Maximum jump height (world units) when fully charged.")]
    [SerializeField] private float maxJumpHeight = 5.0f;

    [Tooltip("Time (seconds) to reach max jump height.")]
    [SerializeField] private float maxChargeTime = 2.0f;

    [Tooltip("You must charge at least this fraction of maxChargeTime before a jump will fire (e.g., 0.33 = one third).")]
    [Range(0f, 1f)]
    [SerializeField] private float minChargeFractionToJump = 0.33f;

    [Tooltip("Optional: how much horizontal control you keep while charging (1 = full, 0 = none).")]
    [Range(0f, 1f)]
    [SerializeField] private float chargeMoveMultiplier = 1.0f;

    [Header("Jump - Hang (Apex)")]
    [SerializeField] private float jumpHangVelocityThreshold = 1.0f;
    [SerializeField] private float jumpHangGravityMultiplier = 0.5f;

    [Header("Gravity Tuning")]
    [SerializeField] private float fallGravityMultiplier = 1.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private GameObject visual;

    private Rigidbody2D rb;
    private KeybindUnlockManager keyMan;
    private ProgressionHandler progMan;

    private Vector2 moveInput;

    // Input state
    private bool jumpHeld;

    // Charge state
    private bool isCharging;
    private float chargeTime;

    private float defaultGravityScale;

    [Header("Crouch")]
    [Tooltip("Visual Y scale multiplier while crouching. 1 = no crouch, 0.5 = half height.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float crouchScaleY = 0.55f;

    [Tooltip("How fast the visual transitions into/out of crouch.")]
    [SerializeField] private float crouchTransitionSpeed = 14f;

    private bool crouchHeld;
    private float crouchFactor; // 0 = standing, 1 = crouched

    private Vector3 visualBaseLocalScale;
    private Vector3 visualBaseLocalPos;
    private float visualBaseHeightWorld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        keyMan = KeybindUnlockManager.Instance;
        progMan = ProgressionHandler.Instance;
        defaultGravityScale = rb.gravityScale;

        CacheVisualCrouchData();
    }

    private void Update()
    {
        bool grounded = IsGrounded();

        // Start/continue charging only while grounded
        if (grounded && jumpHeld)
        {
            isCharging = true;
            chargeTime = Mathf.Min(chargeTime + Time.deltaTime, maxChargeTime);
        }

        // If we leave the ground, stop charging (prevents oddities)
        if (!grounded && isCharging)
        {
            isCharging = false;
            chargeTime = 0f;
        }

        ApplyJumpGravityTuning(grounded);
        ApplyCrouchVisual();
    }

    private void FixedUpdate()
    {
        //Debug.Log(progMan.HasHeldItem);
        float moveMult = (isCharging ? chargeMoveMultiplier : 1f);
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed * moveMult, rb.linearVelocity.y);
    }

    private void PerformChargedJump()
    {
        // Convert charge [0..maxChargeTime] -> [0..1]
        float t = (maxChargeTime <= 0f) ? 1f : Mathf.Clamp01(chargeTime / maxChargeTime);

        // Map to height range. Use SmoothStep for nicer ramp; swap to 't' for linear.
        float eased = Mathf.SmoothStep(0f, 1f, t);
        float height = Mathf.Lerp(minJumpHeight, maxJumpHeight, eased);

        float jumpVelocity = CalculateJumpVelocity(height);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

        // Reset charge
        isCharging = false;
        chargeTime = 0f;
    }

    private void ApplyJumpGravityTuning(bool grounded)
    {
        float targetGravityScale = defaultGravityScale;

        if (!grounded)
        {
            float vy = rb.linearVelocity.y;

            if (vy < 0f)
            {
                targetGravityScale = defaultGravityScale * fallGravityMultiplier;
            }
            else
            {
                if (jumpHeld && Mathf.Abs(vy) <= jumpHangVelocityThreshold)
                {
                    targetGravityScale = defaultGravityScale * jumpHangGravityMultiplier;
                }
            }
        }

        rb.gravityScale = targetGravityScale;
    }

    private float CalculateJumpVelocity(float desiredHeight)
    {
        float g = Mathf.Abs(Physics2D.gravity.y) * defaultGravityScale;
        return Mathf.Sqrt(2f * g * desiredHeight);
    }

    private bool IsGrounded()
    {
        if (groundCheckPoint == null) return false;
        return Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);
    }

    // --- New Input System callbacks ---
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        //Debug.Log(keyMan.IsUnlocked(PlayerAction.Jump));
        //if (!keyMan.IsUnlocked(PlayerAction.Jump)) return;
        //Debug.Log(progMan.HasUpgrade(PlayerAction.Jump));
        if(!progMan.HasUpgrade(PlayerAction.Jump)) return;

        // Hold begins
        if (ctx.started)
        {
            jumpHeld = true;

            // If grounded, begin charging immediately
            if (IsGrounded())
            {
                isCharging = true;
                chargeTime = 0f;
            }
        }

        // Hold ends -> release to jump (if we were charging and grounded)
        if (ctx.canceled)
        {
            jumpHeld = false;

            if (isCharging && IsGrounded())
            {
                // Require at least a minimum charge before allowing the jump.
                float t = (maxChargeTime <= 0f) ? 1f : Mathf.Clamp01(chargeTime / maxChargeTime);
                if (t >= minChargeFractionToJump)
                {
                    PerformChargedJump();
                }
                else
                {
                    // Not charged enough -> no jump.
                    isCharging = false;
                    chargeTime = 0f;
                }
            }
            else
            {
                // If not charging/grounded, just reset
                isCharging = false;
                chargeTime = 0f;
            }
        }
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        //if(!keyMan.IsUnlocked(PlayerAction.Crouch)) return;
        if(!progMan.HasUpgrade(PlayerAction.Crouch)) return;
        // Button-style action: pressed -> crouch, released -> stand.
        if (ctx.started || ctx.performed)
        {
            crouchHeld = true;
        }
        else if (ctx.canceled)
        {
            crouchHeld = false;
        }
    }

    private void CacheVisualCrouchData()
    {
        if (visual == null) return;

        Transform vt = visual.transform;
        visualBaseLocalScale = vt.localScale;
        visualBaseLocalPos = vt.localPosition;

        // Use a renderer to estimate the visual's height in world units so we can
        // offset the visual down as it scales, keeping the "feet" planted.
        var sr = visual.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            visualBaseHeightWorld = sr.bounds.size.y;
            return;
        }

        var r = visual.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            visualBaseHeightWorld = r.bounds.size.y;
        }
    }

    private void ApplyCrouchVisual()
    {
        if (visual == null) return;


        // You cannot stay crouched while airborne. If the input is held mid-air,
        // crouch will resume automatically once grounded again.
        bool grounded = IsGrounded();
        bool canCrouchNow = crouchHeld && grounded;
        // Smoothly move between standing and crouched.
        float target = canCrouchNow ? 1f : 0f;
        crouchFactor = Mathf.MoveTowards(crouchFactor, target, crouchTransitionSpeed * Time.deltaTime);

        float yMult = Mathf.Lerp(1f, crouchScaleY, crouchFactor);

        Transform vt = visual.transform;
        vt.localScale = new Vector3(visualBaseLocalScale.x, visualBaseLocalScale.y * yMult, visualBaseLocalScale.z);

        // Keep the bottom anchored by shifting the visual downward by half the removed height.
        // Convert the world-unit delta into local units (approx) via parent lossyScale.
        if (visualBaseHeightWorld > 0f)
        {
            float removedHeightWorld = visualBaseHeightWorld * (1f - yMult);
            float parentScaleY = Mathf.Abs(transform.lossyScale.y);
            if (parentScaleY < 0.0001f) parentScaleY = 1f;

            float offsetLocalY = -(removedHeightWorld * 0.5f) / parentScaleY;
            vt.localPosition = new Vector3(
                visualBaseLocalPos.x,
                visualBaseLocalPos.y + offsetLocalY,
                visualBaseLocalPos.z
            );
        }
        else
        {
            // Fallback: if no renderer is found, at least preserve original position.
            vt.localPosition = visualBaseLocalPos;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
    }
#endif
}