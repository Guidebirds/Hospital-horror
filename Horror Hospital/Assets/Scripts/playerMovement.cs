using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpHeight = 2f; // use height instead of force
    public float gravity = -20f;
    public float timeToMaxSpeed = 0.5f; // seconds to reach full speed
    public float deceleration = 25f;

    [Header("Mouse Look")]
    public Transform playerCamera;
    public float horizontalSensitivity = 2.5f;
    public float verticalSensitivity = 2.5f;
    public float maxLookAngle = 80f;

    [Header("Controls")]
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Camera FOV")]
    public float jumpFovIncrease = 10f;
    public float runFovIncrease = 5f;
    public float fovSmoothSpeed = 10f;

    [Header("Head Bob")]
    public float bobFrequency = 5f;
    public float bobAmplitude = 0.05f;
    public float bobSmoothing = 8f;

    private Camera cam;
    private float baseFov;
    private Vector3 cameraDefaultLocalPos;

    private float bobTimer = 0f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 moveVelocity;
    private float cameraPitch = 0f;
    private bool isJumping = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        moveVelocity = Vector3.zero;
        if (playerCamera != null)
        {
            cam = playerCamera.GetComponent<Camera>();
            if (cam != null)
                baseFov = cam.fieldOfView;
            cameraDefaultLocalPos = playerCamera.localPosition;
        }
        if (cam == null)
        {
            baseFov = 60f;
        }
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

        float targetSpeed = Input.GetKey(sprintKey) ? runSpeed : walkSpeed;
        Vector3 inputDir = (transform.right * x + transform.forward * z).normalized;
        Vector3 targetVelocity = inputDir * targetSpeed;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            float accel = targetSpeed / Mathf.Max(0.01f, timeToMaxSpeed);
            moveVelocity = Vector3.MoveTowards(moveVelocity, targetVelocity, accel * Time.deltaTime);
        }
        else
        {
            moveVelocity = Vector3.MoveTowards(moveVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Use a more realistic jump formula
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f; // keep grounded

            if (Input.GetButtonDown("Jump"))
            {
                // v = sqrt(2 * h * -g)
                velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
                isJumping = true;
            }
            else
            {
                isJumping = false;
            }
        }
        else
        {
            // Allow a bit of hang time at the top of the jump
            velocity.y += gravity * Time.deltaTime;
        }

        bool isRunning = Input.GetKey(sprintKey) && inputDir.sqrMagnitude > 0.01f;

        if (cam != null)
        {
            float desiredFov = baseFov;
            if (isRunning)
                desiredFov += runFovIncrease;
            if (isJumping)
                desiredFov += jumpFovIncrease;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFov, Time.deltaTime * fovSmoothSpeed);

            // simple head bob
            if (controller.isGrounded && inputDir.sqrMagnitude > 0.01f)
            {
                bobTimer += Time.deltaTime * bobFrequency * (isRunning ? 1.5f : 1f);
                float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;
                Vector3 targetPos = cameraDefaultLocalPos + Vector3.up * bobOffset;
                playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetPos, Time.deltaTime * bobSmoothing);
            }
            else
            {
                bobTimer = 0f;
                playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, cameraDefaultLocalPos, Time.deltaTime * bobSmoothing);
            }
        }

        Vector3 motion = moveVelocity + Vector3.up * velocity.y;
        controller.Move(motion * Time.deltaTime);
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
    }
}
