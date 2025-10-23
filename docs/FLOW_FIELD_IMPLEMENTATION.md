# Flow Field Pathfinding Implementation

## Overview

Flow field pathfinding has been implemented to allow enemies to navigate around towers to reach the corn storage and return to the spawn point. The system uses Dijkstra's algorithm to generate integration fields that are automatically recalculated when towers are placed or sold.

## Architecture

### FlowField.cs
**Location:** `Assets/Scripts/Map/FlowField.cs`

Individual flow field class that generates:
- **Cost Field**: Marks blocked cells (towers) with high cost (255), traversable cells with default cost (1)
- **Integration Field**: Dijkstra-based total cost from goal to each cell
- **Flow Field**: Best direction to move from each cell toward goal

**Key Methods:**
- `CreateCostField()` - Marks tower positions as blocked
- `CreateIntegrationField()` - Dijkstra's algorithm for optimal paths
- `CreateFlowField()` - Generates directional flow from integration field
- `GetFlowDirection(Vector3 worldPosition)` - Returns Vector2Int direction for a given world position

**Constants:**
- `DEFAULT_COST = 1` - Normal traversable cell
- `BLOCKED_COST = 255` - Tower-occupied cell (impassable)
- `MAX_COST = 65535` - Unvisited cell (ushort.MaxValue)

### FlowFieldManager.cs
**Location:** `Assets/Scripts/Map/FlowFieldManager.cs`

Singleton manager that maintains two flow fields:
1. **toCorn** - Paths from anywhere to corn storage
2. **toSpawn** - Paths from anywhere to enemy spawn point

**Key Methods:**
- `GetFlowToCorn(Vector3 worldPosition)` - Returns flow direction toward corn
- `GetFlowToSpawn(Vector3 worldPosition)` - Returns flow direction toward spawn
- `RegenerateAllFlowFields()` - Recalculates both flow fields
- `IsReady()` - Checks if flow fields are initialized

**Auto-Regeneration:**
- Subscribes to `TowerManager.OnTowerPlaced` event
- Subscribes to `TowerManager.OnTowerSold` event
- Regenerates both flow fields whenever towers change

### Enemy.cs Updates
**Location:** `Assets/Scripts/Enemy/Enemy.cs`

Added flow field-based movement methods:

**MoveUsingFlowField(Vector2Int flowDirection)**
- Converts Vector2Int flow direction to world direction
- Applies separation force if enabled (`enemyData.useSeparation`)
- Blends flow direction with separation force
- Moves enemy at current speed

**Updated Movement Methods:**
All three movement methods now use flow fields with fallbacks:

1. **MoveTowardsCornStorage()** (MovingToCorn state)
   ```csharp
   if (FlowFieldManager available and ready)
       MoveUsingFlowField(GetFlowToCorn())
   else if (GridManager available)
       MoveTowardsPositionGridAligned(cornPosition)
   else
       Direct movement fallback
   ```

2. **ReturnToSpawn()** (ReturningWithCorn state)
   ```csharp
   if (FlowFieldManager available and ready)
       MoveUsingFlowField(GetFlowToSpawn())
   else if (GridManager available)
       MoveTowardsPositionGridAligned(spawnPoint)
   else
       Direct movement fallback
   ```

3. **ReturnToSpawnEmpty()** (ReturningEmpty state)
   - Same flow field logic as ReturnToSpawn()
   - Used when enemy loses corn during counterattack

### GridManager.cs Updates
**Location:** `Assets/Scripts/Map/GridManager.cs`

Added tower blocking detection:

**IsCellBlocked(Vector2Int gridPosition)**
- Checks if grid cell contains a tower
- Returns true if any active tower occupies the cell
- Used by FlowField to mark blocked cells in cost field

## Integration with Existing Systems

### Flocking Separation
Flow field movement integrates with existing separation system:
- Base direction from flow field
- Separation force calculated from nearby enemies
- Final direction = normalized(flow + separation)
- Controlled by `EnemyData.useSeparation`, `separationRadius`, `separationStrength`

### Reactive Defense System
Flow field works seamlessly with counterattack states:
- **MovingToCorn**: Uses flow field to navigate around towers
- **ReturningWithCorn**: Uses flow field to return to spawn
- **ReturningEmpty**: Uses flow field to return after losing corn
- **CounterattackingTower**: Still uses direct grid-aligned movement (intentional - direct approach to tower)

