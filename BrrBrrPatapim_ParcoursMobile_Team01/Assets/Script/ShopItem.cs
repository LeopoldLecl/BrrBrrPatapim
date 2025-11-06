using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Script;
using Script.ScriptableObjects.Scripts;

public class ShopItem : MonoBehaviour
{
    [OnValueChanged("ChangeValues")]
    [SerializeField] private ShopItemScriptableObject shopItemData;

    [Header("UI References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private Button buyButton;

    [Header("Colors")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color equippedColor = new Color(0.3f, 1f, 0.3f);

    private bool isPurchased;
    private bool isEquipped;

    private void Awake()
    {
        if (buyButton == null)
            buyButton = GetComponent<Button>();

        if (buyButton != null)
            buyButton.onClick.AddListener(OnButtonClick);
    }

    private System.Collections.IEnumerator Start()
    {
        yield return null; // attend une frame pour que ShopUnlocksManager soit prêt
        InitializeState();
    }

    private void OnDestroy()
    {
        if (buyButton != null)
            buyButton.onClick.RemoveListener(OnButtonClick);
    }


    public void ForceLockedState()
    {
        // Déséquipe visuellement
        SetEquipped(false);

        // Marque comme non acheté
        var id = GetItemId();
        if (!string.IsNullOrEmpty(id))
        {
            isPurchased = false;
        }

        ApplyStateToUI();
    }



    private void InitializeState()
    {
        if (shopItemData == null) return;

        if (ShopUnlocksManager.instance == null)
        {
            ShopUnlocksManager.instance = FindFirstObjectByType<ShopUnlocksManager>();
            if (ShopUnlocksManager.instance == null)
            {
                Debug.LogError("ShopUnlocksManager is missing in scene!");
                return;
            }
        }

        string id = shopItemData.GetItemId();
        isPurchased = ShopUnlocksManager.instance.IsUnlocked(id);
        ApplyStateToUI();
    }

    private void OnButtonClick()
    {
        if (!isPurchased)
        {
            TryPurchase();
        }
        else
        {
            SkinsSelectionManager.Instance?.SetEquippedSkin(this);
        }
    }

    public void SetEquipped(bool equipped)
    {
        isEquipped = equipped;
        ApplyStateToUI();
    }

    private void TryPurchase()
    {
        int price = shopItemData.GetItemPrice();

        if (!ShopUnlocksManager.instance.TrySpendGold(price))
        {
            Debug.Log("Not enough gold to purchase this item.");
            return;
        }

        ShopUnlocksManager.instance.Unlock(shopItemData.GetItemId());
        isPurchased = true;
        shopItemData.InvokeUnlockEvent();
        ApplyStateToUI();
    }

    public string GetItemId()
    {
        return shopItemData != null ? shopItemData.GetItemId() : string.Empty;
    }


    private void ApplyStateToUI()
    {
        if (itemPriceText)
        {
            if (!isPurchased)
                itemPriceText.text = $"{shopItemData.GetItemPrice()} G";
            else if (isEquipped)
                itemPriceText.text = "Equipped";
            else
                itemPriceText.text = "Owned";
        }

        if (buyButton != null)
        {
            var colors = buyButton.colors;
            colors.normalColor = isEquipped ? equippedColor : defaultColor;
            colors.selectedColor = colors.normalColor;
            colors.highlightedColor = isEquipped ? equippedColor : defaultColor * 1.1f;
            buyButton.colors = colors;
        }

        if (itemImage != null)
        {
            itemImage.sprite = shopItemData.GetItemIcon();
            itemImage.enabled = itemImage.sprite != null;
        }
    }

#if UNITY_EDITOR
    public void ChangeValues()
    {
        ApplyStateToUI();
    }
#endif
}
