# Development Guide

## Creating Game Content

This guide explains how to create maps, enemies, towers, and waves for TowerFusion4.

### Maps

1. Right-click in `Assets/Data/Maps/` folder
2. Select `Create > Tower Fusion > Map Data`
3. Configure the following:
   - **Map Name**: Display name for the map
   - **Description**: Brief description of the map
   - **Path Points**: List of world coordinates defining the enemy path
   - **Tower Positions**: Valid positions where towers can be placed
   - **Enemy Spawn Point**: Where enemies begin their journey
   - **Enemy End Point**: Where enemies exit (damages player)
   - **Waves**: List of wave data assets for this map

### Enemies

1. Right-click in `Assets/Data/Enemies/` folder  
2. Select `Create > Tower Fusion > Enemy Data`
3. Configure the following:
   - **Basic Info**: Name, description, sprite
   - **Stats**: Health, move speed, gold reward, damage to player
   - **Resistances**: Physical, magic, fire, ice (0.0 = no resistance, 1.0 = immune)
   - **Special Abilities**: Flying, armored, fast, regenerating
   - **Visual**: Color tint and scale

### Towers

1. Right-click in `Assets/Data/Towers/` folder
2. Select `Create > Tower Fusion > Tower Data`  
3. Configure the following:
   - **Basic Info**: Name, description, sprite, build cost
   - **Combat Stats**: Damage, attack range, attack speed, damage type
   - **Targeting**: Targeting mode, can target flying/ground
   - **Projectile**: Projectile prefab, speed, hitscan option
   - **Special Effects**: Splash damage, slow effect, poison effect
   - **Upgrades**: Array of upgrade options

### Waves

1. Right-click in `Assets/Data/Waves/` folder
2. Select `Create > Tower Fusion > Wave Data`
3. Configure the following:
   - **Wave Info**: Name and wave number
   - **Enemy Groups**: List of enemy types and counts to spawn
   - **Timing**: Delays between groups and individual enemies

## Code Architecture

### Manager Classes (Singletons)
- `GameManager`: Core game state and progression
- `MapManager`: Current map loading and management
- `EnemyManager`: Active enemy tracking
- `TowerManager`: Tower placement and management
- `WaveManager`: Wave progression and enemy spawning

### Data Classes (ScriptableObjects)
- `MapData`: Map configuration and layout
- `EnemyData`: Enemy stats and abilities
- `TowerData`: Tower stats and upgrades
- `WaveData`: Wave composition and timing

### Behavior Classes (MonoBehaviours)
- `Enemy`: Individual enemy behavior
- `Tower`: Individual tower behavior
- `Projectile`: Projectile movement and damage
- `GameUI`: User interface controller

### Event System

The game uses Unity's Action system for loose coupling:

```csharp
// Subscribe to events
GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;

// Fire events
OnEnemyKilled?.Invoke(this);
```

## Performance Tips

### For Mobile (iOS) Performance:
1. Use sprite atlases to reduce draw calls
2. Limit the number of active enemies (recommend max 50-100)
3. Pool projectiles instead of creating/destroying
4. Use simple particle effects
5. Optimize UI updates to avoid every-frame operations

### Memory Management:
1. Use ScriptableObjects for data to avoid duplication
2. Clear references in OnDestroy methods
3. Use object pooling for frequently created objects
4. Compress textures appropriately for mobile

## Testing

### In Editor:
1. Create sample data assets
2. Assign data to managers in the scene
3. Test wave progression and tower placement
4. Verify UI updates correctly

### On Device:
1. Build for iOS and test performance
2. Verify touch input works correctly
3. Test memory usage with profiler
4. Validate game progression and balance

## Common Issues

### Build Errors:
- Ensure all ScriptableObject references are assigned
- Check that sprites are imported with correct settings
- Verify iOS platform settings in Player Settings

### Runtime Issues:
- Check console for null reference exceptions
- Verify singleton initialization order
- Ensure UI elements are properly connected

### Performance Issues:
- Profile on target device, not in editor
- Check for memory leaks with Unity Profiler
- Optimize texture sizes and compression