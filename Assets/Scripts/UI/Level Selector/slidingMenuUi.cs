using SingletonManagers;
using UnityEngine;
namespace LevelSelection
{
public class slidingMenuUi : MonoBehaviour
{   void Start()
{
     AudioManager.PlaySound(SoundKeys.LevelSelectionMusic);
}
void OnDisable()
{
    AudioManager.StopSound(SoundKeys.LevelSelectionMusic);
}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void backToMainMenu()
    {
       SceneIndexes.LoadSceneByIndexAsync(SceneIndexes.MainMenuScene);
    }
    
}
}