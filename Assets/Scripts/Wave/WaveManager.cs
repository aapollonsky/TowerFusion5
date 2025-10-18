using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace TowerFusion
{
    /// <summary>
    /// Manages enemy spawning and wave progression
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
    [Header("Wave Configuration")]
    [SerializeField] private Transform enemyContainer;
    
    [Header("Enemy Role Distribution")]
    [SerializeField] [Range(0f, 1f)] private float stealerPercentage = 0.15f; // 15% stealers, 85% attackers
    [SerializeField] private bool enableCornTheft = true; // Toggle corn theft system
        
        // Singleton instance
        public static WaveManager Instance { get; private set; }
        
        // Wave state
        private bool isWaveActive = false;
        private int currentWaveNumber = 0;
        private Coroutine currentWaveCoroutine;
        
        // Events
        public System.Action<int> OnWaveStarted;
        public System.Action<int> OnWaveCompleted;
        public System.Action<int, int> OnEnemySpawned; // (current, total)
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Start a wave with the given wave number
        /// </summary>
        public void StartWave(int waveNumber)
        {
            if (isWaveActive)
            {
                Debug.LogWarning("Cannot start wave - another wave is already active!");
                return;
            }
            
            WaveData waveData = MapManager.Instance?.GetWaveData(waveNumber);
            if (waveData == null)
            {
                Debug.LogError($"No wave data found for wave {waveNumber}!");
                GameManager.Instance?.EndWave();
                return;
            }
            
            currentWaveNumber = waveNumber;
            isWaveActive = true;
            
            OnWaveStarted?.Invoke(waveNumber);
            
            currentWaveCoroutine = StartCoroutine(SpawnWaveCoroutine(waveData));
            
            Debug.Log($"Starting wave {waveNumber}: {waveData.waveName}");
        }
        
        /// <summary>
        /// Stop the current wave
        /// </summary>
        public void StopWave()
        {
            if (currentWaveCoroutine != null)
            {
                StopCoroutine(currentWaveCoroutine);
                currentWaveCoroutine = null;
            }
            
            isWaveActive = false;
        }
        
        /// <summary>
        /// Coroutine to spawn enemies for a wave
        /// </summary>
        private IEnumerator SpawnWaveCoroutine(WaveData waveData)
        {
            int totalEnemies = waveData.GetTotalEnemyCount();
            int spawnedEnemies = 0;
            
            foreach (var enemyGroup in waveData.enemyGroups)
            {
                // Wait for group delay
                if (enemyGroup.delayBeforeGroup > 0)
                {
                    yield return new WaitForSeconds(enemyGroup.delayBeforeGroup);
                }
                
                // Spawn enemies in this group
                for (int i = 0; i < enemyGroup.count; i++)
                {
                    SpawnEnemy(enemyGroup.enemyType);
                    spawnedEnemies++;
                    
                    OnEnemySpawned?.Invoke(spawnedEnemies, totalEnemies);
                    
                    // Wait between enemy spawns (except for the last enemy)
                    if (i < enemyGroup.count - 1)
                    {
                        yield return new WaitForSeconds(waveData.timeBetweenEnemies);
                    }
                }
                
                // Wait between groups (except for the last group)
                if (enemyGroup != waveData.enemyGroups[waveData.enemyGroups.Count - 1])
                {
                    yield return new WaitForSeconds(waveData.timeBetweenGroups);
                }
            }
            
            // All enemies spawned, now wait for wave to complete
            yield return StartCoroutine(WaitForWaveCompletion());
        }
        
        /// <summary>
        /// Wait for all enemies to be defeated or reach the end
        /// </summary>
        private IEnumerator WaitForWaveCompletion()
        {
            while (EnemyManager.Instance != null && EnemyManager.Instance.GetActiveEnemyCount() > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            // Wave completed
            CompleteWave();
        }
        
        /// <summary>
        /// Complete the current wave
        /// </summary>
        private void CompleteWave()
        {
            isWaveActive = false;
            
            OnWaveCompleted?.Invoke(currentWaveNumber);
            GameManager.Instance?.EndWave();
            
            Debug.Log($"Wave {currentWaveNumber} completed!");
            
            // Check if this was the last wave
            int totalWaves = MapManager.Instance?.GetTotalWaves() ?? 0;
            if (currentWaveNumber >= totalWaves)
            {
                GameManager.Instance?.Victory();
            }
        }
        
        /// <summary>
        /// Spawn a single enemy
        /// </summary>
        private void SpawnEnemy(EnemyData enemyData)
        {
            if (enemyData == null)
            {
                Debug.LogError("Cannot spawn enemy - missing data!");
                return;
            }
            
            Vector3 spawnPosition = MapManager.Instance?.GetEnemySpawnPoint() ?? Vector3.zero;
            Enemy enemy = EnemyManager.Instance?.SpawnEnemy(spawnPosition, enemyData);
            
            if (enemy != null)
            {
                // Assign role based on distribution (85% Attacker, 15% Stealer)
                if (enableCornTheft && CornManager.Instance != null)
                {
                    float roll = Random.value;
                    EnemyRole assignedRole = roll < stealerPercentage ? EnemyRole.Stealer : EnemyRole.Attacker;
                    enemy.SetRole(assignedRole);
                    
                    Debug.Log($"Spawned {enemy.name} as {assignedRole} (roll: {roll:F2}, threshold: {stealerPercentage:F2})");
                }
                else
                {
                    // Corn theft disabled or no CornManager, all enemies are attackers
                    enemy.SetRole(EnemyRole.Attacker);
                }
                
                // Parent to container
                if (enemyContainer != null)
                {
                    enemy.transform.SetParent(enemyContainer);
                }
            }
        }
        
        /// <summary>
        /// Check if a wave is currently active
        /// </summary>
        public bool IsWaveActive()
        {
            return isWaveActive;
        }
        
        /// <summary>
        /// Get current wave number
        /// </summary>
        public int GetCurrentWaveNumber()
        {
            return currentWaveNumber;
        }
    }
}