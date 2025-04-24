using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]

public class InvenSlotData
{
    public ItemData m_itemData;
    public int m_quantity;

    public InvenSlotData(ItemData data,int quantity)
    {
        m_itemData = data;
        this.m_quantity=quantity;
    }
    public bool IsSlotFull()
    {
        return m_quantity >= m_itemData.m_maxStack;
    }
}
[Serializable]
public class InvenData
{
    public InvenSlotData[] m_slotData;
    public InvenData(int size)
    {
        m_slotData = new InvenSlotData[size];
    }
    public void Additem(ItemData data, int amount = 1)
    {
        for (int i = 0; i < m_slotData.Length; i++)
        {
            var slotData = m_slotData[i];

            if (slotData!=null && slotData.m_itemData == data && !slotData.IsSlotFull())
            {
                int space = data.m_maxStack - slotData.m_quantity; ;
                int addAmount = Mathf.Min(space, amount);
                amount -= addAmount;
                if (amount <= 0) return;
            }
        }
        for (int i = 0; i < m_slotData.Length; i++)
        {
            if (m_slotData[i] == null)
            {
                int addAmount = Mathf.Min(data.m_maxStack, amount);
                m_slotData[i] = new InvenSlotData(data, addAmount);
                amount -= addAmount;
                if (amount <= 0) return;
            }
        }


    }
    public void RemoveItem(ItemData data, int amount = 1)
    {
        for (int i = 0; i < m_slotData.Length; i++)
        {
            var slot = m_slotData[i];
            if (slot != null && slot.m_itemData == data)
            {
                slot.m_quantity -= amount;
                if (slot.m_quantity <= 0)
                    m_slotData[i] = null;
                return;
            }
        }
    }
}


