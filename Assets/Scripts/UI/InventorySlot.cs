using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private int nowQuantity;
    private ItemData m_itemData; // 내부 보관용 추가

    [SerializeField] private GameObject m_selected;


    private void OnValidate()
    {
        // 에디터에서 수량 바꾸면 텍스트도 자동 갱신됨
        if (quantityText != null)
        {
            quantityText.text = nowQuantity > 1 ? nowQuantity.ToString() : "";
        }
    }

    public void SetSlot(ItemData itemData, int quantity)
    {
        if (itemData == null)
        {
            ClearSlot(); // 빈 슬롯이면 UI 비우기
            return;
        }

        icon.sprite = itemData.m_itemIcon;
        icon.enabled = true;
        quantityText.text = quantity > 1 ? quantity.ToString() : "";
    }


    public void ClearSlot()
    {
        icon.sprite = null;
        icon.enabled = false;
        nowQuantity = 0;
        quantityText.text = "";
    }

    public int GetQuantity() => nowQuantity;

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

