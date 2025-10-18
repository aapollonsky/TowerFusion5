using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TowerFusion
{
    /// <summary>
    /// Manages target distribution across enemies to prevent all enemies from attacking the same tower
    /// </summary>
    public class EnemyTargetDistributor : MonoBehaviour
    {
        // Singleton instance
        public static EnemyTargetDistributor Instance { get; private set; }
        
        // Maximum number of towers that can be under attack simultaneously
        [SerializeField] private int maxSimultaneousTowers = 3;
        
        // Track which enemies are targeting which towers
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
        
        /// <summary>
        /// Select best tower for an enemy to target, distributing load across towers
        /// Only applies to Attacker enemies - Stealer enemies ignore this system
        /// </summary>
        public Tower SelectTargetForEnemy(Enemy enemy, Vector3 enemyPosition, float detectionRange)
        {
            // Skip distribution for Stealer enemies (they go straight to corn)
            if (enemy != null && enemy.Role == EnemyRole.Stealer)
            {
                return null;
            }
            
            if (TowerManager.Instance == null)
                return null;
            
            var towers = TowerManager.Instance.GetActiveTowers();
            if (towers == null || towers.Count == 0)
                return null;
            
            // Find towers within detection range
            List<Tower> availableTowers = new List<Tower>();
            foreach (var tower in towers)
            {
                if (tower == null || !tower.IsAlive)
                    continue;
                
                float distance = Vector3.Distance(enemyPosition, tower.Position);
                if (distance <= detectionRange)
                {
                    availableTowers.Add(tower);
                }
            }
            
            if (availableTowers.Count == 0)
                return null;
            
            // Clean up assignments for destroyed towers and dead enemies
            CleanupAssignments();
            
            // Get towers that are already under attack
            var towersUnderAttack = towerAssignments.Keys.Where(t => t != null && t.IsAlive).ToList();
            
            // If we've reached the maximum number of towers under attack,
            // only consider those towers (don't spread to new towers)
            if (towersUnderAttack.Count >= maxSimultaneousTowers)
            {
                // Filter available towers to only those already under attack
                availableTowers = availableTowers.Where(t => towersUnderAttack.Contains(t)).ToList();
                
                if (availableTowers.Count == 0)
                {
                    // All towers under attack are out of range, assign to closest attacked tower anyway
                    return towersUnderAttack.OrderBy(t => Vector3.Distance(enemyPosition, t.Position)).FirstOrDefault();
                }
            }
            
            // If only one tower available, assign to it
            if (availableTowers.Count == 1)
            {
                return availableTowers[0];
            }
            
            // Find tower with least enemies assigned
            Tower bestTower = null;
            int minAssignments = int.MaxValue;
            float closestDistance = float.MaxValue;
            
            foreach (var tower in availableTowers)
            {
                int assignmentCount = GetAssignmentCount(tower);
                float distance = Vector3.Distance(enemyPosition, tower.Position);
                
                // Prefer towers with fewer assignments
                // If tied, prefer closer tower
                if (assignmentCount < minAssignments || 
                    (assignmentCount == minAssignments && distance < closestDistance))
                {
                    bestTower = tower;
                    minAssignments = assignmentCount;
                    closestDistance = distance;
                }
            }
            
            return bestTower;
        }
        
        /// <summary>
        /// Register an enemy as targeting a specific tower
        /// </summary>
        public void AssignEnemyToTower(Enemy enemy, Tower tower)
        {
            if (enemy == null || tower == null)
                return;
            
            // Remove enemy from any previous assignments
            UnassignEnemy(enemy);
            
            // Add to new tower's assignment list
            if (!towerAssignments.ContainsKey(tower))
            {
                towerAssignments[tower] = new List<Enemy>();
            }
            
            if (!towerAssignments[tower].Contains(enemy))
            {
                towerAssignments[tower].Add(enemy);
                Debug.Log($"Assigned {enemy.name} to {tower.TowerData.towerName} (now {towerAssignments[tower].Count} enemies targeting it)");
            }
        }
        
        /// <summary>
        /// Unassign an enemy from its current tower
        /// </summary>
        public void UnassignEnemy(Enemy enemy)
        {
            if (enemy == null)
                return;
            
            // Find and remove from all assignments
            var towersToClean = new List<Tower>();
            
            foreach (var kvp in towerAssignments)
            {
                if (kvp.Value.Contains(enemy))
                {
                    kvp.Value.Remove(enemy);
                    
                    // Mark empty lists for removal
                    if (kvp.Value.Count == 0)
                    {
                        towersToClean.Add(kvp.Key);
                    }
                }
            }
            
            // Clean up empty assignments
            foreach (var tower in towersToClean)
            {
                towerAssignments.Remove(tower);
            }
        }
        
        /// <summary>
        /// Get number of enemies currently assigned to a tower
        /// </summary>
        public int GetAssignmentCount(Tower tower)
        {
            if (tower == null || !towerAssignments.ContainsKey(tower))
                return 0;
            
            return towerAssignments[tower].Count;
        }
        
        /// <summary>
        /// Clean up assignments for destroyed towers and dead enemies
        /// </summary>
        private void CleanupAssignments()
        {
            // Remove destroyed towers
            var towersToRemove = towerAssignments.Keys.Where(t => t == null || !t.IsAlive).ToList();
            foreach (var tower in towersToRemove)
            {
                towerAssignments.Remove(tower);
            }
            
            // Remove dead enemies from all lists
            foreach (var kvp in towerAssignments.ToList())
            {
                kvp.Value.RemoveAll(e => e == null || !e.IsAlive);
                
                // Remove empty lists
                if (kvp.Value.Count == 0)
                {
                    towerAssignments.Remove(kvp.Key);
                }
            }
        }
        
        /// <summary>
        /// Get debug info about current assignments
        /// </summary>
        public string GetDebugInfo()
        {
            CleanupAssignments();
            
            if (towerAssignments.Count == 0)
                return "No tower assignments";
            
            string info = "Tower Assignments:\n";
            foreach (var kvp in towerAssignments)
            {
                if (kvp.Key != null)
                {
                    info += $"  {kvp.Key.TowerData.towerName}: {kvp.Value.Count} enemies\n";
                }
            }
            
            return info;
        }
        
        /// <summary>
        /// Clear all assignments (useful for wave reset)
        /// </summary>
        public void ClearAllAssignments()
        {
            towerAssignments.Clear();
            Debug.Log("Cleared all enemy-tower assignments");
        }
    }
}
