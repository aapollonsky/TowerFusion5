# Detailed Setup Guide: 3-Trait Card Selection System

## Overview

This guide will walk you through setting up the 3-trait card selection system with detailed, step-by-step instructions. By the end, players will see 3 trait cards to choose from instead of just 1.

**Time Required:** 10-15 minutes  
**Difficulty:** Beginner-friendly  
**Prerequisites:** Unity 2022.3+ with TextMeshPro installed

---

## Part 1: Create the Trait Card Prefab

### Step 1.1: Create the Base Button

1. **Open your Unity project** and navigate to the scene with your UI
2. **In the Hierarchy panel**, find your Canvas (usually named "Canvas" or "GameCanvas")
3. **Right-click on the Canvas** → Select **UI** → **Button - TextMeshPro**
   - If you see a popup about importing TMP Essentials, click **Import TMP Essentials**
4. **Rename the button:**
   - Select the button in Hierarchy
   - In the Inspector, change the name at the top to: `TraitCardPrefab`

### Step 1.2: Remove Default Text

1. **Select TraitCardPrefab** in Hierarchy
2. **Expand it** (click the arrow next to it) to see its children
3. You'll see a child object named **"Text (TMP)"**
4. **Right-click on "Text (TMP)"** → Select **Delete**
   - We're removing this because we'll add our own custom text elements

### Step 1.3: Add Background Image

1. **Select TraitCardPrefab** in Hierarchy
2. **In the Inspector**, find the **Image component** (should already exist)
3. **Configure the Image:**
   - **Color:** Set to a semi-transparent dark color
     - Click the color picker
     - Set RGB: (0.2, 0.2, 0.2) - dark gray
     - Set A: 0.9 - mostly opaque
   - **Raycast Target:** ✓ Keep checked (needed for clicking)

### Step 1.4: Add Trait Icon (Image)

1. **Right-click on TraitCardPrefab** → **UI** → **Image**
2. **Rename it to:** `TraitIcon` (**exact name, case-sensitive!**)
3. **Configure the RectTransform:**
   - Click the anchor preset (top-left of RectTransform)
   - Hold **Shift + Alt** and click **Top Center**
   - Set **Pos Y:** -60 (move down from top)
   - Set **Width:** 100
   - Set **Height:** 100
4. **Configure the Image:**
   - **Color:** White (so trait icon colors show properly)
   - **Preserve Aspect:** ✓ Check this
   - **Raycast Target:** ☐ Uncheck (we click the card, not the icon)

### Step 1.5: Add Trait Name (Text)

1. **Right-click on TraitCardPrefab** → **UI** → **Text - TextMeshPro**
2. **Rename it to:** `TraitName` (**exact name, case-sensitive!**)
3. **Configure the RectTransform:**
   - Anchor preset: Hold **Shift + Alt**, click **Top Stretch**
   - Set **Pos Y:** -170 (below the icon)
   - Set **Height:** 50
   - Set **Left:** 10
   - Set **Right:** 10
4. **Configure the TextMeshPro component:**
   - **Text:** "Trait Name" (placeholder)
   - **Font Style:** Bold
   - **Font Size:** 28
   - **Alignment:** Center (horizontal and vertical)
   - **Color:** White or Yellow
   - **Wrapping:** ✓ Enable Word Wrapping
   - **Overflow:** Truncate
   - **Raycast Target:** ☐ Uncheck

### Step 1.6: Add Trait Description (Text)

1. **Right-click on TraitCardPrefab** → **UI** → **Text - TextMeshPro**
2. **Rename it to:** `TraitDescription` (**exact name, case-sensitive!**)
3. **Configure the RectTransform:**
   - Anchor preset: Hold **Shift + Alt**, click **Stretch (bottom-right icon)**
   - Set **Top:** 230 (below trait name)
   - Set **Bottom:** 20
   - Set **Left:** 20
   - Set **Right:** 20
4. **Configure the TextMeshPro component:**
   - **Text:** "Description of the trait effect..." (placeholder)
   - **Font Style:** Regular
   - **Font Size:** 16
   - **Alignment:** Top Center
   - **Color:** Light Gray (RGB: 0.8, 0.8, 0.8)
   - **Wrapping:** ✓ Enable Word Wrapping
   - **Overflow:** Truncate
   - **Raycast Target:** ☐ Uncheck

