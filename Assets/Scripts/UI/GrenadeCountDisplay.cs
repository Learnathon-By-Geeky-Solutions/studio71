using UnityEngine;
using TMPro;
using Player;
using System.Collections;

namespace UI
{
    /// <summary>
    /// Displays the current grenade count on the UI.
    /// Updates only when the count changes, not every frame.
    /// </summary>
    public class GrenadeCountDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI grenadeCountText;
        [SerializeField] private string textPrefix = ""; // Can be empty for just the number
        [SerializeField] private string textSuffix = ""; // Can be empty for just the number
        
        private PlayerController playerController;
        
        private void Awake()
        {
            if (grenadeCountText == null)
            {
                grenadeCountText = GetComponent<TextMeshProUGUI>();
                
                if (grenadeCountText == null)
                {
                    Debug.LogError("No TextMeshProUGUI component found for GrenadeCountDisplay. Please assign one in the inspector.");
                    enabled = false;
                }
            }
        }
        
        private void Start()
        {
            // Wait a frame to ensure everything is initialized
            StartCoroutine(InitializeAfterFirstFrame());
        }
        
        private IEnumerator InitializeAfterFirstFrame()
        {
            yield return null; // Wait one frame
            
            // Find the player controller
            playerController = FindObjectOfType<PlayerController>();
            
            if (playerController == null)
            {
                Debug.LogError("No PlayerController found in scene. GrenadeCountDisplay will not function.");
                yield break;
            }
            
            // Subscribe to the grenade count change event
            playerController.OnGrenadeCountChanged += UpdateGrenadeCountDisplay;
            
            // Initialize with current count
            UpdateGrenadeCountDisplay(playerController.GrenadeCount);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe when destroyed to prevent memory leaks
            if (playerController != null)
            {
                playerController.OnGrenadeCountChanged -= UpdateGrenadeCountDisplay;
            }
        }
        
        private void UpdateGrenadeCountDisplay(int count)
        {
            if (grenadeCountText != null)
            {
                grenadeCountText.text = $"{textPrefix}{count}{textSuffix}";
            }
        }
    }
} 