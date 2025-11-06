using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Script;

[RequireComponent(typeof(Button))]
public class ParticleShopItem : MonoBehaviour
{
    [OnValueChanged("ChangeValues")]
    [SerializeField] private ParticleSetScriptableObject particleData;

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
        yield return null; // attend une frame pour laisser le ShopUnlocksManager s’initialiser
        InitializeState();
    }

    private void OnDestroy()
    {
        if (buyButton != null)
            buyButton.onClick.RemoveListener(OnButtonClick);
    }

    private void InitializeState()
    {
        if (particleData == null)
        {
            Debug.LogError("[ParticleShopItem] particleData non assigné !");
            return;
        }

        if (ShopUnlocksManager.instance == null)
        {
            ShopUnlocksManager.instance = FindFirstObjectByType<ShopUnlocksManager>();
            if (ShopUnlocksManager.instance == null)
            {
                Debug.LogError("[ParticleShopItem] ShopUnlocksManager introuvable !");
                return;
            }
        }

        string id = GetItemId();
        isPurchased = ShopUnlocksManager.instance.IsUnlocked(id);

        // Vérifie si déjà équipé
        var equippedKey = ParticleSelectionManager.Instance?.GetEquippedParticleKey();
        isEquipped = !string.IsNullOrEmpty(equippedKey) && equippedKey == id;

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
            Equip();
        }
    }

    private void TryPurchase()
    {
        // Tu peux donner un prix dans un futur ScriptableObject spécifique si besoin.
        // Pour l'instant, on fixe un prix par exemple.
        int price = 100;

        if (!ShopUnlocksManager.instance.TrySpendGold(price))
        {
            Debug.Log("Pas assez d'or pour acheter ce set de particules !");
            return;
        }

        ShopUnlocksManager.instance.Unlock(GetItemId());
        isPurchased = true;

        Debug.Log($"[ParticleShopItem] Particules {particleData.DisplayName} débloquées !");
        ApplyStateToUI();
    }

    private void Equip()
    {
        if (ParticleSelectionManager.Instance == null)
        {
            Debug.LogError("[ParticleShopItem] Aucun ParticleSelectionManager trouvé !");
            return;
        }

        if (isEquipped)
        {
            // Si déjà équipé  déséquipe
            ParticleSelectionManager.Instance.ResetParticleSelection();
            isEquipped = false;
        }
        else
        {
            // Équipe ce set
            ParticleSelectionManager.Instance.EquipParticleSet(particleData);
            isEquipped = true;
        }

        // Déséquipe visuellement les autres items du même shop
        var allItems = FindObjectsByType<ParticleShopItem>(FindObjectsSortMode.None);
        foreach (var item in allItems)
        {
            if (item != this)
                item.SetEquipped(false);
        }

        ApplyStateToUI();
    }

    public void SetEquipped(bool equipped)
    {
        isEquipped = equipped;
        ApplyStateToUI();
    }

    private void ApplyStateToUI()
    {
        if (itemPriceText)
        {
            if (!isPurchased)
                itemPriceText.text = "100"; // prix fixe ici
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
            itemImage.sprite = particleData.Icon;
            itemImage.enabled = itemImage.sprite != null;
        }
    }

#if UNITY_EDITOR
    public void ChangeValues()
    {
        ApplyStateToUI();
    }
#endif

    private string GetItemId()
    {
        return particleData != null ? particleData.ParticleKey : string.Empty;
    }
}
