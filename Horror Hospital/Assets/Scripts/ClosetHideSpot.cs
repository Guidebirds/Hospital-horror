using System.Collections;
using UnityEngine;

public class ClosetHideSpot : HideSpot
{
    public Transform door;
    public Transform peekPoint;
    public float doorOpenAngle = 90f;
    public float doorSpeed = 2f;

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
}