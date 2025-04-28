using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

/// <summary>
/// Provides static access to scene indexes and a method to load scenes.
/// </summary>
namespace SingletonManagers{
public static class SceneIndexes
{
    public static readonly int MaineMenuScene = 0;
    public static readonly int SlidingMenuScene = 1;
    public static readonly int LoadingScene = 2; // Loading scene with videos
    public static readonly int Level1Scene = 3;
    public static readonly int Level2Scene = 4;
    public static readonly int Level3Scene = 5;
    public static readonly int Level4Scene = 6;
    public static readonly int Level5Scene = 7;

        // Static field to track which scene to load after the loading scene
        private static int _targetSceneIndex = -1;
    
    // Static field to track which video to play in the loading scene
    private static int _videoToPlay = -1;
    
    // Video indexes (corresponding to the videos in the LoadingSceneController)
    // Level loading videos
    public static readonly int Level1Video = 0;
    public static readonly int Level2Video = 1;
    public static readonly int Level3Video = 2;
    public static readonly int Level4Video = 3;
    public static readonly int Level5Video = 4;
    
    // Victory videos (could be the same videos or different ones)
    public static readonly int Victory1Video = 5; // Victory video for level 1
    public static readonly int Victory2Video = 6; // Victory video for level 2
    public static readonly int Victory3Video = 7; // Victory video for level 3
    public static readonly int Victory4Video = 8; // Victory video for level 4
    public static readonly int Victory5Video = 9; // Victory video for level 5

    /// <summary>
    /// DEPRECATED: Use LoadSceneByIndexAsync instead.
    /// Loads a scene synchronously by index. Warning: May cause frame drops.
    /// </summary>
    [System.Obsolete("Use LoadSceneByIndexAsync instead to prevent frame drops")]
    public static void LoadSceneByIndex(int sceneIndex)
    {
        Debug.LogWarning("Using synchronous scene loading. This may cause frame drops. Consider using LoadSceneByIndexAsync instead.");
        SceneManager.LoadScene(sceneIndex);
    }
    
    /// <summary>
    /// Loads a scene asynchronously by index.
    /// </summary>
    /// <param name="sceneIndex">The build index of the scene to load</param>
    /// <param name="allowSceneActivation">Whether to automatically activate the scene when loaded</param>
    /// <returns>The AsyncOperation that can be used to track progress</returns>
    public static AsyncOperation LoadSceneByIndexAsync(int sceneIndex, bool allowSceneActivation = true)
    {
        Debug.Log($"Asynchronously loading scene {sceneIndex}");
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneIndex);
        loadOperation.allowSceneActivation = allowSceneActivation;
        return loadOperation;
    }
    
    /// <summary>
    /// Loads a scene with a video transition.
    /// </summary>
    /// <param name="targetSceneIndex">Target scene to load after video</param>
    /// <param name="videoIndex">Video to play during loading</param>
    public static void LoadSceneWithVideo(int targetSceneIndex, int videoIndex)
    {
        // Save the target scene and video to play
        SetTargetScene(targetSceneIndex);
        SetVideoToPlay(videoIndex);
        
        // Load the loading scene asynchronously
        LoadSceneByIndexAsync(LoadingScene);
    }
    
    /// <summary>
    /// Gets the target scene index set before loading the loading scene.
    /// </summary>
    public static int GetTargetScene()
    {
        return _targetSceneIndex;
    }
    
    /// <summary>
    /// Gets the video index to play during loading.
    /// </summary>
    public static int GetVideoToPlay()
    {
        return _videoToPlay;
    }
    
    /// <summary>
    /// Loads a level from the sliding menu with the appropriate video.
    /// </summary>
    public static void LoadLevelFromSlidingMenu(int sceneIndex)
    {
        // Determine which video to play based on the destination scene
        int videoToPlay = -1;
        
        // For regular level scenes (Level1-5)
        if (sceneIndex >= Level1Scene && sceneIndex <= Level5Scene)
        {
            // Calculate level video index dynamically (Level1Scene -> Level1Video, etc.)
            int levelNumber = sceneIndex - Level1Scene + 1; // Convert scene index to 1-5
            
            // Map level number to corresponding video
            switch (levelNumber)
            {
                case 1: videoToPlay = Level1Video; break;
                case 2: videoToPlay = Level2Video; break;
                case 3: videoToPlay = Level3Video; break;
                case 4: videoToPlay = Level4Video; break;
                case 5: videoToPlay = Level5Video; break;
                default: videoToPlay = Level1Video; break; // Fallback
            }
            
            Debug.Log($"Loading level {levelNumber} with intro video {videoToPlay}");
        }
        // For sliding menu (likely after winning a level)
        else if (sceneIndex == SlidingMenuScene)
        {
            // When returning to sliding menu (like after winning),
            // we use the video specified in SetVideoToPlay
            // If none was set (-1), no video will play (blank will be shown)
            videoToPlay = GetVideoToPlay();
            if (videoToPlay < 0)
            {
                Debug.LogWarning("No video specified for transition to sliding menu. Using default.");
                videoToPlay = Level1Video; // Fallback to a default video
            }
        }
        else
        {
            // For any other scene, use a default video
            videoToPlay = Level1Video;
            Debug.LogWarning($"No specific video defined for scene {sceneIndex}. Using default video.");
        }
        
        // Load the scene with the appropriate video
        LoadSceneWithVideo(sceneIndex, videoToPlay);
    }

    private static void SetTargetScene(int sceneIndex)
    {
        _targetSceneIndex = sceneIndex;
    }

    private static void SetVideoToPlay(int videoIndex)
    {
        _videoToPlay = videoIndex;
    }
}
}