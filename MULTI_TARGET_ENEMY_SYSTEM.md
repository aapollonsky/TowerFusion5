# Enemy Target Distribution System 🎯⚔️

## Overview

This system intelligently distributes enemies across multiple towers to prevent all enemies from attacking the same tower. Each enemy attacks only ONE tower at a time, but different enemies in a wave will spread out and attack different towers, creating more strategic and challenging gameplay.

## Key Concept

**Single Target per Enemy, Distributed Targeting Across Wave:**
- Each enemy targets and attacks ONE tower at a time
- Enemies coordinate to spread across different towers
- Prevents "tower ganging" where all enemies attack the same tower
- Creates more strategic defense positioning requirements

## System Components

### 1. EnemyTargetDistributor (New Component)

A singleton manager that tracks which enemies are targeting which towers and intelligently assigns targets to balance the load.

**Key Features:**
- Tracks assignments: `Dictionary<Tower, List<Enemy>>`
- Selects least-targeted towers for new enemies
- Automatically cleans up destroyed towers and dead enemies
- Provides debugging information about current assignments

### 2. Enhanced Enemy Targeting

**EnemyData Configuration:**
- `towerDetectionRange` - How far enemy can detect towers to select from

**Enemy Behavior:**
- Uses distributor to select best tower (least enemies assigned)
- Registers with distributor when acquiring target
- Unregisters when tower destroyed or enemy dies
- Falls back to nearest tower if distributor unavailable

## How It Works

### Target Selection Algorithm

```
1. Enemy needs tower target
2. Find all towers within detectionRange
3. For each tower:
   - Count how many enemies are already targeting it
   - Track distance to enemy
4. Select tower with:
   - FEWEST enemies assigned (priority)
   - CLOSEST distance (tiebreaker)
5. Register enemy-tower assignment in distributor
```

### Example Scenario

**Wave with 5 Enemies, 3 Towers:**

Without Distribution (old behavior):
```
Tower A (closest): 5 enemies
Tower B (middle):  0 enemies  
Tower C (far):     0 enemies
Result: Tower A quickly destroyed, others untouched
```

With Distribution (new behavior):
```
Tower A: 2 enemies
Tower B: 2 enemies
Tower C: 1 enemy
Result: Pressure spread evenly, strategic defense needed
```

## Configuration

### EnemyData Settings

Only one setting needs adjustment:

**Tower Detection Range:**
```
towerDetectionRange: 5.0 (default)
- Small (3.0): Enemy only sees nearby towers
- Medium (5.0): Balanced detection  
- Large (10.0): Enemy can target distant towers
```

**Impact:**
- **Small Range**: Enemies more likely to cluster on closest towers
- **Large Range**: Better distribution across all towers
