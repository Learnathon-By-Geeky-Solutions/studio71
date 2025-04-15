using UnityEngine;

namespace UI.MainMenu
{
    public class ParallaxEffect : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public Transform layerTransform;
            [Range(0f, 1f)]
            public float parallaxFactor;
        }

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private ParallaxLayer[] parallaxLayers;

        private Vector3 previousCameraPosition;

        private void Start()
        {
            if (cameraTransform == null)
                cameraTransform = Camera.main.transform;
                
            previousCameraPosition = cameraTransform.position;
        }

        private void LateUpdate()
        {
            Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;
            
            foreach (ParallaxLayer layer in parallaxLayers)
            {
                if (layer.layerTransform != null)
                {
                    // Move the layer by a fraction of the camera's movement
                    // Layers with smaller parallaxFactor will move slower (background)
                    // Layers with larger parallaxFactor will move faster (foreground)
                    Vector3 parallaxMovement = deltaMovement * layer.parallaxFactor;
                    
                    // Only move in X and Y (since we're working with Canvas in 2D space)
                    layer.layerTransform.position += new Vector3(parallaxMovement.x, parallaxMovement.y, 0);
                }
            }
            
            previousCameraPosition = cameraTransform.position;
        }
    }
}