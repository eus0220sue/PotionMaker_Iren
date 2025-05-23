using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
[CreateAssetMenu(fileName = "CraftData", menuName = "Data/CraftData", order = 1)]

public class CraftData:ScriptableObject
{

    [SerializeField] ItemType.Type m_itemType = ItemType.Type.Material;
    public string m_name = null;

    [SerializeField]int m_index = 0;

    /// <summary>
    /// 재료 아이템 데이터
    /// </summary>
    [SerializeField] ItemData m_inputItemData = null;
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
    [SerializeField] int m_inputAmount;
    
    public ItemType.Type IsItemType { get { return m_itemType; } }
    public ItemData IsInputItemData { get { return m_inputItemData; } }
    public ItemData IsOutputItemData { get { return m_outputItemData; } }

    public int IsOAmount { get { return m_outputAmount; } } 

    public int IsIAmount { get { return m_inputAmount; } }

    public int IsIndex { get { return m_index; } }

}
