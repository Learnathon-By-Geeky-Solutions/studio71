using UnityEngine;

namespace dialogue
{
    /// <summary>
    /// Represents an NPC that can trigger dialogue interactions.
    /// Provides functionality to start a dialogue sequence using an Ink JSON file.
    /// </summary>
    public class NpcDialogueTrigger : MonoBehaviour
    {
        [SerializeField] private TextAsset inkJSON;
        private InkDialogueManager dialogueManager;
        private bool _alreadyTriggerd = false;
        private void Awake()
        {
            // Try to get the InkDialogueManager component
            dialogueManager = GetComponent<InkDialogueManager>();
            
            // If not found on this object, try to find it in the scene
            if (dialogueManager == null)
            {
                dialogueManager = FindFirstObjectByType<InkDialogueManager>();
                if (dialogueManager == null)
                {
                    Debug.LogWarning($"No InkDialogueManager found for NPC {gameObject.name}. Dialogue will not function.");
                }
            }
        }

        /// <summary>
        /// Starts the dialogue sequence using the assigned Ink JSON file.
        /// </summary>
        public void TriggerDialogue()
        {
            if (_alreadyTriggerd) return; 
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
            _alreadyTriggerd=true;
            dialogueManager.StartDialogue(inkJSON);
        }
    }
}