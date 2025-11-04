using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemScriptableObject", menuName = "Scriptable Objects/ShopItemScriptableObject")]
public class ShopItemScriptableObject : ScriptableObject
{
    public Sprite itemIcon;
    public int itemPrice;
    
    //Getters
    public Sprite GetItemIcon() => itemIcon;

    public int GetItemPrice() => itemPrice;
}
