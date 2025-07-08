using System.Collections;
using UnityEngine;

public class ClosetHideSpot : HideSpot
{
    public Transform door;
    public Transform peekPoint;
    public float doorOpenAngle = 90f;
    public float doorSpeed = 2f;

    [Header("Camera")]
    [Tooltip("Field of view while hiding inside the closet")] public float hideFov = 50f;
    [Tooltip("Speed of the FOV transition")] public float fovSmooth = 10f;

    private float originalFov = -1f;
    private Coroutine fovRoutine;

    private Quaternion closedRot;
    private Quaternion openRot;

    void Awake()
    {
        if (door != null)
        {
            closedRot = door.localRotation;
            openRot = closedRot * Quaternion.Euler(0f, doorOpenAngle, 0f);
        }
    }

    public Coroutine AnimateDoors(MonoBehaviour host, bool open, float fraction = 1f)
    {
        if (host == null)
            return null;
        return host.StartCoroutine(AnimateDoorsRoutine(open, fraction));
    }

    IEnumerator AnimateDoorsRoutine(bool open, float fraction)
    {
        if (door == null)
            yield break;

        Quaternion startRot = door.localRotation;
        Quaternion endRot = open
            ? Quaternion.Slerp(closedRot, openRot, fraction)
            : closedRot;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * doorSpeed;
            door.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        door.localRotation = endRot;
    }

    public void OnPlayerEnter()
    {
        if (PlayerMovement.Instance && PlayerMovement.Instance.playerCamera)
        {
            Camera cam = PlayerMovement.Instance.playerCamera.GetComponent<Camera>();
            if (cam)
            {
                originalFov = cam.fieldOfView;
                if (fovRoutine != null) StopCoroutine(fovRoutine);
                fovRoutine = StartCoroutine(FovTransition(cam, hideFov));
            }
        }
    }

    public void OnPlayerExit()
    {
        if (originalFov > 0f && PlayerMovement.Instance && PlayerMovement.Instance.playerCamera)
        {
            Camera cam = PlayerMovement.Instance.playerCamera.GetComponent<Camera>();
            if (cam)
            {
                if (fovRoutine != null) StopCoroutine(fovRoutine);
                fovRoutine = StartCoroutine(FovTransition(cam, originalFov));
            }
        }
        originalFov = -1f;
    }

    IEnumerator FovTransition(Camera cam, float target)
    {
        float start = cam.fieldOfView;
        float t = 0f;
        while (t < 1f)
        {
            cam.fieldOfView = Mathf.Lerp(start, target, t);
            t += Time.deltaTime * fovSmooth;
            yield return null;
        }
        cam.fieldOfView = target;
    }
}