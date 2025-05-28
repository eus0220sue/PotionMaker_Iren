using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{

    [SerializeField] private GameObject m_shopUI;
    [SerializeField] private ExchangeManager m_exchangeManager;
    [SerializeField] private List<ItemData> m_shopGoodsList;
    [SerializeField] private InventorySlot[] m_sellSlot;
    [SerializeField] private ShopSlot m_purchaseSlotPrefab;  // ���� ������
    [SerializeField] private Transform m_purchaseSlotGroup;       // ���� �θ�
    private List<ShopSlot> m_purchaseSlotList = new List<ShopSlot>(); // ������ ���� ����Ʈ


    private List<CraftListUI> slotList = new List<CraftListUI>();

    [Header("�� ����")]
    [SerializeField] private Image m_purchaseTab;
    [SerializeField] private Image m_sellTab;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private GameObject m_sellGroup;
    [SerializeField] private GameObject m_purchaseGroup;

    public int currentIndex = 0; // ���� ���� ����

    public float holdDelay = 0.5f;    // ó�� ������ �� ���� �ݺ����� ������
    public float repeatRate = 0.1f;   // ���� �Է� ����
    public float holdTimer = 0f;

    public enum TabType { Purchase, Sell }
    public TabType currentTab = TabType.Purchase;
    public int selectedIndex = 0;

    void Start()
    {
        m_shopUI.SetActive(false);
        InitShopUI();
        SwitchTab(TabType.Purchase);
        UpdatePurchaseUI();

    }
    public void Update()
    {
        if (!gameObject.activeSelf) return;
        HandleSlotMoveInput();
        // �� ��ȯ
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchTab();
        }
        //�׼�
        if (Input.GetKeyDown(KeyCode.Z))
        {
        }
    }
    public void SwitchTab(TabType tab)
    {
        selectedIndex = 0;
        //SetupList();
        UpdateTabSprites();
    }
    public void HandleSlotMoveInput()
    {
        // �ʱ�ȭ
        holdTimer -= Time.deltaTime;

        // W
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveUp();
            holdTimer = holdDelay;
            return;
        }
        else if (Input.GetKey(KeyCode.UpArrow) && holdTimer <= 0f)
        {
            MoveUp();
            holdTimer = repeatRate;
            return;
        }

        // S
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveDown();
            holdTimer = holdDelay;
            return;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && holdTimer <= 0f)
        {
            MoveDown();
            holdTimer = repeatRate;
            return;
        }

        // A
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLeft();
            holdTimer = holdDelay;
            return;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && holdTimer <= 0f)
        {
            MoveLeft();
            holdTimer = repeatRate;
            return;
        }

        // D
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveRight();
            holdTimer = holdDelay;
            return;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && holdTimer <= 0f)
        {
            MoveRight();
            holdTimer = repeatRate;
            return;
        }

        // Ű �� ������ ������ �ʱ�ȭ
        if (!Input.GetKey(KeyCode.UpArrow) &&
            !Input.GetKey(KeyCode.DownArrow) &&
            !Input.GetKey(KeyCode.LeftArrow) &&
            !Input.GetKey(KeyCode.RightArrow))
        {
            holdTimer = 0f;
        }
        UpdateSlotSelection();

    }


    /// <summary>
    /// �� ��ȯ �̹��� ����
    /// </summary>
    private void UpdateTabSprites()
    {
        m_purchaseTab.sprite = (currentTab == TabType.Purchase) ? selectedSprite : defaultSprite;
        m_sellTab.sprite = (currentTab == TabType.Sell) ? selectedSprite : defaultSprite;
    }
    public void SwitchTab()
    {
        // 1. �� ��ȯ
        currentTab = (currentTab == TabType.Purchase) ? TabType.Sell : TabType.Purchase;

        // 2. ���� �׷� �����ֱ�
        if (currentTab == TabType.Purchase)
        {
            m_purchaseGroup.SetActive(true);
            m_sellGroup.SetActive(false);

            UpdatePurchaseUI(); // ������ ������Ʈ
        }
        else if (currentTab == TabType.Sell)
        {
            m_purchaseGroup.SetActive(false);
            m_sellGroup.SetActive(true);

            UpdateSellUI(); // �Ǹ��� ������Ʈ
        }

        // 3. �� ��ư ���־� ������Ʈ
        UpdateTabSprites();
        selectedIndex = 0;
        UpdateSlotSelection(); // ���� UI ���ŵ� ���� ���ִ� �� ����
    }

    public void UpdatePurchaseUI()
    {
        // 1. ���� ���� �� ����
        foreach (var slot in m_purchaseSlotList)
        {
            Destroy(slot.gameObject);
        }
        m_purchaseSlotList.Clear();
        // 2. �Ǹ� ������ ����Ʈ�� ���鼭 ���� ����
        foreach (var item in m_shopGoodsList)
        {
            if (item != null)
            {
                ShopSlot slot = Instantiate(m_purchaseSlotPrefab, m_purchaseSlotGroup);
                slot.Set(item);
                m_purchaseSlotList.Add(slot);
            }
        }
    }
    public void UpdateSellUI()
    {
        var data = GManager.Instance.IsInvenManager.IsInventoryData;

        if (data == null || data.slots == null)return;
        for (int i = 0; i < m_sellSlot.Length; i++)
        {
            if (i < data.slots.Length)
            {
                var slotData = data.slots[i];

                if (slotData == null)
                {
                }
                else if (slotData.itemData == null)
                {
                }
                else
                {
                    m_sellSlot[i].SetSlot(slotData.itemData, slotData.quantity);
                }
            }
        }

    }
    public void InitShopUI()
    {
        currentTab = TabType.Purchase;
        selectedIndex = 0;

        m_purchaseGroup.SetActive(true); //  ���� ���־� �ʱ�ȭ
        m_sellGroup.SetActive(false);    //  ���� ���� ����

        SwitchTab(currentTab);           // ���� ���� ȣ��
    }
    public void MoveLeft()
    {
        int rowSize = 8;
        int rowStart = (selectedIndex / rowSize) * rowSize;
        selectedIndex = (selectedIndex == rowStart) ? rowStart + rowSize - 1 : selectedIndex - 1;
    }

    public void MoveRight()
    {
        int rowSize = 8;
        int rowStart = (selectedIndex / rowSize) * rowSize;
        selectedIndex = (selectedIndex == rowStart + rowSize - 1) ? rowStart : selectedIndex + 1;
    }

    public void MoveUp()
    {
        if (selectedIndex - 8 >= 0)
            selectedIndex -= 8;
    }

    public void MoveDown()
    {
        int totalCount = GetCurrentSlotCount();
        if (selectedIndex + 8 < totalCount)
            selectedIndex += 8;
    }


    public int GetCurrentSlotCount()
    {
        return currentTab == TabType.Purchase ? m_purchaseSlotList.Count : m_sellSlot.Length;
    }
    public void UpdateSlotSelection()
    {
        if (currentTab == TabType.Purchase)
        {
            for (int i = 0; i < m_purchaseSlotList.Count; i++)
            {
                bool isSelected = i == selectedIndex;
                m_purchaseSlotList[i].SetSelected(isSelected);
                if (isSelected)
                    Debug.Log($"[����] ���� ���� {i} ���õ�: {m_purchaseSlotList[i].GetItemName()}");
            }
        }
        else if (currentTab == TabType.Sell)
        {
            for (int i = 0; i < m_sellSlot.Length; i++)
            {
                bool isSelected = i == selectedIndex;
                m_sellSlot[i].SetSelected(isSelected);
                if (isSelected)
                    Debug.Log($"[����] �Ǹ� ���� {i} ���õ�: {m_sellSlot[i].GetItemName()}");
            }
        }
    }

}
