using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class NetworkedEnemySpawner : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject enemyPrefab;
        public int maxEnemies;
        public float spawnInterval;
    }

    [Header("Spawn Settings")]
    public List<Transform> spawnPoints;
    public List<EnemyType> enemyTypes;

    [Header("Wave Settings")]
    public int currentWave = 1;
    public float timeBetweenWaves = 5f;

    private float lastSpawnTime;
    private int currentEnemyCount;
    private int maxEnemiesPerWave;

    void Start()
    {
        // Only master client handles spawning
        if (PhotonNetwork.IsMasterClient)
        {
            CalculateMaxEnemies();
            StartNextWave();
        }
    }

    void CalculateMaxEnemies()
    {
        maxEnemiesPerWave = 0;
        foreach (var enemyType in enemyTypes)
        {
            maxEnemiesPerWave += enemyType.maxEnemies;
        }
    }

    void StartNextWave()
    {
        if (currentEnemyCount >= maxEnemiesPerWave)
        {
            // Wave complete logic
            currentWave++;
            currentEnemyCount = 0;
        }

        StartCoroutine(SpawnWave());
    }

    System.Collections.IEnumerator SpawnWave()
    {
        foreach (var enemyType in enemyTypes)
        {
            for (int i = 0; i < enemyType.maxEnemies; i++)
            {
                if (currentEnemyCount >= maxEnemiesPerWave) break;

                SpawnEnemy(enemyType.enemyPrefab);
                yield return new WaitForSeconds(enemyType.spawnInterval);
            }
        }
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        // Select random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        // Network instantiate enemy
        PhotonNetwork.Instantiate(enemyPrefab.name, spawnPoint.position, Quaternion.identity);
        
        currentEnemyCount++;
    }

    // Method to track enemy deaths
    public void EnemyDestroyed()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentEnemyCount--;
            
            if (currentEnemyCount <= 0)
            {
                StartNextWave();
            }
        }
    }
}