using System;
using System.Collections;
using System.Collections.Generic;
using Carousel.UI;
using TMPro;
using UnityEngine;

[System.Serializable]
public class LocationData {
    public Sprite sprite; // Keep sprite for the carousel item UI
    public GameObject modelPrefab; // Prefab of the 3D model for this location
    public string description; // Text to display for this location

    [SerializeField]
    public Vector3 worldPosition; // World position to instantiate the model
}

public class LocationCarousel : CarouselController<LocationData>
{
    [Header("References")]
    [SerializeField] private LocationModelManager _modelManager; // Reference to the model manager
    [SerializeField] private TextMeshProUGUI _descriptionText; // Reference to the description TextMeshPro object
    
    private void OnEnable()
    {
        // OnItemSelected.AddListener(LogItem); // Optional: Can keep if needed
        OnCurrentItemUpdated.AddListener(LogItem); // Optional: Can keep if needed
        OnCurrentItemUpdated.AddListener(UpdateActiveModel);
        OnCurrentItemUpdated.AddListener(UpdateDescription); // Add listener for description update
    }

    private void OnDisable()
    {
        // OnItemSelected.RemoveListener(LogItem); // Optional: Can keep if needed
        OnCurrentItemUpdated.RemoveListener(LogItem); // Optional: Can keep if needed
        OnCurrentItemUpdated.RemoveListener(UpdateActiveModel);
        OnCurrentItemUpdated.RemoveListener(UpdateDescription); // Remove listener
    }
    
    protected override void Start()
    {
        base.Start();
        
        // Initialize models using the new manager
        if (_modelManager != null)
        {
            _modelManager.SetupModels(_data);
        }

        // Update description for the initial item
        if (_data.Length > 0 && _data[0] != null) 
        {
            UpdateDescription(_data[0]);
        }
        else if (_descriptionText != null) // Clear text if no data
        {
            _descriptionText.text = "";
        }
    }

    private void LogItem(LocationData data)
    {
        Debug.Log("Carousel Item Updated/Selected: " + (data?.sprite?.name ?? "N/A"));
    }
    
    private void UpdateActiveModel(LocationData data)
    {
        if (_modelManager != null)
        {
            _modelManager.UpdateActiveModel(data);
        }
    }

    // New method to update the description text
    private void UpdateDescription(LocationData data)
    {
        if (_descriptionText != null)
        {
            _descriptionText.text = data != null ? data.description : ""; // Set text or clear if data is null
        }
    }
}