# Trait-Based Projectile System

## Overview
Towers can now shoot multiple independent projectiles in parallel, with each trait defining its own projectile type, fire rate, damage, and impact effects.

## How It Works

### Architecture
```
Tower
  ├── TowerTraitManager
  │     ├── Trait 1 (Explosion)
  │     │     └── TraitProjectileSystem (fires every 2 seconds)
  │     └── Trait 2 (Earth)
  │           └── TraitProjectileSystem (fires every 3 seconds)
  └── Default shooting (still works normally)
```

### Key Components

#### 1. TowerTrait (ScriptableObject)
**New Fields:**
- `hasIndependentProjectile` - Enable trait-specific projectiles
- `projectilePrefab` - Trait's projectile prefab
- `projectileAttackSpeed` - Attacks per second (e.g., 1.0 = 1 shot/sec, 0.5 = 1 shot every 2 sec)
- `projectileSpeed` - Projectile travel speed
- `projectileDamage` - Damage per shot
- `projectileDamageType` - Physical/Magic/etc
- `projectileImpactEffect` - Visual effect on impact
- `projectileAppliesTraitEffects` - If true, applies trait effects (burn, slow, earth hole, etc.)

#### 2. TraitProjectileSystem (MonoBehaviour)
- Manages independent firing for one trait
- Tracks fire rate independently
- Fires at tower's current target
- Respects tower's range
- Automatically created/destroyed with traits

#### 3. Projectile Updates
- Can now be marked as "trait projectile"
- Applies only that trait's effects (not all tower traits)
- Supports custom impact effects per trait

## Configuration Example

### Explosion Trait (Fires every 2 seconds)
```
Has Independent Projectile: ✓
Projectile Prefab: ExplosionProjectile
Attack Speed: 0.5 (= 1 shot every 2 seconds)
Damage: 50
Damage Type: Magic
Impact Effect: ExplosionParticles
Applies Trait Effects: ✓ (explosion AoE damage)
```

### Earth Trait (Fires every 3 seconds)
```
Has Independent Projectile: ✓
Projectile Prefab: EarthProjectile
Attack Speed: 0.33 (= 1 shot every 3 seconds)
Damage: 30
Damage Type: Magic
Impact Effect: EarthHoleSpawn
Applies Trait Effects: ✓ (creates black disk trap)
```

## Usage

### Setup in Unity Editor

1. **Select a trait asset** (e.g., `Assets/Resources/Traits/Explosion.asset`)

2. **Enable independent projectile:**
   - Check `Has Independent Projectile`

3. **Configure projectile:**
   - `Projectile Prefab`: Assign your projectile prefab
   - `Projectile Attack Speed`: 1.0 for 1/sec, 0.5 for every 2 sec, 0.33 for every 3 sec
   - `Projectile Speed`: 10-15 typical
   - `Projectile Damage`: Base damage
   - `Projectile Damage Type`: Magic recommended for traits
   - `Projectile Applies Trait Effects`: ✓ to apply trait effects

4. **Test:**
   - Build tower in game
   - Apply the trait
   - Tower now fires TWO projectiles: default + trait projectile

### Example: Tower with 2 Traits

```
Tower Configuration:
- Base attack: 1 shot/second (default projectile)
- Explosion trait: 1 shot/2 seconds (explosion projectile)
- Earth trait: 1 shot/3 seconds (earth projectile)

Result:
- 3 independent projectile streams
- Each fires at its own rate
- Each applies its own effects
```

## Behavior

### What Gets Applied

#### Default Tower Projectile:
- Applies ALL trait effects (current behavior maintained)
- Fire, Ice, Lightning, etc. all apply together
- Fires at tower's attack speed

#### Trait-Specific Projectile:
- Applies ONLY that trait's effects
- Example: Earth projectile only creates black disk
- Fires at trait's projectileAttackSpeed
- Independent of tower's attack speed

### Attack Speed Examples
```
projectileAttackSpeed = 1.0  → 1 shot/second (1.0s cooldown)
projectileAttackSpeed = 0.5  → 1 shot every 2 seconds (2.0s cooldown)
projectileAttackSpeed = 0.33 → 1 shot every 3 seconds (3.0s cooldown)
projectileAttackSpeed = 2.0  → 2 shots/second (0.5s cooldown)
```

### Targeting
- All projectile systems use the tower's current target
- If tower has no target, trait projectiles don't fire
- Range check: trait projectiles respect tower's range (including range bonuses from traits)

### Attack Speed vs Cooldown
- **Attack Speed**: How many times per second the trait shoots
- **Cooldown**: Time between shots = 1 / Attack Speed
- Example: Attack Speed 0.5 = Cooldown 2 seconds

## Advantages

### Gameplay
1. **Trait Identity**: Each trait feels unique with its own attack pattern
2. **Visual Distinction**: Different projectiles for different effects
3. **Strategic Depth**: Players see exactly which trait is doing what
4. **Combo Potential**: Traits work together with different timing

### Technical
1. **Modular**: Easy to add new trait projectiles
2. **Independent**: Each trait's firing doesn't affect others
3. **Flexible**: Can mix trait projectiles with default tower attack
4. **Scalable**: Performance-friendly (each system is lightweight)

## Backward Compatibility

✅ **Fully Backward Compatible**
- Traits without `hasIndependentProjectile` work as before
- Default tower shooting still functions normally
- Existing trait effects (burn, slow, chain, etc.) unchanged
- No changes needed to existing trait assets unless you want projectiles

## Performance Notes

- Each `TraitProjectileSystem` is a lightweight component
- Only updates when checking fire cooldown
- Projectile creation same as before
- Recommended max: 3-4 trait projectiles per tower

## Debug Logging

### When trait with projectile added:
```
✓ Setup projectile system for trait 'Earth'
TraitProjectileSystem initialized for 'Earth' on Tower_01
  Attack Speed: 0.33/sec, Damage: 30
```

### When projectile fires:
```
Earth fired projectile at Enemy_05
Trait projectile from 'Earth': Applying effects to Enemy_05
```

### When trait removed:
```
Removed projectile system for trait 'Earth'
TraitProjectileSystem for 'Earth' destroyed
```

## Future Enhancements

Possible additions:
- **Custom fire points per trait** (projectiles spawn from different positions)
- **Targeting overrides** (trait targets closest, farthest, strongest, etc.)
- **Charge-up mechanics** per trait
- **Burst fire** (fire X projectiles in quick succession)
- **Homing behavior** per trait projectile
- **Visual charging indicators** for slow-firing trait projectiles

## Example Trait Configurations

### Rapid Fire Trait
```
Attack Speed: 2.0 (2 shots/second)
Damage: 15 (lower per shot, higher DPS)
Speed: 20 (fast projectiles)
```

### Heavy Hitter Trait
```
Attack Speed: 0.25 (1 shot every 4 seconds)
Damage: 100 (huge damage)
Speed: 5 (slow but devastating)
```

### Support Trait
```
Attack Speed: 1.0 (steady)
Damage: 0 (doesn't damage)
Applies Trait Effects: ✓ (applies buff/debuff only)
```

---

**Status**: ✅ Implemented and working
**Version**: 1.0
**Date**: October 13, 2025
