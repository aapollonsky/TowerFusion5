# Tower Traits Quick Start Guide

## 🚀 Get Started in 3 Steps

### Step 1: Create the Traits
In Unity menu:
```
Tools > Tower Fusion > Create Default Traits
```
This creates 7 traits in `Assets/Data/Traits/`

### Step 2: Apply to Tower
**Method A - In Editor:**
1. Select tower GameObject
2. Find `TowerTraitManager` component
3. Drag trait asset into `Applied Traits` list

**Method B - At Runtime:**
```csharp
Tower tower = GetComponent<Tower>();
TowerTrait fireTrait = Resources.Load<TowerTrait>("Traits/Fire");
tower.AddTrait(fireTrait);
```

### Step 3: Test It!
Use demo tools:
```
Tools > Tower Fusion > Demo: Add Fire Trait to Selected Tower
Tools > Tower Fusion > Demo: Add Lightning Trait to Selected Tower
```

---

## 📋 Trait Cheat Sheet

| Trait | Icon | Effect | Best For |
|-------|------|--------|----------|
| 🔥 **Fire** | Red | +50% dmg, burn 10 DPS | Bosses |
| ❄️ **Ice** | Cyan | 30% slow, +25% vulnerability | CC & Support |
| ⚡ **Lightning** | Yellow | Chain to 2 enemies | Groups |
| 🎯 **Sniper** | Green | 2x range, +2s charge | Long range |
| 💰 **Harvest** | Yellow | +1 gold/kill | Economy |
| 💥 **Explosion** | Orange | 75% AoE damage, 2u radius | Packed waves |
| 🌍 **Earth** | Brown | Death = hole (3s), enemies fall in | Choke points |

---

## 💡 Power Combos

### "Thermal Shock" (Ice + Fire)
```
Ice slows & weakens (+25% dmg) → Fire deals massive damage + burn
Result: One-shot potential on most enemies
```

### "Storm Farmer" (Lightning + Harvest)
```
Lightning hits 3 enemies → Harvest gives +3 gold total
Result: Triple economy gain per attack
```

### "Inferno Zone" (Explosion + Fire)
```
Explosion hits multiple enemies → Fire burns all of them
Result: Massive AoE DoT damage
```

### "Permafrost" (Earth + Ice)
```
Earth creates holes on kills → Ice slows enemies into holes
Result: Lethal zones that delete entire enemy groups
```

### "Gold Rush" (Explosion + Harvest)
```
Explosion kills multiple enemies → Harvest multiplies gold gain
Result: Maximum economy efficiency
```

### "Crater Field" (Earth + Explosion)
```
Explosion kills multiple at once → Each creates a hole
Multiple holes = deadly minefield
Result: Self-perpetuating kill zones
```

---

## 🎮 Quick Tips

### When to Use Each Trait:

**Early Game (Waves 1-5):**
- 💰 Harvest - Build your economy
- ❄️ Ice - Slow first waves

**Mid Game (Waves 6-10):**
- 💥 Explosion - Handle larger groups
- ⚡ Lightning - Efficient multi-target
- 🌍 Earth - Create instant-kill holes

**Late Game (Waves 11+):**
- 🔥 Fire - Maximum single-target damage
- 🎯 Sniper - Hit backline threats
- Combos - Stack multiple traits

### Tower Placement Strategy:

**Choke Points:**
- 🌍 Earth (instant kill holes)
- 💥 Explosion (grouped enemies)

**Path Corners:**
- ❄️ Ice (slow for other towers)
- 🌍 Earth (enemies fall into holes)

**Path Entrance:**
- 💰 Harvest (hit everything early)
- ⚡ Lightning (weaken groups)

**Backline:**
- 🎯 Sniper (extended range)
- 🔥 Fire (high-value targets)

---

## 🔧 Testing Your Setup

### In-Game Verification:

1. **Visual Check:**
   - Tower should show colored overlay
   - Trait badge appears near tower
   - Effects visible on enemies

2. **Damage Check:**
   - Watch enemy health bars
   - Check for DoT ticks (Fire, Earth)
   - Verify multi-hits (Lightning, Explosion)

3. **Economy Check:**
   - Kill counter should increase
   - Gold should increase by trait bonus
   - Check resource panel

### Debug Commands:
```csharp
// Check applied traits
var traits = tower.GetAppliedTraits();
Debug.Log($"Tower has {traits.Count} traits");

// Check modified stats
var stats = tower.GetCurrentStats();
Debug.Log($"Range: {stats.range}, Damage: {stats.damage}");

// Check trait manager
var manager = tower.GetComponent<TowerTraitManager>();
Debug.Log($"Trait count: {manager.TraitCount}");
```

---

## 📊 Trait Math Examples

### Fire Trait:
```
Base Damage: 100
Fire Bonus: +50% = 150 direct damage
Burn DoT: 10 DPS × 3s = 30 damage
Total: 180 damage per attack
```

### Explosion Trait:
```
Base Damage: 100
Direct Hit: 100 damage to 1 enemy
Explosion: 75 damage to 3 enemies
Total: 325 damage per attack (if 3+ enemies grouped)
```

### Lightning Trait:
```
Base Damage: 100
Primary Target: 100 damage
Chain Target 1: 100 damage (100% multiplier)
Chain Target 2: 100 damage
Total: 300 damage per attack (if 2+ enemies in range)
```

### Ice + Fire Combo:
```
Base Damage: 100
Ice Brittle: +25% incoming damage
Fire Bonus: +50% = 150 base
With Brittle: 150 × 1.25 = 187.5 direct
Burn DoT: 10 DPS × 3s = 30 damage
Total: 217.5 damage per attack
```

---

## ⚠️ Common Mistakes

### ❌ Don't:
- Stack too many traits without testing
- Use Sniper on fast-attack towers (wasted charge time)
- Place Explosion tower where enemies spread out
- Forget to test trait combinations before wave starts

### ✅ Do:
- Start with 1-2 traits per tower
- Match traits to tower placement
- Combine complementary traits
- Use visual indicators to verify traits applied

---

## 🎯 Recommended Loadouts

### "Starter Defender"
```
1 Tower: Ice (slow)
1 Tower: Harvest (economy)
Total Cost: Low, safe start
```

### "Mid-Game Powerhouse"
```
2 Towers: Explosion + Fire (AoE damage)
1 Tower: Ice (support)
1 Tower: Harvest (economy)
Total: Balanced offense & economy
```

### "Late-Game Fortress"
```
3 Towers: Fire + Ice (thermal shock)
2 Towers: Lightning (chain damage)
2 Towers: Explosion + Harvest (AoE + gold)
1 Tower: Earth (trap zones)
Total: Maximum damage output
```

---

## 📝 Quick Reference Commands

### Create Traits:
```
Tools > Tower Fusion > Create Default Traits
```

### Add Trait (Demo):
```
Tools > Tower Fusion > Demo: Add Fire Trait
Tools > Tower Fusion > Demo: Add Lightning Trait
```

### Test Effects:
```
Tools > Tower Fusion > Test Fire Trait on Selected Enemy
Tools > Tower Fusion > Test Ice Trait via Tower Attack
TowerFusion > Debug > Lightning Trait Tester
```

---

## 🚀 Next Steps

1. **Create your first trait combo** - Try Ice + Fire
2. **Test in sandbox mode** - Verify effects work
3. **Optimize tower placement** - Match traits to locations
4. **Experiment with custom traits** - Create your own!

For detailed information, see **TOWER_TRAITS.md**

Happy defending! 🎮
