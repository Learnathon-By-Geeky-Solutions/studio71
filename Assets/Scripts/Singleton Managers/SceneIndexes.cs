using UnityEngine.SceneManagement;
using UnityEngine;

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
    
    // Static field to track which scene to load after the loading scene
    private static int s_targetSceneToLoad = -1;
    
    // Static field to track which video to play in the loading scene
    private static int s_videoToPlay = -1;
    
    // Video indexes (corresponding to the videos in the LoadingSceneController)
    public static readonly int Level1Video = 0;
    public static readonly int Level2Video = 1;

    /// <summary>
    /// Loads the scene corresponding to the given index.
    /// </summary>
    /// <param name="sceneIndex">The build index of the scene to load.</param>
    public static void LoadSceneByIndex(int sceneIndex)
    {
        // Basic validation could be added here (e.g., check if index is within range)
        SceneManager.LoadScene(sceneIndex);
    }
    
    /// <summary>
    /// Loads the scene with a video transition.
    /// </summary>
    /// <param name="sceneIndex">The build index of the target scene to load after the video.</param>
    /// <param name="videoIndex">The index of the video to play during loading.</param>
    public static void LoadSceneWithVideo(int sceneIndex, int videoIndex)
    {
        // Set the static fields that the loading scene will check
        s_targetSceneToLoad = sceneIndex;
        s_videoToPlay = videoIndex;
        
        // Load the loading scene
        SceneManager.LoadScene(LoadingScene);
    }
    
    /// <summary>
    /// Gets the target scene to load after the loading scene.
    /// </summary>
    /// <returns>The build index of the scene to load.</returns>
    public static int GetTargetScene()
    {
        return s_targetSceneToLoad;
    }
    
    /// <summary>
    /// Gets the index of the video to play during loading.
    /// </summary>
    /// <returns>The index of the video to play.</returns>
    public static int GetVideoToPlay()
    {
        return s_videoToPlay;
    }
    
    /// <summary>
    /// Loads scene from SlidingMenuScene with appropriate video based on the destination.
    /// </summary>
    /// <param name="sceneIndex">The build index of the scene to load.</param>
    public static void LoadLevelFromSlidingMenu(int sceneIndex)
    {
        // Determine which video to play based on the destination scene
        int videoIndex;
        
        if (sceneIndex == Level1Scene)
        {
            videoIndex = Level1Video;
        }
        else if (sceneIndex == Level2Scene)
        {
            videoIndex = Level2Video;
        }
        else
        {
            // For other scenes, just load directly
            LoadSceneByIndex(sceneIndex);
            return;
        }
        
        // Load with the appropriate video
        LoadSceneWithVideo(sceneIndex, videoIndex);
    }
}
}