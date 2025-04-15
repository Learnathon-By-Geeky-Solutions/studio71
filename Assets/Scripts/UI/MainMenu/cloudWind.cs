using UnityEngine;
using DG.Tweening;
using System.Collections;

public class cloudWind : MonoBehaviour
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
    
    private void Start()
    {
        // Calculate screen width in world units
        screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
        
        // Start the movement sequence
        StartHorizontalMovement();
    }
    
    private void StartHorizontalMovement()
    {
        // Determine target X position based on direction
        float targetX = movingRight ? 
            screenWidth/2 + GetComponent<SpriteRenderer>().bounds.size.x/2 : 
            -screenWidth/2 - GetComponent<SpriteRenderer>().bounds.size.x/2;
            
        // Randomly select speed for this movement
        float currentSpeed = Random.Range(minHorizontalSpeed, maxHorizontalSpeed);
        
        // Calculate duration based on distance and speed
        float distance = Mathf.Abs(targetX - transform.position.x);
        float duration = distance / currentSpeed;
        
        // Create the horizontal movement sequence
        Sequence moveSequence = DOTween.Sequence();
        
        // Add horizontal movement
        moveSequence.Append(transform.DOMoveX(targetX, duration).SetEase(Ease.Linear));
        
        // Add occasional vertical movements during horizontal travel
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            // Determine when to add vertical movement
            float segmentDuration = Random.Range(duration * 0.15f, duration * 0.3f);
            
            // Make sure we don't exceed the total duration
            if (timeElapsed + segmentDuration > duration)
                segmentDuration = duration - timeElapsed;
                
            moveSequence.InsertCallback(timeElapsed, () => {
                // Random chance to move vertically
                if (Random.value < verticalMovementChance)
                {
                    // Determine up or down
                    float verticalTarget = transform.position.y + (Random.value > 0.5f ? verticalDistance : -verticalDistance);
                    transform.DOMoveY(verticalTarget, verticalDuration).SetEase(Ease.InOutSine);
                }
                
                // Random chance to pause
                if (Random.value < pauseChance)
                {
                    moveSequence.Pause();
                    DOVirtual.DelayedCall(pauseDuration, () => moveSequence.Play());
                }
                
                // Random chance to change speed
                horizontalSpeed = Random.Range(minHorizontalSpeed, maxHorizontalSpeed);
            });
            
            timeElapsed += segmentDuration;
        }
        
        // When complete, flip direction and start again
        moveSequence.OnComplete(() => {
            movingRight = !movingRight;
            // Reset position to just off screen on the opposite side
            Vector3 newPos = transform.position;
            newPos.x = movingRight ? 
                -screenWidth/2 - GetComponent<SpriteRenderer>().bounds.size.x/2 : 
                screenWidth/2 + GetComponent<SpriteRenderer>().bounds.size.x/2;
            transform.position = newPos;
            
            // Start the sequence again
            StartHorizontalMovement();
        });
    }
}