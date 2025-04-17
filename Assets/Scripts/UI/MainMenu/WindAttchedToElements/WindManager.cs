using UnityEngine;
using DG.Tweening;
using System.Linq;

namespace UI.MainMenu
{
    /// <summary>
    /// Manages swaying animation for multiple objects to simulate wind effects.
    /// </summary>
    public class WindManager : MonoBehaviour
    {
        #region Configuration
        [Header("Wind Settings")]
        [Tooltip("Objects that will be animated to sway in the wind")]
        [SerializeField] private Transform[] objectsToSway;
        
        [Tooltip("Minimum rotation angle for swaying")]
        [SerializeField] private float minSwayAmount = 5f;
        
        [Tooltip("Maximum rotation angle for swaying")]
        [SerializeField] private float maxSwayAmount = 15f;
        
        [Tooltip("Minimum time for a complete sway cycle")]
        [SerializeField] private float minSwayDuration = 1.5f;
        
        [Tooltip("Maximum time for a complete sway cycle")]
        [SerializeField] private float maxSwayDuration = 3f;
        
        [Tooltip("Minimum delay before sway animation starts")]
        [SerializeField] private float minDelay = 0f;
        
        [Tooltip("Maximum delay before sway animation starts")]
        [SerializeField] private float maxDelay = 1f;
        #endregion

        #region Unity Lifecycle Methods
        private void Start()
        {
            ValidateParameters();
            ApplyWindEffect();
        }
        
        private void OnDisable()
        {
            CleanupTweens();
        }
        
        private void OnDestroy()
        {
            CleanupTweens();
        }
        #endregion

        #region Wind Effect Methods
        /// <summary>
        /// Validates that all parameters are properly set
        /// </summary>
        private void ValidateParameters()
        {
            if (objectsToSway == null || objectsToSway.Length == 0)
            {
                Debug.LogWarning("No objects assigned to WindManager for swaying.", this);
            }
            
            // Remove any null references
            if (objectsToSway != null && objectsToSway.Any(obj => obj == null))
            {
                Debug.LogWarning("Some objects in WindManager are null and will be ignored.", this);
                objectsToSway = objectsToSway.Where(obj => obj != null).ToArray();
            }
            
            // Ensure min values are less than max values
            if (minSwayAmount > maxSwayAmount)
            {
                Debug.LogWarning("Min sway amount is greater than max. Swapping values.", this);
                (minSwayAmount, maxSwayAmount) = (maxSwayAmount, minSwayAmount);
            }
            
            if (minSwayDuration > maxSwayDuration)
            {
                Debug.LogWarning("Min sway duration is greater than max. Swapping values.", this);
                (minSwayDuration, maxSwayDuration) = (maxSwayDuration, minSwayDuration);
            }
            
            if (minDelay > maxDelay)
            {
                Debug.LogWarning("Min delay is greater than max. Swapping values.", this);
                (minDelay, maxDelay) = (maxDelay, minDelay);
            }
        }

        /// <summary>
        /// Applies wind sway effect to all configured objects
        /// </summary>
        private void ApplyWindEffect()
        {
            if (objectsToSway == null || objectsToSway.Length == 0)
                return;
                
            foreach (Transform obj in objectsToSway)
            {
                if (obj == null)
                    continue;
                    
                // Generate random values for sway amount, duration, and delay
                float randomSwayAmount = Random.Range(minSwayAmount, maxSwayAmount);
                float randomDuration = Random.Range(minSwayDuration, maxSwayDuration);
                float randomDelay = Random.Range(minDelay, maxDelay);

                // Animate rotation with random parameters
                obj.DORotate(new Vector3(0, 0, randomSwayAmount), randomDuration)
                   .SetEase(Ease.InOutSine) // Smooth easing for natural movement
                   .SetLoops(-1, LoopType.Yoyo) // Infinite looping with Yoyo effect
                   .SetDelay(randomDelay)
                   .SetId(obj.GetInstanceID()); // Set ID for easier cleanup
            }
        }
        
        /// <summary>
        /// Cleans up all active DOTween animations
        /// </summary>
        private void CleanupTweens()
        {
            if (objectsToSway == null)
                return;
                
            foreach (Transform obj in objectsToSway)
            {
                if (obj != null)
                {
                    DOTween.Kill(obj.GetInstanceID());
                }
            }
        }
        #endregion
    }
}
