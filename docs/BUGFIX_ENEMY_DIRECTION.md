# Bug Fix: Enemy Sprite Direction

## Issue
Enemies don't always face their current destination. Specifically:
- Enemies attacking towers might face the wrong direction
- Enemies grabbing corn might face the wrong direction from their approach
- Sprite orientation doesn't match movement intent

## Root Cause
The sprite direction update logic was missing in stationary states:

1. **AttackingTower state:** Enemy stops moving to attack, but sprite direction wasn't updated to face the tower
2. **GrabbingCorn state:** Enemy stops to grab corn, but sprite direction wasn't updated to face corn storage

The movement states (`MovingToEnd`, `SeekingTower`, `MovingToCorn`, `ReturningWithCorn`) all properly updated sprite direction, but the stationary action states did not.

## The Fix

### 1. AttackingTower State
Added direction update to continuously face the tower while attacking.

**Before:**
```csharp
case EnemyBehaviorState.AttackingTower:
    if (currentTowerTarget != null && currentTowerTarget.IsAlive)
    {
        TryAttackTower();  // ❌ No direction update!
        
        // Check if still in range...
    }
    break;
```

**After:**
```csharp
case EnemyBehaviorState.AttackingTower:
    if (currentTowerTarget != null && currentTowerTarget.IsAlive)
    {
        // ✅ Face the tower while attacking
        Vector3 directionToTower = (currentTowerTarget.Position - transform.position).normalized;
        if (directionToTower.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(directionToTower.y, directionToTower.x) * Mathf.Rad2Deg;
            angle = -angle; // Flip Y axis
            currentMovementAngle = angle;
            UpdateDirectionalSprite(currentMovementAngle);
        }
        
        TryAttackTower();
        
        // Check if still in range...
    }
    break;
```

### 2. GrabbingCorn State
Added direction update to face corn storage while grabbing.

**Before:**
```csharp
private void GrabCorn()
{
    cornGrabTimer += Time.deltaTime;  // ❌ No direction update!
    
    if (cornGrabTimer >= enemyData.cornGrabDuration)
    {
        // Grab corn...
    }
}
```

**After:**
```csharp
private void GrabCorn()
{
    // ✅ Face the corn storage while grabbing
    if (CornManager.Instance != null)
    {
        Vector3 cornPosition = CornManager.Instance.GetCornStoragePosition();
        Vector3 directionToCorn = (cornPosition - transform.position).normalized;
        
        if (directionToCorn.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(directionToCorn.y, directionToCorn.x) * Mathf.Rad2Deg;
            angle = -angle; // Flip Y axis
            currentMovementAngle = angle;
            UpdateDirectionalSprite(currentMovementAngle);
        }
    }
    
    cornGrabTimer += Time.deltaTime;
    
    if (cornGrabTimer >= enemyData.cornGrabDuration)
    {
        // Grab corn...
    }
}
```

## State-by-State Direction Update Summary

| Behavior State | Movement | Direction Update | Status |
|---------------|----------|------------------|--------|
| **SeekingTower** | ✅ Moving to tower | ✅ Via `MoveTowardsTower()` → `UpdateMovementDirection()` | Working |
| **AttackingTower** | ❌ Stationary | ✅ **FIXED** - Now faces tower | **Fixed** |
| **MovingToEnd** | ✅ Following path | ✅ Via `MoveAlongPath()` → `UpdateMovementDirection()` | Working |
| **MovingToCorn** | ✅ Moving to storage | ✅ Via direction calculation in method | Working |
| **GrabbingCorn** | ❌ Stationary | ✅ **FIXED** - Now faces storage | **Fixed** |
| **ReturningWithCorn** | ✅ Moving to spawn | ✅ Via direction calculation in method | Working |

## What This Fixes

✅ **Attackers face their target tower** while attacking
- Previously: Might face wrong direction from last movement
- Now: Always faces the tower being attacked

✅ **Stealers face corn storage** while grabbing
- Previously: Might face approach direction or random direction
- Now: Always faces the corn storage during grab animation

