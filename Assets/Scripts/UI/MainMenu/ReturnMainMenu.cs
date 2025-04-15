using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainMenu
{
    public class ReturnMainMenu : MonoBehaviour
    {
        private GameObject player;
        private GameObject[] enemies;

        void Update()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (player == null || enemies.Length == 0)
            {
                SceneManager.LoadSceneAsync(0);
            }
        }
    }
}
