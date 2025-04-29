
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using SingletonManagers;
using dialogue;
namespace LevelSpecific {
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField]
        private List<Transform> spawnPoints;  // List of spawn points
        [SerializeField]
        private List<GameObject> enemies;     // List of enemy prefabs
        [SerializeField]
        private float initialSpawnRate = 2f;  // Initial spawn rate in seconds

        private float spawnRate;              // Current spawn rate
        private Timer _levelTimer;
        private bool _isSpawning = true;
        private void Awake()
        {
            _levelTimer = GameObject.Find("Timer").GetComponent<Timer>();
            if (_levelTimer == null)
            {
                Debug.LogWarning("Timer Not Found On the Scene");
            }
        }
        private void Start()
        {
            spawnRate = initialSpawnRate;
            _levelTimer.StartTimer(LevelConditionManager.Instance._currentConditions.SurviveTime);
            StartCoroutine(SpawnEnemies());
            StartCoroutine(ChangeSpawnRate());
            StartCoroutine(StopSpawning());
        }

        private IEnumerator SpawnEnemies()
        {
            yield return new WaitForSeconds(0.1f);
            while (_isSpawning)
            {
                // Wait while dialogue is open
                yield return new WaitUntil(() => !InkDialogueManager.IsDialogueOpen);

                // Spawn an enemy
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                GameObject enemy = enemies[Random.Range(0, enemies.Count)];
                Instantiate(enemy, spawnPoint.position, Quaternion.identity);

                // Wait for next spawn
                yield return new WaitForSeconds(spawnRate);
            }
        }
        private IEnumerator ChangeSpawnRate()
        {
            yield return new WaitForSeconds(LevelConditionManager.Instance._currentConditions.SurviveTime / 2);
            // Update spawn rate, logic can be customized based on requirements
            spawnRate = Mathf.Max(0.5f, spawnRate - 0.5f);  // Example: decrease spawn rate by 0.5 but keep it above 0.5
            Debug.Log("Spawn rate changed to: " + spawnRate);
        }
        private IEnumerator StopSpawning()
        {
            yield return new WaitForSeconds(LevelConditionManager.Instance._currentConditions.SurviveTime);
            _isSpawning = false;
            LevelConditionManager.Instance.OnTimerFinished();
        }


    }
}