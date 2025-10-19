# Gold System Diagnostic - "Gold Doubling" Investigation

## Issue Report
User reports that gold "seems to double" when the last enemy is killed.

## Gold Sources in the Game

### 1. Enemy Kill Rewards
**When:** Each time an enemy dies
**Amount:** `enemyData.goldReward` (typically 10-50 gold per enemy)
**Code:** `Enemy.Die()` → `GameManager.AddGold(enemyData.goldReward)`

### 2. Wave Completion Bonus
**When:** Wave completes (all enemies dead or reached end)
**Amount:** `50 + (currentWave * 10)`
- Wave 1: 60 gold bonus
- Wave 2: 70 gold bonus
- Wave 3: 80 gold bonus
- etc.

**Code:** `WaveManager.CompleteWave()` → `GameManager.EndWave()` → `GameManager.AddGold(bonus)`

### 3. Tower Selling
**When:** Player sells a tower
**Amount:** Percentage of tower cost (typically 50-75%)
**Code:** `TowerManager.SellTower()` → `GameManager.AddGold(sellPrice)`

## Expected Behavior When Last Enemy Dies

```
1. Enemy.Die() is called
   └─ AddGold(enemyData.goldReward)  // e.g., +10 gold
   └─ Notify EnemyManager.OnEnemyKilled()

2. EnemyManager.UnregisterEnemy()
   └─ Remove from activeEnemies list
   └─ Check: if (activeEnemies.Count == 0)
      └─ Invoke OnAllEnemiesDefeated event

3. WaveManager.WaitForWaveCompletion() coroutine notices enemies == 0
   └─ Calls CompleteWave()
   └─ Calls GameManager.EndWave()
      └─ AddGold(50 + currentWave * 10)  // e.g., +60 gold for wave 1

TOTAL GOLD ADDED: enemyReward + waveBonus
Example: 10 + 60 = 70 gold
```

## Possible Causes of "Doubling"

### Theory 1: Wave Bonus Appears as Doubling
**Scenario:** 
- User kills last enemy worth 10 gold
- Wave bonus of 60 gold is added immediately after
- Total: 70 gold appears on screen
- **Perception:** "Gold doubled!" (but it's reward + bonus)

**Status:** MOST LIKELY - This is working as intended

---

### Theory 2: Die() Called Twice
**Scenario:** 
- Enemy.Die() somehow called multiple times
- Would award gold reward twice

**Protection:** 
```csharp
if (!isAlive)
    return;  // Guard prevents multiple calls
```

**Status:** UNLIKELY - Guard clause should prevent this

---

### Theory 3: Multiple Subscriptions
**Scenario:**
- Enemy's OnEnemyKilled event has multiple listeners
- Each listener adds gold

**Reality:**
- Only EnemyManager listens to OnEnemyKilled
- EnemyManager doesn't add gold, only unregisters enemy

**Status:** NOT THE ISSUE

---

### Theory 4: CompleteWave() Called Multiple Times
**Scenario:**
- Wave completion logic triggered multiple times
- Wave bonus added multiple times

**Protection:**
```csharp
if (gameState == GameState.WaveInProgress)
{
    gameState = GameState.Preparing;  // Changes state
    AddGold(bonus);  // Only if state was WaveInProgress
}
```

**Status:** UNLIKELY - State guard should prevent this

---

## Diagnostic Logging Added

I've added detailed logging to help diagnose the issue:

### In Enemy.Die()
```csharp
Debug.Log($"[Enemy] {name} died - awarded {goldReward} gold to player");
```

### In GameManager.AddGold()
```csharp
Debug.Log($"[GameManager] AddGold({amount}) - New total: {currentGold} gold");
```

### In GameManager.EndWave()
```csharp
Debug.Log($"[GameManager] Wave {currentWave} completed - awarding bonus of {bonus} gold");
```

## How to Test

### Step 1: Start Wave
1. Press Play
2. Start Wave 1
3. Note starting gold amount

### Step 2: Kill All Enemies
1. Kill enemies one by one
2. Watch Console for each kill:
   ```
   [Enemy] EnemyBasic(Clone) died - awarded 10 gold to player
   [GameManager] AddGold(10) - New total: 1010 gold
   ```

### Step 3: Watch Last Enemy
When last enemy dies, you should see:
```
[Enemy] EnemyBasic(Clone) died - awarded 10 gold to player
[GameManager] AddGold(10) - New total: 1020 gold
[GameManager] Wave 1 completed - awarding bonus of 60 gold
[GameManager] AddGold(60) - New total: 1080 gold
```

### Expected Result
- First AddGold: Enemy reward (e.g., 10)
- Second AddGold: Wave bonus (e.g., 60)
- Total increase: 70 gold

### Problem Indicators

❌ **If you see duplicate AddGold for same amount:**
```
[GameManager] AddGold(10) - New total: 1010 gold
[GameManager] AddGold(10) - New total: 1020 gold  // ← DUPLICATE!
```
→ Die() being called twice

❌ **If you see wave bonus multiple times:**
```
[GameManager] AddGold(60) - New total: 1080 gold
[GameManager] AddGold(60) - New total: 1140 gold  // ← DUPLICATE!
```
→ CompleteWave() being called twice

❌ **If AddGold amount doesn't match enemy reward or wave bonus:**
```
[GameManager] AddGold(127) - New total: ???  // ← WRONG AMOUNT!
```
→ Something else is adding gold

## What to Report Back

Please run the test above and share:

1. **Starting gold:** _____
2. **Gold before last enemy dies:** _____
3. **Gold after last enemy dies:** _____
4. **Total gold increase:** _____
5. **Console log output** (copy the AddGold messages)

This will help identify exactly what's happening.

## Likely Explanation

Based on the gold system design, the most likely explanation is:

**This is WORKING AS INTENDED!**

When the last enemy dies:
- You get the **enemy kill reward** (10-50 gold)
- You get the **wave completion bonus** (50-80 gold)

These happen very close together (within 0.5 seconds), which might create the perception that gold "doubled."

**Example:**
```
Before last enemy: 1000 gold
Last enemy dies: +10 gold (enemy reward)
Wave completes: +60 gold (wave bonus)
After: 1070 gold

Perception: "My gold jumped from 1000 to 1070 - it almost doubled!"
Reality: You got two separate rewards that added up
```

## If This Is The Issue

If the "doubling" is just the wave bonus being added, this is correct behavior but could be improved with better UI feedback:

### Potential Solutions

**1. Separate Visual Indicators**
- Show "+10 Gold" when enemy dies
- Show "+60 Wave Bonus!" separately
- Makes it clear these are two different rewards

**2. Delay Wave Bonus**
- Add 1-second delay before wave bonus
- Gives player time to see enemy reward first

**3. Wave Complete UI**
- Show "Wave Complete!" popup
- List all bonuses clearly
- Player understands why gold increased

**4. Combat Text**
- Floating "+10" appears above enemy
- Separate text for wave bonus
- Clear visual separation

## Summary

The gold system has **two independent sources** that both trigger when the last enemy dies:
1. Enemy kill reward (immediate)
2. Wave completion bonus (0-0.5 seconds later)

With the diagnostic logging added, you can now see exactly when and why gold is being added. Please run a test wave and check the Console to confirm whether this is:
- A) Working as intended (two separate bonuses)
- B) Actually a bug (duplicate gold awards)

Based on the logs, we can determine the next steps!
