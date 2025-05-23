using UnityEngine;
using System.Linq;

public static class ItemDB
{
    private static ItemData[] m_allItems;

    static ItemDB()
    {
        m_allItems = Resources.LoadAll<ItemData>("ItemData"); // 경로는 맞게 수정
    }

    public static ItemData GetItemById(string id)
    {
        return m_allItems.FirstOrDefault(item => item.m_itemID == id);
    }
}
