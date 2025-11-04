using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

public class ShopItem : MonoBehaviour
{
    [OnValueChanged("ChangeValues")]
    [SerializeField] private ShopItemScriptableObject shopItemData;

    private Image itemImage;
    private TextMeshProUGUI itemPriceText;
    private bool isPurchased;

    private void Reset()
    {
        itemImage = GetComponent<Image>();
        itemPriceText = GetComponentInChildren<TextMeshProUGUI>();
    }

    [Button("Refresh Values")]
    public void ChangeValues()
    {
        Debug.Log("ShopItem::ChangeValues");
        itemPriceText.text = shopItemData != null ? shopItemData.GetItemPrice().ToString() : "null";
        itemImage.sprite = shopItemData != null ? shopItemData.GetItemIcon() : null;
    }
    
    
}
