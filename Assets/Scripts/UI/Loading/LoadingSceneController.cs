using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using SingletonManagers;
using UnityEngine.UI;

namespace UI.Loading
{
    /// <summary>
    /// Controls the loading scene with video playback and scene transition.
    /// </summary>
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("Video Player")]
        [SerializeField] private VideoPlayer videoPlayer;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject skipButton; // Reference to the Skip button
        [SerializeField] private RawImage videoDisplay; // Reference to the RawImage that displays the video
        [SerializeField] private CanvasGroup fadeCanvasGroup; // Canvas group used for fading
        
        [Header("Videos")]
        [Tooltip("Video clips to play during loading. Index 0 = Level1, Index 1 = Level2")]
        [SerializeField] private VideoClip[] loadingVideos;
        
        [Header("Loading Settings")]
        [Tooltip("Whether to wait for the video to complete or load the scene immediately")]
        [SerializeField] private bool waitForVideoToFinish = true;
        
        [Tooltip("Whether to start loading the next scene in the background while the video plays")]
        [SerializeField] private bool loadInBackground = true;
        
        [Header("Transition Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private Color initialRenderTextureColor = Color.black;
        
        [Header("Debug Settings")]
        [Tooltip("Enable extra debug logging for state transitions")]
        [SerializeField] private bool verboseLogging = false;
        
        // Loading state variables
        private enum LoadingState { NotStarted, Loading, ReadyToActivate, Activating, Completed }
        private LoadingState _sceneLoadingState = LoadingState.NotStarted;
        private LoadingState _videoState = LoadingState.NotStarted;
        
        private AsyncOperation _loadOperation;
        private int _targetSceneIndex;
        private int _videoIndex;
        private Coroutine _loadingCoroutine;
        private RenderTexture _videoRenderTexture;
        
        private void Awake()
        {
            // Create fade canvas group if it doesn't exist
            if (fadeCanvasGroup == null)
            {
                // Try to find it
                fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
                
                // If still null, create one on the canvas
                if (fadeCanvasGroup == null && videoDisplay != null)
                {
                    Canvas canvas = videoDisplay.GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        GameObject fadeObj = new GameObject("FadeGroup");
                        fadeObj.transform.SetParent(canvas.transform, false);
                        RectTransform rect = fadeObj.AddComponent<RectTransform>();
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.sizeDelta = Vector2.zero;
                        
                        // Add an image component that covers the entire screen
                        Image fadeImage = fadeObj.AddComponent<Image>();
                        fadeImage.color = Color.black;
                        
                        fadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();
                    }
                }
            }
            
            // Start with a fade-in effect
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 1f;
            }
        }
        
        private void Start()
        {
            LogMessage("LoadingSceneController starting");
            
            // Clear the RenderTexture
            InitializeRenderTexture();
            
            // Ensure the skip button is disabled initially
            if (skipButton != null)
            {
                skipButton.SetActive(false);
            }
            
            // Get the scene and video info from SceneIndexes
            _targetSceneIndex = SceneIndexes.GetTargetScene();
            _videoIndex = SceneIndexes.GetVideoToPlay();
            
            // Validate the target scene
            if (_targetSceneIndex < 0)
            {
                Debug.LogError("No target scene specified for loading scene.");
                // Fallback to main menu
                _targetSceneIndex = SceneIndexes.MaineMenuScene;
            }
            
            LogMessage($"Target scene: {_targetSceneIndex}, Video index: {_videoIndex}");
            
            // Start the fade-in sequence
            StartCoroutine(FadeIn());
            
            // Start playing the appropriate video
            PlayLoadingVideo();
            
            // Start loading the target scene in the background
            if (loadInBackground)
            {
                _loadingCoroutine = StartCoroutine(LoadTargetSceneAsync());
            }
        }
        
        private void InitializeRenderTexture()
        {
            if (videoPlayer == null || videoPlayer.targetTexture == null)
            {
                LogMessage("VideoPlayer or targetTexture is null, can't initialize");
                return;
            }
            
            _videoRenderTexture = videoPlayer.targetTexture;
            
            // Make sure the texture is created
            _videoRenderTexture.Create();
            
            // Set the initial color (usually black)
            RenderTexture.active = _videoRenderTexture;
            GL.Clear(true, true, initialRenderTextureColor);
            RenderTexture.active = null;
            
            // Set it to the video display if available
            if (videoDisplay != null)
            {
                videoDisplay.texture = _videoRenderTexture;
                
                // Start with zero alpha to avoid flash 
                if (videoDisplay.color.a > 0)
                {
                    Color tempColor = videoDisplay.color;
                    tempColor.a = 0;
                    videoDisplay.color = tempColor;
                }
            }
        }
        
        private IEnumerator FadeIn()
        {
            // Fade in the video display
            if (videoDisplay != null)
            {
                // IMPORTANT: Disable raycast blocking on the video display
                videoDisplay.raycastTarget = false;
                
                float startTime = Time.time;
                Color originalColor = videoDisplay.color;
                Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                
                while (Time.time < startTime + fadeInDuration)
                {
                    float progress = (Time.time - startTime) / fadeInDuration;
                    videoDisplay.color = Color.Lerp(originalColor, targetColor, progress);
                    yield return null;
                }
                
                videoDisplay.color = targetColor;
            }
            
            // Fade out the black overlay
            if (fadeCanvasGroup != null)
            {
                float startTime = Time.time;
                float startAlpha = fadeCanvasGroup.alpha;
                
                while (Time.time < startTime + fadeInDuration)
                {
                    float progress = (Time.time - startTime) / fadeInDuration;
                    fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
                    yield return null;
                }
                
                fadeCanvasGroup.alpha = 0f;
                
                // IMPORTANT: Make sure the fadeCanvasGroup doesn't block raycasts when faded
                fadeCanvasGroup.blocksRaycasts = false;
                fadeCanvasGroup.interactable = false;
            }
            
            // Check for any other elements that might block raycasts
            CheckForRaycastBlockers();
        }
        
        private void CheckForRaycastBlockers()
        {
            // Log canvas information
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                Debug.Log($"Canvas '{canvas.name}' - sortingOrder: {canvas.sortingOrder}, " +
                          $"renderMode: {canvas.renderMode}, " +
                          $"hasGraphicRaycaster: {canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() != null}");
                
                // Check if this canvas contains our skip button
                if (skipButton != null && skipButton.transform.IsChildOf(canvas.transform))
                {
                    Debug.Log($"Skip button is on canvas '{canvas.name}'");
                    
                    // Ensure the canvas has a GraphicRaycaster
                    if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                    {
                        Debug.LogError($"Canvas '{canvas.name}' containing skip button has no GraphicRaycaster! Adding one...");
                        canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    }
                }
            }
            
            // Check for any elements overlapping the button
            if (skipButton != null)
            {
                CheckForOverlappingElements(skipButton);
            }
        }
        
        private void CheckForOverlappingElements(GameObject buttonObj)
        {
            if (buttonObj == null) return;
            
            // Get the button's RectTransform
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            if (buttonRect == null) return;
            
            // Get all RaycastTarget graphics in the scene
            UnityEngine.UI.Graphic[] graphics = FindObjectsOfType<UnityEngine.UI.Graphic>();
            
            Debug.Log($"Checking for elements overlapping skip button. Found {graphics.Length} UI graphics.");
            
            foreach (UnityEngine.UI.Graphic graphic in graphics)
            {
                // Skip the button itself
                if (graphic.gameObject == buttonObj) continue;
                
                // Check if this element is blocking raycasts
                if (graphic.raycastTarget)
                {
                    RectTransform graphicRect = graphic.GetComponent<RectTransform>();
                    Canvas graphicCanvas = graphic.canvas;
                    Canvas buttonCanvas = buttonObj.GetComponentInParent<Canvas>();
                    
                    // If the graphic is on a higher sorting layer than the button
                    if (graphicCanvas != null && buttonCanvas != null && 
                        graphicCanvas.sortingOrder > buttonCanvas.sortingOrder)
                    {
                        Debug.LogWarning($"UI element '{graphic.name}' on canvas with higher sortingOrder ({graphicCanvas.sortingOrder} > {buttonCanvas.sortingOrder}) " +
                                        $"might be blocking the skip button. Disabling its raycastTarget.");
                        
                        // Disable raycast on elements that might block the button
                        graphic.raycastTarget = false;
                    }
                    
                    // Check for objects covering the button at the same sorting level
                    else if (graphicCanvas == buttonCanvas)
                    {
                        // Check if this is a full-screen element
                        Vector2 sizeDelta = graphicRect.sizeDelta;
                        if (sizeDelta.x >= Screen.width * 0.9f && sizeDelta.y >= Screen.height * 0.9f)
                        {
                            Debug.LogWarning($"Large UI element '{graphic.name}' might be covering the skip button. " +
                                           $"Disabling its raycastTarget.");
                            graphic.raycastTarget = false;
                        }
                    }
                }
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event handler when destroyed
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoFinished;
            }
            
            // Stop the coroutine if it's still running
            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
                _loadingCoroutine = null;
            }
        }
        
        private void PlayLoadingVideo()
        {
            LogMessage("Starting video playback");
            
            // Validate the video index
            if (_videoIndex < 0 || _videoIndex >= loadingVideos.Length || loadingVideos[_videoIndex] == null)
            {
                Debug.LogWarning($"No valid video found at index {_videoIndex}. Using default video if available.");
                
                // Try to use the first available video as a fallback
                for (int i = 0; i < loadingVideos.Length; i++)
                {
                    if (loadingVideos[i] != null)
                    {
                        _videoIndex = i;
                        break;
                    }
                }
            }
            
            // Make sure we still have a valid video
            if (_videoIndex >= 0 && _videoIndex < loadingVideos.Length && loadingVideos[_videoIndex] != null)
            {
                // Make sure the video player is prepared first
                videoPlayer.prepareCompleted += OnVideoPrepared;
                
                // Assign the video clip
                videoPlayer.clip = loadingVideos[_videoIndex];
                videoPlayer.Prepare();
                
                _videoState = LoadingState.Loading;
                LogMessage($"Preparing loading video {_videoIndex} for scene {_targetSceneIndex}");
            }
            else
            {
                Debug.LogError("No valid video clips found in LoadingSceneController.");
                _videoState = LoadingState.ReadyToActivate; // Skip video since there isn't one
                
                // If not loading in background, start loading now
                if (!loadInBackground)
                {
                    _loadingCoroutine = StartCoroutine(LoadTargetSceneAsync());
                }
                else
                {
                    CheckAndActivateScene();
                }
            }
        }
        
        private void OnVideoPrepared(VideoPlayer vp)
        {
            // Register for video completion event
            videoPlayer.loopPointReached += OnVideoFinished;
            
            // Start playback
            videoPlayer.Play();
            LogMessage($"Video prepared, starting playback of video {_videoIndex}");
        }
        
        private void OnVideoFinished(VideoPlayer vp)
        {
            if (_videoState == LoadingState.ReadyToActivate)
            {
                LogMessage("Video finished event triggered, but video state already marked as ready");
                return;
            }
            
            LogMessage("Video playback finished");
            _videoState = LoadingState.ReadyToActivate;
            
            // If we're not loading in background, start loading now
            if (!loadInBackground && _sceneLoadingState == LoadingState.NotStarted)
            {
                _loadingCoroutine = StartCoroutine(LoadTargetSceneAsync());
            }
            // Otherwise check if we can activate the scene
            else
            {
                CheckAndActivateScene();
            }
        }
        
        private IEnumerator LoadTargetSceneAsync()
        {
            // Update state
            _sceneLoadingState = LoadingState.Loading;
            
            LogMessage($"Starting to load scene {_targetSceneIndex} asynchronously");
            
            // Start loading the target scene asynchronously
            _loadOperation = SceneManager.LoadSceneAsync(_targetSceneIndex);
            
            // Don't let the scene activate yet
            _loadOperation.allowSceneActivation = false;
            
            // Log loading progress periodically
            float lastProgress = 0f;
            
            // Wait until the load is nearly complete
            while (_loadOperation != null && _loadOperation.progress < 0.9f)
            {
                // Log progress updates but only when it changes significantly
                if (_loadOperation.progress >= lastProgress + 0.1f)
                {
                    LogMessage($"Scene loading progress: {_loadOperation.progress:P0}");
                    lastProgress = _loadOperation.progress;
                }
                yield return null;
            }

            LogMessage("Scene loading reached 90% complete - ready for activation");
            
            // Scene is ready to load, activate the skip button
            ActivateSkipButton();
            
            // Update loading state
            _sceneLoadingState = LoadingState.ReadyToActivate;
            
            // Check if we can activate the scene
            CheckAndActivateScene();
        }
        
        private void Update()
        {
            // This is a safety check to ensure we don't miss state changes
            if (_sceneLoadingState == LoadingState.ReadyToActivate && 
                (!waitForVideoToFinish || _videoState == LoadingState.ReadyToActivate) &&
                _loadOperation != null && !_loadOperation.allowSceneActivation)
            {
                LogMessage("Update safety check detected both video complete and scene ready - activating scene");
                ActivateLoadedScene();
            }
        }
        
        private void CheckAndActivateScene()
        {
            // Do we have both video ready (or skipped) and scene loaded?
            bool videoReady = !waitForVideoToFinish || _videoState == LoadingState.ReadyToActivate;
            bool sceneReady = _sceneLoadingState == LoadingState.ReadyToActivate;
            
            LogMessage($"Checking activation conditions - Video ready: {videoReady}, Scene ready: {sceneReady}");
            
            if (videoReady && sceneReady)
            {
                LogMessage("Both video and scene are ready for activation");
                ActivateLoadedScene();
            }
        }
        
        private void ActivateLoadedScene()
        {
            // Double-check to prevent multiple activations
            if (_sceneLoadingState == LoadingState.Activating || _sceneLoadingState == LoadingState.Completed)
            {
                LogMessage("Scene activation already in progress or completed");
                return;
            }
            
            _sceneLoadingState = LoadingState.Activating;
            
            if (_loadOperation != null)
            {
                LogMessage($"Loading complete. Activating scene {_targetSceneIndex}");
                _loadOperation.allowSceneActivation = true;
                _sceneLoadingState = LoadingState.Completed;
            }
            else
            {
                // Direct load as fallback
                Debug.LogWarning("Using synchronous loading as fallback. This may cause frame drops.");
                _sceneLoadingState = LoadingState.Completed;
                SceneManager.LoadScene(_targetSceneIndex);
            }
        }
        
        // Optional: Add a skip button functionality
        public void SkipVideo()
        {
            LogMessage("Video skip requested");
            
            // Stop video if playing
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            
            // Clean up video player event handlers
            if (videoPlayer != null)
            {
                videoPlayer.prepareCompleted -= OnVideoPrepared;
                videoPlayer.loopPointReached -= OnVideoFinished;
            }
            
            // Force video state to ready
            _videoState = LoadingState.ReadyToActivate;

            // If the scene is already loaded and waiting for activation, activate it immediately
            if (_sceneLoadingState == LoadingState.ReadyToActivate)
            {
                LogMessage("Skip requested and scene is ready - Activating immediately");
                ActivateLoadedScene();
            }
            // Otherwise, rely on the standard check (handles cases where skip is hit before scene load finishes)
            else
            {
                // Handle starting load if necessary (though unlikely if skip button is visible)
                if (!loadInBackground && _sceneLoadingState == LoadingState.NotStarted)
                {
                     LogMessage("Skip requested before background load started - initiating load");
                    _loadingCoroutine = StartCoroutine(LoadTargetSceneAsync());
                }
                else
                {
                    LogMessage("Skip requested - Checking activation status");
                    CheckAndActivateScene();
                }
            }
        }
        
        private void ActivateSkipButton()
        {
            // Activate the skip button when the scene is ready to load
            if (skipButton != null && !skipButton.activeSelf)
            {
                skipButton.SetActive(true);
                
                // Add diagnostic checks for the button's interactivity
                Button button = skipButton.GetComponent<Button>();
                if (button != null)
                {
                    // Ensure the button is interactable
                    button.interactable = true;
                    
                    // Make the button larger and more noticeable
                    RectTransform buttonRect = skipButton.GetComponent<RectTransform>();
                    if (buttonRect != null)
                    {
                        // Increase size slightly for easier clicking
                        Vector2 originalSize = buttonRect.sizeDelta;
                        buttonRect.sizeDelta = originalSize * 1.1f;
                        
                        // Ensure the button is in front (local Z position)
                        Vector3 pos = buttonRect.localPosition;
                        buttonRect.localPosition = new Vector3(pos.x, pos.y, -10f); // Negative Z is "forward" in UI space
                    }
                    
                    // Log the button's configuration
                    Debug.Log($"Skip button activated. Interactable: {button.interactable}, " +
                              $"GameObject active: {skipButton.activeInHierarchy}, " +
                              $"Has onClick listeners: {button.onClick.GetPersistentEventCount() > 0}");
                    
                    // Make sure the button has an onClick listener
                    if (button.onClick.GetPersistentEventCount() == 0)
                    {
                        Debug.LogWarning("Skip button has no onClick listeners! Adding one programmatically.");
                        button.onClick.AddListener(OnSkipButtonClicked);
                    }
                    
                    // CRITICAL: Make sure the button's graphic is raycast-enabled
                    UnityEngine.UI.Graphic[] buttonGraphics = skipButton.GetComponentsInChildren<UnityEngine.UI.Graphic>();
                    foreach (UnityEngine.UI.Graphic graphic in buttonGraphics)
                    {
                        graphic.raycastTarget = true;
                    }
                }
                else
                {
                    Debug.LogError("Skip button GameObject does not have a Button component!");
                }
                
                // Force update of raycast blockers
                CheckForRaycastBlockers();
                
                LogMessage("Skip button activated");
            }
        }
        
        /// <summary>
        /// This should be called from the Skip Button's onClick event in the Inspector
        /// </summary>
        public void OnSkipButtonClicked()
        {
            Debug.Log("SKIP BUTTON CLICKED - Calling SkipVideo()");
            // Call the existing skip method
            SkipVideo();
        }
        
        private void LogMessage(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[LoadingController] {message}");
            }
        }
    }
} 