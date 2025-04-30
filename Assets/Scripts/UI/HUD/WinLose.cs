using UnityEngine;
using TMPro;
using SingletonManagers;
using Unity.VisualScripting;
namespace UI{
public class WinLose : MonoBehaviour
{
    [SerializeField] private GameObject _winText;
    [SerializeField] private GameObject _loseText;
    [SerializeField] private float _returnDelay = 2f;
    
    private void Start()
    {
        LevelConditionManager.Instance._currentConditions.OnWin += WIN;
        LevelConditionManager.Instance._currentConditions.OnLose += LOSE;
    }
    
    private void OnDisable()
    {
        LevelConditionManager.Instance._currentConditions.OnWin -= WIN;
        LevelConditionManager.Instance._currentConditions.OnLose -= LOSE;
    }
    
    private void LOSE()
    {
        _loseText.SetActive(true);
        AudioManager.StopSound(SoundKeys.inGameSound);
        Invoke("ReturnToLevelSelect", _returnDelay);
    }
    
    private void WIN()
    {
        _winText.SetActive(true);
        AudioManager.StopSound(SoundKeys.inGameSound);
        Invoke("PlayWinVideoAndReturn", _returnDelay);
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "invoke cant call static function")]
    private void PlayWinVideoAndReturn()
    {
        // Get the current scene index
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        
        // Get the level number (1-5) based on scene index
        int levelNumber = GetLevelNumber(currentSceneIndex);
        
        // Calculate victory video index or use default if not a valid level
        int videoIndex = levelNumber > 0 
            ? SceneIndexes.Victory1Video + (levelNumber - 1) 
            : SceneIndexes.Victory1Video;
        
        Debug.Log($"Playing victory video {videoIndex} after completing level {currentSceneIndex}");
        
        // Set the target scene to return to sliding menu but play the win video first
        SceneIndexes.LoadSceneWithVideo(SceneIndexes.SlidingMenuScene, videoIndex);
    }
    
    private static int GetLevelNumber(int sceneIndex)
    {
        // Map scene index to level number (1-5)
        if (sceneIndex == SceneIndexes.Level1Scene) return 1;
        if (sceneIndex == SceneIndexes.Level2Scene) return 2;
        if (sceneIndex == SceneIndexes.Level3Scene) return 3;
        if (sceneIndex == SceneIndexes.Level4Scene) return 4;
        if (sceneIndex == SceneIndexes.Level5Scene) return 5;
        
        // Not a valid level scene
        Debug.LogWarning($"Scene index {sceneIndex} is not a recognized level. Using default victory video.");
        return 0;
    }
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "invoke cant call static function")]
    private  void ReturnToLevelSelect()
    {  
        SceneIndexes.LoadSceneByIndexAsync(SceneIndexes.SlidingMenuScene);
    }
}}