### Step 1.7: Configure Card Size

1. **Select TraitCardPrefab** (the root)
2. **In the Inspector, configure RectTransform:**
   - **Width:** 250
   - **Height:** 450
3. **This makes a nice vertical card shape**

### Step 1.8: Add Visual Polish (Optional)

#### Add Border/Outline:
1. **Select TraitCardPrefab**
2. **Add Component** → **UI** → **Outline**
3. **Configure Outline:**
   - **Effect Color:** Black or gold
   - **Effect Distance:** X: 2, Y: -2
   - **Use Graphic Alpha:** ✓ Check

#### Add Shadow:
1. **Select TraitCardPrefab**
2. **Add Component** → **UI** → **Shadow**
3. **Configure Shadow:**
   - **Effect Color:** Black, Alpha: 0.5
   - **Effect Distance:** X: 5, Y: -5
   - **Use Graphic Alpha:** ✓ Check

### Step 1.9: Configure Button Interaction

1. **Select TraitCardPrefab**
2. **Find the Button component** in Inspector
3. **Configure Transition:**
   - **Transition:** Color Tint
   - **Target Graphic:** Drag the **Image** component from below
   - **Normal Color:** White (1, 1, 1, 1)
   - **Highlighted Color:** Light Blue (0.8, 0.9, 1, 1)
   - **Pressed Color:** Bright Blue (0.6, 0.8, 1, 1)
   - **Selected Color:** White
   - **Disabled Color:** Gray (0.5, 0.5, 0.5, 0.5)
   - **Color Multiplier:** 1
   - **Fade Duration:** 0.1

### Step 1.10: Verify Hierarchy Structure

Your hierarchy should look exactly like this:

```
TraitCardPrefab (Button, Image)
├── TraitIcon (Image)
├── TraitName (TextMeshProUGUI)
└── TraitDescription (TextMeshProUGUI)
```

**Critical:** The child names must be **exact** (case-sensitive):
- `TraitIcon` ← not "Icon" or "traitIcon"
- `TraitName` ← not "Name" or "Trait Name"
- `TraitDescription` ← not "Description" or "Trait Description"

### Step 1.11: Create the Prefab

1. **In the Project panel**, navigate to a folder where you want to store the prefab
   - Recommended: `Assets/Prefabs/UI/` or `Assets/UI/Prefabs/`
   - Create folders if they don't exist (Right-click → Create → Folder)

2. **Drag TraitCardPrefab** from the Hierarchy into your chosen Project folder
   - You'll see it turn blue in Hierarchy (indicating it's now a prefab)
   - A prefab file appears in the Project panel

3. **Delete from Hierarchy:**
   - Right-click **TraitCardPrefab** in Hierarchy
   - Select **Delete**
   - We don't need it in the scene, only as a prefab

---

## Part 2: Assign the Prefab to GameUI

### Step 2.1: Locate GameUI

1. **In the Hierarchy**, find the GameObject with the **GameUI** component
   - It's usually named "GameUI" or "UIManager"
   - Often a child of Canvas
   - **Tip:** Use Hierarchy search - type "GameUI" in the search box

2. **Select the GameUI GameObject**

### Step 2.2: Find the Trait Selection Section

1. **In the Inspector**, scroll down to find the **GameUI (Script)** component
2. **Look for the section:** "Trait Selection"
3. You should see these fields:
   - Trait Card Dialog
   - Trait Cards Container
   - Trait Card Prefab ← **This is what we need to assign**
   - Trait Name Text (legacy)
   - Trait Description Text (legacy)
   - Trait Icon Image (legacy)
   - Trait Done Button

### Step 2.3: Assign the Prefab

1. **In the Project panel**, navigate to where you saved the prefab
2. **Drag the TraitCardPrefab** into the **Trait Card Prefab** field in the Inspector
   - OR click the circle ⊙ next to the field and select it from the popup

3. **Verify it's assigned:**
   - The field should now show: `TraitCardPrefab` in gray text
   - Not empty, not "None (Game Object)"

### Step 2.4: Check Existing Fields

Make sure these are already assigned (they should be from your existing setup):

