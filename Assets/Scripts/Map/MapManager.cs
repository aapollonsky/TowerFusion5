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
    [SerializeField] private MapLibrary mapLibrary;
        [SerializeField] private LineRenderer pathRenderer;
        [SerializeField] private Transform towerPositionsParent;
        [SerializeField] private GameObject towerPositionMarkerPrefab;
    [SerializeField] private SpriteRenderer mapSpriteRenderer;
    [SerializeField] private bool useMapSprite = true;
        
        [Header("Wave Generation")]
        [SerializeField] private WaveGenerator waveGenerator;
        
        [Header("Grid System")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private GridVisualizer gridVisualizer;
        
        // Singleton instance
        public static MapManager Instance { get; private set; }
        
        // Properties
        public MapData CurrentMap => currentMap;
    // Expose MapLibrary for read-only access to other systems
    public MapLibrary MapLibrary => mapLibrary;
        
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
            // If no current map assigned, try to auto-load from MapLibrary
            if (currentMap == null && mapLibrary != null && mapLibrary.maps != null && mapLibrary.maps.Count > 0)
            {
                Debug.Log("MapManager: No Current Map assigned, auto-loading first map from MapLibrary.");
                currentMap = mapLibrary.maps[0];
            }

            // Check if we should use auto-generated waves
            if (waveGenerator != null && waveGenerator.UseAutoGeneration)
            {
                Debug.Log("<color=cyan>Using auto-generated waves</color>");
                waveGenerator.GenerateWaves();
            }

            InitializeMap();
        }
        
        /// <summary>
        /// Initialize the current map
        /// </summary>
        public void InitializeMap()
        {
            if (currentMap == null)
            {
                Debug.LogError("No map data assigned to MapManager! Assign a MapData or MapLibrary with maps.");
                return;
            }
            // Set map sprite preview if requested
            if (useMapSprite && mapSpriteRenderer != null)
            {
                mapSpriteRenderer.sprite = currentMap.mapPreview;
            }

            // If using a map sprite, hide the LineRenderer path so the sprite's path isn't covered.
            if (pathRenderer != null)
            {
                pathRenderer.enabled = !useMapSprite;
            }

            SetupPath();
            SetupTowerPositions();
            InitializeGridSystem();
            
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
            ClearCurrentMapVisuals();
            currentMap = newMap;
            InitializeMap();
        }

        /// <summary>
        /// Load map by index from the MapLibrary
        /// </summary>
        public void LoadMapByIndex(int index)
        {
            if (mapLibrary == null)
            {
                Debug.LogError("MapLibrary is not assigned to MapManager");
                return;
            }

            MapData m = mapLibrary.GetMap(index);
            if (m == null)
            {
                Debug.LogError($"No map at index {index} in MapLibrary");
                return;
            }

            LoadMap(m);
        }

        /// <summary>
        /// Load map by name from the MapLibrary
        /// </summary>
        public void LoadMapByName(string name)
        {
            if (mapLibrary == null)
            {
                Debug.LogError("MapLibrary is not assigned to MapManager");
                return;
            }

            MapData m = mapLibrary.GetMapByName(name);
            if (m == null)
            {
                Debug.LogError($"No map named {name} in MapLibrary");
                return;
            }

            LoadMap(m);
        }

        /// <summary>
        /// Load next map in the MapLibrary (wraps around)
        /// </summary>
        public void LoadNextMap()
        {
            if (mapLibrary == null || mapLibrary.maps == null || mapLibrary.maps.Count == 0)
            {
                Debug.LogError("MapLibrary is not configured or empty");
                return;
            }

            int currentIndex = mapLibrary.GetIndex(currentMap);
            int nextIndex = (currentIndex + 1) % mapLibrary.maps.Count;
            LoadMapByIndex(nextIndex);
        }

        private void ClearCurrentMapVisuals()
        {
            // Clear LineRenderer
            if (pathRenderer != null)
            {
                pathRenderer.positionCount = 0;
            }

            // Clear tower markers
            if (towerPositionsParent != null)
            {
                foreach (Transform child in towerPositionsParent)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            // Clear sprite
            if (mapSpriteRenderer != null)
            {
                mapSpriteRenderer.sprite = null;
            }
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
            // Check if using auto-generated waves
            if (waveGenerator != null && waveGenerator.UseAutoGeneration)
            {
                return waveGenerator.GetGeneratedWave(waveNumber);
            }
            
            // Otherwise use map's predefined waves
            if (currentMap == null || waveNumber <= 0 || waveNumber > currentMap.waves.Count)
                return null;
            
            return currentMap.waves[waveNumber - 1];
        }
        
        /// <summary>
        /// Get total number of waves for current map
        /// </summary>
        public int GetTotalWaves()
        {
            // Check if using auto-generated waves
            if (waveGenerator != null && waveGenerator.UseAutoGeneration)
            {
                return waveGenerator.NumberOfWaves;
            }
            
            // Otherwise use map's predefined wave count
            return currentMap?.waves.Count ?? 0;
        }
        
        /// <summary>
        /// Initialize grid system based on current map
        /// </summary>
        private void InitializeGridSystem()
        {
            // Set the map sprite reference in GridManager and reinitialize
            if (mapSpriteRenderer != null)
            {
                if (gridManager != null)
                {
                    gridManager.SetMapSprite(mapSpriteRenderer);
                }
                else if (GridManager.Instance != null)
                {
                    GridManager.Instance.SetMapSprite(mapSpriteRenderer);
                }
                else
                {
                    Debug.LogWarning("MapManager: No GridManager found to initialize grid system");
                }
            }
            else
            {
                Debug.LogWarning("MapManager: No map sprite renderer assigned, grid may not match map size");
            }
            
            // Refresh grid visualization after grid is reinitialized
            if (gridVisualizer != null)
            {
                gridVisualizer.RefreshGrid();
            }
            else if (FindObjectOfType<GridVisualizer>() != null)
            {
                FindObjectOfType<GridVisualizer>().RefreshGrid();
            }
            
            Debug.Log("Grid system initialized with map sprite");
        }
    }
}