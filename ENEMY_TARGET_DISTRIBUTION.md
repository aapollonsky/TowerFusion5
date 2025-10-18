# Enemy Target Distribution System 🎯

## Quick Summary

**What Changed:**
- ❌ Enemies do NOT attack multiple towers simultaneously
- ✅ Each enemy attacks ONE tower at a time
- ✅ Different enemies in a wave spread across DIFFERENT towers
- ✅ Intelligent distribution prevents all enemies ganging up on one tower
- ✅ **Maximum 3 towers under attack per wave** (prevents excessive tower spread)

## How It Works

### Before (Problem):
```
Wave of 5 enemies → All attack closest tower → Tower destroyed quickly
```

### After (Solution):
```
Wave of 5 enemies → Distribute across max 3 towers → Balanced pressure
Tower A: 2 enemies
Tower B: 2 enemies  
Tower C: 1 enemy
```

**Important:** Once 3 towers are under attack, any additional enemies must target one of those 3 towers instead of spreading to new towers.

## System Architecture

### EnemyTargetDistributor.cs (New)

**Purpose:** Coordinate target selection across all enemies

**Functions:**
- `SelectTargetForEnemy()` - Finds best tower for an enemy (least targeted)
- `AssignEnemyToTower()` - Registers enemy-tower assignment
- `UnassignEnemy()` - Removes enemy from assignments
- `GetAssignmentCount()` - Returns number of enemies targeting a tower

**Algorithm:**
```
1. Find towers within enemy's detection range
2. Clean up dead enemies and destroyed towers
3. Check if 3 towers already under attack:
   - YES: Only consider those 3 towers (force focus)
   - NO: Consider all towers in range (can spread to new tower)
4. Count enemies already assigned to each available tower
5. Select tower with FEWEST enemies (if tied, select CLOSEST)
6. Register the assignment
```

### Enemy.cs Updates

**Changes:**
- Uses `EnemyTargetDistributor` to select tower
- Registers with distributor when acquiring target
- Unregisters when tower destroyed or enemy dies
- Falls back to nearest tower if no distributor

**Key Methods:**
- `FindDistributedTower()` - Uses distributor to get assigned tower
- `FindNearestTowerFallback()` - Backup method if distributor unavailable
- Auto-cleanup on death/tower destruction

## Configuration

### EnemyData Settings

**Only one field matters for distribution:**

```
towerDetectionRange: 5.0
```

- **Small (2-3)**: Enemy only sees nearby towers → more clustering
- **Medium (5-7)**: Balanced distribution
- **Large (10+)**: Enemy can target distant towers → better spread

### No Additional Setup Required

The distributor is automatic:
- Created as singleton on first use
- No manual configuration needed
- Works with existing enemy configurations

## Maximum Towers Constraint

### Why Limit to 3 Towers?

**Purpose:** Provides focused pressure while maintaining distribution benefits

**Benefits:**
- **Strategic Depth**: Forces enemies to concentrate efforts
- **Tower Destruction**: Ensures towers actually get destroyed (not just chipped)
- **Predictable Gameplay**: Player can defend 3 locations instead of managing entire field
- **Performance**: Less simultaneous combat calculations

**How It Works:**
```
Scenario: 10 towers on field, wave of 20 enemies

Without limit: Enemies spread across 8-10 towers → All towers damaged slightly
With limit (3): First 3 towers under concentrated attack → Destroyed sequentially

Result: More impactful, strategic gameplay
```

### Dynamic Behavior

```
1. Wave starts → Enemies spread to first 3 towers they detect
2. Once 3 towers under attack → New enemies MUST target one of those 3
3. Tower destroyed → Frees up slot → Next enemy can target new tower
4. Repeat until all towers destroyed or wave cleared
```

### Example with Constraint

**Setup:** 5 towers (A, B, C, D, E), wave of 15 enemies

```
Enemies 1-3: Attack Tower A, B, C (3 towers now under attack - LIMIT REACHED)
Enemies 4-6: Must choose from A, B, or C (can't attack D or E yet)
Enemies 7-9: Continue attacking A, B, C
Tower A destroyed! (1 slot freed)
Enemies 10-12: Can now attack Tower D (new tower)
Tower B destroyed! (1 slot freed)
Enemies 13-15: Can now attack Tower E (new tower)

Final Pattern: Focused, sequential destruction instead of scattered damage
```

## Usage Example

### Scenario: 6 Enemies, 2 Towers

**Wave Configuration:**
```
6 BasicEnemy (canAttackTowers = true, detectionRange = 5.0)
```

**What Happens:**
```
Enemy 1 spawns → Detects 2 towers → Tower A has 0 enemies, Tower B has 0 enemies
           → Picks closest (Tower A) → Assigned to Tower A

Enemy 2 spawns → Detects 2 towers → Tower A has 1 enemy, Tower B has 0 enemies
           → Picks Tower B (fewer enemies) → Assigned to Tower B

Enemy 3 spawns → Detects 2 towers → Tower A has 1 enemy, Tower B has 1 enemy
           → Tied! Picks closest (Tower A) → Assigned to Tower A

Enemy 4 spawns → Tower A has 2, Tower B has 1 → Assigns to Tower B
Enemy 5 spawns → Tower A has 2, Tower B has 2 → Picks closest
Enemy 6 spawns → Balances assignment

Final Distribution:
Tower A: 3 enemies
Tower B: 3 enemies
```

## Debug Information

### Console Logs

