using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private List<Transform> spawnPoints;  // List of spawn points
    [SerializeField]
    private List<GameObject> enemies;     // List of enemy prefabs
    [SerializeField]
    private float initialSpawnRate = 2f;  // Initial spawn rate in seconds

    private float spawnRate;              // Current spawn rate

    private void Start()
    {
        spawnRate = initialSpawnRate;
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // Randomly select a spawn point and an enemy
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject enemy = enemies[Random.Range(0, enemies.Count)];

            // Instantiate the enemy at the selected spawn point
            Instantiate(enemy, spawnPoint.position, Quaternion.identity);

            // Wait for the next spawn
            yield return new WaitForSeconds(spawnRate);
        }
    }

    private void ChangeSpawnRate()
    {
        // Update spawn rate, logic can be customized based on your requirements
        spawnRate = Mathf.Max(0.5f, spawnRate - 0.5f);  // Example: decrease spawn rate by 0.5 but keep it above 0.5
        Debug.Log("Spawn rate changed to: " + spawnRate);
    }
}