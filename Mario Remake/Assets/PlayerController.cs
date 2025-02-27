using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] GameObject jumpDust;
    [SerializeField] ParticleSystem footDust;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float deceleration = 15f;
    [SerializeField] float skidDeceleration = 30f;
    private float currentSpeed;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 previousDirection = Vector3.zero;
    private bool isSkidding = false;

    [Header("Jumping")]
    [SerializeField] float jumpForce = 2f;
    [SerializeField] float firstJumpForce = 2f;
    [SerializeField] float secondJumpForce = 2.5f;
    [SerializeField] float thirdJumpForce = 3.5f;
    [SerializeField] float jumpComboTime = 0.2f;
    [SerializeField] float jumpTime = 2f;
    [SerializeField] float fallMultiplier = 1.5f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] LayerMask groundMask;
    private float jumpTimeCounter;
    private float lastGroundedTime;
    private int jumpCount = 0;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;
    private bool canComboJump = false;

    [Header("Camera")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Animation")]
    [SerializeField] float baseSpeed = 1f;
    private float speed;
    private Vector3 horizontalVelocity;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        jumpTimeCounter = jumpTime;
    }

    void Update()
    {
        HandleMovement();
        HandleJumping();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        bool isChangingDirection = Vector3.Dot(previousDirection, inputDirection) < -0.4f && currentSpeed > moveSpeed * 0.5f;
        bool isStopping = inputDirection.magnitude < 0.1f && currentSpeed > moveSpeed * 0.5f;

        if (isGrounded)
        {
            if ((isChangingDirection || isStopping) && !isSkidding)
            {
                isSkidding = true;
                anim.SetTrigger("Skid");
            }

            if (isSkidding)
            {
                currentSpeed -= skidDeceleration * Time.deltaTime;

                if (currentSpeed <= moveSpeed * 0.2f)
                {
                    isSkidding = false;
                    anim.ResetTrigger("Skid");
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
                    currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * Time.deltaTime);

                    anim.speed = Mathf.Max(speed / baseSpeed, 0.5f);

                }
                else
                {
                    currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
                    anim.speed = 1f;
                }
            }
        }
        else // Limit movement when in air
        {
            float airControlFactor = 0.5f; 
            moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, airControlFactor * Time.deltaTime);
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);

            anim.speed = 1f;
        }


        controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);

        horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        speed = horizontalVelocity.magnitude / 5f;

        anim.SetFloat("Speed", controller.velocity.magnitude);
        anim.SetBool("isGrounded", isGrounded);

        if (inputDirection.magnitude > 0.1f)
            previousDirection = inputDirection;
    }

    void HandleJumping()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (isGrounded)
        {
            if (!canComboJump) 
            {
                canComboJump = true;
                lastGroundedTime = Time.time;
            }
        }
        else
        {
            canComboJump = false;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if (Time.time - lastGroundedTime > jumpComboTime)
            {
                jumpCount = 0; 
            }

            jumpCount = Mathf.Clamp(jumpCount + 1, 1, 3);

            float jumpStrength = jumpCount == 1 ? firstJumpForce :
                                 jumpCount == 2 ? secondJumpForce : thirdJumpForce;

            velocity.y = Mathf.Sqrt(jumpStrength * -2f * gravity);
            isJumping = true;
            lastGroundedTime = Time.time;

            if (jumpCount == 1) anim.SetTrigger("Jump");
            else if (jumpCount == 2) anim.SetTrigger("DoubleJump");
            else if (jumpCount == 3) anim.SetTrigger("TripleJump");

            if (jumpCount == 1) Debug.Log("Jump");
            else if (jumpCount == 2) Debug.Log("DoubleJump");
            else if (jumpCount == 3) Debug.Log("TripleJump");

            Instantiate(jumpDust, transform.position, Quaternion.identity);
        }

        velocity.y += gravity * (velocity.y < 0 ? fallMultiplier : 1f) * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        anim.SetBool("isGrounded", isGrounded);
    }



}
