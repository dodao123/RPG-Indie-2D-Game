using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;  // Danh sách các enemy prefabs
    [SerializeField] private Transform[] spawnPoints;  // Các điểm spawn ngẫu nhiên
    [SerializeField] private int initialEnemyCount = 6;  // Số lượng enemy ban đầu
    [SerializeField] private int killThreshold = 4;  // Số lượng cần giết để spawn thêm enemy
    [SerializeField] private int maxEnemyCount = 12;  // Số lượng tối đa enemy mỗi lần spawn

    private int enemiesKilled = 0;  // Số lượng enemy đã bị giết
    private List<GameObject> enemies = new List<GameObject>();  // Danh sách các enemy đã spawn

    private bool isSpawning = false;  // Kiểm tra trạng thái spawn

    private void Start()
    {
        SpawnEnemies(initialEnemyCount);  // Spawn 6 enemy khi game bắt đầu
    }

    private void Update()
    {
        // Kiểm tra nếu tất cả kẻ thù đã bị giết trong đợt spawn hiện tại
        if (!isSpawning && AreAllEnemiesKilled())
        {
            isSpawning = true;  // Bắt đầu spawn đợt tiếp theo
            StartCoroutine(SpawnNextWave());
        }
    }

    private void SpawnEnemies(int count)
    {
        int spawnCount = Mathf.Min(count, maxEnemyCount);  // Giới hạn số lượng spawn

        for (int i = 0; i < spawnCount; i++)
        {
            // Chọn điểm spawn ngẫu nhiên
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Vector2 spawnPosition = spawnPoints[randomIndex].position;

            // Chọn enemyPrefab ngẫu nhiên từ danh sách
            int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject enemyPrefab = enemyPrefabs[randomEnemyIndex];

            // Spawn enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemies.Add(enemy);
        }
    }

    private bool AreAllEnemiesKilled()
    {
        // Kiểm tra tất cả các kẻ thù trong danh sách đã bị tiêu diệt hay chưa
        foreach (var enemy in enemies)
        {
            if (enemy != null)  // Nếu enemy chưa bị giết
            {
                return false;
            }
        }
        return true;  // Tất cả các kẻ thù đều đã bị giết
    }

    private IEnumerator SpawnNextWave()
    {
        // Đợi cho tới khi tất cả kẻ thù bị giết xong
        yield return new WaitForSeconds(2f);  // Thời gian nghỉ giữa các đợt spawn

        // Reset số lượng kẻ thù đã giết và spawn thêm kẻ thù mới
        enemiesKilled = 0;
        int additionalEnemies = Random.Range(7, 13);  // Spawn thêm từ 7 đến 12 enemy
        SpawnEnemies(additionalEnemies);  // Spawn thêm enemy

        // Tăng số lượng kẻ thù cần giết để spawn thêm
        killThreshold += 4;

        isSpawning = false;  // Cho phép spawn đợt tiếp theo khi cần
    }

    public void OnEnemyKilled()
    {
        enemiesKilled++;  // Tăng số lượng enemy đã bị giết

        if (enemiesKilled >= killThreshold)
        {
            // Kiểm tra nếu tất cả kẻ thù trong đợt spawn này đã bị giết, sau đó spawn thêm đợt mới
            if (AreAllEnemiesKilled())
            {
                StartCoroutine(SpawnNextWave());  // Tiến hành spawn đợt tiếp theo
            }
        }
    }
}
