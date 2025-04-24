using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class ShopSlot : MonoBehaviour
{
    [SerializeField] private Image m_goodsIcon;
    [SerializeField] private GameObject m_highlight;

    private ItemData m_goodsData;

    public ItemData GetItemData() => m_goodsData;

    public void Set(ItemData item)
    {
        if (item == null)
        {
            m_goodsIcon.enabled = false;
            Debug.LogWarning("[ShopSlot] Set() 호출했지만 item이 null입니다.");
            return;
        }

        m_goodsData = item;

        m_goodsIcon.enabled = true;
        m_goodsIcon.sprite = item.m_itemIcon;
        m_highlight.SetActive(false);

        //  디버그 로그
        Debug.Log($"[ShopSlot] 슬롯 세팅 완료 - 아이템: {item.m_itemName}");
    }

    public void Clear()
    {
        m_goodsData = null;
        m_goodsIcon.sprite = null;
        m_goodsIcon.enabled = false;

        //  디버그 로그
        Debug.Log("[ShopSlot] 슬롯 클리어 완료");
    }

    public void SetHighlight(bool isActive)
    {
        m_highlight.SetActive(isActive);

        //  디버그 로그
        Debug.Log($"[ShopSlot] 하이라이트 {(isActive ? "활성화" : "비활성화")}");
    }
}
