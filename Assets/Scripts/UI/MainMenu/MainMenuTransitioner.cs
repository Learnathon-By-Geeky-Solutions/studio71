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
        [Header("Scene Configuration")]
        [Tooltip("Index of the game scene to load when Play is clicked")]
        [SerializeField] private int gameSceneIndex = 1;
        
        [Header("Debug Options")]
        [Tooltip("Whether to log button click events to the console")]
        [SerializeField] private bool enableDebugLogs = true;

        #region Unity Lifecycle Methods
        private void Start()
        {
            PlayBackgroundMusic();
            LogDebug("MainMenuTransitioner started");
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
            LogDebug("Play button clicked");
            LoadGameScene();
        }

        /// <summary>
        /// Called when the Settings button is clicked.
        /// Opens the settings panel or menu.
        /// </summary>
        public void Settings()
        {
            PlayButtonSound();
            LogDebug("Settings button clicked");
            // TODO: Implement settings functionality
        }

        /// <summary>
        /// Called when the Quit button is clicked.
        /// Exits the application.
        /// </summary>
        public void QuitGame()
        {
            PlayButtonSound();
            LogDebug("Quit button clicked");
            QuitApplication();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Plays the background music for the main menu.
        /// </summary>
        private void PlayBackgroundMusic()
        {
            AudioManager.PlaySound(SoundKeys.BackgroundMusic);
        }

        /// <summary>
        /// Plays the button press sound effect.
        /// </summary>
        private void PlayButtonSound()
        {
            AudioManager.PlaySound(SoundKeys.ButtonPress);
        }

        /// <summary>
        /// Loads the game scene asynchronously.
        /// </summary>
        private void LoadGameScene()
        {
            try
            {
                SceneManager.LoadSceneAsync(gameSceneIndex);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game scene at index {gameSceneIndex}: {e.Message}");
            }
        }

        /// <summary>
        /// Quits the application.
        /// </summary>
        private void QuitApplication()
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
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
        #endregion
    }
}

