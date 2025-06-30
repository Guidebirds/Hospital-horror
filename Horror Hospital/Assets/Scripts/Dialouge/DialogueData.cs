using UnityEngine;

public class DialogueData : MonoBehaviour
{
    [System.Serializable]
    public class DialogueOption
    {
        public string optionText;
        public int nextNode = -1; // -1 ends the conversation
    }

    [System.Serializable]
    public class DialogueNode
    {
        [TextArea]
        public string dialogueText;
        public DialogueOption[] options;
    }

    [CreateAssetMenu(menuName = "Dialogue/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public DialogueNode[] nodes;
    }
}
