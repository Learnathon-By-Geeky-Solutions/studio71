using UnityEngine;
using UnityEngine.SceneManagement;
using SingletonManagers;
namespace UI.MainMenu
{
    public class MainMenuTransitioner : MonoBehaviour
    {   
        private void Start()
        {
            // Play background music when the menu loads
            AudioManager.PlaySound("background_music");
        }
        public void PlayGame()
        {   
            AudioManager.PlaySound("button_press");
            SceneManager.LoadSceneAsync(1);
        }

        public void Settings()
        {
            AudioManager.PlaySound("button_press");
        }
        public void QuitGame()
        {   
            AudioManager.PlaySound("button_press");
            Application.Quit();
        }
    }
}

