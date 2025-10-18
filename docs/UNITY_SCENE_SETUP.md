# Unity Scene Setup for Corn Theft System

## Quick Setup (5 Minutes)

### Step 1: Add CornManager to Scene

1. **Open your scene** (likely `MainScene.unity` or the one you're testing in)
2. **Hierarchy window** → Right-click → Create Empty
3. **Rename** to `CornManager`
4. **Inspector** → Add Component → Search "Corn Manager"
5. **Click** "Corn Manager" to add the component
6. ✅ **Done** - Leave all default settings

**What it does:** Manages all corn theft mechanics (singleton)

---

### Step 2: Add CornStorage to Scene

1. **Hierarchy window** → Right-click → Create Empty
2. **Rename** to `CornStorage`
3. **Inspector** → Add Component → Search "Corn Storage"
4. **Click** "Corn Storage" to add the component
5. **Position it on your map:**
   - Select CornStorage
   - Inspector → Transform
   - Set Position to where you want corn stored
   - Example: `(10, 0, 0)` or wherever makes sense on your map
6. **Configure settings:**
   - Initial Corn Count: `20` (or however many you want)
   - Grab Radius: `1.5` (how close enemies need to be to grab corn)
   - Corn Scale Factor: `0.5` (visual grows with corn amount)
7. ✅ **Done** - Visual yellow sphere will appear in Scene view

**What it does:** Physical storage location where corn is kept

**Visual:** Yellow wire sphere in Scene view shows grab radius

---

### Step 3: Configure WaveManager

1. **Hierarchy** → Find and select `WaveManager`
2. **Inspector** → Find "Enemy Role Distribution" section
3. **Check** ✓ "Enable Corn Theft"
4. **Set** "Stealer Percentage" to `0.15` (15% stealers)
5. ✅ **Done**

**What it does:** Controls the 85/15 split between attackers and stealers

---

### Step 4: Configure GameManager

1. **Hierarchy** → Find and select `GameManager`
2. **Inspector** → Find corn-related settings
3. **Check** ✓ "Enable Corn Theft Mode"
4. **Check** ✓ "Use Corn For Loss Condition" (optional - lose when corn gone vs health 0)
5. ✅ **Done**

**What it does:** Determines win/loss conditions based on corn

---

### Step 5: Save Scene

1. **File** → Save Scene (or Ctrl+S / Cmd+S)
2. ✅ **Done**

---

## Verification Checklist

After setup, verify everything is ready:

```
□ CornManager exists in Hierarchy
□ CornManager has "Corn Manager (Script)" component
□ CornStorage exists in Hierarchy
□ CornStorage has "Corn Storage (Script)" component
□ CornStorage Position is on your map (not at origin)
□ CornStorage Initial Corn Count > 0
□ WaveManager "Enable Corn Theft" is CHECKED ✓
□ GameManager "Enable Corn Theft Mode" is CHECKED ✓
□ Scene is saved
```

---

## Test It!

1. **Press Play** ▶️
2. **Start Wave 1**
3. **Watch Console:**
   - Should see: `Spawned EnemyBasic(Clone) as Stealer (roll: 0.12, threshold: 0.15)`
   - Should see: `EnemyBasic(Clone) assigned as STEALER - heading to corn storage`
4. **Watch Scene:**
   - ~15% of enemies should move toward CornStorage
   - ~85% of enemies should attack towers
   - Stealers that reach corn get yellow sphere visual

---

## Visual Reference

### CornManager Inspector
```
┌─────────────────────────────────────┐
│ Transform                           │
│ Position: (0, 0, 0)                 │
│                                     │
│ Corn Manager (Script)               │
│ ✓ Script: CornManager               │
│                                     │
│ (No settings to configure)          │
└─────────────────────────────────────┘
```

### CornStorage Inspector
```
┌─────────────────────────────────────┐
│ Transform                           │
│ Position: (10, 0, 0) ← YOUR MAP POS│
│                                     │
│ Corn Storage (Script)               │
│ ✓ Script: CornStorage               │
│                                     │
│ Initial Corn Count: 20              │
│ Grab Radius: 1.5                    │
│ Corn Scale Factor: 0.5              │
│ Visual Min Scale: 0.5               │
│ Visual Max Scale: 2.0               │
└─────────────────────────────────────┘
```

### WaveManager Inspector (relevant section)
```
┌─────────────────────────────────────┐
│ Wave Manager (Script)               │
│ ...                                 │
│                                     │
│ ▼ Enemy Role Distribution           │
│   ✓ Enable Corn Theft               │
│   Stealer Percentage: 0.15          │
│                                     │
│ ...                                 │
└─────────────────────────────────────┘
```

### GameManager Inspector (relevant section)
```
┌─────────────────────────────────────┐
│ Game Manager (Script)               │
│ ...                                 │
│                                     │
│ ▼ Corn Theft Settings               │
│   ✓ Enable Corn Theft Mode          │
│   ✓ Use Corn For Loss Condition     │
│                                     │
│ ...                                 │
└─────────────────────────────────────┘
```

---

## Common Setup Mistakes

### ❌ CornStorage at (0, 0, 0)
**Problem:** Enemies can't reach it or it's off the map  
**Fix:** Position it where you want corn stored on your map

### ❌ "Enable Corn Theft" unchecked
**Problem:** All enemies will be attackers (100%)  
**Fix:** Check the box in WaveManager Inspector

### ❌ Initial Corn Count = 0
**Problem:** No corn to steal!  
**Fix:** Set Initial Corn Count to 20 or higher

### ❌ Forgot to save scene
**Problem:** Changes lost when reopening Unity  
**Fix:** File → Save Scene (Ctrl+S / Cmd+S)

### ❌ Components not added
**Problem:** GameObjects exist but no scripts attached  
**Fix:** Add Component → Search for script name

---

## Advanced Configuration

### Adjust Stealer Percentage

Want more or fewer stealers?

**WaveManager → Stealer Percentage:**
- `0.05` = 5% stealers (very few)
- `0.15` = 15% stealers (default, balanced)
- `0.30` = 30% stealers (aggressive stealing)
- `0.50` = 50% stealers (half and half)

### Adjust Corn Amount

**CornStorage → Initial Corn Count:**
- `10` = Quick games (corn stolen fast)
- `20` = Balanced (default)
- `50` = Long games (lots of corn to defend)

### Adjust Grab Radius

**CornStorage → Grab Radius:**
- `1.0` = Small radius (enemies must be very close)
- `1.5` = Balanced (default)
- `2.5` = Large radius (easier for enemies to grab)

---

## Troubleshooting

### "Can't find CornManager script"

**Solution:**
1. Check Assets/Scripts/Game/CornManager.cs exists
2. If missing, the file wasn't created properly
3. Check Console for compilation errors

### "Component appears but has red error"

**Solution:**
1. Check Console for compilation errors
2. Double-click error to open script
3. Fix error and wait for recompilation

### "WaveManager doesn't have 'Enable Corn Theft' option"

**Solution:**
1. Script might not have recompiled
2. Try: Assets → Reimport All
3. Or: Close and reopen Unity

---

## What Each Component Does

### CornManager (Singleton)
- Tracks total corn remaining
- Handles corn grab events
- Handles corn steal events
- Determines game loss condition
- Communicates with CornStorage and GameManager

### CornStorage (Component)
- Physical location of corn
- Visual representation (sphere)
- Manages inventory (add/remove corn)
- Detection zone (grab radius)
- Events when corn taken/returned

### WaveManager (Modified)
- Spawns enemies
- Assigns roles (85% Attacker, 15% Stealer)
- Controlled by "Enable Corn Theft" toggle
- Uses random distribution based on percentage

### GameManager (Modified)
- Win/loss conditions
- Listens for corn theft events
- Alternative loss: All corn stolen (instead of health = 0)
- Victory: Wave complete AND corn remaining > 0

---

## Next Steps After Setup

Once you have everything set up:

1. **Test with 1 wave** - Verify behavior works
2. **Check Console logs** - Should see role assignments
3. **Watch enemy behavior** - Some go to corn, some attack towers
4. **Iterate on settings** - Adjust percentages, corn count, etc.

If you still have issues after setup, see `DEBUGGING_CORN_SYSTEM.md` for detailed troubleshooting.
