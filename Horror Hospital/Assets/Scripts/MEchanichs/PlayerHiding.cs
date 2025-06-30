using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerHiding : MonoBehaviour
{
    public float interactDistance = 3f;
    public KeyCode hideKey = KeyCode.E;
    public TMP_Text promptText;

    [Header("Hide Settings")]
    public float transitionDuration = 0.5f;
    public float hideLookSensitivity = 2f;
    public float horizontalLookLimit = 30f;
    public float hideMaxLookAngle = 80f;

    [Header("UI")]
    public GameObject crosshairDot;

    private PlayerMovement movementScript;
    private Transform playerCamera;
    private bool crosshairInitialActive = true;

    private bool isHiding = false;
    public bool IsHiding => isHiding;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Quaternion originalCameraRotation;

    private Quaternion hideBaseRotation;
    private float hideYaw = 0f;
    private float hidePitch = 0f;

    private Coroutine transitionRoutine;

    void Start()
    {
        movementScript = GetComponent<PlayerMovement>();
        playerCamera = movementScript.playerCamera;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
        if (crosshairDot != null)
        {
            crosshairInitialActive = crosshairDot.activeSelf;
        }
    }

    void Update()
    {
        if (isHiding)
        {
            if (promptText != null)
                promptText.text = "Press 'E' to exit";

            if (Input.GetKeyDown(hideKey))
                ExitHide();

            float mouseX = Input.GetAxis("Mouse X") * hideLookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * hideLookSensitivity;

            hideYaw += mouseX;
            hideYaw = Mathf.Clamp(hideYaw, -horizontalLookLimit, horizontalLookLimit);
            hidePitch -= mouseY;
            hidePitch = Mathf.Clamp(hidePitch, -hideMaxLookAngle, hideMaxLookAngle);

            transform.rotation = hideBaseRotation * Quaternion.Euler(0f, hideYaw, 0f);
            playerCamera.localEulerAngles = Vector3.right * hidePitch;

            return;
        }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            HideSpot spot = hit.collider.GetComponent<HideSpot>();
            if (spot != null && spot.hidePoint != null)
            {
                if (promptText != null)
                {
                    promptText.gameObject.SetActive(true);
                    promptText.text = "Press 'E' to hide";
                }

                if (Input.GetKeyDown(hideKey))
                    EnterHide(spot);
                return;
            }
        }

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void EnterHide(HideSpot spot)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        isHiding = true;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalCameraRotation = playerCamera.localRotation;

        hideBaseRotation = spot.hidePoint.rotation;
        hideYaw = 0f;
        hidePitch = 0f;

        if (movementScript != null)
            movementScript.enabled = false;

        if (crosshairDot != null)
            crosshairDot.SetActive(false);

        transitionRoutine = StartCoroutine(SmoothMove(spot.hidePoint.position, spot.hidePoint.rotation));

        if (promptText != null)
            promptText.text = "Press 'E' to exit";
    }

    void ExitHide()
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(SmoothMove(originalPosition, originalRotation));

        playerCamera.localRotation = originalCameraRotation;

        if (movementScript != null)
            movementScript.enabled = true;

        if (crosshairDot != null)
            crosshairDot.SetActive(crosshairInitialActive);

        isHiding = false;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    IEnumerator SmoothMove(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }
}