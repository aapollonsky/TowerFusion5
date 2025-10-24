# Per-Wave Assignment Limit Implementation

## Overview

The reactive defense system implements **per-wave assignment limits** for tower counterattackers with two configurable constraints:

1. **Per-Tower Limit:** Each tower can be assigned up to N enemies to counterattack it during a wave (default: 2)
2. **Per-Wave Tower Limit:** Maximum M towers can be under counterattack per wave (default: 2, 0 = unlimited)

Once these limits are reached, towers will not receive additional counterattackers until the next wave starts, even if assigned enemies are killed.

## Configuration

### Inspector Settings

In the Unity Inspector, `TowerDefenseCoordinator` has two configurable fields:

- **Max Counterattackers Per Tower** (default: 2)
  - How many enemies each tower can be assigned per wave
  - Once reached, that tower gets no more counterattackers this wave
  
- **Max Towers Under Attack Per Wave** (default: 2)
  - Maximum number of towers that can receive counterattack assignments per wave
  - Set to 0 for unlimited towers
  - First N towers to fire get counterattackers, rest are protected

## Key Behavior

### Assignment Rules
1. **Per-Tower Limit:** Each tower gets maximum N counterattackers per wave (default: 2)
2. **Per-Wave Tower Limit:** Maximum M towers can be under attack per wave (default: 2)
3. **No Replacements:** If assigned enemies die, they are NOT replaced
4. **Wave Reset:** When a new wave starts, all counters reset to 0
5. **Priority:** First towers to fire (and eligible) get counterattackers

### Example Scenario (Default Settings: 2 enemies per tower, 2 towers max)

**Wave 1:**
```
Tower A fires → 2 enemies assigned to Tower A
├─ Tower A now in towersUnderAttackThisWave (1/2)
├─ Enemy 1 killed → Tower A still has 2/2 assigned (no replacement)
└─ Enemy 2 killed → Tower A still has 2/2 assigned (no replacement)
    └─ Tower A fires again → No new enemies assigned (quota reached)

Tower B fires → 2 enemies assigned to Tower B
├─ Tower B now in towersUnderAttackThisWave (2/2)
└─ Both enemies alive and attacking

Tower C fires → NO enemies assigned
└─ Max towers under attack reached (2/2) - Tower C is protected!

Tower D fires → NO enemies assigned
└─ Max towers under attack reached (2/2) - Tower D is protected!
```

**Wave 2 Starts:**
```
All counters reset → Each tower can receive counterattackers again
towersUnderAttackThisWave cleared → First 2 towers to fire get assignments
Tower C fires first → 2 enemies assigned (now Tower C is under attack)
Tower D fires second → 2 enemies assigned (now Tower D is under attack)
Tower A fires third → NO assignment (max 2 towers already under attack)
```

## Implementation Details

### TowerDefenseCoordinator.cs

**Data Structures:**
```csharp
// Configurable limits (Inspector-editable)
[SerializeField] private int maxCounterattackersPerTower = 2;
[SerializeField] private int maxTowersUnderAttackPerWave = 2; // 0 = unlimited

// Track currently active assignments
private Dictionary<Tower, List<Enemy>> towerAssignments;

// Track total enemies assigned per tower this wave (does not decrease when enemies die)
private Dictionary<Tower, int> totalAssignedCount;

// Track which towers have been assigned enemies this wave
private HashSet<Tower> towersUnderAttackThisWave;
```

**Key Methods:**

