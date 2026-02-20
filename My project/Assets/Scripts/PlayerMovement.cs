using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float jumpCutMultiplier = 0.5f; // smaller = shorter tap jump

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpHeld;

    private void Awake()
    {
        //rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Jump on next physics step
        if (jumpPressed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
        }

        // Variable jump height (release early -> cut upward velocity)
        if (!jumpHeld && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        Debug.Log(IsGrounded());
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
        Debug.Log("Jump");
        if (ctx.started) jumpPressed = true;
        if (ctx.performed) jumpHeld = true;
        if (ctx.canceled) jumpHeld = false;
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