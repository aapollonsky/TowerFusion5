using UnityEngine;
using System.Collections.Generic;

namespace TowerFusion
{
    /// <summary>
    /// Manages all active enemies in the scene
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        [Header("Enemy Configuration")]
        [SerializeField] private int maxEnemies = 100;
        [Header("Enemy Prefab")]
        [SerializeField] private GameObject enemyPrefab;

        // Singleton instance
        public static EnemyManager Instance { get; private set; }

        // Active enemies
        private List<Enemy> activeEnemies = new List<Enemy>();

        // Events
        public System.Action<Enemy> OnEnemyRegistered;
        public System.Action<Enemy> OnEnemyDestroyed;
        public System.Action OnAllEnemiesDefeated;

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
        /// Spawn an enemy at a position with given EnemyData
        /// </summary>
        public Enemy SpawnEnemy(Vector3 position, EnemyData data)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("EnemyManager: enemyPrefab is not assigned!");
                return null;
            }
            GameObject obj = Instantiate(enemyPrefab, position, Quaternion.identity);
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogError("EnemyManager: Spawned prefab does not have Enemy component!");
                Destroy(obj);
                return null;
            }
            enemy.Initialize(data);
            RegisterEnemy(enemy);
            return enemy;
        }
        

        /// <summary>
        /// Register a new enemy
        /// </summary>
        public void RegisterEnemy(Enemy enemy)
        {
            if (enemy == null || activeEnemies.Contains(enemy))
                return;
            
            if (activeEnemies.Count >= maxEnemies)
            {
                Debug.LogWarning("Maximum enemy limit reached!");
                return;
            }
            
            activeEnemies.Add(enemy);
            
            // Subscribe to enemy events
            enemy.OnEnemyKilled += OnEnemyKilled;
            enemy.OnEnemyReachedEnd += OnEnemyReachedEnd;
            
            OnEnemyRegistered?.Invoke(enemy);
            
            Debug.Log($"Enemy registered: {enemy.EnemyData?.enemyName} (Total: {activeEnemies.Count})");
        }
        
        /// <summary>
        /// Called when an enemy is killed
        /// </summary>
        public void OnEnemyKilled(Enemy enemy)
        {
            UnregisterEnemy(enemy);
        }
        
        /// <summary>
        /// Called when an enemy reaches the end
        /// </summary>
        public void OnEnemyReachedEnd(Enemy enemy)
        {
            UnregisterEnemy(enemy);
        }
        
        /// <summary>
        /// Unregister an enemy
        /// </summary>
        private void UnregisterEnemy(Enemy enemy)
        {
            if (enemy == null || !activeEnemies.Contains(enemy))
                return;
            
            activeEnemies.Remove(enemy);
            
            // Unsubscribe from enemy events
            enemy.OnEnemyKilled -= OnEnemyKilled;
            enemy.OnEnemyReachedEnd -= OnEnemyReachedEnd;
            
            OnEnemyDestroyed?.Invoke(enemy);
            
            Debug.Log($"Enemy unregistered: {enemy.EnemyData?.enemyName} (Remaining: {activeEnemies.Count})");
            
            // Check if all enemies are defeated
            if (activeEnemies.Count == 0)
            {
                OnAllEnemiesDefeated?.Invoke();
            }
        }
        
        /// <summary>
        /// Get the number of active enemies
        /// </summary>
        public int GetActiveEnemyCount()
        {
            return activeEnemies.Count;
        }
        
        /// <summary>
        /// Get all active enemies
        /// </summary>
        public List<Enemy> GetActiveEnemies()
        {
            return new List<Enemy>(activeEnemies);
        }
        
        /// <summary>
        /// Find the closest enemy to a position
        /// </summary>
        public Enemy FindClosestEnemy(Vector3 position, float maxRange = float.MaxValue)
        {
            Enemy closest = null;
            float closestDistance = maxRange;
            
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;
                
                float distance = Vector3.Distance(position, enemy.Position);
                if (distance < closestDistance)
                {
                    closest = enemy;
                    closestDistance = distance;
                }
            }
            
            return closest;
        }
        
        /// <summary>
        /// Find all enemies within range of a position
        /// </summary>
        public List<Enemy> FindEnemiesInRange(Vector3 position, float range)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;
                
                float distance = Vector3.Distance(position, enemy.Position);
                if (distance <= range)
                {
                    enemiesInRange.Add(enemy);
                }
            }
            
            return enemiesInRange;
        }
        
        /// <summary>
        /// Clear all active enemies
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            
            activeEnemies.Clear();
            Debug.Log("All enemies cleared");
        }
        
        /// <summary>
        /// Get enemies of a specific type
        /// </summary>
        public List<Enemy> GetEnemiesOfType(EnemyData enemyType)
        {
            List<Enemy> matchingEnemies = new List<Enemy>();
            
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy != null && enemy.EnemyData == enemyType)
                {
                    matchingEnemies.Add(enemy);
                }
            }
            
            return matchingEnemies;
        }
        
        /// <summary>
        /// Get enemy statistics
        /// </summary>
        public EnemyStats GetEnemyStats()
        {
            EnemyStats stats = new EnemyStats();
            stats.totalEnemies = activeEnemies.Count;
            
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;
                
                stats.totalHealth += enemy.CurrentHealth;
                
                if (enemy.EnemyData.canFly)
                    stats.flyingEnemies++;
                
                if (enemy.EnemyData.isArmored)
                    stats.armoredEnemies++;
            }
            
            return stats;
        }
    }
    
    [System.Serializable]
    public struct EnemyStats
    {
        public int totalEnemies;
        public float totalHealth;
        public int flyingEnemies;
        public int armoredEnemies;
    }
}