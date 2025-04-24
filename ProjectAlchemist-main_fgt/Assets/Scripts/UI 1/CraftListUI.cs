using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftListUI : MonoBehaviour
{
    private CraftData m_craftData;     
    private OilCraftData m_oilData;
    public TMP_Text outputNameText;
    public TMP_Text resNameText;
    public Image m_outputImg;
    public Image m_resImg;

    [SerializeField] private GameObject highlightBorder;
    public CraftData GetCraftData() => m_craftData;
    public OilCraftData GetOilData() => m_oilData;

    public void Setup(CraftData data)
    {
        m_craftData = data;
        if (data == null) return;

        if (data.IsOutputItemData != null)
        {
            m_outputImg.sprite = data.IsOutputItemData.m_itemIcon;
            outputNameText.text = data.IsOutputItemData.m_itemName;
        }

        if (data.IsInputItemData != null)
        {
            m_resImg.sprite = data.IsInputItemData.m_itemIcon;
            resNameText.text = data.IsInputItemData.m_itemName;
        }

        highlightBorder.SetActive(false);
    }

    public void Setup(OilCraftData data)
    {
        m_oilData = data;
        if (data == null) return;

        if (data.IsOutputItem != null)
        {
            m_outputImg.sprite = data.IsOutputItem.m_itemIcon;
            outputNameText.text = data.IsOutputItem.m_itemName;
        }

        if (data.IsInputI1 != null)
        {
            m_resImg.sprite = data.IsInputI1.m_itemIcon;
            resNameText.text = data.IsInputI1.m_itemName;
        }

        highlightBorder.SetActive(false);
    }


    public void SetHighlight(bool isActive)
    {
        highlightBorder.SetActive(isActive);
    }
}
