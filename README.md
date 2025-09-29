# TowerFusion4

A Unity-based 2D Tower Defence game for iOS with support for multiple maps, enemy types, and tower types.

## Features

- **Multiple Maps**: Scriptable Object-based map system with configurable paths and tower positions
- **Multiple Enemy Types**: Flexible enemy system with different stats, resistances, and special abilities
- **Multiple Tower Types**: Modular tower system with upgrades and special effects
- **iOS Ready**: Configured for iOS deployment with proper build settings
- **Modular Architecture**: Clean, maintainable code structure with singleton managers

## Project Structure

```
Assets/
├── Scenes/
│   └── MainScene.unity          # Main game scene
├── Scripts/
│   ├── GameManager.cs           # Core game state management
│   ├── MapManager.cs            # Map loading and management
│   ├── MapData.cs               # ScriptableObject for map configuration
│   ├── EnemyManager.cs          # Enemy spawning and tracking
│   ├── Enemy.cs                 # Individual enemy behavior
│   ├── EnemyData.cs             # ScriptableObject for enemy configuration
│   ├── WaveManager.cs           # Wave progression and enemy spawning
│   ├── WaveData.cs              # ScriptableObject for wave configuration
│   ├── TowerManager.cs          # Tower placement and management
│   ├── Tower.cs                 # Individual tower behavior
│   ├── TowerData.cs             # ScriptableObject for tower configuration
│   ├── Projectile.cs            # Projectile behavior for tower attacks
│   ├── GameUI.cs                # Main game UI controller
│   └── TowerButton.cs           # UI button for tower building
├── Data/
│   ├── Maps/                    # Map configuration files
│   ├── Enemies/                 # Enemy configuration files
│   ├── Towers/                  # Tower configuration files
│   └── Waves/                   # Wave configuration files
├── Prefabs/                     # Game object prefabs
├── Sprites/                     # 2D sprites and images
├── Materials/                   # Materials for rendering
└── Audio/                       # Sound effects and music
```

## Core Systems

### Game Manager
- Manages game state (preparing, wave active, game over)
- Handles player health and gold
- Coordinates between all other systems

### Map System
- **MapData ScriptableObject**: Defines path points, tower positions, and waves
- **MapManager**: Loads and manages the current map
- Supports multiple maps with different layouts and difficulty

### Enemy System
- **EnemyData ScriptableObject**: Defines enemy stats, resistances, and abilities
- **Enemy Component**: Individual enemy behavior and movement
- **EnemyManager**: Tracks all active enemies
- Support for flying enemies, armored enemies, regenerating enemies, etc.

### Tower System
- **TowerData ScriptableObject**: Defines tower stats and upgrade paths
- **Tower Component**: Individual tower targeting and attacking
- **TowerManager**: Handles tower placement, selection, and upgrades
- Support for different damage types, targeting modes, and special effects

### Wave System
- **WaveData ScriptableObject**: Defines enemy groups and spawn timing
- **WaveManager**: Controls wave progression and enemy spawning
- Flexible wave configuration with multiple enemy types per wave

## Getting Started

### Prerequisites
- Unity 2022.3.12f1 or later
- iOS development environment for iOS builds

### Setup Instructions

1. **Open in Unity**: Open the project in Unity Editor
2. **Configure Build Settings**: 
   - Go to File > Build Settings
   - Select iOS platform
   - Configure Player Settings for iOS deployment
3. **Create Content**:
   - Create MapData assets in Assets/Data/Maps/
   - Create EnemyData assets in Assets/Data/Enemies/  
   - Create TowerData assets in Assets/Data/Towers/
   - Create WaveData assets in Assets/Data/Waves/
4. **Build**: Build the project for iOS

### Creating Custom Content

#### Maps
Right-click in Project window > Create > Tower Fusion > Map Data
- Set path points for enemy movement
- Define tower placement positions
- Assign wave data for the map

#### Enemies  
Right-click in Project window > Create > Tower Fusion > Enemy Data
- Configure health, speed, and resistances
- Set special abilities (flying, armored, etc.)
- Define gold reward and damage values

#### Towers
Right-click in Project window > Create > Tower Fusion > Tower Data  
- Set damage, range, and attack speed
- Configure targeting mode and damage type
- Define upgrade path and special effects

#### Waves
Right-click in Project window > Create > Tower Fusion > Wave Data
- Define enemy groups and spawn counts
- Set timing between spawns
- Configure wave progression

## iOS Deployment

The project is pre-configured for iOS deployment with:
- Bundle identifier: `com.towerfusion.towerfusion4`
- Target iOS version: 11.0+
- Optimized for mobile performance
- 2D rendering pipeline suitable for mobile devices

### Build Instructions
1. Set up iOS development certificates in Unity
2. Configure signing in Player Settings
3. Build and run on device or simulator

## Architecture Notes

### Design Patterns Used
- **Singleton Pattern**: For manager classes (GameManager, TowerManager, etc.)
- **Observer Pattern**: Event-driven communication between systems
- **ScriptableObject Pattern**: Data-driven design for game content
- **Component Pattern**: Modular behavior components

### Performance Considerations
- Object pooling ready for projectiles and enemies
- Efficient enemy pathfinding using waypoints
- Optimized for mobile rendering (2D sprites, minimal effects)
- Modular loading system for different maps

## License

This project is created for educational and development purposes.