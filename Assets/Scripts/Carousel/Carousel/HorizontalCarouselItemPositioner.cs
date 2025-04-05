#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Carousel.UI
{
    public class HorizontalCarouselItemPositioner : MonoBehaviour, ICarouselItemPositioner
    {        
        [SerializeField] bool _isStatic = false;
        [SerializeField] float _duration = .25f;
        [SerializeField] float _offsetX;
        [SerializeField] float _gap = 100;
        [SerializeField] int _visibleItem = 3;
        [SerializeField] Ease _ease;


        [Header("For Debugging")]
        [SerializeField] bool _debugCarouselArea;
        
        Image _image;

        bool _realIsStatic = false;

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            _realIsStatic = _isStatic;
        }


        private void OnValidate()
        {
            if (_image == null) _image = GetComponent<Image>();
            
            _image.color = _debugCarouselArea ? new Color(1, 1, 1, 50 / 255f) : new Color(1, 1, 1, 1 / 255f);
            
            if(_isStatic)
            {
                return;
            }

            EditorApplication.delayCall += UpdateSizeDelta;
        }

        public void SetPosition(RectTransform rectTransform, int index)
        {
             if(_realIsStatic) return;

            float endValue = index * _gap + _offsetX;

            float duration = Mathf.Abs(endValue - rectTransform.anchoredPosition.x) > _gap * _visibleItem ? 0 : _duration;

            this.CreateSequence(rectTransform)
            .Join(rectTransform.DOAnchorPosX(endValue, duration).SetEase(_ease));
        }

        public bool IsItemAfter(RectTransform a, RectTransform b)
        {
            return a.anchoredPosition.x > b.anchoredPosition.x;
        }

        private void UpdateSizeDelta()
        {
            if (_image != null && _image.rectTransform != null)
            {
                Vector2 newSize = new Vector2(_visibleItem * _gap, _image.rectTransform.sizeDelta.y);

                if (_image.rectTransform.sizeDelta != newSize)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(_image.rectTransform, "Update RectTransform SizeDelta");
#endif
                     _image.rectTransform.sizeDelta = newSize;

#if UNITY_EDITOR
                    EditorUtility.SetDirty(_image.rectTransform);
#endif
                }
            }

            EditorApplication.delayCall -= UpdateSizeDelta;
        }
    }
}