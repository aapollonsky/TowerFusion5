# Projectile System - Current Features & Capabilities

## 📦 Current Projectile Prefabs

Your game currently has **2 projectile prefabs**:

1. **`Projectile.prefab`** - Basic projectile
2. **`AdvancedProjectile.prefab`** - Advanced projectile with more features

Both are located in: `Assets/Prefab/`

## 🎯 Damage Types Supported

The game supports **4 damage types**:

```csharp
public enum DamageType
{
    Physical,  // Default physical damage
    Magic,     // Magical damage (used by chain lightning)
    Fire,      // Fire damage (can apply burn effects)
    Ice        // Ice damage (can apply slow effects)
}
```

Each enemy can have different resistances to these damage types!

## ⚙️ Current Projectile Features

### ✅ Implemented Features:

1. **Basic Movement**
   - Flies in a straight line toward target
   - Uses Rigidbody2D physics
   - Rotates to face movement direction

2. **Target Prediction**
   - Leads moving targets for better accuracy
   - Predicts enemy position 0.2 seconds ahead
   - Improves hit rate on fast enemies

3. **Collision Detection**
   - Trigger-based collision (OnTriggerEnter2D)
   - Distance-based impact check (0.1 unit threshold)
   - Handles dead or missing targets

4. **Impact Effects**
   - Visual particle burst on hit ✨
   - Auto-destroys after displaying
   - Position-based spawning

5. **Damage Application**
   - Direct damage to primary target
   - Damage type support (Physical/Magic/Fire/Ice)
   - Integration with tower trait system

6. **Special Effects**
   - **Splash Damage** - hits enemies in radius
   - **Slow Effects** - reduces enemy speed
   - **Poison Effects** - damage over time
   - **Trait Effects** - applies tower traits (burn, chain lightning, etc.)

7. **Cleanup**
   - Auto-destroys after 5 seconds (failsafe)
   - Destroys on impact
   - No memory leaks

8. **Customization**
   - Custom sprites via `SetSprite()`
   - Custom colors via `SetColor()`
   - Trail effects support (assign prefab)

## 🚧 Features Marked for Future Implementation

### ⏳ Partially Implemented (Ready to Extend):

1. **Homing Projectiles** 🎯
   ```csharp
   // Current state: Placeholder method exists
   private void UpdateTargeting()
   {
       // For now, projectiles don't home. 
       // This can be extended for homing missiles
   }
   ```
   
   **To Add Homing:**
   - Enable continuous target tracking
   - Adjust velocity each frame toward target
   - Add `homingStrength` parameter for turn rate

2. **Trail Effects** ✨
   ```csharp
   // Already supported in code!
   if (trailEffectPrefab != null)
   {
       Instantiate(trailEffectPrefab, transform);
   }
   ```
   
   **To Add Trails:**
   - Create a trail particle prefab
   - Assign to `trailEffectPrefab` field
   - Particles will follow projectile automatically

## 🎨 Projectile Visual Customization

### Current Customization Options:

```csharp
// Change sprite
projectile.SetSprite(myCustomSprite);

// Change color
projectile.SetColor(Color.red);  // Fire projectile
projectile.SetColor(Color.cyan); // Ice projectile
projectile.SetColor(Color.green); // Poison projectile
```

### Components You Can Modify:

- **SpriteRenderer** - Visual appearance
- **Rigidbody2D** - Physics behavior
- **Collider2D** - Hit detection
- **Impact Effect** - Particle burst on hit
- **Trail Effect** - Particles following projectile

## 📊 Projectile Flow Diagram

```
Tower Fires → Projectile Created
    ↓
Initialize(target, damage, type, speed, tower)
    ↓
SetupMovement() - Calculate velocity & rotation
    ↓
Every Frame: Update()
    ├─ UpdateTargeting() - (Future: homing behavior)
    └─ CheckForImpact() - Distance check
    ↓
OnTriggerEnter2D() OR Distance Check
    ↓
Impact(enemy)
    ├─ Deal primary damage
    ├─ Apply trait effects
    ├─ Apply special effects (slow/poison)
    ├─ Apply splash damage
    └─ Create impact effect ✨
    ↓
Destroy projectile
```

## 🔧 Technical Specifications

### Fields & Configuration:

| Field | Type | Purpose |
|-------|------|---------|
| `spriteRenderer` | SpriteRenderer | Visual display |
| `rb2D` | Rigidbody2D | Movement physics |
| `projectileCollider` | Collider2D | Hit detection |
| `impactEffectPrefab` | GameObject | Particle effect on hit |
| `trailEffectPrefab` | GameObject | Particle trail (optional) |
| `targetEnemy` | Enemy | Target to hit |
| `damage` | float | Damage amount |
| `damageType` | DamageType | Type of damage |
| `speed` | float | Movement speed |
| `originTower` | Tower | Source tower (for traits) |
| `originTowerData` | TowerData | Tower configuration |

