using UnityEngine;

public class Npc : MonoBehaviour
{
     public DialogueTrigger trigger;
 
     public void TriggerDialogue()
    {
        if (trigger != null)
        {
            trigger.StartDialogue();
        }
    }
}
