using UnityEngine;
using UnityEngine.SceneManagement;
using SingletonManagers;
namespace UI.MainMenu
{
    public class MainMenuTransitioner : MonoBehaviour
    {   
        private void Start()
        {
            Debug.Log("MainMenuTransitioner started");
            AudioManager.PlaySound(SoundKeys.BackgroundMusic);
            
        }
        public void PlayGame()
        {   
            AudioManager.PlaySound(SoundKeys.ButtonPress);
            Debug.Log("Play button clicked");
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

