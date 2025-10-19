# How to Change Wave Completion Bonus

## Quick Answer

**In Unity Inspector:**
1. Select `GameManager` in Hierarchy
2. Look for **"Wave Completion Rewards"** section
3. Adjust these values:
   - **Wave Completion Base Bonus:** Base gold for any wave (default: 50)
   - **Wave Completion Per Wave Bonus:** Additional gold per wave level (default: 10)

## Bonus Formula

```
Total Bonus = Base Bonus + (Wave Number × Per Wave Bonus)
```

### Default Values (Current)
- Base Bonus: **50 gold**
- Per Wave Bonus: **10 gold**

### Examples with Default Values

| Wave | Calculation | Total Bonus |
|------|-------------|-------------|
| Wave 1 | 50 + (1 × 10) | **60 gold** |
| Wave 2 | 50 + (2 × 10) | **70 gold** |
| Wave 3 | 50 + (3 × 10) | **80 gold** |
| Wave 5 | 50 + (5 × 10) | **100 gold** |
| Wave 10 | 50 + (10 × 10) | **150 gold** |

## Customization Examples

### Example 1: Higher Base, Lower Scaling
**Settings:**
- Wave Completion Base Bonus: `100`
- Wave Completion Per Wave Bonus: `5`

**Results:**
- Wave 1: 100 + (1 × 5) = **105 gold**
- Wave 5: 100 + (5 × 5) = **125 gold**
- Wave 10: 100 + (10 × 5) = **150 gold**

**Effect:** More consistent rewards across waves

---

### Example 2: Aggressive Scaling
**Settings:**
- Wave Completion Base Bonus: `25`
- Wave Completion Per Wave Bonus: `25`

**Results:**
- Wave 1: 25 + (1 × 25) = **50 gold**
- Wave 5: 25 + (5 × 25) = **150 gold**
- Wave 10: 25 + (10 × 25) = **275 gold**

**Effect:** Later waves much more rewarding

---

### Example 3: Flat Bonus (No Scaling)
**Settings:**
- Wave Completion Base Bonus: `75`
- Wave Completion Per Wave Bonus: `0`

**Results:**
- Wave 1: 75 + (1 × 0) = **75 gold**
- Wave 5: 75 + (5 × 0) = **75 gold**
- Wave 10: 75 + (10 × 0) = **75 gold**

**Effect:** Every wave gives same bonus

---

### Example 4: No Base, High Scaling
**Settings:**
- Wave Completion Base Bonus: `0`
- Wave Completion Per Wave Bonus: `50`

**Results:**
- Wave 1: 0 + (1 × 50) = **50 gold**
- Wave 5: 0 + (5 × 50) = **250 gold**
- Wave 10: 0 + (10 × 50) = **500 gold**

**Effect:** Huge rewards for surviving later waves

---

### Example 5: Generous Economy
**Settings:**
- Wave Completion Base Bonus: `200`
- Wave Completion Per Wave Bonus: `50`

**Results:**
- Wave 1: 200 + (1 × 50) = **250 gold**
- Wave 5: 200 + (5 × 50) = **450 gold**
- Wave 10: 200 + (10 × 50) = **700 gold**

**Effect:** Player gets lots of gold, can build more towers

---

## Step-by-Step Instructions

### In Unity Editor

1. **Open your scene** (MainScene.unity or your test scene)

2. **Find GameManager:**
   - Hierarchy window
   - Look for "GameManager" GameObject

3. **Select GameManager:**
   - Click on it to open Inspector

4. **Find Wave Completion Rewards section:**
   ```
   ┌─────────────────────────────────────┐
   │ Game Manager (Script)               │
   │                                     │
   │ ▼ Wave Completion Rewards           │
   │   Wave Completion Base Bonus: 50    │ ← Change this
   │   Wave Completion Per Wave Bonus: 10│ ← And this
   │                                     │
   └─────────────────────────────────────┘
   ```

