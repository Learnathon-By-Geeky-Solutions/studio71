using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using SingletonManagers;

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
        
        [Header("Videos")]
        [Tooltip("Video clips to play during loading. Index 0 = Level1, Index 1 = Level2")]
        [SerializeField] private VideoClip[] loadingVideos;
        
        [Header("Loading Settings")]
        [Tooltip("Whether to wait for the video to complete or load the scene immediately")]
        [SerializeField] private bool waitForVideoToFinish = true;
        
        [Tooltip("Whether to start loading the next scene in the background while the video plays")]
        [SerializeField] private bool loadInBackground = true;
        
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
        
        private void Start()
        {
            LogMessage("LoadingSceneController starting");
            
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
            
            // Start playing the appropriate video
            PlayLoadingVideo();
            
            // Start loading the target scene in the background
            if (loadInBackground)
            {
                _loadingCoroutine = StartCoroutine(LoadTargetSceneAsync());
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
                // Assign the video clip
                videoPlayer.clip = loadingVideos[_videoIndex];
                
                // Register for video completion event
                videoPlayer.loopPointReached += OnVideoFinished;
                
                // Start playback
                videoPlayer.Play();
                _videoState = LoadingState.Loading;
                LogMessage($"Playing loading video {_videoIndex} for scene {_targetSceneIndex}");
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
            
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            
            // Update state only if not already ready
            if (_videoState != LoadingState.ReadyToActivate)
            {
                _videoState = LoadingState.ReadyToActivate;
                
                // If not loading in background and not started yet, start loading now
                if (!loadInBackground && _sceneLoadingState == LoadingState.NotStarted)
                {
                    _loadingCoroutine = StartCoroutine(LoadTargetSceneAsync());
                }
                else
                {
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
                LogMessage("Skip button activated");
            }
        }
        
        /// <summary>
        /// This should be called from the Skip Button's onClick event in the Inspector
        /// </summary>
        public void OnSkipButtonClicked()
        {
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