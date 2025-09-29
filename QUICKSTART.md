# Quick Start Guide

## Setting Up TowerFusion4

### Prerequisites
1. Unity 2022.3.12f1 or later
2. iOS Build Support module installed

### Basic Setup Steps

1. **Open the Project**
   - Open Unity Hub
   - Click "Add" and select the TowerFusion4 folder
   - Open the project

2. **Check Scene Setup**
   - Open `Assets/Scenes/MainScene.unity`
   - Verify GameManager object exists in the hierarchy

3. **Create Basic Prefabs** (Required for testing)
   
   **Enemy Prefab:**
   - Create Empty GameObject named "Enemy"
   - Add SpriteRenderer component
   - Add Rigidbody2D component (set Body Type to Kinematic)
   - Add CircleCollider2D component
   - Add Enemy script component
   - Save as prefab in `Assets/Prefabs/`

   **Tower Prefab:**
   - Create Empty GameObject named "Tower" 
   - Add SpriteRenderer component
   - Add CircleCollider2D component (set as trigger)
   - Add Tower script component
   - Save as prefab in `Assets/Prefabs/`

   **Projectile Prefab:**
   - Create Empty GameObject named "Projectile"
   - Add SpriteRenderer component
   - Add Rigidbody2D component
   - Add CircleCollider2D component (set as trigger)
   - Add Projectile script component
   - Save as prefab in `Assets/Prefabs/`

4. **Configure Managers**
   - Select GameManager in hierarchy
   - Add other manager components:
     - MapManager
     - EnemyManager  
     - TowerManager
     - WaveManager
   - Assign the sample data assets and prefabs to respective managers

5. **Test in Editor**
   - Press Play to test basic functionality
   - Check Console for any errors

### iOS Build Setup

1. **Switch Platform**
   - File > Build Settings
   - Select iOS
   - Click "Switch Platform"

2. **Configure Player Settings**
   - Edit > Project Settings > Player
   - iOS tab settings are already configured:
     - Bundle Identifier: com.towerfusion.towerfusion4
     - Target minimum iOS Version: 11.0
     - Architecture: ARM64

3. **Build**
   - File > Build Settings
   - Click "Build" or "Build and Run"
   - Select output folder

### Troubleshooting

**Common Issues:**

1. **Script Compilation Errors**
   - Check that all manager GameObjects have the correct script components
   - Verify all asset references are assigned

2. **Missing References** 
   - Assign sample data assets to managers in inspector
   - Ensure prefab references are set

3. **iOS Build Errors**
   - Verify iOS Build Support is installed
   - Check Xcode is properly configured
   - Ensure iOS deployment target matches project settings

### Next Steps

1. **Create Sprites**: Add sprite assets for enemies, towers, and projectiles
2. **Design Levels**: Create more MapData assets with different layouts
3. **Balance Gameplay**: Adjust enemy and tower stats in data assets
4. **Add Audio**: Import sound effects and background music
5. **Polish UI**: Create proper UI sprites and animations

### File Structure for Content Creation

```
Assets/Data/
├── Enemies/          # EnemyData ScriptableObjects
├── Maps/             # MapData ScriptableObjects  
├── Towers/           # TowerData ScriptableObjects
└── Waves/            # WaveData ScriptableObjects

Assets/Prefabs/       # Game object prefabs
Assets/Sprites/       # 2D sprites and textures
Assets/Materials/     # Materials for rendering
Assets/Audio/         # Sound effects and music
```

For detailed content creation instructions, see DEVELOPMENT.md.