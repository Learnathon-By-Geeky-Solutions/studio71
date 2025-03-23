using UnityEngine;
using UnityEngine.SceneManagement;
namespace UI
{
    /// <summary>
    /// Handles the main menu.
    /// </summary>      
    public class MainMenu : MonoBehaviour
    {
        public static void PlayGame()
        {
            SceneManager.LoadSceneAsync(1);
        }
        public static void QuitGame()
        {
            Application.Quit();
        }
    }
}

