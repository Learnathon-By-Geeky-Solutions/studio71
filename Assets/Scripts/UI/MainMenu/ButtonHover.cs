using UnityEngine;
using UnityEngine.EventSystems;
using SingletonManagers;
using DG.Tweening;

namespace UI.MainMenu
{
    /// <summary>
    /// Provides button hover effects including scaling animation and sound feedback.
    /// </summary>
    public class ButtonScaleOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Configuration
        [Tooltip("Scale multiplier when hovering over the button")]
        [SerializeField] private float hoverScale = 1.1f;
        
        [Tooltip("Duration of the scaling animation in seconds")]
        [SerializeField] private float tweenDuration = 0.2f;
        #endregion
        
        #region Private Fields
        private Vector3 originalScale;
        private Tween currentTween;
        #endregion
        
        #region Unity Lifecycle Methods
        private void Awake()
        {
            // Store the original scale of the button
            originalScale = transform.localScale;
        }
        
        private void OnDisable()
        {
            // Ensure the button returns to original scale when disabled
            KillCurrentTween();
            transform.localScale = originalScale;
        }
        
        private void OnDestroy()
        {
            // Clean up any active tweens when destroyed
            KillCurrentTween();
        }
        #endregion
        
        #region Event Handlers
        /// <summary>
        /// Called when the mouse enters the button area
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {   
            // Play hover sound effect
            PlayHoverSound();
            
            // Scale up the button using DOTween
            AnimateScale(originalScale * hoverScale);
        }
        
        /// <summary>
        /// Called when the mouse exits the button area
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            // Scale back to original size
            AnimateScale(originalScale);
        }
        #endregion
        
        #region Helper Methods
        /// <summary>
        /// Animates the button scale to the target value
        /// </summary>
        private void AnimateScale(Vector3 targetScale)
        {
            // Kill any existing animation to avoid conflicts
            KillCurrentTween();
            
            // Create and store the new tween
            currentTween = transform.DOScale(targetScale, tweenDuration)
                .SetEase(Ease.OutQuad);
        }
        
        /// <summary>
        /// Plays the button hover sound effect
        /// </summary>
        private static void PlayHoverSound()
        {
            AudioManager.PlaySound(SoundKeys.ButtonHover);
        }
        
        /// <summary>
        /// Kills the current active tween if it exists
        /// </summary>
        private void KillCurrentTween()
        {
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
                currentTween = null;
            }
        }
        #endregion
    }
}