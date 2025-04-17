using DG.Tweening;
using Carousel.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LevelSelection
{
    /// <summary>
    /// Represents a visual item in the location carousel with animation capabilities.
    /// </summary>
    public class LocationCarouselItem : CarouselItem<LocationData>
    {
        [SerializeField] private Image _image;
        
        private void Awake()
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
                if (_image == null)
                {
                    Debug.LogError("Image component not found on LocationCarouselItem", this);
                }
            }
        }
        
        protected override void OnDataUpdated(LocationData data)
        {
            base.OnDataUpdated(data);
            
            if (_image == null) return;
            
            if (data?.sprite != null)
            {
                _image.sprite = data.sprite;
            }
            else
            {
                Debug.LogWarning("LocationData has no sprite assigned", this);
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            
            if (_image == null) return;
            
            // Create and play activation animation sequence
            Sequence sequence = DOTween.Sequence();
            sequence.Join(_image.DOFade(1, 0.25f));
            sequence.Join(RectTransform.DOScale(1, 0.25f));
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            
            if (_image == null) return;
            
            // Create and play deactivation animation sequence
            Sequence sequence = DOTween.Sequence();
            sequence.Join(_image.DOFade(0.25f, 0.25f));
            sequence.Join(RectTransform.DOScale(0.75f, 0.25f));
        }
        
        protected void OnDestroy()
        {
            // Kill any DOTween animations when destroyed
            if (_image != null)
            {
                DOTween.Kill(_image);
            }
            
            if (RectTransform != null)
            {
                DOTween.Kill(RectTransform);
            }
        }
    }
}