using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCDialogue : MonoBehaviour
{
    public DialogueData dialogue;
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    private DialogueManager manager;
    private Transform playerCam;

    void Start()
    {
        playerCam = Camera.main.transform;
        manager = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {
        if (manager == null || playerCam == null || dialogue == null)
            return;

        if (manager.dialoguePanel != null && manager.dialoguePanel.activeSelf)
            return; // ignore interaction while a dialogue is running

        if (Input.GetKeyDown(interactKey))
        {
            Ray ray = new Ray(playerCam.position, playerCam.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    manager.StartDialogue(dialogue);
                }
            }
        }
    }
}