using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCDialogue : MonoBehaviour
{
    [SerializeField] public DialogueData dialogue;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private Transform lookTarget;
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.6f, 0f);

    private DialogueManager manager;
    private Transform playerCam;

    void Awake()
    {
        manager = FindFirstObjectByType<DialogueManager>();
        playerCam = Camera.main ? Camera.main.transform : null;
        if (lookTarget == null) lookTarget = transform;
    }

    void Update()
    {
        if (manager == null || playerCam == null || dialogue == null)
            return;

        // skip while another conversation is running
        if (manager.dialoguePanel != null && manager.dialoguePanel.activeSelf)
            return;

        if (Input.GetKeyDown(interactKey) || Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(playerCam.position, playerCam.forward,
                                out RaycastHit hit, interactDistance) &&
                hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Vector3 target = lookTarget ? lookTarget.position
                                            : transform.TransformPoint(lookOffset);
                manager.StartDialogue(dialogue, target);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 target = lookTarget ? lookTarget.position
                                    : transform.TransformPoint(lookOffset);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(target, 0.1f);
    }
}