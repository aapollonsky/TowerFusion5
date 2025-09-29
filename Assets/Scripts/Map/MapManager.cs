using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// Manages the current map and handles map-related functionality
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        [Header("Map Configuration")]
        [SerializeField] private MapData currentMap;
        [SerializeField] private LineRenderer pathRenderer;
        [SerializeField] private Transform towerPositionsParent;
        [SerializeField] private GameObject towerPositionMarkerPrefab;
        
        // Singleton instance
        public static MapManager Instance { get; private set; }
        
        // Properties
        public MapData CurrentMap => currentMap;
        
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
            InitializeMap();
        }
        
        /// <summary>
        /// Initialize the current map
        /// </summary>
        public void InitializeMap()
        {
            if (currentMap == null)
            {
                Debug.LogError("No map data assigned to MapManager!");
                return;
            }
            
            SetupPath();
            SetupTowerPositions();
            
            Debug.Log($"Map '{currentMap.mapName}' initialized successfully.");
        }
        
        /// <summary>
        /// Load a new map
        /// </summary>
        public void LoadMap(MapData newMap)
        {
            if (newMap == null)
            {
                Debug.LogError("Cannot load null map data!");
                return;
            }
            
            currentMap = newMap;
            InitializeMap();
        }
        
        /// <summary>
        /// Setup the visual path using LineRenderer
        /// </summary>
        private void SetupPath()
        {
            if (pathRenderer == null || currentMap.pathPoints.Count < 2)
                return;
            
            pathRenderer.positionCount = currentMap.pathPoints.Count;
            pathRenderer.SetPositions(currentMap.pathPoints.ToArray());
            pathRenderer.startWidth = currentMap.pathWidth;
            pathRenderer.endWidth = currentMap.pathWidth;
        }
        
        /// <summary>
        /// Setup tower position markers
        /// </summary>
        private void SetupTowerPositions()
        {
            if (towerPositionsParent == null || towerPositionMarkerPrefab == null)
                return;
            
            // Clear existing markers
            foreach (Transform child in towerPositionsParent)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // Create new markers
            foreach (Vector3 position in currentMap.towerPositions)
            {
                GameObject marker = Instantiate(towerPositionMarkerPrefab, position, Quaternion.identity, towerPositionsParent);
                marker.name = $"TowerPosition_{position.x}_{position.z}";
            }
        }
        
        /// <summary>
        /// Get the enemy spawn point
        /// </summary>
        public Vector3 GetEnemySpawnPoint()
        {
            return currentMap?.enemySpawnPoint ?? Vector3.zero;
        }
        
        /// <summary>
        /// Get the enemy end point
        /// </summary>
        public Vector3 GetEnemyEndPoint()
        {
            return currentMap?.enemyEndPoint ?? Vector3.zero;
        }
        
        /// <summary>
        /// Get path points for enemy movement
        /// </summary>
        public Vector3[] GetPathPoints()
        {
            return currentMap?.pathPoints.ToArray() ?? new Vector3[0];
        }
        
        /// <summary>
        /// Check if position is valid for tower placement
        /// </summary>
        public bool CanPlaceTowerAt(Vector3 position)
        {
            if (currentMap == null)
                return false;
            
            return currentMap.IsValidTowerPosition(position);
        }
        
        /// <summary>
        /// Get the closest valid tower position
        /// </summary>
        public Vector3 GetClosestTowerPosition(Vector3 position)
        {
            if (currentMap == null)
                return position;
            
            return currentMap.GetClosestTowerPosition(position);
        }
        
        /// <summary>
        /// Get wave data for the current map
        /// </summary>
        public WaveData GetWaveData(int waveNumber)
        {
            if (currentMap == null || waveNumber <= 0 || waveNumber > currentMap.waves.Count)
                return null;
            
            return currentMap.waves[waveNumber - 1];
        }
        
        /// <summary>
        /// Get total number of waves for current map
        /// </summary>
        public int GetTotalWaves()
        {
            return currentMap?.waves.Count ?? 0;
        }
    }
}