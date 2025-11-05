using UnityEngine;

public class SkinsSelectionManager : MonoBehaviour
{
    public static SkinsSelectionManager Instance;

    private ShopItem currentEquippedItem;
    private string equippedSkinId;

    private const string EquippedSkinKey = "EQUIPPED_SKIN_ID";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Charger le dernier skin équipé sauvegardé
        equippedSkinId = PlayerPrefs.GetString(EquippedSkinKey, string.Empty);
    }

    private void Start()
    {
        // Une fois tous les ShopItems initialisés, on restaure celui qui correspond à l'ID sauvegardé
        if (!string.IsNullOrEmpty(equippedSkinId))
        {
            ShopItem[] items = FindObjectsByType<ShopItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                var id = item.GetItemId();
                if (id == equippedSkinId)
                {
                    SetEquippedSkin(item, save: false);
                    break;
                }
            }
        }
    }

    public void SetEquippedSkin(ShopItem item, bool save = true)
    {
        // Si on reclique sur le même skin > déséquipé
        if (currentEquippedItem == item)
        {
            currentEquippedItem.SetEquipped(false);
            currentEquippedItem = null;
            equippedSkinId = string.Empty;

            if (save)
                PlayerPrefs.DeleteKey(EquippedSkinKey);

            return;
        }

        // Déséquipe l'ancien
        if (currentEquippedItem != null)
            currentEquippedItem.SetEquipped(false);

        // Équipe le nouveau
        currentEquippedItem = item;
        currentEquippedItem.SetEquipped(true);
        equippedSkinId = item.GetItemId();

        if (save)
        {
            PlayerPrefs.SetString(EquippedSkinKey, equippedSkinId);
            PlayerPrefs.Save();
        }
    }

    public bool IsEquipped(ShopItem item)
    {
        return currentEquippedItem == item;
    }

    public string GetEquippedSkinId() => equippedSkinId;
}
