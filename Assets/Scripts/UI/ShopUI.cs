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
    [SerializeField] private ShopSlot m_purchaseSlotPrefab;  // 슬롯 프리팹
    [SerializeField] private Transform m_purchaseSlotGroup;       // 슬롯 부모
    private List<ShopSlot> m_purchaseSlotList = new List<ShopSlot>(); // 생성된 슬롯 리스트


    private List<CraftListUI> slotList = new List<CraftListUI>();

    [Header("탭 관련")]
    [SerializeField] private Image m_purchaseTab;
    [SerializeField] private Image m_sellTab;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private GameObject m_sellGroup;
    [SerializeField] private GameObject m_purchaseGroup;

    private int currentIndex = 0; // 현재 선택 슬롯

    private float holdDelay = 0.5f;    // 처음 눌렀을 때 다음 반복까지 딜레이
    private float repeatRate = 0.1f;   // 연속 입력 간격
    private float holdTimer = 0f;

    private enum TabType { Purchase, Sell }
    private TabType currentTab = TabType.Purchase;
    private int selectedIndex = 0;

    void Start()
    {
        m_shopUI.SetActive(false);
        InitShopUI();
        SwitchTab(TabType.Purchase);
        UpdatePurchaseUI();

    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        HandleSlotMoveInput();
        // 탭 전환
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchTab();
        }
        //액션
        if (Input.GetKeyDown(KeyCode.Z))
        {
        }
    }
    void SwitchTab(TabType tab)
    {
        selectedIndex = 0;
        //SetupList();
        UpdateTabSprites();
    }

    private void HandleSlotMoveInput()
    {
        // 초기화
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

        // 키 안 누르고 있으면 초기화
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
    /// 탭 전환 이미지 변경
    /// </summary>
    private void UpdateTabSprites()
    {
        m_purchaseTab.sprite = (currentTab == TabType.Purchase) ? selectedSprite : defaultSprite;
        m_sellTab.sprite = (currentTab == TabType.Sell) ? selectedSprite : defaultSprite;
    }
    public void SwitchTab()
    {
        // 1. 탭 전환
        currentTab = (currentTab == TabType.Purchase) ? TabType.Sell : TabType.Purchase;

        // 2. 슬롯 그룹 보여주기
        if (currentTab == TabType.Purchase)
        {
            m_purchaseGroup.SetActive(true);
            m_sellGroup.SetActive(false);

            UpdatePurchaseUI(); // 구매탭 업데이트
        }
        else if (currentTab == TabType.Sell)
        {
            m_purchaseGroup.SetActive(false);
            m_sellGroup.SetActive(true);

            UpdateSellUI(); // 판매탭 업데이트
        }

        // 3. 탭 버튼 비주얼 업데이트
        UpdateTabSprites();
        selectedIndex = 0;
        UpdateSlotSelection(); // 선택 UI 갱신도 같이 해주는 게 좋음
    }

    private void UpdatePurchaseUI()
    {
        // 1. 기존 슬롯 다 삭제
        foreach (var slot in m_purchaseSlotList)
        {
            Destroy(slot.gameObject);
        }
        m_purchaseSlotList.Clear();
        // 2. 판매 아이템 리스트를 돌면서 슬롯 생성
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
    private void UpdateSellUI()
    {
        var data = GManager.Instance.IsinvenManager.IsInventoryData;

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

        m_purchaseGroup.SetActive(true); //  강제 비주얼 초기화
        m_sellGroup.SetActive(false);    //  이전 상태 제거

        SwitchTab(currentTab);           // 기존 로직 호출
    }
    private void MoveLeft()
    {
        int rowSize = 8;
        int rowStart = (selectedIndex / rowSize) * rowSize;
        selectedIndex = (selectedIndex == rowStart) ? rowStart + rowSize - 1 : selectedIndex - 1;
    }

    private void MoveRight()
    {
        int rowSize = 8;
        int rowStart = (selectedIndex / rowSize) * rowSize;
        selectedIndex = (selectedIndex == rowStart + rowSize - 1) ? rowStart : selectedIndex + 1;
    }

    private void MoveUp()
    {
        if (selectedIndex - 8 >= 0)
            selectedIndex -= 8;
    }

    private void MoveDown()
    {
        int totalCount = GetCurrentSlotCount();
        if (selectedIndex + 8 < totalCount)
            selectedIndex += 8;
    }


private int GetCurrentSlotCount()
{
    return currentTab == TabType.Purchase ? m_purchaseSlotList.Count : m_sellSlot.Length;
}
    private void UpdateSlotSelection()
    {
        if (currentTab == TabType.Purchase)
        {
            for (int i = 0; i < m_purchaseSlotList.Count; i++)
            {
                bool isSelected = i == selectedIndex;
                m_purchaseSlotList[i].SetSelected(isSelected);
                if (isSelected)
                    Debug.Log($"[상점] 구매 슬롯 {i} 선택됨: {m_purchaseSlotList[i].GetItemName()}");
            }
        }
        else if (currentTab == TabType.Sell)
        {
            for (int i = 0; i < m_sellSlot.Length; i++)
            {
                bool isSelected = i == selectedIndex;
                m_sellSlot[i].SetSelected(isSelected);
                if (isSelected)
                    Debug.Log($"[상점] 판매 슬롯 {i} 선택됨: {m_sellSlot[i].GetItemName()}");
            }
        }
    }

}
