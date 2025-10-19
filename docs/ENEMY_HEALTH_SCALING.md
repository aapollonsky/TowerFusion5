# Enemy Health Scaling Per Wave

## Overview
Enemy health automatically increases with each wave to provide progressive difficulty scaling.

## How It Works

### Formula
```
Scaled Health = Base Health × (1.0 + (Wave - 1) × Scaling Percentage)
```

**Capped at Maximum Multiplier**

### Default Settings
- **Health Scaling Per Wave:** 15% (0.15)
- **Max Health Multiplier:** 3.0x (300% of base)

### Examples with Defaults

| Wave | Calculation | Multiplier | Enemy (100 HP Base) |
|------|-------------|------------|---------------------|
| 1 | 100 × 1.0 | 1.0x | **100 HP** |
| 2 | 100 × 1.15 | 1.15x | **115 HP** |
| 3 | 100 × 1.30 | 1.30x | **130 HP** |
| 5 | 100 × 1.60 | 1.60x | **160 HP** |
| 10 | 100 × 2.35 | 2.35x | **235 HP** |
| 15 | 100 × 3.0 (capped) | 3.0x | **300 HP** |
| 20 | 100 × 3.0 (capped) | 3.0x | **300 HP** |

## Configuration

### In Unity Inspector

1. Select `GameManager` in Hierarchy
2. Look for **"Enemy Scaling"** section
3. Adjust these settings:

```
Enemy Scaling
├─ Enable Health Scaling: ✓
├─ Health Scaling Per Wave: 0.15 (slider 0.0 - 1.0)
└─ Max Health Multiplier: 3.0 (slider 1.0 - 10.0)
```

### Settings Explained

**Enable Health Scaling** (checkbox)
- ✓ Enabled: Health scales with waves
- ✗ Disabled: Enemies always use base health

**Health Scaling Per Wave** (0.0 - 1.0)
- `0.10` = 10% more health per wave (gentle scaling)
- `0.15` = 15% more health per wave (default, balanced)
- `0.20` = 20% more health per wave (aggressive scaling)
- `0.50` = 50% more health per wave (extreme difficulty)

**Max Health Multiplier** (1.0 - 10.0)
- `2.0` = Cap at 200% of base health
- `3.0` = Cap at 300% of base health (default)
- `5.0` = Cap at 500% of base health (very high cap)
- `10.0` = Cap at 1000% of base health (extreme cap)

## Customization Examples

### Example 1: Gentle Scaling (Easy Mode)
```
Enable Health Scaling: ✓
Health Scaling Per Wave: 0.10 (10% per wave)
Max Health Multiplier: 2.0 (200% max)
```

**Results:**
- Wave 1: 100 HP (1.0x)
- Wave 5: 140 HP (1.4x)
- Wave 10: 190 HP (1.9x)
- Wave 11+: 200 HP (2.0x cap reached)

---

### Example 2: Aggressive Scaling (Hard Mode)
```
Enable Health Scaling: ✓
Health Scaling Per Wave: 0.25 (25% per wave)
Max Health Multiplier: 5.0 (500% max)
```

**Results:**
- Wave 1: 100 HP (1.0x)
- Wave 3: 150 HP (1.5x)
- Wave 5: 200 HP (2.0x)
- Wave 10: 325 HP (3.25x)
- Wave 17+: 500 HP (5.0x cap reached)

---

### Example 3: Linear Increase (No Cap Concerns)
```
Enable Health Scaling: ✓
Health Scaling Per Wave: 0.05 (5% per wave)
Max Health Multiplier: 10.0 (1000% max)
```

**Results:**
- Wave 1: 100 HP (1.0x)
- Wave 10: 145 HP (1.45x)
- Wave 20: 195 HP (1.95x)
- Wave 50: 345 HP (3.45x) - Still under cap

---

### Example 4: Explosive Late Game
```
Enable Health Scaling: ✓
Health Scaling Per Wave: 0.30 (30% per wave)
Max Health Multiplier: 3.0 (300% max)
```

**Results:**
- Wave 1: 100 HP (1.0x)
- Wave 2: 130 HP (1.3x)
- Wave 3: 160 HP (1.6x)
- Wave 5: 220 HP (2.2x)
- Wave 7+: 300 HP (3.0x cap reached)

**Effect:** Difficulty ramps up FAST, caps early

---

### Example 5: Disabled Scaling
```
Enable Health Scaling: ✗ (unchecked)
```

**Results:**
- All waves: Base health only (e.g., 100 HP)
- No scaling whatsoever

---

## System Behavior

### When Enemy Spawns

1. **EnemyManager spawns enemy** with base EnemyData
2. **Enemy.Initialize()** is called
3. **Checks GameManager** for current wave number
4. **Calculates scaled health** using formula
5. **Sets currentHealth and scaledMaxHealth**
6. **Logs scaling** to Console (if health changed)

### Console Output

**Wave 1 (No Scaling):**
```
Enemy spawned - no log (health = base)
```

**Wave 2 (With Scaling):**
```
[Enemy] FastEnemy health scaled: 80 → 92 (Wave 2)
[Enemy] TankEnemy health scaled: 200 → 230 (Wave 2)
```

**Wave 10:**
```
[Enemy] BasicEnemy health scaled: 100 → 235 (Wave 10)
```

### Health Bar Display

- Health bars automatically use scaled max health
- Percentage calculations are accurate
- Health bar shows relative to scaled max, not base

### What Gets Scaled

