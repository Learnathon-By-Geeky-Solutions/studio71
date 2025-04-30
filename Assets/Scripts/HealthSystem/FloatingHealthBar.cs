using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
        
        [Header("Visual Effects")]
        [Tooltip("Duration of the health value animation in seconds")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 1f); // Red
        [SerializeField] private Color healFlashColor = new Color(0f, 1f, 0f, 1f); // Green
        [SerializeField] private float flashDuration = 0.3f;
        [SerializeField] private float pulseSizeMultiplier = 1.2f;
        [SerializeField] private float pulseSpeed = 5f;
        
        private Image fillImage;
        private Color originalFillColor;
        private Vector3 originalScale;
        private Health healthComponent;
        private Camera mainCamera;
        private float lastHealthValue;
        private Coroutine healthChangeCoroutine;
        private Coroutine colorFlashCoroutine;
        private Coroutine pulseCoroutine;

        private const float Epsilon = 0.001f; // Small tolerance for float comparisons

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
                
                // Store original scale for pulsing effects
                originalScale = canvas.transform.localScale;
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
                
                // Get the fill image for color effects
                fillImage = healthSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    originalFillColor = fillImage.color;
                }
                else
                {
                    Debug.LogWarning("Health slider fill image not found, color effects won't work.", this);
                }
            }
            else
            {
                Debug.LogError("Health slider reference is missing!", this);
                enabled = false;
            }
            
            // Subscribe to health change events
            if (healthComponent != null)
            {
                healthComponent.OnHealthChanged.AddListener(OnHealthValueChanged);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event listeners
            if (healthComponent != null)
            {
                healthComponent.OnHealthChanged.RemoveListener(OnHealthValueChanged);
            }
        }

        private void LateUpdate()
        {
            if (healthComponent == null || mainCamera == null || canvas == null) 
                return;

            // Position and rotate the health bar
            UpdatePosition();
            UpdateRotation();
        }
        
        /// <summary>
        /// Called when the health value changes.
        /// </summary>
        private void OnHealthValueChanged(float newHealth)
        {
            // Don't animate if this is the initial setup (from near 0 to near max)
            if (Mathf.Abs(lastHealthValue - 0f) < Epsilon && 
                Mathf.Abs(newHealth - healthComponent.MaxHealth) < Epsilon)
            {
                healthSlider.value = newHealth;
                lastHealthValue = newHealth;
                return;
            }
            
            // Determine if damage or healing occurred
            bool isDamage = newHealth < lastHealthValue;
            
            // Stop any running animations
            if (healthChangeCoroutine != null)
                StopCoroutine(healthChangeCoroutine);
                
            // Start new animation
            healthChangeCoroutine = StartCoroutine(AnimateHealthChange(lastHealthValue, newHealth));
            
            // Flash color based on damage or healing
            if (fillImage != null)
            {
                if (colorFlashCoroutine != null)
                    StopCoroutine(colorFlashCoroutine);
                    
                colorFlashCoroutine = StartCoroutine(FlashFillColor(isDamage ? damageFlashColor : healFlashColor));
            }
            
            // Pulse size effect
            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);
                
            pulseCoroutine = StartCoroutine(PulseHealthBar());
            
            // Update the last health value
            lastHealthValue = newHealth;
        }
        
        /// <summary>
        /// Smoothly animates the health slider value change.
        /// </summary>
        private IEnumerator AnimateHealthChange(float startValue, float endValue)
        {
            float elapsed = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                
                // Use a smooth easing function
                float smoothT = t * t * (3f - 2f * t); // Smoothstep formula
                
                healthSlider.value = Mathf.Lerp(startValue, endValue, smoothT);
                yield return null;
            }
            
            // Ensure we end at the exact target value
            healthSlider.value = endValue;
            healthChangeCoroutine = null;
        }
        
        /// <summary>
        /// Flashes the fill color and returns to the original color.
        /// </summary>
        private IEnumerator FlashFillColor(Color flashColor)
        {
            // Quick flash to the target color
            float elapsed = 0f;
            
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flashDuration);
                
                // Flash in and out - start with flashColor and return to originalFillColor
                fillImage.color = Color.Lerp(flashColor, originalFillColor, t);
                yield return null;
            }
            
            // Ensure we end with the original color
            fillImage.color = originalFillColor;
            colorFlashCoroutine = null;
        }
        
        /// <summary>
        /// Creates a pulse effect by scaling the health bar up and down.
        /// </summary>
        private IEnumerator PulseHealthBar()
        {
            // Quick pulse out and back in
            float elapsed = 0f;
            float pulseDuration = 0.3f;
            
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                
                // Pulse curve: quickly out and slower back in
                float pulseValue;
                if (elapsed < pulseDuration * 0.3f)
                {
                    // Scale up phase (quick)
                    pulseValue = Mathf.Lerp(1f, pulseSizeMultiplier, elapsed / (pulseDuration * 0.3f));
                }
                else
                {
                    // Scale down phase (slower)
                    pulseValue = Mathf.Lerp(pulseSizeMultiplier, 1f, (elapsed - pulseDuration * 0.3f) / (pulseDuration * 0.7f));
                }
                
                canvas.transform.localScale = originalScale * pulseValue;
                yield return null;
            }
            
            // Ensure we end at the original scale
            canvas.transform.localScale = originalScale;
            pulseCoroutine = null;
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