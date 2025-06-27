using UnityEngine;

public class GeneratorInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public Renderer buttonRenderer; // Renderer of the button to recolor
    public Material greenMaterial;  // Material to apply when generator is active

    private Camera playerCam;
    private bool activated = false;

    void Start()
    {
        playerCam = Camera.main;
    }

    void Update()
    {
        if (activated)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    ActivateGenerator();
                }
            }
        }
    }

    void ActivateGenerator()
    {
        activated = true;
        Debug.Log("Generator is on. Elevator will work now.");
        if (buttonRenderer != null && greenMaterial != null)
        {
            buttonRenderer.material = greenMaterial;
        }
    }
}
