using UnityEngine;
using DG.Tweening;

namespace UI.MainMenu
{
    /// <summary>
    /// Simulates wind effects on grass elements by applying continuous random
    /// movement and rotation for a natural swaying effect.
    /// </summary>
    public class GrassWindAttached : MonoBehaviour
    {
        #region Configuration
        [Header("Movement Settings")]
        [Tooltip("Minimum time for a single movement cycle")]
        [SerializeField] private float minMovementDuration = 0.5f;
        
        [Tooltip("Maximum time for a single movement cycle")]
        [SerializeField] private float maxMovementDuration = 1.5f;
        
        [Header("X Axis (Side to Side)")]
        [Tooltip("Maximum distance to move horizontally")]
        [SerializeField] private float xMovementAmount = 0.05f;
        
        [Header("Y Axis (Up and Down)")]
        [Tooltip("Maximum distance to move vertically")]
        [SerializeField] private float yMovementAmount = 0.03f;
        
        [Header("Z Axis (Forward and Back)")]
        [Tooltip("Maximum distance to move in depth")]
        [SerializeField] private float zMovementAmount = 0.08f;
        
        [Header("Rotation")]
        [Tooltip("Maximum rotation angle in degrees")]
        [SerializeField] private float rotationAmount = 3f;
        #endregion
        
        #region Private Fields
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Sequence currentSequence;
        #endregion
        
        #region Unity Lifecycle Methods
        private void Awake()
        {
            // Store original position and rotation
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }
        
        private void Start()
        {
            ValidateParameters();
            StartGrassMovement();
        }
        
        private void OnDisable()
        {
            CleanupSequence();
        }
        
        private void OnDestroy()
        {
            CleanupSequence();
        }
        #endregion
        
        #region Movement Methods
        /// <summary>
        /// Validates movement parameters and applies defaults if needed
        /// </summary>
        private void ValidateParameters()
        {
            if (minMovementDuration <= 0)
            {
                Debug.LogWarning($"Invalid minMovementDuration ({minMovementDuration}). Setting to default.", this);
                minMovementDuration = 0.5f;
            }
            
            if (maxMovementDuration <= minMovementDuration)
            {
                Debug.LogWarning($"Invalid maxMovementDuration ({maxMovementDuration}). Setting based on minimum value.", this);
                maxMovementDuration = minMovementDuration + 0.5f;
            }
        }
        
        /// <summary>
        /// Starts a new random grass movement sequence
        /// </summary>
        private void StartGrassMovement()
        {
            CleanupSequence();
            
            // Create a new sequence
            currentSequence = DOTween.Sequence();
            
            // Generate random movement parameters
            float movementDuration = Random.Range(minMovementDuration, maxMovementDuration);
            Vector3 targetPosition = GenerateRandomOffset();
            Quaternion targetRotation = GenerateRandomRotation();
            
            // Add position and rotation tweens to the sequence
            currentSequence.Append(transform.DOLocalMove(targetPosition, movementDuration)
                .SetEase(Ease.InOutSine));
            currentSequence.Join(transform.DOLocalRotateQuaternion(targetRotation, movementDuration)
                .SetEase(Ease.InOutSine));
            
            // When complete, start a new movement
            currentSequence.OnComplete(() => {
                StartGrassMovement();
            });
        }
        
        /// <summary>
        /// Generates a random position offset based on the configured movement limits
        /// </summary>
        private Vector3 GenerateRandomOffset()
        {
            float randomX = Random.Range(-xMovementAmount, xMovementAmount);
            float randomY = Random.Range(-yMovementAmount, yMovementAmount);
            float randomZ = Random.Range(-zMovementAmount, zMovementAmount);
            
            return originalPosition + new Vector3(randomX, randomY, randomZ);
        }
        
        /// <summary>
        /// Generates a random rotation based on the configured rotation limits
        /// </summary>
        private Quaternion GenerateRandomRotation()
        {
            float randomRotX = Random.Range(-rotationAmount, rotationAmount);
            float randomRotZ = Random.Range(-rotationAmount, rotationAmount);
            
            return Quaternion.Euler(
                originalRotation.eulerAngles.x + randomRotX,
                originalRotation.eulerAngles.y,
                originalRotation.eulerAngles.z + randomRotZ
            );
        }
        
        /// <summary>
        /// Cleans up any active DOTween sequences
        /// </summary>
        private void CleanupSequence()
        {
            // Kill any existing sequence to avoid conflicts
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.Kill();
                currentSequence = null;
            }
        }
        #endregion
    }
}