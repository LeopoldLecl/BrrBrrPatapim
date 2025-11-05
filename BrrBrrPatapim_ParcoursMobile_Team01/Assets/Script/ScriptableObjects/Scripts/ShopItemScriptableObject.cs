using UnityEngine;
using UnityEngine.Events;
using System;

namespace Script.ScriptableObjects.Scripts
{
    [CreateAssetMenu(fileName = "ShopItemScriptableObject", menuName = "Scriptable Objects/ShopItemScriptableObject")]
    public class ShopItemScriptableObject : ScriptableObject
    {
        [Header("Item Data")]
        [SerializeField] private Sprite itemIcon;
        [SerializeField] private int itemPrice;

        [SerializeField, Tooltip("Unique ID for this shop item. Generated automatically if empty.")]
        private string id;

        [SerializeField, Tooltip("Unity IAP Product ID configured in the store (optional).")]
        private string productId;

        [Header("Unlock Event")]
        [Tooltip("Called when this item is successfully unlocked or purchased.")]
        [SerializeField] private UnityEvent onUnlocked;

        // --- Events for editor auto-refresh ---
        public event Action<ShopItemScriptableObject> Changed;
        public event Action<Sprite> IconChanged;

        // --- Getters ---
        public Sprite GetItemIcon() => itemIcon;
        public int GetItemPrice() => itemPrice;
        public string GetItemId() => id;
        public string GetProductId() => productId;

        // --- Setters ---
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

        /// <summary>
        /// Appelé quand le joueur débloque ou achète cet item.
        /// Déclenche tous les callbacks assignés dans l'inspecteur.
        /// </summary>
        public void InvokeUnlockEvent()
        {
            onUnlocked?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-génère un ID unique si manquant
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
                UnityEditor.EditorUtility.SetDirty(this);
            }

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
