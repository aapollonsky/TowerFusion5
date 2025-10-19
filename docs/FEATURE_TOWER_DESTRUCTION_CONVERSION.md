# Feature: Tower Destruction Converts Attackers to Stealers

## Overview
When a tower is destroyed, all enemies that were attacking it will convert from **Attackers** to **Stealers** and redirect to steal corn instead of seeking another tower.

## Strategic Implications

### For Players
**Risk vs Reward:**
- ❌ **Downside:** Destroyed towers create new corn stealers
- ✅ **Upside:** Removes tower from being attacked
- 🎯 **Strategy:** Balance tower health vs corn defense

**Tower Management:**
- Weak towers might need replacement before destruction
- Consider letting disposable towers fall to create manageable stealer waves
- Protect high-value towers to prevent stealer conversion

**Corn Defense:**
- Expect stealer surges when towers are destroyed
- Position defenses to handle both roles
- Monitor corn storage when towers are under heavy attack

### For Enemies
**Dynamic Role Switching:**
- Start as Attacker (85% chance)
- Attack assigned tower until destroyed
- Convert to Stealer when tower falls
- Attempt to steal corn instead of continuing attack

**Fallback Behavior:**
- If no corn available: Move to end point (damage player)
- If CornManager disabled: Continue seeking towers (old behavior)

## Implementation Details

### Core Method: `ConvertToStealer()`

```csharp
private void ConvertToStealer()
{
    // Only convert attackers
    if (assignedRole != EnemyRole.Attacker)
        return;
    
    // Check if corn system available
    if (CornManager.Instance == null || CornManager.Instance.RemainingCorn <= 0)
    {
        // No corn available, move to end instead
        behaviorState = EnemyBehaviorState.MovingToEnd;
        return;
    }
    
    // Convert role and behavior
    assignedRole = EnemyRole.Stealer;
    behaviorState = EnemyBehaviorState.MovingToCorn;
    
    // Clean up tower targeting
    currentTowerTarget = null;
    isAttackingTower = false;
    EnemyTargetDistributor.Instance?.UnassignEnemy(this);
}
```

### Conversion Triggers

**1. Enemy Destroys Tower (AttackTower method)**
```csharp
if (!tower.IsAlive)
{
    // Unassign from distributor
    EnemyTargetDistributor.Instance?.UnassignEnemy(this);
    
    currentTowerTarget = null;
    isAttackingTower = false;
    
    ConvertToStealer(); // ← Convert instead of seeking new tower
}
```

**2. Tower Becomes Invalid During Attack (HandleTowerBehavior)**
```csharp
case EnemyBehaviorState.AttackingTower:
    if (currentTowerTarget != null && currentTowerTarget.IsAlive)
    {
        // Attack logic...
    }
    else
    {
        // Tower destroyed or invalid
        currentTowerTarget = null;
        isAttackingTower = false;
        ConvertToStealer(); // ← Convert when tower lost
    }
    break;
```

**3. Tower Lost Before Reaching (UpdateTowerTargeting)**
```csharp
if (currentTowerTarget != null && !currentTowerTarget.IsAlive)
{
    // Unassign from distributor
    EnemyTargetDistributor.Instance?.UnassignEnemy(this);
    
    currentTowerTarget = null;
    isAttackingTower = false;
    
    ConvertToStealer(); // ← Convert instead of finding new tower
    return; // Don't look for another tower
}
```

## Behavior Flow

### Before Tower Destroyed
```
Enemy spawns as Attacker (85%)
  ↓
Seeks assigned tower (via distributor)
  ↓
Moves toward tower
  ↓
Reaches attack range
  ↓
Attacks tower repeatedly
```

### After Tower Destroyed
```
Tower health reaches 0
  ↓
ConvertToStealer() called
  ↓
Check: Corn available?
  ├─ Yes → Convert to Stealer
  │   ↓
  │   behaviorState = MovingToCorn
  │   ↓
  │   Navigate to corn storage
  │   ↓
  │   Grab corn (1 second)
  │   ↓
  │   Return to spawn with corn
  │
  └─ No → Move to end point (damage player)
```

## Edge Cases Handled

### ✅ No Corn Available
**Scenario:** Tower destroyed but all corn already stolen/grabbed

**Behavior:**
```csharp
if (CornManager.Instance.RemainingCorn <= 0)
{
    behaviorState = EnemyBehaviorState.MovingToEnd;
    return;
}
```

**Result:** Enemy moves to end point and damages player (traditional behavior)

### ✅ CornManager Not Available
**Scenario:** Corn theft system disabled or CornManager not in scene

**Behavior:**
```csharp
if (CornManager.Instance == null)
{
    behaviorState = EnemyBehaviorState.MovingToEnd;
    return;
}
```

**Result:** Enemy moves to end point (graceful degradation)

### ✅ Already a Stealer
**Scenario:** Method called on enemy already stealing corn

**Behavior:**
```csharp
if (assignedRole != EnemyRole.Attacker)
    return;
```

**Result:** No change, method exits early

### ✅ Tower Destroyed by Different Source
**Scenario:** Player projectiles, other towers, or scripted events destroy tower

**Behavior:** `UpdateTowerTargeting()` detects invalid tower every frame

**Result:** Conversion happens on next frame update

### ✅ Multiple Enemies on Same Tower
**Scenario:** 3 enemies attacking same tower, tower is destroyed

**Behavior:**
- Enemy that dealt killing blow converts immediately in `AttackTower()`
- Other 2 enemies detect invalid tower in next `UpdateTowerTargeting()` call
- All 3 convert to stealers (or move to end if no corn)

**Result:** All attackers of destroyed tower redirect to corn