✅ **Improved visual polish**
- Enemy orientation always matches their current action/intent
- No more enemies attacking/grabbing while facing sideways

✅ **Consistent behavior across all states**
- All states now update sprite direction appropriately
- Movement states: Face movement direction
- Action states: Face action target

## Direction Calculation Pattern

All direction updates now follow this consistent pattern:

```csharp
// 1. Calculate direction to target
Vector3 direction = (targetPosition - transform.position).normalized;

// 2. Check if direction is valid
if (direction.magnitude > 0.1f)
{
    // 3. Convert to angle (0° = right, 90° = down in Unity Y-down)
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    
    // 4. Flip Y axis for Unity's coordinate system
    angle = -angle;
    
    // 5. Update current angle and sprite
    currentMovementAngle = angle;
    UpdateDirectionalSprite(currentMovementAngle);
}
```

This pattern is now used in:
- `MoveTowardsCornStorage()` - Face corn while moving
- `GrabCorn()` - **NEW** - Face corn while grabbing
- `ReturnToSpawn()` - Face spawn while returning
- `HandleTowerBehavior()` → `AttackingTower` - **NEW** - Face tower while attacking

## Performance Impact

**Minimal:** The direction calculation is very lightweight:
- Vector subtraction: O(1)
- Normalization: O(1)
- Atan2 calculation: O(1)
- Sprite update: Already happening in movement states

Added calculations run only for enemies in stationary action states (small subset of all enemies).

## Testing Verification

After this fix, verify:

1. **Attacking Enemies:**
   - Approach tower from any direction
   - Stop at attack range
   - ✅ **Should face the tower** while attacking
   - Move away if tower destroyed
   - ✅ **Should face movement direction** again

2. **Stealing Enemies:**
   - Approach corn storage from any direction
   - Stop to grab corn (1 second delay)
   - ✅ **Should face corn storage** during grab
   - Run back to spawn with corn
   - ✅ **Should face spawn direction** while returning

3. **Direction Smoothness:**
   - No sudden snapping or jittering
   - Smooth transitions between states
   - Consistent orientation throughout actions

## Edge Cases Handled

✅ **Null checks:** Direction calculation only happens if target exists
✅ **Magnitude check:** Direction only updated if valid (> 0.1f)
✅ **Coordinate system:** Y-axis flip accounts for Unity's coordinate system
✅ **State transitions:** Direction naturally updates when switching states

## Related Files Modified

- `Assets/Scripts/Enemy/Enemy.cs`
  - Method: `HandleTowerBehavior()` - Line ~322
    - Added direction calculation in `AttackingTower` case
  - Method: `GrabCorn()` - Line ~800
    - Added direction calculation at start of method

## Before & After Examples

### Example 1: Attacking Tower from Below
**Before:**
```
Enemy approaches tower from below (facing up ⬆️)
Stops at attack range
Still facing up ⬆️ while attacking tower above
❌ Direction doesn't match intent
```

**After:**
```
Enemy approaches tower from below (facing up ⬆️)
Stops at attack range
Updates to face tower ⬆️ while attacking
✅ Direction matches action
```

### Example 2: Stealing Corn from Right Side
**Before:**
```
Enemy approaches corn from right (facing left ⬅️)
Stops to grab corn
Might face left ⬅️ or random direction during grab
❌ Direction doesn't match action
```

**After:**
```
Enemy approaches corn from right (facing left ⬅️)
Stops to grab corn
Updates to face corn storage ⬅️ during grab
✅ Direction matches action
```

## Future Enhancements

While this fix addresses the immediate issue, future improvements could include:

1. **Smooth rotation:** Lerp/Slerp rotation instead of instant snap
2. **Attack animations:** Specific attack directional sprites
3. **Grab animations:** Specific grab/pickup directional sprites
4. **Anticipation:** Turn toward target slightly before reaching it

These would require animation system changes and are beyond the scope of this bug fix.

---

**Status:** ✅ FIXED (as of October 18, 2025)

**Priority:** Medium - Visual polish issue, not gameplay-breaking

**Impact:** Improves visual consistency and player perception of enemy behavior
