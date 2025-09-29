using UnityEngine;
using System.Collections.Generic;

namespace TowerFusion
{
    /// <summary>
    /// ScriptableObject for defining map data
    /// </summary>
    [CreateAssetMenu(fileName = "New Map", menuName = "Tower Fusion/Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("Map Information")]
        public string mapName;
        public string description;
        public Sprite mapPreview;
        
        [Header("Path Configuration")]
        public List<Vector3> pathPoints = new List<Vector3>();
        public float pathWidth = 1f;
        
        [Header("Tower Placement")]
        public List<Vector3> towerPositions = new List<Vector3>();
        
        [Header("Wave Configuration")]
        public List<WaveData> waves = new List<WaveData>();
        
        [Header("Map Settings")]
        public int maxTowers = 10;
        public Vector3 enemySpawnPoint;
        public Vector3 enemyEndPoint;
        
        /// <summary>
        /// Get the next path point from current position
        /// </summary>
        public Vector3 GetNextPathPoint(int currentIndex)
        {
            if (currentIndex < 0 || currentIndex >= pathPoints.Count - 1)
                return enemyEndPoint;
            
            return pathPoints[currentIndex + 1];
        }
        
        /// <summary>
        /// Check if a position is valid for tower placement
        /// </summary>
        public bool IsValidTowerPosition(Vector3 position, float tolerance = 0.5f)
        {
            // If no specific tower positions are configured, allow placement anywhere
            if (towerPositions == null || towerPositions.Count == 0)
                return true;

            foreach (Vector3 towerPos in towerPositions)
            {
                if (Vector3.Distance(position, towerPos) <= tolerance)
                    return true;
            }

            return false;
        }
        
        /// <summary>
        /// Get the closest valid tower position
        /// </summary>
        public Vector3 GetClosestTowerPosition(Vector3 position)
        {
            if (towerPositions.Count == 0)
                return position;
            
            Vector3 closest = towerPositions[0];
            float closestDistance = Vector3.Distance(position, closest);
            
            foreach (Vector3 towerPos in towerPositions)
            {
                float distance = Vector3.Distance(position, towerPos);
                if (distance < closestDistance)
                {
                    closest = towerPos;
                    closestDistance = distance;
                }
            }
            
            return closest;
        }
    }
}