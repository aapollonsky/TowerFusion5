# Feature: 3-Trait Card Selection System

## Overview
Enhanced the trait selection system to show **3 random unique trait cards** instead of a single trait, allowing players to choose which trait they want.

## Change Date
October 19, 2025

## What Changed

### Before
- Clicking the trait button showed **1 random trait**
- Player could only accept or reject that single trait
- No meaningful choice for the player

### After
- Clicking the trait button shows **3 random unique traits**
- Player can **choose which trait** they want
- More strategic gameplay and player agency

## Implementation

### New UI Components

#### GameUI.cs - New Fields
```csharp
[Header("Trait Selection")]
[SerializeField] private GameObject traitCardDialog;
[SerializeField] private Transform traitCardsContainer; // NEW: Container for 3 trait cards
[SerializeField] private GameObject traitCardPrefab;    // NEW: Prefab for individual trait card
```

#### New Private Fields
```csharp
private TowerTrait[] currentTraitOptions = new TowerTrait[3]; // 3 traits to choose from
private GameObject[] traitCardInstances = new GameObject[3];  // UI instances
```

### New Methods

#### `Generate3UniqueTraits()` - Lines 676-721
```csharp
private TowerTrait[] Generate3UniqueTraits()
```
- Generates 3 unique random traits from the available pool
- Respects trait probabilities (uses `GenerateRandomTrait()` internally)
- Ensures no duplicates (unless fewer than 3 traits exist)
- Returns array of 3 TowerTrait objects

**Algorithm:**
1. Create pool of all valid traits (excluding nulls)
2. Check if we have at least 3 traits (if not, allow duplicates)
3. Generate 3 traits using weighted random selection
4. Regenerate if duplicate detected (max 50 attempts per trait)
5. Return array of 3 unique traits

#### `Display3TraitCards()` - Lines 512-553
```csharp
private void Display3TraitCards()
```
- Clears existing trait card instances
- Instantiates 3 trait card prefabs in the container
- Populates each card with:
  - Trait name
  - Trait description
  - Trait icon
- Adds click listener to each card to call `SelectTrait(index)`

**Expected Prefab Structure:**
```
TraitCardPrefab (GameObject)
├── TraitName (TextMeshProUGUI)
├── TraitDescription (TextMeshProUGUI)
├── TraitIcon (Image)
└── Button (Component on root)
```

#### `DisplayLegacySingleTraitCard()` - Lines 555-572
```csharp
private void DisplayLegacySingleTraitCard()
```
- Fallback for old UI system
- Uses first trait from the 3 options
- Allows gradual migration to new system
- Logs warning suggesting to use 3-card system

#### `ClearTraitCards()` - Lines 574-585
```csharp
private void ClearTraitCards()
```
- Destroys all instantiated trait card GameObjects
- Clears the `traitCardInstances` array
- Called when closing dialog or generating new cards

#### `SelectTrait(int index)` - Lines 587-601
```csharp
private void SelectTrait(int index)
```
- Called when user clicks one of the 3 trait cards
- Validates index and trait exists
- Sets `selectedTrait` to the chosen option
- Calls `AcceptTrait()` to finalize selection

### Modified Methods

#### `ShowTraitCard()` - Lines 468-510
**Before:**
```csharp
selectedTrait = GenerateRandomTrait();
// Display single trait in UI
```

**After:**
```csharp
currentTraitOptions = Generate3UniqueTraits();
if (traitCardsContainer != null && traitCardPrefab != null)
    Display3TraitCards();  // NEW: 3-card system
else
    DisplayLegacySingleTraitCard();  // Fallback
```

#### `AcceptTrait()` - Lines 613-634
**Added cleanup:**
```csharp
ClearTraitCards();  // NEW: Clean up card instances
currentTraitOptions = new TowerTrait[3];  // NEW: Reset options
```

## Unity Setup

### Inspector Configuration

**GameUI Component:**
1. **Trait Card Dialog** - The parent dialog GameObject
2. **Trait Cards Container** ⭐ **NEW** - Empty Transform to hold 3 cards (e.g., HorizontalLayoutGroup)
3. **Trait Card Prefab** ⭐ **NEW** - Prefab with Button, texts, and icon

### Creating the Trait Card Prefab

**Recommended Structure:**
```
TraitCardPrefab (Prefab)
├── Button (Component on root)
├── Background (Image)
├── TraitIcon (Image)
│   └── Named: "TraitIcon"
├── TraitName (TextMeshProUGUI)
│   └── Named: "TraitName"
└── TraitDescription (TextMeshProUGUI)
    └── Named: "TraitDescription"
```

**Layout Recommendations:**
- Card Size: ~200-300px wide, 400-500px tall
- Icon: Top center, 100x100px
- Name: Below icon, large font (24-32pt)
- Description: Below name, smaller font (14-18pt)
- Button component on root for clicking
- Add hover effect for visual feedback

**Container Setup:**
```
TraitCardsContainer (GameObject)
├── Horizontal Layout Group
│   ├── Spacing: 20
│   ├── Child Alignment: Middle Center
│   └── Child Force Expand: Width & Height
└── (Cards instantiated here at runtime)
```

## Gameplay Flow

### User Experience

**Step 1: Click Trait Button**
```
Player clicks "Get Trait" button
↓
System generates 3 unique random traits
↓
Dialog opens showing 3 trait cards side-by-side
```

**Step 2: Choose Trait**
```
Player sees:
- [Card 1: Rapid Fire] [Card 2: Poison] [Card 3: Multishot]
↓
Player clicks on "Poison"
↓
System selects "Poison" trait
```

