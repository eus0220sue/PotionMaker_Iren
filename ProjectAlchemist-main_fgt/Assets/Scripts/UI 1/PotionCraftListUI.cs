using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionCraftListUI : MonoBehaviour
{
    private PotionCraftData m_potionData;
    public TMP_Text outputNameText;
    public PotionCraftData GetPotionData() => m_potionData;
    public void Setup(PotionCraftData data)
    {
        m_potionData = data;
        if (data == null) return;

        if (data.IsOutputItem != null)
        {
            outputNameText.text = data.IsOutputItem.m_itemName;
        }
    }
    public void SetHighlight(bool isActive)
    {
        outputNameText.color = isActive
            ? Color.black
            : new Color32(161, 116, 37, 255);
    }

}
