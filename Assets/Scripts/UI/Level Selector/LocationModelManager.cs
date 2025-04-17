using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace LevelSelection
{
    /// <summary>
    /// Manages the instantiation, animation, and rotation of 3D location models in the level selector.
    /// </summary>
    public class LocationModelManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Animation Settings")]
        [Tooltip("How far back along the Z-axis the model sits when inactive.")]
        [SerializeField] private float _depthOffset = 2f;
        
        [Tooltip("Duration of the forward/back animation.")]
        [SerializeField] private float _animationDuration = 0.5f; 
        
        [Tooltip("Rotation speed in degrees per second for the active model.")]
        [SerializeField] private float _rotationSpeed = 45f;
        #endregion

        #region Private Fields
        // Stores instantiated models keyed by their LocationData
        private readonly Dictionary<LocationData, GameObject> _models = new Dictionary<LocationData, GameObject>();
        
        // Tracks the currently active/raised location
        private LocationData _currentActive;
        
        // Reference to the currently running rotation coroutine
        private Coroutine _currentRotationCoroutine;
        #endregion

        #region Public Methods
        /// <summary>
        /// Public setup method, typically called by the carousel controller.
        /// Initializes models and sets the first one as active.
        /// </summary>
        public void SetupModels(LocationData[] locations)
        {
            if (locations == null || locations.Length == 0)
            {
                Debug.LogWarning("No locations provided for model setup", this);
                return;
            }

            InitializeModels(locations);
            
            // Set the initial active model state and start its rotation
            if (locations[0] != null && _models.TryGetValue(locations[0], out GameObject initialModel))
            {
                _currentActive = locations[0];
                
                if (initialModel != null)
                {
                    // Start the first item in the 'forward' position
                    initialModel.transform.position = _currentActive.worldPosition;
                    // Start rotating the initial model
                    StartRotation(initialModel);
                }
            }
        }

        /// <summary>
        /// Updates the active model, animating the old one back and the new one forward.
        /// </summary>
        public void UpdateActiveModel(LocationData newActiveLocation)
        {
            // No change if the location is the same
            if (_currentActive == newActiveLocation) return;

            // Stop rotation on the previously active model
            StopCurrentRotation();

            // Animate the previously active model back
            AnimateModelBack(_currentActive);

            // Animate the new active model forward
            AnimateModelForward(newActiveLocation);

            // Update the tracked active location
            _currentActive = newActiveLocation;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes the models based on the provided location data array.
        /// </summary>
        private void InitializeModels(LocationData[] locations)
        {
            ClearModels();

            if (locations == null) return;

            foreach (var location in locations)
            {
                if (location != null)
                {
                    CreateModelInstance(location);
                }
            }
        }

        /// <summary>
        /// Creates a single model instance for a given location.
        /// </summary>
        private void CreateModelInstance(LocationData location)
        {
            // Prevent duplicates
            if (_models.ContainsKey(location)) return;
            
            // Ensure prefab exists
            if (location.modelPrefab == null)
            {
                Debug.LogWarning($"Model prefab is null for location: {location.name}", this);
                return;
            }

            // Calculate the initial "back" position using Z-axis offset
            Vector3 initialPosition = location.worldPosition + Vector3.back * _depthOffset;

            // Instantiate the model prefab using its own rotation
            GameObject modelInstance = Instantiate(
                location.modelPrefab, 
                initialPosition, 
                location.modelPrefab.transform.rotation, 
                transform
            );
            
            if (modelInstance != null)
            {
                modelInstance.name = $"Model_{location.name}";
                _models.Add(location, modelInstance);
            }
        }

        /// <summary>
        /// Animates a model back to its inactive position.
        /// </summary>
        private void AnimateModelBack(LocationData location)
        {
            if (location == null || !_models.TryGetValue(location, out GameObject model) || model == null)
            {
                return;
            }

            // Calculate the "back" position using Z-axis offset
            Vector3 backPosition = location.worldPosition + Vector3.back * _depthOffset;
            
            // Animate the model back
            model.transform.DOMove(backPosition, _animationDuration)
                .SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// Animates a model forward to its active position and starts rotation.
        /// </summary>
        private void AnimateModelForward(LocationData location)
        {
            if (location == null)
            {
                return;
            }

            if (!_models.TryGetValue(location, out GameObject model) || model == null)
            {
                string locationName = location.name ?? location.sprite?.name ?? "Unknown";
                Debug.LogWarning($"Model instance not found for LocationData: {locationName}", this);
                return;
            }

            // Animate the model forward and start rotation when done
            model.transform.DOMove(location.worldPosition, _animationDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => StartRotation(model));
        }

        /// <summary>
        /// Starts the rotation animation on the given model.
        /// </summary>
        private void StartRotation(GameObject modelToRotate)
        {
            if (modelToRotate == null) return;
            
            // Ensure we don't start multiple rotations
            StopCurrentRotation();
            
            _currentRotationCoroutine = StartCoroutine(RotateModelCoroutine(modelToRotate.transform));
        }

        /// <summary>
        /// Stops any active rotation coroutine.
        /// </summary>
        private void StopCurrentRotation()
        {
            if (_currentRotationCoroutine != null)
            {
                StopCoroutine(_currentRotationCoroutine);
                _currentRotationCoroutine = null;
            }
        }

        /// <summary>
        /// Coroutine that continuously rotates a model transform.
        /// </summary>
        private IEnumerator RotateModelCoroutine(Transform modelTransform)
        {
            // Check if transform is still valid in case object gets destroyed
            while (modelTransform != null)
            { 
                modelTransform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }
            
            // Ensure coroutine reference is cleared if the loop exits
            _currentRotationCoroutine = null;
        }
        
        /// <summary>
        /// Cleans up all instantiated model GameObjects and stops rotation.
        /// </summary>
        private void ClearModels()
        {
            StopCurrentRotation();

            // Process each model in the dictionary
            foreach (var model in _models.Values.Where(model => model != null))
            {
                // Kill any active DoTween animations on the object before destroying
                DOTween.Kill(model.transform);
                Destroy(model);
            }

            _models.Clear();
            _currentActive = null;
        }
        #endregion

        #region Unity Lifecycle Methods
        private void OnDisable()
        {
            StopCurrentRotation();
        }

        private void OnDestroy()
        {
            StopCurrentRotation();
            // ClearModels() is implicitly called by Unity, but stopping the coroutine explicitly is good practice
        }
        #endregion
    }
} 