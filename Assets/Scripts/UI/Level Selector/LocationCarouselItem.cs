using DG.Tweening;
using Carousel.UI;
using UnityEngine;
using UnityEngine.UI;
namespace LevelSelection{
public class LocationCarouselItem : CarouselItem<LocationData>
{
    [SerializeField] Image _image;
    
    protected override void OnDataUpdated(LocationData data)
    {
        base.OnDataUpdated(data);
        _image.sprite = data.sprite;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        Sequence sequence = this.CreateSequence();
        sequence.Join(_image.DOFade(1, .25f));
        sequence.Join(RectTransform.DOScale(1, .25f));
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        Sequence sequence = this.CreateSequence();
        sequence.Join(_image.DOFade(.25f, .25f));
        sequence.Join(RectTransform.DOScale(.75f, .25f));
    }
}
}