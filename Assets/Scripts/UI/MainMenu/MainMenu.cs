using UnityEngine;
using UnityEngine.SceneManagement;
using SingletonManagers;
namespace UI.MainMenu
{
    public class MainMenu : MonoBehaviour
    {   
        private void Start()
        {
            // Play background music when the menu loads
            AudioManager.PlaySound("background_music");
        }
        public static void PlayGame()
        {   
            AudioManager.PlaySound("button_press");
            SceneManager.LoadSceneAsync(1);
        }

        public static void Settings()
        {
            AudioManager.PlaySound("button_press");
        }
        public static void QuitGame()
        {   
            AudioManager.PlaySound("button_press");
            Application.Quit();
        }
    }
}

