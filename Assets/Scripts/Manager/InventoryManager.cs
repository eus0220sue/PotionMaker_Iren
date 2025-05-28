using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    [SerializeField] private InventoryData InventoryData;
    public InventoryData IsInventoryData => InventoryData; // 읽기 전용 속성으로 외부 접근 허용
    
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

        // 획득 메시지 UI 표시
        GManager.Instance.IsGetMessageUI.AddItemMessage(item.m_itemName, amount);

        SoundManager.Instance.PlaySystemSound(7);
        GManager.Instance.IsQuestManager.TryCompleteStepAll();
    }



    public void RemoveItem(ItemData item, int amount = 1)
    {
        InventoryData.RemoveItem(item, amount);
        GManager.Instance.IsInventoryUI.UpdateUI(); // 추가
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
            Debug.LogWarning("[인벤토리 공간 부족] 아이템을 추가할 수 없습니다.");
            return;
        }
    }
    public void ConsumeItem(ItemData item, int amount)
    {
        CheckInvenItem(item, amount); // 소모 전에 반드시 확인
        InventoryData.RemoveItem(item, amount);
        GManager.Instance.IsInventoryUI.UpdateUI();
    }

    public bool HasItemById(string itemId)
    {
        // ItemDatabase에서 ID로 ItemData 찾아서 확인
        ItemData target = ItemDB.GetItemById(itemId); // 이 부분은 당신의 구조에 따라 구현
        if (target == null) return false;

        return InventoryData.HasItem(target, 1); // 최소 1개 이상 보유 여부
    }
    public int GetItemCount(ItemData item)
    {
        if (InventoryData == null || item == null)
            return 0;

        return InventoryData.GetItemCount(item);
    }

}

