# BonusWheel - Quarter Mapping Guide

## Your Wheel Layout

Based on your wheel image with the **red pointer at the TOP**:

```
           🔺 POINTER (TOP)
           goldRewards[0]
                |
    [3] --------+-------- [1]
   LEFT         |        RIGHT
                |
           goldRewards[2]
            (BOTTOM)
```

## How to Set Up Your Rewards Array

Looking at your wheel with values **1, 20, 10, 5**:

### Visual Layout (from your image):
```
           🔺 (TOP)
            20g
             |
    20g -----+-----  5g
   (LEFT)    |    (RIGHT)
             |
            10g
          (BOTTOM)
```

### Inspector Setup:
```csharp
goldRewards[0] = 20;  // TOP quarter (where pointer points at 0°)
goldRewards[1] = 5;   // RIGHT quarter  
goldRewards[2] = 10;  // BOTTOM quarter
goldRewards[3] = 20;  // LEFT quarter
```

### In Unity Inspector:
1. Select your BonusWheel GameObject
2. Find "Gold Rewards" array
3. Set Size to 4
4. Enter values:
   - Element 0: **20** (Top)
   - Element 1: **5** (Right)
   - Element 2: **10** (Bottom)
   - Element 3: **20** (Left)

## How It Works

### Wheel Rotation:
- **0°** = Top quarter aligned with pointer
- **90°** = Right quarter (wheel rotated 90° clockwise)
- **180°** = Bottom quarter (wheel rotated 180°)
- **270°** = Left quarter (wheel rotated 270° clockwise)

### When Spin() is Called:
1. Randomly picks a target quarter (0-3)
2. Calculates rotation to align that quarter with the top pointer
3. Spins 3-5 full rotations + final position
4. Grants the reward from `goldRewards[targetQuarter]`

## Visual Quarter Boundaries

```
       315°|45°
          🔺
    ------+----- 
    |  0  |  1  |
270°|     |     |90°
    ------+-----
    |  3  |  2  |
    ------+-----
       225°|135°
          180°
```

- **Quarter 0** (Top): 315° to 45° → goldRewards[0]
- **Quarter 1** (Right): 45° to 135° → goldRewards[1]  
- **Quarter 2** (Bottom): 135° to 225° → goldRewards[2]
- **Quarter 3** (Left): 225° to 315° → goldRewards[3]

## Testing Your Setup

Add this debug code to verify correct mapping:

```csharp
void Start()
{
    Debug.Log($"Top (Q0): {goldRewards[0]} gold");
    Debug.Log($"Right (Q1): {goldRewards[1]} gold");
    Debug.Log($"Bottom (Q2): {goldRewards[2]} gold");
    Debug.Log($"Left (Q3): {goldRewards[3]} gold");
}
```

When the wheel spins, check the console log:
```
BonusWheel: Target=1, FinalAngle=270.0°, Landed=1, Reward=5
```
This means it landed on Quarter 1 (Right) and gave 5 gold ✓

## Common Setup Mistakes

❌ **Wrong**: Placing rewards in visual reading order (clockwise)
```csharp
// Don't do this!
goldRewards = { 20, 5, 10, 20 }; // Reading clockwise from top-right
```

✅ **Correct**: Array index matches quarter position from pointer
```csharp
// Do this!
goldRewards[0] = 20;  // TOP (where pointer is)
goldRewards[1] = 5;   // RIGHT (90° clockwise from pointer)
goldRewards[2] = 10;  // BOTTOM (180° from pointer)
goldRewards[3] = 20;  // LEFT (270° clockwise from pointer)
```

## Your Exact Configuration

For your wheel image (1, 20, 10, 5), set it up as:

```
Gold Rewards:
  Element 0: 20  ← Top quarter (under the pointer)
  Element 1: 5   ← Right quarter  
  Element 2: 10  ← Bottom quarter
  Element 3: 20  ← Left quarter (wait, I see "1" in your image?)

Reward Names:
  Element 0: "20 Gold"
  Element 1: "5 Gold"
  Element 2: "10 Gold"
  Element 3: "20 Gold"
```

**Note**: Looking at your image more carefully, I see the left quarter might be "1" not "20". If so:
- Element 3 should be **1** (not 20)

The wheel will now correctly award the reward that aligns with the top pointer! 🎯

