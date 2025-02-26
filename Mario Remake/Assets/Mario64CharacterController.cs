using UnityEngine;

public class Mario64CharacterController : MonoBehaviour
{
    public float maxMoveSpeed = 6f;
    public float acceleration = 12f;
    public float deceleration = 18f;
    public float jumpForce = 8f;
    public float gravity = -25f;
    public float fallMultiplier = 1.5f;
    public float turnSmoothTime = 0.1f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float turnSmoothVelocity;
    private Vector3 moveDirection;
    private float currentSpeed = 0f;
    private bool isSkidding = false;
    private Vector3 previousDirection = Vector3.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        bool isChangingDirection = Vector3.Dot(previousDirection, inputDirection) < -0.3f && currentSpeed > maxMoveSpeed * 0.5f;
        bool isStopping = inputDirection.magnitude < 0.1f && currentSpeed > maxMoveSpeed * 0.5f;

        if (isGrounded)
        {
            if ((isChangingDirection || isStopping) && !isSkidding && currentSpeed > maxMoveSpeed * 0.5f)
            {
                isSkidding = true;
            }
        }

        if (isSkidding)
        {
            currentSpeed -= deceleration * 2f * Time.deltaTime;
            if (currentSpeed <= maxMoveSpeed * 0.2f || inputDirection.magnitude > 0.1f)
            {
                isSkidding = false;
            }
        }
        else
        {
            if (inputDirection.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxMoveSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            }
        }

        controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
        if (inputDirection.magnitude > 0.1f)
            previousDirection = inputDirection;
    }

    void HandleJump()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        if (velocity.y < 0)
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
