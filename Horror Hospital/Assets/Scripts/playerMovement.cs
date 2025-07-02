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
    [SerializeField] private float runFovIncrease = 6f;   // added when detected
    [SerializeField] private float fovSmoothSpeed = 10f;

    // runtime-adjustable smoothing speed for FOV transitions
    private float currentFovSmoothSpeed;

    // look-at target when external scripts want the player to face a point
    private Vector3? lookTarget;
    private float lookSmoothSpeed;

    /* ────────────── Runtime ────────────── */
    private CharacterController controller;
    private Camera cam;
    private float baseFov;
    private float fovOffset = 0f;  // additional FOV offset, e.g. for zooming
    private float cameraPitch;
    private Vector3 velocity;
    public bool CanMove { get; set; } = true;  // no idea what im doing, but it works

    [HideInInspector] public bool isDetected = false;  // toggled by other scripts
    public bool IsDetected { get => isDetected; set => isDetected = value; }

    public void SetFovOffset(float offset)
    {
        fovOffset = offset;
    }

    public void SetFovSmoothSpeed(float speed)
    {
        currentFovSmoothSpeed = speed;
    }

    public void ResetFovSmoothSpeed()
    {
        currentFovSmoothSpeed = fovSmoothSpeed;
    }

    public void SmoothLookAtPoint(Vector3 worldPoint, float speed)
    {
        if (playerCamera == null)
            return;

        lookTarget = worldPoint;
        lookSmoothSpeed = speed;
    }

    public void ClearLookTarget()
    {
        lookTarget = null;
    }

    public void LookAtPoint(Vector3 worldPoint)
    {
        if (playerCamera == null)
            return;

        Vector3 dir = worldPoint - playerCamera.position;
        if (dir.sqrMagnitude < 0.001f)
            return;

        // horizontal rotation (yaw)
        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude > 0.0001f)
        {
            Quaternion yawRot = Quaternion.LookRotation(flatDir);
            transform.rotation = Quaternion.Euler(0f, yawRot.eulerAngles.y, 0f);
        }

        // vertical rotation (pitch)
        Quaternion lookRot = Quaternion.LookRotation(dir);
        float pitch = lookRot.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        cameraPitch = Mathf.Clamp(-pitch, -maxLookAngle, maxLookAngle);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
    }

    /* ────────────── Unity Callbacks ────────────── */
    void Awake()
    {
        /* --- singleton setup (safe, optional) --- */
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        controller = GetComponent<CharacterController>();
        currentFovSmoothSpeed = fovSmoothSpeed;

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
        HandleLookTarget();
    }

    /* ────────────── Movement ────────────── */
    private void HandleMovement()
    {
        float x = CanMove ? Input.GetAxisRaw("Horizontal") : 0f;
        float z = CanMove ? Input.GetAxisRaw("Vertical") : 0f;

        // desired direction in local space (no Y)
        Vector3 inputDir = new Vector3(x, 0f, z).normalized;
        bool wantsToRun = CanMove && Input.GetKey(sprintKey) && inputDir.sqrMagnitude > 0.01f;

        /* --- FOV change (apply even when movement disabled) --- */
        if (cam)
        {
            float targetFov = baseFov + fovOffset;

            // when the player is detected, widen FOV for a scare effect
            if (isDetected)
                targetFov += runFovIncrease;

            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * currentFovSmoothSpeed);
        }

        if (!CanMove)
        {
            velocity = Vector3.zero;
            return;
        }

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

        controller.Move((move + Vector3.up * velocity.y) * Time.deltaTime);
    }

    /* ────────────── Mouse Look ────────────── */
    private void HandleMouseLook()
    {
        // only rotate camera when the cursor is locked (e.g. not in a dialogue UI)
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        // yaw
        transform.Rotate(Vector3.up * mouseX);

        // pitch
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        if (playerCamera) playerCamera.localEulerAngles = Vector3.right * cameraPitch;
    }

    private void HandleLookTarget()
    {
        if (!lookTarget.HasValue || playerCamera == null)
            return;

        Vector3 dir = lookTarget.Value - playerCamera.position;
        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);

        Quaternion targetRot = transform.rotation;
        if (flatDir.sqrMagnitude > 0.0001f)
        {
            Quaternion yawRot = Quaternion.LookRotation(flatDir);
            targetRot = Quaternion.Euler(0f, yawRot.eulerAngles.y, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, lookSmoothSpeed * Time.deltaTime);
        }

        Quaternion lookRot = Quaternion.LookRotation(dir);
        float pitch = lookRot.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        float targetPitch = Mathf.Clamp(-pitch, -maxLookAngle, maxLookAngle);
        cameraPitch = Mathf.MoveTowards(cameraPitch, targetPitch, lookSmoothSpeed * Time.deltaTime);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;

        bool yawDone = Quaternion.Angle(transform.rotation, targetRot) < 0.1f;
        bool pitchDone = Mathf.Abs(cameraPitch - targetPitch) < 0.1f;
        if (yawDone && pitchDone)
            lookTarget = null;
    }
}