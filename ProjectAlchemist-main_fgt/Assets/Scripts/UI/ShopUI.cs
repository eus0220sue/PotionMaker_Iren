using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
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


    private float holdDelay = 0.2f;
    private float holdTimer = 0f;
    private enum TabType { Purchase, Sell }
    private TabType currentTab = TabType.Purchase;
    private int selectedIndex = 0;


    private void Update()
    {
        if (!gameObject.activeSelf) return;
        HandleSlotMoveInput();
        // 탭 전환
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchTab();
            //currentTab = (currentTab == TabType.Purchase) ? TabType.Sell : TabType.Purchase;
            //SwitchTab(currentTab);
        }
        // 제작 실행 (Z키)
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
        // 1. 먼저 GetKeyDown 처리 (처음 누른 순간만 반응)
        if (Input.GetKeyDown(KeyCode.W))
        {
            MoveSlot(-1);
            holdTimer = holdDelay;
            return; //  중복 입력 방지
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            MoveSlot(1);
            holdTimer = holdDelay;
            return;
        }

        // 2. 누르고 있는 상태 처리 (Hold)
        if (Input.GetKey(KeyCode.W))
        {
            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                MoveSlot(-1);
                holdTimer = holdDelay;
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                MoveSlot(1);
                holdTimer = holdDelay;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }
    void MoveSlot(int dir)
    {
        selectedIndex = Mathf.Clamp(selectedIndex + dir, 0, slotList.Count - 1);

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
    }

    private void UpdatePurchaseUI()
    {
        // 1. 기존 슬롯 다 삭제
        foreach (var slot in m_purchaseSlotList)
        {
            Destroy(slot.gameObject);
        }
        m_purchaseSlotList.Clear();
        Debug.Log("[ShopUI] 기존 구매 슬롯 모두 삭제 완료");

        // 2. 판매 아이템 리스트를 돌면서 슬롯 생성
        foreach (var item in m_shopGoodsList)
        {
            if (item != null)
            {
                ShopSlot slot = Instantiate(m_purchaseSlotPrefab, m_purchaseSlotGroup);
                slot.Set(item);
                m_purchaseSlotList.Add(slot);

                //  디버그 로그 추가
                Debug.Log($"[ShopUI] 구매 슬롯 생성 완료 - 아이템: {item.m_itemName}");
            }
            else
            {
                //  아이템이 null이면 경고 로그
                Debug.LogWarning("[ShopUI] m_shopGoodsList에 null 항목이 있습니다.");
            }
        }

        Debug.Log($"[ShopUI] 구매 슬롯 세팅 완료 - 총 {m_purchaseSlotList.Count}개 생성됨");
    }



    private void UpdateSellUI()
    {
        var data = GManager.Instance.IsinvenManager.IsInventoryData;

        for (int i = 0; i < m_sellSlot.Length; i++)
        {
            if (i < data.slots.Length && data.slots[i] != null && data.slots[i].itemData != null)
            {
                m_sellSlot[i].SetSlot(data.slots[i].itemData, data.slots[i].quantity);
            }
            else
            {
                m_sellSlot[i].ClearSlot();
            }
        }
    }

}
