using UnityEngine;
using TMPro;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerKeyDoorInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public TMP_Text promptText;
    public LayerMask interactMask = -1;
    public CrosshairUI crosshairUI;

    private Transform cam;
    private bool hasKey = false;

    void Start()
    {
        cam = GetComponent<PlayerMovement>().playerCamera;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
        if (crosshairUI == null)
            crosshairUI = FindObjectOfType<CrosshairUI>();
    }

    void Update()
    {
        if (cam == null)
            return;

        bool highlighted = false;
        Ray ray = new Ray(cam.position, cam.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            KeyItem key = hit.collider.GetComponent<KeyItem>();
            if (key != null && !key.collected)
            {
                highlighted = true;
                if (promptText != null)
                {
                    promptText.gameObject.SetActive(true);
                    promptText.text = "Press 'E' to pick up key";
                }
                if (Input.GetKeyDown(interactKey))
                {
                    key.Pickup();
                    hasKey = true;
                    if (promptText != null)
                        promptText.gameObject.SetActive(false);
                }
                if (crosshairUI != null)
                    crosshairUI.SetHighlighted(true);
                return;
            }

            Door door = hit.collider.GetComponent<Door>();
            if (door != null)
            {
                highlighted = true;
                if (promptText != null)
                {
                    if (door.IsOpen)
                    {
                        promptText.gameObject.SetActive(true);
                        promptText.text = "Press 'E' to close";
                    }
                    else if (hasKey)
                    {
                        promptText.gameObject.SetActive(true);
                        promptText.text = "Press 'E' to open";
                    }
                    else
                    {
                        promptText.gameObject.SetActive(true);
                        promptText.text = "Need key to open";
                    }
                }

                if (Input.GetKeyDown(interactKey))
                {
                    if (door.IsOpen)
                    {
                        door.Close();
                        if (promptText != null)
                            promptText.gameObject.SetActive(false);
                    }
                    else if (hasKey)
                    {
                        door.Open();
                        if (promptText != null)
                            promptText.gameObject.SetActive(false);
                    }
                }

                if (crosshairUI != null)
                    crosshairUI.SetHighlighted(true);
                return;
            }

            SimpleInteractable interact = hit.collider.GetComponent<SimpleInteractable>();
            if (interact != null)
            {
                highlighted = true;
                if (promptText != null)
                {
                    promptText.gameObject.SetActive(true);
                    promptText.text = "Press 'E' to interact";
                }
                if (Input.GetKeyDown(interactKey))
                {
                    interact.Toggle();
                    if (promptText != null)
                        promptText.gameObject.SetActive(false);
                }
                if (crosshairUI != null)
                    crosshairUI.SetHighlighted(true);
                return;
            }
        }

        if (crosshairUI != null)
            crosshairUI.SetHighlighted(highlighted);

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }
}