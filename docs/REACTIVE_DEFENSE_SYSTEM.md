# Reactive Defense System Design

## Overview

All enemies are now **Stealers** by default. They only attack towers when provoked (tower fires at them), creating a reactive defense mechanism.

## Core Mechanics

### Default Behavior: Stealing
- **100% of enemies** start as stealers
- Move toward corn storage
- Grab corn if available
- Return to spawn point with corn

### Reactive Defense: Counterattack
**Trigger:** When any tower fires at any enemy
**Response:** Up to 2 corn-less enemies counterattack that specific tower
**Selection criteria:**
- Must NOT have corn
- Must be available (not already counterattacking another tower)
- Closest enemies to the tower have priority

**Counterattack flow:**
1. Tower fires → `OnTowerFired` event
2. `TowerDefenseCoordinator` receives event
3. Find up to 2 eligible enemies (no corn, not already counterattacking)
4. Prioritize enemies closest to the tower
5. Assign them to counterattack that tower
6. Enemies switch to `CounterattackingTower` state
7. Move to tower and attack until destroyed (never give up)
8. When tower dies → resume stealing behavior

### Edge Cases

**No corn available:**
- Enemy reaches corn pile
- No corn in storage
- Wait for same duration as grabbing corn (`CORN_GRAB_DURATION`)
- Return to spawn empty-handed

**Corn reappears while waiting:**
- If corn-carrying enemy dies on return journey
- Corn returns to storage
- Waiting enemy grabs it immediately

## State Machine

### Old States (Remove)
- ~~`SeekingTower`~~ - No longer needed
- ~~`AttackingTower`~~ - Replaced with `CounterattackingTower`

### New States
1. **`MovingToCorn`** - Default: Moving toward corn storage
2. **`GrabbingCorn`** - Standing at corn pile, grabbing corn
3. **`ReturningWithCorn`** - Has corn, moving back to spawn
4. **`CounterattackingTower`** - Reactive: Moving to/attacking tower that fired
5. **`WaitingForCorn`** - At corn pile, no corn available, waiting briefly
6. **`ReturningEmpty`** - No corn grabbed, returning to spawn

## Architecture Changes

### Remove: EnemyRole System
```csharp
// DELETE THIS
public enum EnemyRole { Attacker, Stealer }
private EnemyRole assignedRole;
```

All enemies are stealers, no role assignment needed.

### New: TowerDefenseCoordinator

Singleton manager that coordinates counterattacks:

```csharp
public class TowerDefenseCoordinator : MonoBehaviour
{
    private const int MAX_COUNTERATTACKERS = 2;
    private Dictionary<Tower, List<Enemy>> counterattackAssignments;
    
    // Called when tower fires
    public void OnTowerFired(Tower tower, Enemy targetEnemy)
    {
        // Find up to 2 eligible enemies
        List<Enemy> eligible = FindEligibleCounterattackers(tower);
        
        // Assign them to counterattack
        foreach (Enemy enemy in eligible.Take(2))
        {
            AssignCounterattack(enemy, tower);
        }
    }
    
    private List<Enemy> FindEligibleCounterattackers(Tower tower)
    {
        // Must be alive
        // Must NOT have corn
        // Must NOT be already counterattacking another tower
        // Sort by distance to tower (closest first = higher priority)
        // All enemies can potentially respond (no distance limit)
    }
}
```

### Update: Tower.cs

Add event for when tower fires:

```csharp
public System.Action<Tower, Enemy> OnTowerFired;

private void Attack(Enemy target)
{
    // Deal damage
    target.TakeDamage(damage);
    
    // Notify reactive defense system
    OnTowerFired?.Invoke(this, target);
}
```

### Update: Enemy.cs

Simplified behavior:

