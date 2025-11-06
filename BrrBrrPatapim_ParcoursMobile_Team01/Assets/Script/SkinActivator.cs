using UnityEngine;
using System.Collections.Generic;
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

    private GameObject _activeSkin;

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
        _activeSkin = null;

        foreach (var def in defaultSkins)
            if (def != null)
                def.SetActive(!hasEquipped);

        foreach (var entry in skins)
        {
            if (entry.skinObject == null || entry.shopItem == null)
                continue;

            bool match = string.Equals(entry.shopItem.GetSkinKey(), equippedKey, System.StringComparison.OrdinalIgnoreCase);
            entry.skinObject.SetActive(match);

            if (match)
                _activeSkin = entry.skinObject;
        }

        if (!hasEquipped)
        {
            if (defaultSkins != null && defaultSkins.Length > 0)
                _activeSkin = defaultSkins[0];
        }

        Debug.Log($"[SkinActivator] Active skin = {(hasEquipped ? equippedKey : "default")}");
    }

    /// <summary>
    /// Retourne toutes les particules associées au skin actuellement actif.
    /// </summary>
    public List<ParticleSystem> GetActiveSkinParticles()
    {
        List<ParticleSystem> particles = new List<ParticleSystem>();

        if (_activeSkin != null)
        {
            var found = _activeSkin.GetComponentsInChildren<ParticleSystem>(true);
            particles.AddRange(found);
        }

        return particles;
    }
}
