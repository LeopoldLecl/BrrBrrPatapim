using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Purchasing;

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
        
        [SerializeField] TextMeshProUGUI goldText;
        
        public string environment = "production";
 
        // async void Start() {
        //     try {
        //         var options = new InitializationOptions()
        //             .SetEnvironmentName(environment);
        //
        //         await UnityServices.InitializeAsync(options);
        //     }
        //     catch (Exception exception) {
        //         // An error occurred during initialization.
        //         Debug.LogException(exception);
        //     }
        // }
        
        private const string PlayerPrefsKey = "SHOP_UNLOCKS_JSON";
        private const string PlayerPrefsGoldKey = "SHOP_GOLD_AMOUNT";

        private static HashSet<string> _unlockedCache;
        private static bool _loaded;
        private static bool _ugsInitialized;
        private static bool _ugsInitializing;
        
        [SerializeField]
        private int goldAmount; // default 0

        public int GoldAmount
        {
            get
            {
                return goldAmount;
            }
            private set
            {
                goldAmount = value;
            }
        }

        private void Awake()
        {
            instance ??= this;
            // Load persisted gold on startup
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
            {
                Save();
            }
        }

        public void Lock(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            EnsureLoaded();
            if (_unlockedCache.Remove(id))
            {
                Save();
            }
        }

        public void ClearAll()
        {
            _unlockedCache = new HashSet<string>();
            _loaded = true;
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            // Reset gold and remove persisted value
            if (instance != null) instance.GoldAmount = 0;
            PlayerPrefs.DeleteKey(PlayerPrefsGoldKey);
            PlayerPrefs.Save();
        }

        public void Save()
        {
            EnsureLoaded();
            var data = new ShopUnlocksData { unlockedIds = new List<string>(_unlockedCache) };
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        // --- Gold persistence & helpers ---
        public static int GetGold()
        {
            // If instance is alive, return its cached value; otherwise fetch from PlayerPrefs
            return instance != null ? instance.GoldAmount : PlayerPrefs.GetInt(PlayerPrefsGoldKey, 0);
        }

        public void SetGold(int amount)
        {
            if (amount < 0) amount = 0;
            if (instance != null)
            {
                instance.GoldAmount = amount;
            }
            
            
            if (goldText != null)
            {
                goldText.text = goldAmount.ToString();
            }
            
            PlayerPrefs.SetInt(PlayerPrefsGoldKey, amount);
            PlayerPrefs.Save();
        }

        public void AddGold(int delta)
        {
            // Support negative delta; clamp at 0..int.MaxValue
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
        
        public void OnGoldPurchased(Product product)
        {
            if (product == null) return;
            if (product.definition.id == "product_gold_small")
            {
                AddGold(100);
            }
            else if (product.definition.id == "product_gold_medium")
            {
                AddGold(250);
            }
            else if (product.definition.id == "product_gold_large")
            {
                AddGold(600);
            }
        }
    }
}
