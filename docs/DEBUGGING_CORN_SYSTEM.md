# Debugging Corn Theft System

## Issue: All enemies attacking towers, no stealers visible

### Checklist to Debug

#### 1. Verify GameObjects Exist in Scene
```
□ CornManager GameObject exists in Hierarchy
□ CornStorage GameObject exists in Hierarchy
□ CornStorage is positioned on the map (not at origin)
```

**How to check:**
- Open Hierarchy window
- Search for "Corn"
- Should see both GameObjects

---

#### 2. Verify Components Are Attached
```
□ CornManager has CornManager.cs component
□ CornStorage has CornStorage.cs component
```

**How to check:**
- Select CornManager in Hierarchy
- Inspector should show "Corn Manager (Script)"
- Select CornStorage in Hierarchy
- Inspector should show "Corn Storage (Script)"

---

#### 3. Verify WaveManager Configuration
```
□ WaveManager exists in scene
□ Enable Corn Theft is CHECKED
□ Stealer Percentage is set (default: 0.15)
```

**How to check:**
- Select WaveManager in Hierarchy
- Look for "Enemy Role Distribution" section
- Enable Corn Theft: ✓
- Stealer Percentage: 0.15

**If missing:** WaveManager might not have refreshed. Try:
1. Select WaveManager
2. Right-click component → Remove Component
3. Add Component → WaveManager
4. Re-check settings

---

#### 4. Check Console for Role Assignment Logs

**Expected logs when wave starts:**
```
Spawned EnemyBasic(Clone) as Attacker (roll: 0.67, threshold: 0.15)
Spawned EnemyBasic(Clone) as Stealer (roll: 0.12, threshold: 0.15)
EnemyBasic(Clone) role set to STEALER
EnemyBasic(Clone) assigned as STEALER - heading to corn storage
```

**If you see:**
- ❌ No "Spawned X as..." logs → WaveManager code not executing
- ❌ All "Attacker" logs → CornManager.Instance is null
- ❌ "Stealer" but then "ATTACKER" behavior → Enemy.SetRole not working

---

#### 5. Verify CornManager.Instance Is Not Null

**Add temporary debug script:**

```csharp
using UnityEngine;
using TowerFusion;

public class DebugCornSystem : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"CornManager.Instance: {CornManager.Instance != null}");
        Debug.Log($"CornStorage exists: {FindObjectOfType<CornStorage>() != null}");
        Debug.Log($"WaveManager.Instance: {WaveManager.Instance != null}");
    }
}
```

**Expected output:**
```
CornManager.Instance: True
CornStorage exists: True
WaveManager.Instance: True
```

---

#### 6. Check EnemyData Default Role

**Possible issue:** All EnemyData assets have `defaultRole = Attacker`

**How to fix:**
1. Project window → Assets/Data/Enemies/
2. Select an EnemyData asset
3. Look for "Enemy Role" section
4. Change "Default Role" to "Attacker" (this will be overridden)
5. The WaveManager should override this anyway

---

#### 7. Verify Enemy Behavior State

**Add debug log to Enemy.cs Update() method:**

Find the Update() method and add at the top:
```csharp
private void Update()
{
    if (!isAlive || hasReachedEnd)
        return;
    
    // DEBUG: Remove after testing
    if (Time.frameCount % 60 == 0) // Log every 60 frames
    {
        Debug.Log($"{name}: Role={assignedRole}, State={behaviorState}");
    }
    
    // Rest of Update() code...
}
```

**Expected output:**
```
EnemyBasic(Clone): Role=Stealer, State=MovingToCorn
EnemyBasic(Clone): Role=Attacker, State=SeekingTower
```

---

#### 8. Check CornStorage Position

**Issue:** CornStorage at (0,0,0) might be unreachable

**How to check:**
1. Select CornStorage
2. Inspector → Transform → Position
3. Should be on your map (e.g., 10, 0, 0)
4. NOT at origin unless that's your map

**Visual check:**
- Scene view → Select CornStorage
- Yellow wire sphere shows grab radius
- Should be visible on map

---

#### 9. Force Stealer for Testing

**Temporary fix to force all enemies to be stealers:**

In `WaveManager.cs`, temporarily change:
```csharp
// OLD (line ~180)
EnemyRole assignedRole = roll < stealerPercentage ? EnemyRole.Stealer : EnemyRole.Attacker;

// TEMP TEST - Force all stealers
EnemyRole assignedRole = EnemyRole.Stealer;
```

