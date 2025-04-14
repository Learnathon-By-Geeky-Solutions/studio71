using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// Renamed from LocationMapDots
public class LocationModelManager : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("How far back along the Z-axis the model sits when inactive.")]
    [SerializeField] private float _depthOffset = 2f;
    [Tooltip("Duration of the forward/back animation.")]
    [SerializeField] private float _animationDuration = 0.5f; 
    [Tooltip("Rotation speed in degrees per second for the active model.")]
    [SerializeField] private float _rotationSpeed = 45f; 

    // Stores instantiated models keyed by their LocationData
    private Dictionary<LocationData, GameObject> _models = new Dictionary<LocationData, GameObject>();
    // Tracks the currently active/raised location
    private LocationData _currentActive;
    // Reference to the currently running rotation coroutine
    private Coroutine _currentRotationCoroutine;
    // Reference to the model being rotated
    private GameObject _currentlyRotatingModel;

    /// <summary>
    /// Initializes the models based on the provided location data array.
    /// Destroys existing models before creating new ones.
    /// </summary>
    public void InitializeModels(LocationData[] locations)
    {
        ClearModels();

        foreach (var location in locations)
        {
            CreateModelInstance(location);
        }
    }

    /// <summary>
    /// Creates a single model instance for a given location.
    /// Instantiates the prefab defined in LocationData at the specified world position, offset along the Z-axis.
    /// </summary>
    private void CreateModelInstance(LocationData location)
    {
        // Prevent duplicates and ensure prefab exists
        if (_models.ContainsKey(location) || location.modelPrefab == null)
            return;

        // Calculate the initial "back" position using Z-axis offset
        Vector3 initialPosition = location.worldPosition + Vector3.back * _depthOffset;

        // Instantiate the model prefab using its own rotation
        GameObject modelInstance = Instantiate(location.modelPrefab, initialPosition, location.modelPrefab.transform.rotation, transform); 

        // Store the instance
        _models.Add(location, modelInstance);
    }

    /// <summary>
    /// Updates the active model, animating the old one back and the new one forward, and managing rotation.
    /// </summary>
    public void UpdateActiveModel(LocationData newActiveLocation)
    {
        // No change if the location is the same
        if (_currentActive == newActiveLocation) return;

        // Stop rotation on the previously active model (if any)
        StopCurrentRotation();

        // Animate the previously active model (if any) back
        if (_currentActive != null && _models.ContainsKey(_currentActive))
        {
            GameObject previousModel = _models[_currentActive];
            if (previousModel != null) // Check if not destroyed
            {
                // Calculate the "back" position using Z-axis offset
                Vector3 backPosition = _currentActive.worldPosition + Vector3.back * _depthOffset; 
                previousModel.transform.DOMove(backPosition, _animationDuration).SetEase(Ease.OutCubic);
            }
        }

        // Animate the new active model forward (to its defined worldPosition)
        if (newActiveLocation != null && _models.ContainsKey(newActiveLocation))
        {
            GameObject newModel = _models[newActiveLocation];
             if (newModel != null) // Check if not destroyed
            {
                Vector3 forwardPosition = newActiveLocation.worldPosition; // The target position is the original worldPosition
                 // Start rotation *after* the move animation completes
                 newModel.transform.DOMove(forwardPosition, _animationDuration)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() => StartRotation(newModel)); 
            }
        }
        else
        {
             Debug.LogWarning($"Model instance not found for LocationData: {newActiveLocation?.sprite?.name ?? "NULL"}");
        }


        // Update the tracked active location
        _currentActive = newActiveLocation;
    }

    private void StartRotation(GameObject modelToRotate)
    {
        // Ensure we don't start multiple rotations on the same object
        StopCurrentRotation(); 

        if (modelToRotate != null)
        {
             _currentlyRotatingModel = modelToRotate;
            _currentRotationCoroutine = StartCoroutine(RotateModelCoroutine(_currentlyRotatingModel.transform));
        }
    }

    private void StopCurrentRotation()
    {
        if (_currentRotationCoroutine != null)
        {
            StopCoroutine(_currentRotationCoroutine);
            _currentRotationCoroutine = null;
            _currentlyRotatingModel = null; // Clear the reference
        }
    }

    private IEnumerator RotateModelCoroutine(Transform modelTransform)
    {
        // Check if transform is still valid in case object gets destroyed
        while (modelTransform != null) 
        { 
            modelTransform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }
        // Ensure coroutine reference is cleared if the loop exits (e.g., object destroyed)
        _currentRotationCoroutine = null; 
        _currentlyRotatingModel = null;
    }
    
    /// <summary>
    /// Cleans up all instantiated model GameObjects and stops rotation.
    /// </summary>
    private void ClearModels()
    {
        StopCurrentRotation(); // Stop rotation before destroying models

        foreach (var pair in _models)
        {
            if (pair.Value != null)
            {
                // Kill any active DoTween animations on the object before destroying
                DOTween.Kill(pair.Value.transform); 
                Destroy(pair.Value);
            }
        }

        _models.Clear();
        _currentActive = null;
    }

    /// <summary>
    /// Public setup method, typically called by the carousel controller.
    /// </summary>
    public void SetupModels(LocationData[] locations)
    {
        InitializeModels(locations);
         // Set the initial active model state and start its rotation
        if (locations.Length > 0 && locations[0] != null && _models.ContainsKey(locations[0]))
        {
            _currentActive = locations[0];
            GameObject initialModel = _models[_currentActive];
            if(initialModel != null)
            {
                 initialModel.transform.position = _currentActive.worldPosition; // Start the first item in the 'forward' position
                 StartRotation(initialModel); // Start rotating the initial model
            }
        }
    }

    // Stop rotation when the component is disabled
    private void OnDisable() {
        StopCurrentRotation(); 
    }

     // Ensure cleanup happens if the GameObject is destroyed
     private void OnDestroy() {
         StopCurrentRotation(); 
         // ClearModels() is implicitly called by Unity lifecycle if needed, but stopping coroutine explicitly is good practice.
     }
} 