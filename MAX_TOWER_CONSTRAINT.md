# Maximum Tower Constraint (3 Tower Limit)

## Quick Reference

**Rule:** A wave can attack **maximum 3 towers simultaneously**

**Location:** `EnemyTargetDistributor.cs` → `maxSimultaneousTowers` field (default: 3)

---

## Visual Examples

### Example 1: Small Wave (5 enemies, 3 towers available)

```
Wave starts:
┌─────────┐  ┌─────────┐  ┌─────────┐
│Tower A  │  │Tower B  │  │Tower C  │
└─────────┘  └─────────┘  └─────────┘
    ↑            ↑            ↑
    │            │            │
Enemy 1      Enemy 2      Enemy 3
Enemy 4      Enemy 5

Result: Only 3 towers attacked (within limit)
Distribution: Tower A=2, Tower B=2, Tower C=1
```

### Example 2: Large Wave (15 enemies, 6 towers available)

```
Phase 1 - First 3 enemies spread:
┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐
│Tower A  │  │Tower B  │  │Tower C  │  │Tower D  │  │Tower E  │  │Tower F  │
└─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘
    ↑            ↑            ↑
Enemy 1      Enemy 2      Enemy 3

Phase 2 - Limit reached! Enemies 4-9 forced to A/B/C:
┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐
│Tower A  │  │Tower B  │  │Tower C  │  │Tower D  │  │Tower E  │  │Tower F  │
└─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘
    ↑            ↑            ↑            X            X            X
Enemy 1      Enemy 2      Enemy 3     (locked)     (locked)     (locked)
Enemy 4      Enemy 5      Enemy 6
Enemy 7      Enemy 8      Enemy 9

Phase 3 - Tower A destroyed! Slot freed:
┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐
│DESTROYED│  │Tower B  │  │Tower C  │  │Tower D  │  │Tower E  │  │Tower F  │
└─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘
     X           ↑            ↑            ↑            X            X
            Enemy 5      Enemy 6      Enemy 10    (locked)     (locked)
            Enemy 8      Enemy 9      Enemy 11
                                      Enemy 12

Final Result: Towers destroyed sequentially (A → B → D) instead of all 6 damaged
```

---

## Configuration

### Adjust in Unity Inspector

1. Select `EnemyTargetDistributor` GameObject
2. Find `Max Simultaneous Towers` field
3. Change value (default: 3)

### Balance Guide

| Value | Effect | Gameplay Impact |
|-------|--------|-----------------|
| 1 | All enemies attack ONE tower | Very focused, towers destroyed extremely fast |
| 2 | Enemies split between TWO towers | Focused but some distribution |
| **3** | **Enemies spread across THREE towers** | **Balanced (recommended)** |
| 4-5 | More spread out | Slower tower destruction, more defensive management |
| 10+ | Essentially unlimited | Enemies spread thin, towers rarely destroyed |

---

## Algorithm Flow

```
START: Enemy needs target

1. Find towers in detection range
   └─> If none found → Move to end point
   
2. Clean up dead enemies & destroyed towers

3. Count towers currently under attack
   └─> If >= 3 towers under attack:
       ├─> Filter: Only consider those 3 towers
       └─> New towers LOCKED OUT
       
   └─> If < 3 towers under attack:
       └─> Can target any tower in range
       
4. Select tower with FEWEST enemies
   └─> Tie-breaker: Choose CLOSEST
   
5. Assign enemy to selected tower

END
```

---

## Testing Commands

### In Enemy.cs (for debugging)

```csharp
// See current distribution
Debug.Log(EnemyTargetDistributor.Instance.GetDebugInfo());

// Output example:
// Tower Assignments:
//   BasicTower: 3 enemies
//   CannonTower: 2 enemies
//   SniperTower: 1 enemy
// (Only 3 towers listed because of max constraint)
```

### Watch Console During Wave

Look for logs like:
```
"Assigned EnemyBasic(4) to BasicTower (now 3 enemies targeting it)"
"Assigned EnemyBasic(5) to BasicTower (now 4 enemies targeting it)"
^ Notice same tower getting more enemies because limit reached
```

---

## Common Scenarios

### Scenario: More towers than limit

**Setup:** 10 towers on field, maxSimultaneousTowers = 3

**Result:** Only 3 towers attacked at once. Others safe until one of the 3 is destroyed.

**Player Strategy:** Focus defense on those 3 towers, let others generate resources.

---

### Scenario: Fewer towers than limit

**Setup:** 2 towers on field, maxSimultaneousTowers = 3

**Result:** Both towers attacked (under limit). Constraint not active.

**Behavior:** Normal distribution between 2 towers.

---

### Scenario: Tower destroyed mid-wave

**Setup:** 3 towers under attack (limit reached), Tower A destroyed

**Result:** 
- Enemies previously on Tower A seek new target
- Now only 2 towers under attack (below limit)
- Next enemy can attack a NEW 3rd tower
- Limit re-applies once 3 towers targeted again

**Pattern:** Creates sequential destruction cascade

---

## Performance Impact

**Overhead:** Negligible
- Added one integer comparison per target selection
- No additional data structures
- O(1) check: `if (towersUnderAttack.Count >= maxSimultaneousTowers)`

**Memory:** None
- Uses existing towerAssignments dictionary
- No new allocations

---

## Why This Improves Gameplay

### Without Constraint (Old Behavior)
```
Problem: 20 enemies spread across 10 towers
→ Each tower takes 20 damage over time
→ No towers destroyed
→ Player slowly loses all towers to chip damage
→ Unfun, defensive stalemate
```

### With Constraint (New Behavior)
```
Solution: 20 enemies focus on 3 towers at a time
→ Tower A destroyed quickly
→ Enemies move to Tower D (new target)
→ Tower B destroyed
→ Enemies move to Tower E
→ Sequential destruction, strategic defense, exciting gameplay!
```

---

## Related Files

- `EnemyTargetDistributor.cs` - Contains `maxSimultaneousTowers` field
- `Enemy.cs` - Uses distributor for target selection
- `ENEMY_TARGET_DISTRIBUTION.md` - Full system documentation

---

## Quick Start

1. **Do nothing!** Default value of 3 is balanced
2. **To test:** Change value in Inspector and run wave
3. **To disable:** Set to very high number (999) for unlimited spread

**Recommendation:** Keep at 3 unless you have specific design reasons to change it.
