using TMPro;
using UnityEngine;

namespace Interaction
{
    public class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private float _interactionRadius = 2f; // Interaction distance
        [SerializeField] private GameObject _interactionButton;  // UI Button
        [SerializeField] private TextMeshProUGUI _interactionText; // Button Text

        private Collider _currentTarget;

        private void Update()
        {
            FindNearestInteractable();
        }

        /// <summary>
        /// Detects and sets the nearest interactable object.
        /// </summary>
        private void FindNearestInteractable()
        {
            if (DialogueManager.IsDialogueOpen) return; // Don't detect new objects during dialogue

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
                _interactionButton?.SetActive(true); // Show UI button safely

                _interactionText.text = _currentTarget.CompareTag("Npc") ? "Talk" : "Pick Up Item";
            }
            else
            {
                _currentTarget = null;
                _interactionButton?.SetActive(false); // Hide UI button safely
            }
        }

        public void Interact()
        {
            if (_interactionButton != null) _interactionButton.SetActive(false); // Hide button when interaction starts

            if (_currentTarget == null) return;

            if (_currentTarget.CompareTag("Npc"))
            {
                TriggerNpcDialogue(_currentTarget.gameObject);
            }
            else if (_currentTarget.CompareTag("Interactable"))
            {
                HandleItemPickup(_currentTarget.gameObject);
            }
        }

        /// <summary>
        /// Starts dialogue with an NPC.
        /// </summary>
        private void TriggerNpcDialogue(GameObject npc)
        {
            Debug.Log($"Starting dialogue with {npc.name}");
            if (npc.TryGetComponent(out Npc npcComponent))
            {
                npcComponent.TriggerDialogue();
            }
        }

        /// <summary>
        /// Handles picking up an interactable item.
        /// </summary>
        private void HandleItemPickup(GameObject item)
        {
            Debug.Log($"Picked up {item.name}");
            Destroy(item);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
