using UnityEngine;
using System;

namespace Script.ScriptableObjects.Scripts
{
    [CreateAssetMenu(fileName = "ShopItemScriptableObject", menuName = "Scriptable Objects/ShopItemScriptableObject")]
    public class ShopItemScriptableObject : ScriptableObject
    {
        [SerializeField] private Sprite itemIcon;
        [SerializeField] private int itemPrice;

        [SerializeField, Tooltip("Unique ID for this shop item. Generated automatically if empty.")]
        private string id;

        [SerializeField, Tooltip("Unity IAP Product ID configured in the store.")]
        private string productId;

        // Change notifications so scene users can refresh automatically
        public event Action<ShopItemScriptableObject> Changed;
        public event Action<Sprite> IconChanged;

        // Getters
        public Sprite GetItemIcon() => itemIcon;
        public int GetItemPrice() => itemPrice;
        public string GetItemId() => id;
        public string GetProductId() => productId;

        // Setters
        public void SetItemIcon(Sprite newIcon)
        {
            if (itemIcon == newIcon) return;
            itemIcon = newIcon;
            RaiseChanged(iconOnly: true);
        }

        public void SetItemPrice(int newPrice)
        {
            if (itemPrice == newPrice) return;
            itemPrice = newPrice;
            RaiseChanged();
        }

        public void SetProductId(string newProductId)
        {
            if (productId == newProductId) return;
            productId = newProductId;
            RaiseChanged();
        }

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
            // Notify listeners that values may have changed via inspector edits
            RaiseChanged();
        }

        [ContextMenu("Regenerate ID (Caution)")]
        private void RegenerateId()
        {
            id = Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
            RaiseChanged();
        }
    #endif

        private void RaiseChanged(bool iconOnly = false)
        {
        #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
        #endif
            if (iconOnly)
            {
                IconChanged?.Invoke(itemIcon);
            }
            else
            {
                IconChanged?.Invoke(itemIcon);
                Changed?.Invoke(this);
            }
        }
    }
}
