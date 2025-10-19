# How to See Corn Count

## Overview
There are **three ways** to see how much corn is left in the pile:

## 1. In-Game Floating Text (Game View) 🎮

### What You'll See
- **Floating text** above the corn pile
- Shows: `🌽 20` (corn emoji + number)
- **Always faces camera** (billboard effect)
- **Color-coded** based on amount:
  - ✅ **White** - Normal (>10 corn)
  - ⚠️ **Yellow** - Warning (6-10 corn)
  - 🚨 **Red** - Critical (≤5 corn)

### Location
- Positioned **2 units above** the corn pile (configurable)
- Visible in **Game view** during play
- Follows the corn storage if it moves

### Customization (Inspector)
Select `CornStorage` in Hierarchy:
```
UI Display
├─ Show Corn Count: ✓ (enable/disable)
├─ Text Offset: (0, 2, 0) (position above pile)
├─ Text Size: 3 (font size)
└─ Text Color: White (normal color)
```

---

## 2. Scene View Label (Editor Only) 🛠️

### What You'll See
- **Editor gizmo label** in Scene view
- Shows: `Corn Storage` + `20/20` (current/max)
- Only visible in **Scene view** (not Game view)
- Only visible in **Unity Editor** (not builds)

### Location
- 2 units above corn storage
- Visible when CornStorage is in view
- Shows in both Edit and Play modes

### Usage
- Great for **level design**
- See corn count while placing objects
- Verify initial settings

---

## 3. Console Logs 📝

### What You'll See
```
[CornStorage] Corn taken by EnemyBasic(Clone). Remaining: 19
[CornManager] Corn grabbed by stealer. Remaining: 19
[CornManager] Corn successfully stolen by EnemyBasic(Clone)! Remaining: 18
```

### When Logs Appear
- When enemy **grabs corn** from storage
- When enemy **successfully steals** corn (reaches spawn)
- When corn is **returned** to storage (enemy killed)

### How to View
1. Open **Console window** (Window → General → Console)
2. Press **Play**
3. Watch logs as corn is grabbed/stolen

---

## Visual Comparison

### In-Game View (What Players See)
```
       🌽 15
        ↑
    [Floating text - always visible]
        ↑
     ╱     ╲
    │ Corn  │  ← Yellow cylinder (shrinks as corn taken)
     ╲     ╱
    ─────────
```

### Scene View (What Developers See)
```
  Corn Storage
      15/20          ← Editor label
        ↑
       🌽 15         ← Floating text (if in Play mode)
        ↑
     ╱     ╲
    │ Corn  │
     ╲     ╱
    ─────────
      (○)            ← Yellow wire sphere (grab radius)
```

---

## Detailed Feature Breakdown

### Floating Text Features

**Auto-Created:**
- Created in `Start()` if `showCornCount = true`
- Child of CornStorage GameObject
- Uses TextMeshPro for crisp rendering

**Always Visible:**
- Rendered in 3D world space
- Billboard component makes it face camera
- Visible from any angle

**Color Coding:**
```csharp
if (currentCornCount <= 5)
    color = Red;     // 🚨 CRITICAL - almost gone!
else if (currentCornCount <= 10)
    color = Yellow;  // ⚠️ WARNING - getting low
else
    color = White;   // ✅ NORMAL - plenty left
```

**Updates Automatically:**
- Changes when corn grabbed
- Changes when corn returned (enemy killed)
- Instant update (no delay)

---

## Configuration Options

### Enable/Disable Text Display

**In Inspector:**
1. Select `CornStorage` in Hierarchy
2. UI Display section
3. Uncheck "Show Corn Count"

**In Code:**
```csharp
cornStorage.showCornCount = false;
```

### Adjust Text Position

**Higher Above Pile:**
```
Text Offset: (0, 5, 0)  // 5 units up
```

**To the Side:**
```
Text Offset: (2, 2, 0)  // 2 units right, 2 units up
```

### Adjust Text Size

**Larger Text:**
```
Text Size: 5
```

**Smaller Text:**
```
Text Size: 1.5
```

### Change Text Color

**Inspector:**
- Click color picker next to "Text Color"
- Choose your preferred color
- This is the "normal" color (overridden at low corn)

