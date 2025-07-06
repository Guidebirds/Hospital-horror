using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerHiding : MonoBehaviour
{
    // ??????????????????????????????????????????????????????????????????????
    #region Inspector

    [Header("Input / Interaction")]
    public float interactDistance = 3f;
    public KeyCode hideKey = KeyCode.E;
    public KeyCode peekKey = KeyCode.P;
    public TMP_Text promptText;

    [Header("Hide Settings")]
    public float transitionDuration = 0.5f;
    public float hideLookSensitivity = 2f;
    public float horizontalLookLimit = 30f;
    public float hideMaxLookAngle = 80f;
    public bool alignToHidePoint = true;        // keep for backwards-compat

    [Header("Camera Smoothing")]
    public bool smoothCameraTransition = true;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI")]
    public GameObject crosshairDot;
    public CrosshairUI crosshairUI;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip enterClip;
    public AudioClip exitClip;
    public AudioClip peekClip;

    #endregion
    // ??????????????????????????????????????????????????????????????????????
    #region Private State

    private PlayerMovement movementScript;
    private Transform playerCamera;
    private bool crosshairInitialActive = true;

    private bool isHiding = false;
    public bool IsHiding => isHiding;
    private bool isFullyHidden = false;
    private bool isPeeking = false;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Quaternion originalCamRot;

    private Quaternion hideBaseRot;
    private float hideYaw = 0f;
    private float hidePitch = 0f;

    private Coroutine transitionRoutine;
    private HideSpot currentSpot;
    private ClosetHideSpot currentCloset;
    private Coroutine doorRoutine;

    #endregion
    // ??????????????????????????????????????????????????????????????????????
    #region Mono Behaviour

    void Start()
    {
        movementScript = GetComponent<PlayerMovement>();
        playerCamera = movementScript.playerCamera;

        if (promptText) promptText.gameObject.SetActive(false);
        if (crosshairDot)
        {
            crosshairInitialActive = crosshairDot.activeSelf;
            if (!crosshairUI) crosshairUI = crosshairDot.GetComponent<CrosshairUI>();
        }
    }

    void Update()
    {
        if (HandleHiddenInput()) return;   // early-out when hiding

        // ?? Ray-cast for a HideSpot ?????????????????????????????
        Ray ray = new(playerCamera.position, playerCamera.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            HideSpot spot = hit.collider.GetComponent<HideSpot>();
            if (spot && spot.hidePoint)
            {
                if (crosshairUI) crosshairUI.SetHighlighted(true);
                if (promptText)
                {
                    promptText.gameObject.SetActive(true);
                    promptText.text = "Press 'E' to hide";
                }
                if (Input.GetKeyDown(hideKey)) EnterHide(spot);
                return;
            }
        }

        // no target ? clear UI
        if (crosshairUI) crosshairUI.SetHighlighted(false);
        if (promptText) promptText.gameObject.SetActive(false);
    }

    #endregion
    // ??????????????????????????????????????????????????????????????????????
    #region Hiding Logic

    bool HandleHiddenInput()
    {
        if (!isHiding) return false;

        // UI refresh
        if (crosshairUI) crosshairUI.SetHighlighted(false);
        if (promptText)
        {
            promptText.text = currentCloset
              ? (isPeeking ? "Press 'E' to exit" : "Press 'E' to exit\nHold 'P' to peek")
              : "Press 'E' to exit";
        }

        if (isFullyHidden && Input.GetKeyDown(hideKey)) { ExitHide(); return true; }

        if (currentCloset)
        {
            if (!isPeeking && Input.GetKeyDown(peekKey)) StartPeek();
            else if (isPeeking && Input.GetKeyUp(peekKey)) EndPeek();
        }

        // look-around limits
        float mx = Input.GetAxis("Mouse X") * hideLookSensitivity;
        float my = Input.GetAxis("Mouse Y") * hideLookSensitivity;
        hideYaw = Mathf.Clamp(hideYaw + mx, -horizontalLookLimit, horizontalLookLimit);
        hidePitch = Mathf.Clamp(hidePitch - my, -hideMaxLookAngle, hideMaxLookAngle);

        transform.rotation = hideBaseRot * Quaternion.Euler(0, hideYaw, 0);
        playerCamera.localEulerAngles = Vector3.right * hidePitch;
        return true;
    }

    void EnterHide(HideSpot spot)
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);

        isHiding = true;
        isFullyHidden = false;
        isPeeking = false;
        currentSpot = spot;
        currentCloset = spot.GetComponent<ClosetHideSpot>();

        originalPos = transform.position;
        originalRot = transform.rotation;
        originalCamRot = playerCamera.localRotation;

        // ?? Decide facing direction ????????????????????????????
        if (spot.lookTarget)                     // top priority
        {
            hideBaseRot = Quaternion.LookRotation(spot.lookTarget.forward, Vector3.up);
        }
        else if (alignToHidePoint)               // legacy option
        {
            hideBaseRot = currentCloset
                ? spot.hidePoint.rotation * Quaternion.Euler(0, 180, 0)
                : spot.hidePoint.rotation;
        }
        else hideBaseRot = transform.rotation;

        hideYaw = hidePitch = 0f;

        if (movementScript) movementScript.enabled = false;
        if (crosshairDot) crosshairDot.SetActive(false);
        if (crosshairUI) crosshairUI.SetHighlighted(false);

        PlayClip(enterClip);
        transitionRoutine = StartCoroutine(EnterSequence());

        if (promptText) promptText.text = "Press 'E' to exit";
    }

    IEnumerator EnterSequence()
    {
        if (currentCloset) yield return currentCloset.AnimateDoors(this, true);

        // through optional entry nodes
        if (currentSpot.entryPoints?.Length > 0)
        {
            foreach (var pt in currentSpot.entryPoints)
            {
                if (!pt) continue;
                Quaternion rot = pt.rotation;
                if (currentCloset) rot *= Quaternion.Euler(0, 180, 0);
                yield return SmoothMove(pt.position, rot, true, playerCamera.localRotation);
            }
        }

        // final slide to hide point
        Quaternion finalRot = hideBaseRot;
        yield return SmoothMove(currentSpot.hidePoint.position, finalRot,
                                true, Quaternion.identity);

        if (currentCloset) yield return currentCloset.AnimateDoors(this, false);
        isFullyHidden = true;
    }

    void ExitHide()
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        if (doorRoutine != null) StopCoroutine(doorRoutine);

        isFullyHidden = false;
        transitionRoutine = StartCoroutine(ExitSequence());
    }

    IEnumerator ExitSequence()
    {
        if (currentCloset) yield return currentCloset.AnimateDoors(this, true);

        yield return SmoothMove(originalPos, originalRot, true, originalCamRot);

        if (currentCloset) yield return currentCloset.AnimateDoors(this, false);

        PlayClip(exitClip);
        if (movementScript) movementScript.enabled = true;
        if (crosshairDot) crosshairDot.SetActive(crosshairInitialActive);
        if (crosshairUI) crosshairUI.SetHighlighted(false);

        isHiding = isPeeking = false;
        currentSpot = null;
        currentCloset = null;
        if (promptText) promptText.gameObject.SetActive(false);
    }

    #endregion
    // ??????????????????????????????????????????????????????????????????????
    #region Peeking

    void StartPeek()
    {
        if (!currentCloset || !currentCloset.peekPoint) return;
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        if (doorRoutine != null) StopCoroutine(doorRoutine);

        isPeeking = true;
        PlayClip(peekClip);
        doorRoutine = currentCloset.AnimateDoors(this, true, 0.25f);
        Quaternion rot = currentCloset.peekPoint.rotation * Quaternion.Euler(0, 180, 0);
        transitionRoutine = StartCoroutine(SmoothMove(currentCloset.peekPoint.position, rot));
    }

    void EndPeek()
    {
        if (!currentCloset || !currentCloset.peekPoint) return;
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        if (doorRoutine != null) StopCoroutine(doorRoutine);

        doorRoutine = currentCloset.AnimateDoors(this, false);
        Quaternion rot = currentSpot.hidePoint.rotation * Quaternion.Euler(0, 180, 0);
        transitionRoutine = StartCoroutine(SmoothMove(currentSpot.hidePoint.position, rot));
        isPeeking = false;
        PlayClip(peekClip);
    }

    #endregion
    // ??????????????????????????????????????????????????????????????????????
    #region Helpers

    IEnumerator SmoothMove(Vector3 targetPos, Quaternion targetRot,
                           bool adjustCam = false, Quaternion targetCam = default)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion startCam = playerCamera ? playerCamera.localRotation : Quaternion.identity;

        float t = 0f;
        while (t < 1f)
        {
            float eased = smoothCameraTransition ? easeCurve.Evaluate(t) : t;
            transform.position = Vector3.Lerp(startPos, targetPos, eased);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, eased);
            if (adjustCam && playerCamera)
                playerCamera.localRotation = Quaternion.Slerp(startCam, targetCam, eased);

            t += Time.deltaTime / transitionDuration;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        if (adjustCam && playerCamera) playerCamera.localRotation = targetCam;
    }

    void PlayClip(AudioClip clip)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip);
    }

    #endregion
}
