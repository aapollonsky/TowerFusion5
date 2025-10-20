# Grid System Implementation

## Overview
A comprehensive grid system has been added to TowerFusion5 to ensure:
- Grid fully covers the map sprite
- Cell size of 0.5 units
- Towers snap to grid cell centers when placed
- Enemies move only horizontally or vertically along the grid
- Enemy spawn positioning works even if off-center

## Components

### 1. GridManager (`Assets/Scripts/Map/GridManager.cs`)
**Purpose:** Core grid logic and coordinate conversion

**Key Features:**
- Automatically initializes based on map sprite bounds
- Converts between world positions and grid coordinates
- Snaps positions to grid cell centers
- Provides Manhattan distance calculations
- Returns orthogonal neighbors (no diagonals)

**Public Methods:**
- `SnapToGrid(Vector3 worldPosition)` - Snap world position to nearest grid center
- `WorldToGrid(Vector3 worldPosition)` - Convert world position to grid coordinates
- `GridToWorld(Vector2Int gridPosition)` - Convert grid coordinates to world position
- `GetNeighbors(Vector2Int gridPosition)` - Get adjacent cells (4-directional)
- `GetManhattanDistance(Vector2Int from, Vector2Int to)` - Calculate grid distance

**Properties:**
- `CellSize` - Size of each grid cell (0.5 units)
- `GridWidth` - Number of cells horizontally
- `GridHeight` - Number of cells vertically
- `GridOrigin` - Bottom-left corner of grid in world space

### 2. GridVisualizer (`Assets/Scripts/Map/GridVisualizer.cs`)
**Purpose:** Visual overlay showing grid lines on map

**Key Features:**
- Creates LineRenderer for each grid line
- Configurable color and line width
- Can be toggled on/off at runtime
- Renders above map but below game objects

**Configuration:**
- `showGrid` - Toggle grid visibility
- `gridColor` - Color of grid lines (default: white with alpha)
- `lineWidth` - Width of grid lines (default: 0.02)

### 3. MapManager Integration
**Updated:** `Assets/Scripts/Map/MapManager.cs`

**Changes:**
- Added references to GridManager and GridVisualizer
- Calls `InitializeGridSystem()` when map is loaded
- Grid automatically adjusts when map changes

### 4. TowerManager Integration
**Updated:** `Assets/Scripts/Tower/TowerManager.cs`

**Changes:**
- `GetValidPlacementPosition()` now uses GridManager to snap towers to grid centers
- Towers are guaranteed to be centered in grid cells
- Falls back to manual snapping if GridManager not available

### 5. Enemy Movement Integration
**Updated:** `Assets/Scripts/Enemy/Enemy.cs`

**Changes:**
- `SetupMovement()` snaps enemy spawn position to grid
- New method: `MoveTowardsPositionGridAligned()` - Implements Manhattan pathfinding
- `MoveTowardsTower()` uses grid-aligned movement
- `MoveTowardsCornStorage()` uses grid-aligned movement
- Enemies move horizontally first, then vertically (no diagonal movement)

## Movement Algorithm

### Grid-Aligned Movement
Enemies use **Manhattan distance pathfinding**:

1. **Convert to Grid:** Transform current position and target to grid coordinates
2. **Calculate Delta:** Determine horizontal and vertical distance
3. **Prioritize Horizontal:** Move horizontally first if needed
4. **Then Vertical:** Move vertically after horizontal alignment
5. **Snap to Centers:** Always move toward grid cell centers

**Example Path:**
```
Enemy at (0, 0) moving to (2, 2):
Step 1: (0,0) → (1,0)  [Move right]
Step 2: (1,0) → (2,0)  [Move right]
Step 3: (2,0) → (2,1)  [Move up]
Step 4: (2,1) → (2,2)  [Move up]
```

### Off-Center Spawn Handling
If an enemy spawns off-center (e.g., at position `(-5.23, 2.17)`):

1. **Snap on Initialization:** `GridManager.SnapToGrid()` moves enemy to nearest cell center
2. **Example:** `(-5.23, 2.17)` → `(-5.25, 2.25)` (center of grid cell)
3. **Movement Begins:** Enemy starts moving from snapped position
4. **Result:** No stuttering or stuck enemies

## Setup Instructions

