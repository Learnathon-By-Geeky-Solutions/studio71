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
            AudioManager.PlaySound(SoundKeys.BackgroundMusic);
        }
        public void PlayGame()
        {   
            AudioManager.PlaySound(SoundKeys.ButtonPress);
            SceneManager.LoadSceneAsync(1);
        }

        public void Settings()
        {
            AudioManager.PlaySound(SoundKeys.ButtonPress);
            Debug.Log("Settings button clicked");
        }
        public void QuitGame()
        {   
            AudioManager.PlaySound(SoundKeys.ButtonPress);
            Debug.Log("Quit button clicked");
            Application.Quit();
        }
    }
}

