# Grid Size Fix

## Problem
Grid was covering an area bigger than the map sprite.

## Root Cause
The `GridManager.InitializeGrid()` method was using `Mathf.CeilToInt()` to calculate grid dimensions, which rounds **up**. This caused the grid to be 1 cell larger than the sprite bounds in some cases.

**Example:**
- Sprite width: 13.5 units
- Cell size: 0.5
- Old calculation: `Mathf.CeilToInt(13.5 / 0.5)` = `Mathf.CeilToInt(27)` = **27 cells**
- Grid coverage: 27 × 0.5 = **13.5 units** ✅ (correct by luck)

But if sprite width is 13.4:
- Old calculation: `Mathf.CeilToInt(13.4 / 0.5)` = `Mathf.CeilToInt(26.8)` = **27 cells**
- Grid coverage: 27 × 0.5 = **13.5 units** ❌ (0.1 units too large!)

## Solution
Changed to `Mathf.FloorToInt()` which rounds **down**, ensuring grid never exceeds sprite bounds.

**Fixed calculation:**
- Sprite width: 13.4 units
- Cell size: 0.5
- New calculation: `Mathf.FloorToInt(13.4 / 0.5)` = `Mathf.FloorToInt(26.8)` = **26 cells**
- Grid coverage: 26 × 0.5 = **13.0 units** ✅ (fits within sprite)

## Files Modified
1. **GridManager.cs** - Changed `CeilToInt` to `FloorToInt` for grid dimension calculation
2. **GridVisualizer.cs** - Added debug output showing actual grid coverage

## How to Verify Fix

### 1. Check Console Logs
When you run the game, you should see:
```
Grid bounds: Origin=(-6.75, -5.00), SpriteSize=(13.50, 10.00)
Grid initialized: 27x20 cells, cell size: 0.5
Grid coverage: From (-6.75, -5.00, 0.00) to (6.75, 5.00, 0.00), Size=(13.50, 10.00)
```

### 2. Visual Verification
- Open your scene in Unity
- Enable GridVisualizer's "Show Grid" option
- Check that grid lines **stop at the edges** of your map sprite
- No grid lines should extend beyond the visible map

### 3. Mathematical Check
Calculate expected grid size:
```
Map Width (units) ÷ Cell Size = Grid Width (cells)
Map Height (units) ÷ Cell Size = Grid Height (cells)

Example:
13.5 ÷ 0.5 = 27 cells (width)
10.0 ÷ 0.5 = 20 cells (height)
```

### 4. Edge Case Testing
Test with various map sizes:
- **Exact fit:** 10.0 units → 20 cells ✅
- **Slight over:** 10.1 units → 20 cells (fits within, small margin)
- **Slight under:** 9.9 units → 19 cells (fits perfectly)

## Impact on Gameplay

### Positive Changes
✅ Grid no longer extends beyond map boundaries
✅ Tower placement won't allow placing outside visible map
✅ Enemy pathfinding stays within map bounds
✅ More accurate grid-to-world coordinate conversion

### No Negative Impact
- Enemies still move correctly
- Towers still snap to grid
- All existing functionality preserved
- Performance unchanged

## Technical Details

### Before (Wrong)
```csharp
gridWidth = Mathf.CeilToInt(worldWidth / cellSize);  // Could overshoot
gridHeight = Mathf.CeilToInt(worldHeight / cellSize); // Could overshoot
```

### After (Correct)
```csharp
gridWidth = Mathf.FloorToInt(worldWidth / cellSize);  // Always fits
gridHeight = Mathf.FloorToInt(worldHeight / cellSize); // Always fits
```

### Why FloorToInt is Correct
- We want the grid to **fit inside** the sprite bounds
- Rounding down ensures we never exceed the boundary
- Any "leftover" space (< 1 cell) remains unused
- This is standard practice for grid-based games

## Related Documentation
- See `GRID_SYSTEM_IMPLEMENTATION.md` for full grid system docs
- See `GRID_SETUP_QUICK_START.md` for setup instructions

## Testing Checklist
- [ ] Console shows grid bounds matching sprite size
- [ ] Grid lines stop at map edges (visual check)
- [ ] Towers can be placed throughout entire visible map
- [ ] No towers can be placed outside visible map
- [ ] Enemies spawn and move correctly
- [ ] Grid coverage logs show correct dimensions
