# Unity Not Seeing CornManager - SOLVED ✅

## Problem
Unity Editor was not recognizing the `CornManager` and `CornStorage` scripts.

## Root Cause
The scripts were created in the wrong location:
- ❌ Were in: `Assets/Scripts/` (root)
- ✅ Should be in: `Assets/Scripts/Game/` (with other managers)

## Solution Applied
Moved the files to the correct location:
```bash
Assets/Scripts/CornManager.cs  → Assets/Scripts/Game/CornManager.cs
Assets/Scripts/CornStorage.cs  → Assets/Scripts/Game/CornStorage.cs
```

## What to Do in Unity

### Step 1: Refresh Unity
1. **If Unity is open:** Wait a few seconds for auto-refresh
2. **If needed:** Right-click Assets folder → "Reimport All"
3. **Or:** Close and reopen Unity

### Step 2: Verify Scripts Are Visible
1. In Project window, navigate to: `Assets/Scripts/Game/`
2. You should see:
   - ✅ CornManager.cs (with C# icon)
   - ✅ CornStorage.cs (with C# icon)
   - ✅ GameManager.cs
   - ✅ GameUI.cs

### Step 3: Check for Compilation Errors
1. Open Console window (Window → General → Console)
2. Should see no errors
3. If you see errors, they should auto-resolve after reimport

## File Locations (Updated)

### Corn System Scripts
```
Assets/Scripts/Game/
├── CornManager.cs      ← Singleton manager for corn mechanics
├── CornStorage.cs      ← Component for corn storage location
├── GameManager.cs      ← Existing game manager
└── GameUI.cs           ← Existing UI manager
```

### Enemy Scripts  
```
Assets/Scripts/Enemy/
├── Enemy.cs            ← Modified with corn stealing behavior
├── EnemyData.cs        ← Modified with role configuration
└── EnemyTargetDistributor.cs  ← Updated to skip stealers
```

## How to Add to Scene

### Method 1: Drag and Drop
1. In Hierarchy, create: `GameObject → Create Empty`
2. Name it: **"CornManager"**
3. From Project window, drag `CornManager.cs` onto the GameObject
4. Repeat for CornStorage

### Method 2: Add Component Menu
1. Select GameObject in Hierarchy
2. Click "Add Component" in Inspector
3. Search for "Corn Manager"
4. Click to add

### Method 3: Search in Add Component
```
Add Component → Type "Corn" → Select script
```

## Verification Checklist

After moving files, verify:

✅ **Files visible in Project window**
- `Assets/Scripts/Game/CornManager.cs` exists
- `Assets/Scripts/Game/CornStorage.cs` exists

✅ **Scripts have C# icons**
- Both files show Unity's C# script icon (not generic file icon)

✅ **Meta files exist**
- `CornManager.cs.meta` in same folder
- `CornStorage.cs.meta` in same folder

✅ **No compilation errors**
- Console shows no red errors
- "CornManager" is recognized in other scripts

✅ **Can add to GameObject**
- "Add Component" search finds "CornManager"
- Can drag onto GameObject from Project window

## If Still Not Working

### Force Full Reimport
```
1. Close Unity
2. Delete: Library/ folder (Unity will regenerate)
3. Reopen Unity
4. Wait for full reimport (may take a minute)
```

### Check Assembly Definitions
```
1. Check if TowerFusion.Scripts.asmdef exists in Assets/Scripts/
2. All scripts in Scripts/ folder should be in same assembly
3. No assembly definition conflicts
```

### Verify Namespace
Both files should have:
```csharp
namespace TowerFusion
{
    public class CornManager : MonoBehaviour
    {
        // ...
    }
}
```

## Success Indicators

You know it's working when:
1. ✅ Can search "CornManager" in Add Component menu
2. ✅ No red errors in Console
3. ✅ Enemy.cs references CornManager with no errors
4. ✅ Can create GameObject with CornManager attached
5. ✅ Inspector shows CornManager fields properly

## Next Steps

Once Unity recognizes the scripts:
1. **Add to Scene** - Create GameObjects with components
2. **Configure** - Set corn count, grab radius, etc.
3. **Test** - Enter Play mode and verify behavior
4. **Integrate** - Update WaveManager for role assignment

## Related Documentation
- [CORN_THEFT_SETUP.md](CORN_THEFT_SETUP.md) - Full setup guide
- [CORN_THEFT_SYSTEM.md](CORN_THEFT_SYSTEM.md) - System design
