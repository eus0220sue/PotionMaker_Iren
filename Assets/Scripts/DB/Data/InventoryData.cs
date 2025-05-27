using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlotData
{
    public ItemData itemData;
    public int quantity;

    public int nowQuantity => quantity;

    public InventorySlotData()
    {
        itemData = null;
        quantity = 0;
    }

    public InventorySlotData(ItemData data, int quantity)
    {
        this.itemData = data;
        this.quantity = quantity;
    }

    public bool IsFull()
    {
        return itemData != null && quantity >= itemData.m_maxStack;
    }

    public bool IsEmpty()
    {
        return itemData == null || quantity <= 0;
    }

    public void Clear()
    {
        itemData = null;
        quantity = 0;
    }
}

[System.Serializable]
public class InventoryData
{
    public InventorySlotData[] slots;

    public InventoryData(int size)
    {
        slots = new InventorySlotData[size];

        //  모든 슬롯을 기본 상태로 초기화 (null 방지)
        for (int i = 0; i < size; i++)
        {
            slots[i] = new InventorySlotData();
        }
    }

    public void AddItem(ItemData data, int amount = 1)
    {
        // 1. 기존 슬롯에 추가
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot.itemData == data && !slot.IsFull())
            {
                int space = data.m_maxStack - slot.quantity;
                int addAmount = Mathf.Min(space, amount);
                slot.quantity += addAmount;
                amount -= addAmount;
                if (amount <= 0) return;
            }
        }

        // 2. 빈 슬롯에 추가
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot.itemData == null)
            {
                int addAmount = Mathf.Min(data.m_maxStack, amount);
                slot.itemData = data;
                slot.quantity = addAmount;
                amount -= addAmount;
                if (amount <= 0) return;
            }
        }

        if (amount > 0)
        {
            Debug.LogWarning("[InventoryData] 인벤토리 가득참. 남은 아이템: " + amount);
        }
    }

    public void RemoveItem(ItemData data, int amount = 1)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot.itemData == data)
            {
                slot.quantity -= amount;
                if (slot.quantity <= 0)
                {
                    slot.Clear(); //  null로 두지 않고 초기화
                }
                return;
            }
        }
    }

    public bool HasItem(ItemData item, int amount)
    {
        if (slots == null)
        {
            return false;
        }

        if (item == null)
        {
            return false;
        }

        int total = 0;
        int index = 0;

        foreach (var slot in slots)
        {
            if (slot == null)
            {
            }
            else if (slot.itemData == null)
            {
            }
            else
            {

                if (slot.itemData == item)
                {
                    total += slot.quantity;
                }
            }

            index++;
        }

        return total >= amount;
    }

    public bool HasSpaceForItem(ItemData item, int amount)
    {
        int remaining = amount;

        foreach (var slot in slots)
        {
            if (slot.itemData == item)
            {
                int space = item.m_maxStack - slot.quantity;
                remaining -= space;
            }
            else if (slot.itemData == null)
            {
                remaining -= item.m_maxStack;
            }

            if (remaining <= 0)
                return true;
        }

        return false;
    }
    public int GetItemCount(ItemData item)
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.itemData == item)
                count += slot.quantity;
        }
        return count;
    }
}
