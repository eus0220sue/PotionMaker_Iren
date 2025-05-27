using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Image m_iconImage;  // 슬롯 아이콘 이미지 참조

    public void Set(ItemData item, int amount)
    {
        if (item == null)
        {
            icon.enabled = false;
            countText.text = "";
            return;
        }

        icon.enabled = true;
        icon.sprite = item.m_itemIcon;
        countText.text = amount.ToString();
    }
    public void Clear()
    {
        icon.sprite = null ;
        icon.enabled = false;
        countText.text = "";
    }
    // Slot.cs 에서 이미지 컬러 제어 메서드
    public void SetIconColor(Color color)
    {
        if (m_iconImage != null)
            m_iconImage.color = color;
    }
}
