using UnityEngine;
using UnityEngine.UI;

namespace HealthSystem
{
    public class FloatingHealthBar : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private float heightOffset = 1.5f;
        [SerializeField] private Vector3 rotation = new Vector3(60, 0, 0);
        
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
            
            // Configure canvas to face camera and work in world space
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.rotation = Quaternion.Euler(rotation);
            
            // Set slider range
            if (healthComponent != null)
            {
                healthSlider.maxValue = healthComponent.MaxHealth;
                healthSlider.value = healthComponent.CurrentHealth;
            }
        }
        
        private void LateUpdate()
        {
            if (healthComponent == null) return;
            
            // Update health bar using slider
            healthSlider.value = healthComponent.CurrentHealth;
            
            // Position health bar above entity
            Vector3 position = transform.parent.position;
            position.y += heightOffset;
            canvas.transform.position = position;
        }
    }
}