# GitHub Copilot Instructions for TowerFusion5

## Project Overview

TowerFusion5 is a Unity 2D tower defense game where:
- Players place towers to defend against enemy waves
- Enemies attack and destroy towers before reaching the end point
- Each enemy attacks ONE tower at a time, with intelligent distribution across multiple towers
- Maximum 3 towers can be under attack simultaneously per wave

## Architecture Patterns

### ScriptableObject Pattern
- `TowerData.cs` - Tower configurations (damage, range, cost, health)
- `EnemyData.cs` - Enemy configurations (speed, health, attack stats)
- All game data is asset-based, not hardcoded

### Singleton Managers
- `TowerManager` - Handles tower placement and lifecycle
- `EnemyManager` - Handles enemy spawning and wave management
- `EnemyTargetDistributor` - Coordinates target selection across enemies

### State Machine
- Enemy behavior uses states: `SeekingTower`, `AttackingTower`, `MovingToEnd`
- Enemies prioritize tower destruction over reaching end point

## Coding Standards

### Naming Conventions
- Classes: `PascalCase` (e.g., `EnemyTargetDistributor`)
- Private fields: `camelCase` (e.g., `towerAssignments`)
- Public properties: `PascalCase` (e.g., `MaxHealth`)
- Methods: `PascalCase` (e.g., `SelectTargetForEnemy`)

### Namespace
All game scripts use `namespace TowerFusion`

### Comments
Use XML documentation comments for public methods:
```csharp
/// <summary>
/// Brief description of what the method does
/// </summary>
```

### Unity Patterns
- Use `[SerializeField]` for inspector-exposed private fields
- Prefer events over direct coupling (`OnTowerDestroyed`, `OnEnemyKilled`)
- Always null-check before accessing manager instances
- Clean up event subscriptions in `OnDestroy()`

## Key Systems

### Tower Attack System
- Towers have health (`maxHealth`, `isDestructible`)
- Enemies can attack towers if `canAttackTowers = true`
- Tower destruction triggers cleanup via events

### Target Distribution
- `EnemyTargetDistributor.SelectTargetForEnemy()` selects targets
- Algorithm: Choose tower with fewest enemies (closest if tied)
- Constraint: Max 3 towers under attack per wave (`maxSimultaneousTowers`)
- Automatic cleanup when enemies die or towers destroyed

### Movement System
- Enemies spawn → seek towers → attack until destroyed → move to end
- Uses Unity's built-in navigation or custom waypoint system
- Detection range determines which towers are visible to enemy

## Important Files

- `Enemy.cs` - Enemy AI and attack behavior
- `Tower.cs` - Tower health and destruction
- `EnemyTargetDistributor.cs` - Target coordination
- `TowerData.cs` / `EnemyData.cs` - Configuration assets

## When Creating New Features

1. **Check for existing managers** - Don't create duplicate singletons
2. **Use ScriptableObjects** for data - Keep code separate from configuration
3. **Add events** for loose coupling - Let systems communicate via events
4. **Follow the namespace** - Always use `namespace TowerFusion`
5. **Think about distribution** - Consider how multiple enemies/towers interact

## Common Pitfalls to Avoid

- ❌ Don't make all enemies attack the nearest tower (use distributor)
- ❌ Don't create tight coupling between Enemy and Tower classes
- ❌ Don't forget to unsubscribe from events in `OnDestroy()`
- ❌ Don't bypass the EnemyTargetDistributor for target selection
- ❌ Don't exceed `maxSimultaneousTowers` constraint

## Testing Guidelines

- Test with large waves (20+ enemies) to verify distribution
- Test tower destruction mid-wave to ensure cleanup works
- Test with 1 tower, 3 towers, and 10+ towers scenarios
- Verify console logs show balanced distribution
- Check for null reference errors when towers/enemies die

## Performance Considerations

- Keep target selection O(n) where n is number of towers in range
- Use cleanup methods to prevent memory leaks from dead enemies
- Dictionary lookups for assignments are O(1)
- Avoid expensive operations in `Update()` - use events instead

Store all documentation in the docs folder

