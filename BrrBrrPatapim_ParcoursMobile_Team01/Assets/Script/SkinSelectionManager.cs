using UnityEngine;
using System.Collections;
using Script;

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

        equippedSkinId = PlayerPrefs.GetString(EquippedSkinKey, string.Empty);
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(equippedSkinId))
            StartCoroutine(RestoreEquippedSkinWhenReady());
    }

    private IEnumerator RestoreEquippedSkinWhenReady()
    {
        int attempts = 0;
        while (attempts < 20)
        {
            ShopItem[] allItems = FindObjectsByType<ShopItem>(FindObjectsSortMode.None);
            if (allItems.Length > 0)
            {
                Debug.Log($"[Restore] Found {allItems.Length} ShopItems after {attempts} checks");

                foreach (var item in allItems)
                {
                    string id = item.GetItemId();
                    bool unlocked = ShopUnlocksManager.instance != null &&
                                    ShopUnlocksManager.instance.IsUnlocked(id);

                    Debug.Log($"[Restore] Checking {item.name}  id={id}, unlocked={unlocked}");

                    if (id == equippedSkinId && unlocked)
                    {
                        Debug.Log($"[Restore] Match found! Restoring {item.name}");
                        currentEquippedItem = item;
                        currentEquippedItem.SetEquipped(true);
                        yield break;
                    }
                }
            }

            attempts++;
            yield return new WaitForSeconds(0.1f); 
        }

        Debug.LogWarning("[Restore]  Aucun ShopItem trouvé après plusieurs tentatives.");
    }

    public void SetEquippedSkin(ShopItem item, bool save = true)
    {
        if (currentEquippedItem == item)
        {
            currentEquippedItem.SetEquipped(false);
            currentEquippedItem = null;
            equippedSkinId = string.Empty;

            if (save)
            {
                PlayerPrefs.DeleteKey(EquippedSkinKey);
                PlayerPrefs.Save();
            }
            return;
        }

        if (currentEquippedItem != null)
            currentEquippedItem.SetEquipped(false);

        currentEquippedItem = item;
        currentEquippedItem.SetEquipped(true);
        equippedSkinId = item.GetItemId();

        if (save)
        {
            PlayerPrefs.SetString(EquippedSkinKey, equippedSkinId);
            PlayerPrefs.Save();
        }

        Debug.Log($" Skin équipé sauvegardé : {equippedSkinId}");
    }

    public bool IsEquipped(ShopItem item)
    {
        return currentEquippedItem == item;
    }

    public string GetEquippedSkinId() => equippedSkinId;

    public void ForceUnequipAll()
    {
        if (currentEquippedItem != null)
        {
            currentEquippedItem.SetEquipped(false);
            currentEquippedItem = null;
        }

        equippedSkinId = string.Empty;
        PlayerPrefs.DeleteKey(EquippedSkinKey);
        PlayerPrefs.Save();

        Debug.Log(" Skin déséquipé et supprimé des PlayerPrefs");
    }

    public void RestoreEquippedSkin()
    {
        if (!string.IsNullOrEmpty(equippedSkinId))
            StartCoroutine(RestoreEquippedSkinWhenReady());
    }
}
