using System;
using Player;
using SingletonManagers;
using UnityEngine;
namespace UI
{
    public class PauseMenu : MonoBehaviour
    {

        private bool s_gameIsPaused = false;
        public  bool IsGamePaused()
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
        }
        private void OnDisable()
        {
            if (_playerController != null)
            {
                Resume();
            }
            InputHandler.Instance.OnPause -= Pause;

        }

        // Update is called once per frame


        private void Resume()
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            s_gameIsPaused = false;
            _playerController.enabled = true; // Enable player controls when resuming
            AudioManager.StopSound(SoundKeys.BackgroundMusic);
            Debug.Log("Resuming game...");
            // Hide the pause menu UI here
        }

        private void Pause()
        {
            if (!IsGamePaused()) // Use getter method
            {
                pauseMenuUI.SetActive(true);
                Time.timeScale = 0f;
                s_gameIsPaused = true;
                _playerController.enabled = false; // Disable player controls when paused
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
            SceneIndexes.LoadSceneByIndex(SceneIndexes.MaineMenuScene);
        }
        public void Sound()
        {
            soundMenuUI.SetActive(true);
            pauseMenuButtons.SetActive(false);
        }
    }
}

