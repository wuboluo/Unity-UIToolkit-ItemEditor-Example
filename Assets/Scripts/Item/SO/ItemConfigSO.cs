using UnityEngine;

// [CreateAssetMenu(fileName = "ItemConfigSO", menuName = "Item/Item Config")]
public class ItemConfigSO : ScriptableObject
{
    public string GUID { get; set; }

    // 通用属性
    public int itemID;
    public string itemName;
    public ItemType itemType;
    public Sprite itemIcon;

    // 描述
    public string itemDescription;

    // 使用规则
    public bool canPickedUp;
    public bool canDropped;
    public bool canAte;

    // 价值
    public int itemPrice;
    [Range(0, 1)] public float sellPercentage;
}