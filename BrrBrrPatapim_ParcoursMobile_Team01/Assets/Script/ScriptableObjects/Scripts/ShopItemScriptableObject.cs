using UnityEngine;
using System;

namespace Script.ScriptableObjects.Scripts
{
    [CreateAssetMenu(fileName = "ShopItemScriptableObject", menuName = "Scriptable Objects/ShopItemScriptableObject")]
    public class ShopItemScriptableObject : ScriptableObject
    {
        public Sprite itemIcon;
        public int itemPrice;

        [SerializeField, Tooltip("Unique ID for this shop item. Generated automatically if empty.")]
        private string id;

        [SerializeField, Tooltip("Unity IAP Product ID configured in the store.")]
        private string productId;

        //Getters
        public Sprite GetItemIcon() => itemIcon;

        public int GetItemPrice() => itemPrice;

        public string GetItemId() => id;

        public string GetProductId() => productId;

    #if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-generate a deterministic unique id if missing
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
    #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
    #endif
            }
        }

        [ContextMenu("Regenerate ID (Caution)")]
        private void RegenerateId()
        {
            id = Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
    #endif
    }
}