---

## Technical Details

### Components Used

**Billboard.cs (NEW)**
```csharp
// Makes text always face camera
public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
```

**TextMeshPro**
- Unity's advanced text rendering
- Better quality than legacy Text component
- Supports rich formatting

### Performance

**Very Lightweight:**
- 1 TextMeshPro component per storage
- Updates only when corn count changes
- Billboard rotation in LateUpdate (minimal cost)

**No Impact:**
- No per-frame text updates
- No string allocations per frame
- No raycasts or complex calculations

---

## Troubleshooting

### "I don't see the floating text"

**Check 1: Is it enabled?**
- Select CornStorage → Inspector
- UI Display → Show Corn Count should be ✓

**Check 2: Is it off-screen?**
- Text might be above camera view
- Try adjusting Text Offset (reduce Y value)

**Check 3: Is TextMeshPro imported?**
- Window → TextMeshPro → Import TMP Essential Resources
- Unity should prompt on first use

**Check 4: Camera angle**
- Text uses billboard, should be visible
- Try different camera angles

### "Text is too small/large"

**Adjust Text Size:**
- Select CornStorage → Inspector
- UI Display → Text Size
- Try values between 1 and 5

### "Text is wrong position"

**Adjust Text Offset:**
- Select CornStorage → Inspector  
- UI Display → Text Offset
- Increase Y for higher, decrease for lower

### "Text shows wrong number"

**Check Console:**
- Should see corn count updates
- Verify corn is actually changing

**Force Update:**
```csharp
// In code, force visual update
cornStorage.TakeCorn(enemy);  // Should auto-update
```

### "Corn emoji doesn't show"

**Font Issue:**
- Some fonts don't support emoji
- Text will show as `? 15` or just `15`
- Still readable, just less fancy

**Fix:**
```csharp
// In UpdateVisuals(), change to:
cornCountText.text = $"Corn: {currentCornCount}";
// Or just:
cornCountText.text = $"{currentCornCount}";
```

---

## Alternative Display Methods

If you want different ways to show corn count:

### 1. UI Panel (Top of Screen)

Create UI Canvas with text:
```csharp
// Subscribe to CornStorage events
cornStorage.OnCornTaken += (count) => {
    uiText.text = $"Corn: {count}";
};
```

### 2. Progress Bar

Visual bar showing corn remaining:
```csharp
float percent = (float)cornCount / initialCount;
progressBar.fillAmount = percent;
```

### 3. Individual Corn Objects

Instead of scaled pile, show actual corn:
```csharp
// Instantiate corn prefabs (1 per corn)
// Destroy when taken
// More visual but more performance cost
```

### 4. Minimap Indicator

Show corn status on minimap:
```csharp
// Color minimap icon based on corn amount
if (cornCount < 5)
    minimapIcon.color = Color.red;
```

---

## Best Practices

### For Visibility
✅ Place text **2-5 units** above pile  
✅ Use **size 3-4** for readability  
✅ Keep **color-coding** for quick assessment  
✅ Use **white** as default (high contrast)

### For Performance
✅ One text per corn storage (not per corn)  
✅ Update only on **events**, not per frame  
✅ Use **TextMeshPro** (better than UI Text)  
✅ Billboard in **LateUpdate** (after camera moves)

### For UX
✅ Show **number only** or with icon  
✅ **Color-code** critical states (red = danger)  
✅ Position **above** pile (not blocking view)  
✅ Make it **always visible** (billboard)

---

## Quick Reference

| Method | Visibility | When | Where | Customizable |
|--------|-----------|------|-------|--------------|
| **Floating Text** | Always (Game) | Play mode | Above pile | ✅ Yes |
| **Scene Label** | Editor only | Always | Scene view | ❌ No |
| **Console Logs** | When events occur | Play mode | Console | ❌ No |

---

## Summary

**Easiest Way:** Look for floating **`🌽 20`** text above corn pile in Game view

**For Debugging:** Check Console logs for detailed corn events

**For Level Design:** Use Scene view gizmo labels

All three methods work together to give you complete visibility into corn status! 🌽✨
