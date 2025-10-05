# Trait System Implementation Guide

## Overview

The trait system allows players to click a "trait" button once per wave to receive a random trait card. The trait becomes available for assignment to towers after accepting it via the dialog.

## Components Added

### GameUI Additions

**New Serialized Fields:**
- `traitButton` - Button that shows trait card dialog
- `traitCardDialog` - GameObject containing the trait selection UI
- `traitNameText` - Displays the trait name
- `traitDescriptionText` - Displays the trait description  
- `traitIconImage` - Displays the trait icon
- `traitDoneButton` - Button to accept the trait
- `availableTraits[5]` - Array of 5 TowerTrait assets
- `traitProbabilities[5]` - Probability weights for each trait (default: 0.2 each)

**New Methods:**
- `ShowTraitCard()` - Generates and displays random trait
- `AcceptTrait()` - Accepts trait and closes dialog
- `GenerateRandomTrait()` - Weighted random trait selection

## Setup Instructions

### 1. Create Trait Assets
Run `Tools > Tower Fusion > Setup Trait System` to create the 5 default traits:
- Fire.asset
- Ice.asset  
- Lightning.asset
- Sniper.asset
- Harvest.asset

### 2. Create UI Elements
In your scene, create the following UI structure:

```
Canvas
├── GameUI
│   ├── TraitButton (Button)
│   └── TraitCardDialog (Panel)
│       ├── TraitNameText (TextMeshPro)
│       ├── TraitDescriptionText (TextMeshPro)  
│       ├── TraitIconImage (Image)
│       └── DoneButton (Button)
```

### 3. Configure GameUI Component
1. Assign the trait button to `traitButton` field
2. Assign the dialog panel to `traitCardDialog` field  
3. Assign the text components to their respective fields
4. Assign the 5 trait assets to `availableTraits` array
5. Verify `traitProbabilities` shows [0.2, 0.2, 0.2, 0.2, 0.2]

## Behavior

### Button State
- **Available**: When game state is "Preparing" and button hasn't been used this wave
- **Disabled**: When wave is active or button was already used this wave
- **Reset**: Button becomes available again when new wave preparation starts

### Trait Selection  
- Uses weighted probability selection based on `traitProbabilities` array
- Each trait has 0.2 (20%) chance by default
- Probabilities can be adjusted per trait in inspector

### Dialog Flow
1. Player clicks "trait" button
2. Random trait is generated and displayed in dialog
3. Player clicks "done" to accept the trait  
4. Dialog closes and trait becomes available for tower assignment
5. Button is disabled for remainder of wave

## Testing

### Probability Testing
Run `Tools > Tower Fusion > Test Trait Probabilities` to verify random distribution over 1000 trials.

### Manual Testing
1. Enter Play mode
2. Ensure game state is "Preparing" 
3. Click trait button - dialog should appear
4. Click "done" - dialog should close
5. Try clicking trait button again - should be disabled
6. Start wave, then prepare next wave - button should be enabled again

## Future Enhancements

The current implementation handles trait generation and UI. Next steps:
1. Integrate with tower selection system for trait assignment
2. Add visual feedback when trait is available for assignment
3. Add trait inventory/queue system for multiple traits
4. Add trait rarity system with different probabilities

## Configuration

### Adjusting Probabilities
In GameUI inspector, modify the `Trait Probabilities` array:
- Values should sum to 1.0 for 100% coverage
- Individual values can be 0 to disable a trait
- Example for rare traits: [0.4, 0.3, 0.2, 0.08, 0.02]

### Adding New Traits  
1. Create new TowerTrait ScriptableObject asset
2. Increase `availableTraits` array size  
3. Add corresponding probability value
4. Assign new trait asset to array slot