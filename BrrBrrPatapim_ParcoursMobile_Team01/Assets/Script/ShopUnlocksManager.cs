using System;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Script
{
    [Serializable]
    public class ShopUnlocksData
    {
        public List<string> unlockedIds = new List<string>();
    }

    public class ShopUnlocksManager : MonoBehaviour
    {
        
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

        private static HashSet<string> _unlockedCache;
        private static bool _loaded;
        private static bool _ugsInitialized;
        private static bool _ugsInitializing;

        private static void EnsureLoaded()
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

        public static bool IsUnlocked(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            EnsureLoaded();
            return _unlockedCache.Contains(id);
        }

        public static void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            EnsureLoaded();
            if (_unlockedCache.Add(id))
            {
                Save();
            }
        }

        public static void Lock(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            EnsureLoaded();
            if (_unlockedCache.Remove(id))
            {
                Save();
            }
        }

        public static void ClearAll()
        {
            _unlockedCache = new HashSet<string>();
            _loaded = true;
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            PlayerPrefs.Save();
        }

        public static void Save()
        {
            EnsureLoaded();
            var data = new ShopUnlocksData { unlockedIds = new List<string>(_unlockedCache) };
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }
    }
}
