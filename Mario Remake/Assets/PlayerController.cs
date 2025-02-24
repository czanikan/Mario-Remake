using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    [SerializeField] Animator anim;
    [SerializeField] GameObject jumpDust;
    [SerializeField] ParticleSystem footDust;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float runSpeed = 8f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    private float currentSpeed;
    private Vector3 moveDirection;

    [Header("Jumping")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Camera")]
    public Transform cameraTransform;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Animation")]
    public float baseSpeed = 1f;

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
        isGrounded = controller.isGrounded;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Rotate towards movement direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Calculate movement direction
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Accelerate and decelerate movement
            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;
            if (currentSpeed < targetSpeed)
                currentSpeed += acceleration * Time.deltaTime;
            else
                currentSpeed -= deceleration * Time.deltaTime;

            currentSpeed = Mathf.Clamp(currentSpeed, 0, targetSpeed);
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);

            footDust.enableEmission = true;

            // Adjust animation speed dynamically

            Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            float speed = horizontalVelocity.magnitude / 5f;

            anim.speed = Mathf.Max(speed / baseSpeed, 0.1f);
        }
        else
        {
            // Decelerate when no input
            currentSpeed = Mathf.Max(0, currentSpeed - deceleration * Time.deltaTime);
            anim.speed = baseSpeed;

            footDust.enableEmission = false;
        }

        anim.SetFloat("Speed", controller.velocity.magnitude);
        anim.SetBool("isGrounded", isGrounded);
    }

    void HandleJumping()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to stick to ground
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            anim.SetTrigger("Jump");
            Instantiate(jumpDust, transform.position, Quaternion.identity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
