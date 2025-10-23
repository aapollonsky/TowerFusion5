using UnityEngine;
using System.Collections.Generic;

namespace TowerFusion
{
    /// <summary>
    /// Represents a flow field for pathfinding using Dijkstra's algorithm
    /// </summary>
    public class FlowField
    {
        private GridManager grid;
        private Vector2Int goalCell;
        
        // Cost field: cost to reach each cell (higher = harder to traverse)
        private byte[,] costField;
        
        // Integration field: total cost from goal to each cell (Dijkstra result)
        private ushort[,] integrationField;
        
        // Flow field: best direction to move from each cell
        private Vector2Int[,] flowField;
        
        private const byte BLOCKED_COST = 255;
        private const byte DEFAULT_COST = 1;
        private const ushort MAX_COST = ushort.MaxValue;
        
        public FlowField(GridManager gridManager, Vector2Int goal)
        {
            grid = gridManager;
            goalCell = goal;
            
            int width = grid.GridWidth;
            int height = grid.GridHeight;
            
            costField = new byte[width, height];
            integrationField = new ushort[width, height];
            flowField = new Vector2Int[width, height];
            
            CalculateFlowField();
        }
        
        /// <summary>
        /// Get the flow direction at a given world position
        /// </summary>
        public Vector2Int GetFlowDirection(Vector3 worldPos)
        {
            Vector2Int gridPos = grid.WorldToGrid(worldPos);
            
            if (!IsValidCell(gridPos))
                return Vector2Int.zero;
            
            return flowField[gridPos.x, gridPos.y];
        }
        
        /// <summary>
        /// Get the cost at a given grid cell
        /// </summary>
        public byte GetCost(Vector2Int gridPos)
        {
            if (!IsValidCell(gridPos))
                return BLOCKED_COST;
            
            return costField[gridPos.x, gridPos.y];
        }
        
        /// <summary>
        /// Check if a grid cell is valid
        /// </summary>
        private bool IsValidCell(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < grid.GridWidth &&
                   cell.y >= 0 && cell.y < grid.GridHeight;
        }
        
        /// <summary>
        /// Calculate the complete flow field (cost → integration → flow)
        /// </summary>
        private void CalculateFlowField()
        {
            CreateCostField();
            CreateIntegrationField();
            CreateFlowField();
        }
        
        /// <summary>
        /// Create the cost field based on obstacles and towers
        /// </summary>
        private void CreateCostField()
        {
            int width = grid.GridWidth;
            int height = grid.GridHeight;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    
                    // Check if cell is blocked by tower
                    if (grid.IsCellBlocked(cell))
                    {
                        costField[x, y] = BLOCKED_COST;
                    }
                    else
                    {
                        costField[x, y] = DEFAULT_COST;
                    }
                }
            }
        }
        
        /// <summary>
        /// Create integration field using Dijkstra's algorithm
        /// </summary>
        private void CreateIntegrationField()
        {
            int width = grid.GridWidth;
            int height = grid.GridHeight;
            
            // Initialize all cells to max cost
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    integrationField[x, y] = MAX_COST;
                }
            }
            
            // Validate goal position
            if (!IsValidCell(goalCell))
            {
                Debug.LogError($"FlowField: Invalid goal position {goalCell}. Grid size: {width}x{height}");
                return;
            }
            
            // Goal has zero cost
            integrationField[goalCell.x, goalCell.y] = 0;
            
            // Dijkstra's algorithm using priority queue
            Queue<Vector2Int> openSet = new Queue<Vector2Int>();
            openSet.Enqueue(goalCell);
            
            while (openSet.Count > 0)
            {
                Vector2Int current = openSet.Dequeue();
                
                // Get neighbors (4-directional: up, down, left, right)
                List<Vector2Int> neighbors = GetNeighbors(current);
                
                foreach (Vector2Int neighbor in neighbors)
                {
                    // Skip blocked cells
                    if (costField[neighbor.x, neighbor.y] == BLOCKED_COST)
                        continue;
                    
                    // Calculate new cost
                    ushort currentCost = integrationField[current.x, current.y];
                    byte moveCost = costField[neighbor.x, neighbor.y];
                    ushort newCost = (ushort)(currentCost + moveCost);
                    
                    // Prevent overflow
                    if (newCost < currentCost)
                        newCost = MAX_COST;
                    
                    // Update if we found a better path
                    if (newCost < integrationField[neighbor.x, neighbor.y])
                    {
                        integrationField[neighbor.x, neighbor.y] = newCost;
                        openSet.Enqueue(neighbor);
                    }
                }
            }
        }
        
        /// <summary>
        /// Create flow field from integration field
        /// </summary>
        private void CreateFlowField()
        {
            int width = grid.GridWidth;
            int height = grid.GridHeight;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    
                    // Skip blocked cells
                    if (costField[x, y] == BLOCKED_COST)
                    {
                        flowField[x, y] = Vector2Int.zero;
                        continue;
                    }
                    
                    // Find neighbor with lowest integration cost
                    Vector2Int bestDirection = Vector2Int.zero;
                    ushort lowestCost = integrationField[x, y];
                    
                    List<Vector2Int> neighbors = GetNeighbors(cell);
                    
                    foreach (Vector2Int neighbor in neighbors)
                    {
                        ushort neighborCost = integrationField[neighbor.x, neighbor.y];
                        
                        if (neighborCost < lowestCost)
                        {
                            lowestCost = neighborCost;
                            bestDirection = neighbor - cell;
                        }
                    }
                    
                    flowField[x, y] = bestDirection;
                }
            }
        }
        
        /// <summary>
        /// Get valid neighbors of a cell (4-directional)
        /// </summary>
        private List<Vector2Int> GetNeighbors(Vector2Int cell)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            
            // Up
            if (cell.y + 1 < grid.GridHeight)
                neighbors.Add(new Vector2Int(cell.x, cell.y + 1));
            
            // Down
            if (cell.y - 1 >= 0)
                neighbors.Add(new Vector2Int(cell.x, cell.y - 1));
            
            // Right
            if (cell.x + 1 < grid.GridWidth)
                neighbors.Add(new Vector2Int(cell.x + 1, cell.y));
            
            // Left
            if (cell.x - 1 >= 0)
                neighbors.Add(new Vector2Int(cell.x - 1, cell.y));
            
            return neighbors;
        }
        
        /// <summary>
        /// Recalculate the flow field (called when obstacles change)
        /// </summary>
        public void Recalculate()
        {
            CalculateFlowField();
        }
    }
}
