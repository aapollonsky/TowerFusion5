# Per-Wave Assignment Limit Implementation

## Overview

The reactive defense system has been updated to implement a **per-wave assignment limit** for tower counterattackers. Each tower can be assigned up to 2 enemies to counterattack it during a wave. Once this limit is reached, the tower will not receive additional counterattackers until the next wave starts, even if the assigned enemies are killed.

## Key Behavior

### Assignment Rules
1. **Per-Wave Limit:** Each tower gets maximum 2 counterattackers per wave
2. **No Replacements:** If assigned enemies die, they are NOT replaced
3. **Wave Reset:** When a new wave starts, all tower counters reset to 0
4. **Per-Tower Tracking:** Each tower has its own independent counter

### Example Scenario

**Wave 1:**
```
Tower A fires → 2 enemies assigned to counterattack Tower A
├─ Enemy 1 killed → Tower A still has 2/2 assigned (no replacement)
└─ Enemy 2 killed → Tower A still has 2/2 assigned (no replacement)
    └─ Tower A fires again → No new enemies assigned (quota reached)

Tower B fires → 2 enemies assigned to counterattack Tower B
├─ Both enemies alive and attacking
└─ Tower B fires again → No new enemies assigned (quota reached)
```

**Wave 2 Starts:**
```
All tower counters reset → Each tower can receive 2 new counterattackers
Tower A fires → 2 new enemies assigned
Tower B fires → 2 new enemies assigned
```

## Implementation Details

### TowerDefenseCoordinator.cs

**Data Structures:**
```csharp
// Track currently active assignments
private Dictionary<Tower, List<Enemy>> towerAssignments;

// Track total enemies assigned per tower this wave (does not decrease when enemies die)
private Dictionary<Tower, int> totalAssignedCount;
```

**Key Methods:**

#### OnWaveStarted(int waveNumber)
Called when a new wave begins:
- Clears all assignment lists
- Resets all `totalAssignedCount` values to 0
- Logs reset message

```csharp
private void OnWaveStarted(int waveNumber)
{
    // Clear all assignments
    foreach (var tower in towerAssignments.Keys)
        towerAssignments[tower].Clear();
    
    // Reset all counters to 0
    foreach (var tower in totalAssignedCount.Keys.ToList())
        totalAssignedCount[tower] = 0;
}
```

#### OnTowerFired(Tower tower, Enemy targetEnemy)
Called when a tower fires:
- Checks if tower has reached its assignment limit (`totalAssignedCount >= 2`)
- If limit reached, logs message and returns (no new assignments)
- If limit not reached, finds eligible enemies and assigns them
- Increments `totalAssignedCount[tower]` for each assignment

```csharp
private void OnTowerFired(Tower tower, Enemy targetEnemy)
{
    int totalAssigned = totalAssignedCount[tower];
    
    if (totalAssigned >= MAX_COUNTERATTACKERS_PER_TOWER)
    {
        Debug.Log("Tower has already had 2 enemies assigned. No replacements.");
        return;
    }
    
    // Assign enemies and increment counter
    totalAssignedCount[tower]++;
}
```

#### UnassignEnemy(Enemy enemy)
Called when enemy dies or cancels counterattack:
- Removes enemy from assignment lists
- Cleans up dead enemies
- **Does NOT decrement `totalAssignedCount`** (this is the key!)
- Counter only resets at wave start

```csharp
public void UnassignEnemy(Enemy enemy)
{
    if (assignments.Remove(enemy))
    {
        assignments.RemoveAll(e => e == null || !e.IsAlive);
        
        // NOTE: Do NOT reset totalAssignedCount here!
        // Counter only resets when a new wave starts.
    }
}
```

### Enemy.cs Integration

Enemies unassign themselves from `TowerDefenseCoordinator` in two scenarios:

**1. When Dying:**
```csharp
private void Die()
{
    // ... other death logic ...
    
    if (TowerDefenseCoordinator.Instance != null)
    {
        TowerDefenseCoordinator.Instance.UnassignEnemy(this);
    }
}
```

**2. When Canceling Counterattack:**
```csharp
private void CancelCounterattack()
{
    if (TowerDefenseCoordinator.Instance != null)
    {
        TowerDefenseCoordinator.Instance.UnassignEnemy(this);
    }
    
    currentTowerTarget = null;
    behaviorState = EnemyBehaviorState.MovingToCorn;
}
```

## Design Rationale

### Why Per-Wave Limit?

1. **Strategic Depth:** Players must consider when to place/upgrade towers
2. **Wave Pacing:** Ensures each wave has distinct counterattack opportunities
3. **Predictability:** Players can anticipate max 2 enemies per tower per wave
4. **Balance:** Prevents overwhelming tower with unlimited counterattackers

### Why No Replacements?

