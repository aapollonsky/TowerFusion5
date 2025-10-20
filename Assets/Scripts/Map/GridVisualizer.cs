using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// Visualizes the grid overlay on the map
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);
        [SerializeField] private float lineWidth = 0.02f;
        
        private GameObject gridLinesParent;
        
        private void Start()
        {
            // Don't create grid immediately - wait for MapManager to initialize
            // Grid will be created via RefreshGrid() call from MapManager
        }
        
        /// <summary>
        /// Refresh the grid visualization (call after grid is reinitialized)
        /// </summary>
        public void RefreshGrid()
        {
            // Clear existing grid
            if (gridLinesParent != null)
            {
                Destroy(gridLinesParent);
            }
            
            // Create new grid if enabled
            if (showGrid && GridManager.Instance != null && GridManager.Instance.IsInitialized)
            {
                CreateGridLines();
            }
        }
        
        /// <summary>
        /// Create visual grid lines
        /// </summary>
        private void CreateGridLines()
        {
            if (GridManager.Instance == null || !GridManager.Instance.IsInitialized)
            {
                Debug.LogWarning("GridVisualizer: GridManager not initialized");
                return;
            }
            
            // Create parent object for grid lines
            gridLinesParent = new GameObject("GridLines");
            gridLinesParent.transform.SetParent(transform);
            
            GridManager grid = GridManager.Instance;
            float cellSize = grid.CellSize;
            Vector3 origin = grid.GridOrigin;
            int width = grid.GridWidth;
            int height = grid.GridHeight;
            
            // Calculate grid bounds
            float gridWorldWidth = width * cellSize;
            float gridWorldHeight = height * cellSize;
            
            Vector3 gridEndPoint = new Vector3(
                origin.x + gridWorldWidth,
                origin.y + gridWorldHeight,
                0f
            );
            
            Debug.Log($"Grid coverage: From {origin} to {gridEndPoint}, Size=({gridWorldWidth:F2}, {gridWorldHeight:F2})");
            
            int lineCount = 0;
            
            // Create vertical lines
            for (int x = 0; x <= width; x++)
            {
                float worldX = origin.x + (x * cellSize);
                CreateLine(
                    new Vector3(worldX, origin.y, 0f),
                    new Vector3(worldX, origin.y + gridWorldHeight, 0f),
                    $"VerticalLine_{x}"
                );
                lineCount++;
            }
            
            // Create horizontal lines
            for (int y = 0; y <= height; y++)
            {
                float worldY = origin.y + (y * cellSize);
                CreateLine(
                    new Vector3(origin.x, worldY, 0f),
                    new Vector3(origin.x + gridWorldWidth, worldY, 0f),
                    $"HorizontalLine_{y}"
                );
                lineCount++;
            }
            
            Debug.Log($"Grid visualizer created: {lineCount} lines, covering area from {origin} to ({origin.x + gridWorldWidth:F2}, {origin.y + gridWorldHeight:F2})");
        }
        
        /// <summary>
        /// Create a single line
        /// </summary>
        private void CreateLine(Vector3 start, Vector3 end, string name)
        {
            GameObject lineObj = new GameObject(name);
            lineObj.transform.SetParent(gridLinesParent.transform);
            
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = gridColor;
            lr.endColor = gridColor;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.sortingOrder = 1; // Render above map but below other elements
        }
        
        /// <summary>
        /// Toggle grid visibility
        /// </summary>
        public void SetGridVisible(bool visible)
        {
            showGrid = visible;
            if (gridLinesParent != null)
            {
                gridLinesParent.SetActive(visible);
            }
        }
    }
}
