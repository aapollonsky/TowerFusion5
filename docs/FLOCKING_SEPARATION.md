# Flocking Separation System

## Overview

The flocking separation system prevents enemies from clumping together by applying a repulsion force when they get too close to each other. This creates more natural and spread-out movement patterns.

## How It Works

### Separation Force Calculation

When an enemy moves, it:
1. Checks all nearby enemies within `separationRadius`
2. Calculates a repulsion force away from each nearby enemy
3. The force is stronger when enemies are closer (inverse relationship)
4. All repulsion forces are averaged together
5. The separation force is blended with the primary movement direction

### Force Formula

```
separationForce = Σ(awayDirection * strength * (1 - distance/radius)) / nearbyCount
```

Where:
- `awayDirection` = normalized vector away from nearby enemy
- `strength` = `separationStrength` parameter
- `distance` = distance to nearby enemy
- `radius` = `separationRadius` parameter
- `nearbyCount` = number of nearby enemies

### Movement Blending

```
finalDirection = (targetDirection + separationForce).normalized
```

The separation force is added to the primary movement direction, then normalized to maintain consistent speed.

## Configuration Parameters

### EnemyData Settings

**`useSeparation`** (bool, default: `true`)
- Enable/disable separation behavior
- Set to `false` for enemies that should ignore others (e.g., flying enemies)

**`separationRadius`** (float, range: 0.1-5, default: `1.0`)
- Distance at which enemies start repelling each other
- Smaller values = enemies can get closer before separating
- Larger values = enemies maintain more distance

**`separationStrength`** (float, range: 0-10, default: `2.0`)
- How strongly enemies push away from each other
- Lower values = gentle separation, more natural clustering
- Higher values = strong repulsion, very spread out formations

## Recommended Settings by Enemy Type

### Standard Ground Enemies
```
useSeparation: true
separationRadius: 1.0
separationStrength: 2.0
```
Balanced separation for normal infantry-type enemies.

### Large/Heavy Enemies
```
useSeparation: true
separationRadius: 1.5
separationStrength: 3.0
```
Need more personal space due to larger size.

### Fast/Agile Enemies
```
useSeparation: true
separationRadius: 0.8
separationStrength: 1.5
```
Can tolerate closer proximity, more mobile.

### Flying Enemies
```
useSeparation: true
separationRadius: 1.2
separationStrength: 2.5
```
Can separate in 3D space (though current implementation is 2D).

### Swarm Enemies
```
useSeparation: true
separationRadius: 0.5
separationStrength: 1.0
```
Intentionally cluster together for swarm behavior.

### Boss/Special Enemies
```
useSeparation: false
```
Ignore other enemies completely.

## Performance Considerations

### Complexity
- **Per-enemy cost**: O(n) where n = total active enemies
- **Total system cost**: O(n²) for all enemies
- For 20 enemies: 400 distance checks per frame
- For 50 enemies: 2500 distance checks per frame

### Optimization Strategies

1. **Spatial Partitioning** (not yet implemented)
   - Only check enemies in nearby grid cells
   - Reduces checks from O(n²) to O(n*k) where k = avg enemies per cell

2. **Distance Culling**
   - Early exit when distance > separationRadius
   - Avoids unnecessary vector calculations

3. **Frame Skipping** (not yet implemented)
   - Only recalculate separation every N frames
   - Interpolate between calculations

4. **LOD System** (not yet implemented)
   - Disable separation for off-screen enemies
   - Reduce separation frequency for distant enemies

## Integration with Other Systems

### Grid-Aligned Movement
The separation force is applied **after** grid pathfinding but **before** final position update. This allows enemies to deviate from strict grid alignment while still following the general path.

### Corn Stealing
Separation works in all states:
- `MovingToCorn` - Prevents bottlenecks approaching corn storage
- `ReturningWithCorn` - Spreads out return journey
- `ReturningEmpty` - Natural spacing on way back
- `CounterattackingTower` - Multiple enemies attacking same tower stay separated

### Tower Counterattack
When 2 enemies counterattack the same tower, separation ensures they approach from slightly different angles rather than occupying the exact same position.

## Testing Scenarios

1. **Bottleneck Test**: Spawn 10+ enemies → narrow path → watch separation prevent overlap
2. **Corn Storage Rush**: 20 enemies moving to same point → should fan out as they approach
3. **Return Journey**: Enemies with corn returning to spawn → should maintain spacing
4. **Tower Attack**: 2 enemies counterattacking same tower → should approach from slightly different positions

## Future Enhancements

1. **Alignment** - Enemies match velocity of nearby enemies (flock together)
2. **Cohesion** - Enemies steer toward center of nearby group (stay together)
3. **Full Flocking** - Combine separation + alignment + cohesion for realistic group behavior
4. **Leader Following** - Designate leader enemy, others follow in formation
5. **Spatial Hashing** - Optimize performance with grid-based neighbor lookup
6. **3D Separation** - For flying enemies, use full 3D repulsion forces

## Debug Visualization

To visualize separation forces in the editor:
- Draw debug lines showing separation vectors
- Color-code enemies by nearby count (green=isolated, red=crowded)
- Show separation radius as debug circles

(Not yet implemented - could be added to Enemy.cs OnDrawGizmos)

---

**Last Updated**: October 23, 2025
**Status**: Implemented and ready for testing
