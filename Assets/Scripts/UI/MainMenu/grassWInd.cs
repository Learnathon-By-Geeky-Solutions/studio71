using UnityEngine;
using DG.Tweening;

public class GrassWind : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float minMovementDuration = 0.5f;
    [SerializeField] private float maxMovementDuration = 1.5f;
    
    [Header("X Axis (Side to Side)")]
    [SerializeField] private float xMovementAmount = 0.05f;
    
    [Header("Y Axis (Up and Down)")]
    [SerializeField] private float yMovementAmount = 0.03f;
    
    [Header("Z Axis (Forward and Back)")]
    [SerializeField] private float zMovementAmount = 0.08f;
    
    [Header("Rotation")]
    [SerializeField] private float rotationAmount = 3f;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Sequence currentSequence;
    
    private void Start()
    {
        // Store original position and rotation
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        
        // Start the continuous movement
        StartGrassMovement();
    }
    
    private void StartGrassMovement()
    {
        // Kill any existing sequence to avoid conflicts
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }
        
        // Create a new sequence
        currentSequence = DOTween.Sequence();
        
        // Randomize the movement duration
        float movementDuration = Random.Range(minMovementDuration, maxMovementDuration);
        
        // Generate random offset values
        float randomX = Random.Range(-xMovementAmount, xMovementAmount);
        float randomY = Random.Range(-yMovementAmount, yMovementAmount);
        float randomZ = Random.Range(-zMovementAmount, zMovementAmount);
        Vector3 targetPosition = originalPosition + new Vector3(randomX, randomY, randomZ);
        
        // Generate random rotation values
        float randomRotX = Random.Range(-rotationAmount, rotationAmount);
        float randomRotZ = Random.Range(-rotationAmount, rotationAmount);
        Quaternion targetRotation = Quaternion.Euler(
            originalRotation.eulerAngles.x + randomRotX,
            originalRotation.eulerAngles.y,
            originalRotation.eulerAngles.z + randomRotZ
        );
        
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
    
    private void OnDisable()
    {
        // Kill the sequence when the object is disabled
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }
    }
}