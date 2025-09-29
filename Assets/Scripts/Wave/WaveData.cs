using UnityEngine;
using System.Collections.Generic;

namespace TowerFusion
{
    /// <summary>
    /// ScriptableObject for defining wave data
    /// </summary>
    [CreateAssetMenu(fileName = "New Wave", menuName = "Tower Fusion/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Header("Wave Information")]
        public string waveName;
        public int waveNumber;
        
        [Header("Enemy Groups")]
        public List<EnemyGroup> enemyGroups = new List<EnemyGroup>();
        
        [Header("Timing")]
        public float timeBetweenGroups = 2f;
        public float timeBetweenEnemies = 0.5f;
        
        /// <summary>
        /// Get total number of enemies in this wave
        /// </summary>
        public int GetTotalEnemyCount()
        {
            int total = 0;
            foreach (var group in enemyGroups)
            {
                total += group.count;
            }
            return total;
        }
    }
    
    [System.Serializable]
    public class EnemyGroup
    {
        public EnemyData enemyType;
        public int count;
        public float delayBeforeGroup = 0f;
    }
}