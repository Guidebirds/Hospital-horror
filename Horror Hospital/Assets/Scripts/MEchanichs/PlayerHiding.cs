using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerHiding : MonoBehaviour
{
    /* ──────── Inspector ──────── */

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
    public bool alignToHidePoint = true;

    [Header("Camera Smoothing")]
    public bool smoothCameraTransition = true;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Extra seconds after each transition while mouse-look stays locked.")]
    public float postTransitionLock = 0.1f;

    [Header("UI")]
    public GameObject crosshairDot;
    public CrosshairUI crosshairUI;

    [Header("Audio ─ Enter")]
    public AudioClip enterClipCommon;
    public AudioClip enterClipRare;
    [Range(0f, 1f)] public float rareEnterChance = 0.15f;

    [Header("Audio ─ Exit")]
    public AudioClip exitClip;

    [Header("Audio ─ Peek")]
    public AudioClip[] peekClips;

    /* ──────── Private state ──────── */

    private PlayerMovement movementScript;
    private Transform playerCamera;

    private bool crosshairInitialActive;

    private bool isHiding = false;
    public bool IsHiding => isHiding;        // for other scripts
    private bool isFullyHidden = false;
    private bool isPeeking = false;
    private bool isTransitioning = false;
    private float postLockTimer = 0f;

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

    private int peekClipIndex = 0;
    private readonly System.Random rng = new();

    /* ──────── MonoBehaviour ──────── */


// ────────────────────────────────────────────────────────────────────
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
        // global post-transition mouse-lock
        if (postLockTimer > 0f) postLockTimer -= Time.deltaTime;

        if (HandleHiddenInput()) return;

        // ── Ray-cast for HideSpot ───────────────────────────────
        Ray ray = new(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
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

        if (crosshairUI) crosshairUI.SetHighlighted(false);
        if (promptText) promptText.gameObject.SetActive(false);
    }

    // ────────────────────────────────────────────────────────────────────
    #region Hiding Logic

    bool HandleHiddenInput()
    {
        if (!isHiding) return false;

        // lock all look input during slides AND during extra delay
        if (isTransitioning || postLockTimer > 0f) return true;

        if (crosshairUI) crosshairUI.SetHighlighted(false);

        if (promptText)
        {
            promptText.text = currentCloset
              ? (isPeeking ? "Press 'E' to exit"
                           : "Press 'E' to exit\nHold 'P' to peek")
              : "Press 'E' to exit";
        }

        if (isFullyHidden && Input.GetKeyDown(hideKey))
        {
            ExitHide();
            return true;
        }

        if (currentCloset)
        {
            if (!isPeeking && Input.GetKeyDown(peekKey)) StartPeek();
            else if (isPeeking && Input.GetKeyUp(peekKey)) EndPeek();
        }

        // limited mouse-look while hidden
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

        // Decide final facing
        if (spot.lookTarget)
            hideBaseRot = Quaternion.LookRotation(spot.lookTarget.forward, Vector3.up);
        else if (alignToHidePoint)
            hideBaseRot = currentCloset
                ? spot.hidePoint.rotation * Quaternion.Euler(0, 180, 0)
                : spot.hidePoint.rotation;
        else
            hideBaseRot = transform.rotation;

        hideYaw = hidePitch = 0f;

        if (movementScript) movementScript.enabled = false;
        if (crosshairDot) crosshairDot.SetActive(false);
        if (crosshairUI) crosshairUI.SetHighlighted(false);

        PlayEnterClip();

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

        yield return SmoothMove(currentSpot.hidePoint.position, hideBaseRot,
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

        // optional custom path out
        if (currentSpot.exitPath?.Length > 0)
            foreach (var pt in currentSpot.exitPath)
                if (pt) yield return SmoothMove(pt.position, pt.rotation);

        // final exit
        Vector3 endPos = currentSpot.exitPoint ? currentSpot.exitPoint.position : originalPos;
        Quaternion endRot = currentSpot.exitPoint ? currentSpot.exitPoint.rotation : originalRot;
        yield return SmoothMove(endPos, endRot, true, originalCamRot);

        if (currentCloset) yield return currentCloset.AnimateDoors(this, false);

        PlayClip(exitClip);

        if (movementScript) movementScript.enabled = true;
        if (crosshairDot) crosshairDot.SetActive(crosshairInitialActive);
        if (crosshairUI) crosshairUI.SetHighlighted(false);

        isHiding = false;
        isPeeking = false;
        currentSpot = null;
        currentCloset = null;

        if (promptText) promptText.gameObject.SetActive(false);
    }

    #endregion
    // ────────────────────────────────────────────────────────────────────
    #region Peeking

    void StartPeek()
    {
        if (!currentCloset || !currentCloset.peekPoint) return;
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        if (doorRoutine != null) StopCoroutine(doorRoutine);

        isPeeking = true;

        PlayPeekClip();

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

        PlayPeekClip();
    }

    #endregion
    // ────────────────────────────────────────────────────────────────────
    #region Helpers

    IEnumerator SmoothMove(Vector3 targetPos, Quaternion targetRot,
                           bool adjustCam = false, Quaternion targetCamRot = default)
    {
        isTransitioning = true;

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
                playerCamera.localRotation = Quaternion.Slerp(startCam, targetCamRot, eased);

            t += Time.deltaTime / transitionDuration;
            yield return null;
        }

        transform.SetPositionAndRotation(targetPos, targetRot);
        if (adjustCam && playerCamera) playerCamera.localRotation = targetCamRot;

        isTransitioning = false;
        postLockTimer = postTransitionLock;   // keep mouse off a moment longer
    }

    void PlayClip(AudioClip clip)
    {
        if (clip && movementScript && movementScript.enabled)   // allow sound even when disabled
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }

    void PlayEnterClip()
    {
        AudioClip clipToPlay =
            rng.NextDouble() < rareEnterChance && enterClipRare ? enterClipRare : enterClipCommon;
        PlayClip(clipToPlay);
    }

    void PlayPeekClip()
    {
        if (peekClips == null || peekClips.Length == 0) return;

        AudioClip clip = peekClips[peekClipIndex % peekClips.Length];
        peekClipIndex++;
        PlayClip(clip);
    }

    #endregion
}
