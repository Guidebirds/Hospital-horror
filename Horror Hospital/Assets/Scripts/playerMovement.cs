using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    /* ────────────── Singleton (optional) ────────────── */
    public static PlayerMovement Instance { get; private set; }

    /* ────────────── Inspector ────────────── */
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float detectedRunSpeed = 15f;
    [SerializeField] private float jumpHeight = 2f;      // height, not force
    [SerializeField] private float gravity = -9.81f;

    [Header("Mouse Look")]
    [SerializeField] public Transform playerCamera;
    [SerializeField] private float horizontalSensitivity = 2.5f;
    [SerializeField] private float verticalSensitivity = 2.5f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Camera FOV")]
    [SerializeField] private float runFovIncrease = 5f;   // only when detected
    [SerializeField] private float fovSmoothSpeed = 10f;

    /* ────────────── Runtime ────────────── */
    private CharacterController controller;
    private Camera cam;
    private float baseFov;
    private float cameraPitch;
    private Vector3 velocity;

    [HideInInspector] public bool isDetected = false;  // toggled by other scripts
    public bool IsDetected { get => isDetected; set => isDetected = value; }

    /* ────────────── Unity Callbacks ────────────── */
    void Awake()
    {
        /* --- singleton setup (safe, optional) --- */
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        controller = GetComponent<CharacterController>();

        if (playerCamera)
        {
            cam = playerCamera.GetComponent<Camera>();
            baseFov = cam ? cam.fieldOfView : 60f;
        }
        else
        {
            baseFov = 60f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    /* ────────────── Movement ────────────── */
    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // desired direction in local space (no Y)
        Vector3 inputDir = new Vector3(x, 0f, z).normalized;
        bool wantsToRun = Input.GetKey(sprintKey) && inputDir.sqrMagnitude > 0.01f;

        float speed = walkSpeed;
        if (wantsToRun)
            speed = isDetected ? detectedRunSpeed : runSpeed;

        Vector3 move = (transform.right * x + transform.forward * z).normalized * speed;

        /* --- jumping & gravity --- */
        if (controller.isGrounded)
        {
            velocity.y = velocity.y < 0 ? -2f : velocity.y;   // stick to ground

            if (Input.GetButtonDown("Jump"))
                velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity); // v = √(2gh)
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        /* --- FOV change only when sprinting & detected --- */
        if (cam)
        {
            float targetFov = (wantsToRun && isDetected) ? baseFov + runFovIncrease : baseFov;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * fovSmoothSpeed);
        }

        controller.Move((move + Vector3.up * velocity.y) * Time.deltaTime);
    }

    /* ────────────── Mouse Look ────────────── */
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        // yaw
        transform.Rotate(Vector3.up * mouseX);

        // pitch
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        if (playerCamera) playerCamera.localEulerAngles = Vector3.right * cameraPitch;
    }
}