using System;
using Player;
using SingletonManagers;
using UnityEngine;
namespace UI.PauseMenu{
public class PauseMenu : MonoBehaviour
{   
    // Encapsulated the state with a private field and a public static getter method
    private static bool s_gameIsPaused = false;
    public static bool IsGamePaused() => s_gameIsPaused;

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
        s_gameIsPaused = false; // Set private field
        _playerController.enabled = true; // Enable player controls when resuming
        AudioManager.StopSound(SoundKeys.BackgroundMusic);
        Debug.Log("Resuming game...");
        // Hide the pause menu UI here
    }

    void Pause()
    {   if(!IsGamePaused()) // Use getter method
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            s_gameIsPaused = true; // Set private field
            _playerController.enabled = false; // Disable player controls when paused
            AudioManager.PlaySound(SoundKeys.BackgroundMusic);

        }
        else  {
            Resume();
        }
        // Show the pause menu UI here
    }
    
    public static void LoadMenu()
    {
        SceneIndexes.LoadSceneByIndex(SceneIndexes.MaineMenuScene);
    }
    public void Sound()
    {
        soundMenuUI.SetActive(true);
        pauseMenuButtons.SetActive(false);
    }
}
}