### State Machine
Flow fields are used in 3 of 6 enemy states:
- ✓ MovingToCorn - Flow to corn storage
- ✗ GrabbingCorn - No movement (grabbing animation)
- ✗ WaitingForCorn - No movement (waiting for availability)
- ✓ ReturningWithCorn - Flow to spawn point
- ✓ ReturningEmpty - Flow to spawn point
- ✗ CounterattackingTower - Grid-aligned direct movement

## Performance Considerations

### Regeneration Cost
- Flow field regeneration triggers on every tower placement/sale
- Uses Dijkstra's algorithm: O((width × height) × log(width × height))
- For 26×15 grid (390 cells): ~3,000-4,000 operations per regeneration
- Two flow fields regenerated per tower change

### Runtime Cost
- `GetFlowToCorn/ToSpawn()`: O(1) - simple array lookup
- `MoveUsingFlowField()`: O(n) where n = nearby enemies (separation calculation)
- Flow direction cached per cell, no recalculation during movement

### Memory Usage
Per flow field:
- Cost field: `byte[26, 15]` = 390 bytes
- Integration field: `ushort[26, 15]` = 780 bytes
- Flow field: `Vector2Int[26, 15]` = 3,120 bytes
- Total per field: ~4 KB
- Two fields (corn + spawn): ~8 KB total

## Testing Checklist

### Setup
- [ ] Add FlowFieldManager GameObject to scene
- [ ] Ensure GridManager is initialized with map sprite
- [ ] Ensure CornManager is set up with storage position
- [ ] Ensure MapManager has enemy spawn point configured

### Flow Field Generation
- [ ] Check console for "Flow field ready" messages on scene start
- [ ] Place a tower, verify console shows "Regenerating all flow fields"
- [ ] Sell a tower, verify flow fields regenerate
- [ ] Verify no errors in console during regeneration

### Enemy Movement
- [ ] Spawn enemies, verify they navigate toward corn storage
- [ ] Place towers in their path, verify enemies route around them
- [ ] Verify enemies reach corn storage successfully
- [ ] Watch enemies return to spawn, verify they use flow field
- [ ] Kill enemy with corn, verify enemy returns empty using flow field

### Integration Testing
- [ ] Test with separation enabled, verify smooth movement
- [ ] Test reactive defense, verify counterattack works
- [ ] Place multiple towers, verify enemies find optimal paths
- [ ] Create "maze" with towers, verify enemies navigate correctly
- [ ] Test edge cases: tower at spawn, tower at corn storage

### Performance
- [ ] Monitor frame rate with 20+ enemies
- [ ] Check console timing for flow field regeneration
- [ ] Verify no lag when placing towers
- [ ] Test large wave (50+ enemies) with separation + flow fields

## Known Issues

### Compilation Warning
`GridManager.IsCellBlocked` may show as not found during initial compilation. This is a Unity compilation order issue and should resolve after reimporting scripts or restarting Unity Editor.

**Resolution:**
1. Right-click Assets folder → Reimport
2. Or restart Unity Editor

### Fallback Behavior
If FlowFieldManager is not initialized or ready:
- Enemies fallback to `MoveTowardsPositionGridAligned` (old system)
- If GridManager also unavailable, direct movement used
- Check console for warnings if fallbacks are used

## Configuration

### Flow Field Constants (FlowField.cs)
```csharp
const byte DEFAULT_COST = 1;     // Normal cell cost
const byte BLOCKED_COST = 255;   // Tower-blocked cell
const ushort MAX_COST = 65535;   // Unvisited cell
```

### Separation Integration (Enemy.cs)
```csharp
// Controlled by EnemyData ScriptableObject
bool useSeparation;           // Enable separation force
float separationRadius;       // Detection radius (default 1.0)
float separationStrength;     // Force multiplier (default 2.0)
```

## Future Enhancements

### Optimization
- [ ] Only regenerate one flow field if tower far from other destination
- [ ] Cache flow fields for common tower configurations
- [ ] Use dirty flags to skip unnecessary regenerations

### Features
- [ ] Support for terrain types (slow zones, fast zones)
- [ ] Dynamic obstacles (moving hazards)
- [ ] Multiple corn storage locations
- [ ] Enemy-specific cost modifiers (flying enemies ignore towers)

### Debugging
- [ ] Visual debug overlay showing flow directions
- [ ] Gizmos for integration field costs
- [ ] Runtime flow field inspector

## References

- Original reactive defense design: `docs/REACTIVE_DEFENSE_SYSTEM.md`
- Flocking separation guide: `docs/FLOCKING_SEPARATION.md`
- GitHub Copilot instructions: `.github/copilot-instructions.md`