```csharp
private enum EnemyBehaviorState 
{ 
    MovingToCorn, 
    GrabbingCorn, 
    WaitingForCorn,
    ReturningWithCorn, 
    ReturningEmpty,
    CounterattackingTower 
}

private void Update()
{
    switch (behaviorState)
    {
        case MovingToCorn:
            MoveTowardsCornStorage();
            if (AtCornPile()) {
                if (CornAvailable()) {
                    behaviorState = GrabbingCorn;
                } else {
                    behaviorState = WaitingForCorn;
                    waitStartTime = Time.time;
                }
            }
            break;
            
        case WaitingForCorn:
            if (CornAvailable()) {
                behaviorState = GrabbingCorn;
            } else if (Time.time - waitStartTime > waitDuration) {
                behaviorState = ReturningEmpty;
            }
            break;
            
        case GrabbingCorn:
            // Grab animation/timer
            if (GrabComplete()) {
                hasCorn = true;
                behaviorState = ReturningWithCorn;
            }
            break;
            
        case ReturningWithCorn:
        case ReturningEmpty:
            MoveTowardsSpawnPoint();
            if (AtSpawn()) {
                if (hasCorn) {
                    DeliverCorn();
                }
                Despawn();
            }
            break;
            
        case CounterattackingTower:
            if (towerTarget == null || !towerTarget.IsAlive) {
                // Tower destroyed, resume stealing
                towerTarget = null;
                behaviorState = MovingToCorn;
            } else {
                MoveTowardsTower();
                if (InAttackRange()) {
                    AttackTower();
                }
            }
            break;
    }
}

// Called by TowerDefenseCoordinator
public void AssignCounterattack(Tower tower)
{
    if (hasCorn) return; // Can't counterattack with corn
    
    towerTarget = tower;
    behaviorState = CounterattackingTower;
}
```

## Benefits of This System

### Gameplay
- **More dynamic**: Enemy behavior changes based on player actions
- **Strategic depth**: Players must consider whether to fire (provokes counterattack)
- **Risk/reward**: Killing enemies = good, but triggers defenders
- **Emergent behavior**: Different scenarios each wave

### Technical
- **Simpler than fixed roles**: No percentage calculation
- **Event-driven**: Clean decoupling via events
- **Reactive not proactive**: Enemies don't seek towers, only respond
- **Fewer states**: No complex attacker seeking logic

## Implementation Notes

### Priority Order
1. Remove EnemyRole enum and assignment (clean up)
2. Add OnTowerFired event to Tower
3. Create TowerDefenseCoordinator
4. Update Enemy states and logic
5. Update EnemyManager to spawn all as stealers
6. Test and balance

### Balancing Parameters
- `MAX_COUNTERATTACKERS = 2` - How many enemies respond per tower shot
- `CORN_GRAB_DURATION` - Reused for both grabbing corn AND waiting at empty pile
- **Response distance**: All enemies can respond, but closer enemies have higher priority
- **Counterattack persistence**: Always pursue until tower destroyed (never give up)

### Edge Case Handling
- **Tower destroyed mid-counterattack**: Resume stealing behavior
- **Multiple towers fire simultaneously**: Each tower gets up to 2 enemies assigned to it (enemies from shared pool, but each tower can have its own 2 counterattackers)
- **Not enough corn-less enemies**: Fewer than 2 might respond per tower (that's ok)
- **Enemy destroys tower while counterattacking**: Resume stealing behavior immediately

## Testing Scenarios

1. **Basic stealing**: All enemies go to corn → grab → return
2. **Single tower fires**: 2 enemies counterattack, others continue stealing
3. **Multiple towers fire**: Each tower gets up to 2 enemies from the shared pool of available enemies (if 3 towers fire and 6 enemies available, each gets 2)
4. **Tower destroyed**: Counterattacking enemies resume stealing
5. **No corn available**: Enemies wait (same duration as grabbing) → return empty
6. **Corn reappears**: Waiting enemy grabs it
7. **All enemies have corn**: Tower fires but no counterattack (all carrying corn)
8. **Distant tower fires**: Furthest enemies still respond if they're the only ones available

## Future Enhancements

- **Variable response**: Different enemies respond differently (some cowardly, some aggressive)
- **Group tactics**: Defenders coordinate their attacks
- **Warning system**: Defenders call for help from nearby allies
- **Revenge mechanic**: Extra defenders if tower kills an enemy

---

This system transforms the gameplay from static roles (85% attacker / 15% stealer) to dynamic reactive behavior where the player's actions directly influence enemy tactics.