### Key Methods:

```csharp
// Initialization
Initialize(enemy, damage, type, speed)
Initialize(enemy, damage, type, speed, tower) // With trait support

// Configuration
SetSpecialEffects(towerData)
SetSprite(sprite)
SetColor(color)

// Movement
SetupMovement() - Initial velocity calculation
UpdateTargeting() - Frame-by-frame targeting

// Impact
Impact(enemy) - Handle hit
ApplySpecialEffects(enemy) - Slow, poison, etc.
ApplySplashDamage() - Area damage
CreateImpactEffect() - Visual feedback
```

## 🚀 Easy Expansion Points

### 1. Add Homing Missiles

Modify `UpdateTargeting()`:
```csharp
private void UpdateTargeting()
{
    if (!isHoming || targetEnemy == null || !targetEnemy.IsAlive)
        return;
    
    Vector3 direction = (targetEnemy.Position - transform.position).normalized;
    rb2D.velocity = Vector3.Lerp(rb2D.velocity, direction * speed, homingStrength * Time.deltaTime);
    
    // Update rotation
    float angle = Mathf.Atan2(rb2D.velocity.y, rb2D.velocity.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
}
```

### 2. Add Gravity (Arrows)

Already has field support! Just enable:
```csharp
[SerializeField] private bool hasGravity = true;
[SerializeField] private float gravityMultiplier = 1f;
```

Then in `Update()`:
```csharp
if (hasGravity)
{
    rb2D.velocity += Vector2.down * gravityMultiplier * Time.deltaTime;
}
```

### 3. Add Piercing Projectiles

Track hit enemies:
```csharp
private List<Enemy> hitEnemies = new List<Enemy>();
private int maxPierceCount = 3;

private void OnTriggerEnter2D(Collider2D other)
{
    Enemy enemy = other.GetComponent<Enemy>();
    if (enemy != null && !hitEnemies.Contains(enemy))
    {
        hitEnemies.Add(enemy);
        enemy.TakeDamage(damage, damageType);
        
        if (hitEnemies.Count >= maxPierceCount)
        {
            Impact(enemy); // Final impact
        }
    }
}
```

### 4. Add Different Impact Effects per Damage Type

```csharp
private void CreateImpactEffect()
{
    GameObject effectToUse = impactEffectPrefab;
    
    // Choose effect based on damage type
    switch (damageType)
    {
        case DamageType.Fire:
            effectToUse = fireImpactEffect;
            break;
        case DamageType.Ice:
            effectToUse = iceImpactEffect;
            break;
        case DamageType.Magic:
            effectToUse = magicImpactEffect;
            break;
    }
    
    if (effectToUse != null)
    {
        Instantiate(effectToUse, transform.position, Quaternion.identity);
    }
}
```

## 🎯 Recommended Next Steps

### Quick Additions:

1. **Create themed impact effects** for each damage type:
   - Fire: Orange/red burst
   - Ice: Blue/white crystalline
   - Magic: Purple/pink sparkles
   - Physical: Yellow burst (current default)

2. **Add trail effects** for visual feedback:
   - Arrows: White streak
   - Fireballs: Orange flames
   - Magic bolts: Colored sparkles

3. **Implement homing** for advanced towers:
   - Guided missiles
   - Magic seeking bolts
   - Heat-seeking projectiles

4. **Add projectile variations**:
   - Slow heavy cannonballs (gravity)
   - Fast piercing arrows (pierce)
   - Explosive shells (large splash)

## 📝 Summary

**Current Status:**
- ✅ Solid base projectile system
- ✅ Multiple damage types
- ✅ Splash damage support
- ✅ Trait integration
- ✅ Impact effects working
- ✅ Customizable visuals
- ⏳ Ready for easy expansion

**What Works:**
- Basic projectiles fly and hit targets accurately
- Impact effects show on hit
- Damage types supported
- Special effects (splash, slow, poison)
- Tower traits applied on hit
- Auto-cleanup prevents memory leaks

**Easy to Add:**
- Homing missiles (method stub ready)
- Gravity/arcing projectiles (fields ready)
- Trail effects (system ready)
- Piercing projectiles (straightforward)
- Typed impact effects (simple switch statement)

Your projectile system is production-ready and easily extensible! 🎉
