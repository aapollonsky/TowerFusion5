# Quick Reference: New Enemy Behavior (No More MovingToEnd)

## Enemy States (Updated)

### Available States
1. **SeekingTower** - Attacker looking for a tower to attack
2. **AttackingTower** - Attacker actively attacking a tower
3. **MovingToCorn** - Stealer moving to corn pile
4. **GrabbingCorn** - Stealer picking up corn (animation delay)
5. **ReturningWithCorn** - Stealer returning to spawn with corn

### Removed State
- ~~**MovingToEnd**~~ - No longer exists (enemies don't path to end)

---

## Enemy Flow Charts

### Attacker Flow
```
┌─────────┐
│  SPAWN  │
└────┬────┘
     │
     ▼
┌──────────────┐
│ SeekingTower │ ◄───────────────┐
└──────┬───────┘                 │
       │                         │
       │ (tower found)           │ (target lost)
       ▼                         │
┌─────────────────┐              │
│ AttackingTower  │──────────────┘
└──────┬──────────┘
       │
       │ (tower destroyed)
       ▼
┌──────────────────┐
│ Convert to       │
│ Stealer          │
└──────┬───────────┘
       │
       ▼
  [Stealer Flow]
```

### Stealer Flow
```
┌─────────┐
│  SPAWN  │
└────┬────┘
     │
     ▼
┌──────────────┐
│ MovingToCorn │
└──────┬───────┘
       │
       │ (reached corn)
       ▼
┌──────────────┐
│ GrabbingCorn │
└──────┬───────┘
       │
       │ (grabbed corn)
       ▼
┌────────────────────┐
│ ReturningWithCorn  │
└────────┬───────────┘
         │
         │ (reached spawn)
         ▼
    ┌─────────┐
    │ SUCCESS │ → Player loses corn
    └─────────┘
         OR
    ┌─────────┐
    │ KILLED  │ → Drop corn, returns to storage
    └─────────┘
```

---

## Key Behavior Changes

### What Changed

| Scenario | OLD Behavior | NEW Behavior |
|----------|-------------|--------------|
| Attacker destroys tower | Look for another tower OR move to end | Convert to stealer |
| Attacker finds no towers | Move to end | Convert to stealer |
| No corn available | Move to end | Die immediately |
| Lost tower target | Move to end | Clear target, seek new tower |

### What Stayed The Same

- Stealers always go for corn (unchanged)
- Attackers attack towers (unchanged)
- Tower destruction triggers conversion (unchanged)
- Corn drop on death (unchanged)

---

## Game Objective

**Goal:** Prevent enemies from stealing ALL the corn

**How Enemies Win:**
1. Attackers destroy all towers (or find no towers)
2. All attackers convert to stealers
3. Stealers grab all corn
4. Stealers return all corn to spawn
5. `CornManager` detects `RemainingCorn == 0`
6. Game Over - Player Loses

**How Players Win:**
1. Kill enemies before they steal all corn
2. Complete all waves with corn remaining
3. `GameManager.EndWave()` detects last wave complete
4. Game Over - Player Wins

---

## Common Scenarios

### Scenario 1: Tower Near Spawn
```
Enemy spawns → Detects nearby tower → Attacks tower → Destroys tower
→ Converts to stealer → Goes to corn → Grabs corn → Returns to spawn
```

### Scenario 2: Tower Near Corn Pile
```
Enemy spawns → Finds no towers in range → Converts to stealer
→ Goes to corn → Finds tower on the way → IGNORES IT (is stealer)
→ Grabs corn → Returns to spawn
```

Wait, that's wrong! Let me fix this...

### Scenario 2: Tower Near Corn Pile (CORRECT)
```
Enemy spawns as Attacker → Seeks towers → Detects tower near corn
→ Attacks tower → Destroys tower → Converts to stealer
→ Grabs corn → Returns to spawn
```

### Scenario 3: No Towers
```
Enemy spawns as Attacker → Finds no towers → Converts to stealer
→ Goes to corn → Grabs corn → Returns to spawn
```

### Scenario 4: Killed While Carrying Corn
```
Stealer grabs corn → Returns to spawn → Player tower kills stealer
→ Corn drops → `CornManager` returns corn to storage automatically
```

---

## Code Locations

### Main Changes
- `Assets/Scripts/Enemy/Enemy.cs` - Line 53 (enum), lines 180-190, 285-298, 340-355, 370-383

### Key Methods
- `ConvertToStealer()` - Converts attacker to stealer
- `UpdateTowerTargeting()` - Looks for towers, converts if none found
- `HandleTowerBehavior()` - Handles attacker states
- `Die()` - Handles death (no more ReachEnd)

### Deprecated Methods
- `MoveAlongPath()` - No longer called
- `ReachEnd()` - No longer called

---

## Debugging

### Console Messages to Watch For

**Normal Flow:**
```
"Spawned Enemy_X as Attacker (roll: 0.75, threshold: 0.15)"
"Enemy_X role set to ATTACKER"
"Enemy_X found tower Tower_Y at distance 5.2"
"Enemy_X found no towers, converting to stealer"
"Enemy_X role set to STEALER"
```

**Abnormal Flow:**
```
"Enemy_X tower destroyed but no corn available, dying"  ← Should be rare
```

### Common Issues

**Problem:** Enemies walk past towers
**Cause:** `canAttackTowers = false` in EnemyData
**Fix:** Set `canAttackTowers = true` in Inspector

**Problem:** Enemies die immediately after spawning
**Cause:** No towers found AND no corn available
**Fix:** Ensure either towers exist OR corn is available

**Problem:** Game never ends
**Cause:** Enemies stuck in SeekingTower loop
**Fix:** Check tower detection range and tower placement

---

## Testing Checklist

### Basic Flow
- [ ] Attackers spawn and seek towers ✓
- [ ] Attackers attack and destroy towers ✓
- [ ] Attackers convert to stealers after tower destroyed ✓
- [ ] Stealers go directly to corn ✓
- [ ] Stealers return with corn to spawn ✓

### Edge Cases
- [ ] Attacker spawns with no towers nearby → converts to stealer
- [ ] All towers destroyed → all attackers become stealers
- [ ] Last corn stolen → player loses
- [ ] All waves complete with corn remaining → player wins
- [ ] Stealer killed while carrying corn → corn returns

### No Errors
- [ ] No `MovingToEnd` errors in console
- [ ] No null reference exceptions
- [ ] No stuck enemies

---

## Quick Tips

1. **Enemies now ALWAYS have a purpose:**
   - Attack towers OR steal corn (never just "moving to end")

2. **Tower placement matters more:**
   - Towers near spawn: Delay attackers
   - Towers near corn: Last line of defense

3. **No health damage from reaching end:**
   - Old tower defense style removed
   - Only lose by corn theft

4. **Enemy conversion is automatic:**
   - When tower destroyed → convert
   - When no towers found → convert
   - When no corn available → die

5. **Corn is the ONLY objective:**
   - Defend it at all costs
   - Game revolves around corn theft
   - Everything else supports this goal

---

## Summary

**Removed:** `MovingToEnd` state (enemies no longer path to end)  
**Added:** Automatic conversion to stealers when no tower targets  
**Result:** Clearer game objective focused on corn defense  

**Key Insight:** Enemies are now either "tower destroyers" (attackers) or "corn thieves" (stealers) - nothing else!