#### OnWaveStarted(int waveNumber)
Called when a new wave begins:
- Clears all assignment lists
- Resets all `totalAssignedCount` values to 0
- Clears `towersUnderAttackThisWave` set
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
    
    // Clear towers under attack tracking
    towersUnderAttackThisWave.Clear();
}
```

#### OnTowerFired(Tower tower, Enemy targetEnemy)
Called when a tower fires:
- Checks if tower already has assignments this wave
- **NEW:** Checks if max towers under attack limit reached (if not 0)
- Checks if tower has reached its per-tower assignment limit
- If limits not reached, finds eligible enemies and assigns them
- Increments `totalAssignedCount[tower]` for each assignment
- **NEW:** Adds tower to `towersUnderAttackThisWave` set

```csharp
private void OnTowerFired(Tower tower, Enemy targetEnemy)
{
    bool towerAlreadyUnderAttack = towersUnderAttackThisWave.Contains(tower);
    
    // Check max towers under attack (0 = unlimited)
    if (!towerAlreadyUnderAttack && maxTowersUnderAttackPerWave > 0 
        && towersUnderAttackThisWave.Count >= maxTowersUnderAttackPerWave)
    {
        return; // Tower is protected, no assignments
    }
    
    // Check per-tower limit
    if (totalAssignedCount[tower] >= maxCounterattackersPerTower)
        return;
    
    // Assign enemies and add tower to set
    // ...
    towersUnderAttackThisWave.Add(tower);
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

### Why Per-Wave Limits?

**Per-Tower Limit (default: 2 enemies per tower):**
1. **Strategic Depth:** Players must consider when to place/upgrade towers
2. **Wave Pacing:** Ensures each wave has distinct counterattack opportunities
3. **Predictability:** Players can anticipate max 2 enemies per tower per wave
4. **Balance:** Prevents overwhelming tower with unlimited counterattackers

**Per-Wave Tower Limit (default: 2 towers max):**
1. **Defense Distribution:** Protects some towers, forces strategic placement
2. **Priority System:** First towers to fire are most vulnerable
3. **Late-Game Scaling:** With many towers, only subset gets attacked
4. **Player Choice:** Players can choose which towers to activate first

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
✓ Expected: Max 2 enemies assigned per tower
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

### Scenario 5: Max Towers Under Attack Limit
```
Wave 1 starts (maxTowersUnderAttackPerWave = 2)
Tower A fires → 2 enemies assigned (1/2 towers under attack)
Tower B fires → 2 enemies assigned (2/2 towers under attack)
Tower C fires → NO assignment (max towers reached)
Tower D fires → NO assignment (max towers reached)
✓ Expected: Only first 2 towers get counterattackers
```

### Scenario 6: Unlimited Towers Mode
```
Set maxTowersUnderAttackPerWave = 0
Tower A fires → 2 enemies assigned
Tower B fires → 2 enemies assigned
Tower C fires → 2 enemies assigned
Tower D fires → 2 enemies assigned
... all towers get assignments (no limit)
✓ Expected: All towers can receive counterattackers
```

### Scenario 7: Tower Destroyed
```
Wave 1: Tower A gets 2 enemies assigned
Enemies destroy Tower A
Tower A removed from tracking
✓ Expected: No memory leaks, clean removal
```

### Scenario 8: Custom Configuration
```
Set maxCounterattackersPerTower = 3
Set maxTowersUnderAttackPerWave = 1
Wave starts
Tower A fires → 3 enemies assigned (1/1 towers)
Tower B fires → NO assignment (max 1 tower already under attack)
✓ Expected: Configuration values respected
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
TowerDefenseCoordinator: Assigned Goblin_05 to counterattack Basic Tower (total assigned: 1/2, towers under attack: 1/2)
TowerDefenseCoordinator: Assigned Goblin_07 to counterattack Basic Tower (total assigned: 2/2, towers under attack: 1/2)
```

### Quota Reached (Per-Tower)
```
TowerDefenseCoordinator: Basic Tower fired at Goblin_10!
TowerDefenseCoordinator: Basic Tower has already had 2 enemies assigned (max 2). No replacements.
```

### Max Towers Limit Reached
```
TowerDefenseCoordinator: Cannon Tower fired at Goblin_15!
TowerDefenseCoordinator: Max towers under attack (2) already reached. Cannot assign enemies to Cannon Tower.
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
