using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace UI.MainMenu
{
    /// <summary>
    /// Controls cloud movement with simulated wind effects including horizontal drift,
    /// vertical movement, and occasional pauses to create natural cloud behavior.
    /// </summary>
    public class CloudWindAttached : MonoBehaviour
    {
        #region Configuration
        [Header("Movement Settings")]
        [Tooltip("Base horizontal movement speed")]
        [SerializeField] private float horizontalSpeed = 2f;
        
        [Tooltip("Maximum random horizontal speed")]
        [SerializeField] private float maxHorizontalSpeed = 5f;
        
        [Tooltip("Minimum random horizontal speed")]
        [SerializeField] private float minHorizontalSpeed = 1f;
        
        [Tooltip("Probability (0-1) of vertical movement during travel")]
        [Range(0f, 1f)]
        [SerializeField] private float verticalMovementChance = 0.3f;
        
        [Tooltip("Distance to move vertically (up or down)")]
        [SerializeField] private float verticalDistance = 1f;
        
        [Tooltip("Duration of vertical movement")]
        [SerializeField] private float verticalDuration = 3f;
        
        [Tooltip("Width of the screen in world units (auto-calculated if 0)")]
        [SerializeField] private float screenWidth;
        
        [Tooltip("Probability (0-1) of pausing during travel")]
        [Range(0f, 1f)]
        [SerializeField] private float pauseChance = 0.2f;
        
        [Tooltip("Duration of pause when it occurs")]
        [SerializeField] private float pauseDuration = 1.5f;
        #endregion
        
        #region Private Fields
        private bool movingRight = true;
        private SpriteRenderer _spriteRenderer;
        private bool isInitialized = false;
        #endregion
        
        #region Unity Lifecycle Methods
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            if (!isInitialized) return;
            
            // Calculate screen width in world units using the main camera if not set
            if (screenWidth <= 0)
            {
                CalculateScreenWidth();
            }
            
            // Start the movement sequence
            StartHorizontalMovement();
        }
        
       
        #endregion
        
        #region Initialization Methods
        /// <summary>
        /// Initialize required components and validate setup
        /// </summary>
        private void InitializeComponents()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                Debug.LogError("CloudWindAttached requires a SpriteRenderer component.", this);
                enabled = false; // Disable script if no renderer
                return;
            }
            
            isInitialized = true;
        }
        
        /// <summary>
        /// Calculate the screen width in world units
        /// </summary>
        private void CalculateScreenWidth()
        {
            if (Camera.main == null)
            {
                Debug.LogError("Main Camera not found. Cannot calculate screen width.", this);
                enabled = false;
                return;
            }
            
            screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
            if (screenWidth <= 0)
            {
                Debug.LogError("Invalid screen width calculation. Setting to default value.", this);
                screenWidth = 20f; // Fallback default
            }
        }
        #endregion
        
        #region Movement Methods
        /// <summary>
        /// Begin horizontal cloud movement from one side of the screen to the other
        /// </summary>
        private void StartHorizontalMovement()
        {
            if (_spriteRenderer == null) return; // Safety check
            
            // Determine target X position based on direction and sprite width
            float spriteWidth = _spriteRenderer.bounds.size.x;
            float targetX = movingRight ? 
                (screenWidth / 2) + (spriteWidth / 2) :
                (-screenWidth / 2) - (spriteWidth / 2);
                
            // Randomly select speed for this movement
            float currentSpeed = Random.Range(minHorizontalSpeed, maxHorizontalSpeed);
            
            // Calculate duration based on distance and speed
            float distance = Mathf.Abs(targetX - transform.position.x);
            // Avoid division by zero if speed is somehow zero
            float duration = currentSpeed > 0 ? distance / currentSpeed : 10f; 
            
            // Create the horizontal movement sequence
            Sequence moveSequence = DOTween.Sequence();
            
            // Add horizontal movement
            moveSequence.Append(transform.DOMoveX(targetX, duration).SetEase(Ease.Linear));
            
            // Schedule occasional checks during horizontal travel
            ScheduleMovementSegments(moveSequence, duration);
            
            // When complete, handle completion
            moveSequence.OnComplete(HandleMovementCompletion);
        }

        /// <summary>
        /// Schedule periodic checkpoints during cloud movement for potential vertical shifts or pauses
        /// </summary>
        private void ScheduleMovementSegments(Sequence moveSequence, float totalDuration)
        {
            if (totalDuration <= 0) return;
            
            float timeElapsed = 0;
            while (timeElapsed < totalDuration)
            {
                // Determine duration for the next segment check
                float segmentDuration = Random.Range(totalDuration * 0.15f, totalDuration * 0.3f);
                
                // Clamp segment duration to not exceed remaining time
                segmentDuration = Mathf.Min(segmentDuration, totalDuration - timeElapsed);

                // Use a local variable for the callback to capture the current state correctly
                Sequence currentSequence = moveSequence;            
                moveSequence.InsertCallback(timeElapsed, () => HandleMovementSegment(currentSequence));
                
                timeElapsed += segmentDuration;

                // Break if somehow duration is non-positive to prevent infinite loop
                if (segmentDuration <= 0) break; 
            }
        }

        /// <summary>
        /// Handle behavior at a movement segment checkpoint - may cause vertical shifts or pauses
        /// </summary>
        private void HandleMovementSegment(Sequence sequence)
        {
            // Random chance to move vertically
            if (Random.value < verticalMovementChance)
            {
                float verticalTargetY = transform.position.y + (Random.value > 0.5f ? verticalDistance : -verticalDistance);
                // Use a short duration tween for the vertical shift
                transform.DOMoveY(verticalTargetY, verticalDuration).SetEase(Ease.InOutSine);
            }
            
            // Random chance to pause
            if (Random.value < pauseChance && sequence != null && sequence.IsActive())
            {
                sequence.Pause();
                DOVirtual.DelayedCall(pauseDuration, () => { 
                    if (sequence != null && sequence.IsActive()) 
                        sequence.Play(); 
                });
            }
        }

        /// <summary>
        /// Handle completion of the horizontal movement - reset position and start again in opposite direction
        /// </summary>
        private void HandleMovementCompletion()
        {
            if (_spriteRenderer == null) return; // Safety check

            // Flip direction
            movingRight = !movingRight;
            
            // Reset position to just off screen on the opposite side
            float spriteWidth = _spriteRenderer.bounds.size.x;
            Vector3 newPos = transform.position;
            newPos.x = movingRight ? 
                (-screenWidth / 2) - (spriteWidth / 2) :
                (screenWidth / 2) + (spriteWidth / 2);
            transform.position = newPos;
            
            // Start the sequence again
            StartHorizontalMovement();
        }
        #endregion
    }
}