1. **Player Skill Reward:** Killing counterattackers provides lasting benefit
2. **Tower Survivability:** Towers can outlast their attackers
3. **Resource Management:** Players decide whether to defend towers or let them fall
4. **Clear Rules:** Simple to understand and explain

### Why Reset Each Wave?

1. **Fresh Start:** Each wave provides new tactical opportunities
2. **Long-Term Strategy:** Towers that survive gain value in future waves
3. **Difficulty Scaling:** Later waves can have more enemies, more opportunities
4. **Clear Boundaries:** Wave structure provides natural reset points

## Testing Scenarios

### Scenario 1: Normal Assignment
```
Wave 1 starts
Tower fires → Enemy A assigned (1/2)
Tower fires → Enemy B assigned (2/2)
Tower fires → No assignment (quota reached)
✓ Expected: Max 2 enemies assigned
```

### Scenario 2: Enemy Death
```
Wave 1 starts
Tower fires → Enemy A, B assigned (2/2)
Player kills Enemy A
Tower fires → No assignment (still 2/2)
✓ Expected: No replacement enemy assigned
```

### Scenario 3: Wave Reset
```
Wave 1: Tower gets 2 enemies assigned
Wave 1 ends, Wave 2 starts
Tower fires → Enemy C assigned (1/2)
✓ Expected: Counter reset, new assignments possible
```

### Scenario 4: Multiple Towers
```
Wave 1 starts
Tower A fires → 2 enemies assigned to Tower A (Tower A: 2/2)
Tower B fires → 2 enemies assigned to Tower B (Tower B: 2/2)
✓ Expected: Each tower has independent counter
```

### Scenario 5: Tower Destroyed
```
Wave 1: Tower A gets 2 enemies assigned
Enemies destroy Tower A
Tower A removed from tracking
✓ Expected: No memory leaks, clean removal
```

## Console Log Examples

### Wave Start
```
TowerDefenseCoordinator: New wave 2 started - resetting all tower assignment counters
TowerDefenseCoordinator: All towers can now receive up to 2 new counterattackers this wave
```

### Assignment
```
TowerDefenseCoordinator: Basic Tower fired at Goblin_03!
TowerDefenseCoordinator: Assigned Goblin_05 to counterattack Basic Tower (total assigned: 1/2)
TowerDefenseCoordinator: Assigned Goblin_07 to counterattack Basic Tower (total assigned: 2/2)
```

### Quota Reached
```
TowerDefenseCoordinator: Basic Tower fired at Goblin_10!
TowerDefenseCoordinator: Basic Tower has already had 2 enemies assigned (max 2). No replacements.
```

### Enemy Death
```
TowerDefenseCoordinator: Unassigned Goblin_05 from Basic Tower
[Enemy does not reset counter]
```

## Performance Considerations

### Memory Usage
- `towerAssignments`: O(T × E) where T = towers, E = enemies per tower (max 2)
- `totalAssignedCount`: O(T) where T = number of towers
- Both dictionaries scale linearly with tower count

### Computational Cost
- Wave reset: O(T) - iterate all towers once
- Assignment check: O(1) - dictionary lookup
- Unassignment: O(T) - worst case iterate all towers

### Event Subscriptions
- Subscribe to `WaveManager.OnWaveStarted` once in Start()
- Subscribe to each tower's `OnTowerFired` event when tower registered
- Proper cleanup in OnDestroy() prevents memory leaks

## Related Systems

### WaveManager
Provides `OnWaveStarted` event that triggers counter reset:
```csharp
public System.Action<int> OnWaveStarted;
```

### Enemy State Machine
Six states with counterattack integration:
1. MovingToCorn
2. GrabbingCorn
3. WaitingForCorn
4. ReturningWithCorn
5. ReturningEmpty
6. CounterattackingTower ← Assigned enemies enter this state

### Tower System
Fires `OnTowerFired` event before dealing damage:
```csharp
public System.Action<Tower, Enemy> OnTowerFired;
```

## Future Enhancements

### Potential Improvements
- [ ] **Difficulty Scaling:** Increase max counterattackers in later waves (3, 4, 5...)
- [ ] **Tower Traits:** Some towers could have +1 counterattack limit
- [ ] **Enemy Abilities:** Some enemies could "not count" toward limit
- [ ] **Wave Modifiers:** Special waves with different assignment rules

### Debugging Tools
- [ ] UI showing current assignments per tower (1/2, 2/2, etc.)
- [ ] Visual indicators above towers showing assignment status
- [ ] Debug mode showing assignment history per wave

## References

- Reactive Defense Design: `docs/REACTIVE_DEFENSE_SYSTEM.md`
- Trait Integration: `docs/TRAIT_REACTIVE_DEFENSE_INTEGRATION.md`
- GitHub Copilot Instructions: `.github/copilot-instructions.md`
