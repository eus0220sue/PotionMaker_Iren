using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] public GameObject Inven;
    [SerializeField] public InventorySlot[] m_invenSlot;
    [SerializeField] public InventorySlot[] m_quickSlot;

    [SerializeField] public bool isOpen = false;

    private int currentInventoryIndex = 0; // �κ��丮 ���� ���� ����

    private int currentQuickIndex = 0;


    public void Start()
    {
        Inven.SetActive(false);
        GManager.Instance.SetInventoryUI(this); // GManager���� ���� ����
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
        // �κ��丮�� ���������� �׳� �ݱ� (�׻� ���)
        if (isOpen)
        {
            isOpen = false;
            Inven.SetActive(false);
            Debug.Log("�κ��丮 ����");
            return;
        }

        // UI�� ���������� �κ��丮 ���� ����
        if (GManager.Instance.IsUIManager.UIOpenFlag)
        {
            Debug.Log("�ٸ� UI�� �����־� �κ��丮 ���� ����");
            return;
        }

        // �κ��丮 ����
        isOpen = true;
        Inven.SetActive(true);
        Debug.Log("�κ��丮 ����");
    }



    public void UpdateUI()
    {
        var data = GManager.Instance.IsInvenManager.IsInventoryData;

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
        Debug.Log($"[������] {index + 1}�� ���� ���õ�");

        // ���� ���̶���Ʈ ����
        for (int i = 0; i < m_quickSlot.Length; i++)
        {
            if (i == index)
            {
                m_quickSlot[i].SetSelected(true); // ���õ� ���� ǥ��
            }
            else
            {
                m_quickSlot[i].SetSelected(false);
            }
        }
    }
    private void HandleInventoryInput()
    {
        switch (true)
        {
            case bool _ when Input.GetKeyDown(KeyCode.LeftArrow):
                MoveLeft();
                break;
            case bool _ when Input.GetKeyDown(KeyCode.RightArrow):
                MoveRight();
                break;
            case bool _ when Input.GetKeyDown(KeyCode.UpArrow):
                MoveUp();
                break;
            case bool _ when Input.GetKeyDown(KeyCode.DownArrow):
                MoveDown();
                break;
        }

        UpdateInventorySlotSelection();
    }

    private void MoveLeft()
    {
        int rowStart = (currentInventoryIndex / 8) * 8; // ���� �� ���� ��ȣ

        if (currentInventoryIndex == rowStart)
            currentInventoryIndex = rowStart + 7; // �� ù ��° �����̸� �� ����������
        else
            currentInventoryIndex -= 1;
    }
    private void MoveRight()
    {
        int rowStart = (currentInventoryIndex / 8) * 8; // ���� �� ���� ��ȣ

        if (currentInventoryIndex == rowStart + 7)
            currentInventoryIndex = rowStart; // �� ������ �����̸� �� ù ��°��
        else
            currentInventoryIndex += 1;
    }
    private void MoveUp()
    {
        if (currentInventoryIndex - 8 >= 0)
            currentInventoryIndex -= 8;
        else
        {
            // �� �����̸� �׳� ����
        }
    }
    private void MoveDown()
    {
        if (currentInventoryIndex + 8 < m_invenSlot.Length)
            currentInventoryIndex += 8;
        else
        {
            // �� �Ʒ����̸� �׳� ����
        }
    }
    private void UpdateInventorySlotSelection()
    {
        for (int i = 0; i < m_invenSlot.Length; i++)
        {
            if (i == currentInventoryIndex)
                m_invenSlot[i].SetSelected(true); // ����
            else
                m_invenSlot[i].SetSelected(false); // ����
        }
    }
}

