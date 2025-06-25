using UnityEngine;
using TMPro;

[RequireComponent(typeof(playerMovement))]
public class PlayerHiding : MonoBehaviour
{
    public float interactDistance = 3f;
    public KeyCode hideKey = KeyCode.E;
    public TMP_Text promptText;

    private playerMovement movementScript;
    private Transform playerCamera;

    private bool isHiding = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        movementScript = GetComponent<playerMovement>();
        playerCamera = movementScript.playerCamera;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isHiding)
        {
            if (promptText != null)
                promptText.text = "Press 'E' to exit";

            if (Input.GetKeyDown(hideKey))
                ExitHide();
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
        isHiding = true;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        transform.position = spot.hidePoint.position;
        transform.rotation = spot.hidePoint.rotation;

        if (movementScript != null)
            movementScript.enabled = false;

        if (promptText != null)
            promptText.text = "Press 'E' to exit";
    }

    void ExitHide()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (movementScript != null)
            movementScript.enabled = true;

        isHiding = false;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }
}