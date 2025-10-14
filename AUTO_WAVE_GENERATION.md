# Auto Wave Generation System 🌊

## Overview
The Auto Wave Generation system procedurally creates balanced, progressive waves of enemies based on difficulty settings. No manual wave configuration needed!

## Features

### 🎯 **Smart Enemy Scaling**
- Exponential enemy count growth with diminishing returns
- Health and difficulty progression over waves
- Balanced enemy type introduction

### ⚖️ **Difficulty Levels**
- **Level 1 - Easy**: 70% enemies, slower progression
- **Level 2 - Medium**: 100% enemies, standard progression
- **Level 3 - Hard**: 130% enemies, faster progression
- **Level 4 - Very Hard**: 160% enemies, aggressive progression
- **Level 5 - Extreme**: 200% enemies, maximum challenge

### 📊 **Wave Composition**
- **Early Waves (0-30%)**: Single enemy type, simple groups
- **Mid Waves (30-70%)**: 2-3 enemy types, mixed composition
- **Late Waves (70-100%)**: All available types, complex patterns

### 🔄 **Progressive Difficulty**
- Gradual introduction of harder enemies
- Increasing enemy counts with manageable cap
- Varied group sizes and timing for unpredictability

## Setup

### 1. Add WaveGenerator Component

In Unity, add the `WaveGenerator` component to your MapManager GameObject:

```
Hierarchy:
  └── MapManager
      ├── MapManager (script)
      └── WaveGenerator (script)  ← Add this
```

### 2. Configure WaveGenerator

In the Inspector:

**Auto-Generation Settings:**
- ✅ **Use Auto Generation**: Enable automatic wave generation
- **Difficulty Level**: 1 (Easy) to 5 (Extreme)
- **Number of Waves**: How many waves to generate (5-100)

**Enemy Pool:**
- Drag all available enemy types from `Assets/Data/Enemies/` into this list
- The generator will automatically select appropriate enemies per wave

**Balance Parameters:**
- **Enemy Count Growth**: % increase per wave (default: 10%)
- **Enemy Health Growth**: % health scaling per wave (default: 8%)
- **Base Enemy Count**: Starting number of enemies (default: 5)
- **Max Enemies Per Wave**: Cap to prevent overwhelming waves (default: 50)

### 3. Link to MapManager

In the MapManager Inspector:
- **Wave Generation** section → **Wave Generator**: Drag the WaveGenerator component here

### 4. Assign Enemy Pool

Drag enemy assets into the **Available Enemies** list:
```
Available Enemies (Size: 2+)
  └── Element 0: BasicEnemy
  └── Element 1: BasicEnemy2
  └── Element 2: [Your other enemies]
```

## How It Works

### Wave Generation Algorithm

1. **Calculate Enemy Count**
   ```
   count = baseCount * (1 + growth)^(wave-1) * difficultyMultiplier
   ```

2. **Select Enemy Types**
   - Early waves: Weakest enemies only
   - Mid waves: Mix of weak and medium enemies
   - Late waves: All enemy types including strongest

3. **Create Enemy Groups**
   - **Simple waves** (early): 1-2 large groups
   - **Complex waves** (late): 3-6 varied groups with delays

4. **Balance Timing**
   - Adjust spawn delays based on difficulty
   - Add randomness to group delays (±50%)

### Progression Curve

```
Wave 1-6:    Single enemy type, small groups
Wave 7-14:   Two enemy types, medium groups
Wave 15-20:  Three+ enemy types, large mixed groups
Wave 21+:    All enemies, chaotic composition
```

## Configuration Examples

### Easy Campaign (Beginner Friendly)
```
Difficulty Level: 1
Number of Waves: 15
Enemy Count Growth: 8%
Base Enemy Count: 4
Max Enemies Per Wave: 30
```

### Medium Campaign (Standard)
```
Difficulty Level: 2
Number of Waves: 20
Enemy Count Growth: 10%
Base Enemy Count: 5
Max Enemies Per Wave: 50
```

