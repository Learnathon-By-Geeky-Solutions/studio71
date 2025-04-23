using UnityEngine;
using UnityEngine.SceneManagement;
using SingletonManagers;

namespace UI.MainMenu
{
    /// <summary>
    /// Handles menu transitions and scene loading for the main menu.
    /// </summary>
    public class MainMenuTransitioner : MonoBehaviour
    {
        //For sonarCloud FallBack
         private bool isBackgroundPlaying = false;

        #region Unity Lifecycle Methods
        private void Start()
        {
            isBackgroundPlaying = true;
                PlayBackgroundMusic();
                print($"Background music is playing: {isBackgroundPlaying}");
         
        }
        #endregion

        #region Public Button Actions
        /// <summary>
        /// Called when the Play button is clicked.
        /// Loads the game scene asynchronously.
        /// </summary>
        public void PlayGame()
        {
            PlayButtonSound();
            AudioManager.StopSound(SoundKeys.BackgroundMusic);
            isBackgroundPlaying = false;
            print($"Background music is not playing: {isBackgroundPlaying}");
            SceneIndexes.LoadSceneByIndex(SceneIndexes.SlidingMenuScene);
        }

        /// <summary>
        /// Called when the Settings button is clicked.
        /// Opens the settings panel or menu.
        /// </summary>
        public static void Settings()
        {
            PlayButtonSound();
            
        }

        /// <summary>
        /// Called when the Quit button is clicked.
        /// Exits the application.
        /// </summary>
        public static void QuitGame()
        {
            PlayButtonSound();
           
            QuitApplication();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Plays the background music for the main menu.
        /// </summary>
        private static void PlayBackgroundMusic()
        {
            AudioManager.PlaySound(SoundKeys.BackgroundMusic);
        }

        /// <summary>
        /// Plays the button press sound effect.
        /// </summary>
        private static void PlayButtonSound()
        {
            AudioManager.PlaySound(SoundKeys.ButtonPress);
        }

      

        /// <summary>
        /// Quits the application.
        /// </summary>
        private static void QuitApplication()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        /// <summary>
        /// Logs a debug message if debug logging is enabled.
        /// </summary>
        
        #endregion
    }
}

