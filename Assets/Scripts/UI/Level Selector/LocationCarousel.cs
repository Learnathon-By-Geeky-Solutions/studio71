using Carousel.UI;
using TMPro;
using TextProcessing;
using UnityEngine;
using SingletonManagers;

namespace LevelSelection
{
    /// <summary>
    /// Represents the data for a location in the carousel.
    /// </summary>
    [System.Serializable]
    public class LocationData 
    {
        [Header("Visual Elements")]
        public Sprite sprite;
        public GameObject modelPrefab;
        
        [Header("Information")]
        public string name;
        [TextArea(3, 5)]
        public string description;

        [Header("Positioning")]
        public Vector3 worldPosition;

        [Header("Scene Loading")]
        public int sceneIndexToLoad = -1; // Default to -1 (no scene)
    }

    /// <summary>
    /// Manages a carousel of location data with associated 3D models and text displays.
    /// </summary>
    public class LocationCarousel : CarouselController<LocationData>
    {
        [Header("References")]
        [SerializeField] private LocationModelManager _modelManager;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        
        private void OnEnable()
        {
            RegisterEventListeners();
        }

        protected override void OnDisable()
        {
            UnregisterEventListeners();
        }
        
        protected override void Start()
        {
            base.Start();
            
            InitializeModels();
            UpdateInitialTexts();
        }

        private void RegisterEventListeners()
        {
            OnCurrentItemUpdated.AddListener(LogItem);
            OnCurrentItemUpdated.AddListener(UpdateActiveModel);
            OnCurrentItemUpdated.AddListener(UpdateName);
            OnCurrentItemUpdated.AddListener(UpdateDescription);
            OnItemSelected.AddListener(LoadSelectedLevelScene);
        }

        private void UnregisterEventListeners()
        {
            OnCurrentItemUpdated.RemoveListener(LogItem);
            OnCurrentItemUpdated.RemoveListener(UpdateActiveModel);
            OnCurrentItemUpdated.RemoveListener(UpdateName);
            OnCurrentItemUpdated.RemoveListener(UpdateDescription);
            OnItemSelected.RemoveListener(LoadSelectedLevelScene);
        }

        private void InitializeModels()
        {
            if (_modelManager != null)
            {
                _modelManager.SetupModels(_data);
            }
            else
            {
                Debug.LogWarning("LocationModelManager is not assigned to the LocationCarousel.");
            }
        }

        private void UpdateInitialTexts()
        {
            if (_data.Length > 0 && _data[0] != null) 
            {
                UpdateName(_data[0]);
                UpdateDescription(_data[0]);
            }
            else
            { 
                ClearTextFields();
            }
        }

        private void ClearTextFields()
        {
            if (_nameText != null) _nameText.text = "";
            if (_descriptionText != null) _descriptionText.text = "";
        }

        private static void LogItem(LocationData data)
        {
            string itemName = data?.name ?? data?.sprite?.name ?? "N/A";
            Debug.Log($"Carousel Item Updated/Selected: {itemName}");
        }
        
        private void UpdateActiveModel(LocationData data)
        {
            if (_modelManager != null)
            {
                _modelManager.UpdateActiveModel(data);
            }
        }

        /// <summary>
        /// Updates the name text display with the current location's name.
        /// </summary>
        private void UpdateName(LocationData data)
        {
            if (_nameText == null) return;

            string originalName = data?.name ?? "";
            _nameText.text = BanglaTextFixer.ApplyTextFix(originalName);
        }

        /// <summary>
        /// Updates the description text display with the current location's description.
        /// </summary>
        private void UpdateDescription(LocationData data)
        {
            if (_descriptionText == null) return;

            string originalDescription = data?.description ?? "";
            _descriptionText.text = BanglaTextFixer.ApplyTextFix(originalDescription);
        }

        /// <summary>
        /// Loads the scene associated with the selected location data.
        /// </summary>
        private static void LoadSelectedLevelScene(LocationData data)
        {
            if (data == null) return;

            // Check if a valid scene index is assigned
            if (data.sceneIndexToLoad >= 0) 
            {
                // Use the new method to load from sliding menu with appropriate video
                SceneIndexes.LoadLevelFromSlidingMenu(data.sceneIndexToLoad);
            }
            else { Debug.LogWarning($"Selected location '{data.name}' has no valid sceneIndexToLoad assigned ({data.sceneIndexToLoad}).");}
        }
    }
}