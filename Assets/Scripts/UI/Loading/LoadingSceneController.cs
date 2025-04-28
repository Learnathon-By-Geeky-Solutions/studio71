using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using SingletonManagers;

namespace UI.Loading
{
    /// <summary>
    /// Controls the loading scene with video playback.
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
        
        private AsyncOperation _loadOperation;
        private int _targetSceneIndex;
        private int _videoIndex;
        private bool _isVideoFinished = false;
        
        private void Start()
        {
            // Get the scene and video info from SceneIndexes
            _targetSceneIndex = SceneIndexes.GetTargetScene();
            _videoIndex = SceneIndexes.GetVideoToPlay();
            
            // Validate the target scene
            if (_targetSceneIndex < 0)
            {
                // Ensure the skip button is disabled initially
                if (skipButton != null)
                {
                    skipButton.SetActive(false);
                }
                
                Debug.LogError("No target scene specified for loading scene.");
                // Fallback to main menu
                _targetSceneIndex = SceneIndexes.MaineMenuScene;
            }
            
            // Start playing the appropriate video
            PlayLoadingVideo();
            
            // Start loading the target scene in the background
            if (loadInBackground)
            {
                StartCoroutine(LoadTargetSceneAsync());
            }
        }
        
        private void PlayLoadingVideo()
        {
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
                Debug.Log($"Playing loading video {_videoIndex} for scene {_targetSceneIndex}");
            }
            else
            {
                Debug.LogError("No valid video clips found in LoadingSceneController.");
                _isVideoFinished = true; // Skip video since there isn't one
            }
        }
        
        private void OnVideoFinished(VideoPlayer vp)
        {
            _isVideoFinished = true;
            
            // If we're not loading in background, start loading now
            if (!loadInBackground)
            {
                StartCoroutine(LoadTargetSceneAsync());
            }
            // If we are loading in background and the scene is ready, activate it
            else if (_loadOperation != null && _loadOperation.isDone)
            {
                ActivateLoadedScene();
            }
        }
        
        private IEnumerator LoadTargetSceneAsync()
        {
            Debug.Log($"Starting to load scene {_targetSceneIndex} in background");
            // Start loading the target scene asynchronously
            _loadOperation = SceneManager.LoadSceneAsync(_targetSceneIndex);
            
            // Don't let the scene activate yet
            _loadOperation.allowSceneActivation = false;
            
            // Log loading progress periodically
            float lastProgress = 0f;
            
            // Wait until the load is nearly complete
            while (_loadOperation.progress < 0.9f)
            {
                // Log progress updates but only when it changes significantly
                if (_loadOperation.progress >= lastProgress + 0.1f)
                {
                    Debug.Log($"Scene loading progress: {_loadOperation.progress:P0}");
                    lastProgress = _loadOperation.progress;
                }
                yield return null;
            }

            Debug.Log("Scene loading reached 90% complete - ready for activation");
            
            // Scene is ready to load, activate the skip button
            ActivateSkipButton();
            
            // Scene is ready to activate
            if (!waitForVideoToFinish || _isVideoFinished)
            {
                ActivateLoadedScene();
            }
            else if (waitForVideoToFinish)
            {
                // Continue waiting for the video to finish if needed
                Debug.Log("Scene loaded but waiting for video to finish...");
                
                // Wait here until the video is finished
                while (!_isVideoFinished)
                {
                    yield return null;
                }
                
                // Video is now finished, activate the scene
                ActivateLoadedScene();
            }
        }
        
        private void Update()
        {
            // This is a backup check, but the coroutine now handles this more reliably
            if (waitForVideoToFinish && loadInBackground)
            { 
                if (_isVideoFinished && _loadOperation != null && _loadOperation.progress >= 0.9f && !_loadOperation.allowSceneActivation)
                {
                    Debug.Log("Update method detected both video complete and scene ready - activating scene");
                    ActivateLoadedScene();
                }
            }
        }
        
        private void ActivateLoadedScene()
        {
            if (_loadOperation != null)
            {
                Debug.Log($"Loading complete. Activating scene {_targetSceneIndex}");
                _loadOperation.allowSceneActivation = true;
            }
            else
            {
                // Direct load as fallback
                SceneManager.LoadScene(_targetSceneIndex);
            }
        }
        
        // Optional: Add a skip button functionality
        public void SkipVideo()
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            _isVideoFinished = true;
            
            if (!loadInBackground)
            {
                StartCoroutine(LoadTargetSceneAsync());
            }
            else if (_loadOperation != null && _loadOperation.progress >= 0.9f)
            {
                ActivateLoadedScene();
            }
        }
        
        private void ActivateSkipButton()
        {
            // Activate the skip button when the scene is ready to load
            if (skipButton != null)
            {
                skipButton.SetActive(true);
                Debug.Log("Scene loading complete - Skip button activated");
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
    }
} 