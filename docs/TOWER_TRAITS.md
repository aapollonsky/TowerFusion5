# Tower Traits System - Complete Guide

## 🎯 Overview

Tower Traits are special modifiers that can be applied to towers to enhance their abilities and add special effects. Traits can modify stats, add visual effects, and grant special abilities like burn, freeze, or chain lightning.

---

## 🔥 Currently Supported Traits

Your game has **7 pre-defined traits** ready to use:

### 1. **Fire** 🔥
**Category:** Elemental  
**Description:** "+50% damage, burn DoT (3 seconds)"

**Effects:**
- ✅ **+50% Damage** - Increases base damage by 50**6. Earth + Ice = "Permafrost Zone"**
- Earth creates holes on kills (enemies disappear)
- Ice slows enemies into holes
- **Result:** Lethal kill zones that delete entire groups

**7. Explosion + Harvest = "Gold Rush"**
- Explosion kills multiple enemies
- Harvest gives gold for each kill
- **Result:** Maximum economy efficiency

**8. Earth + Explosion = "Crater Field"**
- Explosion kills multiple enemies at once
- Each death creates a hole
- Multiple holes = deadly minefield
- **Result:** Self-perpetuating kill zones

---n Effect** - Enemies take 10 damage per second for 3 seconds
- 🎨 **Visual:** Red overlay (40% opacity)

**Best For:**
- High single-target damage
- Damage over time strategy
- Bosses and tanky enemies

**Configuration:**
```csharp
damageMultiplier = 1.5f
hasBurnEffect = true
burnDamagePerSecond = 10f
burnDuration = 3f
overlayColor = Color.red
```

---

### 2. **Ice** ❄️
**Category:** Elemental  
**Description:** "-30% enemy speed, brittle effect (+25% incoming damage)"

**Effects:**
- ✅ **Slow Effect** - Reduces enemy speed by 30% for 2 seconds
- ✅ **Brittle Effect** - Enemy takes +25% damage from all sources for 3 seconds
- 🎨 **Visual:** Cyan overlay (40% opacity)

**Best For:**
- Crowd control
- Supporting other towers
- Fast enemies
- Setup for burst damage

**Configuration:**
```csharp
hasSlowEffect = true
slowMultiplier = 0.7f  // 30% reduction
slowDuration = 2f

hasBrittleEffect = true
brittleDamageMultiplier = 1.25f  // +25% incoming damage
brittleDuration = 3f

overlayColor = Color.cyan
```

---

### 3. **Lightning** ⚡
**Category:** Elemental  
**Description:** "Chain to 2 additional enemies"

**Effects:**
- ✅ **Chain Lightning** - Hits primary target, then chains to 2 nearby enemies
- ✅ **Full Damage** - Chain targets take 100% damage (chainDamageMultiplier = 1.0)
- ✅ **2 unit range** - Chains within 2 units of each target
- 🎨 **Visual:** Yellow overlay (40% opacity)

**Best For:**
- Grouped enemies
- Wave clear
- Multi-target scenarios
- Efficient damage distribution

**Configuration:**
```csharp
hasChainEffect = true
chainTargets = 2
chainDamageMultiplier = 1f  // 100% damage
chainRange = 2f

overlayColor = Color.yellow
```

**How It Works:**
1. Tower hits Enemy A (full damage)
2. Lightning chains to Enemy B within 2 units (full damage)
3. Lightning chains to Enemy C within 2 units of B (full damage)
4. Total: 3 enemies damaged from one attack!

---

### 4. **Sniper** 🎯
**Category:** Range  
**Description:** "+100% range, +2 second charge time"

**Effects:**
- ✅ **Double Range** - Attack range increased by 100%
- ⏱️ **Charge Time** - Tower must charge for 2 seconds before each shot
- 🎨 **Visual:** Green overlay (30% opacity)

**Best For:**
- Long-range strategy
- Positioning flexibility
- High-value targets
- Patient playstyle

**Configuration:**
```csharp
rangeMultiplier = 2f  // +100% range
chargeTimeBonus = 2f  // +2 seconds

overlayColor = Color.green
```

