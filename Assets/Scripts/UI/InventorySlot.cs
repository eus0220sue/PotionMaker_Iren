using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text quantityText;
    [SerializeField] int m_nowQuantity;
    public ItemData m_itemData; // 내부 보관용 추가

    [SerializeField] private GameObject m_selected;


    private void OnValidate()
    {
        // 에디터에서 수량 바꾸면 텍스트도 자동 갱신됨
        if (quantityText != null)
        {
            quantityText.text = m_nowQuantity > 1 ? m_nowQuantity.ToString() : "";
        }
    }

    public void SetSlot(ItemData itemData, int quantity)
    {
        if (itemData == null)
        {
            ClearSlot(); // 빈 슬롯이면 UI 비우기
            return;
        }

        m_itemData = itemData;       //  내부 상태 저장
        m_nowQuantity = quantity;      //  수량도 저장

        icon.sprite = itemData.m_itemIcon;
        icon.enabled = true;
        quantityText.text = quantity > 1 ? quantity.ToString() : "";
    }


    public void ClearSlot()
    {
        icon.sprite = null;
        icon.enabled = false;
        m_itemData = null;
        m_nowQuantity = 0;
        quantityText.text = "";
    }


    public int GetQuantity() => m_nowQuantity;

    public void SetSelected(bool isOn)
    {
        if (m_selected != null)
            m_selected.SetActive(isOn);
    }

    public string GetItemName()
    {
        return m_itemData != null ? m_itemData.m_itemName : "빈 슬롯";
    }

}

