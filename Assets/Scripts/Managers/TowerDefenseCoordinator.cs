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
        private const int MAX_COUNTERATTACKERS_PER_TOWER = 2;
        
        // Singleton instance
        public static TowerDefenseCoordinator Instance { get; private set; }
        
        // Track which enemies are assigned to which towers
        private Dictionary<Tower, List<Enemy>> towerAssignments = new Dictionary<Tower, List<Enemy>>();
        
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
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.OnTowerRegistered -= OnTowerRegistered;
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
            
            // Initialize empty assignment list
            if (!towerAssignments.ContainsKey(tower))
            {
                towerAssignments[tower] = new List<Enemy>();
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
            
            // Remove assignments
            if (towerAssignments.ContainsKey(tower))
            {
                towerAssignments.Remove(tower);
            }
            
            Debug.Log($"TowerDefenseCoordinator: Unregistered tower {tower.TowerData.towerName}");
        }
        
        /// <summary>
        /// Called when a tower fires at an enemy (reactive defense trigger)
        /// </summary>
        private void OnTowerFired(Tower tower, Enemy targetEnemy)
        {
            Debug.Log($"TowerDefenseCoordinator: {tower.TowerData.towerName} fired at {targetEnemy.EnemyData.enemyName}!");
            
            // Find eligible enemies to counterattack
            List<Enemy> eligible = FindEligibleCounterattackers(tower);
            
            // Get current assignments for this tower
            if (!towerAssignments.ContainsKey(tower))
            {
                towerAssignments[tower] = new List<Enemy>();
            }
            
            List<Enemy> currentAssignments = towerAssignments[tower];
            
            // Clean up dead/invalid enemies from assignments
            currentAssignments.RemoveAll(e => e == null || !e.IsAlive);
            
            // Calculate how many more enemies we can assign
            int slotsAvailable = MAX_COUNTERATTACKERS_PER_TOWER - currentAssignments.Count;
            
            if (slotsAvailable <= 0)
            {
                Debug.Log($"TowerDefenseCoordinator: {tower.TowerData.towerName} already has max counterattackers ({MAX_COUNTERATTACKERS_PER_TOWER})");
                return;
            }
            
            // Assign up to 'slotsAvailable' enemies
            int assigned = 0;
            foreach (Enemy enemy in eligible)
            {
                if (assigned >= slotsAvailable)
                    break;
                
                // Assign enemy to counterattack
                enemy.AssignCounterattack(tower);
                currentAssignments.Add(enemy);
                assigned++;
                
                Debug.Log($"TowerDefenseCoordinator: Assigned {enemy.EnemyData.enemyName} to counterattack {tower.TowerData.towerName} ({currentAssignments.Count}/{MAX_COUNTERATTACKERS_PER_TOWER})");
            }
            
            if (assigned == 0)
            {
                Debug.Log($"TowerDefenseCoordinator: No eligible enemies available to counterattack {tower.TowerData.towerName}");
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
                if (kvp.Value.Remove(enemy))
                {
                    Debug.Log($"TowerDefenseCoordinator: Unassigned {enemy.EnemyData.enemyName} from {kvp.Key.TowerData.towerName}");
                }
            }
        }
    }
}