**Gameplay:**
- Tower shows charging indicator
- Can't fire while charging
- After charge completes: BOOM! Double range attack

---

### 5. **Harvest** 💰
**Category:** Utility  
**Description:** "+1 gold per kill"

**Effects:**
- ✅ **Gold Bonus** - Earn +1 gold for each enemy killed by this tower
- 🎨 **Visual:** Yellow overlay (20% opacity)

**Best For:**
- Economic strategy
- Early game investment
- Gold farming
- Long term gains

**Configuration:**
```csharp
hasGoldReward = true
goldPerKill = 1

overlayColor = Color.yellow
overlayAlpha = 0.2f
```

**Economics:**
- Kill 50 enemies = +50 gold
- Can fund additional towers
- Compounds over time

---

### 6. **Explosion** 💥
**Category:** Elemental  
**Description:** "Impact causes explosion dealing 75% damage in 2 unit radius"

**Effects:**
- ✅ **Area Damage** - Deals 75% of attack damage to all enemies within 2 units
- ✅ **Splash Effect** - Multiple enemies take damage from single projectile
- 🎨 **Visual:** Orange overlay (40% opacity), expanding explosion visual

**Best For:**
- Grouped enemies
- Wave clear
- Maximizing damage efficiency
- Dense formations

**Configuration:**
```csharp
hasExplosionEffect = true
explosionRadius = 2f
explosionDamageMultiplier = 0.75f  // 75% damage

overlayColor = new Color(1f, 0.5f, 0f)  // Orange
```

**How It Works:**
1. Tower shoots projectile at Enemy A
2. Projectile hits Enemy A (100% damage)
3. Explosion occurs at impact point
4. All enemies within 2 units take 75% damage
5. Great for tightly packed enemy groups!

**Example:**
- Tower damage: 100
- Direct hit: 100 damage
- Explosion: 75 damage to 3 nearby enemies
- **Total damage: 325!**

---

### 7. **Earth** 🌍
**Category:** Elemental  
**Description:** "Turns hit enemy into ground hole (3 seconds, enemies fall in and disappear)"

**Effects:**
- ✅ **Hole Creation** - When enemy dies, creates ground hole at death location
- ✅ **Instant Kill** - Enemies that touch the hole fall in and disappear
- ✅ **Swallow Animation** - Enemies shrink and spiral into hole over 0.5 seconds
- ✅ **Duration** - Hole lasts 3 seconds
- 🎨 **Visual:** Brown overlay (40% opacity), dark hole effect

**Best For:**
- Choke points
- Path denial
- High traffic areas
- Combo with slow effects

**Configuration:**
```csharp
hasEarthTrapEffect = true
trapDuration = 3f  // Duration of hole
trapRadius = 1f    // Hole detection radius

overlayColor = new Color(0.6f, 0.4f, 0.2f)  // Brown
```

**How It Works:**
1. Tower kills an enemy
2. Enemy's corpse transforms into ground hole
3. Hole remains for 3 seconds
4. Any enemy touching hole falls in (instant kill)
5. Swallow animation: enemy shrinks, spirals, disappears
6. Hole closes after 3 seconds

**Tactical Use:**
- Place Earth tower at path corners to create kill zones
- Combine with Ice trait to slow enemies into holes
- Use near spawn points to thin out waves
- Multiple holes can exist simultaneously
- Can eliminate entire groups if positioned well

**Kill Potential:**
- Single kill creates hole
- Hole can eliminate multiple enemies (no limit!)
- Best in high-density areas
- Strategic positioning = massive value

---

## 📊 Trait Categories

Traits are organized into **4 categories**:

| Category | Purpose | Example Traits |
|----------|---------|----------------|
| **Elemental** | Damage types & effects | Fire, Ice, Lightning, Explosion, Earth |
| **Range** | Distance modifiers | Sniper |
| **Utility** | Special abilities | Harvest |
| **Support** | Team buffs | (Available for custom traits) |

---

## 🎨 Visual Effects

Each trait adds visual feedback to towers:

