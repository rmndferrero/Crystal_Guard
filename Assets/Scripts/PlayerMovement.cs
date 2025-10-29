using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public Transform groundCheck;

    public float dashForce = 20f;
    public float dashCooldown = 1.5f;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector2 moveInput;
    private bool isGrounded;
    private float cooldownTimer = 0f;
    private bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        CheckGround();

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        MovePlayer();
    }

    public void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded && cooldownTimer <= 0)
        {
            if (moveInput.magnitude > 0.1f)
            {
                StartDash();
            }
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void MovePlayer()
    {
        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        direction.Normalize();
        rb.linearVelocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);
    }

    void StartDash()
    {
        cooldownTimer = dashCooldown;
        isDashing = true;

        Vector3 dashDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        dashDirection.Normalize();

        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);

        Invoke("EndDash", 0.1f);
    }

    void EndDash()
    {
        isDashing = false;
    }
}