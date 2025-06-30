using UnityEngine;
using UnityEngine.UI;
using static DialogueData;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Button optionButtonPrefab;
    public Transform optionContainer;

    private DialogueData currentData;
    private int currentNodeIndex;

    void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    /* ---------- Public API ---------- */

    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.nodes == null || data.nodes.Length == 0)
            return;

        currentData = data;
        currentNodeIndex = 0;

        dialoguePanel.SetActive(true);
        ShowCurrentNode();
    }

    public void EndDialogue()
    {
        ClearOptions();
        dialoguePanel.SetActive(false);

        currentData = null;
        currentNodeIndex = 0;
    }

    public void SelectOption(int nextIndex)
    {
        if (currentData == null) return;

        if (nextIndex == -1)
        {
            EndDialogue();
            return;
        }

        if (nextIndex >= 0 && nextIndex < currentData.nodes.Length)
        {
            currentNodeIndex = nextIndex;
            ShowCurrentNode();
        }
        else
        {
            EndDialogue();     // invalid index – fail-safe
        }
    }

    /* ---------- Internals ---------- */

    private void ShowCurrentNode()
    {
        ClearOptions();

        DialogueData.DialogueNode node = currentData.nodes[currentNodeIndex];
        if (dialogueText != null) dialogueText.text = node.dialogueText;

        foreach (DialogueData.DialogueOption opt in node.options)
        {
            Button btn = Instantiate(optionButtonPrefab, optionContainer);

            if (btn.TryGetComponent(out Text btnText))
                btnText.text = opt.optionText;

            int capturedIndex = opt.nextNode;                   // capture loop var
            btn.onClick.AddListener(() => SelectOption(capturedIndex));
        }
    }

    private void ClearOptions()
    {
        foreach (Transform child in optionContainer)
            Destroy(child.gameObject);
    }
}