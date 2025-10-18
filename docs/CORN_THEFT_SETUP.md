# Corn Theft System - Quick Setup Guide

## Overview

The corn theft system has been fully implemented! This guide will help you set it up in your Unity scene.

## ✅ What's Been Implemented

### Core Systems
- ✅ `CornStorage.cs` - Stores corn and handles theft
- ✅ `CornManager.cs` - Singleton manager for corn mechanics
- ✅ `EnemyRole` enum - Attacker vs Stealer roles
- ✅ Enemy AI states - MovingToCorn, GrabbingCorn, ReturningWithCorn
- ✅ Corn carrying mechanics - Visual indicator, speed penalty, drop on death
- ✅ Target distribution update - Stealers bypass tower targeting

### Files Modified
1. **Assets/Scripts/Enemy/EnemyData.cs** - Added role configuration fields
2. **Assets/Scripts/Enemy/Enemy.cs** - Added corn stealing behavior and new states
3. **Assets/Scripts/Enemy/EnemyTargetDistributor.cs** - Skip stealers from tower distribution
4. **Assets/Scripts/Game/CornStorage.cs** - NEW FILE (Component)
5. **Assets/Scripts/Game/CornManager.cs** - NEW FILE (Singleton Manager)

## 🎮 Unity Scene Setup

### Step 1: Add Corn Storage to Scene

1. Create empty GameObject: `GameObject → Create Empty`
2. Name it: **"CornStorage"**
3. Add component: `CornStorage.cs`
4. Configure in Inspector:
   - **Initial Corn Count**: 20 (or desired amount)
   - **Grab Radius**: 1.0 (how close enemy must be)
   - **Corn Visual Parent**: (optional - for visual scaling)

5. Position it where you want the corn storage on your map

### Step 2: Add Corn Manager to Scene

1. Create empty GameObject: `GameObject → Create Empty`
2. Name it: **"CornManager"**
3. Add component: `CornManager.cs`
4. In Inspector, assign:
   - **Corn Storage**: Drag the CornStorage GameObject here

### Step 3: Configure Enemy Data Assets

Open your EnemyData ScriptableObjects and configure:

**For most enemies (Attackers):**
- **Default Role**: Attacker
- **Can Attack Towers**: true (keep existing behavior)

**For stealer type enemies (Optional):**
- **Default Role**: Stealer
- **Corn Grab Duration**: 1.0 seconds
- **Corn Carry Speed Multiplier**: 0.8 (80% speed when carrying)

> **Note:** You don't need to create special stealer enemies. The WaveManager will randomly assign 15% of enemies as stealers dynamically.

### Step 4: Update WaveManager (To Do Next)

The WaveManager needs to be updated to assign roles randomly:
- 85% Attacker role
- 15% Stealer role

## 🎯 How It Works

### Enemy Behavior Flow

**Attackers (85%):**
```
Spawn → Seek Towers → Attack Towers → Move to End
```

**Stealers (15%):**
```
Spawn → Move to Corn → Grab Corn → Return to Spawn
       ↓ (if killed with corn)
    Drop Corn → Corn Returns to Storage
```

### Win/Loss Conditions (To Implement)

**Win:** Defeat all waves while keeping at least 1 corn in storage

**Loss:** All corn successfully stolen and returned to spawn

## 🔧 Testing

### In Play Mode

1. **Check Console Logs:**
   ```
   "Enemy assigned as STEALER - heading to corn storage"
   "Enemy grabbed corn! Returning to spawn"
   "Corn returned to storage"
   ```

2. **Watch Enemy Behavior:**
   - 85% should attack towers
   - 15% should ignore towers and go to corn
   - Enemies with corn have yellow sphere above them
   - Killed corn carriers drop corn

3. **Monitor Corn Count:**
   - Check CornStorage component in Inspector during play
   - CornCount decreases when grabbed
   - CornCount increases when carrier dies

### Debug Commands

In any script, you can call:
```csharp
Debug.Log(CornManager.Instance.GetDebugInfo());

// Output:
// Corn Status:
//   In Storage: 15/20
//   Successfully Stolen: 3
//   In Transit: 2
```

## 🎨 Visual Improvements (Optional)

### Replace Corn Visual

Currently, enemies carrying corn show a yellow sphere. To improve:

1. Create/Import corn sprite
2. Open `Enemy.cs`
3. Find `CreateCornVisual()` method
4. Replace `GameObject.CreatePrimitive(PrimitiveType.Sphere)` with:
   ```csharp
   cornVisual = new GameObject("CornVisual");
   SpriteRenderer sr = cornVisual.AddComponent<SpriteRenderer>();
   sr.sprite = yourCornSprite; // Assign your sprite
   ```

### Corn Storage Visual

1. Create corn pile sprite/model
2. Assign to CornStorage's **Corn Visual Parent**
3. The system will automatically scale it based on remaining corn

## ⚠️ Known Limitations

### Still To Implement:

1. **WaveManager Integration:**
   - Needs to assign 85/15 role split when spawning
   - Currently uses `EnemyData.defaultRole`

2. **GameManager Win/Loss:**
   - Needs to check corn count for loss condition
   - Needs to prevent win if all corn stolen

3. **UI Display:**
   - Show remaining corn count
   - Show stolen corn count
   - Warning when corn gets low

4. **Corn Visual:**
   - Currently uses primitive sphere (yellow ball)
   - Should use actual corn sprite

## 📋 Next Steps

### Priority 1: WaveManager Update
```csharp
// In WaveManager.cs, when spawning enemy:
float roll = Random.value;
EnemyRole role = roll < 0.85f ? EnemyRole.Attacker : EnemyRole.Stealer;
spawnedEnemy.SetRole(role);
```

### Priority 2: GameManager Update
```csharp
// Check loss condition
if (CornManager.Instance.IsGameLost())
{
    GameOver("All corn stolen!");
}

// Check win condition
if (allWavesComplete && CornManager.Instance.RemainingCorn > 0)
{
    Victory();
}
```

### Priority 3: UI Update
```csharp
// Display corn count
cornCountText.text = $"Corn: {CornManager.Instance.RemainingCorn}";
```

## 🐛 Troubleshooting

### "CornManager.Instance is null"
- Make sure CornManager GameObject exists in scene
- Check that CornManager.cs is attached
- CornManager should be present before enemies spawn

### "Enemies not becoming stealers"
- WaveManager not yet updated with role assignment
- Currently uses `EnemyData.defaultRole`
- Manually set an EnemyData to `Stealer` role to test

### "Corn not returning when enemy dies"
- Check console for "Corn returned to storage" message
- Ensure CornManager is in scene
- Check that `hasCorn` is true before enemy dies

### "Enemies going wrong direction"
- Check that `spawnPoint` is set correctly
- Stored in `Enemy.Initialize()` from transform.position
- Should be where enemy first spawns

## 📚 Related Documentation

- [CORN_THEFT_SYSTEM.md](CORN_THEFT_SYSTEM.md) - Full design document
- [ENEMY_TARGET_DISTRIBUTION.md](ENEMY_TARGET_DISTRIBUTION.md) - Target distribution system
- [MAX_TOWER_CONSTRAINT.md](MAX_TOWER_CONSTRAINT.md) - 3-tower attack limit

## 🎉 Summary

The core corn theft mechanics are complete! You can now:
- Place corn storage on map
- Enemies will dynamically become stealers (once WaveManager updated)
- Corn theft creates parallel threat to tower attacks
- Corn returns if carrier dies
- Game loss when all corn stolen

Next steps are integrating with WaveManager and GameManager for the full gameplay loop.
