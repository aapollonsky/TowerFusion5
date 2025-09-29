# Unity Editor Setup Guide for TowerFusion4

## Reliable Project Opening Instructions

### Prerequisites
1. **Unity Version**: Ensure you have Unity 2022.3.12f1 or later installed
2. **iOS Build Support**: Install iOS Build Support module (if building for iOS)

### Step-by-Step Opening Process

1. **Open Unity Hub**
   - Launch Unity Hub application

2. **Add Project**
   - Click "Add" button in Unity Hub
   - Navigate to the TowerFusion4 project folder
   - Select the folder and click "Add"

3. **Open Project**
   - Click on the TowerFusion4 project in Unity Hub
   - Wait for Unity Editor to load (this may take a few minutes on first open)

4. **Verify Project Loading**
   - Open `Assets/Scenes/MainScene.unity`
   - Check the Hierarchy for "GameManager" object
   - GameManager should have 5 components:
     - Transform
     - GameManager (Script)
     - TowerManager (Script)
     - WaveManager (Script)
     - EnemyManager (Script)
     - MapManager (Script)

5. **Check for Errors**
   - Open Console window (Window > General > Console)
   - Should see "Game initialized - Tower Fusion 4" message
   - Should see "Map 'Basic Map' initialized successfully." message
   - No red error messages should appear

### Common Issues and Solutions

#### Script Compilation Errors
- **Issue**: Missing script references or compilation errors
- **Solution**: All required prefabs and script references have been created. If you still see errors, try:
  - Reimport all assets: Assets > Reimport All
  - Clear Library folder and reopen project

#### Missing Prefabs
- **Issue**: Inspector shows "Missing (Mono Script)" 
- **Solution**: The project now includes all required prefabs in `Assets/Prefabs/`:
  - Enemy.prefab
  - Tower.prefab
  - Projectile.prefab

#### Scene Setup Issues
- **Issue**: GameManager missing components
- **Solution**: The MainScene has been properly configured with all manager components and their required references

### Project Structure Verification
After opening successfully, you should see:
```
Assets/
├── Data/
│   ├── Enemies/     (BasicEnemy.asset, ArmoredEnemy.asset)
│   ├── Maps/        (BasicMap.asset)
│   ├── Towers/      (ArcherTower.asset)
│   └── Waves/       (Wave1.asset)
├── Prefabs/         (Enemy.prefab, Tower.prefab, Projectile.prefab)
├── Scenes/          (MainScene.unity)
└── Scripts/         (All game scripts)
```

### Testing the Setup
1. **Play Mode Test**:
   - Press Play button in Unity Editor
   - No errors should appear in Console
   - GameManager should initialize properly

2. **Prefab Validation**:
   - Navigate to Assets/Prefabs folder
   - Double-click each prefab to verify they open without errors
   - Check that all components are properly assigned

### If Still Having Issues
1. Check Unity Console for specific error messages
2. Verify Unity version matches requirements (2022.3.12f1+)
3. Try opening project on a clean Unity installation
4. Check that all files were properly downloaded/cloned from repository

## Next Steps After Successful Opening
- Follow QUICKSTART.md for development setup
- See DEVELOPMENT.md for creating custom content
- Check README.md for full project documentation