✅ **Enemy MaxHealth** - Yes, scales per wave
✅ **Current Health** - Yes, starts at scaled max
✅ **Health Bar Display** - Yes, uses scaled max

❌ **Enemy Speed** - No, remains constant
❌ **Damage** - No, remains constant
❌ **Gold Reward** - No, remains constant
❌ **Other Stats** - No, only health scales

## Balancing Guidelines

### Consider These Factors

**1. Tower Damage Output**
- High DPS towers → Can handle higher health scaling
- Low DPS towers → Need gentler health scaling

**2. Wave Count**
- Many waves (20+) → Lower scaling per wave
- Few waves (5-10) → Higher scaling per wave

**3. Player Progression**
- Fast tower upgrades → Higher scaling works
- Slow economy → Lower scaling needed

**4. Corn Theft Pressure**
- High corn theft → Lower scaling (players already stressed)
- Low corn theft → Higher scaling (add challenge)

**5. Enemy Variety**
- Many enemy types → Lower scaling (variety provides challenge)
- Few enemy types → Higher scaling (needs difficulty from health)

### Recommended Presets

**Casual:**
```
Scaling: 0.08 (8%)
Max: 2.0x
Wave 10: ~172% health
```

**Normal (Default):**
```
Scaling: 0.15 (15%)
Max: 3.0x
Wave 10: ~235% health
```

**Hard:**
```
Scaling: 0.20 (20%)
Max: 4.0x
Wave 10: ~280% health
```

**Expert:**
```
Scaling: 0.25 (25%)
Max: 5.0x
Wave 10: ~325% health
```

**Nightmare:**
```
Scaling: 0.30 (30%)
Max: 10.0x
Wave 10: ~370% health
```

## Technical Details

### Code Implementation

**GameManager.cs:**
```csharp
public float GetScaledEnemyHealth(float baseHealth)
{
    if (!enableHealthScaling || currentWave <= 1)
        return baseHealth;
    
    float multiplier = 1.0f + ((currentWave - 1) * healthScalingPerWave);
    multiplier = Mathf.Min(multiplier, maxHealthMultiplier);
    
    return baseHealth * multiplier;
}
```

**Enemy.cs:**
```csharp
public void Initialize(EnemyData data)
{
    float baseHealth = data.maxHealth;
    float scaledHealth = GameManager.Instance.GetScaledEnemyHealth(baseHealth);
    
    currentHealth = scaledHealth;
    scaledMaxHealth = scaledHealth;
    
    // Rest of initialization...
}
```

### Performance

**Very Lightweight:**
- One calculation per enemy spawn
- Simple multiplication (O(1))
- No per-frame overhead
- Negligible memory impact

## Testing

### Quick Test

1. **Set scaling values** in GameManager Inspector
2. **Press Play**
3. **Start Wave 1** - Note enemy health
4. **Start Wave 2** - Check Console for scaling log
5. **Verify:** Health should be higher in Wave 2

### Full Test

1. Complete multiple waves
2. Check Console logs for scaling confirmation
3. Verify health increases match formula
4. Test with different enemy types
5. Verify health bars display correctly

### Console Verification

Look for these logs:
```
[Enemy] BasicEnemy health scaled: 100 → 115 (Wave 2)
[Enemy] BasicEnemy health scaled: 100 → 130 (Wave 3)
[Enemy] BasicEnemy health scaled: 100 → 160 (Wave 5)
```

## Troubleshooting

### "Health doesn't seem to scale"

**Check:**
1. GameManager → Enable Health Scaling is ✓
2. You're on Wave 2+ (Wave 1 has no scaling)
3. Console shows scaling logs
4. Health Scaling Per Wave > 0

### "Enemies too strong/weak"

**Adjust:**
- Too strong → Lower Health Scaling Per Wave
- Too weak → Raise Health Scaling Per Wave
- Or adjust tower damage instead

### "Scaling stops increasing"

**Cause:** Hit the Max Health Multiplier cap

**Solution:**
- Increase Max Health Multiplier
- Or accept the cap (intended behavior)

### "Console spam with scaling logs"

**Normal:** One log per enemy spawn with scaling

**If excessive:** Reduce enemy count per wave

## Advanced: Per-Enemy-Type Scaling

If you want **different scaling** for different enemy types, you could:

### Option 1: Base Health Variation
- Set different base health in EnemyData
- Scaling multiplier applies to all
- Fast enemies: Low base (80 HP) → scales slower in absolute terms
- Tank enemies: High base (300 HP) → scales faster in absolute terms

### Option 2: Custom Scaling (Code Modification)
```csharp
// In Enemy.Initialize(), add:
float customScaling = enemyData.isFast ? 0.10f : healthScalingPerWave;
float multiplier = 1.0f + ((currentWave - 1) * customScaling);
```

### Option 3: Enemy-Specific Multipliers
```csharp
// In EnemyData.cs, add:
public float healthScalingMultiplier = 1.0f;

// In GameManager.GetScaledEnemyHealth(), modify:
float scaledHealth = baseHealth * multiplier * enemyData.healthScalingMultiplier;
```

## Summary

**Quick Steps:**
1. Select GameManager
2. Enemy Scaling section
3. Adjust sliders
4. Test in Play mode

**Formula:**
```
Health = Base × (1 + (Wave-1) × Scaling) [capped at Max]
```

**Defaults:**
- 15% per wave
- 3x max
- Wave 1: 1x, Wave 10: 2.35x

Tune to match your game's difficulty curve! 💪✨
