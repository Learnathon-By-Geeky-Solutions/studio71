using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LocationMapDots : MonoBehaviour
{
    [SerializeField] private RectTransform _mapContainer; // Parent container for the map
    [SerializeField] private Image _dotPrefab; // Red dot prefab
    [SerializeField] private float _normalSize = 1f; // Normal dot size
    [SerializeField] private float _highlightedSize = 1.5f; // Size when highlighted
    [SerializeField] private float _animationDuration = 0.3f; // Animation duration
    [SerializeField] private Color _normalColor = Color.red; // Normal dot color
    [SerializeField] private Color _highlightedColor = new Color(1f, 0.3f, 0.3f, 1f); // Highlighted dot color
    
    private Dictionary<LocationData, Image> _dots = new Dictionary<LocationData, Image>();
    private LocationData _currentActive;
    
    // Call this method to initialize dots based on location data
    public void InitializeDots(LocationData[] locations)
    {
        ClearDots();
        
        foreach (var location in locations)
        {
            CreateDot(location);
        }
    }
    
    private void CreateDot(LocationData location)
    {
        if (_dots.ContainsKey(location))
            return;
            
        Image dot = Instantiate(_dotPrefab, _mapContainer);
        
        // Set position based on the location data
        RectTransform rectTransform = dot.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = location.mapPosition;
        
        // Set initial appearance
        dot.color = _normalColor;
        rectTransform.localScale = Vector3.one * _normalSize;
        
        _dots.Add(location, dot);
    }
    
    public void HighlightDot(LocationData location)
    {
        if (!_dots.ContainsKey(location))
            return;
            
        // Reset previous active dot if any
        if (_currentActive != null && _dots.ContainsKey(_currentActive))
        {
            Image previousDot = _dots[_currentActive];
            previousDot.transform.DOScale(_normalSize, _animationDuration);
            previousDot.DOColor(_normalColor, _animationDuration);
        }
        
        // Highlight new dot
        _currentActive = location;
        Image dot = _dots[location];
        dot.transform.DOScale(_highlightedSize, _animationDuration);
        dot.DOColor(_highlightedColor, _animationDuration);
    }
    
    public void UpdateActiveDot(LocationData location)
    {
        if (_currentActive != location)
        {
            _currentActive = location;
            
            // Update visual state of all dots
            foreach (var pair in _dots)
            {
                Image dot = pair.Value;
                if (pair.Key == location)
                {
                    dot.transform.DOScale(_highlightedSize, _animationDuration);
                    dot.DOColor(_highlightedColor, _animationDuration);
                }
                else
                {
                    dot.transform.DOScale(_normalSize, _animationDuration);
                    dot.DOColor(_normalColor, _animationDuration);
                }
            }
        }
    }
    
    private void ClearDots()
    {
        foreach (var pair in _dots)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value.gameObject);
            }
        }
        
        _dots.Clear();
        _currentActive = null;
    }
    
    // This should be called when the LocationCarousel initializes its data
    public void SetupDots(LocationData[] locations)
    {
        InitializeDots(locations);
    }
} 