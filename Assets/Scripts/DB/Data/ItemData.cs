using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemData", menuName = "Data/Item Data", order = 1)]

public class ItemData : ScriptableObject
{
    [SerializeField] ItemType.Type m_itemType = ItemType.Type.Material;
    public string m_itemID = null;

    public string m_itemName;
    public int m_maxStack;
    public Sprite m_itemIcon;
    public ItemType ItemType;
    [TextArea] public string description;
    public bool m_usableItem;

    public ItemType.Type IsItemType {  get { return m_itemType; } }
}