### Hard Campaign (Challenge)
```
Difficulty Level: 3
Number of Waves: 25
Enemy Count Growth: 12%
Base Enemy Count: 6
Max Enemies Per Wave: 70
```

### Endless Mode (Extreme)
```
Difficulty Level: 4-5
Number of Waves: 50+
Enemy Count Growth: 15%
Base Enemy Count: 8
Max Enemies Per Wave: 100
```

## Balancing Tips

### Enemy Pool Composition
For best results, include enemies with varied stats:
- **Basic enemies**: Low health, fast spawn
- **Tanky enemies**: High health, slow
- **Fast enemies**: Low health, high speed
- **Elite enemies**: High health, moderate speed

### Growth Rate Guidelines
- **Low (5-8%)**: Steady, manageable progression
- **Medium (10-12%)**: Standard challenge curve
- **High (15-20%)**: Aggressive, intense difficulty

### Wave Count Recommendations
- **Short campaign**: 10-15 waves
- **Medium campaign**: 20-30 waves
- **Long campaign**: 40-50 waves
- **Endless**: 50+ waves (will keep generating)

## Runtime Behavior

### First Generation
When the game starts:
1. MapManager checks if `UseAutoGeneration = true`
2. WaveGenerator generates all waves based on settings
3. Waves are cached for quick access

### Wave Access
```csharp
// MapManager automatically routes to generated waves
WaveData wave = MapManager.Instance.GetWaveData(waveNumber);
int totalWaves = MapManager.Instance.GetTotalWaves();
```

### Regeneration
To regenerate waves with new settings:
```csharp
waveGenerator.ClearGeneratedWaves();
waveGenerator.GenerateWaves();
```

## Comparison: Auto vs Manual Waves

| Feature | Auto-Generated | Manual |
|---------|---------------|--------|
| **Setup Time** | < 1 minute | Hours |
| **Consistency** | Perfect balance | Varies |
| **Customization** | Parameter-based | Complete control |
| **Iteration Speed** | Instant | Slow |
| **Best For** | Testing, procedural | Crafted experiences |

## Debugging

### Enable Debug Logs
Generated waves will log:
```
Generating 20 waves at difficulty level 2...
  Wave 1: 5 enemies in 1 groups
  Wave 2: 6 enemies in 1 groups
  Wave 3: 7 enemies in 2 groups
  ...
✓ Generated 20 balanced waves!
```

### Common Issues

**No enemies spawning?**
- Check that enemy assets are assigned to **Available Enemies**
- Verify **Use Auto Generation** is checked

**Waves too easy/hard?**
- Adjust **Difficulty Level**
- Modify **Enemy Count Growth** parameter
- Change **Base Enemy Count**

**Too many/few enemies?**
- Adjust **Max Enemies Per Wave**
- Modify **Enemy Count Growth**

## Advanced Usage

### Custom Difficulty Curves

You can modify `WaveGenerator.cs` to create custom curves:

```csharp
// Example: Boss waves every 5 waves
if (waveNumber % 5 == 0)
{
    // Add boss enemy group
}
```

### Dynamic Difficulty Adjustment

```csharp
// Increase difficulty based on player performance
if (playerIsWinning)
{
    waveGenerator.difficultyLevel++;
    waveGenerator.ClearGeneratedWaves();
}
```

## Integration with Existing Maps

You can use auto-generation on existing maps:

1. Keep your map's path and tower positions
2. Enable WaveGenerator on MapManager
3. Auto-generated waves replace manual wave list
4. Path and towers remain unchanged

## Performance

- **Generation Time**: < 100ms for 50 waves
- **Memory**: Minimal (waves cached after generation)
- **Runtime Impact**: Zero (pre-generated at Start)

## Future Enhancements

Potential additions:
- [ ] Per-enemy-type spawn weights
- [ ] Custom wave templates
- [ ] Boss wave insertion
- [ ] Adaptive difficulty based on player performance
- [ ] Wave preview in editor
- [ ] Save/export generated waves as assets

## Summary