- **Trait Card Dialog:** Should reference your existing trait dialog GameObject
  - Usually something like "TraitCardDialog" or "TraitSelectionPanel"
  - This is the popup that appears when you click "Get Trait"

**Note:** You do NOT need to assign:
- Trait Cards Container ← Auto-created
- Legacy fields (Name Text, Description Text, Icon Image) ← Optional fallback

---

## Part 3: Test the System

### Step 3.1: Enter Play Mode

1. **Click the Play button** ▶️ at the top of Unity
2. **Wait for the game to load**

### Step 3.2: Start a Wave

1. **Click "Start Wave"** or similar button to begin
2. **Let some enemies spawn**

### Step 3.3: Open Trait Selection

1. **Click the "Get Trait" button** (or whatever your trait button is named)
2. **You should see:**
   - A dialog appears (your existing TraitCardDialog)
   - **3 trait cards** displayed horizontally
   - Each card shows:
     - Icon at top
     - Trait name in middle
     - Description at bottom

### Step 3.4: Select a Trait

1. **Click on any of the 3 cards**
2. **Verify:**
   - Dialog closes
   - Selected trait is now available for assignment
   - You see "Apply [TraitName]" button when selecting a tower

### Step 3.5: Check Console

1. **Open the Console window** (Window → General → Console)
2. **Look for these messages** (means it's working):
   ```
   Generated 3 trait options: RapidFire, Poison, Multishot
   Created new TraitCardsContainer in traitCardDialog with HorizontalLayoutGroup
   Showing trait selection dialog with 3 options
   User selected trait: Poison
   ```

---

## Part 4: Troubleshooting

### Problem 1: Still Shows Old Single-Card UI

**This is the most common issue!**

**Cause:** The trait card prefab isn't assigned, so the system falls back to the legacy single-card UI.

**Solution:**
1. **Exit Play mode** if you're in it
2. Select **GameUI** in Hierarchy
3. Inspector → **GameUI (Script)** → **Trait Selection** section
4. Find **"Trait Card Prefab"** field
5. Is it assigned? 
   - ❌ Shows `None (Game Object)` → **Assign your prefab!**
   - ✅ Shows `TraitCardPrefab` → It's assigned correctly

**To assign:**
- In Project panel, find your `TraitCardPrefab`
- Drag it into the **Trait Card Prefab** field
- OR click the circle ⊙ button and select it

**Console Check:**
- If you see: `"Using legacy single-card UI..."` in Console
- This confirms prefab is not assigned

### Problem 2: Cards Don't Appear

**Check A - Dialog Exists:**
1. Select GameUI
2. Is "Trait Card Dialog" assigned?
   - Should reference your existing dialog
   - If empty, drag your trait dialog GameObject here

**Check B - Console Errors:**
1. Open Console
2. Look for red error messages
3. Common errors:
   - `"traitCardDialog is null!"` → Assign dialog
   - `"Could not find or create trait cards container"` → Dialog is null

### Problem 3: Cards Appear but Are Blank

**Cause:** Child object names don't match expected names

**Solution:**
1. Open your TraitCardPrefab in Project panel (double-click)
2. Check child object names in Hierarchy:
   - Must be **exactly:** `TraitIcon`, `TraitName`, `TraitDescription`
   - Case matters! `traitName` won't work
3. Rename if needed (click name, press F2)
4. Save prefab (Ctrl+S or Cmd+S)

### Problem 4: Can't Click Cards

**Cause:** Button component missing or disabled

**Solution:**
1. Open TraitCardPrefab
2. Select the root GameObject
3. Check Inspector:
   - Is there a **Button** component?
   - Is **Interactable** checked?
   - Is **Transition** set to "Color Tint"?
4. If Button missing: Add Component → UI → Button

### Problem 5: Only 1 Card Appears

**Cause:** Container too narrow or layout issue

**Solution:**
1. Enter Play mode
2. In Hierarchy, find your TraitCardDialog
3. Look for a child called "TraitCardsContainer" (auto-created)
4. Select it, check Inspector:
   - **RectTransform Width:** Should be 800-900+
   - **Horizontal Layout Group** component exists
   - **Spacing:** 20
   - **Child Force Expand Width:** ✓ Checked

### Problem 6: Cards Overlap

**Solution:**
1. Find TraitCardsContainer in Hierarchy (while in Play mode)
2. Select it → Inspector → Horizontal Layout Group
3. Increase **Spacing** to 30 or 40
4. OR make TraitCardDialog wider
5. OR make cards narrower (edit prefab width to 200)

### Problem 7: "traitCardDialog is null" Error

**Cause:** GameUI doesn't know where the dialog is

**Solution:**
1. Select GameUI in Hierarchy
2. Inspector → GameUI (Script) → Trait Selection
3. Find **Trait Card Dialog** field at top
4. Drag your trait dialog GameObject here
   - Usually in Canvas → TraitCardDialog or similar
   - The panel that pops up when you click "Get Trait"

---

## Part 5: Customization

### Change Card Appearance

#### Card Size:
1. Open TraitCardPrefab (double-click in Project)
2. Select root → Inspector → RectTransform
3. Change **Width** and **Height**
4. Common sizes:
   - Tall: 250x450 (default)
   - Square: 300x300
   - Wide: 400x300

#### Colors:
1. Select TraitCardPrefab root
2. Inspector → Image → Color
3. Try different themes:
   - Dark: RGB(0.2, 0.2, 0.2), A: 0.9
   - Light: RGB(0.9, 0.9, 0.9), A: 0.95
   - Blue: RGB(0.2, 0.3, 0.5), A: 0.9

#### Fonts:
1. Select TraitName or TraitDescription
2. Inspector → TextMeshProUGUI
3. Change **Font Asset**
4. Change **Font Size**

### Change Layout

#### Vertical Layout (Cards stacked):
While in Play mode:
1. Find TraitCardsContainer in Hierarchy
2. Inspector → Remove **Horizontal Layout Group**
3. Add Component → Layout → **Vertical Layout Group**
4. Set Spacing: 20

#### Grid Layout (2x2 or 3x1):
While in Play mode:
1. Find TraitCardsContainer
2. Inspector → Remove Horizontal Layout Group
3. Add Component → Layout → **Grid Layout Group**
4. Set Cell Size: 250x400
5. Set Spacing: 20, 20

**Note:** Layout changes in Play mode are temporary. To make permanent:
1. Note the settings you like
2. Exit Play mode
3. Manually create TraitCardsContainer in your scene
4. Add and configure the layout component
5. Assign it to GameUI → Trait Cards Container field

### Change Number of Cards

**To show 5 cards instead of 3:**

1. Open `GameUI.cs` in your code editor
2. Find this line (around line 58):
   ```csharp
   private TowerTrait[] currentTraitOptions = new TowerTrait[3];
   ```
3. Change 3 to 5:
   ```csharp
   private TowerTrait[] currentTraitOptions = new TowerTrait[5];
   ```
4. Find this line:
   ```csharp
   private GameObject[] traitCardInstances = new GameObject[3];
   ```
5. Change to 5:
   ```csharp
   private GameObject[] traitCardInstances = new GameObject[5];
   ```
6. Find method `Generate3UniqueTraits()` (line ~723)
7. Change loop to 5:
   ```csharp
   for (int i = 0; i < 5; i++)
   ```
8. Save and return to Unity

### Add Sound Effects

1. **Get sound files:**
   - Card flip sound (whoosh.wav)
   - Selection sound (click.wav)

2. **Import to Unity:**
   - Drag sound files into Project → Assets/Audio/

3. **Create Audio Source:**
   - Select GameUI in Hierarchy
   - Add Component → Audio → Audio Source
   - Uncheck "Play On Awake"

4. **Assign sounds in code** (requires scripting):
   ```csharp
   // In GameUI.cs, add fields:
   [SerializeField] private AudioClip cardFlipSound;
   [SerializeField] private AudioClip selectionSound;
   [SerializeField] private AudioSource audioSource;
   
   // In Display3TraitCards():
   if (audioSource != null && cardFlipSound != null)
       audioSource.PlayOneShot(cardFlipSound);
   
   // In SelectTrait():
   if (audioSource != null && selectionSound != null)
       audioSource.PlayOneShot(selectionSound);
   ```

---

## Part 6: Verification Checklist

Before finishing, verify everything works:

### Prefab Check:
- [ ] Prefab exists in Project panel
- [ ] Prefab named `TraitCardPrefab`
- [ ] Has Button component on root
- [ ] Has 3 children: TraitIcon, TraitName, TraitDescription (exact names)
- [ ] Children have correct components (Image, TextMeshProUGUI)
- [ ] Card looks good in prefab preview

### Inspector Check:
- [ ] GameUI found in scene
- [ ] Trait Card Prefab field assigned
- [ ] Trait Card Dialog field assigned (existing dialog)
- [ ] No errors in Inspector

### Runtime Check:
- [ ] Enter Play mode successfully
- [ ] Click "Get Trait" button
- [ ] 3 cards appear horizontally
- [ ] Cards show icon, name, description
- [ ] Can click any card
- [ ] Dialog closes after selection
- [ ] Selected trait becomes available
- [ ] Can apply trait to tower

### Console Check:
- [ ] No red errors
- [ ] See "Generated 3 trait options..." message
- [ ] See "Created new TraitCardsContainer..." message
- [ ] See "User selected trait..." message
- [ ] No yellow warnings (or only harmless ones)

---

## Quick Reference

### File Locations
- **Prefab:** `Assets/Prefabs/UI/TraitCardPrefab.prefab`
- **Script:** `Assets/Scripts/Game/GameUI.cs`
- **Scene:** Your main game scene

### Important Names (Case-Sensitive!)
- `TraitCardPrefab` ← Prefab name
- `TraitIcon` ← Image child
- `TraitName` ← Text child
- `TraitDescription` ← Text child
- `TraitCardsContainer` ← Auto-created container

### Key Inspector Fields
- **GameUI → Trait Card Prefab** ← Assign your prefab here
- **GameUI → Trait Card Dialog** ← Your existing dialog

### Console Messages (Success)
```
Generated 3 trait options: [names]
Created new TraitCardsContainer in traitCardDialog with HorizontalLayoutGroup
Showing trait selection dialog with 3 options
User selected trait: [name]
```

---

## Getting Help

### Still Having Issues?

1. **Check Console for errors** - Red text gives clues
2. **Verify exact names** - Case-sensitive!
3. **Check prefab structure** - 3 children with correct components
4. **Try legacy UI** - Remove prefab assignment to see single-card fallback
5. **Check existing dialog** - Does "Get Trait" button work at all?

### Common Mistakes
- ❌ Child named "Icon" instead of "TraitIcon"
- ❌ Forgot to make it a prefab (still in scene)
- ❌ Assigned GameObject instead of prefab
- ❌ Dialog too narrow for 3 cards
- ❌ Button not on root GameObject

### Debug Mode
Add these to see detailed logs:

In `GameUI.cs`, in `Display3TraitCards()`, add:
```csharp
Debug.Log($"Container: {traitCardsContainer?.name ?? "null"}");
Debug.Log($"Prefab: {traitCardPrefab?.name ?? "null"}");
Debug.Log($"Creating card {i}: {currentTraitOptions[i]?.traitName}");
```

---

## Success!

If you made it here and everything works:

🎉 **Congratulations!** You now have a 3-trait card selection system!

Players can now:
- ✅ See 3 unique trait options
- ✅ Choose which trait they want
- ✅ Make strategic decisions
- ✅ Enjoy more engaging gameplay

The system automatically:
- ✅ Generates 3 unique traits
- ✅ Creates the container
- ✅ Adds proper layout
- ✅ Handles cleanup
- ✅ Falls back to legacy UI if needed

**Next steps:**
- Customize card appearance
- Add more traits
- Add sound effects
- Add animations
- Share your game! 🎮

---

## Summary

**What you did:**
1. Created TraitCardPrefab with 3 children
2. Assigned prefab to GameUI
3. Tested in Play mode

**What you get:**
- 3 trait cards instead of 1
- Player choice and strategy
- Auto-managed container
- Professional UI

**Time spent:** ~10-15 minutes  
**Difficulty:** ⭐⭐☆☆☆ (Easy)  
**Result:** 🌟🌟🌟🌟🌟 (Amazing!)

Enjoy your enhanced trait selection system! 🎴✨
