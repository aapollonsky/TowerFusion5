# Tower Destruction → Stealer Conversion - Quick Reference

## What Changed

When a tower is destroyed, enemies attacking it will **convert to corn stealers** instead of seeking another tower.

## Why This Matters

### Strategic Impact
- 🏰 **Tower destruction has consequences** - Creates more corn stealers
- 🌽 **Defend both towers AND corn** - Can't ignore either
- 🎯 **Risk/reward decisions** - Let weak tower fall or protect it?

## How It Works

```
Enemy attacks tower
        ↓
Tower destroyed
        ↓
Enemy converts to stealer
        ↓
    ┌───┴───┐
    │       │
Corn    No corn
exists  available
    │       │
    ↓       ↓
Steal   Move to
corn    end point
```

## Example Wave

**Wave Start:**
- 20 enemies spawn
- 17 Attackers (85%)
- 3 Stealers (15%)

**Tower 1 Destroyed (3 attackers on it):**
- 14 Attackers remaining
- 6 Stealers (3 original + 3 converted)

**Tower 2 Destroyed (2 attackers on it):**
- 12 Attackers remaining
- 8 Stealers (6 + 2 converted)

**Result:** 40% of enemies now stealing corn (up from 15%)

## What You'll See

### In Console
```
EnemyBasic(Clone) attacked Tower for 10 damage!
EnemyBasic(Clone) converted to STEALER after tower destruction!
EnemyBasic(Clone) reached corn storage, starting grab
```

### In Game
1. Enemy attacks tower until destroyed
2. Enemy turns and heads to corn storage
3. Enemy grabs corn (yellow sphere appears)
4. Enemy runs back to spawn

## Edge Cases

| Situation | Behavior |
|-----------|----------|
| Corn available | Convert to stealer |
| No corn left | Move to end point |
| CornManager disabled | Move to end point |
| Already a stealer | No change |
| Multiple enemies on tower | All convert |

## Testing Steps

1. ✅ Place tower near enemies
2. ✅ Let enemies attack it
3. ✅ Let them destroy tower (don't repair)
4. ✅ Watch enemies redirect to corn
5. ✅ Console shows "converted to STEALER"

## Strategic Tips

### For Defense
- **Protect critical towers** - Destruction creates stealers
- **Use disposable towers carefully** - They'll create corn threats
- **Watch corn when towers fall** - Expect stealer surge
- **Position defenses** - Cover both towers and corn path

### For Offense (as game designer)
- **Weak towers** - Easy targets, create stealer pressure
- **Strong towers** - Enemies stuck attacking, less corn threat
- **Tower placement** - Near corn = quick stealer conversion

## Code Changes

**New Method:**
```csharp
ConvertToStealer()
```

**Modified Methods:**
- `AttackTower()` - Calls ConvertToStealer on destruction
- `HandleTowerBehavior()` - Calls ConvertToStealer when tower invalid  
- `UpdateTowerTargeting()` - Calls ConvertToStealer instead of seeking new tower

## Quick Stats

| Metric | Value |
|--------|-------|
| Initial Stealers | 15% of wave |
| Per Tower Destroyed | +X stealers (X = enemies attacking it) |
| Max Enemies Per Tower | 3 (distributor limit) |
| Conversion Time | Instant |
| Fallback Behavior | Move to end if no corn |

---

**TL;DR:** Tower falls → Attackers become stealers → More corn theft pressure 🌽⚔️
