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

    private Vector2 moveInput;

    // Input state
    private bool jumpHeld;

    // Charge state
    private bool isCharging;
    private float chargeTime;

    private float defaultGravityScale;

    private void Awake()
    {
        //rb = visual.GetComponent<Rigidbody2D>();
        //if (rb == null) 
        rb = GetComponent<Rigidbody2D>();
        keyMan = KeybindUnlockManager.Instance;
        defaultGravityScale = rb.gravityScale;
        
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
    }

    private void FixedUpdate()
    {
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
        Debug.Log(keyMan.IsUnlocked(PlayerAction.Jump));
        if(!keyMan.IsUnlocked(PlayerAction.Jump)) return;
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
                PerformChargedJump();
            }
            else
            {
                // If not charging/grounded, just reset
                isCharging = false;
                chargeTime = 0f;
            }
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