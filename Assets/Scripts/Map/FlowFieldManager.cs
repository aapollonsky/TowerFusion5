using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// Manages multiple flow fields for different destinations (corn storage and spawn point)
    /// </summary>
    public class FlowFieldManager : MonoBehaviour
    {
        // Singleton instance
        public static FlowFieldManager Instance { get; private set; }
        
        // Flow fields for different destinations
        private FlowField toCornField;
        private FlowField toSpawnField;
        
        private GridManager grid;
        private bool isInitialized = false;
        
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
            Initialize();
        }
        
        /// <summary>
        /// Initialize flow fields
        /// </summary>
        private void Initialize()
        {
            grid = GridManager.Instance;
            
            if (grid == null || !grid.IsInitialized)
            {
                Debug.LogError("FlowFieldManager: GridManager not found or not initialized!");
                return;
            }
            
            // Subscribe to tower placement events
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.OnTowerPlaced += OnTowerPlaced;
                TowerManager.Instance.OnTowerSold += OnTowerSold;
            }
            
            RegenerateAllFlowFields();
            isInitialized = true;
            
            Debug.Log("FlowFieldManager initialized successfully");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.OnTowerPlaced -= OnTowerPlaced;
                TowerManager.Instance.OnTowerSold -= OnTowerSold;
            }
        }
        
        /// <summary>
        /// Called when a tower is placed - recalculate flow fields
        /// </summary>
        private void OnTowerPlaced(Tower tower)
        {
            Debug.Log($"FlowFieldManager: Tower placed at {tower.Position}, recalculating flow fields...");
            RegenerateAllFlowFields();
        }
        
        /// <summary>
        /// Called when a tower is sold - recalculate flow fields
        /// </summary>
        private void OnTowerSold(Tower tower)
        {
            Debug.Log($"FlowFieldManager: Tower sold at {tower.Position}, recalculating flow fields...");
            RegenerateAllFlowFields();
        }
        
        /// <summary>
        /// Regenerate all flow fields
        /// </summary>
        public void RegenerateAllFlowFields()
        {
            if (!grid.IsInitialized)
            {
                Debug.LogWarning("FlowFieldManager: GridManager not initialized, skipping regeneration");
                return;
            }
            
            // Get corn storage position
            Vector3 cornWorldPos = Vector3.zero;
            if (CornManager.Instance != null)
            {
                cornWorldPos = CornManager.Instance.GetCornStoragePosition();
            }
            else
            {
                Debug.LogWarning("FlowFieldManager: CornManager not found, using center as corn position");
                cornWorldPos = Vector3.zero;
            }
            
            Vector2Int cornGridPos = grid.WorldToGrid(cornWorldPos);
            
            // Clamp to valid grid bounds
            cornGridPos.x = Mathf.Clamp(cornGridPos.x, 0, grid.GridWidth - 1);
            cornGridPos.y = Mathf.Clamp(cornGridPos.y, 0, grid.GridHeight - 1);
            
            // Get spawn position
            Vector3 spawnWorldPos = Vector3.zero;
            if (MapManager.Instance != null)
            {
                spawnWorldPos = MapManager.Instance.GetEnemySpawnPoint();
            }
            else
            {
                Debug.LogWarning("FlowFieldManager: MapManager not found, using origin as spawn position");
            }
            
            Vector2Int spawnGridPos = grid.WorldToGrid(spawnWorldPos);
            
            // Clamp to valid grid bounds
            spawnGridPos.x = Mathf.Clamp(spawnGridPos.x, 0, grid.GridWidth - 1);
            spawnGridPos.y = Mathf.Clamp(spawnGridPos.y, 0, grid.GridHeight - 1);
            
            // Create flow fields
            toCornField = new FlowField(grid, cornGridPos);
            toSpawnField = new FlowField(grid, spawnGridPos);
            
            Debug.Log($"FlowFieldManager: Regenerated flow fields - Corn: {cornGridPos}, Spawn: {spawnGridPos}");
        }
        
        /// <summary>
        /// Get flow direction toward corn storage
        /// </summary>
        public Vector2Int GetFlowToCorn(Vector3 worldPos)
        {
            if (!isInitialized || toCornField == null)
            {
                Debug.LogWarning("FlowFieldManager: Flow field to corn not initialized");
                return Vector2Int.zero;
            }
            
            return toCornField.GetFlowDirection(worldPos);
        }
        
        /// <summary>
        /// Get flow direction toward spawn point
        /// </summary>
        public Vector2Int GetFlowToSpawn(Vector3 worldPos)
        {
            if (!isInitialized || toSpawnField == null)
            {
                Debug.LogWarning("FlowFieldManager: Flow field to spawn not initialized");
                return Vector2Int.zero;
            }
            
            return toSpawnField.GetFlowDirection(worldPos);
        }
        
        /// <summary>
        /// Check if flow field manager is ready
        /// </summary>
        public bool IsReady()
        {
            return isInitialized && toCornField != null && toSpawnField != null;
        }
    }
}
