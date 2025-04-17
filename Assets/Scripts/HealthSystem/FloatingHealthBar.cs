using UnityEngine;
using UnityEngine.UI;

namespace HealthSystem
{
    /// <summary>
    /// Manages a floating health bar above an entity with a Health component.
    /// </summary>
    public class FloatingHealthBar : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private float heightOffset = 1.5f;

        private Health healthComponent;
        private Camera mainCamera;
        private float lastHealthValue;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Find health component on parent
            healthComponent = GetComponentInParent<Health>();

            if (healthComponent == null)
            {
                Debug.LogError("No Health component found on parent object!", this);
                enabled = false;
                return;
            }

            // Get main camera reference
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found in the scene!", this);
                enabled = false;
                return;
            }

            // Configure canvas to work in world space
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                
                // Set initial position
                UpdatePosition();
                
                // Set initial rotation
                UpdateRotation();
            }
            else
            {
                Debug.LogError("Canvas reference is missing!", this);
                enabled = false;
            }

            // Set slider range
            if (healthSlider != null)
            {
                healthSlider.maxValue = healthComponent.MaxHealth;
                healthSlider.value = healthComponent.CurrentHealth;
                lastHealthValue = healthComponent.CurrentHealth;
            }
            else
            {
                Debug.LogError("Health slider reference is missing!", this);
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (healthComponent == null || mainCamera == null || canvas == null) 
                return;

            // Only update the slider value if health has changed
            if (lastHealthValue != healthComponent.CurrentHealth)
            {
                healthSlider.value = healthComponent.CurrentHealth;
                lastHealthValue = healthComponent.CurrentHealth;
            }

            // Position and rotate the health bar
            UpdatePosition();
            UpdateRotation();
        }
        
        /// <summary>
        /// Updates the position of the health bar above the entity.
        /// </summary>
        private void UpdatePosition()
        {
            if (transform.parent == null) return;
            
            Vector3 position = transform.parent.position;
            position.y += heightOffset;
            canvas.transform.position = position;
        }
        
        /// <summary>
        /// Updates the rotation of the health bar to face the camera.
        /// </summary>
        private void UpdateRotation()
        {
            // Make the health bar face the camera using LookAt instead of direct rotation copying
            // This prevents flipping issues that can occur with Billboard-style rotation copying
            canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.forward, Vector3.up);
        }
    }
}