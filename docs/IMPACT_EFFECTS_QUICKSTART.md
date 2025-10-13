# Impact Effects - Quick Start Guide

## ✅ What I've Created For You

1. **CreateImpactEffect.cs** (Editor tool) - Creates impact effects
2. **ImpactEffectInfo.cs** (Editor tool) - Status checker and quick setup window
3. **AutoDestroyParticleSystem.cs** (Runtime script) - Component that auto-cleans up effects
4. **IMPACT_EFFECTS.md** - Full documentation

All scripts are in the correct locations and properly namespaced!

## 🚀 How to Add Impact Effects (2 Minutes)

### Option 1: Quick Setup (Recommended)
1. In Unity menu: `Tower Fusion > Create Impact Effect`
2. Done! Test your game.

### Option 2: Check Status First
1. In Unity menu: `Tower Fusion > Impact Effect Info`
2. Click "Create Impact Effect Now"
3. Done! Test your game.

## 🎯 What You'll See

When a projectile hits an enemy:
- **Burst of yellow particles** exploding outward
- **Quick flash effect** (0.5 seconds)
- **Particles fade and shrink** as they disappear

## ✨ The Effect Details

- **20 particles** per impact
- **Bright yellow/orange color** (very visible!)
- **Particles fly outward** at 3-5 units/second
- **Size: 0.15-0.3 units** (good for 2D games)
- **Auto-destroys** after finishing (no memory leaks)

## 🔧 Technical Details

### What the Tool Does:
1. Creates `Assets/Prefab/Effects/` folder
2. Creates `BasicImpactEffect.prefab` with particle system
3. Assigns it to both projectile prefabs automatically
4. Adds auto-destroy component for cleanup

### Projectile Integration:
Your `Projectile.cs` already has this code:
```csharp
private void CreateImpactEffect()
{
    if (impactEffectPrefab != null)
    {
        Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
    }
}
```

The tool just assigns the `impactEffectPrefab` field!

## 🎨 Customization

### Change Colors:
1. Select `BasicImpactEffect.prefab`
2. Particle System > Start Color
3. Try: Red for fire, Blue for ice, Green for poison

### Make It Bigger:
1. Select `BasicImpactEffect.prefab`  
2. Particle System > Start Size
3. Increase to 0.3-0.5 for larger effects

### More Particles:
1. Select `BasicImpactEffect.prefab`
2. Emission > Bursts > Count
3. Increase from 20 to 30-40

### Longer Duration:
1. Select `BasicImpactEffect.prefab`
2. Particle System > Start Lifetime
3. Increase from 0.5 to 1.0 seconds

## 🐛 Troubleshooting

### "I don't see any effects!"

**Check 1: Are effects assigned?**
- `Tower Fusion > Impact Effect Info` to check status
- Both projectile prefabs should show ✓

**Check 2: Are projectiles hitting?**
- Check console for "Projectile: Trigger hit on Enemy"
- If no hits, adjust enemy colliders

**Check 3: Camera seeing particles?**
- Effects use "Default" sorting layer
- Should be visible on any camera

### "Effects appear at wrong position"

The effect spawns at `transform.position` when projectile impacts.
This should be where the projectile hits the enemy.

### "Too many particles / lag"

Reduce particle count:
1. Select `BasicImpactEffect.prefab`
2. Emission > Bursts > Count
3. Reduce from 20 to 10-15

## 🎯 Next Steps

### Create Variations:
1. Duplicate `BasicImpactEffect.prefab`
2. Rename (e.g., `ExplosiveImpact`, `IceImpact`)
3. Customize colors and size
4. Assign to different tower types

### Add More Effects:
- **Trail effects** - particles following projectile
- **Charge effects** - particles around tower while charging
- **Kill effects** - special effects when enemy dies
- **Upgrade effects** - sparkles when tower upgrades

All of these use the same pattern:
1. Create particle system prefab
2. Add AutoDestroyParticleSystem component
3. Instantiate at the right position

## 📝 Summary

You now have:
- ✅ Automatic impact effect creation tool
- ✅ Status checker to verify setup
- ✅ Auto-cleanup to prevent memory leaks
- ✅ Customizable particle effects
- ✅ Full documentation

Just run `Tower Fusion > Create Impact Effect` and you're done!
