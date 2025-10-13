# Impact Effects - Troubleshooting Guide

## ✅ Issue: "Prefab has missing scripts"

**Fixed!** The `AutoDestroyParticleSystem` component is now properly located at:
```
Assets/Scripts/Effects/AutoDestroyParticleSystem.cs
```

### If you still see the error:

1. **Wait for Unity to recompile** (check bottom-right corner of Unity)
2. **If error persists:**
   - Delete the old prefab: `Assets/Prefab/Effects/BasicImpactEffect.prefab`
   - Run `Tower Fusion > Create Impact Effect` again
   - The new prefab will have the correct script reference

## 🔍 Understanding the Fix

### What Happened:
The component was initially defined inside the Editor script, which Unity can't use at runtime.

### The Solution:
Created a proper runtime script in the correct location:
- **Location**: `Assets/Scripts/Effects/AutoDestroyParticleSystem.cs`
- **Namespace**: `TowerFusion`
- **Purpose**: Automatically destroys particle effects after they finish playing

### How It Works:
```csharp
// When the particle effect spawns
void Start()
{
    // Calculate total lifetime
    destroyTime = duration + particle_lifetime;
    
    // Schedule automatic cleanup
    Destroy(gameObject, destroyTime);
}
```

## 📁 File Structure

Your project should now have:
```
Assets/
├── Editor/
│   ├── CreateImpactEffect.cs         ← Creates effects
│   └── ImpactEffectInfo.cs           ← Status checker
├── Scripts/
│   ├── Effects/
│   │   └── AutoDestroyParticleSystem.cs  ← Runtime cleanup
│   └── Tower/
│       └── Projectile.cs              ← Uses effects
└── Prefab/
    ├── Effects/
    │   └── BasicImpactEffect.prefab   ← The effect (created by tool)
    ├── Projectile.prefab              ← Uses the effect
    └── AdvancedProjectile.prefab      ← Uses the effect
```

## ✨ What Changed

### Before (Broken):
- `AutoDestroyParticleSystem` was inside Editor folder
- Unity couldn't find it at runtime
- Prefab showed "Missing Script" error

### After (Fixed):
- `AutoDestroyParticleSystem` is in proper runtime location
- Properly namespaced under `TowerFusion`
- Prefab references work correctly

## 🚀 Next Steps

1. **Wait for Unity to finish compiling**
2. **Run the tool**: `Tower Fusion > Create Impact Effect`
3. **Test the game** - effects should work now!

## 🐛 If Problems Persist

### Check Console:
Look for these success messages:
```
[AutoDestroy] BasicImpactEffect will auto-destroy in 0.5s
[Impact] Created effect at (x, y, z) - frameNumber
```

### Verify Script Location:
Make sure the file exists at:
```
Assets/Scripts/Effects/AutoDestroyParticleSystem.cs
```

### Verify Script Content:
The script should start with:
```csharp
using UnityEngine;

namespace TowerFusion
{
    public class AutoDestroyParticleSystem : MonoBehaviour
    {
        // ... component code ...
    }
}
```

### Still Having Issues?

1. **Close Unity**
2. **Delete these folders** (Unity will regenerate them):
   - `Library/ScriptAssemblies/`
3. **Reopen Unity**
4. **Let it recompile everything**
5. **Run the tool again**

## ✅ Success Checklist

- [ ] `AutoDestroyParticleSystem.cs` exists in `Assets/Scripts/Effects/`
- [ ] Unity has finished compiling (no spinning icon)
- [ ] No errors in Console
- [ ] Running `Tower Fusion > Create Impact Effect` completes successfully
- [ ] Console shows: "Created Basic Impact Effect at: Assets/Prefab/Effects/BasicImpactEffect.prefab"
- [ ] Console shows: "Assigned impact effect to 2 projectile prefabs"

When all checks pass, test your game!
