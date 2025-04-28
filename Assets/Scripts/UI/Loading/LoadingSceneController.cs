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
            // Start loading the target scene asynchronously
            _loadOperation = SceneManager.LoadSceneAsync(_targetSceneIndex);
            
            // Don't let the scene activate yet
            _loadOperation.allowSceneActivation = false;
            
            // Wait until the load is nearly complete
            while (_loadOperation.progress < 0.9f)
            {
                yield return null;
            }
            
            // Scene is ready to activate
            if (!waitForVideoToFinish || _isVideoFinished)
            {
                ActivateLoadedScene();
            }
            // Otherwise we'll wait for the video to finish
        }
        
        private void Update()
        {
            // If we're waiting for both the video and scene load to complete
            if (waitForVideoToFinish && loadInBackground && _isVideoFinished && _loadOperation != null && _loadOperation.progress >= 0.9f)
            {
                ActivateLoadedScene();
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
    }
} 