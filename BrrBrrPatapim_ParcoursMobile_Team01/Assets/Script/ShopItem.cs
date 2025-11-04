using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Script; // for ShopUnlocksManager
using Script.ScriptableObjects.Scripts; // for ShopItemScriptableObject
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class ShopItem : MonoBehaviour
{
    [OnValueChanged("ChangeValues")]
    [SerializeField] private ShopItemScriptableObject shopItemData;

    private Image itemImage;
    private TextMeshProUGUI itemPriceText;
    private bool isPurchased;

    private void Awake()
    {
        if (itemImage == null) itemImage = GetComponent<Image>();
        if (itemPriceText == null) itemPriceText = GetComponentInChildren<TextMeshProUGUI>();
        InitializeState();
    }

    private void OnEnable()
    {
        InitializeState();
    }

    private void Reset()
    {
        itemImage = GetComponent<Image>();
        itemPriceText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void InitializeState()
    {
        if (shopItemData == null)
        {
            isPurchased = false;
            ApplyStateToUI();
            return;
        }

        var id = shopItemData.GetItemId();
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning($"ShopItem '{name}' has ShopItemScriptableObject without ID. Open the asset in the editor to auto-generate an ID.");
            isPurchased = false;
        }
        else
        {
            isPurchased = ShopUnlocksManager.IsUnlocked(id);
        }
        ApplyStateToUI();
    }

    private void ApplyStateToUI()
    {
        if (itemPriceText != null)
        {
            if (shopItemData == null)
            {
                itemPriceText.text = "null";
            }
            else if (isPurchased)
            {
                itemPriceText.text = "Owned";
            }
            else
            {
                itemPriceText.text = shopItemData.GetItemPrice().ToString();
            }
        }

        if (itemImage != null)
        {
            itemImage.sprite = shopItemData != null ? shopItemData.GetItemIcon() : null;
        }

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            // Let IAPButton handle the purchase; we only disable when already owned.
            btn.interactable = !isPurchased;
        }
    }

    [Button("Refresh Values")]
    public void ChangeValues()
    {
        ApplyStateToUI();
    }

#if UNITY_PURCHASING
    // Hook this to IAPButton's On Purchase Complete (Product)
    public void OnIAPPurchaseComplete(Product product)
    {
        if (shopItemData == null) return;
        if (isPurchased) { ApplyStateToUI(); return; }
        // Optional: verify product.definition.id == shopItemData.GetProductId()
        var id = shopItemData.GetItemId();
        if (string.IsNullOrEmpty(id)) return;
        ShopUnlocksManager.Unlock(id);
        isPurchased = true;
        ApplyStateToUI();
    }

    // Hook this to IAPButton's On Purchase Failed (Product, PurchaseFailureReason)
    public void OnIAPPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        // No state change; ensure UI remains interactive if not owned
        ApplyStateToUI();
    }
#endif
}
