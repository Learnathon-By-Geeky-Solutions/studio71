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
        [SerializeField] private GameObject soundMenuUI;
        [SerializeField] private GameObject pauseMenuButtons;
         

        #region Unity Lifecycle Methods
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "start can not be static")]
        private void Start()
        {
           
                PlayBackgroundMusic();
              
         
        }
        #endregion

        #region Public Button Actions
        /// <summary>
        /// Called when the Play button is clicked.
        /// Loads the sliding menu scene asynchronously.
        /// </summary>
        public static void PlayGame()
        {
            PlayButtonSound();
            AudioManager.StopSound(SoundKeys.BackgroundMusic);
            SceneIndexes.LoadSceneByIndexAsync(SceneIndexes.SlidingMenuScene);
        }

        /// <summary>
        /// Called when the Settings button is clicked.
        /// Opens the settings panel or menu.
        /// </summary>
        public void Settings()
        {
            PlayButtonSound();
            soundMenuUI.SetActive(true);
            pauseMenuButtons.SetActive(false);
            
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

