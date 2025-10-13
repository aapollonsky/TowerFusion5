# Impact Effect Setup Instructions

## Quick Setup

1. **In Unity Editor**, go to the menu: `Tower Fusion > Create Impact Effect`
   
2. This will automatically:
   - Create a new folder: `Assets/Prefab/Effects/`
   - Create the impact effect prefab: `BasicImpactEffect.prefab`
   - Assign it to both `Projectile.prefab` and `AdvancedProjectile.prefab`

3. **Test it!** Run the game and shoot some enemies. You should now see yellow burst effects when projectiles hit enemies.

## What the Impact Effect Looks Like

- **Burst of 20 particles** that explode outward
- **Bright yellow color** that fades to orange/gray
- **0.5 second lifetime** - quick and snappy
- **Particles shrink and fade** as they disappear
- **Auto-destroys** after finishing

## Customizing Impact Effects

If you want to customize the impact effect:

1. Find the prefab at: `Assets/Prefab/Effects/BasicImpactEffect.prefab`
2. Select it in the Project window
3. In the Inspector, expand the **Particle System** component
4. Adjust properties like:
   - **Start Size** - make particles bigger/smaller
   - **Start Color** - change the color
   - **Start Speed** - control how fast particles fly out
   - **Start Lifetime** - how long particles exist

## Creating Different Effects

You can create variations for different tower types:

1. Duplicate `BasicImpactEffect.prefab`
2. Rename it (e.g., `ExplosiveImpactEffect`)
3. Modify the particle settings
4. Assign it to specific projectile prefabs manually

## Troubleshooting

**"I don't see any effects!"**
- Check that the projectile prefabs have the impact effect assigned
- Select `Projectile.prefab` and look for "Impact Effect Prefab" field
- Make sure the game is running and projectiles are hitting enemies

**"Effects appear but look wrong"**
- Check your camera is set to see the "Effects" sorting layer
- Make sure particle rendering mode is set to Billboard
- Verify the effect prefab has the AutoDestroyParticleSystem component

**"I want different effects for different towers"**
- Create multiple impact effect prefabs
- Assign them manually to projectile prefabs
- Or create different projectile prefabs for different tower types