## Statistics & Balance

### Initial Distribution (Wave Start)
- 85% Attackers → Attack towers
- 15% Stealers → Steal corn immediately

### Dynamic Changes (During Wave)
- Tower destroyed → Attackers convert → Stealer count increases
- Corn depleted → New conversions go to end point instead

### Example Scenario
```
Wave starts: 20 enemies
  - 17 Attackers
  - 3 Stealers

Tower destroyed (3 enemies attacking it):
  - 14 Attackers (remaining)
  - 6 Stealers (3 original + 3 converted)

Another tower destroyed (2 enemies attacking it):
  - 12 Attackers (remaining)
  - 8 Stealers (6 + 2 converted)

Final distribution: 40% Stealers (up from 15%)
```

## Testing Checklist

### Basic Functionality
```
□ Enemy attacks tower until destroyed
□ Enemy converts to stealer when tower falls
□ Console log: "converted to STEALER after tower destruction!"
□ Enemy navigates to corn storage
□ Enemy grabs corn successfully
□ Enemy returns to spawn with corn
```

### Edge Cases
```
□ No corn available → Enemy moves to end instead
□ CornManager disabled → Enemy moves to end instead
□ Multiple enemies on same tower → All convert
□ Tower destroyed by projectile → Attackers convert
□ Already a stealer → No change (method exits)
```

### Visual Verification
```
□ Enemy changes direction toward corn after conversion
□ Yellow sphere appears when corn grabbed
□ Enemy sprite faces corn storage while moving
□ No jittering or stuck enemies
```

### System Integration
```
□ EnemyTargetDistributor unassigns enemy properly
□ Wave completes when all enemies done (killed/reached end/stolen corn)
□ No null reference errors in console
□ No infinite loops or stuck states
```

## Console Logs

### Successful Conversion
```
EnemyBasic(Clone) attacked Tower for 10 damage!
EnemyBasic(Clone) converted to STEALER after tower destruction!
EnemyBasic(Clone) reached corn storage, starting grab
[CornStorage] Corn taken by EnemyBasic(Clone). Remaining: 19
EnemyBasic(Clone) grabbed corn! Returning to spawn with 0.8x speed
```

### No Corn Available
```
EnemyBasic(Clone) attacked Tower for 10 damage!
EnemyBasic(Clone) tower destroyed but no corn available, moving to end
```

### Multiple Conversions
```
EnemyBasic(Clone) converted to STEALER after tower destruction!
EnemyBasic(Clone) converted to STEALER after tower destruction!
EnemyBasic(Clone) converted to STEALER after tower destruction!
```

## Related Files Modified

- `Assets/Scripts/Enemy/Enemy.cs`
  - **Added:** `ConvertToStealer()` method (line ~155)
  - **Modified:** `AttackTower()` - Call ConvertToStealer on destruction
  - **Modified:** `HandleTowerBehavior()` - Call ConvertToStealer when tower invalid
  - **Modified:** `UpdateTowerTargeting()` - Call ConvertToStealer and return early

## Design Rationale

### Why This Feature?

**1. Strategic Depth**
- Players must consider tower health vs corn defense
- Adds decision-making: repair/replace towers or let them fall?

**2. Dynamic Difficulty**
- Tower destruction increases corn theft pressure
- Creates escalating threat as defenses weaken

**3. Realistic Behavior**
- Enemy finds alternative objective when original target destroyed
- Makes logical sense: "Tower gone → Steal corn instead"

**4. Prevents Exploits**
- Without this: Players could create "disposable" towers to waste enemy time
- With this: Destroyed towers have consequences (more stealers)

### Alternative Designs Considered

**❌ Seek Another Tower:**
- Too predictable, no strategic consequence
- Players could cycle enemies through weak towers

**❌ Move to End Immediately:**
- Wastes enemy potential
- No alternative objective

**❌ Random Choice:**
- Inconsistent, hard to strategize around
- Less elegant than unified conversion

**✅ Convert to Stealer:**
- Clear consequence for tower destruction
- Adds strategic layer
- Makes use of corn theft system
- Fallback behavior if no corn available

## Future Enhancements

### Potential Additions

**1. Conversion Animation**
- Visual effect when converting (color flash, icon)
- Audio cue for conversion
- Particle effect showing role change

**2. Partial Conversion**
- Percentage-based conversion (e.g., 50% convert, 50% move to end)
- Configurable per EnemyData
- Adds more tuning options

**3. Tower Destruction Bonus**
- Extra speed/damage for converted stealers
- "Motivated by success" mechanic
- Rewards enemies for destroying towers

**4. Player Notification**
- UI warning: "Tower destroyed - enemies converting!"
- Minimap indicators for new stealers
- Alert system for corn defense

**5. Difficulty Scaling**
- Higher difficulties: More/all enemies convert
- Lower difficulties: Fewer enemies convert
- Configurable conversion rate

## Configuration Options

To adjust behavior, modify these values in code:

### Disable Feature
```csharp
// In ConvertToStealer(), add at start:
return; // Disable conversion feature
```

### Partial Conversion (50% chance)
```csharp
// In ConvertToStealer(), add after role check:
if (Random.value < 0.5f)
{
    behaviorState = EnemyBehaviorState.MovingToEnd;
    return; // 50% chance to not convert
}
```

### Always Move to End Instead
```csharp
// In ConvertToStealer(), replace conversion logic:
behaviorState = EnemyBehaviorState.MovingToEnd;
return;
```

---

**Status:** ✅ IMPLEMENTED (as of October 18, 2025)

**Priority:** High - Core gameplay mechanic

**Impact:** Major strategic depth added to tower defense + corn theft hybrid
