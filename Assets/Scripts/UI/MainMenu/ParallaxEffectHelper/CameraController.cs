using UnityEngine;

namespace UI.MainMenu
{
    /// <summary>
    /// Controls camera movement in response to mouse position to create a dynamic scene effect.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        #region Configurable Parameters
        [Header("Movement Settings")]
        [Tooltip("How quickly the camera moves in response to mouse movement")]
        [SerializeField] private float mouseSensitivity = 0.5f;
        
        [Tooltip("Time in seconds to smooth camera movement")]
        [SerializeField] private float smoothTime = 0.3f;
        
        [Tooltip("Min and max X movement bounds")]
        [SerializeField] private Vector2 horizontalBounds = new Vector2(-10f, 10f);
        
        [Tooltip("Min and max Y movement bounds")]
        [SerializeField] private Vector2 verticalBounds = new Vector2(-5f, 5f);
        #endregion

        #region Private Fields
        private Vector3 originalPosition;
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 targetPosition;
        private Camera mainCamera;
        private bool isInitialized = false;
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main camera not found. CameraController requires a camera tagged as 'MainCamera'.", this);
                enabled = false;
                return;
            }
            
            originalPosition = transform.position;
            isInitialized = true;
        }

        private void OnEnable()
        {
            if (!isInitialized)
                return;
            
            // Reset to original position when enabled
            transform.position = originalPosition;
            currentVelocity = Vector3.zero;
        }
        
        private void Update()
        {
            if (!isInitialized)
                return;
                
            UpdateTargetPosition();
            MoveCamera();
        }
        #endregion

        #region Camera Movement Methods
        /// <summary>
        /// Updates the target position based on mouse position.
        /// </summary>
        private void UpdateTargetPosition()
        {
            // Convert mouse position to viewport coordinates (0-1 range)
            Vector2 viewportPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition);
            
            // Convert to -1 to 1 range for easier movement calculation
            Vector2 mouseOffset = new Vector2(
                (viewportPosition.x - 0.5f) * 2f,
                (viewportPosition.y - 0.5f) * 2f
            );
            
            // Calculate target position based on mouse offset and constrain to bounds
            targetPosition = originalPosition + new Vector3(
                Mathf.Clamp(mouseOffset.x * mouseSensitivity, horizontalBounds.x, horizontalBounds.y),
                Mathf.Clamp(mouseOffset.y * mouseSensitivity, verticalBounds.x, verticalBounds.y),
                0
            );
        }

        /// <summary>
        /// Smoothly moves the camera toward the target position.
        /// </summary>
        private void MoveCamera()
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref currentVelocity,
                smoothTime
            );
        }
        #endregion
    }
}