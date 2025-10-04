# Lightning Trait Testing Guide

## How to Test Lightning Trait Effects

### Method 1: Using the Test Component
1. **Add the test component**: Add `LightningTraitTest` component to any GameObject in your scene
2. **Assign references**: 
   - Drag a Tower to the "Test Tower" field
   - The component will auto-find enemies, or drag them to "Test Enemies" array
3. **Use the context menu options** (right-click the component in Inspector):
   - **"Apply Lightning Trait"** - Adds lightning trait to the tower
   - **"Setup Test Enemies"** - Positions enemies in a line for easy testing
   - **"Test Lightning Chain"** - Manually triggers chain lightning effect
   - **"Debug Chain Range"** - Shows debug info about what enemies are in range

### Method 2: Using the Editor Window
1. **Open the window**: Go to `TowerFusion > Debug > Lightning Trait Tester` in the menu
2. **Select a tower**: Drag a tower from the scene into the "Tower" field
3. **Click buttons**:
   - **"Apply Lightning Trait"** - Adds the trait
   - **"Spawn Test Enemies"** - Creates enemies around the tower
   - **"Test Chain Lightning"** - Triggers the effect

### What to Look For

#### Console Messages
When lightning trait is working, you should see these debug messages:
```
Applied Lightning Trait to [TowerName]
Applying trait effects on attack to [EnemyName] with [X] traits
Applying trait: Lightning (hasChainEffect: True)
Trait 'Lightning' has chain effect - applying to [EnemyName]
Applying chain effect from [EnemyName] with range [X]
Found [X] colliders in range
Found chain target: [EnemyName] at distance [X]
Chain targets found: [X], will deal [X] damage each
Chain lightning: [X] damage to [EnemyName] from [EnemyName]
Creating lightning effect from [pos] to [pos]
```

#### Visual Effects
1. **Tower Visual**: Tower should show yellow overlay when lightning trait is applied
2. **Lightning Lines**: Yellow lightning lines should appear between enemies (temporary)
3. **Flash Effects**: Yellow sphere flashes should appear at chained enemy positions
4. **Scene View**: Debug.DrawLine shows yellow lines in Scene view (not Game view)

### Troubleshooting

#### No Chain Lightning Happening
- **Check Console**: Look for the debug messages above
- **Enemy Distance**: Enemies must be within 3 units of each other (default chain range)
- **Multiple Enemies**: Need at least 2 enemies for chaining to occur
- **Layer Issues**: The script now finds enemies regardless of layer

#### No Visual Effects
- **Yellow Lines**: Should be visible for 0.3 seconds
- **Flash Effects**: Yellow spheres should appear at enemy positions
- **Scene View**: Debug lines are always visible in Scene view

#### Tower Not Attacking
- **Enemy in Range**: Make sure enemies are within tower attack range
- **Tower Active**: Ensure tower is enabled and has target acquisition working
- **Projectile Integration**: Lightning effects trigger when projectiles hit enemies

### Quick Test Setup
1. Place a tower in the scene
2. Add 3-4 enemies near the tower (within 2-3 units of each other)
3. Add `LightningTraitTest` component to any GameObject
4. Assign the tower to the component
5. Right-click component → "Apply Lightning Trait"
6. Right-click component → "Test Lightning Chain"
7. Watch console for debug messages and scene for visual effects

### Expected Behavior
- Primary enemy takes normal damage
- Up to 2 nearby enemies take chain damage (same amount as primary)
- Visual lightning effect connects chained enemies
- Yellow flash effects appear at each chained enemy
- Console shows detailed debug information