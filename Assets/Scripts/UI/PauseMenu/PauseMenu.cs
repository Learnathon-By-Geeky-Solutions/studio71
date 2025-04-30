using System;
using dialogue;
using Player;
using SingletonManagers;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace UI
{
    public class PauseMenu : MonoBehaviour
    {

        private bool s_gameIsPaused = false;
        public bool IsGamePaused()
        {
            return s_gameIsPaused;
        }

        [SerializeField] private GameObject pauseMenuUI;

        [SerializeField] private GameObject soundMenuUI;
        [SerializeField] private GameObject pauseMenuButtons;
        private PlayerController _playerController;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            if (_playerController == null)
            {
                Debug.LogError("PlayerController not found in the scene.");
            }

        }
        private void OnEnable()
        {
            InputHandler.Instance.OnPause += Pause;
            AudioManager.PlaySound(SoundKeys.inGameSound);
        }
        private void OnDisable()
        { 
            InputHandler.Instance.OnPause -= Pause;
            Resume();
            AudioManager.StopSound(SoundKeys.inGameSound);

        }

        // Update is called once per frame


        public void Resume()
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            s_gameIsPaused = false;
            _playerController.enabled = true; // Enable player controls when resuming
            AudioManager.StopSound(SoundKeys.BackgroundMusic);
            AudioManager.PlaySound(SoundKeys.inGameSound);
            Debug.Log("Resuming game...");
            // Hide the pause menu UI here
        }
        private void Pause()
        {
            if (SceneManager.GetActiveScene().buildIndex < 3 || InkDialogueManager.IsDialogueOpen) return;
            if (!IsGamePaused())
            {
                pauseMenuUI.SetActive(true);
                Time.timeScale = 0f;
                s_gameIsPaused = true;
                _playerController.enabled = false;
                AudioManager.StopSound(SoundKeys.inGameSound);
                AudioManager.PlaySound(SoundKeys.BackgroundMusic);

            }
            else
            {
                Resume();
            }
            // Show the pause menu UI here
        }

        public static void LoadMenu()
        {
            AudioManager.StopSound(SoundKeys.BackgroundMusic);
            SceneIndexes.LoadSceneByIndexAsync(SceneIndexes.MainMenuScene);
        }
        public void Sound()
        {
            soundMenuUI.SetActive(true);
            pauseMenuButtons.SetActive(false);
        }

        public static void buttonSound()
        {
            AudioManager.PlaySound(SoundKeys.ButtonPress);
        }
    }
}

