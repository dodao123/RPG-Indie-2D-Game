using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private int initialEnemyCount = 6;
    [SerializeField] private int killThreshold = 4;
    [SerializeField] private int maxEnemyCount = 12;

    private int enemiesKilled = 0;
    private int wavesCompleted = 0;
    private bool isBossSpawned = false;
    private List<GameObject> enemies = new List<GameObject>();
    private bool isSpawning = false;

    private void Start()
    {
        SpawnEnemies(initialEnemyCount);
    }

    private void Update()
    {
        if (!isSpawning && AreAllEnemiesKilled())
        {
            isSpawning = true;
            StartCoroutine(SpawnNextWave());
        }
    }

    private void SpawnEnemies(int count)
    {
        int spawnCount = Mathf.Min(count, maxEnemyCount);
        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Vector2 spawnPosition = spawnPoints[randomIndex].position;
            int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject enemyPrefab = enemyPrefabs[randomEnemyIndex];
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemies.Add(enemy);
        }
    }

    private bool AreAllEnemiesKilled()
    {
        enemies.RemoveAll(enemy => enemy == null);
        return enemies.Count == 0;
    }

    private IEnumerator SpawnNextWave()
    {
        yield return new WaitForSeconds(2f);
        enemiesKilled = 0;
        wavesCompleted++;

        if (wavesCompleted >= 1 && !isBossSpawned)
        {
            SpawnBoss();
            SpawnBoss();
        }
        else
        {
            int additionalEnemies = Random.Range(7, 13);
            SpawnEnemies(additionalEnemies);
            killThreshold += 4;
        }
        isSpawning = false;
    }

    private void SpawnBoss()
    {
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            MonsterAI bossAI = boss.GetComponent<MonsterAI>();
            isBossSpawned = true;

            // Kiểm tra waves completed và cho phép tấn công
            if (bossAI != null && wavesCompleted >= 1)
            {
                bossAI.AllowPlayerToAttack();
                Debug.Log($"Boss spawned after {wavesCompleted} waves - Player can now attack!");
            }
        }
    }

    public void OnEnemyKilled()
    {
        enemiesKilled++;
    }

    // Thêm getter để MonsterAI có thể kiểm tra waves
    public int GetWavesCompleted()
    {
        return wavesCompleted;
    }
}