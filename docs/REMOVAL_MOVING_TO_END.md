# Removal of MovingToEnd State

## Overview
Removed the `MovingToEnd` behavior state from the enemy AI system as it no longer aligns with the corn theft game objective.

## Change Date
October 19, 2025

## Reason for Change
The game's objective is for enemies to **steal all the corn**, not to reach the end of the path. The `MovingToEnd` state was a holdover from traditional tower defense mechanics where enemies damage the player's base by reaching the end.

## What Changed

### Removed State
- `EnemyBehaviorState.MovingToEnd` - Completely removed from the enum

### New Enemy Flow

**Attackers:**
```
Spawn → SeekingTower → AttackingTower → (tower destroyed) → Convert to Stealer
                                      ↓
                              (no towers found) → Convert to Stealer
```

**Stealers:**
```
Spawn → MovingToCorn → GrabbingCorn → ReturningWithCorn → (success)
                                                         ↓
                                                  (killed) → Drop corn
```

### Code Changes

#### Enemy.cs

**Enum Definition:**
```csharp
// OLD:
private enum EnemyBehaviorState { MovingToEnd, SeekingTower, AttackingTower, MovingToCorn, GrabbingCorn, ReturningWithCorn }

// NEW:
private enum EnemyBehaviorState { SeekingTower, AttackingTower, MovingToCorn, GrabbingCorn, ReturningWithCorn }
```

**ConvertToStealer() - When no corn available:**
```csharp
// OLD:
if (CornManager.Instance == null || CornManager.Instance.RemainingCorn <= 0)
{
    behaviorState = EnemyBehaviorState.MovingToEnd;
    Debug.Log($"{name} tower destroyed but no corn available, moving to end");
    return;
}

// NEW:
if (CornManager.Instance == null || CornManager.Instance.RemainingCorn <= 0)
{
    Debug.Log($"{name} tower destroyed but no corn available, dying");
    Die();
    return;
}
```

**Update() - Removed MovingToEnd case:**
```csharp
// OLD:
case EnemyBehaviorState.SeekingTower:
case EnemyBehaviorState.AttackingTower:
case EnemyBehaviorState.MovingToEnd:
    if (enemyData.canAttackTowers)
    {
        UpdateTowerTargeting();
        HandleTowerBehavior();
    }
    else
    {
        MoveAlongPath();
    }
    break;

// NEW:
case EnemyBehaviorState.SeekingTower:
case EnemyBehaviorState.AttackingTower:
    if (enemyData.canAttackTowers)
    {
        UpdateTowerTargeting();
        HandleTowerBehavior();
    }
    else
    {
        ConvertToStealer();
    }
    break;
```

**UpdateTowerTargeting() - Convert to stealer when no towers:**
```csharp
// OLD:
else
{
    // No towers found, head to end point
    behaviorState = EnemyBehaviorState.MovingToEnd;
}

// NEW:
else
{
    // No towers found, convert to stealer
    Debug.Log($"{name} found no towers, converting to stealer");
    ConvertToStealer();
}
```

**HandleTowerBehavior() - SeekingTower case:**
```csharp
// OLD:
else
{
    behaviorState = EnemyBehaviorState.MovingToEnd;
}

// NEW:
else
{
    // Lost tower target, look for another or convert to stealer
    currentTowerTarget = null;
}
```

**HandleTowerBehavior() - Removed MovingToEnd case:**
```csharp
// OLD:
case EnemyBehaviorState.MovingToEnd:
    MoveAlongPath();
    break;

// NEW:
// (case removed entirely)
```

### Deprecated Methods

The following methods are now **deprecated** but kept for backwards compatibility:

#### `MoveAlongPath()`
- **Status:** No longer called in corn theft mode
- **Reason:** Enemies don't follow paths to the end anymore
- **Kept because:** May be useful for future game modes or testing

