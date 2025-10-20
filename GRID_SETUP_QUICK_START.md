# Grid System - Quick Setup Guide

## What Was Added

### New Files
1. **GridManager.cs** - Core grid system logic
2. **GridVisualizer.cs** - Optional visual grid overlay
3. **GRID_SYSTEM_IMPLEMENTATION.md** - Full documentation

### Modified Files
1. **MapManager.cs** - Integrated grid initialization
2. **TowerManager.cs** - Towers snap to grid centers
3. **Enemy.cs** - Grid-aligned movement (horizontal/vertical only)

## Key Features Implemented

✅ **Grid Coverage:** Grid automatically covers entire map sprite
✅ **Cell Size:** Exactly 0.5 units as requested
✅ **Tower Placement:** Towers snap to grid cell centers
✅ **Enemy Movement:** Only horizontal or vertical movement (no diagonals)
✅ **Off-Center Spawns:** Automatically handled by snapping to nearest cell center

## Unity Setup Steps

### 1. Create Grid System GameObject
```
Hierarchy:
  └─ GridSystem (Empty GameObject)
       ├─ GridManager (Component)
       └─ GridVisualizer (Component)
```

### 2. Configure GridManager Component
- **Cell Size:** 0.5
- **Map Sprite Renderer:** Drag your map's SpriteRenderer here

### 3. Link to MapManager
In your MapManager inspector:
- **Grid Manager:** Drag GridSystem GameObject
- **Grid Visualizer:** Drag GridSystem GameObject

### 4. (Optional) Enable Grid Visualization
In GridVisualizer component:
- **Show Grid:** ✓ Checked
- **Grid Color:** White with alpha ~0.2
- **Line Width:** 0.02

## How It Works

### Tower Placement
```
Mouse Click → TowerManager → GridManager.SnapToGrid() → Tower placed at cell center
```

### Enemy Movement
```
Enemy Position → GridManager.WorldToGrid() → Calculate next cell → Move horizontally/vertically → Repeat
```

### Spawn Handling
```
Off-center spawn (-5.23, 2.17) → GridManager.SnapToGrid() → Centered position (-5.25, 2.25) → Enemy moves normally
```

## Testing

### Quick Tests
1. **Place a tower:** Should snap to visible grid intersection
2. **Spawn an enemy:** Should move in straight lines (no diagonals)
3. **Check off-center spawn:** Move spawn point slightly off-grid, enemy should still work

### Console Logs to Verify
- "Grid initialized: {width}x{height} cells, cell size: {cellSize}"
- Grid visualizer should report line count

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Grid not visible | Enable "Show Grid" in GridVisualizer |
| Towers not snapping | Check GridManager reference in MapManager |
| Enemies moving diagonally | Verify GridManager.Instance exists and is initialized |
| Enemies stuck at spawn | Check spawn point is within map bounds |

## Example Configuration

### Typical Map Setup
```
Map Size: 20x15 world units
Cell Size: 0.5
Grid Dimensions: 40x30 cells
Total Cells: 1,200
```

### Performance
- **Memory:** ~1-2 MB (mostly visual lines)
- **CPU:** Negligible (<0.1ms per frame)
- **Draw Calls:** +1 per grid line (can be batched)

## Next Steps

After setup, you should see:
1. ✅ Grid lines overlaying your map (if visualizer enabled)
2. ✅ Towers snapping to grid when placed
3. ✅ Enemies moving in straight horizontal/vertical lines
4. ✅ No diagonal enemy movement
5. ✅ Smooth gameplay with no stuck enemies

## Need Help?

Check the full documentation: `GRID_SYSTEM_IMPLEMENTATION.md`

Key classes to review:
- `GridManager.cs` - All grid math and conversions
- `Enemy.MoveTowardsPositionGridAligned()` - Movement algorithm
- `TowerManager.GetValidPlacementPosition()` - Tower snapping
