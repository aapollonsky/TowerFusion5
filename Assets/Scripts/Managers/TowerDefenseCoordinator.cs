using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TowerFusion
{
    /// <summary>
    /// Coordinates reactive defense: when a tower fires, assigns up to 2 corn-less enemies to counterattack it
    /// </summary>
    public class TowerDefenseCoordinator : MonoBehaviour
    {
        [Header("Counterattack Configuration")]
        [SerializeField] [Tooltip("Maximum enemies that can be assigned to counterattack each tower per wave")]
        private int maxCounterattackersPerTower = 2;
        
        [SerializeField] [Tooltip("Maximum number of towers that can be under counterattack per wave (0 = unlimited)")]
        private int maxTowersUnderAttackPerWave = 2;
        
        // Singleton instance
        public static TowerDefenseCoordinator Instance { get; private set; }
        
        // Track which enemies are assigned to which towers
        private Dictionary<Tower, List<Enemy>> towerAssignments = new Dictionary<Tower, List<Enemy>>();
        
        // Track total number of enemies ever assigned to each tower (prevents refilling when enemies die)
        private Dictionary<Tower, int> totalAssignedCount = new Dictionary<Tower, int>();
        
        // Track which towers have been assigned enemies this wave
        private HashSet<Tower> towersUnderAttackThisWave = new HashSet<Tower>();
        
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
        
        private void Start()
        {
            // Subscribe to tower events
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.OnTowerRegistered += OnTowerRegistered;
            }
            
            // Subscribe to wave events
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted += OnWaveStarted;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.OnTowerRegistered -= OnTowerRegistered;
            }
            
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted -= OnWaveStarted;
            }
        }
        
        /// <summary>
        /// Called when a new tower is registered
        /// </summary>
        private void OnTowerRegistered(Tower tower)
        {
            // Subscribe to tower's fire event
            tower.OnTowerFired += OnTowerFired;
            tower.OnTowerDestroyed += OnTowerDestroyed;
            
            // Initialize empty assignment list and counters
            if (!towerAssignments.ContainsKey(tower))
            {
                towerAssignments[tower] = new List<Enemy>();
                totalAssignedCount[tower] = 0;
            }
            
            Debug.Log($"TowerDefenseCoordinator: Registered tower {tower.TowerData.towerName}");
        }
        
        /// <summary>
        /// Called when a tower is destroyed
        /// </summary>
        private void OnTowerDestroyed(Tower tower)
        {
            // Unsubscribe from events
            tower.OnTowerFired -= OnTowerFired;
            tower.OnTowerDestroyed -= OnTowerDestroyed;
            
            // Remove assignments and counters
            if (towerAssignments.ContainsKey(tower))
            {
                towerAssignments.Remove(tower);
            }
            if (totalAssignedCount.ContainsKey(tower))
            {
                totalAssignedCount.Remove(tower);
            }
            
            Debug.Log($"TowerDefenseCoordinator: Unregistered tower {tower.TowerData.towerName}");
        }
        
        /// <summary>
        /// Called when a new wave starts - reset all assignment counters
        /// </summary>
        private void OnWaveStarted(int waveNumber)
        {
            Debug.Log($"TowerDefenseCoordinator: New wave {waveNumber} started - resetting all tower assignment counters");
            
            // Clear all assignments
            foreach (var tower in towerAssignments.Keys)
            {
                towerAssignments[tower].Clear();
            }
            
            // Reset all assignment counters
            foreach (var tower in totalAssignedCount.Keys.ToList())
            {
                totalAssignedCount[tower] = 0;
            }
            
            // Reset towers under attack tracking
            towersUnderAttackThisWave.Clear();
            
            Debug.Log($"TowerDefenseCoordinator: All towers can now receive up to {maxCounterattackersPerTower} counterattackers. Max {maxTowersUnderAttackPerWave} towers can be attacked this wave.");
        }
        
        /// <summary>
        /// Called when a tower fires at an enemy (reactive defense trigger)
        /// </summary>
        private void OnTowerFired(Tower tower, Enemy targetEnemy)
        {
            Debug.Log($"TowerDefenseCoordinator: {tower.TowerData.towerName} fired at {targetEnemy.EnemyData.enemyName}!");
            
            // Get current assignments for this tower
            if (!towerAssignments.ContainsKey(tower))
            {
                towerAssignments[tower] = new List<Enemy>();
                totalAssignedCount[tower] = 0;
            }
            
            // Check if this tower has already been assigned enemies this wave
            bool towerAlreadyUnderAttack = towersUnderAttackThisWave.Contains(tower);
            
            // Check if we've reached the max towers under attack limit (0 = unlimited)
            if (!towerAlreadyUnderAttack && maxTowersUnderAttackPerWave > 0 && towersUnderAttackThisWave.Count >= maxTowersUnderAttackPerWave)
            {
                Debug.Log($"TowerDefenseCoordinator: Max towers under attack ({maxTowersUnderAttackPerWave}) already reached. Cannot assign enemies to {tower.TowerData.towerName}.");
                return;
            }
            
            // Check total assigned count (doesn't decrease when enemies die)
            int totalAssigned = totalAssignedCount[tower];
            
            if (totalAssigned >= maxCounterattackersPerTower)
            {
                Debug.Log($"TowerDefenseCoordinator: {tower.TowerData.towerName} has already had {totalAssigned} enemies assigned (max {maxCounterattackersPerTower}). No replacements.");
                return;
            }
            
            // Calculate how many more enemies we can assign based on lifetime total
            int slotsAvailable = maxCounterattackersPerTower - totalAssigned;
            
            // Find eligible enemies to counterattack
            List<Enemy> eligible = FindEligibleCounterattackers(tower);
            
            if (eligible.Count == 0)
            {
                Debug.Log($"TowerDefenseCoordinator: No eligible enemies available to counterattack {tower.TowerData.towerName}");
                return;
            }
            
            List<Enemy> currentAssignments = towerAssignments[tower];
            
            // Clean up dead/invalid enemies from current assignments list (for tracking purposes)
            currentAssignments.RemoveAll(e => e == null || !e.IsAlive);
            
            // Assign up to 'slotsAvailable' enemies
            int assigned = 0;
            foreach (Enemy enemy in eligible)
            {
                if (assigned >= slotsAvailable)
                    break;
                
                // Assign enemy to counterattack
                enemy.AssignCounterattack(tower);
                currentAssignments.Add(enemy);
                totalAssignedCount[tower]++; // Increment lifetime total
                assigned++;
                
                // Mark this tower as under attack this wave
                towersUnderAttackThisWave.Add(tower);
                
                Debug.Log($"TowerDefenseCoordinator: Assigned {enemy.EnemyData.enemyName} to counterattack {tower.TowerData.towerName} (total assigned: {totalAssignedCount[tower]}/{maxCounterattackersPerTower}, towers under attack: {towersUnderAttackThisWave.Count}/{maxTowersUnderAttackPerWave})");
            }
        }
        
        /// <summary>
        /// Find eligible enemies that can counterattack this tower
        /// </summary>
        private List<Enemy> FindEligibleCounterattackers(Tower tower)
        {
            // Get all active enemies
            List<Enemy> allEnemies = EnemyManager.Instance?.GetActiveEnemies();
            if (allEnemies == null || allEnemies.Count == 0)
            {
                return new List<Enemy>();
            }
            
            // Filter eligible enemies
            List<Enemy> eligible = new List<Enemy>();
            
            foreach (Enemy enemy in allEnemies)
            {
                // Must be alive
                if (!enemy.IsAlive)
                    continue;
                
                // Must NOT have corn
                if (enemy.HasCorn)
                    continue;
                
                // Must NOT be already counterattacking THIS SPECIFIC tower
                // (Enemies can only attack one tower at a time, but different enemies can attack different towers)
                if (IsEnemyAssignedToTower(enemy, tower))
                    continue;
                
                // Must NOT be already counterattacking a DIFFERENT tower
                // (Each enemy can only counterattack one tower at a time)
                if (IsEnemyAssignedToAnyTower(enemy))
                    continue;
                
                // This enemy is eligible
                eligible.Add(enemy);
            }
            
            // Sort by distance to tower (closest first = higher priority)
            eligible = eligible.OrderBy(e => Vector3.Distance(e.Position, tower.Position)).ToList();
            
            Debug.Log($"TowerDefenseCoordinator: Found {eligible.Count} eligible enemies for {tower.TowerData.towerName}");
            
            return eligible;
        }
        
        /// <summary>
        /// Check if an enemy is already assigned to a specific tower
        /// </summary>
        private bool IsEnemyAssignedToTower(Enemy enemy, Tower tower)
        {
            if (!towerAssignments.ContainsKey(tower))
                return false;
            
            return towerAssignments[tower].Contains(enemy);
        }
        
        /// <summary>
        /// Check if an enemy is already assigned to counterattack any tower
        /// </summary>
        private bool IsEnemyAssignedToAnyTower(Enemy enemy)
        {
            foreach (var assignments in towerAssignments.Values)
            {
                if (assignments.Contains(enemy))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Unassign an enemy from any tower (called when enemy dies or changes state)
        /// </summary>
        public void UnassignEnemy(Enemy enemy)
        {
            foreach (var kvp in towerAssignments)
            {
                Tower tower = kvp.Key;
                List<Enemy> assignments = kvp.Value;
                
                if (assignments.Remove(enemy))
                {
                    Debug.Log($"TowerDefenseCoordinator: Unassigned {enemy.EnemyData.enemyName} from {tower.TowerData.towerName}");
                    
                    // Clean up dead enemies from the list
                    assignments.RemoveAll(e => e == null || !e.IsAlive);
                    
                    // NOTE: Do NOT reset totalAssignedCount here!
                    // Each tower gets up to 2 enemies assigned PER WAVE, not per engagement.
                    // Counter only resets when a new wave starts.
                }
            }
        }
    }
}