### Overlay System:
- **Color Tint** - Tower sprite gets colored overlay
- **Adjustable Opacity** - Subtle to pronounced
- **Badge Icons** - Small icons float near tower
- **Particle Effects** - Optional particle systems
- **Animated Badges** - Floating/pulsing animations

### Default Visual Settings:
```csharp
overlayColor = Color.red;      // Trait color
overlayAlpha = 0.3f;           // 30% opacity
badgeScale = 1.2f;             // 120% size
animateBadge = true;           // Enable animations
badgeOffset = Vector2(0.8, 0.8); // Position offset
```

---

## 📏 Stat Modification System

Traits can modify **4 tower stats**:

### 1. **Damage**
```csharp
damageMultiplier = 1.5f;  // Multiply by 150%
damageBonus = 10f;        // Add flat +10 damage

// Formula: (baseDamage + bonus) * multiplier
// Example: (25 + 10) * 1.5 = 52.5 damage
```

### 2. **Range**
```csharp
rangeMultiplier = 2f;     // Multiply by 200%
rangeBonus = 1f;          // Add flat +1 range

// Formula: (baseRange + bonus) * multiplier
// Example: (3 + 1) * 2 = 8 range
```

### 3. **Attack Speed**
```csharp
attackSpeedMultiplier = 1.2f;  // 20% faster
attackSpeedBonus = 0.5f;       // +0.5 attacks/sec

// Formula: (baseSpeed + bonus) * multiplier
// Example: (1.0 + 0.5) * 1.2 = 1.8 attacks/sec
```

### 4. **Charge Time**
```csharp
chargeTimeBonus = 2f;     // +2 seconds

// Formula: baseCharge + bonus
// Example: 0 + 2 = 2 second charge time
```

---

## 🔧 Special Effect Types

### 1. **Burn Effect** 🔥
```csharp
hasBurnEffect = true;
burnDamagePerSecond = 10f;
burnDuration = 3f;
```
- Damage over time
- Independent of attack damage
- Stacks are refreshed, not added
- Visual: Fire particles on enemy

### 2. **Slow Effect** ❄️
```csharp
hasSlowEffect = true;
slowMultiplier = 0.7f;  // 70% speed = 30% reduction
slowDuration = 2f;
```
- Reduces enemy movement speed
- Does not stack (strongest effect wins)
- Visual: Blue tint on enemy

### 3. **Brittle Effect** 💎
```csharp
hasBrittleEffect = true;
brittleDamageMultiplier = 1.25f;  // +25% damage taken
brittleDuration = 3f;
```
- Increases all incoming damage
- Stacks multiplicatively
- Helps all towers damage this enemy
- Visual: Cracked ice overlay

### 4. **Chain Effect** ⚡
```csharp
hasChainEffect = true;
chainTargets = 2;
chainDamageMultiplier = 1f;
chainRange = 2f;
```
- Attacks jump to nearby enemies
- Each target can only be hit once per attack
- Visual: Lightning arc between enemies

### 5. **Gold Reward** 💰
```csharp
hasGoldReward = true;
goldPerKill = 1;
```
- Bonus gold on kill
- Applies only to kills by this tower
- Adds to base gold reward

### 6. **Explosion Effect** 💥
```csharp
hasExplosionEffect = true;
explosionRadius = 2f;
explosionDamageMultiplier = 0.75f;
```
- Deals area damage on impact
- Affects all enemies in radius
- Damage is percentage of main attack
- Visual: Expanding orange explosion

### 7. **Earth Trap Effect** 🌍
```csharp
hasEarthTrapEffect = true;
trapDuration = 3f;   // 3 seconds (changed from 4s)
trapRadius = 1f;
trapPrefab = null;  // Optional custom prefab
```
- Creates hole when enemy dies
- Enemies that touch hole fall in and disappear (instant kill)
- Swallow animation: shrink, spiral, vanish (0.5s)
- Hole persists for duration
- Visual: Very dark hole with depth gradient

---

## 🎮 How to Use Traits

### Method 1: Create Default Traits (Recommended)
```
1. In Unity menu: Tools > Tower Fusion > Create Default Traits
2. Traits are created in: Assets/Data/Traits/
3. Drag trait onto tower in Inspector
```

