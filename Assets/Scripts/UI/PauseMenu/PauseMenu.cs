using System;
using Player;
using SingletonManagers;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{   
    // Encapsulated the state
    private static bool s_gameIsPaused = false;
    public static bool GameIsPaused => s_gameIsPaused; // Public getter

    [SerializeField] private GameObject pauseMenuUI;
    
    [SerializeField] private GameObject  soundMenuUI;
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
        InputHandler.Instance.OnPause -= Pause;
       
    }

    // Update is called once per frame
    
    
    public void Resume()
    {   pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        s_gameIsPaused = false; // Use private field
        _playerController.enabled = true; // Enable player controls when resuming
        AudioManager.StopSound(SoundKeys.BackgroundMusic);
        Debug.Log("Resuming game...");
        // Hide the pause menu UI here
    }

    void Pause()
    {   if(!s_gameIsPaused) // Use private field
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            s_gameIsPaused = true; // Use private field
            _playerController.enabled = false; // Disable player controls when paused
            AudioManager.PlaySound(SoundKeys.BackgroundMusic);

        }
        else  {
            Resume();
        }
        // Show the pause menu UI here
    }
    
    public void LoadMenu()
    {
        Debug.Log("Loading menu...");
        // Load the main menu scene here
        // SceneManager.LoadScene("MainMenu"); // Uncomment and replace with your main menu scene name
    }
    public void Sound()
    {
        soundMenuUI.SetActive(true);
        pauseMenuButtons.SetActive(false);

        // Restart the current scene here
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Uncomment to restart the current scene
    }
}
