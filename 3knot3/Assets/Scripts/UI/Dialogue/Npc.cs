using UnityEngine;


/// <summary>
/// Represents an NPC with a dialogue trigger.
/// </summary>
namespace dialogue{
 public class Npc : MonoBehaviour
    {
        [SerializeField] private TextAsset inkJSON;
        private InkDialogueManager dialogueManager;

        private void Awake()
        {
            // Get the InkDialogueManager component
            dialogueManager = GetComponent<InkDialogueManager>();
            
            // If not found on this object, try to find it in the scene
            if (dialogueManager == null)
            {
                dialogueManager = FindFirstObjectByType<InkDialogueManager>();
            }
        }

        public void TriggerDialogue()
        {
            if (inkJSON == null)
            {
                Debug.LogError($"No ink JSON file assigned to NPC {gameObject.name}");
                return;
            }

            if (dialogueManager == null)
            {
                Debug.LogError($"No InkDialogueManager found for NPC {gameObject.name}");
                return;
            }

            dialogueManager.StartDialogue(inkJSON);
        }
    }
}