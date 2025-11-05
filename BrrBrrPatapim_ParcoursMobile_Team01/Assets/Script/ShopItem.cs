using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Script; // for ShopUnlocksManager
using Script.ScriptableObjects.Scripts;
using UnityEngine.Purchasing; // for ShopItemScriptableObject
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class ShopItem : MonoBehaviour
{
    [OnValueChanged("ChangeValues")]
    [SerializeField] private ShopItemScriptableObject shopItemData;

    // Allow manual hookup in inspector; will also be auto-resolved at runtime.
    [SerializeField] private Image itemImage;
    private TextMeshProUGUI itemPriceText;
    private bool isPurchased;

    private void Awake()
    {
        EnsureReferences();
        SubscribeToData();
        InitializeState();
    }

    private void OnEnable()
    {
        EnsureReferences();
        SubscribeToData();
        InitializeState();
    }

    private void OnDisable()
    {
        UnsubscribeFromData();
    }

    private void OnDestroy()
    {
        UnsubscribeFromData();
    }

    private void Reset()
    {
        // Prefer local, then children as fallback so prefab variants continue to work
        if (itemImage == null) itemImage = GetComponent<Image>();
        if (itemImage == null) itemImage = GetComponentInChildren<Image>(true);
        if (itemPriceText == null) itemPriceText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void EnsureReferences()
    {
        if (itemImage == null)
        {
            itemImage = GetComponent<Image>();
            if (itemImage == null) itemImage = GetComponentInChildren<Image>(true);
        }
        if (itemPriceText == null)
        {
            itemPriceText = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void SubscribeToData()
    {
        if (shopItemData == null) return;
        UnsubscribeFromData(); // avoid duplicates
        shopItemData.IconChanged += OnIconChanged;
        shopItemData.Changed += OnDataChanged;
    }

    private void UnsubscribeFromData()
    {
        if (shopItemData == null) return;
        shopItemData.IconChanged -= OnIconChanged;
        shopItemData.Changed -= OnDataChanged;
    }

    private void OnIconChanged(Sprite _)
    {
        // Only update sprite to be lightweight
        var sprite = shopItemData != null ? shopItemData.GetItemIcon() : null;
        if (itemImage != null)
        {
            itemImage.sprite = sprite;
            if (sprite != null) itemImage.enabled = true;
        }
    }

    private void OnDataChanged(ShopItemScriptableObject _)
    {
        InitializeState();
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
            isPurchased = ShopUnlocksManager.instance.IsUnlocked(id);
        }
        ApplyStateToUI();
    }

    private void ApplyStateToUI()   
    {
        // Price text
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

        // Sprite assignment for UI Image or SpriteRenderer
        var sprite = shopItemData != null ? shopItemData.GetItemIcon() : null;
        if (itemImage != null)
        {
            itemImage.sprite = sprite;
            // Ensure image is visible if a sprite exists
            if (sprite != null)
            {
                itemImage.enabled = true;
                var c = itemImage.color; c.a = Mathf.Max(c.a, 1f); itemImage.color = c;
            }
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
        EnsureReferences();
        UnsubscribeFromData();
        SubscribeToData();
        ApplyStateToUI();
    }

    public void OnIAPPurchaseComplete(Product product)
    {
        if (shopItemData == null) return;
        if (isPurchased) { ApplyStateToUI(); return; }
        // Optional: verify product.definition.id == shopItemData.GetProductId()
        var id = shopItemData.GetItemId();
        if (string.IsNullOrEmpty(id)) return;
        ShopUnlocksManager.instance.Unlock(id);
        isPurchased = true;
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
