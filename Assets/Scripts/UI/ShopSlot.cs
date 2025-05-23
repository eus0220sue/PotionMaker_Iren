using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class ShopSlot : MonoBehaviour
{
    [SerializeField] private Image m_goodsIcon;
    [SerializeField] private GameObject m_selected;

    private ItemData m_goodsData;

    public ItemData GetItemData() => m_goodsData;

    public void Set(ItemData item)
    {
        if (item == null)
        {
            m_goodsIcon.enabled = false;
            return;
        }

        m_goodsData = item;

        m_goodsIcon.enabled = true;
        m_goodsIcon.sprite = item.m_itemIcon;
        m_selected.SetActive(false);
    }

    public void Clear()
    {
        m_goodsData = null;
        m_goodsIcon.sprite = null;
        m_goodsIcon.enabled = false;

    }

    public void SetSelected(bool isActive)
    {
        m_selected.SetActive(isActive);
    }

    public string GetItemName()
    {
        return m_goodsData != null ? m_goodsData.m_itemName : "ºó ½½·Ô";
    }

}
