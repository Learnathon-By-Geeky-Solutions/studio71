using UnityEngine;
using UnityEngine.SceneManagement;
using SingletonManagers;
using UnityEngine.Video;
using System.Collections;

namespace UI.MainMenu
{
    /// <summary>
    /// Handles menu transitions and scene loading for the main menu.
    /// </summary>
    public class MainMenuTransitioner : MonoBehaviour
    {
        [SerializeField] private GameObject soundMenuUI;
        [SerializeField] private GameObject pauseMenuButtons;
        [SerializeField] private GameObject _videoGameobject;
        [SerializeField] private GameObject _rawImageGameObject;


        #region Unity Lifecycle Methods
        private void OnEnable()
        {
            StartCoroutine(VideoFinished());
            if (!LevelConditionManager.Instance._videoPlayed) return;
            _videoGameobject.SetActive(false);
            _rawImageGameObject.SetActive(false);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "start can not be static")]
        private void Start()
        {
            if (!_videoGameobject.activeInHierarchy)
            {
                PlayBackgroundMusic();
            }
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

        private IEnumerator VideoFinished()
        {

            yield return new WaitForSeconds((float)_videoGameobject.GetComponent<VideoPlayer>().clip.length);
            if (_videoGameobject.activeInHierarchy)
            {
                PlayBackgroundMusic();
            }
            _videoGameobject.SetActive(false);
            _rawImageGameObject.SetActive(false);
        }
        
        #endregion
    }
}

