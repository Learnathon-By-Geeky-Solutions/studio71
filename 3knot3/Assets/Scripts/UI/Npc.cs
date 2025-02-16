using UnityEngine;

namespace UI.DialogueSystem{
/// <summary>
/// Represents an NPC with a dialogue trigger.
/// </summary>
public class Npc : MonoBehaviour
{
    [SerializeField] private DialogueTrigger _dialogueTrigger;

    public void TriggerDialogue()
    {
        _dialogueTrigger?.StartDialogue();
    }
}
}