using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening; // Make sure to import DOTween

public class ButtonScaleOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float tweenDuration = 0.2f;
    
    private Vector3 originalScale;
    
    private void Awake()
    {
        // Store the original scale of the button
        originalScale = transform.localScale;
    }
    
    // Called when the mouse enters the button area
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Scale up the button using DOTween
        transform.DOScale(originalScale * hoverScale, tweenDuration);
    }
    
    // Called when the mouse exits the button area
    public void OnPointerExit(PointerEventData eventData)
    {
        // Scale back to original size
        transform.DOScale(originalScale, tweenDuration);
    }
}