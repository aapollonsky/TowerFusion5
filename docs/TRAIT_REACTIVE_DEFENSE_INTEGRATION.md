# Trait System Integration with Reactive Defense

## Overview

This document explains how the tower trait system integrates with the reactive defense system to ensure enemies counterattack towers regardless of whether the damage comes from the tower itself or from trait-based projectiles.

## Problem

The reactive defense system relies on the `OnTowerFired` event to detect when a tower attacks an enemy. This event triggers `TowerDefenseCoordinator` to assign up to 2 corn-less enemies to counterattack that tower.

**Original Implementation:**
- ✅ Tower.Attack() triggers `OnTowerFired` event correctly
- ❌ TraitProjectileSystem.FireProjectile() did NOT trigger the event
- ❌ Enemies wouldn't counterattack towers with independent trait projectiles

## Solution

Added `OnTowerFired` event invocation to `TraitProjectileSystem.FireProjectile()` method to match the behavior of the normal tower attack.

### Code Changes

**File:** `Assets/Scripts/Tower/TraitProjectileSystem.cs`

**Method:** `FireProjectile(Enemy target)`

```csharp
private void FireProjectile(Enemy target)
{
    if (trait.projectilePrefab == null)
    {
        Debug.LogWarning($"Trait '{trait.traitName}' has no projectile prefab assigned!");
        return;
    }
    
    // NEW: Notify reactive defense system BEFORE creating projectile
    tower.OnTowerFired?.Invoke(tower, target);
    
    // Create projectile...
    GameObject projectileObj = Instantiate(...);
    // ... rest of method
}
```

## How It Works

### Normal Tower Attacks

1. **Tower.Attack(Enemy target)** is called
2. **OnTowerFired?.Invoke(this, target)** triggers immediately
3. **TowerDefenseCoordinator** assigns up to 2 enemies to counterattack
4. Tower deals damage (hitscan or projectile)
5. Trait effects applied via **TowerTraitManager.ApplyTraitEffectsOnAttack()**

### Trait Independent Projectiles

1. **TraitProjectileSystem.Update()** checks cooldown timer
2. **TryFire()** checks if target exists and is in range
3. **FireProjectile(Enemy target)** is called
4. **OnTowerFired?.Invoke(tower, target)** triggers immediately ✅ **NEW**
5. **TowerDefenseCoordinator** assigns up to 2 enemies to counterattack
6. Projectile is created and initialized
7. Projectile hits target and applies damage + trait effects

### Secondary Trait Effects

Chain lightning, explosions, and other area effects do **NOT** trigger `OnTowerFired` because:
- They are secondary effects that occur after the primary attack
- The primary attack already triggered the event
- Enemies are already assigned to counterattack from the primary target

**Examples:**
- **Chain Lightning**: Primary shot triggers event → 2 enemies assigned → chain hits secondary targets (no additional counterattackers)
- **Explosion**: Primary hit triggers event → 2 enemies assigned → explosion damages nearby enemies (no additional counterattackers)
- **Earth Trap**: Primary hit triggers event → 2 enemies assigned → trap captures target (no additional counterattackers)

## Event Timing

The `OnTowerFired` event is triggered **BEFORE** dealing damage in both cases:

**Tower.Attack():**
```csharp
private void Attack(Enemy target)
{
    // Notify reactive defense system BEFORE dealing damage
    OnTowerFired?.Invoke(this, target);
    
    if (towerData.isHitscan)
        DealDamageToTarget(target);
    else
        CreateProjectile(target);
}
```

**TraitProjectileSystem.FireProjectile():**
```csharp
private void FireProjectile(Enemy target)
{
    // Notify reactive defense system BEFORE creating projectile
    tower.OnTowerFired?.Invoke(tower, target);
    
    GameObject projectileObj = Instantiate(...);
    // ...
}
```

This ensures `TowerDefenseCoordinator` can assign counterattackers before the target potentially dies from the attack.

## Testing Checklist

