using UnityEngine;

/// <summary>
/// Casts a ray from the player's camera every frame and
/// highlights the crosshair whenever an <see cref="IInteractable"/>
/// is in the centre of the screen.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class CrosshairHighlighter : MonoBehaviour
{
    public float highlightDistance = 4f;
    public LayerMask highlightMask = -1;
    public CrosshairUI crosshairUI;

    private Transform cam;
    private PlayerHiding hiding;

    void Start()
    {
        var movement = GetComponent<PlayerMovement>();
        cam = movement ? movement.playerCamera : null;
        hiding = GetComponent<PlayerHiding>();
        if (crosshairUI == null)
            crosshairUI = FindFirstObjectByType<CrosshairUI>();
    }

    void Update()
    {
        if (crosshairUI == null || cam == null)
            return;

        if (hiding != null && hiding.IsHiding)
        {
            crosshairUI.SetHighlighted(false);
            return;
        }

        Ray ray = new Ray(cam.position, cam.forward);
        bool highlight = false;
        if (Physics.Raycast(ray, out RaycastHit hit, highlightDistance, highlightMask))
        {
            bool hasInteractable =
                hit.collider.GetComponent<IInteractable>() != null ||
                hit.collider.GetComponentInParent<IInteractable>() != null ||
                hit.collider.GetComponentInChildren<IInteractable>() != null;

            bool hasHideSpot =
                hit.collider.GetComponent<HideSpot>() != null ||
                hit.collider.GetComponentInParent<HideSpot>() != null ||
                hit.collider.GetComponentInChildren<HideSpot>() != null;

            highlight = hasInteractable || hasHideSpot;
        }
        crosshairUI.SetHighlighted(highlight);
    }
}