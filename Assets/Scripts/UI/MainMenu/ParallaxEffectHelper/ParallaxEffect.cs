using UnityEngine;

namespace UI.MainMenu
{
    /// <summary>
    /// Creates a parallax effect by moving different layers at different speeds relative to camera movement.
    /// </summary>
    public class ParallaxEffect : MonoBehaviour
    {
        #region Serialized Data
        [System.Serializable]
        public class ParallaxLayer
        {
            [Tooltip("Transform to apply parallax effect to")]
            public Transform layerTransform;
            
            [Tooltip("How fast the layer moves relative to camera movement (0 = stationary, 1 = moves with camera)")]
            [Range(0f, 1f)]
            public float parallaxFactor;
        }
        
        [Tooltip("Reference to the camera - if not set, will use Camera.main")]
        [SerializeField] private Transform cameraTransform;
        
        [Tooltip("Layers to apply parallax effect to, with their movement factors")]
        [SerializeField] private ParallaxLayer[] parallaxLayers;
        #endregion

        #region Private Fields
        private Vector3 previousCameraPosition;
        private bool isInitialized = false;
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            InitializeCamera();
        }

        private void OnEnable()
        {
            if (isInitialized)
            {
                // Reset the previous camera position when enabled to prevent jumps
                previousCameraPosition = cameraTransform.position;
            }
        }

        private void LateUpdate()
        {
            if (!isInitialized)
                return;
                
            UpdateLayerPositions();
        }
        #endregion

        #region Parallax Methods
        /// <summary>
        /// Find and set up the camera reference.
        /// </summary>
        private void InitializeCamera()
        {
            if (cameraTransform == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("No main camera found and no camera assigned to ParallaxEffect.", this);
                    enabled = false;
                    return;
                }
                
                cameraTransform = mainCamera.transform;
            }
            
            previousCameraPosition = cameraTransform.position;
            isInitialized = true;
            
            // Verify layers setup
            if (parallaxLayers == null || parallaxLayers.Length == 0)
            {
                Debug.LogWarning("No parallax layers assigned to ParallaxEffect.", this);
            }
        }
        
        /// <summary>
        /// Updates all layer positions based on camera movement and their parallax factors.
        /// </summary>
        private void UpdateLayerPositions()
        {
            Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;
            
            // Only process if camera has moved
            if (deltaMovement.sqrMagnitude > 0)
            {
                foreach (ParallaxLayer layer in parallaxLayers)
                {
                    if (layer?.layerTransform != null)
                    {
                        // Move the layer by a fraction of the camera's movement
                        // Layers with smaller parallaxFactor will move slower (background)
                        // Layers with larger parallaxFactor will move faster (foreground)
                        Vector3 parallaxMovement = deltaMovement * layer.parallaxFactor;
                        
                        // Only move in X and Y (since we're working with Canvas in 2D space)
                        layer.layerTransform.position += new Vector3(parallaxMovement.x, parallaxMovement.y, 0);
                    }
                }
            }
            
            previousCameraPosition = cameraTransform.position;
        }
        #endregion
    }
}