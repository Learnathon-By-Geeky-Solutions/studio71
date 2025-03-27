using System;
using System.Collections;
using System.Collections.Generic;
using Carousel.UI;
using UnityEngine;

[System.Serializable]
public class LocationData {
    public Sprite sprite;

    [SerializeField]
    public Vector2 mapPosition; // Position of the dot on the map
}

public class LocationCarousel : CarouselController<LocationData>
{
    [SerializeField] private LocationMapDots _mapDots; // Reference to the map dots manager
    
    private void OnEnable()
    {
        OnItemSelected.AddListener(LogItem);
        OnItemSelected.AddListener(HighlightDot);
        OnCurrentItemUpdated.AddListener(LogItem);
        OnCurrentItemUpdated.AddListener(UpdateActiveDot);
    }

    private void OnDisable()
    {
        OnItemSelected.RemoveListener(LogItem);
        OnItemSelected.RemoveListener(HighlightDot);
        OnCurrentItemUpdated.RemoveListener(LogItem);
        OnCurrentItemUpdated.RemoveListener(UpdateActiveDot);
    }
    
    protected override void Start()
    {
        base.Start();
        
        // Initialize map dots
        if (_mapDots != null)
        {
            _mapDots.SetupDots(_data);
        }
        
        // Update active dot for initial state
        if (_data.Length > 0)
        {
            UpdateActiveDot(_data[0]);
        }
    }

    private void LogItem(LocationData data)
    {
        Debug.Log(data.sprite);
    }
    
    private void HighlightDot(LocationData data)
    {
        if (_mapDots != null)
        {
            _mapDots.HighlightDot(data);
        }
    }
    
    private void UpdateActiveDot(LocationData data)
    {
        if (_mapDots != null)
        {
            _mapDots.UpdateActiveDot(data);
        }
    }
}