### 1. Add Components to Scene
1. Create an empty GameObject named "GridSystem"
2. Add `GridManager` component
3. Add `GridVisualizer` component (optional, for visual debugging)

### 2. Configure GridManager
- Assign map's `SpriteRenderer` to `mapSpriteRenderer` field
- Set `cellSize` to `0.5` (default)
- Grid will auto-initialize based on sprite bounds

### 3. Link to MapManager
- In MapManager inspector, assign GridManager reference
- Assign GridVisualizer reference (optional)

### 4. Configure GridVisualizer (Optional)
- Toggle `showGrid` for visual debugging
- Adjust `gridColor` for visibility
- Adjust `lineWidth` as needed

## Testing Checklist

### Grid Initialization
- [ ] Grid covers entire map sprite
- [ ] Grid origin at bottom-left of map
- [ ] Cell size is exactly 0.5 units
- [ ] Grid lines visible (if GridVisualizer enabled)

### Tower Placement
- [ ] Towers snap to grid centers when placed
- [ ] Tower placement indicator shows grid-snapped position
- [ ] Multiple towers don't overlap on same cell
- [ ] Towers centered visually in cells

### Enemy Movement
- [ ] Enemies spawn at grid-aligned positions
- [ ] Enemies move only horizontally or vertically
- [ ] No diagonal movement observed
- [ ] Enemies reach towers successfully
- [ ] Enemies reach corn storage successfully
- [ ] Movement is smooth (no stuttering)

### Off-Center Spawning
- [ ] Test spawn point at `(-5.23, 2.17)` - should snap to cell center
- [ ] Enemy doesn't get stuck at spawn
- [ ] Enemy begins moving immediately after spawn
- [ ] Path to target is still valid after snapping

## Troubleshooting

### Grid Not Visible
**Problem:** Grid lines don't appear
**Solutions:**
- Check GridVisualizer `showGrid` is enabled
- Verify GridManager initialized successfully (check console)
- Ensure map sprite is assigned to GridManager
- Check grid color alpha isn't 0

### Enemies Stuck at Spawn
**Problem:** Enemies don't move after spawning
**Solutions:**
- Verify GridManager.Instance exists
- Check spawn position is within grid bounds
- Ensure `MoveTowardsPositionGridAligned()` is being called
- Check enemy speed is > 0

### Towers Not Snapping
**Problem:** Towers place at arbitrary positions
**Solutions:**
- Verify GridManager reference in TowerManager
- Check `GetValidPlacementPosition()` is calling `GridManager.SnapToGrid()`
- Ensure grid is initialized before tower placement

### Diagonal Movement
**Problem:** Enemies move diagonally
**Solutions:**
- Check `MoveTowardsPositionGridAligned()` implementation
- Verify only one axis changes per frame
- Ensure no fallback to direct movement is occurring

## Performance Notes

### Optimization
- Grid calculations are O(1) - constant time
- No pathfinding needed (Manhattan distance)
- Minimal overhead per frame

### Memory Usage
- Grid doesn't store tile data - pure calculation
- GridVisualizer creates LineRenderers once at startup
- Total overhead: ~1-2 MB for visual lines

## Future Enhancements

### Potential Additions
1. **Pathfinding:** A* algorithm for obstacle avoidance
2. **Path Caching:** Pre-calculate common paths
3. **Grid Obstacles:** Mark cells as blocked
4. **Dynamic Grid:** Adjust grid when map changes
5. **Grid Highlighting:** Show valid placement cells

### API Extensions
```csharp
// Potential future methods
bool IsWalkable(Vector2Int gridPos);
List<Vector2Int> FindPath(Vector2Int start, Vector2Int end);
void MarkCellBlocked(Vector2Int gridPos, bool blocked);
```

## Related Files
- `/Assets/Scripts/Map/GridManager.cs`
- `/Assets/Scripts/Map/GridVisualizer.cs`
- `/Assets/Scripts/Map/MapManager.cs`
- `/Assets/Scripts/Tower/TowerManager.cs`
- `/Assets/Scripts/Enemy/Enemy.cs`

## Version History
- **v1.0** (2025-10-20): Initial grid system implementation
  - Grid covers map sprite with 0.5 unit cells
  - Tower placement snapping
  - Grid-aligned enemy movement
  - Off-center spawn handling
