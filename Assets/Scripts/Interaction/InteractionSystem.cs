using TMPro;
using System.Collections;
using UnityEngine;
using dialogue;
using Player;

namespace Interaction
{
    /// <summary>
    /// Manages detection and interaction with objects in the game world.
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float _interactionRadius = 2f;
        [SerializeField] private float _detectionInterval = 0.2f; // How often to check for interactables
        
        [Header("UI References")]
        [SerializeField] private GameObject _interactionButton;
        [SerializeField] private TextMeshProUGUI _interactionText;

        public Collider _currentTarget { get; private set; }
        private PlayerAnimation _playerAnimation;
        private float _nextDetectionTime;

        private void Awake()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
            if (_playerAnimation == null)
            {
                Debug.LogWarning($"No PlayerAnimation component found on {gameObject.name}");
            }
        }

        private void Start()
        {
            if (_interactionButton == null)
            {
                Debug.LogWarning("Interaction button is not assigned!");
            }
            
            if (_interactionText == null)
            {
                Debug.LogWarning("Interaction text is not assigned!");
            }
        }

        private void Update()
        {
            // Check for interactables at specified intervals instead of every frame
            if (Time.time >= _nextDetectionTime)
            {
                FindNearestInteractable();
                _nextDetectionTime = Time.time + _detectionInterval;
            }
        }

        /// <summary>
        /// Detects and sets the nearest interactable object.
        /// </summary>
        private void FindNearestInteractable()
        {
            // Don't detect new objects during dialogue
            if (InkDialogueManager.IsDialogueOpen) 
            {
                return;
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, _interactionRadius);
            Collider nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (hit != null && (hit.CompareTag("Npc") || hit.CompareTag("Interactable")))
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearest = hit;
                        nearestDistance = distance;
                    }
                }
            }

            UpdateInteractionUI(nearest);
        }

        /// <summary>
        /// Updates the interaction UI based on the detected object.
        /// </summary>
        private void UpdateInteractionUI(Collider nearest)
        {
            if (nearest != null)
            {
                _currentTarget = nearest;
                
                if (_interactionButton != null && !_currentTarget.CompareTag("Npc"))
                {
                    _interactionButton.SetActive(true);
                     _interactionText.text = "Pick Up Item";
                }
                
                if (_interactionText != null && _currentTarget.CompareTag("Npc"))
                {
                    Interact();
                }
            }
            else
            {
                _currentTarget = null;
                
                if (_interactionButton != null)
                {
                    _interactionButton.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Handles interaction with the current target.
        /// Called by the interaction button or key press.
        /// </summary>
        public void Interact()
        {
            if (_interactionButton != null)
            {
                _interactionButton.SetActive(false);
            }

            if (_currentTarget == null)
            {
                return;
            }

            if (_currentTarget.CompareTag("Npc"))
            {
                TriggerNpcDialogue(_currentTarget.gameObject);
            }
            if (_currentTarget.CompareTag("Interactable"))
            {
                if (_playerAnimation != null && _playerAnimation.AnimationLength.ContainsKey("Pick Up"))
                {
                    StartCoroutine(DelayedAction(_playerAnimation.AnimationLength["Pick Up"], () =>
                    {
                        HandleItemPickup(_currentTarget.gameObject);
                    }));
                }
                else
                {
                    HandleItemPickup(_currentTarget.gameObject);
                }
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

            var npcComponent = npc.GetComponent<NpcDialogueTrigger>();
            if (npcComponent == null)
            {
                Debug.LogError($"No NpcDialogueTrigger component found on GameObject {npc.name}");
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
            if (item == null)
            {
                return;
            }
            
            Debug.Log($"Picked up {item.name}");
            Destroy(item);
        }

        /// <summary>
        /// Executes an action after a specified delay.
        /// </summary>
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
