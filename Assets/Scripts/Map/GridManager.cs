using UnityEngine;
using System.Collections.Generic;

namespace TowerFusion
{
    /// <summary>
    /// Manages the grid system for tower placement and enemy movement
    /// Grid is based on map sprite bounds with configurable cell size
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private float cellSize = 0.5f;
        [SerializeField] private SpriteRenderer mapSpriteRenderer;
        [SerializeField] private bool useExactSpriteSize = true; // New option
        [SerializeField] [Range(0f, 1f)] private float gridMargin = 0f; // Optional margin
        
        // Grid data
        private Vector3 gridOrigin;
        private int gridWidth;
        private int gridHeight;
        private bool isInitialized = false;
        
        // Singleton
        public static GridManager Instance { get; private set; }
        
        // Properties
        public float CellSize => cellSize;
        public Vector3 GridOrigin => gridOrigin;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public bool IsInitialized => isInitialized;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeGrid();
        }
        
        /// <summary>
        /// Set the map sprite renderer reference and reinitialize grid
        /// </summary>
        public void SetMapSprite(SpriteRenderer spriteRenderer)
        {
            mapSpriteRenderer = spriteRenderer;
            InitializeGrid();
        }
        
        /// <summary>
        /// Initialize grid based on map sprite bounds
        /// </summary>
        public void InitializeGrid()
        {
            if (mapSpriteRenderer == null || mapSpriteRenderer.sprite == null)
            {
                Debug.LogWarning("GridManager: No map sprite assigned, using default grid size");
                // Default fallback grid (20x15 world units)
                gridOrigin = new Vector3(-10f, -7.5f, 0f);
                gridWidth = (int)(20f / cellSize);
                gridHeight = (int)(15f / cellSize);
            }
            else
            {
                // Calculate grid based on sprite bounds
                Bounds spriteBounds = mapSpriteRenderer.bounds;
                
                // Debug: Show all sprite information
                Sprite sprite = mapSpriteRenderer.sprite;
                Debug.Log($"Sprite Info: " +
                    $"PixelsPerUnit={sprite.pixelsPerUnit}, " +
                    $"Rect={sprite.rect}, " +
                    $"Pivot={sprite.pivot}, " +
                    $"Transform Scale={mapSpriteRenderer.transform.localScale}");
                
                // Grid origin is bottom-left corner
                gridOrigin = new Vector3(
                    spriteBounds.min.x,
                    spriteBounds.min.y,
                    0f
                );
                
                // Calculate grid dimensions - use Floor to ensure we don't exceed sprite bounds
                float worldWidth = spriteBounds.size.x;
                float worldHeight = spriteBounds.size.y;
                
                // Apply margin if specified (reduces grid size)
                if (gridMargin > 0f)
                {
                    worldWidth *= (1f - gridMargin);
                    worldHeight *= (1f - gridMargin);
                    Debug.Log($"Applied {gridMargin * 100f}% margin, adjusted size: ({worldWidth:F2}, {worldHeight:F2})");
                }
                
                // If useExactSpriteSize is true, we want the grid to match sprite dimensions exactly
                // by centering the grid within the sprite bounds
                if (useExactSpriteSize)
                {
                    // Calculate how many complete cells fit
                    int cellsWidth = Mathf.FloorToInt(worldWidth / cellSize);
                    int cellsHeight = Mathf.FloorToInt(worldHeight / cellSize);
                    
                    // Calculate exact grid size that fits these cells
                    float exactGridWidth = cellsWidth * cellSize;
                    float exactGridHeight = cellsHeight * cellSize;
                    
                    // Center the grid within sprite bounds
                    float offsetX = (worldWidth - exactGridWidth) * 0.5f;
                    float offsetY = (worldHeight - exactGridHeight) * 0.5f;
                    
                    gridOrigin = new Vector3(
                        spriteBounds.min.x + offsetX,
                        spriteBounds.min.y + offsetY,
                        0f
                    );
                    
                    gridWidth = cellsWidth;
                    gridHeight = cellsHeight;
                }
                else
                {
                    // Original behavior: start from bottom-left
                    gridWidth = Mathf.FloorToInt(worldWidth / cellSize);
                    gridHeight = Mathf.FloorToInt(worldHeight / cellSize);
                }
                
                // Calculate actual grid coverage
                float actualGridWidth = gridWidth * cellSize;
                float actualGridHeight = gridHeight * cellSize;
                Vector3 gridMax = new Vector3(gridOrigin.x + actualGridWidth, gridOrigin.y + actualGridHeight, 0f);
                
                Debug.Log($"Sprite Bounds: Min={spriteBounds.min}, Max={spriteBounds.max}, Size=({worldWidth:F2}, {worldHeight:F2})");
                Debug.Log($"Grid Coverage: Origin={gridOrigin}, Max={gridMax}, Size=({actualGridWidth:F2}, {actualGridHeight:F2})");
                Debug.Log($"Unused space: Width={worldWidth - actualGridWidth:F3}, Height={worldHeight - actualGridHeight:F3}");
            }
            
            isInitialized = true;
            Debug.Log($"Grid initialized: {gridWidth}x{gridHeight} cells, cell size: {cellSize}");
        }
        
        /// <summary>
        /// Snap world position to nearest grid cell center
        /// </summary>
        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            if (!isInitialized)
                return worldPosition;
            
            // Convert to grid coordinates
            Vector2Int gridPos = WorldToGrid(worldPosition);
            
            // Convert back to world position (center of cell)
            return GridToWorld(gridPos);
        }
        
        /// <summary>
        /// Convert world position to grid coordinates
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            if (!isInitialized)
                return Vector2Int.zero;
            
            Vector3 localPos = worldPosition - gridOrigin;
            
            int gridX = Mathf.FloorToInt(localPos.x / cellSize);
            int gridY = Mathf.FloorToInt(localPos.y / cellSize);
            
            // Clamp to grid bounds
            gridX = Mathf.Clamp(gridX, 0, gridWidth - 1);
            gridY = Mathf.Clamp(gridY, 0, gridHeight - 1);
            
            return new Vector2Int(gridX, gridY);
        }
        
        /// <summary>
        /// Convert grid coordinates to world position (center of cell)
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            if (!isInitialized)
                return Vector3.zero;
            
            float worldX = gridOrigin.x + (gridPosition.x * cellSize) + (cellSize * 0.5f);
            float worldY = gridOrigin.y + (gridPosition.y * cellSize) + (cellSize * 0.5f);
            
            return new Vector3(worldX, worldY, 0f);
        }
        
        /// <summary>
        /// Check if grid position is within bounds
        /// </summary>
        public bool IsValidGridPosition(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
                   gridPosition.y >= 0 && gridPosition.y < gridHeight;
        }
        
        /// <summary>
        /// Get neighboring grid cells (up, down, left, right only - no diagonals)
        /// </summary>
        public List<Vector2Int> GetNeighbors(Vector2Int gridPosition)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            
            // Cardinal directions only (no diagonals)
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1),  // Down
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(1, 0)    // Right
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = gridPosition + dir;
                if (IsValidGridPosition(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
            
            return neighbors;
        }
        
        /// <summary>
        /// Calculate Manhattan distance between two grid positions
        /// </summary>
        public int GetManhattanDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(to.x - from.x) + Mathf.Abs(to.y - from.y);
        }
        
        /// <summary>
        /// Check if a grid cell is blocked by a tower
        /// </summary>
        public bool IsCellBlocked(Vector2Int gridPosition)
        {
            if (!IsValidGridPosition(gridPosition))
                return true;
            
            // Convert grid position to world position
            Vector3 worldPos = GridToWorld(gridPosition);
            
            // Check if any tower occupies this cell
            if (TowerManager.Instance != null)
            {
                var towers = TowerManager.Instance.GetActiveTowers();
                if (towers != null)
                {
                    foreach (var tower in towers)
                    {
                        if (tower == null || !tower.IsAlive)
                            continue;
                        
                        // Check if tower is in this grid cell
                        Vector2Int towerGridPos = WorldToGrid(tower.Position);
                        if (towerGridPos == gridPosition)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
    }
}
