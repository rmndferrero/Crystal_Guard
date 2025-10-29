using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = new PlayerInput();

        playerInput.OnFoot.Enable();
    }

    void OnEnable()
    {
        playerInput.OnFoot.Enable();
    }

    void OnDisable()
    {
        playerInput.OnFoot.Disable();
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

    void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnDash()
    {
        if (isGrounded && cooldownTimer <= 0)
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
        Vector3 targetVelocity = direction.normalized * moveSpeed;

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    void StartDash()
    {
        cooldownTimer = dashCooldown;
        Vector3 dashDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
    }
}