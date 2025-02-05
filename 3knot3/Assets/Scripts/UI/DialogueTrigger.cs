using UnityEngine;
using TMPro;
public class DialogueTrigger : MonoBehaviour
{
    public Message[] messages;
    public Actor[] actors;
    public void StartDialogue()
    {
        FindFirstObjectByType<DialogueManager>().openDialogue(messages, actors);
    }
}
    [System.Serializable]
    public class Message
    {
        public int actorId;
        public string text;
    }
    [System.Serializable]
    public class Actor
    {
        public Sprite sprite;
        public string name;
    }
