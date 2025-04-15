using System;
using System.Collections;
using System.Collections.Generic;
using Carousel.UI;
using TMPro;
using TextProcessing;
using UnityEngine;
using LevelSelection;

[System.Serializable]
public class LocationData {
    public Sprite sprite; // Keep sprite for the carousel item UI
    public GameObject modelPrefab; // Prefab of the 3D model for this location
    public string name; // Name to display for this location
    public string description; // Text to display for this location

    [SerializeField]
    public Vector3 worldPosition; // World position to instantiate the model
}

public class LocationCarousel : CarouselController<LocationData>
{
    [Header("References")]
    [SerializeField] private LocationModelManager _modelManager; // Reference to the model manager
    [SerializeField] private TextMeshProUGUI _nameText; // Reference to the Name TextMeshPro object
    [SerializeField] private TextMeshProUGUI _descriptionText; // Reference to the description TextMeshPro object
    
    private void OnEnable()
    {
        // OnItemSelected.AddListener(LogItem); // Optional: Can keep if needed
        OnCurrentItemUpdated.AddListener(LogItem); // Optional: Can keep if needed
        OnCurrentItemUpdated.AddListener(UpdateActiveModel);
        OnCurrentItemUpdated.AddListener(UpdateName); // Add listener for name update
        OnCurrentItemUpdated.AddListener(UpdateDescription); // Add listener for description update
    }

    protected override void  OnDisable()
    {
        // OnItemSelected.RemoveListener(LogItem); // Optional: Can keep if needed
        OnCurrentItemUpdated.RemoveListener(LogItem); // Optional: Can keep if needed
        OnCurrentItemUpdated.RemoveListener(UpdateActiveModel);
        OnCurrentItemUpdated.RemoveListener(UpdateName); // Remove listener
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

        // Update texts for the initial item
        if (_data.Length > 0 && _data[0] != null) 
        {
            UpdateName(_data[0]); // Update initial name
            UpdateDescription(_data[0]);
        }
        else // Clear texts if no data
        { 
            if (_nameText != null) _nameText.text = "";
            if (_descriptionText != null) _descriptionText.text = "";
        }
    }

    private void LogItem(LocationData data)
    {
        Debug.Log("Carousel Item Updated/Selected: " + (data?.name ?? data?.sprite?.name ?? "N/A")); // Log name if available
    }
    
    private void UpdateActiveModel(LocationData data)
    {
        if (_modelManager != null)
        {
            _modelManager.UpdateActiveModel(data);
        }
    }

    // New method to update the name text
    private void UpdateName(LocationData data)
    {
        if (_nameText != null)
        {
            string originalName = data != null ? data.name : "";
            string fixedName = string.IsNullOrEmpty(originalName)
                                ? ""
                                : BanglaTextFixer.Instance.FixBanglaText(originalName);

            _nameText.text = fixedName; // Set the fixed name text
        }
    }

    // Modified method to update the description text using the fixer
    private void UpdateDescription(LocationData data)
    {
        if (_descriptionText != null)
        {
            string originalDescription = data != null ? data.description : "";
            string fixedDescription = string.IsNullOrEmpty(originalDescription) 
                                        ? "" 
                                        : BanglaTextFixer.Instance.FixBanglaText(originalDescription);
            
            _descriptionText.text = fixedDescription; // Set the fixed text
        }
    }
}