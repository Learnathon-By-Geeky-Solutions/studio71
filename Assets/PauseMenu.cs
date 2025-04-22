using UnityEngine;

public class PauseMenu : MonoBehaviour
{   
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    public void Resume()
    {   pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        // Hide the pause menu UI here
    }
    void Pause()
    {   pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        // Show the pause menu UI here
    }
    public void LoadMenu()
    {
        Debug.Log("Loading menu...");
        // Load the main menu scene here
        // SceneManager.LoadScene("MainMenu"); // Uncomment and replace with your main menu scene name
    }
    public void restart()
    {
        Debug.Log("Restarting game...");
        // Restart the current scene here
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Uncomment to restart the current scene
    }
}