**Expected:** ALL enemies should now go to corn storage

**If this works:** Problem is with the random assignment or percentage
**If this doesn't work:** Problem is with the SetRole() or behavior state logic

---

#### 10. Check for Compilation Errors

**Unity Console (bottom of editor):**
```
□ No red error messages
□ Scripts compiled successfully
□ "All compiler errors have to be fixed" message gone
```

**If errors exist:**
1. Double-click error to open script
2. Fix the error
3. Wait for recompilation
4. Try again

---

## Common Issues & Solutions

### Issue: "CornManager.Instance is null"

**Cause:** CornManager not in scene or Awake() not called

**Solution:**
1. Check CornManager exists in Hierarchy
2. Make sure scene is saved
3. Check Console for "CornManager: Subscribed to corn theft events"

---

### Issue: "Enable Corn Theft" field not visible in WaveManager

**Cause:** Script hasn't recompiled or cached Inspector

**Solution:**
1. Select a different GameObject
2. Select WaveManager again
3. Or: Close Unity and reopen
4. Or: Assets → Reimport All

---

### Issue: Enemies spawn but immediately disappear

**Cause:** Enemy prefab might be missing Enemy component

**Solution:**
1. Check EnemyManager has enemyPrefab assigned
2. Check prefab has Enemy.cs component
3. Check Console for errors

---

### Issue: Enemies freeze at corn storage

**Cause:** CornStorage has no corn OR grab radius too small

**Solution:**
1. Check CornStorage: Initial Corn Count > 0
2. Increase Grab Radius to 2.0 for testing
3. Check Console for "reached corn storage" message

---

## Step-by-Step Test Plan

### Test 1: Verify System Exists
```
1. Open Scene
2. Hierarchy → Search "Corn"
3. Should see: CornManager, CornStorage
   ✓ Pass: Both exist
   ✗ Fail: Add missing GameObjects
```

### Test 2: Verify Components
```
1. Select CornManager
2. Inspector shows "Corn Manager (Script)"
   ✓ Pass: Component attached
   ✗ Fail: Add CornManager component
```

### Test 3: Verify Configuration
```
1. Select WaveManager
2. Find "Enemy Role Distribution"
3. Enable Corn Theft: ✓
4. Stealer Percentage: 0.15
   ✓ Pass: Configured correctly
   ✗ Fail: Check settings
```

### Test 4: Play Mode Test
```
1. Press Play
2. Start first wave
3. Watch Console:
   - Should see "Spawned X as Stealer" (~15% of enemies)
   - Should see "Spawned X as Attacker" (~85% of enemies)
   ✓ Pass: Roles being assigned
   ✗ Fail: Check CornManager exists
```

### Test 5: Visual Confirmation
```
1. In Play mode during wave
2. Watch enemies:
   - Some should go toward corn storage
   - Some should attack towers
   ✓ Pass: Dual behavior working
   ✗ Fail: All attacking towers? See debug steps above
```

---

## Quick Fix Checklist

If nothing works, try this sequence:

1. **Close Unity**
2. **Delete Library folder** (Unity will regenerate)
3. **Reopen Unity** (wait for import)
4. **Check Hierarchy:** CornManager + CornStorage exist
5. **Select WaveManager:** Enable Corn Theft ✓
6. **Press Play**
7. **Check Console** for "Spawned X as Stealer" messages

---

## Contact Debug Info

When asking for help, provide:

```
Unity Version: [your version]
Scene Name: [your scene]
Console Logs: [copy first 10 lines]
WaveManager Settings:
  - Enable Corn Theft: [✓ or ✗]
  - Stealer Percentage: [value]
CornManager Exists: [Yes/No]
CornStorage Position: [x, y, z]
Enemy Count in Wave: [number]
```

---

## Success Indicators

You know it's working when you see:

✅ Console: "Spawned X as Stealer (roll: 0.12, threshold: 0.15)"  
✅ Console: "EnemyBasic(Clone) assigned as STEALER - heading to corn storage"  
✅ Visual: Some enemies move toward corn storage instead of towers  
✅ Visual: Yellow sphere appears when corn grabbed  
✅ Console: "[CornManager] Corn taken. Remaining: 19"

If you see all these, the system is working! 🎉
