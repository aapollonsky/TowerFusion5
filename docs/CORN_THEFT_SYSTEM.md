# Corn Theft Game System 🌽

## Game Objective Redesign

**New Goal:** Defend your corn storage from thieving enemies!

### Win Condition
- Defeat all waves while keeping at least 1 corn in storage

### Loss Condition
- All corn stolen and successfully returned to enemy spawn point

## Core Mechanics

### Corn Storage
- Fixed location on the map (like enemy end point)
- Starts with configurable amount of corn (e.g., 20 corns)
- Enemies can grab one corn at a time
- Corn visually returns if enemy carrying it dies

### Enemy Roles

**85% Attackers (Tower Destroyers)**
- Current behavior: Attack towers to clear path
- Ignore corn storage
- Use target distribution system (max 3 towers)
- Purpose: Distract player and destroy defenses

**15% Stealers (Corn Thieves)**
- New behavior: Bypass towers, go straight to corn
- Grab one corn from storage
- Run back to spawn point with corn
- If killed: Drop corn, corn returns to storage
- Don't attack towers (unless blocked?)

## Enemy AI States

### Attacker States (85% of enemies)
```
SeekingTower → AttackingTower → (tower destroyed) → MovingToEnd
```

### Stealer States (15% of enemies)
```
MovingToCorn → GrabbingCorn → ReturningWithCorn → (reaches spawn) → CornStolen!

If killed while carrying:
ReturningWithCorn → Die() → DropCorn() → CornReturnsToStorage
```

## System Architecture

### New Components

#### CornStorage.cs
```csharp
public class CornStorage : MonoBehaviour
{
    [SerializeField] private int initialCornCount = 20;
    private int currentCornCount;
    
    public Vector3 Position { get; }
    public int CornCount { get; }
    public bool HasCorn { get; }
    
    public bool TakeCorn(Enemy thief);
    public void ReturnCorn();
    
    // Events
    public event Action<int> OnCornTaken;
    public event Action<int> OnCornReturned;
    public event Action OnAllCornStolen;
}
```

#### CornManager.cs (Singleton)
```csharp
public class CornManager : MonoBehaviour
{
    public static CornManager Instance { get; }
    
    public CornStorage Storage { get; }
    public int TotalCornStolen { get; }
    public int RemainingCorn { get; }
    
    public void RegisterCornSteal(Enemy thief);
    public void RegisterCornReturn();
    
    public bool IsGameLost(); // All corn stolen
}
```

### Modified Components

#### EnemyData.cs
```csharp
[SerializeField] private EnemyRole role = EnemyRole.Attacker;
[SerializeField] private float cornGrabDuration = 1f; // Time to grab corn

public enum EnemyRole
{
    Attacker,  // 85% - Attack towers
    Stealer    // 15% - Steal corn
}
```

#### Enemy.cs - New Fields
```csharp
private EnemyRole role;
private bool hasCorn = false;
private GameObject cornVisual; // Show corn on enemy's back

// New States
private enum State
{
    // Attacker states
    SeekingTower,
    AttackingTower,
    MovingToEnd,
    
    // Stealer states
    MovingToCorn,
    GrabbingCorn,
    ReturningWithCorn,
    
    Dead
}
```

#### Enemy.cs - New Methods
```csharp
private void MoveTowardsCornStorage();
private void GrabCorn();
private void ReturnToSpawn();
private void DropCorn();
```

#### WaveManager.cs
```csharp
// Assign roles when spawning
private void SpawnEnemy(EnemyData data)
{
    // 85% chance of Attacker, 15% chance of Stealer
    float roll = Random.value;
    EnemyRole role = roll < 0.85f ? EnemyRole.Attacker : EnemyRole.Stealer;
    
    enemy.SetRole(role);
}
```

#### GameManager.cs - Updated Win/Loss
```csharp
private void CheckWinCondition()
{
    // Win: All waves complete AND corn remaining
    if (allWavesComplete && CornManager.Instance.RemainingCorn > 0)
    {
        OnGameWon();
    }
}

private void CheckLossCondition()
{
    // Loss: All corn stolen
    if (CornManager.Instance.IsGameLost())
    {
        OnGameLost();
    }
}
```

#### EnemyTargetDistributor.cs
```csharp
// Only distribute Attacker enemies
public Tower SelectTargetForEnemy(Enemy enemy, Vector3 enemyPosition, float detectionRange)
{
    // Skip distribution for Stealer enemies
    if (enemy.Role == EnemyRole.Stealer)
        return null;
    
    // Existing distribution logic for Attackers...
}
```

## Visual Feedback

### Corn Storage
- Visual representation of corn pile
- Count display showing remaining corn
- Animation when corn taken
- Particle effect when corn returns

### Enemy with Corn
- Visible corn sprite on enemy's back/hands
- Different movement animation (slower? weighted?)
- Trail/glow effect to highlight them
- Icon/marker above head

### UI Elements
- **Corn Counter**: Large display showing remaining corn
- **Stolen Counter**: How many corn successfully stolen
- **Enemy Roles**: Mini-map or indicators showing stealers vs attackers

## Balance Considerations

