using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Message[] messages;
    public Actor[] actors;
    public void StartDialogue(){
        FindFirstObjectByType<DialogueManager>().OpenDialogue(messages, actors);
    }
}
[System.Serializable]
public class Message{
    public int actorId;
    public string message;
}
[System.Serializable]
public class Actor{
    public string name;
    public Sprite sprite;
}