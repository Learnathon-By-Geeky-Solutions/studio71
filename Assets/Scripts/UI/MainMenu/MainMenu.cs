using UnityEngine;
using UnityEngine.SceneManagement;
using SingletonManagers;
namespace UI

{
    /// <summary>
    /// Handles the main menu.
    /// </summary>      
    public class MainMenu : MonoBehaviour
    {   
         private void Start()
        {
            // Play background music when the menu loads
            AudioManager.Instance.PlaySound("background_music");
        }
        public static void PlayGame()
        {   AudioManager.Instance.PlaySound("button_press");
            SceneManager.LoadSceneAsync(1);
        }

        public static void Settings()
        {
            AudioManager.Instance.PlaySound("button_press");
        }
        public static void QuitGame()
        {   AudioManager.Instance.PlaySound("button_press");
            Application.Quit();
        }
    }
}

