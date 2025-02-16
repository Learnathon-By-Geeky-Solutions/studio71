using UnityEngine;
namespace UI.DialogueSystem
{
    /// <summary>
    /// Triggers dialogue when interacted with.
    /// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private Message[] _messages;
    [SerializeField] private Actor[] _actors;

    public void StartDialogue()
    {
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.OpenDialogue(_messages, _actors);
        }
    }
}

[System.Serializable]
public class Message{
    public int ActorId;
    public string Dialogue;
}
[System.Serializable]
public class Actor{
    public string Name;
    public Sprite Sprite;
}
}