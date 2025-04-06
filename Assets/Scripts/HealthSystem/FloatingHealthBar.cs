using UnityEngine;
using UnityEngine.UI;

namespace HealthSystem
{
    public class FloatingHealthBar : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private float heightOffset = 1.5f;

        private Health healthComponent;
        private Camera mainCamera;

        private void Awake()
        {
            // Find health component on parent
            healthComponent = GetComponentInParent<Health>();

            if (healthComponent == null)
            {
                Debug.LogError("No Health component found on parent object!");
            }

            // Get main camera reference
            mainCamera = Camera.main;

            // Configure canvas to work in world space
            canvas.renderMode = RenderMode.WorldSpace;

            // Set slider range
            if (healthComponent != null)
            {
                healthSlider.maxValue = healthComponent.MaxHealth;
                healthSlider.value = healthComponent.CurrentHealth;
            }
        }

        private void LateUpdate()
        {
            if (healthComponent == null || mainCamera == null) return;

            // Update health bar using slider
            healthSlider.value = healthComponent.CurrentHealth;

            // Position health bar above entity
            Vector3 position = transform.parent.position;
            position.y += heightOffset;
            canvas.transform.position = position;

            // Make the health bar face the camera (billboard effect)
            canvas.transform.rotation = mainCamera.transform.rotation;
        }
    }
}