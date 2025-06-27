using UnityEngine;

public class GeneratorInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    private Camera playerCam;
    private bool activated = false;

    void Start()
    {
        playerCam = Camera.main;     // reference the player’s camera
    }

    void Update()
    {
        if (activated) return;

        // Check for E key press while looking at the generator
        if (Input.GetKeyDown(interactKey))
        {
            Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    activated = true;
                    Debug.Log("Generator is on. Elevator will work now.");
                    // TODO: notify elevator script when you add one
                }
            }
        }
    }
}