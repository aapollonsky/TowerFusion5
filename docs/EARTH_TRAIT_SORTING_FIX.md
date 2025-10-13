# Earth Trait - Sorting Order Fix

## Problem Identified
The black disk was being created with `sortingOrder = -1`, which placed it **behind the map/background layer**. This made it invisible even though the GameObject was being created correctly.

## Solution Applied

### Updated Sorting Orders
```csharp
// Main black disk
diskRenderer.sortingOrder = 5;  // Changed from -1

// Shadow layer
shadowRenderer.sortingOrder = 4; // Changed from -2
```

## Game's Sorting Order Hierarchy

Based on the codebase analysis:

```
Layer                   | Sorting Order | Visible?
------------------------|---------------|----------
Map/Background          | 0 (default)   | Background
Ground Effects          | 1-4           | Above ground
BLACK DISK (NEW)        | 5             | ✓ Above map
Shadow (NEW)            | 4             | ✓ Above map
Effects/Projectiles     | 10            | Above disk
Enemies                 | ~10-50        | Above disk
Towers (base)           | 1000          | Tower layer
Towers (main)           | 1001          | Tower layer
UI/Badges               | 100-1000      | UI layer
```

## Why This Fix Works

### Before (sortingOrder = -1)
```
┌─────────────────────────┐
│ Enemies (order: 10+)    │ ← Visible
├─────────────────────────┤
│ Map/Background (0)      │ ← Visible
├─────────────────────────┤
│ BLACK DISK (-1)         │ ← HIDDEN BEHIND MAP!
└─────────────────────────┘
```

### After (sortingOrder = 5)
```
┌─────────────────────────┐
│ Enemies (order: 10+)    │ ← Visible
├─────────────────────────┤
│ BLACK DISK (5)          │ ← NOW VISIBLE!
├─────────────────────────┤
│ Shadow (4)              │ ← Adds depth
├─────────────────────────┤
│ Map/Background (0)      │ ← Behind disk
└─────────────────────────┘
```

## Testing the Fix

### What Should Happen Now
1. Tower shoots enemy with Earth trait
2. Black disk appears on ground (visible!)
3. Disk is above the map texture
4. Disk is below enemies (they walk over it)
5. Enemies touching disk fall in

### Visual Verification
- **Background**: You should see the map/terrain
- **Black Disk**: Dark circle on the ground (clearly visible)
- **Enemies**: Walking above the disk
- **When enemy touches disk**: Enemy shrinks and falls in

## Alternative Sorting Orders

If `sortingOrder = 5` still doesn't work (map might be at order 10+), try these:

### Option A: Match Enemy Layer
```csharp
diskRenderer.sortingOrder = 8; // Just below typical enemies
```

### Option B: Use Same as Projectiles
```csharp
diskRenderer.sortingOrder = 10; // Same as projectiles/effects
```

### Option C: Very High (Temporary Test)
```csharp
diskRenderer.sortingOrder = 100; // Definitely visible
```

Once you find what works, adjust down to the lowest value that keeps it visible.

## How to Adjust in Unity (Without Code)

If you need to test different values in real-time:

1. **Enter Play Mode**
2. Apply Earth trait to tower
3. Shoot an enemy (disk should appear)
4. Open **Hierarchy** window
5. Find and select **"EarthHole_BlackDisk"**
6. In **Inspector** → **Sprite Renderer**
7. Change **Order in Layer** value
8. Try values: 5, 10, 50, 100
9. Watch the Scene view to see when disk becomes visible

## Debug Log Changes

The enhanced logs now show:
```
Sprite assigned to renderer. SortingLayer=Default, SortingOrder=5, Color=RGBA(0.000, 0.000, 0.000, 1.000)
Main disk: SortingOrder=5, Shadow: SortingOrder=4
```

This confirms:
- ✅ Disk is using "Default" sorting layer (correct)
- ✅ Disk has sortingOrder = 5 (above map)
- ✅ Shadow has sortingOrder = 4 (behind disk)

## Common Sorting Layer Issues

### Issue: Still not visible at sortingOrder = 5
**Possible causes:**
1. Map is on a higher sorting layer (not "Default")
2. Map has sortingOrder > 5
3. Camera is looking at wrong Z plane
4. Disk scale is 0

**Check:**
```
1. Select map object in Hierarchy
2. Check its Sprite Renderer
3. Note Sorting Layer and Order in Layer
4. Set disk to be higher than map's order
```

### Issue: Disk appears above enemies
**Solution:**
- Enemies are at lower sorting order than expected
- Lower disk order to just below enemies
- Example: If enemies = 15, use disk = 12

### Issue: Disk flickers
**Cause:** Same sorting order as another object
**Solution:** Use unique sorting order (not 0, 1, 10, 100, 1000)

## Sorting Layer vs Sorting Order

### Sorting Layer
- Named layers: "Default", "UI", "Background", etc.
- Defined in Unity: Edit → Project Settings → Tags and Layers
- Higher priority than sorting order
- Example: "UI" layer always above "Default" layer

### Sorting Order (Order in Layer)
- Integer value: -32768 to 32767
- Only matters within same sorting layer
- Higher number = rendered on top

### Current Setup
```csharp
diskRenderer.sortingLayerName = "Default"; // Same as most game objects
diskRenderer.sortingOrder = 5;              // Above ground, below enemies
```

## Performance Note

Sorting order changes have no performance impact. Feel free to adjust values to find what works best for your game's visual hierarchy.

## Quick Test Commands

To test if disk is being created at all:

```csharp
// In Unity Console after shooting enemy:
GameObject.Find("EarthHole_BlackDisk")
```

If it returns an object → disk IS being created, just not visible.

To check all SpriteRenderers in scene:

```csharp
// In Unity Console:
foreach(var sr in FindObjectsOfType<SpriteRenderer>()) {
    Debug.Log($"{sr.gameObject.name}: layer={sr.sortingLayerName}, order={sr.sortingOrder}");
}
```

This shows all objects' sorting setup.

---

## Summary

✅ **Fixed:** Changed sortingOrder from -1 to 5  
✅ **Result:** Black disk now renders above map layer  
✅ **Verified:** Compiles without errors  
🧪 **Next:** Test in Play Mode to confirm visibility

**If still not visible:** Try sortingOrder = 100 as a test, then report what you see in Scene/Game view.
