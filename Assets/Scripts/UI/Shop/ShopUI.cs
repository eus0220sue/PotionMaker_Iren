using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject m_shopUI;

    [Header("구매 목록")]
    [SerializeField] private List<ItemData> m_shopGoodsList;
    [SerializeField] private ShopSlot m_purchaseSlotPrefab;   // 슬롯 프리팹
    [SerializeField] private Transform m_purchaseSlotGroup;   // 슬롯 부모
    private readonly List<ShopSlot> m_purchaseSlotList = new(); // 동적 생성 슬롯

    [Header("판매 목록(인벤토리 미러링)")]
    [SerializeField] private InventorySlot[] m_sellSlot;

    [Header("탭 관련")]
    [SerializeField] private Image m_purchaseTab;
    [SerializeField] private Image m_sellTab;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private GameObject m_sellGroup;
    [SerializeField] private GameObject m_purchaseGroup;

    [Header("팝업 패널")]
    //팝업 패널 게임 오브젝트[패널]
    [SerializeField] private GameObject m_popupPanel;     // POPUPpanel (루트
    //수량

    //팝업 UI[실제 뜨는 거]
    [SerializeField] private GameObject m_quantityPopup;    // UI

    //텍스트 리스트 
    [SerializeField] private TMP_Text m_askText;        // Image/AskText->구매or 판매하시겠습니까?
    [SerializeField] private TMP_Text m_qtyText;        // Image/QtyText  -> "현재/최대"
    [SerializeField] private TMP_Text m_priceText;      // Image/PriceText (총액/단가 표시)

    //--추가해야하는거 선택되었을때 이미지, 아닐때 이미지+ 바뀌는 텍스트 색
    [SerializeField] private Sprite m_selSprite; //선택시 이미지
    [SerializeField] private Sprite m_normSprite; //비선택시 이미지


    [SerializeField] private Color m_selTextColor = Color.black;
    [SerializeField] private Color m_normTextColor = Color.white;

    [SerializeField] private Image m_btnConfirm;     // Image/Btn/PurchaseSellBtn->구매or 판매 확정 버튼
    [SerializeField] private TMP_Text m_btnConfirmLabel;// ↑ 버튼 내부 텍스트(TMP)-> 구매하기or 판매하기
    [SerializeField] private Image m_btnClose;       // Image/Btn/CloseBtn->취소
    [SerializeField] private TMP_Text m_btnCloseLabel; // POPUPpanel/Image/Btn/CloseBtn 하위 TMP_Text


    //아이템[이름&이미지]
    [SerializeField] private TMP_Text m_itemNameText;   // Image/ItemInfo/ItemName
    [SerializeField] private Image m_itemImg;        // Image/ItemInfo/ItemImg


    // 팝업 내 선택지 선택 인덱스 
    private int m_popupSelIndex = 0; // 0=확정(구매/판매), 1=취소
    // 현재 탭(구매/판매)에 맞춘 라벨 텍스트 + 선택 초기화
    private bool IsBuyTab => currentTab == TabType.Purchase;


    private enum QtyContext { None, Buy, Sell }
    private ExchangeManager m_exchangeManager;

    // 수량 팝업 상태
    private bool m_isQtyPopupOpen = false;
    private int m_qtyCurrent = 1;
    private int m_qtyMax = 1;
    private QtyContext m_qtyContext = QtyContext.None;

    // 보류 대상
    private ItemData m_pendingItem = null;
    private InventorySlot m_pendingSellSlot = null;
    private ShopSlot m_pendingBuySlot = null;

    // 이동/선택
    public enum TabType { Purchase, Sell }
    public TabType currentTab = TabType.Purchase;
    public int selectedIndex = 0;

    // 키 홀드 가속
    public float holdDelay = 0.5f;    // 처음 눌렀을 때 다음 반복까지 딜레이
    public float repeatRate = 0.1f;   // 연속 입력 간격
    public float holdTimer = 0f;

    private void OnEnable()
    {
        if (m_exchangeManager == null)
            m_exchangeManager = GManager.Instance?.IsExchangeManager;

        // 다중 인스턴스 탐지
        var all = FindObjectsOfType<ExchangeManager>(true);
        if (all.Length > 1)
            Debug.LogWarning($"[ShopUI] ExchangeManager가 {all.Length}개 있습니다. 참조 인스턴스={m_exchangeManager?.name}({m_exchangeManager?.GetInstanceID()})");
    }


    private void Start()
    {
        if (m_shopUI != null) m_shopUI.SetActive(false);

        InitShopUI();
        SwitchTab(TabType.Purchase);
        UpdatePurchaseUI();
        UpdateSellUI();
        UpdateSlotSelection();
    }

    private void Update()
    {
        // 팝업이 열려있으면 팝업 입력만 처리
        if (m_isQtyPopupOpen)
        {
            HandleQuantityPopupInput();
            return;
        }

        // 상점 UI가 꺼져있으면 입력 무시
        if (m_shopUI == null || !m_shopUI.activeSelf) return;

        HandleSlotMoveInput();

        // 탭 전환
        if (Input.GetKeyDown(KeyCode.Tab))
            SwitchTab();

        // 액션
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentTab == TabType.Purchase)
                BuySelected();   // 팝업 오픈
            else
                SellSelected();  // 팝업 오픈
        }
    }

    // 외부에서 열고 닫는 함수 (원하면 버튼에 연결)
    public void OpenShop()
    {
        if (m_shopUI != null) m_shopUI.SetActive(true);
        UpdatePurchaseUI();
        UpdateSellUI();
        UpdateSlotSelection();
    }

    public void CloseShop()
    {
        if (m_shopUI != null) m_shopUI.SetActive(false);
        CloseQuantityPopup();
    }

    /// <summary>
    /// 상점 상태 초기화 
    /// </summary>
    public void InitShopUI()
    {
        currentTab = TabType.Purchase;
        selectedIndex = 0;

        // 비주얼 초기화
        if (m_purchaseGroup != null) m_purchaseGroup.SetActive(true);
        if (m_sellGroup != null) m_sellGroup.SetActive(false);

        UpdateTabSprites();
    }

    /// <summary>
    /// 탭 전환
    /// </summary>
    /// <param name="tab"></param>
    public void SwitchTab(TabType tab)
    {
        currentTab = tab;
        selectedIndex = 0;
        UpdateTabSprites();

        if (currentTab == TabType.Purchase)
        {
            if (m_purchaseGroup) m_purchaseGroup.SetActive(true);
            if (m_sellGroup) m_sellGroup.SetActive(false);
            UpdatePurchaseUI();
        }
        else
        {
            if (m_purchaseGroup) m_purchaseGroup.SetActive(false);
            if (m_sellGroup) m_sellGroup.SetActive(true);
            UpdateSellUI();
        }
        UpdateSlotSelection();
    }

    public void SwitchTab()
    {
        SwitchTab(currentTab == TabType.Purchase ? TabType.Sell : TabType.Purchase);
    }

    private void UpdateTabSprites()
    {
        if (m_purchaseTab) m_purchaseTab.sprite = (currentTab == TabType.Purchase) ? selectedSprite : defaultSprite;
        if (m_sellTab) m_sellTab.sprite = (currentTab == TabType.Sell) ? selectedSprite : defaultSprite;
    }

    public void UpdatePurchaseUI()
    {
        // 기존 슬롯 제거
        foreach (var slot in m_purchaseSlotList)
            if (slot) Destroy(slot.gameObject);
        m_purchaseSlotList.Clear();

        // 새로 생성
        if (m_shopGoodsList != null)
        {
            foreach (var item in m_shopGoodsList)
            {
                if (!item) continue;
                var slot = Instantiate(m_purchaseSlotPrefab, m_purchaseSlotGroup);
                slot.Set(item);
                m_purchaseSlotList.Add(slot);
            }
        }

        // 선택 보정
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, m_purchaseSlotList.Count - 1));
    }

    public void UpdateSellUI()
    {
        var data = GManager.Instance?.IsInvenManager?.IsInventoryData;
        if (data == null || data.slots == null || m_sellSlot == null) return;

        for (int i = 0; i < m_sellSlot.Length; i++)
        {
            if (!m_sellSlot[i]) continue;

            if (i < data.slots.Length && data.slots[i] != null && data.slots[i].itemData != null)
                m_sellSlot[i].SetSlot(data.slots[i].itemData, data.slots[i].quantity);
            else
                m_sellSlot[i].SetSlot(null, 0); // 비우기
        }

        // 선택 보정
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, m_sellSlot.Length - 1));
    }

    private void UpdateSlotSelection()
    {
        if (currentTab == TabType.Purchase)
        {
            for (int i = 0; i < m_purchaseSlotList.Count; i++)
            {
                bool selected = (i == selectedIndex);
                m_purchaseSlotList[i].SetSelected(selected);
            }
        }
        else
        {
            for (int i = 0; i < m_sellSlot.Length; i++)
            {
                if (!m_sellSlot[i]) continue;
                bool selected = (i == selectedIndex);
                m_sellSlot[i].SetSelected(selected);
            }
        }
    }

    private int GetCurrentSlotCount()
    {
        return currentTab == TabType.Purchase ? m_purchaseSlotList.Count : (m_sellSlot != null ? m_sellSlot.Length : 0);
    }

    private void HandleSlotMoveInput()
    {
        holdTimer -= Time.deltaTime;

        // Up
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveUp();
            holdTimer = holdDelay; return;
        }
        else if (Input.GetKey(KeyCode.UpArrow) && holdTimer <= 0f)
        {
            MoveUp();
            holdTimer = repeatRate; return;
        }

        // Down
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveDown();
            holdTimer = holdDelay; return;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && holdTimer <= 0f)
        {
            MoveDown();
            holdTimer = repeatRate; return;
        }

        // Left
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLeft();
            holdTimer = holdDelay; return;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && holdTimer <= 0f)
        {
            MoveLeft();
            holdTimer = repeatRate; return;
        }

        // Right
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveRight();
            holdTimer = holdDelay; return;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && holdTimer <= 0f)
        {
            MoveRight();
            holdTimer = repeatRate; return;
        }

        // 아무 키도 안 누르면 타이머 초기화
        if (!Input.GetKey(KeyCode.UpArrow) &&
            !Input.GetKey(KeyCode.DownArrow) &&
            !Input.GetKey(KeyCode.LeftArrow) &&
            !Input.GetKey(KeyCode.RightArrow))
        {
            holdTimer = 0f;
        }

        UpdateSlotSelection();
    }

    private void MoveLeft()
    {
        int rowSize = 8;
        int rowStart = (selectedIndex / rowSize) * rowSize;
        int rowEnd = rowStart + rowSize - 1;
        selectedIndex = (selectedIndex == rowStart) ? rowEnd : selectedIndex - 1;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, GetCurrentSlotCount() - 1);
    }

    private void MoveRight()
    {
        int rowSize = 8;
        int rowStart = (selectedIndex / rowSize) * rowSize;
        int rowEnd = rowStart + rowSize - 1;
        selectedIndex = (selectedIndex == rowEnd) ? rowStart : selectedIndex + 1;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, GetCurrentSlotCount() - 1);
    }

    private void MoveUp()
    {
        if (selectedIndex - 8 >= 0)
            selectedIndex -= 8;
        UpdateSlotSelection();
    }

    private void MoveDown()
    {
        int total = GetCurrentSlotCount();
        if (selectedIndex + 8 < total)
            selectedIndex += 8;
        UpdateSlotSelection();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 구매/판매 선택 → 수량 팝업 오픈
    // ─────────────────────────────────────────────────────────────────────────────

    public void BuySelected()
    {
        Debug.Log("[ShopUI] BuySelected() entered");

        // 필수 진단 로그
        var slot = (selectedIndex >= 0 && selectedIndex < m_purchaseSlotList.Count) ? m_purchaseSlotList[selectedIndex] : null;
        var item = slot?.GetItemData();
        int unit = (m_exchangeManager && item) ? m_exchangeManager.GetBuyPrice(item) : -1;
        int gold = m_exchangeManager ? m_exchangeManager.GetPlayerGold() : -1;
        Debug.Log($"[ShopUI] diag | EM={(m_exchangeManager ? m_exchangeManager.name : "null")} gold={gold} unit={unit} item={(item ? item.m_itemName : "null")}");

        if (m_purchaseSlotList == null || m_purchaseSlotList.Count == 0) return;
        if (selectedIndex < 0 || selectedIndex >= m_purchaseSlotList.Count) return;
        if (item == null) return;

        int affordableMax = ComputeAffordableMax(item);
        if (affordableMax <= 0)
        {
            Debug.Log($"[상점] 골드 부족으로 구매 불가 | 보유:{gold} | 필요(1개):{Mathf.Max(0, unit)} | 아이템:{item.m_itemName}");
            return;
        }
        OpenQuantityPopup(item, affordableMax, null, slot);
    }


    public void SellSelected()
    {
        if (m_sellSlot == null || m_sellSlot.Length == 0) return;
        if (selectedIndex < 0 || selectedIndex >= m_sellSlot.Length) return;

        var slot = m_sellSlot[selectedIndex];
        var item = slot?.GetItemData();
        int have = slot != null ? slot.GetQuantity() : 0;
        if (item == null || have <= 0) return;

        OpenQuantityPopup(item, have, slot, null);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 수량 팝업 로직 (공용)
    // ─────────────────────────────────────────────────────────────────────────────

    private int ComputeAffordableMax(ItemData item)
    {
        if (!m_exchangeManager || item == null) return 0;
        int unit = Mathf.Max(0, m_exchangeManager.GetBuyPrice(item));
        int gold = Mathf.Max(0, m_exchangeManager.GetPlayerGold());
        if (unit <= 0) return 0;               // 무료거나 가격 이상 → 0 처리
        int affordable = gold / unit;
        return Mathf.Max(0, affordable);       // ★ 0 허용
    }




    private void OpenQuantityPopup(ItemData item, int max, InventorySlot sellSlot, ShopSlot buySlot)
    {
        m_qtyContext = IsBuyTab ? QtyContext.Buy : QtyContext.Sell;
        m_pendingItem = item;
        m_pendingSellSlot = sellSlot;
        m_pendingBuySlot = buySlot;

        m_qtyMax = Mathf.Max(1, max);
        m_qtyCurrent = 1;

        // 텍스트/아이콘/수량/가격 갱신
        RefreshPopupTexts(item);
        UpdateQtyAndPriceVisuals();

        // 기본 선택 = 확정(0)
        m_popupSelIndex = 0;
        ApplySelectVisuals();

        if (m_popupPanel) m_popupPanel.SetActive(true);
        if (m_quantityPopup) m_quantityPopup.SetActive(true); // 실제 표시 오브젝트라면
        m_isQtyPopupOpen = true;
    }

    private void CloseQuantityPopup()
    {
        if (m_popupPanel) m_popupPanel.SetActive(false);
        if (m_quantityPopup) m_quantityPopup.SetActive(false);
        m_isQtyPopupOpen = false;

        m_qtyContext = QtyContext.None;
        m_pendingItem = null;
        m_pendingSellSlot = null;
        m_pendingBuySlot = null;
    }

    private void UpdateQuantityText()
    {
        if (m_qtyText) m_qtyText.text = $"{m_qtyCurrent}/{m_qtyMax}";
    }


    private void ApplySelectVisuals()
    {
        bool selConfirm = (m_popupSelIndex == 0);

        // 배경 이미지 스프라이트 교체 (스프라이트가 있으면 사용)
        if (m_btnConfirm)
        {
            if (selConfirm && m_selSprite) m_btnConfirm.sprite = m_selSprite;
            else if (!selConfirm && m_normSprite) m_btnConfirm.sprite = m_normSprite;
            // 스프라이트가 없을 경우를 대비하여 알파/색만 살짝 강조하고 싶으면 여기에 추가 가능
        }

        if (m_btnClose)
        {
            if (!selConfirm && m_selSprite) m_btnClose.sprite = m_selSprite;
            else if (selConfirm && m_normSprite) m_btnClose.sprite = m_normSprite;
        }

        // 라벨 색상
        if (m_btnConfirmLabel) m_btnConfirmLabel.color = selConfirm ? m_selTextColor : m_normTextColor;
        if (m_btnCloseLabel) m_btnCloseLabel.color = selConfirm ? m_normTextColor : m_selTextColor;
    }

    private void RefreshPopupTexts(ItemData item)
    {
        string verb = IsBuyTab ? "구매" : "판매";

        if (m_askText) m_askText.text = $"{verb}하시겠습니까?";
        if (m_btnConfirmLabel) m_btnConfirmLabel.text = $"{verb}하기";

        // 아이템명/아이콘
        if (m_itemNameText) m_itemNameText.text = item ? item.m_itemName : "";
        if (m_itemImg)
        {
            if (item && item.m_itemIcon) { m_itemImg.sprite = item.m_itemIcon; m_itemImg.enabled = true; }
            else { m_itemImg.sprite = null; m_itemImg.enabled = false; }
        }
    }
    // 수량/가격 텍스트
    private void UpdateQtyAndPriceVisuals()
    {
        string q = $"{m_qtyCurrent}/{m_qtyMax}";
        if (m_qtyText) m_qtyText.text = q;

        if (m_priceText && m_pendingItem && m_exchangeManager)
        {
            int unit = IsBuyTab ? m_exchangeManager.GetBuyPrice(m_pendingItem)
                                 : m_exchangeManager.GetSellPrice(m_pendingItem);
            int total = Mathf.Max(0, unit) * Mathf.Max(0, m_qtyCurrent);
            m_priceText.text = IsBuyTab ? $"총액 {total} (단가 {unit})" : $"수익 {total} (단가 {unit})";
        }
    }

    private void HandleQuantityPopupInput()
    {
        // A: 감소 (1에서 누르면 구매=보유골드 최대 / 판매=소지개수 최대)
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (m_qtyCurrent == 1)
            {
                if (m_qtyContext == QtyContext.Buy)
                    m_qtyMax = Mathf.Max(1, ComputeAffordableMax(m_pendingItem));
                m_qtyCurrent = m_qtyMax;
            }
            else
            {
                m_qtyCurrent = Mathf.Max(1, m_qtyCurrent - 1);
            }
            UpdateQtyAndPriceVisuals();
        }

        // D: 증가 (최대 초과 금지)
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (m_qtyCurrent < m_qtyMax)
            {
                m_qtyCurrent += 1;
                UpdateQtyAndPriceVisuals();
            }
        }

        // ←/→: 버튼 선택 이동 (0=확정, 1=취소)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            m_popupSelIndex = Mathf.Max(0, m_popupSelIndex - 1);
            ApplySelectVisuals();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            m_popupSelIndex = Mathf.Min(1, m_popupSelIndex + 1);
            ApplySelectVisuals();
        }

        // space: 현재 선택 실행
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (m_popupSelIndex == 0) ConfirmQuantity();
            else CancelQuantity();
        }

        // z/Esc: 취소
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Escape))
            CancelQuantity();
    }
    // Z키로 "구매하기/판매하기" 확정
    private void ConfirmQuantity()
    {
        if (m_pendingItem == null || m_qtyCurrent <= 0 || m_exchangeManager == null)
        {
            CloseQuantityPopup();
            return;
        }

        if (m_qtyContext == QtyContext.Buy)
        {
            bool ok = m_exchangeManager.TryBuy(m_pendingItem, m_qtyCurrent);
            int goldNow = m_exchangeManager.GetPlayerGold();
            int unit = Mathf.Max(0, m_exchangeManager.GetBuyPrice(m_pendingItem));
            int total = unit * Mathf.Max(0, m_qtyCurrent);

            if (ok)
                Debug.Log($"[상점] 구매 성공: {m_pendingItem.m_itemName} x{m_qtyCurrent} | 단가:{unit} | 결제:{total} | 잔여 골드:{goldNow}");
            else
                Debug.Log($"[상점] 구매 실패: {m_pendingItem.m_itemName} x{m_qtyCurrent} | 단가:{unit} | 필요:{total} | 보유:{goldNow}");

            UpdatePurchaseUI();
            UpdateSellUI();
        }
        else if (m_qtyContext == QtyContext.Sell)
        {
            bool ok = m_exchangeManager.TrySell(m_pendingItem, m_qtyCurrent);
            int goldNow = m_exchangeManager.GetPlayerGold();
            int unit = Mathf.Max(0, m_exchangeManager.GetSellPrice(m_pendingItem));
            int total = unit * Mathf.Max(0, m_qtyCurrent);

            if (ok)
                Debug.Log($"[상점] 판매 성공: {m_pendingItem.m_itemName} x{m_qtyCurrent} | 단가:{unit} | 수익:{total} | 현재 골드:{goldNow}");
            else
                Debug.Log($"[상점] 판매 실패: {m_pendingItem.m_itemName} x{m_qtyCurrent} | 단가:{unit} | 예정 수익:{total} | 현재 골드:{goldNow}");

            UpdateSellUI();
        }

        CloseQuantityPopup();
    }

    // X/Esc 또는 "취소" 선택 시
    private void CancelQuantity()
    {
        CloseQuantityPopup();
    }


}
