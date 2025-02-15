using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player interaction.
/// </summary>
namespace Interaction
{
    public class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private float _interactionRadius = 2f; // Interaction distance
        [SerializeField] private GameObject interactionButton;  // UI Button
        [SerializeField] private TextMeshProUGUI interactionText; // Button Text
        
        private Collider _currentTarget;
        

        void Update()
        {  
            DetectNearestInteractable();
        }

        private void DetectNearestInteractable()
        {   
            if (DialogueManager.isDialogueOpen) return; // Don't detect new objects during dialogue

            Collider[] hits = Physics.OverlapSphere(transform.position, _interactionRadius);
            Collider nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Npc") || hit.CompareTag("Interactable"))
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearest = hit;
                        nearestDistance = distance;
                    }
                }
            }

            if (nearest != null)
            {
                _currentTarget = nearest;
                interactionButton.SetActive(true); // Show UI button

                if (_currentTarget.CompareTag("Npc"))
                {
                    interactionText.text = "Talk"; // Change text for NPCs
                }
                else if (_currentTarget.CompareTag("Interactable"))
                {
                    interactionText.text = "Pick Up Item"; // Change text for interactables
                }
            }
            else
            {
                _currentTarget = null;
                interactionButton.SetActive(false); // Hide UI button
            }
        }

        public void Interact()
        { 
            interactionButton.SetActive(false); // Hide button when interaction starts
            
            if (_currentTarget != null)
            {
                if (_currentTarget.CompareTag("Npc"))
                {   
                    
                    StartDialogue(_currentTarget.gameObject);
                }
                else if (_currentTarget.CompareTag("Interactable"))
                {   
                    PickUpItem(_currentTarget.gameObject);
                }
            }
        }

        private void StartDialogue(GameObject npc)
        {
            Debug.Log($"Starting dialogue with {npc.name}");
            Npc npcComponent = npc.GetComponent<Npc>();
            if (npcComponent != null)
            {
                npcComponent.TriggerDialogue();
            }
        }

        private void PickUpItem(GameObject item)
        {
            Debug.Log($"Picked up {item.name}");
            Destroy(item);
        }

       
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
