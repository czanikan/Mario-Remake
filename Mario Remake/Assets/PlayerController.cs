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
    [SerializeField] float jumpTime = 2f;
    [SerializeField] float fallMultiplier = 1.5f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] LayerMask groundMask;
    private float jumpTimeCounter;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;

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
            // Reduce movement speed when not on the ground
            float airControlFactor = 0.5f; 
            moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, airControlFactor * Time.deltaTime);
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);

            anim.speed = 1f;
        }


        controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);

        horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        speed = horizontalVelocity.magnitude / 5f;

        Debug.Log(horizontalVelocity);

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

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpTimeCounter = jumpTime;
            anim.SetTrigger("Jump");
            Instantiate(jumpDust, transform.position, Quaternion.identity);
            anim.speed = 1;
        }

        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }

        velocity.y += gravity * (velocity.y < 0 ? fallMultiplier : 1f) * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        anim.SetBool("isGrounded", isGrounded);
    }

}
