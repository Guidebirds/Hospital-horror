using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private DialogueManager manager;
    private Transform playerCam;

    void Awake()
    {
        manager = FindFirstObjectByType<DialogueManager>();
        playerCam = Camera.main ? Camera.main.transform : null;
    }

    void Update()
    {
        if (manager == null || playerCam == null || dialogue == null)
            return;

        // skip while another conversation is running
        if (manager.dialoguePanel != null && manager.dialoguePanel.activeSelf)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            if (Physics.Raycast(playerCam.position, playerCam.forward,
                                out RaycastHit hit, interactDistance) &&
                hit.collider != null && hit.collider.gameObject == gameObject)
            {
                manager.StartDialogue(dialogue);
            }
        }
    }
}