5. **Adjust values:**
   - Click in the number field
   - Type new value
   - Press Enter

6. **Save scene:**
   - File → Save (Ctrl+S / Cmd+S)

7. **Test it:**
   - Press Play
   - Complete a wave
   - Check Console for bonus amount
   - Verify gold awarded matches your formula

---

## Balancing Guidelines

### Consider These Factors

**1. Enemy Kill Rewards**
- If enemies give lots of gold → Lower wave bonus
- If enemies give little gold → Higher wave bonus

**2. Tower Costs**
- Expensive towers → Need higher bonuses
- Cheap towers → Lower bonuses work fine

**3. Game Difficulty**
- Hard game → Higher bonuses (player needs resources)
- Easy game → Lower bonuses (avoid too much gold)

**4. Wave Count**
- Many waves (20+) → Lower per-wave scaling
- Few waves (5-10) → Higher per-wave scaling

**5. Corn Theft Pressure**
- High corn theft → More gold (need to rebuild after tower loss)
- Low corn theft → Standard gold economy

### Recommended Presets

**Easy Mode:**
```
Base: 100
Per Wave: 20
Wave 1: 120 gold
Wave 10: 300 gold
```

**Normal Mode (Current Default):**
```
Base: 50
Per Wave: 10
Wave 1: 60 gold
Wave 10: 150 gold
```

**Hard Mode:**
```
Base: 25
Per Wave: 5
Wave 1: 30 gold
Wave 10: 75 gold
```

**Survival Mode:**
```
Base: 0
Per Wave: 15
Wave 1: 15 gold
Wave 10: 150 gold (rewards surviving long)
```

---

## Testing Your Changes

### Quick Test

1. Set values in Inspector
2. Press Play
3. Start Wave 1
4. Complete the wave (kill all enemies)
5. Check Console:
   ```
   [GameManager] Wave 1 completed - awarding bonus of 60 gold
   ```
6. Verify the amount matches your formula

### Full Test

Create a test with multiple waves:
1. Complete Wave 1 → Note bonus
2. Complete Wave 2 → Note bonus (should be higher)
3. Complete Wave 5 → Note bonus (should scale up)
4. Verify formula is working correctly

---

## Advanced: Different Bonuses for Different Waves

If you want **specific** bonuses for certain waves (not a formula), you could:

### Option 1: Array-Based Bonuses
```csharp
// In GameManager.cs, replace with:
public int[] waveCompletionBonuses = new int[] { 
    60,   // Wave 1
    70,   // Wave 2
    100,  // Wave 3 (extra bonus!)
    80,   // Wave 4
    150   // Wave 5 (big bonus!)
};

// In EndWave():
int bonus = (currentWave >= 1 && currentWave <= waveCompletionBonuses.Length) 
    ? waveCompletionBonuses[currentWave - 1] 
    : waveCompletionBaseBonus;
```

### Option 2: Milestone Bonuses
```csharp
// In EndWave():
int bonus = waveCompletionBaseBonus + (currentWave * waveCompletionPerWaveBonus);

// Add milestone bonuses
if (currentWave % 5 == 0)  // Every 5th wave
    bonus += 100;  // Bonus milestone reward!
```

---

## Current Default Values

As of October 18, 2025, the default values are:
- **Base Bonus:** 50 gold
- **Per Wave Bonus:** 10 gold

These provide a balanced economy for standard tower defense gameplay with corn theft mechanics.

---

## Summary

**Quick Steps:**
1. Select GameManager in Hierarchy
2. Inspector → Wave Completion Rewards
3. Change the two numbers
4. Save scene
5. Test!

**Formula:**
```
Bonus = Base + (Wave × PerWave)
```

**Example:**
- Base: 50, PerWave: 10
- Wave 1 bonus: 60 gold
- Wave 5 bonus: 100 gold

Adjust to match your game's economy! 💰✨
