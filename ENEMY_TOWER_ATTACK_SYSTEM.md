# Enemy Tower Attack System 🎯

## Overview

This system adds the ability for enemies to attack and destroy towers, and modifies enemy movement behavior to prioritize tower destruction before reaching the end point.

## Key Features

### 1. Tower Health System
Towers now have health and can be destroyed by enemies.

**TowerData.cs Changes:**
- `maxHealth` (default: 100f) - Maximum health for the tower
- `isDestructible` (default: true) - Whether enemies can destroy this tower

**Tower.cs Changes:**
- Health tracking with `currentHealth` and `isAlive` state
- `TakeDamage(float damage)` - Apply damage to the tower
- Health bar visualization (hidden when at full health)
- `OnTowerDestroyed` event fired when tower is destroyed
- Automatic cleanup when destroyed

### 2. Enemy Attack Abilities
Enemies can now be configured to attack towers.

**EnemyData.cs Changes:**
- `canAttackTowers` (default: false) - Enable tower attacking for this enemy type
- `towerAttackDamage` (default: 10f) - Damage dealt per attack
- `towerAttackRange` (default: 0.5f) - Range at which enemy can attack towers
- `towerAttackCooldown` (default: 1f) - Seconds between attacks

### 3. Enemy Behavior System
Enemies now have three behavioral states:

**SeekingTower:**
- Scans for nearest tower
- Moves directly toward targeted tower
- Transitions to attacking when in range

**AttackingTower:**
- Stays in position and attacks tower
- Continues until tower is destroyed
- Returns to seeking if tower moves out of range

**MovingToEnd:**
- Fallback behavior when no towers are available
- Follows original path points to end point
- Same as non-attacking enemies

### 4. Intelligent Movement
**Enemy.cs Changes:**
- State machine for behavior management
- Tower detection and targeting system
- Dynamic pathfinding to nearest tower
- Automatic switching between behaviors

**Movement Priority:**
1. Spawn at enemy spawn point
2. Find and target nearest tower
3. Move to and attack tower until destroyed
4. Repeat steps 2-3 for all towers
5. Move to end point when no towers remain

### 5. Tower Manager Integration
**TowerManager.cs Changes:**
- Subscribes to `OnTowerDestroyed` event
- Automatically removes destroyed towers from active list
- Updates UI when towers are destroyed
- Handles tower destruction cleanup

## Configuration Guide

### Making an Enemy Attack Towers

1. Open the enemy's `EnemyData` ScriptableObject in the Inspector
2. Expand the "Tower Attacking" section
3. Enable `Can Attack Towers` checkbox
4. Set `Tower Attack Damage` (e.g., 10-50)
5. Set `Tower Attack Range` (typically 0.5-1.0)
6. Set `Tower Attack Cooldown` (seconds between attacks, e.g., 1-2)

### Making Indestructible Towers

1. Open the tower's `TowerData` ScriptableObject
2. Expand the "Defense" section
3. Uncheck `Is Destructible`
4. Tower will ignore all enemy attacks

### Adjusting Tower Health

1. Open `TowerData` ScriptableObject
2. Adjust `Max Health` value
3. Higher values = more durable towers
4. Recommended range: 50-500

## Example Configurations

### Tank Enemy (Heavy Tower Attacker)
```
Can Attack Towers: ✓
Tower Attack Damage: 25
Tower Attack Range: 0.5
Tower Attack Cooldown: 1.5
```

### Swarm Enemy (Fast Light Attacker)
```
Can Attack Towers: ✓
Tower Attack Damage: 5
Tower Attack Range: 0.5
Tower Attack Cooldown: 0.5
```

### Standard Enemy (Bypasses Towers)
```
Can Attack Towers: ✗
(All other fields inactive)
```

### Basic Tower
```
Max Health: 100
Is Destructible: ✓
```

### Fortified Tower
```
Max Health: 300
Is Destructible: ✓
```

### Invulnerable Tower
```
Max Health: 100
Is Destructible: ✗
```

## Gameplay Flow

### Enemy Spawn with Tower Attackers:
1. Enemy spawns at spawn point
2. Immediately scans for nearest tower
3. If tower found:
   - Moves directly to tower (ignoring path)
   - Attacks tower when in range
   - Destroys tower and seeks next one
4. If no towers found:
   - Follows path to end point
   - Damages player on arrival

### Mixed Waves:
- Some enemies attack towers (canAttackTowers = true)
- Others bypass and head to end (canAttackTowers = false)
- Creates strategic defense decisions

## Technical Details

### Performance Considerations
- Tower targeting updates only when needed
- Distance checks use magnitude squared where possible
- State machine reduces per-frame calculations
- Dead tower references are cleaned up automatically

### Event System
- `Tower.OnTowerDestroyed` - Fired when tower health reaches 0
- `TowerManager.OnTowerSold` - Reused for destroyed towers (UI compatibility)
- Automatic unregistration prevents memory leaks

### Edge Cases Handled
- Tower destroyed while enemy is attacking it
- All towers destroyed before wave ends
- Enemies spawn after all towers destroyed
- Tower invulnerability setting respected
- Health bar only shown when damaged

## Balancing Tips

### Tower Survivability
- Base health: 100 (1-2 enemies can destroy)
- Medium health: 200-300 (small group needed)
- High health: 500+ (requires focused attack)

### Enemy Attack Power
- Weak: 5-10 damage (10-20 attacks to destroy tower)
- Medium: 15-30 damage (3-7 attacks)
- Strong: 50+ damage (1-2 attacks)

### Attack Speed
- Slow: 2-3 second cooldown
- Medium: 1-1.5 second cooldown
- Fast: 0.5-0.8 second cooldown

### Recommended Ratios
- **Early Waves:** 0-25% tower attackers
- **Mid Waves:** 25-50% tower attackers  
- **Late Waves:** 50-75% tower attackers
- **Boss Waves:** Include strong tower attackers

## Future Enhancements

Potential additions:
- Tower armor/resistance system
- Tower self-repair mechanics
- Area-of-effect tower attacks against groups
- Tower placement to block enemy paths
- Tower destruction rewards/penalties
- Special enemy types (siege units, saboteurs)
- Tower reinforcement abilities

## Debugging

Enable debug logs to see:
- `Enemy.cs`: Tower targeting, attack attempts, state changes
- `Tower.cs`: Damage taken, health updates, destruction
- `TowerManager.cs`: Tower registration/unregistration

All debug messages include relevant context for troubleshooting.