### Method 2: Add Trait to Tower at Runtime
```csharp
// Get tower
Tower tower = GetComponent<Tower>();

// Load trait
TowerTrait fireTrait = Resources.Load<TowerTrait>("Traits/Fire");

// Apply trait
tower.AddTrait(fireTrait);
```

### Method 3: Use Demo Tools
```
Tools > Tower Fusion > Demo: Add Fire Trait to Selected Tower
Tools > Tower Fusion > Demo: Add Lightning Trait to Selected Tower
```

---

## 🧪 Testing Traits

### Built-in Test Tools:

**Fire Trait:**
```
Tools > Tower Fusion > Test Fire Trait on Selected Enemy
Tools > Tower Fusion > Test Fire Trait via Tower Attack
```

**Ice Trait:**
```
Tools > Tower Fusion > Test Ice Trait on Selected Enemy
Tools > Tower Fusion > Test Ice Trait via Tower Attack
```

**Lightning Trait:**
```
TowerFusion > Debug > Lightning Trait Tester
```

**Visual Debug:**
```
Tools > Tower Fusion > Debug: Add Ice Trait with Enhanced Visuals
```

---

## 🔨 Creating Custom Traits

### Option 1: Use Unity Menu
```
1. Assets > Create > Tower Fusion > Tower Trait
2. Configure in Inspector
3. Save in Assets/Data/Traits/
```

### Option 2: Via Code
```csharp
var customTrait = ScriptableObject.CreateInstance<TowerTrait>();

// Basic Info
customTrait.traitName = "Explosive";
customTrait.description = "+200% splash damage";
customTrait.category = TraitCategory.Elemental;

// Stat Modifications
customTrait.damageMultiplier = 1.5f;
customTrait.rangeBonus = 1f;

// Special Effects
customTrait.hasBurnEffect = true;
customTrait.burnDamagePerSecond = 15f;
customTrait.burnDuration = 2f;

// Visual
customTrait.overlayColor = Color.red;
customTrait.overlayAlpha = 0.5f;

// Save as asset
AssetDatabase.CreateAsset(customTrait, "Assets/Data/Traits/Explosive.asset");
```

### Option 3: Extend Factory
Add to `TowerTraitFactory.cs`:
```csharp
private static void CreateExplosiveTrait(string path)
{
    var trait = ScriptableObject.CreateInstance<TowerTrait>();
    trait.name = "Explosive";
    // ... configuration ...
    AssetDatabase.CreateAsset(trait, $"{path}/Explosive.asset");
}
```

---

## 💡 Trait Combination Strategies

### Powerful Combos:

**1. Ice + Fire = "Thermal Shock"**
- Ice slows and makes brittle (+25% damage)
- Fire deals +50% damage + burn DoT
- **Result:** Massive damage burst on slowed targets

**2. Lightning + Harvest = "Storm Farmer"**
- Lightning hits 3 enemies per attack
- Harvest gives +3 gold total
- **Result:** Efficient gold generation

**3. Sniper + Fire = "Artillery"**
- Sniper doubles range (can hit backline)
- Fire adds +50% damage + burn
- **Result:** Long-range assassin

**4. Ice + Lightning = "Frozen Storm"**
- Ice slows and weakens entire group
- Lightning chains through grouped enemies
- **Result:** Crowd control + area damage

**5. Explosion + Fire = "Inferno"**
- Explosion hits multiple enemies at once
- Fire adds burn DoT to all hit enemies
- **Result:** Massive AoE damage over time

**6. Earth + Ice = "Permafrost Zone"**
- Earth creates traps on kills
- Ice slows enemies into traps
- **Result:** Lethal kill zones

**7. Explosion + Harvest = "Gold Rush"**
- Explosion can kill multiple enemies
- Harvest gives gold for each kill
- **Result:** Efficient economy building

---

## 📈 Trait Statistics

### Performance Metrics:

