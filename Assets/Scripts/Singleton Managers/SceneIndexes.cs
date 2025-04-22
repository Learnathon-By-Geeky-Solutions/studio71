using UnityEngine.SceneManagement;

/// <summary>
/// Provides static access to scene indexes and a method to load scenes.
/// </summary>
namespace SingletonManagers{
public static class SceneIndexes
{
    public static readonly int MaineMenuScene = 0;
    public static readonly int SlidingMenuScene = 1;
    public static readonly int Level1Scene = 2;
    public static readonly int Level2Scene = 3;

    /// <summary>
    /// Loads the scene corresponding to the given index.
    /// </summary>
    /// <param name="sceneIndex">The build index of the scene to load.</param>
    public static void LoadSceneByIndex(int sceneIndex)
    {
        // Basic validation could be added here (e.g., check if index is within range)
        SceneManager.LoadScene(sceneIndex);
    }
}
}