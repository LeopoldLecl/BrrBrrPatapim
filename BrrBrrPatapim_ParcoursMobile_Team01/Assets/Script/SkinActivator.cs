using UnityEngine;
using Script.ScriptableObjects.Scripts;

public class SkinActivator : MonoBehaviour
{
    [System.Serializable]
    public class SkinEntry
    {
        [Tooltip("Référence directe au ShopItemScriptableObject de ce skin.")]
        public ShopItemScriptableObject shopItem;

        [Tooltip("GameObject correspondant à ce skin dans la scène.")]
        public GameObject skinObject;
    }

    [Header("Skins par défaut (actifs si aucun skin n’est équipé)")]
    [SerializeField] private GameObject[] defaultSkins;

    [Header("Autres skins (activés selon le ShopItemScriptableObject équipé)")]
    [SerializeField] private SkinEntry[] skins;

    private void Start()
    {
        if (SkinsSelectionManager.Instance != null)
            StartCoroutine(WaitAndRefresh());
    }

    private System.Collections.IEnumerator WaitAndRefresh()
    {
        yield return new WaitForSeconds(0.25f);
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        string equippedKey = SkinsSelectionManager.Instance != null
            ? SkinsSelectionManager.Instance.GetEquippedSkinId()
            : string.Empty;

        bool hasEquipped = !string.IsNullOrEmpty(equippedKey);

        // Désactive/active les skins par défaut
        foreach (var def in defaultSkins)
        {
            if (def != null)
                def.SetActive(!hasEquipped);
        }

        // Active le bon skin
        bool found = false;

        foreach (var entry in skins)
        {
            if (entry.skinObject == null || entry.shopItem == null) continue;

            // Compare le skinKey du ScriptableObject au skin équipé
            string key = entry.shopItem.GetSkinKey();
            bool match = string.Equals(key, equippedKey, System.StringComparison.OrdinalIgnoreCase);
            entry.skinObject.SetActive(match);

            if (match)
            {
                found = true;
                Debug.Log($"[SkinActivator] Enabled {entry.skinObject.name} for skin '{key}'");
            }
        }

        if (!found && hasEquipped)
            Debug.LogWarning($"[SkinActivator] Aucun skin trouvé correspondant à la clé '{equippedKey}'");

        Debug.Log($"[SkinActivator] Active skin = {(hasEquipped ? equippedKey : "default")}");
    }
}
