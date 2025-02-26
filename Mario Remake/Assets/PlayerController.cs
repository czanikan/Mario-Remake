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
    [SerializeField] float fallMultiplier = 1.5f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] LayerMask groundMask;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Camera")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Animation")]
    [SerializeField] float baseSpeed = 1f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        bool isChangingDirection = Vector3.Dot(previousDirection, inputDirection) < -0.3f && currentSpeed > moveSpeed * 0.5f;
        bool isStopping = inputDirection.magnitude < 0.1f && currentSpeed > moveSpeed * 0.5f;

        if (isGrounded)
        {
            if (isChangingDirection || isStopping)
            {
                if (!isSkidding && currentSpeed > moveSpeed * 0.5f)
                {
                    isSkidding = true;
                    anim.SetTrigger("Skid");
                }
                
            }

            if (isSkidding)
            {
                currentSpeed -= deceleration * 3f * Time.deltaTime;

                if (currentSpeed <= moveSpeed * 0.2f || inputDirection.magnitude > 0.1f)
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

                    Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
                    float speed = horizontalVelocity.magnitude / 5f;

                    anim.speed = Mathf.Max(speed / baseSpeed, 0.1f);
                }
                else
                {
                    currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
                }
            }
        }

        controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);

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
            anim.SetBool("isGrounded", true);
        }
        else
        {
            anim.SetBool("isGrounded", false);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            anim.SetTrigger("Jump");

            Instantiate(jumpDust, transform.position, Quaternion.identity);
        }

        velocity.y += gravity * (velocity.y < 0 ? fallMultiplier : 1f) * Time.deltaTime;

        controller.Move((moveDirection.normalized * currentSpeed + new Vector3(0, velocity.y, 0)) * Time.deltaTime);
    }
}
