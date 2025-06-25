using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundDistance = 0.1f;
    public LayerMask groundMask;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        // Lock cursor to screen center
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();

        // Jump input in Update for responsiveness
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    /// <summary>
    /// Handles walking and running based on Rigidbody movement.
    /// </summary>
    void HandleMovement()
    {
        // Check if player is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Move via Rigidbody
        Vector3 targetPos = rb.position + move * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);
    }

    /// <summary>
    /// Applies an impulse force to make the player jump.
    /// </summary>
    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Handles camera rotation based on mouse input.
    /// </summary>
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Vertical look
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal look
        transform.Rotate(Vector3.up * mouseX);
    }
}
