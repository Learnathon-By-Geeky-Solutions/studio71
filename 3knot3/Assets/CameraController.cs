using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private Vector2 horizontalBounds = new Vector2(-10f, 10f);
    [SerializeField] private Vector2 verticalBounds = new Vector2(-5f, 5f);
    
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Vector3 currentVelocity = Vector3.zero;
    
    private void Start()
    {
        originalPosition = transform.position;
        targetPosition = originalPosition;
    }
    
    private void Update()
    {
        // Convert mouse position to viewport coordinates (0-1 range)
        Vector2 viewportPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        
        // Convert to -1 to 1 range for easier movement calculation
        Vector2 mouseOffset = new Vector2(
            (viewportPosition.x - 0.5f) * 2f,
            (viewportPosition.y - 0.5f) * 2f
        );
        
        // Calculate target position based on mouse offset
        targetPosition = originalPosition + new Vector3(
            Mathf.Clamp(mouseOffset.x * mouseSensitivity, horizontalBounds.x, horizontalBounds.y),
            Mathf.Clamp(mouseOffset.y * mouseSensitivity, verticalBounds.x, verticalBounds.y),
            0
        );
        
        // Smoothly move camera to target position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }
}