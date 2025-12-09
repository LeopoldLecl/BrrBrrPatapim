using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Example script showing how to use the BonusWheel component.
/// Attach this to a GameObject with a Button component.
/// </summary>
public class BonusWheelExample : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BonusWheel bonusWheel;
    [SerializeField] private Button spinButton;
    [SerializeField] private TextMeshProUGUI resultText;
    
    void Start()
    {
        // Subscribe to button click
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(OnSpinButtonClicked);
        }
        
        // Subscribe to reward granted event
        if (bonusWheel != null)
        {
            bonusWheel.OnRewardGranted += OnRewardReceived;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (spinButton != null)
        {
            spinButton.onClick.RemoveListener(OnSpinButtonClicked);
        }
        
        if (bonusWheel != null)
        {
            bonusWheel.OnRewardGranted -= OnRewardReceived;
        }
    }
    
    private void OnSpinButtonClicked()
    {
        if (bonusWheel == null)
        {
            Debug.LogError("BonusWheel reference is not set!");
            return;
        }
        
        // Only spin if not already spinning
        if (!bonusWheel.IsSpinning())
        {
            bonusWheel.Spin();
            
            // Disable button during spin
            if (spinButton != null)
            {
                spinButton.interactable = false;
            }
            
            // Clear result text
            if (resultText != null)
            {
                resultText.text = "Spinning...";
            }
        }
    }
    
    private void OnRewardReceived(int goldAmount, string rewardName)
    {
        Debug.Log($"Received reward: {rewardName} ({goldAmount} gold)");
        
        // Update UI
        if (resultText != null)
        {
            resultText.text = $"You won: {rewardName}!";
        }
        
        // Re-enable button
        if (spinButton != null)
        {
            spinButton.interactable = true;
        }
    }
}

