using UnityEngine;

public class GeneratorInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public Renderer buttonRenderer; // Renderer of the button to recolor
    public Material greenMaterial;  // Material to apply when generator is active
    public Material offMaterial;    // Material to apply when generator is off

    private Camera playerCam;
    private bool activated = false;

    void Start()
    {
        playerCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    ToggleGenerator();
                }
            }
        }
    }

    void ToggleGenerator()
    {
        activated = !activated;
        Debug.Log(activated ? "Generator is on. Elevator will work now." : "Generator is off.");
        if (buttonRenderer != null)
        {
            if (activated && greenMaterial != null)
                buttonRenderer.material = greenMaterial;
            else if (!activated && offMaterial != null)
                buttonRenderer.material = offMaterial;
        }
    }
}