using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public Button optionButtonPrefab;
    public Transform optionContainer;

    private DialogueData currentData;
    private int currentNodeIndex;

    // remember previous cursor state so we can restore it when the dialogue ends
    private CursorLockMode prevLockState;
    private bool prevCursorVisible;


    /* ────────────────────────── Unity ────────────────────────── */

    void Awake()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
    }

    /* ────────────────────────── Public API ───────────────────── */

    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.nodes?.Length == 0) return;

        currentData = data;
        currentNodeIndex = 0;

        // unlock cursor so the player can interact with the UI
        prevLockState = Cursor.lockState;
        prevCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // prevent player from walking during dialogue and zoom in a bit
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.CanMove = false;
            PlayerMovement.Instance.SetFovOffset(-10f);
        }

        dialoguePanel.SetActive(true);
        ShowCurrentNode();
    }

    public void EndDialogue()
    {
        ClearOptions();
        dialoguePanel.SetActive(false);

        // restore previous cursor state
        Cursor.lockState = prevLockState;
        Cursor.visible = prevCursorVisible;

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.CanMove = true;
            PlayerMovement.Instance.SetFovOffset(0f);
        }

        currentData = null;
        currentNodeIndex = 0;
    }

    public void SelectOption(int nextIndex)
    {
        if (currentData == null) return;

        if (nextIndex == -1) { EndDialogue(); return; }

        if (nextIndex >= 0 && nextIndex < currentData.nodes.Length)
        {
            currentNodeIndex = nextIndex;
            ShowCurrentNode();
        }
        else EndDialogue();
    }

    /* ────────────────────────── Internals ────────────────────── */

    private void ShowCurrentNode()
    {
        ClearOptions();

        var node = currentData.nodes[currentNodeIndex];
        if (dialogueText) dialogueText.text = node.dialogueText;

        foreach (var opt in node.options)
        {
            Button btn = Instantiate(optionButtonPrefab, optionContainer);
            btn.gameObject.SetActive(true);                 // ← ensures visibility

            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt) txt.text = opt.optionText;

            int captured = opt.nextNode;
            btn.onClick.AddListener(() => SelectOption(captured));
        }
    }

    private void ClearOptions()
    {
        foreach (Transform child in optionContainer)
            Destroy(child.gameObject);
    }
}