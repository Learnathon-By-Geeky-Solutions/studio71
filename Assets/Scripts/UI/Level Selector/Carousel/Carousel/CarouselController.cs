using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Carousel.UI
{
    public enum Origin {
        TopLeft,
        CenterLeft,
        BottomLeft,
        TopRight,
        CenterRight,
        BottomRight,
        Top,
        Center,
        Bottom
    }

    [RequireComponent(typeof(Image), typeof(Mask))]
    public class CarouselController<T> : MonoBehaviour
    {
        [Header("Carousel Data")]
        [SerializeField] protected T[] _data;

        [Header("Carousel Item")]
        [SerializeField] protected CarouselItem<T> _carouselItemPrefab;

        [Header("Carousel Settings")]
        [SerializeField] protected Origin _childOrigin;
        [SerializeField] protected bool _isInfinity = false;
        [SerializeField] protected int _repeat = 2;
        [SerializeField] protected int _indexRepeatOffset = 1;

        [SerializeField] protected UnityEvent<T> _onItemSelected;
        [SerializeField] protected UnityEvent<T> _onCurrentItemUpdated;
        [SerializeField] protected UnityEvent _onNext;
        [SerializeField] protected UnityEvent _onPrev;

        protected int _currentIndex;
        protected ICarouselItemPositioner _positioner;
        protected List<CarouselItem<T>> _carouselItems = new List<CarouselItem<T>>();

        public UnityEvent<T> OnItemSelected { get => _onItemSelected; }
        public UnityEvent<T> OnCurrentItemUpdated { get => _onCurrentItemUpdated; }
        public UnityEvent OnPrev { get => _onPrev; }
        public UnityEvent OnNext { get => _onNext; }

        protected virtual void Awake()
        {
            _positioner = GetComponent<ICarouselItemPositioner>() ?? gameObject.AddComponent<HorizontalCarouselItemPositioner>();
        }

        protected virtual void OnValidate()
        {
            if (_repeat < 2) _repeat = 2;
            if (Application.isPlaying) UpdateData();
        }

        protected virtual void Start()
        {
            CreateCarouselItems();
            UpdateData();
        }

        protected virtual void CreateCarouselItems()
        {
            int itemsCount = _isInfinity ? _data.Length * _repeat : _data.Length;
            _carouselItems.Capacity = itemsCount;

            for (int i = 0; i < itemsCount; i++)
            {
                var newItem = Instantiate(_carouselItemPrefab, transform);
                newItem.Data = _data[i % _data.Length];

                var rect = newItem.transform as RectTransform;
                SetChildOrigin(rect);
                rect.anchoredPosition = new Vector3(0, 0);

                newItem.OnSelected += AdjustIndexForClickedItem;

                _carouselItems.Add(newItem);
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var item in _carouselItems)
            {
                item.OnSelected += AdjustIndexForClickedItem;
            }
        } 

        protected virtual void SetChildOrigin(RectTransform rect)
        {
            switch(_childOrigin)
            {
                case Origin.TopLeft:
                    rect.anchorMin = new Vector2(0, 1f);
                    rect.anchorMax = new Vector2(0, 1f);
                    rect.pivot = new Vector2(0, 1f);
                    break;
                case Origin.CenterLeft:
                    rect.anchorMin = new Vector2(0, .5f);
                    rect.anchorMax = new Vector2(0, .5f);
                    rect.pivot = new Vector2(0, .5f);
                    break;
                case Origin.BottomLeft:
                    rect.anchorMin = new Vector2(0, 0f);
                    rect.anchorMax = new Vector2(0, 0f);
                    rect.pivot = new Vector2(0, 0f);
                    break;
                    case Origin.TopRight:
                    rect.anchorMin = new Vector2(1, 1f);
                    rect.anchorMax = new Vector2(1, 1f);
                    rect.pivot = new Vector2(1, 1f);
                    break;
                case Origin.CenterRight:
                    rect.anchorMin = new Vector2(1, .5f);
                    rect.anchorMax = new Vector2(1, .5f);
                    rect.pivot = new Vector2(1, .5f);
                    break;
                case Origin.BottomRight:
                    rect.anchorMin = new Vector2(1, 0f);
                    rect.anchorMax = new Vector2(1, 0f);
                    rect.pivot = new Vector2(1, 0f);
                    break;
                case Origin.Top:
                    rect.anchorMin = new Vector2(.5f, 1f);
                    rect.anchorMax = new Vector2(.5f, 1f);
                    rect.pivot = new Vector2(.5f, 1f);
                    break;
                case Origin.Center:
                    rect.anchorMin = new Vector2(.5f, .5f);
                    rect.anchorMax = new Vector2(.5f, .5f);
                    rect.pivot = new Vector2(.5f, .5f);
                    break;
                case Origin.Bottom:
                    rect.anchorMin = new Vector2(.5f, 0);
                    rect.anchorMax = new Vector2(.5f, 0);
                    rect.pivot = new Vector2(.5f, 0);
                    break;
                default:
                    break;
            }
        }

        protected CarouselItem<T> GetCarouselItemAt(int index)
        {
            return _carouselItems[GetCarouselIndex(index)];
        }

        protected int GetCarouselIndex(int index)
        {
            return (index % _carouselItems.Count + _carouselItems.Count) % _carouselItems.Count;
        }

        protected virtual void UpdateData()
        {
            for (int i = 0; i < _carouselItems.Count; i++)
            {
                var item = GetCarouselItemAt(i);
                bool isActive = _isInfinity ? i == GetCarouselIndex(_currentIndex + _data.Length * _indexRepeatOffset) : i == _currentIndex;
                item.SetActive(isActive);

                if (isActive)
                {
                    _onCurrentItemUpdated?.Invoke(item.Data);
                }

                MoveItemToPositionAtIndex(item, _isInfinity ? GetCarouselIndex(i - _currentIndex) - _data.Length * _indexRepeatOffset : i - _currentIndex);
            }
        }

        protected virtual void MoveItemToPositionAtIndex(CarouselItem<T> item, int index)
        {
            _positioner?.SetPosition(item._rectTransform, index);
        }

        public virtual void Next()
        {
            if (!_isInfinity && _currentIndex + 1 >= _data.Length) return;

            _currentIndex++;
          
            UpdateData();
            _onNext?.Invoke();
        }

        public virtual void Previous()
        {
            if (!_isInfinity && _currentIndex - 1 < 0) return;

            _currentIndex--;

            UpdateData();
            _onPrev?.Invoke();
        }

        public virtual void Select()
        {
            _onItemSelected?.Invoke(_carouselItems[GetCarouselIndex(_currentIndex)].Data);
        }

        protected virtual void OnCarouselItemClicked(CarouselItem<T> clickedItem)
        {
            if (_isInfinity)
            {
                AdjustIndexForClickedItem(clickedItem);
            }
            else
            {
                _currentIndex = _carouselItems.IndexOf(clickedItem);
                UpdateData();
            }
        }

        protected virtual void AdjustIndexForClickedItem(CarouselItem<T> clickedItem)
        {
            var targetItem = GetCarouselItemAt(_currentIndex + _data.Length);
            int direction = _positioner.IsItemAfter(targetItem._rectTransform, clickedItem._rectTransform) ? -1 : 1;

            while (GetCarouselItemAt(_currentIndex + _data.Length) != clickedItem)
            {
                _currentIndex += direction;
            }

            UpdateData();
        }
    }
}