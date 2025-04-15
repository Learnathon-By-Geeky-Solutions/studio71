using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainMenu
{
    public class ReturnMainMenu : MonoBehaviour
    {
        void Update()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (player == null || enemies.Length == 0)
            {
                SceneManager.LoadSceneAsync(0);
            }
        }
    }
}
