using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpForce = 8f;
    public float gravity = -20f;

    [Header("Mouse Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 2.5f;
    public float maxLookAngle = 80f;

    [Header("Controls")]
    public KeyCode sprintKey = KeyCode.LeftShift;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Move();
        MouseLook();
    }

    void Move()
    {
        // Get input
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Check for sprinting
        float currentSpeed = Input.GetKey(sprintKey) ? runSpeed : walkSpeed;

        // Convert input to world movement
        Vector3 move = transform.right * x + transform.forward * z;
        move = move.normalized * currentSpeed;

        // Apply gravity
        if (controller.isGrounded)
        {
            velocity.y = -2f;
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = jumpForce;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Move controller
        controller.Move((move + velocity) * Time.deltaTime);
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player (y-axis)
        transform.Rotate(Vector3.up * mouseX);

        // Camera pitch (up/down)
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
    }
}