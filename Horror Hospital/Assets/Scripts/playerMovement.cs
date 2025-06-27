using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float detectedRunSpeed = 15f;
    public float jumpHeight = 2f;     // height, not force
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public Transform playerCamera;
    public float horizontalSensitivity = 2.5f;
    public float verticalSensitivity = 2.5f;
    public float maxLookAngle = 80f;

    [Header("Controls")]
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Camera FOV")]
    public float runFovIncrease = 5f;   // Only applied when detected
    public float fovSmoothSpeed = 10f;

    private Camera cam;
    private float baseFov;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch = 0f;
    private bool isJumping = false;
    [HideInInspector] public bool isDetected = false; // set by other scripts

    // Property wrapper so other scripts can toggle detection state
    public bool IsDetected
    {
        get => isDetected;
        set => isDetected = value;
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera != null)
        {
            cam = playerCamera.GetComponent<Camera>();
            if (cam != null)
                baseFov = cam.fieldOfView;
        }

        if (cam == null)
            baseFov = 60f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Move();
        MouseLook();
    }

    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Direction the player wants to move in local space (no y component)
        Vector3 inputDir = new Vector3(x, 0f, z).normalized;

        // Sprint input – allowed at all times
        bool wantsToRun = Input.GetKey(sprintKey) && inputDir.sqrMagnitude > 0.01f;

        // Pick the speed: walk, run, or detected‑run
        float currentSpeed = walkSpeed;
        if (wantsToRun)
            currentSpeed = isDetected ? detectedRunSpeed : runSpeed;

        Vector3 move = (transform.right * x + transform.forward * z).normalized * currentSpeed;

        // Jumping & gravity
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f; // stick to ground

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity); // v = √(2gh)
                isJumping = true;
            }
            else
            {
                isJumping = false;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // FOV change only when sprinting AND detected
        if (cam != null)
        {
            float desiredFov = baseFov;
            if (wantsToRun && isDetected)
                desiredFov += runFovIncrease;

            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFov, Time.deltaTime * fovSmoothSpeed);
        }

        // Apply movement (CharacterController expects motion per‑frame)
        Vector3 motion = move + Vector3.up * velocity.y;
        controller.Move(motion * Time.deltaTime);
    }

    private void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        // Yaw rotates the whole player
        transform.Rotate(Vector3.up * mouseX);

        // Pitch only rotates the camera
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
    }
}