### Configurable Values
```csharp
[SerializeField] private int initialCornCount = 20;           // Starting corn
[SerializeField] private float stealerPercentage = 0.15f;     // 15% stealers
[SerializeField] private float stealerSpeedMultiplier = 1.2f; // Stealers faster?
[SerializeField] private float cornCarrySpeedPenalty = 0.8f;  // Slower with corn
[SerializeField] private int cornLossThreshold = 5;           // Warning at 5 corn left
```

### Strategic Depth
- **Tower Placement**: Protect corn storage vs intercept stealers
- **Priority Targeting**: Kill stealers first or focus on attackers?
- **Resource Management**: Upgrade towers vs build more coverage
- **Risk/Reward**: Let stealers through to focus on attackers?

## Movement Paths

### Attacker Path (Current)
```
Spawn → Seek Towers → Attack Towers → Move to End
```

### Stealer Path (New)
```
Spawn → Beeline to Corn Storage → Grab Corn → Beeline back to Spawn

Visual:
[Spawn Point] ----→ [Corn Storage] ←---- [Spawn Point]
                         ↓
                    Grab Corn!
```

### Pathfinding Considerations
- Stealers might need simpler pathfinding (direct line?)
- Or use waypoint system but skip tower waypoints?
- Should stealers avoid towers or take shortest path?

## Implementation Phases

### Phase 1: Core Systems ✅ (Start Here)
1. Create `CornStorage.cs` component
2. Create `CornManager.cs` singleton
3. Add corn storage to map/scene

### Phase 2: Enemy Roles
4. Add `EnemyRole` enum to `EnemyData.cs`
5. Update `Enemy.cs` with role field
6. Modify `WaveManager.cs` spawn logic (85/15 split)

### Phase 3: Stealer Behavior
7. Add new AI states to `Enemy.cs`
8. Implement corn pathfinding
9. Implement corn grab mechanics
10. Implement return-to-spawn behavior

### Phase 4: Corn Carrying
11. Add `hasCorn` state to enemies
12. Implement drop-on-death mechanics
13. Add corn return animation/effect

### Phase 5: Game Flow
14. Update win/loss conditions in `GameManager.cs`
15. Update UI to show corn count
16. Add visual feedback for corn theft

### Phase 6: Polish
17. Add visual indicators for stealers
18. Balance testing (percentage, speeds, counts)
19. Add particle effects and animations
20. Update all documentation

## Edge Cases

### What if corn storage empty when stealer arrives?
**Solution:** Stealer becomes attacker or moves to end point

### What if stealer blocked by towers?
**Option A:** Stealer attacks blocking tower (breaks character)
**Option B:** Stealer takes damage but doesn't fight back (passive)
**Option C:** Stealers are "sneaky" and towers don't target them until they have corn

### What if all enemies are stealers?
**Constraint:** Ensure minimum attacker count per wave (e.g., always at least 1 attacker)

### What if player has no towers?
**Current:** Stealers win easily (intended - player should defend!)

## Testing Scenarios

1. **10 enemies, 2 stealers**: Verify 80/20 split approximately
2. **Kill stealer with corn**: Confirm corn returns to storage
3. **Stealer reaches spawn**: Confirm corn counter decrements
4. **All corn stolen**: Verify game loss
5. **Wave complete with corn**: Verify game continues
6. **Final wave, corn remains**: Verify game win

## Configuration Example

### MapData.cs (Updated)
```csharp
[Header("Corn Storage")]
public Vector3 cornStoragePosition;
public int initialCornCount = 20;

[Header("Spawn Points")]
public Vector3 enemySpawnPoint;
public Vector3 enemyReturnPoint; // Same as spawn usually
```

### WaveData.cs (Optional Enhancement)
```csharp
[Header("Enemy Composition")]
[Range(0f, 1f)] public float stealerPercentage = 0.15f; // Per-wave override
```

## Visual Design Notes

### Corn Storage Visual
- Large basket/pile of corn at designated location
- Decreases in size as corn stolen
- Glowing/important looking (player focus)

### Corn Visual on Enemy
- Small corn sprite attached to enemy
- Rotate/bob animation
- Highlight color (yellow/gold glow)

### Path Indicators
- Optional: Show stealer paths to storage
- Dotted lines or footprints
- Helps player anticipate threats

## Difficulty Scaling

### Easy Mode
- More starting corn (30+)
- Fewer stealers (10%)
- Slower stealers

### Hard Mode
- Less starting corn (10-15)
- More stealers (20-25%)
- Faster stealers
- Stealers can attack if blocked

### Dynamic Difficulty
- If player losing: Reduce stealer percentage
- If player winning easily: Increase stealer percentage

## Summary

This redesign transforms the game from "survive waves" to "defend the corn," adding:

✅ **Strategic Depth**: Two enemy types require different defense strategies
✅ **Clear Objective**: Visual corn storage that player actively protects  
✅ **Tension**: Watching corn count decrease creates urgency
✅ **Risk/Reward**: Kill order decisions matter
✅ **Visual Feedback**: Enemies carrying corn are visible threats

The 85/15 split ensures most enemies still attack towers (preserving current gameplay) while adding the corn theft mechanic as a parallel threat.
