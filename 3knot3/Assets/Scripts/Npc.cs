using UnityEngine;

public class Npc : MonoBehaviour
{
public DialogueTrigger trigger;
public void OnCollisionEnter(Collision collision){
    if(collision.gameObject.tag=="Player")
        {trigger.StartDialogue();
        Debug.Log("collison");
        }

}
}

