using TMPro;
using System.Collections;
using UnityEngine;
using dialogue;
using Player;
// Replace 'YourNamespace' with the actual namespace where the Npc class is defined

namespace Interaction
{
    public class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private float _interactionRadius = 2f; // Interaction distance
        [SerializeField] private GameObject _interactionButton;  // UI Button
        [SerializeField] private TextMeshProUGUI _interactionText; // Button Text

        public Collider _currentTarget { get; private set; }


        //PlayerAnimation Variable
        PlayerAnimation _playerAnimation;

        private void Awake()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
        }
        private void Update()
        {
            FindNearestInteractable();
        }

        /// <summary>
        /// Detects and sets the nearest interactable object.
        /// </summary>
        private void FindNearestInteractable()
        {
            if (InkDialogueManager.IsDialogueOpen) return; // Don't detect new objects during dialogue

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

        private void Interact()
        {
            if (_interactionButton != null) _interactionButton.SetActive(false); // Hide button when interaction starts

            if (_currentTarget == null) return;

            if (_currentTarget.CompareTag("Npc"))
            {
                TriggerNpcDialogue(_currentTarget.gameObject);
            }
            else if (_currentTarget.CompareTag("Interactable"))
            {
                StartCoroutine(DelayedAction(_playerAnimation._animationLengths["Pick Up"], () =>
                {
                    HandleItemPickup(_currentTarget.gameObject);
                }));
            }
        }

        /// <summary>
        /// Starts dialogue with an NPC.
        /// </summary>
        private static void TriggerNpcDialogue(GameObject npc)
{
    if (npc == null)
    {
        Debug.LogError("Attempted to trigger dialogue with null NPC");
        return;
    }

    var npcComponent = npc.GetComponent<Npc>();
    if (npcComponent == null)
    {
        Debug.LogError($"No Npc component found on GameObject {npc.name}");
        return;
    }

    Debug.Log($"Starting dialogue with {npc.name}");
    npcComponent.TriggerDialogue();
}
        /// <summary>
        /// Handles picking up an interactable item.
        /// </summary>
        private static void HandleItemPickup(GameObject item)
        {
            Debug.Log($"Picked up {item.name}");
            Destroy(item);
        }


        private static IEnumerator DelayedAction(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
