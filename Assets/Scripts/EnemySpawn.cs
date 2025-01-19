using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages enemy wave spawning, soul collection mechanics, and wave progression
/// </summary>
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

    private void Start()
    {
        InitializeAndStartFirstWave();
    }

    private void Update()
    {
        CheckSoulCollection();
    }

    /// <summary>
    /// Initializes game state and starts the first wave
    /// </summary>
    private void InitializeAndStartFirstWave()
    {
        currentWave = 0;
        playerSoulPoints = 0;
        StartNextWave();
    }

    /// <summary>
    /// Handles soul collection interaction check
    /// </summary>
    private void CheckSoulCollection()
    {
        if (!isSoulInteractable || activeSoulPoint == null) return;

        float distanceToPlayer = Vector2.Distance(activeSoulPoint.position, GetPlayerPosition());
        if (distanceToPlayer <= soulInteractRadius && Input.GetKeyDown(soulCollectionKey))
        {
            CollectSoul();
        }
    }

    /// <summary>
    /// Initiates the next wave of enemies
    /// </summary>
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

    /// <summary>
    /// Spawns appropriate enemies based on current wave
    /// </summary>
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

    /// <summary>
    /// Spawns regular enemies for the current wave
    /// </summary>
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

    /// <summary>
    /// Spawns a boss enemy
    /// </summary>
    /// <param name="isFinalWave">Whether this is the final wave (boss only)</param>
    private void SpawnBoss(bool isFinalWave)
    {
        Vector3 spawnPosition = bossSpawnPoints[Random.Range(0, bossSpawnPoints.Length)].position;
        GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(boss);
    }

    /// <summary>
    /// Handles enemy death event
    /// </summary>
    public void OnEnemyKilled()
    {
        CleanupDeadEnemies();

        if (activeEnemies.Count == 1)  // Spawn soul point when one enemy remains
        {
            CreateSoulInteractionPoint();
        }
    }

    /// <summary>
    /// Removes destroyed enemies from tracking list
    /// </summary>
    private void CleanupDeadEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    /// <summary>
    /// Creates a soul interaction point with particles
    /// </summary>
    private void CreateSoulInteractionPoint()
    {
        activeSoulPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (soulParticlePrefab != null && activeSoulParticle == null)
        {
            activeSoulParticle = Instantiate(soulParticlePrefab, activeSoulPoint.position, Quaternion.identity);
        }

        isSoulInteractable = true;
    }

    /// <summary>
    /// Handles soul collection by the player
    /// </summary>
    private void CollectSoul()
    {
        playerSoulPoints++;
        HandleSoulCollectionEffects();
        StartCoroutine(StartNextWaveWithDelay());
    }

    /// <summary>
    /// Manages particle effects for soul collection
    /// </summary>
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

    /// <summary>
    /// Gets the current player position
    /// </summary>
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