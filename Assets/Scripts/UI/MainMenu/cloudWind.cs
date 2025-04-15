using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace UI.MainMenu
{
    public class CloudWind : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float horizontalSpeed = 2f;
        [SerializeField] private float maxHorizontalSpeed = 5f;
        [SerializeField] private float minHorizontalSpeed = 1f;
        [SerializeField] private float verticalMovementChance = 0.3f;
        [SerializeField] private float verticalDistance = 1f;
        [SerializeField] private float verticalDuration = 3f;
        [SerializeField] private float screenWidth;
        [SerializeField] private float pauseChance = 0.2f;
        [SerializeField] private float pauseDuration = 1.5f;
        
        private bool movingRight = true;
        private SpriteRenderer _spriteRenderer;
        
        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                Debug.LogError("cloudWind requires a SpriteRenderer component.", this);
                enabled = false; // Disable script if no renderer
                return;
            }
            
            // Calculate screen width in world units using the main camera
            if (Camera.main == null)
            {
                 Debug.LogError("Main Camera not found. Cannot calculate screen width.", this);
                 enabled = false;
                 return;
            }
            screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
            
            // Start the movement sequence
            StartHorizontalMovement();
        }
        
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
            float duration = currentSpeed > 0 ? distance / currentSpeed : float.MaxValue; 
            
            // Create the horizontal movement sequence
            Sequence moveSequence = DOTween.Sequence();
            
            // Add horizontal movement
            moveSequence.Append(transform.DOMoveX(targetX, duration).SetEase(Ease.Linear));
            
            // Schedule occasional checks during horizontal travel
            ScheduleMovementSegments(moveSequence, duration);
            
            // When complete, handle completion
            moveSequence.OnComplete(HandleMovementCompletion);
        }

        // Extracted method to schedule segment checks
        private void ScheduleMovementSegments(Sequence moveSequence, float totalDuration)
        {
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

        // Extracted method to handle logic within a movement segment
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
            if (Random.value < pauseChance)
            {
                sequence.Pause();
                DOVirtual.DelayedCall(pauseDuration, () => { if (sequence != null && sequence.IsActive()) sequence.Play(); });
            }
            
            // Random chance to change speed (Note: This doesn't change the current tween's duration)
            // horizontalSpeed = Random.Range(minHorizontalSpeed, maxHorizontalSpeed); // This doesn't affect the *current* tween
        }

        // Extracted method to handle sequence completion
        private void HandleMovementCompletion()
        {
            if (_spriteRenderer == null) return; // Safety check

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
    }
}