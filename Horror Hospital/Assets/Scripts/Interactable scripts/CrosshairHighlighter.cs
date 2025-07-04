using UnityEngine;

/// <summary>
/// Casts a ray from the player's camera every frame and
/// highlights the crosshair whenever an <see cref="IInteractable"/>
/// is in the centre of the screen.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class CrosshairHighlighter : MonoBehaviour
{
    public float highlightDistance = 3f;
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
            crosshairUI = FindObjectOfType<CrosshairUI>();
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
            if (hit.collider.GetComponent<IInteractable>() != null)
                highlight = true;
        }
        crosshairUI.SetHighlighted(highlight);
    }
}