### Scenario 1: Tower Without Traits
- [ ] Tower attacks enemy
- [ ] Up to 2 corn-less enemies assigned to counterattack
- [ ] Assigned enemies change state to `CounterattackingTower`
- [ ] Enemies move toward tower and attack

### Scenario 2: Tower With Standard Traits (No Independent Projectiles)
- [ ] Tower with Fire/Ice/Slow/etc. attacks enemy
- [ ] Up to 2 corn-less enemies assigned to counterattack
- [ ] Trait effects apply (burn, slow, etc.)
- [ ] Secondary effects (chain, explosion) do NOT trigger additional counterattacks

### Scenario 3: Tower With Independent Projectile Traits
- [ ] Trait projectile fires at cooldown interval
- [ ] **OnTowerFired event triggers** ✅
- [ ] Up to 2 corn-less enemies assigned to counterattack
- [ ] Trait projectile hits and applies damage
- [ ] If trait has secondary effects, they apply without additional counterattacks

### Scenario 4: Tower With Multiple Traits
- [ ] Tower's main attack triggers counterattack assignment
- [ ] Each independent trait projectile triggers counterattack assignment
- [ ] Multiple traits = multiple opportunities for enemies to be assigned
- [ ] Max 2 enemies per firing tower maintained

### Scenario 5: Rapid Fire Traits
- [ ] Trait with low cooldown (e.g., 0.5s) fires frequently
- [ ] Each shot triggers OnTowerFired
- [ ] Enemies continuously assigned/reassigned to counterattack
- [ ] System handles rapid event firing without issues

## Performance Considerations

### Event Frequency
- Normal tower: Fires every `attackSpeed` seconds (typically 1-3s)
- Trait projectile: Fires every `projectileCooldown` seconds (typically 0.5-2s)
- Both use same event system, no additional overhead

### Counterattack Assignment Cost
- **TowerDefenseCoordinator.OnTowerFired()**: O(n) where n = total enemies alive
- Filters corn-less enemies, checks if already assigned
- Assigns up to 2 enemies per firing tower
- Efficient even with 50+ enemies and multiple firing towers

## Related Systems

### TowerDefenseCoordinator
**File:** `Assets/Scripts/Managers/TowerDefenseCoordinator.cs`

Listens to `OnTowerFired` and manages counterattack assignments:
- Finds up to 2 corn-less enemies not already assigned
- Calls `enemy.AssignToCounterattack(tower)`
- Maintains dictionary of tower → enemies assignments

### Enemy State Machine
**File:** `Assets/Scripts/Enemy/Enemy.cs`

Enemies transition to `CounterattackingTower` state when assigned:
- Moves toward tower using grid-aligned movement
- Attacks tower until destroyed or cancelled
- Returns to stealing behavior if tower destroyed

### Tower.OnTowerFired Event
**File:** `Assets/Scripts/Tower/Tower.cs`

Public event that any system can subscribe to:
```csharp
public System.Action<Tower, Enemy> OnTowerFired;
```

Currently subscribed by:
1. **TowerDefenseCoordinator** - Assigns counterattackers

## Future Enhancements

### Potential Improvements
- [ ] **Cooldown per tower**: Prevent same tower from triggering assignments too frequently
- [ ] **Priority targeting**: Assign faster/stronger enemies to counterattack dangerous towers
- [ ] **Range consideration**: Prioritize enemies that are closer to the firing tower
- [ ] **Trait-specific behavior**: Some traits might trigger different counterattack patterns

### Debugging Tools
- [ ] Visual indicator when OnTowerFired triggers
- [ ] Console log showing which enemies assigned to which tower
- [ ] Gizmo lines from assigned enemies to their target tower
- [ ] UI showing current tower assignments

## References

- Reactive Defense System: `docs/REACTIVE_DEFENSE_SYSTEM.md`
- Flow Field Implementation: `docs/FLOW_FIELD_IMPLEMENTATION.md`
- Trait Projectile System: `docs/TRAIT_PROJECTILE_SYSTEM.md`
- GitHub Copilot Instructions: `.github/copilot-instructions.md`
