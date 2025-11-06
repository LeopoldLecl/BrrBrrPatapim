using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Purchasing;
using System.Collections;

namespace Script
{
    [Serializable]
    public class ShopUnlocksData
    {
        public List<string> unlockedIds = new List<string>();
    }

    public class ShopUnlocksManager : MonoBehaviour
    {
        public static ShopUnlocksManager instance;

        [Header("UI References")]
        [Tooltip("Tous les textes où afficher la quantité d’or dans le jeu.")]
        [SerializeField] private List<TextMeshProUGUI> goldTexts = new List<TextMeshProUGUI>();

        [Header("Environment")]
        public string environment = "production";

        private const string PlayerPrefsKey = "SHOP_UNLOCKS_JSON";
        private const string PlayerPrefsGoldKey = "SHOP_GOLD_AMOUNT";
        private const string EquippedSkinKey = "EQUIPPED_SKIN_ID";

        private static HashSet<string> _unlockedCache;
        private static bool _loaded;

        [SerializeField] private int goldAmount;

        public int GoldAmount
        {
            get => goldAmount;
            private set => goldAmount = value;
        }

        private void Awake()
        {
            instance ??= this;
            LoadGold();
            EnsureLoaded();
            UpdateGoldUI();
        }

        private void Start()
        {
            UpdateGoldUI();
            StartCoroutine(WaitFor1SecCoroutine());
        }

        IEnumerator WaitFor1SecCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            gameObject.SetActive(false);
        }

        private void LoadGold()
        {
            goldAmount = PlayerPrefs.GetInt(PlayerPrefsGoldKey, 0);
        }

        private void EnsureLoaded()
        {
            if (_loaded) return;

            var json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                _unlockedCache = new HashSet<string>();
            }
            else
            {
                try
                {
                    var data = JsonUtility.FromJson<ShopUnlocksData>(json);
                    _unlockedCache = data != null && data.unlockedIds != null
                        ? new HashSet<string>(data.unlockedIds)
                        : new HashSet<string>();
                }
                catch
                {
                    _unlockedCache = new HashSet<string>();
                }
            }
            _loaded = true;
        }

        public bool IsUnlocked(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            EnsureLoaded();
            return _unlockedCache.Contains(id);
        }

        public void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            EnsureLoaded();
            if (_unlockedCache.Add(id))
                Save();
        }

        public void Lock(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            EnsureLoaded();
            if (_unlockedCache.Remove(id))
                Save();
        }

        public void Save()
        {
            EnsureLoaded();
            var data = new ShopUnlocksData { unlockedIds = new List<string>(_unlockedCache) };
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        //  Reset complet : or, unlocks, équipements
        [ContextMenu("Reset All Shop Data")]
        public void ResetAll()
        {
            Debug.Log(" Reset complet du shop...");

            _unlockedCache = new HashSet<string>();
            _loaded = true;

            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            PlayerPrefs.DeleteKey(PlayerPrefsGoldKey);
            PlayerPrefs.DeleteKey(EquippedSkinKey);
            PlayerPrefs.Save();

            goldAmount = 0;
            UpdateGoldUI();

            if (SkinsSelectionManager.Instance != null)
                SkinsSelectionManager.Instance.ForceUnequipAll();

            var items = FindObjectsByType<ShopItem>(FindObjectsSortMode.None);
            foreach (var item in items)
                item.ForceLockedState();

            Debug.Log(" Shop totalement réinitialisé (gold, unlocks, skins).");
        }

        // --- Gestion de l'or ---
        public static int GetGold()
        {
            return instance != null ? instance.GoldAmount : PlayerPrefs.GetInt(PlayerPrefsGoldKey, 0);
        }

        public void SetGold(int amount)
        {
            if (amount < 0) amount = 0;
            if (instance != null)
                instance.GoldAmount = amount;

            PlayerPrefs.SetInt(PlayerPrefsGoldKey, amount);
            PlayerPrefs.Save();

            UpdateGoldUI();
        }

        public void AddGold(int delta)
        {
            long newValue = (long)GetGold() + delta;
            if (newValue < 0) newValue = 0;
            if (newValue > int.MaxValue) newValue = int.MaxValue;
            SetGold((int)newValue);
        }

        public bool TrySpendGold(int cost)
        {
            if (cost < 0) cost = 0;
            int current = GetGold();
            if (current < cost) return false;

            SetGold(current - cost);
            return true;
        }

        //  Met à jour tous les textes de gold référencés
        private void UpdateGoldUI()
        {
            foreach (var text in goldTexts)
            {
                if (text != null)
                    text.text = $"{goldAmount}";
            }
        }

        //  Permet d’enregistrer dynamiquement un texte en runtime
        public void RegisterGoldText(TextMeshProUGUI text)
        {
            if (text == null) return;
            if (!goldTexts.Contains(text))
                goldTexts.Add(text);

            text.text = $"{goldAmount}";
        }

        //  Permet d’enlever un texte si tu détruis un objet à runtime
        public void UnregisterGoldText(TextMeshProUGUI text)
        {
            if (text == null) return;
            goldTexts.Remove(text);
        }

        public void OnGoldPurchased(Product product)
        {
            if (product == null) return;

            if (product.definition.id == "product_gold_small")
                AddGold(100);
            else if (product.definition.id == "product_gold_medium")
                AddGold(250);
            else if (product.definition.id == "product_gold_large")
                AddGold(600);
        }
    }
}
