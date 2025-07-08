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

    private float originalFov = -1f;

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
                cam.fieldOfView = hideFov;
            }
        }
    }

    public void OnPlayerExit()
    {
        if (originalFov > 0f && PlayerMovement.Instance && PlayerMovement.Instance.playerCamera)
        {
            Camera cam = PlayerMovement.Instance.playerCamera.GetComponent<Camera>();
            if (cam)
                cam.fieldOfView = originalFov;
        }
        originalFov = -1f;
    }
}