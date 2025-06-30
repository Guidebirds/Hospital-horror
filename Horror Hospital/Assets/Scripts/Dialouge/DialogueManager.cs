using UnityEngine;
using UnityEngine.UI;
using static DialogueData;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Button optionButtonPrefab;
    public Transform optionContainer;

    private DialogueData currentData;
    private int currentNodeIndex;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.nodes.Length == 0)
            return;

        currentData = data;
        currentNodeIndex = 0;
        dialoguePanel.SetActive(true);
        ShowCurrentNode();
    }

    public void EndDialogue()
    {
        currentData = null;
        currentNodeIndex = 0;
        dialoguePanel.SetActive(false);
        foreach (Transform child in optionContainer)
            Destroy(child.gameObject);
    }

    public void SelectOption(int nextIndex)
    {
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
            EndDialogue();
        }
    }

    private void ShowCurrentNode()
    {
        foreach (Transform child in optionContainer)
            Destroy(child.gameObject);

        DialogueNode node = currentData.nodes[currentNodeIndex];
        if (dialogueText != null)
            dialogueText.text = node.dialogueText;

        foreach (var opt in node.options)
        {
            Button btn = Instantiate(optionButtonPrefab, optionContainer);
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = opt.optionText;
            int nextIndex = opt.nextNode;
            btn.onClick.AddListener(() => SelectOption(nextIndex));
        }
    }
}