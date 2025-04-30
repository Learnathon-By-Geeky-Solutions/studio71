using SingletonManagers;
using UnityEngine;
namespace LevelSelection
{
    public class SlidingMenuUi : MonoBehaviour
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "start can not be static")]
        void Start()
        {
            AudioManager.PlaySound(SoundKeys.LevelSelectionMusic);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "ondisable can not be static")]
        void OnDisable()
        {
            AudioManager.StopSound(SoundKeys.LevelSelectionMusic);
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public static void backToMainMenu()
        {    AudioManager.PlaySound(SoundKeys.ButtonPress);
            SceneIndexes.LoadSceneByIndexAsync(SceneIndexes.MainMenuScene);
        }

    }
}