**Step 3: Apply Trait**
```
Dialog closes
↓
"Apply Poison" button appears when tower selected
↓
Player applies trait to chosen tower
```

## Code Example

### How Selection Works

```csharp
// 1. Generate 3 unique traits
currentTraitOptions = Generate3UniqueTraits();
// Result: [RapidFire, Poison, Multishot]

// 2. Display as 3 cards
Display3TraitCards();
// Creates 3 card instances with click handlers

// 3. User clicks card 1 (Poison)
SelectTrait(1);
// Sets selectedTrait = currentTraitOptions[1] (Poison)

// 4. Accept the selection
AcceptTrait();
// availableTraitForAssignment = Poison
// ClearTraitCards() - Clean up UI
```

## Backward Compatibility

### Legacy UI Support

If `traitCardsContainer` or `traitCardPrefab` is **NOT assigned:**
- System falls back to old single-card UI
- Uses `traitNameText`, `traitDescriptionText`, `traitIconImage`
- Shows only the first of the 3 generated traits
- Legacy "Done" button accepts the trait

**Migration Path:**
1. Keep old UI elements assigned during transition
2. Create new 3-card prefab and container
3. Assign new fields in inspector
4. Test 3-card system
5. Remove old single-card UI elements

## Probability System

### How Trait Probabilities Work

The `traitProbabilities` array is **still respected**:

```csharp
// Each trait generation uses weighted random
for (int i = 0; i < 3; i++)
{
    trait = GenerateRandomTrait(); // Uses probabilities
    // Ensure uniqueness
}
```

**Example:**
- Rapid Fire: 15% chance
- Poison: 15% chance
- Multishot: 15% chance
- Piercing: 15% chance
- Slow: 15% chance
- Explosive: 12.5% chance
- Chain Lightning: 12.5% chance

When generating 3 traits, each generation respects these weights, but no duplicates appear.

## Edge Cases

### Fewer Than 3 Traits Available

**Scenario:** Only 2 traits exist in `availableTraits`

**Behavior:**
```csharp
bool allowDuplicates = availablePool.Count < 3;
// allowDuplicates = true

// Result: Some cards may show duplicate traits
// [RapidFire, Poison, RapidFire]
```

### All Traits Same Probability

**Scenario:** `traitProbabilities` array is null or wrong length

**Behavior:**
```csharp
// Fallback to equal probability
int randomIndex = Random.Range(0, availableTraits.Length);
return availableTraits[randomIndex];
```

### Trait Generation Timeout

**Scenario:** Can't find unique traits after 50 attempts

**Behavior:**
```csharp
while (System.Array.IndexOf(selectedTraits, trait) >= 0 && attempts < 50)
{
    trait = GenerateRandomTrait();
    attempts++;
}
// After 50 attempts, uses whatever trait was generated (may be duplicate)
```

## Testing Checklist

- [ ] Click trait button → 3 unique traits appear
- [ ] Click any of the 3 cards → that trait is selected
- [ ] Selected trait appears in "Apply X" button
- [ ] Trait can be applied to tower successfully
- [ ] No duplicates appear (if 3+ traits available)
- [ ] Works with fewer than 3 traits (duplicates allowed)
- [ ] Legacy UI still works if new fields not assigned
- [ ] Cards are cleared when dialog closes
- [ ] Trait probabilities are respected
- [ ] Visual feedback on hover/click

## Console Debug Messages

**New messages:**
```
"Generated 3 trait options: RapidFire, Poison, Multishot"
"Showing trait selection dialog with 3 options"
"User selected trait: Poison"
"Using legacy single-card UI. For 3-card selection, assign traitCardsContainer and traitCardPrefab in inspector."
```

## Benefits

### Player Experience
✅ **More Choice** - Pick the trait that fits your strategy  
✅ **More Strategic** - Consider synergies with existing towers  
✅ **More Engaging** - Active decision-making vs passive acceptance  
✅ **More Replayability** - Different trait combinations each game

### Game Design
✅ **Reduces RNG frustration** - Bad trait? Pick a different one  
✅ **Increases build diversity** - More ways to build your defense  
✅ **Better difficulty curve** - Players optimize their choices  

## Future Enhancements

### Potential Improvements
1. **Rarity System** - Show trait rarity (Common, Rare, Legendary)
2. **Trait Preview** - Hover to see detailed stats/effects
3. **Reroll Option** - Spend gold to generate 3 new traits
4. **Trait Synergies** - Highlight traits that combo well together
5. **Animation** - Cards flip in or slide into view
6. **Sound Effects** - Card flip sound, selection sound

### Advanced Features
```csharp
// Example: Weighted generation based on current game state
if (PlayerIsLosing())
{
    // Increase chance of defensive traits
    BoostProbability(defensiveTraits, 1.5f);
}
```

## Related Files

**Modified:**
- `Assets/Scripts/Game/GameUI.cs` - Main implementation

**New Documentation:**
- `docs/FEATURE_3_TRAIT_SELECTION.md` - This file

**Related Systems:**
- Trait system (`TowerTrait.cs`, `TraitApplier.cs`)
- Tower selection (`TowerManager.cs`)
- UI system (`GameUI.cs`)

## Summary

**Old System:** 1 random trait → Accept or reject  
**New System:** 3 unique random traits → Choose your favorite  

**Result:** More player agency, strategic depth, and engaging gameplay! 🎴✨
