using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    [SerializeField] private InventoryData InventoryData;
    public InventoryData IsInventoryData => InventoryData; // �б� ���� �Ӽ����� �ܺ� ���� ���
    
    [SerializeField] private int inventorySize = 24;

    private void Awake()
    {
        if (InventoryData == null || InventoryData.slots == null || InventoryData.slots.Length != inventorySize)
        {
            InventoryData = new InventoryData(inventorySize);
        }
    }
    public void AddItem(ItemData item, int amount = 1)
    {
        InventoryData.AddItem(item, amount);
        GManager.Instance.IsInventoryUI.UpdateUI();

        // ȹ�� �޽��� UI ǥ��
        GManager.Instance.IsGetMessageUI.AddItemMessage(item.m_itemName, amount);

        SoundManager.Instance.PlaySystemSound(7);
        GManager.Instance.IsQuestManager.TryCompleteStepAll();
    }



    public void RemoveItem(ItemData item, int amount = 1)
    {
        InventoryData.RemoveItem(item, amount);
        GManager.Instance.IsInventoryUI.UpdateUI(); // �߰�
    }

    public void CheckInvenItem(ItemData item, int amount)
    {
        if (!InventoryData.HasItem(item, amount))
        {
            return;
        }

    }
    public void CheckInvenSpace(ItemData item, int amount)
    {
        if (!InventoryData.HasSpaceForItem(item, amount))
        {
            Debug.LogWarning("[�κ��丮 ���� ����] �������� �߰��� �� �����ϴ�.");
            return;
        }
    }
    public void ConsumeItem(ItemData item, int amount)
    {
        CheckInvenItem(item, amount); // �Ҹ� ���� �ݵ�� Ȯ��
        InventoryData.RemoveItem(item, amount);
        GManager.Instance.IsInventoryUI.UpdateUI();
    }

    public bool HasItemById(string itemId)
    {
        // ItemDatabase���� ID�� ItemData ã�Ƽ� Ȯ��
        ItemData target = ItemDB.GetItemById(itemId); // �� �κ��� ����� ������ ���� ����
        if (target == null) return false;

        return InventoryData.HasItem(target, 1); // �ּ� 1�� �̻� ���� ����
    }
    public int GetItemCount(ItemData item)
    {
        if (InventoryData == null || item == null)
            return 0;

        return InventoryData.GetItemCount(item);
    }

}

