# Quick Setup: 3-Trait Card Selection

## What You Need to Do in Unity

### Step 1: Create Trait Card Prefab

1. **Create new GameObject** in scene
   - Right-click in Hierarchy → UI → Button
   - Name it: `TraitCardPrefab`

2. **Add child elements:**
   ```
   TraitCardPrefab (Button)
   ├── TraitIcon (UI → Image)
   ├── TraitName (UI → Text - TextMeshPro)
   └── TraitDescription (UI → Text - TextMeshPro)
   ```

3. **Set names exactly as shown** (code looks for these names)

4. **Style the card:**
   - Card size: ~250x450 pixels
   - Background: Semi-transparent panel
   - Icon: Top center, 100x100
   - Name: Bold, 28pt
   - Description: Regular, 16pt, word wrap enabled

5. **Add hover effect (optional):**
   - Select Button component
   - Set Highlighted Color to light color
   - Set Pressed Color to bright color

6. **Make it a prefab:**
   - Drag `TraitCardPrefab` from Hierarchy to Project folder
   - Delete from scene
   - You now have a reusable prefab!

### Step 2: Assign in GameUI Inspector

1. **Select GameUI** GameObject in scene

2. **Find "Trait Selection" section** in Inspector

3. **Assign the prefab:**
   - **Trait Card Prefab** → Drag TraitCardPrefab from Project

4. **Container auto-setup:**
   - ✅ **No need to manually create a container!**
   - The system automatically finds or creates `TraitCardsContainer` inside your existing `TraitCardDialog`
   - It will add a HorizontalLayoutGroup automatically

### Step 3: Test It!

1. **Press Play**
2. **Start a wave**
3. **Click "Get Trait" button**
4. **You should see 3 trait cards!**
5. **Click any card to select it**

## Expected Result

```
Before:                  After:
┌─────────────┐         ┌───────┐ ┌───────┐ ┌───────┐
│  Poison     │         │ Rapid │ │Poison │ │Multi- │
│             │   →     │ Fire  │ │       │ │ shot  │
│ [Accept]    │         │       │ │       │ │       │
└─────────────┘         └───────┘ └───────┘ └───────┘
                         (Click any to choose)
```

## How It Works

### Automatic Container Setup

The system is **smart** and automatically:

1. **Looks for existing container** named `TraitCardsContainer` in your `TraitCardDialog`
2. **Creates one if not found** with proper layout settings:
   - HorizontalLayoutGroup (spacing: 20)
   - Centered alignment
   - Padding: 20px all sides
   - Auto-expand children

3. **Uses the existing TraitCardDialog** - no need to modify your scene!

### What Gets Created Automatically

When you first show traits:
```
TraitCardDialog (your existing dialog)
└── TraitCardsContainer (auto-created)
    ├── TraitCard1 (instantiated from prefab)
    ├── TraitCard2 (instantiated from prefab)
    └── TraitCard3 (instantiated from prefab)
```

## Troubleshooting

### Problem: Cards don't appear
**Check 1:** Is `Trait Card Prefab` assigned in GameUI inspector?
**Check 2:** Look for this in console:
```
"Created new TraitCardsContainer in traitCardDialog with HorizontalLayoutGroup"
```
This means the container was auto-created successfully.

**Check 3:** If you see:
```
"Using legacy single-card UI..."
```
→ Assign the `traitCardPrefab` in GameUI inspector

### Problem: Cards appear but have no text/icons
**Solution:** Check child object names in prefab:
- Must be exactly: `TraitName`, `TraitDescription`, `TraitIcon`
- Case-sensitive!

### Problem: Clicking cards does nothing
**Solution:** 
- Make sure prefab has Button component on root GameObject
- Check that Button is interactable (checked in inspector)

### Problem: Only 1 card appears or cards overlap
**Solution:**
- The auto-created container should handle this
- If you manually created a container, check HorizontalLayoutGroup settings
- Make sure TraitCardDialog is wide enough (800-900px)

### Problem: "traitCardDialog is null" error
**Solution:**
- Make sure GameUI has `Trait Card Dialog` assigned in inspector
- This should already be setup in your scene

## Visual Customization

### Card Styling Options

**Rarity Colors:**
```csharp
// In prefab, add colored border:
Common: Gray (#808080)
Rare: Blue (#3498db)
Epic: Purple (#9b59b6)
Legendary: Gold (#f39c12)
```

**Hover Effect:**
```
Button → Transition: Color Tint
Normal: White (1, 1, 1, 1)
Highlighted: Light Blue (0.8, 0.9, 1, 1)
Pressed: Bright Blue (0.6, 0.8, 1, 1)
Duration: 0.1s
```

**Layout Variations:**

**Horizontal (Current):**
```
[Card] [Card] [Card]
```

**Vertical:**
```
[Card]
[Card]
[Card]
```
Change HorizontalLayoutGroup → VerticalLayoutGroup

**Grid:**
```
[Card] [Card]
[Card]
```
Use GridLayoutGroup instead

## Code Reference

### How It Works
```csharp
// 1. Button clicked
ShowTraitCard()

// 2. Generate 3 traits
Generate3UniqueTraits()
→ Returns: [RapidFire, Poison, Multishot]

// 3. Create UI
Display3TraitCards()
→ Instantiates 3 cards from prefab
→ Sets name, description, icon for each
→ Adds click listener: SelectTrait(0/1/2)

// 4. User clicks card 1
SelectTrait(1)
→ selectedTrait = Poison

// 5. Accept
AcceptTrait()
→ availableTraitForAssignment = Poison
→ ClearTraitCards() - cleanup
→ Close dialog
```

## Next Steps

After basic setup works:
1. **Style the cards** to match your game's theme
2. **Add animations** (card flip, slide in, etc.)
3. **Add sound effects** (card flip sound, selection sound)
4. **Add hover tooltips** for more trait details
5. **Implement rarity system** for visual distinction

## Summary

✅ Create trait card prefab with proper child names  
✅ Assign prefab in GameUI inspector  
✅ Container auto-creates itself - no manual setup needed!  
✅ Works with your existing TraitCardDialog  
✅ Test and enjoy 3-trait selection!  

**Time to complete:** ~5-10 minutes (even faster now!)  
**Difficulty:** Easy (just create prefab and assign it)  
**Result:** Much more engaging trait selection! 🎴✨

## What's Different from Manual Setup?

**Old way:** Create container → Add layout → Position → Assign both container and prefab  
**New way:** Just create prefab → Assign prefab → Done! ✨

The system automatically:
- Finds or creates the container
- Adds HorizontalLayoutGroup
- Sets up all layout properties
- Works with your existing dialog

Zero hassle, maximum convenience! 🚀
