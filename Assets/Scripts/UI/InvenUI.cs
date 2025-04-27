using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] public GameObject Inven;
    [SerializeField] public InventorySlot[] m_invenSlot;
    [SerializeField] public InventorySlot[] m_quickSlot;

    [SerializeField] public bool isOpen = false;

    private int currentInventoryIndex = 0; // 인벤토리 현재 선택 슬롯

    private int currentQuickIndex = 0;


    public void Start()
    {
        Inven.SetActive(false);
        GManager.Instance.SetInventoryUI(this); // GManager에서 참조 가능
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        { 
            OpenInventory();
        }
        HandleQuickSlotInput();
        if (isOpen)
        {
            HandleInventoryInput();
        }
    }

    public void OpenInventory()
    {
        isOpen = !isOpen;
        Inven.SetActive(isOpen);
        Debug.Log($"인벤토리 상태: {isOpen}");

    }


    public void UpdateUI()
    {
        var data = GManager.Instance.IsinvenManager.IsInventoryData;

        for (int i = 0; i < m_invenSlot.Length; i++)
        {
            if (i < data.slots.Length && data.slots[i] != null)
            {
                m_invenSlot[i].SetSlot(data.slots[i].itemData, data.slots[i].quantity);
            }
            else
            {
                m_invenSlot[i].ClearSlot();
            }
        }
        for (int i = 0; i < m_quickSlot.Length; i++)
        {
            if (i < data.slots.Length && data.slots[i] != null)
            {
                m_quickSlot[i].SetSlot(data.slots[i].itemData, data.slots[i].nowQuantity);
            }
            else
            {
                m_quickSlot[i].ClearSlot();
            }
        }
    }

    private void HandleQuickSlotInput()
    {
        for (int i = 0; i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetQuickSlotIndex(i);
            }
        }
    }

    private void SetQuickSlotIndex(int index)
    {
        currentQuickIndex = index;
        Debug.Log($"[퀵슬롯] {index + 1}번 슬롯 선택됨");

        // 슬롯 하이라이트 갱신
        for (int i = 0; i < m_quickSlot.Length; i++)
        {
            if (i == index)
            {
                m_quickSlot[i].SetSelected(true); // 선택된 슬롯 표시
            }
            else
            {
                m_quickSlot[i].SetSelected(false);
            }
        }
    }
    private void HandleInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            MoveUp();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            MoveDown();
        }

        UpdateInventorySlotSelection();
    }
    private void MoveLeft()
    {
        int rowStart = (currentInventoryIndex / 8) * 8; // 현재 줄 시작 번호

        if (currentInventoryIndex == rowStart)
            currentInventoryIndex = rowStart + 7; // 줄 첫 번째 슬롯이면 → 마지막으로
        else
            currentInventoryIndex -= 1;
    }
    private void MoveRight()
    {
        int rowStart = (currentInventoryIndex / 8) * 8; // 현재 줄 시작 번호

        if (currentInventoryIndex == rowStart + 7)
            currentInventoryIndex = rowStart; // 줄 마지막 슬롯이면 → 첫 번째로
        else
            currentInventoryIndex += 1;
    }
    private void MoveUp()
    {
        if (currentInventoryIndex - 8 >= 0)
            currentInventoryIndex -= 8;
        else
        {
            // 맨 위줄이면 그냥 유지
        }
    }
    private void MoveDown()
    {
        if (currentInventoryIndex + 8 < m_invenSlot.Length)
            currentInventoryIndex += 8;
        else
        {
            // 맨 아랫줄이면 그냥 유지
        }
    }
    private void UpdateInventorySlotSelection()
    {
        for (int i = 0; i < m_invenSlot.Length; i++)
        {
            if (i == currentInventoryIndex)
                m_invenSlot[i].SetSelected(true); // 선택
            else
                m_invenSlot[i].SetSelected(false); // 비선택
        }
    }
}

