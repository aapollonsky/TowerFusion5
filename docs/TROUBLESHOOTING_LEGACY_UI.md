# Troubleshooting: Still Shows Old Single-Card UI

## Problem

After setting up the 3-trait card system, the trait dialog still shows the **old single-card UI** instead of 3 cards.

## What You See

❌ **Wrong (Legacy UI):**
```
┌─────────────────────┐
│   [Icon]            │
│                     │
│   Rapid Fire        │
│                     │
│ Increases attack... │
│                     │
│     [Accept]        │
└─────────────────────┘
```

✅ **Correct (3-Card UI):**
```
┌────────┐ ┌────────┐ ┌────────┐
│ [Icon] │ │ [Icon] │ │ [Icon] │
│ Rapid  │ │ Poison │ │ Multi- │
│ Fire   │ │        │ │ shot   │
└────────┘ └────────┘ └────────┘
```

---

## Root Cause

The system **automatically falls back** to the legacy single-card UI when:
1. **Trait Card Prefab is not assigned** in GameUI inspector
2. **Trait Card Prefab is null** or deleted
3. **Prefab reference is broken** (prefab was moved/renamed)

---

## Solution: Assign the Prefab

### Step 1: Exit Play Mode
- Click the **Stop button** ⏹️ if in Play mode
- Changes made in Play mode don't persist!

### Step 2: Select GameUI
1. Open Unity Editor
2. In **Hierarchy panel**, search for "GameUI"
3. Click on the **GameUI** GameObject

### Step 3: Find Trait Selection Section
1. In **Inspector panel**, scroll down
2. Find **GameUI (Script)** component
3. Look for the **"Trait Selection"** header

### Step 4: Check Trait Card Prefab Field
Look at the **"Trait Card Prefab"** field:

**If it shows:** `None (Game Object)`
- ❌ **Not assigned!** This is the problem.

**If it shows:** `TraitCardPrefab` (in gray text)
- ✅ **Assigned correctly**

### Step 5: Assign the Prefab
1. In **Project panel**, navigate to where you saved the prefab
   - Usually: `Assets/Prefabs/UI/TraitCardPrefab`
2. **Drag the prefab** into the **Trait Card Prefab** field
3. **OR** click the circle ⊙ button next to the field
   - A popup appears
   - Find and double-click **TraitCardPrefab**

### Step 6: Verify Assignment
The field should now show:
```
Trait Card Prefab: TraitCardPrefab
```

### Step 7: Test Again
1. Click **Play** ▶️
2. Start a wave
3. Click "Get Trait" button
4. **You should now see 3 cards!**

---

## Console Messages

### If Prefab Not Assigned

When you open the trait dialog, Console shows:
```
Using legacy single-card UI. For 3-card selection, assign traitCardsContainer and traitCardPrefab in inspector.
```

**This confirms the prefab is not assigned.**

### If Prefab Assigned Correctly

Console shows:
```
Generated 3 trait options: RapidFire, Poison, Multishot
Created new TraitCardsContainer in traitCardDialog with HorizontalLayoutGroup
Showing trait selection dialog with 3 options
```

**This confirms it's working!**

---

## How the System Works

### Decision Flow

```
ShowTraitCard()
  ↓
Generate 3 unique traits
  ↓
Is traitCardPrefab assigned?
  ├─ YES → Display3TraitCards()
  │           ↓
  │         Hide legacy UI elements
  │         Create 3 cards
  │         Show 3-card system ✓
  │
  └─ NO → DisplayLegacySingleTraitCard()
              ↓
            Show legacy UI elements
            Display single trait
            Log warning message
```

### Why Legacy UI Exists

The legacy single-card UI is kept as a **fallback**:
- Allows gradual migration from old to new system
- Prevents complete breakage if setup is incomplete
- Provides clear warning message in Console
- Game still works (just with 1 card instead of 3)

---

## Additional Checks

### Prefab Still Exists?

1. Navigate to prefab location in Project panel
2. Can you see **TraitCardPrefab**?
   - ✅ Yes → Good, just need to assign it
   - ❌ No → Need to create it again (see setup guide)

