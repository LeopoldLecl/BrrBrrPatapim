using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Script;

public class BonusWheel : MonoBehaviour
{
    [Header("Wheel Settings")]
    [SerializeField] private Transform wheelTransform;
    [SerializeField] private float spinDuration = 3f;
    [SerializeField] private int minSpins = 3; // Minimum full rotations
    [SerializeField] private int maxSpins = 5; // Maximum full rotations
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Rewards (4 Quarters)")]
    [Tooltip("Rewards for each quarter. [0]=Top, [1]=Right, [2]=Bottom, [3]=Left (pointer at top)")]
    [SerializeField] private int[] goldRewards = { 50, 100, 150, 200 };
    [SerializeField] private string[] rewardNames = { "50 Gold", "100 Gold", "150 Gold", "200 Gold" };
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spinSound;
    [SerializeField] private AudioClip winSound;
    
    [Header("Events")]
    public Action<int, string> OnRewardGranted; // goldAmount, rewardName
    public UnityEvent OnSpinComplete; // Called when spin animation completes
    
    private bool isSpinning;
    private int currentQuarter; // 0-3 for the 4 quarters
    
    void Start()
    {
        // If wheelTransform is not set, use this transform
        if (wheelTransform == null)
            wheelTransform = transform;
        
        // Validate reward arrays
        if (goldRewards.Length != 4)
        {
            Debug.LogWarning("BonusWheel: goldRewards array should have exactly 4 elements. Resizing...");
            Array.Resize(ref goldRewards, 4);
        }
        
        if (rewardNames.Length != 4)
        {
            Debug.LogWarning("BonusWheel: rewardNames array should have exactly 4 elements. Resizing...");
            Array.Resize(ref rewardNames, 4);
        }
    }
    
    /// <summary>
    /// Spins the wheel and grants a reward when complete
    /// </summary>
    public void Spin()
    {
        if (isSpinning)
        {
            Debug.LogWarning("BonusWheel: Already spinning!");
            return;
        }
        
        StartCoroutine(SpinCoroutine());
    }
    
    private IEnumerator SpinCoroutine()
    {
        isSpinning = true;
        
        // Play spin sound
        if (audioSource != null && spinSound != null)
            audioSource.PlayOneShot(spinSound);
        
        // Determine random result (0-3 for 4 quarters)
        int targetQuarter = UnityEngine.Random.Range(0, 4);
        
        // Calculate target rotation
        // Each quarter is 90 degrees (360/4)
        float quarterAngle = 90f;
        
        // Random number of full spins
        int fullSpins = UnityEngine.Random.Range(minSpins, maxSpins + 1);
        
        // Calculate the target angle within 0-360 range
        // Pointer is at TOP (0°/360°), wheel rotates clockwise
        // Quarter 0 (goldRewards[0]): 315° - 45° (top, centered at 0°)
        // Quarter 1 (goldRewards[1]): 45° - 135° (right, centered at 90°)
        // Quarter 2 (goldRewards[2]): 135° - 225° (bottom, centered at 180°)
        // Quarter 3 (goldRewards[3]): 225° - 315° (left, centered at 270°)
        
        // Calculate center angle for target quarter
        // We need to rotate so the target quarter's center aligns with the top pointer
        // This means we rotate TO that quarter's opposite position
        // If we want quarter 0 (at top) to win, wheel should be at 0°
        // If we want quarter 1 (at right) to win, wheel rotates so that section moves to top (rotate -90° or +270°)
        // If we want quarter 2 (at bottom) to win, rotate -180° or +180°
        // If we want quarter 3 (at left) to win, rotate -270° or +90°
        
        // Since wheel rotates clockwise (positive direction in Unity Z-axis)
        // To move a quarter TO the top pointer, we rotate by: (4 - targetQuarter) * 90
        // But for visual appeal, we want to spin in positive direction
        // So we calculate: 360 - (targetQuarter * 90)
        float targetAngleNormalized = (360f - (targetQuarter * quarterAngle)) % 360f;
        
        // Calculate total rotation (full spins + target angle)
        float totalRotation = fullSpins * 360f + targetAngleNormalized;
        
        // Spin animation (using absolute rotation values)
        float elapsed = 0f;
        
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinDuration;
            
            // Apply animation curve for smooth easing
            float curveValue = spinCurve.Evaluate(t);
            
            // Calculate current rotation from 0 to total
            float currentRotation = Mathf.Lerp(0, totalRotation, curveValue);
            
            // Apply rotation
            wheelTransform.rotation = Quaternion.Euler(0, 0, currentRotation);
            
            yield return null;
        }
        
        // Ensure final rotation is exact and normalized
        wheelTransform.rotation = Quaternion.Euler(0, 0, targetAngleNormalized);
        
        // Calculate which quarter we actually landed on based on final angle
        float finalAngle = targetAngleNormalized % 360f;
        if (finalAngle < 0) finalAngle += 360f;
        
        // Determine quarter from final angle (pointer at top)
        // Adjust angle so 0° is centered at top (add 45° offset)
        float adjustedAngle = (finalAngle + 45f) % 360f;
        int landedQuarter = (4 - Mathf.FloorToInt(adjustedAngle / quarterAngle)) % 4;
        
        // Store current quarter
        currentQuarter = landedQuarter;
        
        Debug.Log($"BonusWheel: Target={targetQuarter}, FinalAngle={finalAngle:F1}°, Landed={landedQuarter}, Reward={goldRewards[landedQuarter]}");
        
        // Grant reward
        GrantReward(landedQuarter);
        
        // Play win sound
        if (audioSource != null && winSound != null)
            audioSource.PlayOneShot(winSound);
        
        // Invoke completion event
        OnSpinComplete?.Invoke();
        
        isSpinning = false;
    }
    
    private void GrantReward(int quarter)
    {
        // Validate quarter index
        if (quarter < 0 || quarter >= 4)
        {
            Debug.LogError($"BonusWheel: Invalid quarter index {quarter}");
            return;
        }
        
        int goldAmount = goldRewards[quarter];
        string rewardName = rewardNames[quarter];
        
        // Add gold using the ShopUnlocksManager
        if (ShopUnlocksManager.instance != null)
        {
            ShopUnlocksManager.instance.AddGold(goldAmount);
            Debug.Log($"BonusWheel: Granted {goldAmount} gold ({rewardName})");
        }
        else
        {
            Debug.LogWarning("BonusWheel: ShopUnlocksManager.instance is null. Cannot grant gold.");
        }
        
        // Invoke event for UI updates or other game logic
        OnRewardGranted?.Invoke(goldAmount, rewardName);
    }
    
    /// <summary>
    /// Returns true if the wheel is currently spinning
    /// </summary>
    public bool IsSpinning()
    {
        return isSpinning;
    }
    
    /// <summary>
    /// Get the current quarter the wheel is on (0-3)
    /// </summary>
    public int GetCurrentQuarter()
    {
        return currentQuarter;
    }
    
    /// <summary>
    /// Force stop the wheel (use with caution)
    /// </summary>
    public void ForceStop()
    {
        StopAllCoroutines();
        isSpinning = false;
    }
}
