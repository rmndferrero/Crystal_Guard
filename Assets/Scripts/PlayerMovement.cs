using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public Transform groundCheck;

    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.5f;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector2 moveInput;
    private bool isGrounded;
    private float cooldownTimer = 0f;
    private bool isDashing = false;

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

        if (!isDashing)
        {
            MovePlayer();
        }
    }

    void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnDash()
    {
        if (isGrounded && cooldownTimer <= 0 && !isDashing)
        {
            if (moveInput.magnitude > 0.1f)
            {
                StartCoroutine(StartDash());
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

    IEnumerator StartDash()
    {
        isDashing = true;
        cooldownTimer = dashCooldown;

        Vector3 dashDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            float verticalSpeed = rb.linearVelocity.y;

            if (verticalSpeed > 0)
            {
                verticalSpeed = 0;
            }

            rb.linearVelocity = new Vector3(dashDirection.x * dashSpeed, verticalSpeed, dashDirection.z * dashSpeed);
            yield return null;
        }

        isDashing = false;
    }
}