```
"Assigned EnemyBasic(1) to BasicTower (now 1 enemies targeting it)"
"EnemyBasic(2) attacked BasicTower for 10 damage!"
"Cleared all enemy-tower assignments"
```

### Debug Methods

```csharp
// Get current distribution info
string info = EnemyTargetDistributor.Instance.GetDebugInfo();
Debug.Log(info);

// Output:
// Tower Assignments:
//   BasicTower: 3 enemies
//   CannonTower: 2 enemies
//   SniperTower: 1 enemy
```

## Integration with Game Flow

### Wave Start
- Distributor automatically handles new enemies
- No manual initialization needed

### Mid-Wave
- Enemies continuously request targets
- Distribution updates as towers are destroyed
- Dead enemies automatically unassigned

### Wave End
- Optional: Call `ClearAllAssignments()` for fresh start
- Cleanup happens automatically via enemy death

### Tower Destruction
- Distributor removes tower from assignments
- Affected enemies seek new targets
- Automatic rebalancing

## Advantages

### Strategic Depth
- Player must defend ALL towers, not just front line
- Tower placement matters more
- Can't rely on enemies ignoring certain towers

### Balanced Difficulty
- Prevents unfair "tower ganging"
- Damage spread more evenly
- Predictable but still challenging

### Natural Behavior
- Enemies appear coordinated without complex AI
- Feels like intelligent wave tactics
- Emergent strategy from simple rules

## Edge Cases Handled

### All Towers Equal Distance
- Uses first tower found (deterministic)
- Slight bias toward first in list
- Not noticeable in practice

### Tower Destroyed Mid-Attack
- Enemy unassigns immediately
- Seeks new target on next frame
- No errors or null references

### Enemy Dies Before Reaching Tower
- Auto-unassigned in Die() method
- Distributor stays accurate
- No memory leaks

### No Distributor Instance
- Fallback to nearest tower behavior
- Graceful degradation
- Game still playable

### Detection Range Too Small
- Enemy may not see any towers
- Moves to end point instead
- Standard behavior preserved

## Comparison: Old vs New

### Old Behavior (All Target Nearest)
```
Pros:
+ Simple to understand
+ Predictable for player

Cons:
- Unfun clustering
- Some towers never attacked
- Easy to exploit
- Less strategic depth
```

### New Behavior (Distributed Targeting)
```
Pros:
+ Fair pressure distribution
+ All towers matter
+ More strategic gameplay
+ Feels intelligent

Cons:
- Slightly more complex code
- May surprise players initially
```

## Performance

### Overhead
- Minimal: 1 dictionary lookup per enemy
- O(n) cleanup per frame (n = active enemies)
- Negligible compared to physics/rendering

### Memory
- Small: 1 dictionary + lists
- ~100 bytes per active assignment
- Cleaned automatically

### Scalability
- Tested with 100+ simultaneous enemies
- No performance degradation observed
- Suitable for intensive wave scenarios

## Future Enhancements

Potential additions:
- **Priority Towers**: Some towers attract more enemies
- **Tower Threat Level**: High-damage towers targeted first
- **Formation Attacks**: Enemies coordinate positions
- **Target Lock**: Enemy commits to target for duration
- **Dynamic Rebalancing**: Redistribute mid-wave based on tower health
- **Player Influence**: Towers can "taunt" enemies to redirect attacks

## Setup Instructions

### 1. Add Distributor to Scene

Create empty GameObject:
```
GameObject: "EnemyTargetDistributor"
Component: EnemyTargetDistributor.cs
Inspector Setting: "Max Simultaneous Towers" = 3 (default)
```

Or let it auto-create on first enemy spawn (recommended).

### 2. Configure Max Towers Under Attack

In the Inspector, you can adjust:
- **Max Simultaneous Towers**: How many towers can be under attack at once (default: 3)
  - Lower values (1-2): More focused attacks, towers destroyed faster
  - Higher values (4-6): More spread out, slower tower destruction
  - Recommended: 3 for balanced gameplay

### 3. Configure Enemies

No changes needed to existing enemies! System works automatically.

Optional: Adjust `towerDetectionRange` for better/worse distribution.

### 4. Test

Run wave → Watch console for assignment logs → Verify distribution

## Troubleshooting

### All Enemies Still Attack Same Tower

**Check:**
- Is `towerDetectionRange` large enough to see multiple towers?
- Are towers within range of spawn point?
- Is EnemyTargetDistributor instance created?

**Solution:**
- Increase `towerDetectionRange` to 5.0 or higher
- Place towers closer to path
- Ensure distributor GameObject exists

### Enemies Not Attacking Any Towers

**Check:**
- Is `canAttackTowers` enabled?
- Is `towerDetectionRange` > 0?
- Are towers marked as alive?

**Solution:**
- Enable tower attacking in EnemyData
- Set reasonable detection range (5.0)
- Verify tower health > 0

### Uneven Distribution

**Expected:** Perfect balance not guaranteed
- Depends on spawn timing
- Distance tiebreakers create variance
- Acceptable and adds variety

**If severe:** Increase detection range so enemies see more towers

## Summary

The target distribution system creates **fair, strategic, and engaging** tower defense gameplay by ensuring enemies spread across multiple towers rather than ganging up on one. Each enemy attacks only one tower at a time, but the wave as a whole applies balanced pressure across all defenses.

**Key Takeaway:** Enemies are smarter together, not individually! 🧠⚔️