| Trait | Damage Increase | Special Benefit | Use Case |
|-------|----------------|-----------------|----------|
| Fire | +50% direct + burn | +30 total damage over 3s | Single target DPS |
| Ice | 0% | +25% team damage (brittle) | Support/CC |
| Lightning | 0% | Hits 3x enemies | Multi-target |
| Sniper | 0% | 2x range | Positioning |
| Harvest | 0% | +gold economy | Long term value |
| Explosion | 0% | 75% AoE damage | Grouped enemies |
| Earth | 0% | Instant kill holes (3s) | Path control |

### Best Traits by Situation:

| Situation | Recommended Trait | Reason |
|-----------|------------------|--------|
| Boss Fight | Fire | Maximum single-target DPS |
| Large Wave | Lightning | Efficient multi-target |
| Fast Enemies | Ice | Slow them down |
| Distant Targets | Sniper | Extended range |
| Early Game | Harvest | Build economy |
| Dense Groups | Explosion | AoE damage |
| Choke Points | Earth | Instant kill holes |
| Path Control | Earth | Multiple enemies per hole |

---

## 🐛 Troubleshooting

### "Trait not applying"
✅ **Check:** Tower has `TowerTraitManager` component  
✅ **Check:** Trait asset exists in Assets/Data/Traits/  
✅ **Check:** Console for error messages

### "No visual effect"
✅ **Check:** `overlayAlpha` > 0  
✅ **Check:** Tower has `SpriteRenderer`  
✅ **Check:** Overlay renderer is created

### "Chain lightning not working"
✅ **Check:** Multiple enemies within `chainRange`  
✅ **Check:** `hasChainEffect = true`  
✅ **Check:** `chainTargets` > 0

### "Burn damage not applying"
✅ **Check:** Enemy has `Enemy.cs` component  
✅ **Check:** `hasBurnEffect = true`  
✅ **Check:** `burnDamagePerSecond` > 0

### "Explosion not hitting nearby enemies"
✅ **Check:** Multiple enemies within `explosionRadius`  
✅ **Check:** `hasExplosionEffect = true`  
✅ **Check:** Enemies have colliders

### "Earth trap not appearing"
✅ **Check:** Enemy was killed (not just damaged)  
✅ **Check:** `hasEarthTrapEffect = true`  
✅ **Check:** `trapDuration` > 0  
✅ **Check:** Console for error messages

### "Enemies not falling into hole"
✅ **Check:** Enemies are touching the hole collider  
✅ **Check:** Hole radius is 1 unit  
✅ **Check:** Enemy has collider2D  
✅ **Check:** Hole hasn't expired yet (3 seconds)

---

## 📝 Trait File Locations

```
Assets/
├── Data/
│   └── Traits/
│       ├── Fire.asset
│       ├── Ice.asset
│       ├── Lightning.asset
│       ├── Sniper.asset
│       ├── Harvest.asset
│       ├── Explosion.asset
│       └── Earth.asset
├── Scripts/
│   ├── Tower/
│   │   ├── TowerTrait.cs          ← Trait definition
│   │   ├── TowerTraitManager.cs   ← Trait logic
│   │   └── Tower.cs               ← Integration
│   ├── Effects/
│   │   └── EarthTrap.cs           ← Earth trap behavior
│   └── Editor/
│       └── TowerTraitFactory.cs   ← Trait creation tool
```

---

## 🚀 Summary

**Currently Available:**
- ✅ 7 pre-defined traits (Fire, Ice, Lightning, Sniper, Harvest, Explosion, Earth)
- ✅ 4 stat modifiers (damage, range, speed, charge time)
- ✅ 7 special effect types (burn, slow, brittle, chain, gold, explosion, earth trap)
- ✅ Full visual system (overlays, badges, particles)
- ✅ Easy creation tools
- ✅ Testing utilities
- ✅ Trait combination support

**System Capabilities:**
- Unlimited custom traits
- Stack multiple traits on one tower
- Runtime trait application
- Visual feedback
- Balance-friendly stat system
- Extensible effect system

**Ready to expand with:**
- More elemental types (Earth, Wind, etc.)
- Support traits (team buffs, auras)
- Upgrade-based traits
- Synergy traits
- Evolving traits
- Conditional traits

Your trait system is production-ready and highly flexible! 🎉
