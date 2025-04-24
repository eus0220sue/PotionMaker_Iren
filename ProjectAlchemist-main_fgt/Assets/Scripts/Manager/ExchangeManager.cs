using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExchangeManager : MonoBehaviour
{
    [SerializeField] public InventoryManager InvenManager;
    [SerializeField] public UserData m_userData;
    [SerializeField] public GradeType m_gradeType;
    public void Craft(CraftData data)
    {
        if (!InvenManager.IsInventoryData.HasItem(data.IsInputItemData, data.IsIAmount))
        {
            return;
        }
        if (!InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItemData, data.IsOAmount))
        {
            return;
        }

        InvenManager.ConsumeItem(data.IsInputItemData, data.IsIAmount);
        InvenManager.AddItem(data.IsOutputItemData, data.IsOAmount);

    }

    public void OilCraft(OilCraftData data)
    {
        if (!InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1))
        {
            return;
        }
        if (!InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItem, data.IsOAmount))
        {
            return;
        }

        InvenManager.ConsumeItem(data.IsInputI1, data.IsIAmount1);
        InvenManager.ConsumeItem(data.IsInputI2, data.IsIAmount2);

        InvenManager.AddItem(data.IsOutputItem, data.IsOAmount);

    }

    public void PotionCraft(PotionCraftData data)
    {
        if (m_userData.IsGrade < data.IsGradeType)
        {
            Debug.Log("[제작 불가능] 등급이 부족합니다.");
            return;
        }

        if (!InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1))
        {
            return;
        }
        if (!InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItem, data.IsOAmount))
        {
            return;
        }

        InvenManager.ConsumeItem(data.IsInputI1, data.IsIAmount1);
        InvenManager.ConsumeItem(data.IsInputI2, data.IsIAmount2);

        InvenManager.AddItem(data.IsOutputItem, data.IsOAmount);

    }
}