#### `ReachEnd()`
- **Status:** No longer called in corn theft mode
- **Reason:** Enemies don't reach the end - they steal corn instead
- **Kept because:** Referenced in comments, may be useful for debugging

## Game Logic Impact

### What Happens Now When...

**All towers are destroyed:**
- All remaining attackers convert to stealers
- They go to corn pile, grab corn, return to spawn
- Game ends when all corn is stolen

**No towers exist from the start:**
- Attackers spawn, find no towers
- Immediately convert to stealers
- All enemies rush corn pile

**Attacker destroys their tower:**
- Converts to stealer via `ConvertToStealer()`
- Heads to corn pile

**No corn left:**
- Attackers with destroyed tower targets call `Die()`
- Attackers seeking towers continue seeking (won't find any if all destroyed)
- Game ends with player victory

### Win/Loss Conditions

**Player Loses:**
- All corn successfully stolen and returned to spawn
- Checked in `CornManager.OnCornSuccessfullyStolen()`

**Player Wins:**
- All waves completed AND corn remaining > 0
- Checked in `GameManager.EndWave()` when `currentWave > totalWaves`

## Testing Checklist

After this change, verify:

- [ ] Attackers spawn and seek towers
- [ ] Attackers attack towers correctly
- [ ] When tower destroyed, attacker converts to stealer
- [ ] When no towers found, attacker converts to stealer
- [ ] Stealers spawn and go directly to corn
- [ ] Stealers grab corn and return to spawn
- [ ] When all corn stolen, player loses
- [ ] When all waves complete with corn remaining, player wins
- [ ] No errors about `MovingToEnd` state
- [ ] Enemies never use the path system anymore (unless converted to stealers who navigate directly)

## Console Messages

**New debug messages:**
```
"[Enemy_X] found no towers, converting to stealer"
"[Enemy_X] tower destroyed but no corn available, dying"
```

**Removed messages:**
```
"[Enemy_X] tower destroyed but no corn available, moving to end"
```

## Migration Notes

**For developers:**
- If you have custom enemy behaviors that reference `MovingToEnd`, they will break
- Replace with either `SeekingTower` or `ConvertToStealer()` call
- Path following is now only indirect (stealers navigate to corn/spawn)

**For game designers:**
- Enemies will NEVER damage the player's health by reaching the end
- The only way to lose is by corn theft
- Balance tower strength and count accordingly

## Related Systems

**Affected:**
- `Enemy.cs` - Main changes
- `EnemyManager.cs` - No longer receives `OnEnemyReachedEnd` events from normal gameplay
- `GameManager.cs` - No longer tracks health loss from enemies reaching end

**Unaffected:**
- `CornManager.cs` - Still handles corn theft
- `WaveManager.cs` - Still spawns enemies with roles
- `TowerManager.cs` - Still handles tower destruction
- `EnemyTargetDistributor.cs` - Still distributes targets

## Future Considerations

### Potential New Game Modes

If you want to add a "classic tower defense" mode in the future:

1. Add a `gameMode` enum in `GameManager`:
```csharp
public enum GameMode { CornTheft, ClassicTowerDefense }
public GameMode currentGameMode = GameMode.CornTheft;
```

2. Re-enable `MovingToEnd` state conditionally:
```csharp
if (currentGameMode == GameMode.ClassicTowerDefense)
{
    behaviorState = EnemyBehaviorState.MovingToEnd;
}
else
{
    ConvertToStealer();
}
```

3. Keep the deprecated methods for this purpose

### Alternative Design

Could have kept `MovingToEnd` but had it behave like "stealer when no corn":
- Pro: Less code changes
- Con: Confusing state name that doesn't match behavior
- Decision: Clean removal is better for code clarity

## Summary

**Before:** Enemies could move to the end of the path and damage the player
**After:** Enemies only attack towers or steal corn - no path following to end
**Result:** Simpler, clearer code that matches the corn theft game objective

This change makes the codebase more maintainable and aligns the implementation with the game's actual objective: **defend your corn storage from thieving enemies!**
