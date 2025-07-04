using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public Transform doorTransform;
    public Transform handleTransform;
    public float openAngle = 90f;
    public float openSpeed = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    private Quaternion closedRot;
    private Quaternion openRot;
    private Quaternion handleClosedRot;
    private Quaternion handleOpenRot;
    private bool open = false;
    private Coroutine animRoutine;

    void Start()
    {
        if (doorTransform == null)
            doorTransform = transform;
        closedRot = doorTransform.localRotation;
        openRot = closedRot * Quaternion.Euler(0f, openAngle, 0f);

        if (handleTransform != null)
        {
            handleClosedRot = handleTransform.localRotation;
            handleOpenRot = handleClosedRot * Quaternion.Euler(0f, 0f, -45f);
        }
    }

    public bool IsOpen => open;

    public void Open()
    {
        if (open)
            return;
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        open = true;
        animRoutine = StartCoroutine(AnimateDoor(true));
        PlaySound(openSound);
    }

    public void Close()
    {
        if (!open)
            return;
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        open = false;
        animRoutine = StartCoroutine(AnimateDoor(false));
        PlaySound(closeSound);
    }

    IEnumerator AnimateDoor(bool opening)
    {
        Quaternion startRot = doorTransform.localRotation;
        Quaternion endRot = opening ? openRot : closedRot;
        Quaternion startHandleRot = handleTransform != null ? handleTransform.localRotation : Quaternion.identity;
        Quaternion endHandleRot = Quaternion.identity;
        if (handleTransform != null)
            endHandleRot = opening ? handleOpenRot : handleClosedRot;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            doorTransform.localRotation = Quaternion.Slerp(startRot, endRot, t);
            if (handleTransform != null)
                handleTransform.localRotation = Quaternion.Slerp(startHandleRot, endHandleRot, t);
            yield return null;
        }
        doorTransform.localRotation = endRot;
        if (handleTransform != null)
            handleTransform.localRotation = endHandleRot;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}