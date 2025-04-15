using DG.Tweening;
using UnityEngine;

namespace Carousel.UI
{
    public static class UIUtils 
    {
        public static Sequence CreateSequence(this MonoBehaviour mono, object target = null)
        {
            object t = target == null ? mono.gameObject : target; 

            DOTween.Kill(t, true);
            Sequence s = DOTween.Sequence();
            s.SetTarget(t);
            return s;
        }
    }
}