The Auto Wave Generation system provides:
- ✅ Zero manual wave configuration
- ✅ Balanced, progressive difficulty
- ✅ 5 difficulty presets
- ✅ Configurable parameters
- ✅ Instant regeneration
- ✅ Perfect for testing and iteration

Just assign your enemies, set difficulty, and play!

## System Architecture

```
┌─────────────────────────────────────────────────────┐
│                   MapManager                         │
│  ┌───────────────────────────────────────────┐      │
│  │   On Start: Check Auto-Generation          │      │
│  │   ↓                                        │      │
│  │   If enabled → WaveGenerator.Generate()    │      │
│  └───────────────────────────────────────────┘      │
│                       │                              │
│                       ↓                              │
│  ┌───────────────────────────────────────────┐      │
│  │   GetWaveData(waveNumber)                  │      │
│  │   ↓                                        │      │
│  │   Auto? → WaveGenerator.GetGeneratedWave() │      │
│  │   Manual? → MapData.waves[index]           │      │
│  └───────────────────────────────────────────┘      │
└─────────────────────────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────┐
│                  WaveGenerator                       │
│  ┌───────────────────────────────────────────┐      │
│  │  GenerateWaves()                           │      │
│  │  ↓                                         │      │
│  │  For each wave 1 to N:                     │      │
│  │    1. Calculate enemy count                │      │
│  │    2. Select enemy types (progression)     │      │
│  │    3. Create enemy groups                  │      │
│  │    4. Set spawn timing                     │      │
│  │    5. Add random variation                 │      │
│  └───────────────────────────────────────────┘      │
│                                                      │
│  Configuration:                                      │
│  • Difficulty Level (1-5)                            │
│  • Number of Waves                                   │
│  • Enemy Pool (Available Enemies)                    │
│  • Growth Parameters                                 │
└─────────────────────────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────┐
│                  WaveManager                         │
│  ┌───────────────────────────────────────────┐      │
│  │  StartWave(waveNumber)                     │      │
│  │  ↓                                         │      │
│  │  Get wave data from MapManager             │      │
│  │  ↓                                         │      │
│  │  Spawn enemies according to wave data      │      │
│  └───────────────────────────────────────────┘      │
└─────────────────────────────────────────────────────┘
```

## Wave Progression Example

```
Difficulty Level: 2 (Medium)
Available Enemies: BasicEnemy (100 HP), TankEnemy (300 HP)

Wave 1:  [ 5x BasicEnemy ]
         Simple, single group

Wave 5:  [ 4x BasicEnemy ] → delay → [ 4x BasicEnemy ]
         Two groups, same type

Wave 10: [ 6x BasicEnemy ] → delay → [ 8x TankEnemy ]
         Two types introduced

Wave 15: [ 5x BasicEnemy ] → [ 10x BasicEnemy ] → [ 8x TankEnemy ]
         Three groups, mixed

Wave 20: [ 8x BasicEnemy ] → [ 6x TankEnemy ] → [ 10x BasicEnemy ] → [ 14x TankEnemy ]
         Complex, alternating groups
```

## Balancing Formula

### Enemy Count Per Wave
```
count = baseCount × (1 + growth/100)^(wave-1) × difficulty × random(0.9-1.1)
```

**Example (Medium, 10% growth):**
- Wave 1:  5 × 1.00^0 × 1.0 = **5 enemies**
- Wave 5:  5 × 1.10^4 × 1.0 = **7 enemies**
- Wave 10: 5 × 1.10^9 × 1.0 = **12 enemies**
- Wave 15: 5 × 1.10^14 × 1.0 = **19 enemies**
- Wave 20: 5 × 1.10^19 × 1.0 = **31 enemies**

### Enemy Type Selection
```
maxDifficulty = progression × (totalEnemyTypes - 1)

Wave 1-6:   maxDifficulty = 0.0-0.3 → Only weakest enemies
Wave 7-14:  maxDifficulty = 0.3-0.7 → Weak + medium enemies
Wave 15-20: maxDifficulty = 0.7-1.0 → All enemy types
```
