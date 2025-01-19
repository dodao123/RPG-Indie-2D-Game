using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Manages enemy wave spawning, soul collection mechanics, and wave progression
public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform[] bossSpawnPoints;
    [SerializeField] private int maxWaves = 4;
    [SerializeField] private float delayBetweenWaves = 4f;

    [Header("Soul Collection Settings")]
    [SerializeField] private float soulInteractRadius = 2f;
    [SerializeField] private ParticleSystem soulParticlePrefab;
    [SerializeField] private float particleDuration = 3f;
    [SerializeField] private KeyCode soulCollectionKey = KeyCode.R;

    // Wave tracking
    private int currentWave;
    private bool isWaveInProgress;
    private List<GameObject> activeEnemies = new List<GameObject>();

    // Soul collection state
    private int playerSoulPoints;
    private bool isSoulInteractable;
    private Transform activeSoulPoint;
    private ParticleSystem activeSoulParticle;

    // Danh sách lưu trữ các con quái bị giết
    private List<GameObject> deadEnemies = new List<GameObject>();

    private void Start()
    {
        InitializeAndStartFirstWave();
    }

    private void Update()
    {
        CheckSoulCollection();
        Debug.Log(activeEnemies.Count);
    }

    /// Initializes game state and starts the first wave
    private void InitializeAndStartFirstWave()
    {
        currentWave = 0;
        playerSoulPoints = 0;
        StartNextWave();
    }

    /// Handles soul collection interaction check
    private void CheckSoulCollection()
    {
        if (!isSoulInteractable || activeSoulPoint == null) return;

        float distanceToPlayer = Vector2.Distance(activeSoulPoint.position, GetPlayerPosition());
        if (distanceToPlayer <= soulInteractRadius && Input.GetKeyDown(soulCollectionKey))
        {
            CollectSoul();
        }
    }

    /// Initiates the next wave of enemies
    private void StartNextWave()
    {
        currentWave++;
        isWaveInProgress = true;
        isSoulInteractable = false;
        activeEnemies.Clear();

        if (currentWave > maxWaves)
        {
            OnGameComplete();
            return;
        }

        SpawnEnemiesForCurrentWave();
    }

    /// Spawns appropriate enemies based on current wave
    private void SpawnEnemiesForCurrentWave()
    {
        if (currentWave == maxWaves)
        {
            SpawnBoss(true);  // Final wave - boss only
            return;
        }

        SpawnRegularEnemies();

        if (currentWave == maxWaves - 1)  // Penultimate wave
        {
            SpawnBoss(false);  // Boss with regular enemies
        }
    }

    /// Spawns regular enemies for the current wave
    private void SpawnRegularEnemies()
    {
        int enemyCount = currentWave == 1 ? 6 : 8;  // First wave: 6 enemies, others: 8 enemies

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemy = Instantiate(
                enemyPrefabs[Random.Range(0, enemyPrefabs.Length)],
                spawnPoints[Random.Range(0, spawnPoints.Length)].position,
                Quaternion.identity
            );
            activeEnemies.Add(enemy);
        }
    }

    /// Spawns a boss enemy
    /// Spawns a boss enemy
    private void SpawnBoss(bool isFinalWave)
    {
        if (isFinalWave)
        {
            // Spawn 4 bosses at the 4 spawn points
            for (int i = 0; i < bossSpawnPoints.Length; i++)
            {
                Vector3 spawnPosition = bossSpawnPoints[i].position;
                GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
                activeEnemies.Add(boss);
            }
        }
        else
        {
            // If not final wave, spawn a single boss
            Vector3 spawnPosition = bossSpawnPoints[Random.Range(0, bossSpawnPoints.Length)].position;
            GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
            activeEnemies.Add(boss);
        }
    }


    /// Handles enemy death event
    public void OnEnemyKilled()
    {
        // Thêm các con quái bị giết vào danh sách deadEnemies
        deadEnemies.Clear();
        foreach (var enemy in activeEnemies)
        {
            if (enemy == null)
            {
                deadEnemies.Add(enemy);
            }
        }

        // Chỉ xóa các con quái đã chết khỏi danh sách activeEnemies sau khi hoàn thành kiểm tra
        CleanupDeadEnemies();

        // Kiểm tra số lượng quái vật còn lại
        if (activeEnemies.Count == 3)  // Tạo điểm linh hồn khi chỉ còn một quái vật sống sót
        {
            CreateSoulInteractionPoint();
        }
    }

    /// Removes destroyed enemies from tracking list
    private void CleanupDeadEnemies()
    {
        // Xóa các con quái bị giết khỏi activeEnemies
        foreach (var deadEnemy in deadEnemies)
        {
            activeEnemies.Remove(deadEnemy);
        }
        // Giải phóng danh sách deadEnemies sau khi đã làm sạch
        deadEnemies.Clear();
    }

    /// Creates a soul interaction point with particles
    private void CreateSoulInteractionPoint()
    {
        activeSoulPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (soulParticlePrefab != null && activeSoulParticle == null)
        {
            activeSoulParticle = Instantiate(soulParticlePrefab, activeSoulPoint.position, Quaternion.identity);
        }

        isSoulInteractable = true;
    }

    /// Handles soul collection by the player
    private void CollectSoul()
    {
        playerSoulPoints++;
        HandleSoulCollectionEffects();
        StartCoroutine(StartNextWaveWithDelay());
    }

    /// Manages particle effects for soul collection
    private void HandleSoulCollectionEffects()
    {
        if (activeSoulParticle != null)
        {
            ParticleSystem collectEffect = Instantiate(soulParticlePrefab, activeSoulPoint.position, Quaternion.identity);
            collectEffect.Play();
            Destroy(collectEffect.gameObject, particleDuration);
            Destroy(activeSoulParticle.gameObject);
        }

        isSoulInteractable = false;
        activeSoulPoint = null;
    }

    private IEnumerator StartNextWaveWithDelay()
    {
        yield return new WaitForSeconds(delayBetweenWaves);
        StartNextWave();
    }

    /// Gets the current player position
    private Vector2 GetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? (Vector2)player.transform.position : Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        if (activeSoulPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(activeSoulPoint.position, soulInteractRadius);
        }
    }

    private void OnGameComplete()
    {
        Debug.Log("All waves completed!");
        isWaveInProgress = false;
    }

    // Public accessors
    public int GetPlayerSoulPoints() => playerSoulPoints;
    public int GetCurrentWave() => currentWave;
    public bool IsWaveInProgress() => isWaveInProgress;
}
