using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
[CreateAssetMenu(fileName = "OilCraftData", menuName = "Data/OilCraftData", order = 1)]

public class OilCraftData:ScriptableObject
{

    [SerializeField] ItemType.Type m_itemType = ItemType.Type.Material;
    [SerializeField] string m_name = null;

    [SerializeField] int m_index = 0;
    /// <summary>
    /// 재료 아이템 데이터
    /// </summary>
    [SerializeField] ItemData m_inputI1 = null;
    /// <summary>
    /// 재료 아이템 데이터2
    /// </summary>
    [SerializeField] ItemData m_inputI2 = null;
    /// <summary>
    /// 결과물 아이템 데이터
    /// </summary>
    [SerializeField] ItemData m_outputItemData = null;
    /// <summary>
    /// 결과물 제작량
    /// </summary>
    [SerializeField] int m_outputAmount;
    /// <summary>
    /// 재료 소모량
    /// </summary>
    [SerializeField] int m_IAmount1;
    [SerializeField] int m_IAmount2;

    public ItemType.Type IsItemType { get { return m_itemType; } }
    public ItemData IsInputI1 { get { return m_inputI1; } }
    public ItemData IsInputI2 { get { return m_inputI2; } }
    public ItemData IsOutputItem { get { return m_outputItemData; } }

    public int IsOAmount { get { return m_outputAmount; } }

    public int IsIAmount1 { get { return m_IAmount1; } }

    public int IsIAmount2 { get { return m_IAmount2; } }

    public int IsIndex { get { return m_index; } }
}