### Prefab Has Correct Structure?

1. **Double-click** TraitCardPrefab in Project
2. In Hierarchy (prefab mode), verify structure:
   ```
   TraitCardPrefab (Button, Image)
   ├── TraitIcon (Image)
   ├── TraitName (TextMeshProUGUI)
   └── TraitDescription (TextMeshProUGUI)
   ```
3. Names must be **exact** (case-sensitive)

### GameUI References Correct Prefab?

1. Select GameUI in Hierarchy
2. Inspector → GameUI (Script)
3. Look at **Trait Card Prefab** field
4. **Hover over it** - tooltip should show full path
5. Does path match where your prefab actually is?

---

## Common Mistakes

### ❌ Mistake 1: Assigned GameObject Instead of Prefab

**Wrong:** Dragging TraitCardPrefab from **Hierarchy** (blue icon)  
**Right:** Dragging TraitCardPrefab from **Project** (cube icon)

If you drag from Hierarchy, you're assigning a scene object, not a prefab. System can't instantiate scene objects.

### ❌ Mistake 2: Made Changes in Play Mode

**Problem:** Assigned prefab while in Play mode  
**Result:** Assignment lost when exiting Play mode

**Solution:** Always exit Play mode before making Inspector changes

### ❌ Mistake 3: Prefab Was Deleted

**Problem:** Prefab was deleted or moved  
**Result:** Field shows `None (Game Object)` or `Missing (Game Object)`

**Solution:** Recreate prefab following setup guide

### ❌ Mistake 4: Wrong Prefab Assigned

**Problem:** Assigned a different prefab (e.g., button prefab, tower prefab)  
**Result:** Cards appear but are blank or broken

**Solution:** 
1. Clear the field (click X button)
2. Assign the correct **TraitCardPrefab**

---

## Quick Fix Checklist

Run through this checklist:

- [ ] Exit Play mode (if in it)
- [ ] GameUI GameObject selected in Hierarchy
- [ ] GameUI (Script) component visible in Inspector
- [ ] "Trait Selection" section found
- [ ] "Trait Card Prefab" field found
- [ ] Prefab exists in Project panel
- [ ] Prefab dragged into field
- [ ] Field shows `TraitCardPrefab` (not `None`)
- [ ] Press Play and test
- [ ] Open trait dialog
- [ ] See 3 cards (not 1)
- [ ] Console shows success messages (not warning)

---

## Still Not Working?

### Double-Check These:

1. **Prefab name:** Must be `TraitCardPrefab` (or adjust code)
2. **Child names:** Must be `TraitIcon`, `TraitName`, `TraitDescription`
3. **Components:** Button on root, Image/TextMeshPro on children
4. **GameUI script:** Has latest code with 3-card system
5. **Unity version:** 2022.3+ recommended

### Try This:

1. **Create a test:**
   - Create a brand new GameObject
   - Add GameUI component
   - Assign just the Trait Card Prefab field
   - See if it works on clean slate

2. **Reimport assets:**
   - Right-click GameUI.cs in Project
   - Select "Reimport"
   - Wait for compilation

3. **Check Console for errors:**
   - Any red errors when entering Play mode?
   - Fix those first before testing traits

---

## Success Criteria

You'll know it's working when:

✅ **No warning in Console** about "legacy single-card UI"  
✅ **3 cards appear** horizontally when opening trait dialog  
✅ **Can click any of the 3 cards** to select them  
✅ **Legacy UI elements are hidden** (old name/description text not visible)  
✅ **Console shows** "Created new TraitCardsContainer..."  

---

## Summary

**Problem:** Shows old single-card UI  
**Cause:** Trait Card Prefab not assigned  
**Solution:** Exit Play mode → Select GameUI → Assign prefab → Test  
**Verify:** See 3 cards + no console warning  

**Time to fix:** ~30 seconds  
**Difficulty:** ⭐☆☆☆☆ (Very Easy)

The system is designed to **never break** - it just falls back to the old UI if setup is incomplete. Simply assign the prefab and you're good to go! 🎴✨
