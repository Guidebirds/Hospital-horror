using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpHeight = 2f; // use height instead of force
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
    private bool isJumping = false;

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
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        float currentSpeed = Input.GetKey(sprintKey) ? runSpeed : walkSpeed;
        Vector3 move = transform.right * x + transform.forward * z;
        move = move.normalized * currentSpeed;

        // Use a more realistic jump formula
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f; // keep grounded

            if (Input.GetButtonDown("Jump"))
            {
                // v = sqrt(2 * h * -g)
                velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            }
        }
        else
        {
            // Allow a bit of hang time at the top of the jump
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move((move + velocity) * Time.deltaTime);
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
    }
}