using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftUI : MonoBehaviour
{
    [SerializeField] ExchangeManager m_exchangeManager;

    [SerializeField] TMP_Text tabLabelText;

    [SerializeField] Slot podwerInputSlot;
    [SerializeField] Slot podwerOutputSlot;
    [SerializeField] Slot oilInput1Slot;
    [SerializeField] Slot oilInput2Slot;
    [SerializeField] Slot oilOutputSlot;

    [SerializeField] GameObject m_podwerBox;
    [SerializeField] GameObject m_oilBox;
    [SerializeField] GameObject m_craftUI;

    [Header("탭 표시용 오브젝트 (이미지 등)")]
    [SerializeField] GameObject powderTabObject;
    [SerializeField] GameObject oilTabObject;

    [Header("스크롤 리스트")]
    [SerializeField] Transform contentRoot;
    [SerializeField] GameObject craftItemPrefab;

    [Header("탭별 제작 데이터")]
    [SerializeField] List<CraftData> powderCraftList;
    [SerializeField] List<OilCraftData> oilCraftList;

    [Header("리스트 표시 부모")]
    [SerializeField] Transform craftListContent;
    [SerializeField] ScrollRect scrollRect;

    [SerializeField] Animator m_powderTagAnimator;
    [SerializeField] Animator m_oilTagAnimator;


    private float scrollStepY = 100f;     // 슬롯 하나 높이
    public enum TabType { Powder, Oil }
    private TabType currentTab = TabType.Powder;
    private List<CraftListUI> slotList = new List<CraftListUI>();
    private int selectedIndex = 0;

    private float holdDelay = 0.2f;
    private float holdTimer = 0f;

    void Start()
    {
        m_craftUI.SetActive(false); ;
        SwitchTab(TabType.Powder);
        SetupCraftList();
        HighlightSlot();
        InitCraftUI();
        autoFindExchangeManager();
    }
    void Update()
    {
        if (!gameObject.activeSelf) return;
        HandleSlotMoveInput();
        // 탭 전환
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentTab = (currentTab == TabType.Powder) ? TabType.Oil : TabType.Powder;
            SwitchTab(currentTab);
        }
        // 제작 실행 (Z키)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryCraftSelected();
        }
    }
    private bool hasInitialized = false;

    void SwitchTab(TabType tab)
    {
        bool isPowder = (tab == TabType.Powder);

        // 처음 진입 시엔 애니메이션 재생하지 않음
        if (hasInitialized)
        {
            m_powderTagAnimator.SetBool("IsOn", isPowder);
            m_oilTagAnimator.SetBool("IsOn", !isPowder);
        }
        else
        {
            // Animator 파라미터를 건드리지 않고 초기화만
            hasInitialized = true;
        }

        currentTab = tab;

        m_podwerBox.SetActive(isPowder);
        m_oilBox.SetActive(!isPowder);
        tabLabelText.text = isPowder ? "분쇄기" : "추출기";

        SetupCraftList();
        selectedIndex = 0;
        HighlightSlot();
    }

    private void SetupCraftList()
    {
        foreach (Transform child in craftListContent)
            Destroy(child.gameObject);

        slotList.Clear();

        if (currentTab == TabType.Powder)
        {
            foreach (var data in powderCraftList)
            {
                var obj = Instantiate(craftItemPrefab, craftListContent);
                var itemUI = obj.GetComponent<CraftListUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(data); // CraftData용 Setup
                    slotList.Add(itemUI);
                }
            }
        }
        else if (currentTab == TabType.Oil)
        {
            foreach (var data in oilCraftList) // List<OilCraftData>
            {
                var obj = Instantiate(craftItemPrefab, craftListContent);
                var itemUI = obj.GetComponent<CraftListUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(data); // OilCraftData용 Setup
                    slotList.Add(itemUI);
                }
            }
        }
    }
    public void autoFindExchangeManager()
    {
        m_exchangeManager = FindObjectOfType<ExchangeManager>();
    }

    private void HighlightSlot()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            slotList[i].SetHighlight(i == selectedIndex);
        }
        UpdateCraftBoxUI();

        ScrollToSelected();
    }
    private void ScrollToSelected()
    {
        if (scrollRect == null || slotList.Count == 0) return;

        int visibleMidIndex = 2; // 0부터 시작하므로 3번째가 인덱스 2
        float viewportHeight = scrollRect.viewport.rect.height;
        float contentHeight = scrollRect.content.rect.height;
        float maxScrollY = contentHeight - viewportHeight;

        float scrollY = 0f;

        if (selectedIndex > visibleMidIndex)
        {
            scrollY = (selectedIndex - visibleMidIndex) * scrollStepY;
            scrollY = Mathf.Min(scrollY, maxScrollY); // 너무 내려가지 않도록 제한
        }

        scrollRect.content.anchoredPosition = new Vector2(
            scrollRect.content.anchoredPosition.x,
            scrollY
        );
    }
    public int GetSelectedIndex()
    {
        return selectedIndex;
    }
    public CraftListUI GetSelectedSlot()
    {
        if (slotList.Count == 0 || selectedIndex < 0 || selectedIndex >= slotList.Count)
            return null;
        return slotList[selectedIndex];
    }
    private void UpdateCraftBoxUI()
    {
        if (currentTab == TabType.Powder)
        {
            var data = powderCraftList[selectedIndex];
            if (data == null) return;

            podwerInputSlot.Set(data.IsInputItemData, data.IsIAmount);
            podwerOutputSlot.Set(data.IsOutputItemData, data.IsOAmount);
        }
        if (currentTab == TabType.Oil)
        {
            var data = oilCraftList[selectedIndex];
            if (data == null) return;

            oilInput1Slot.Set(data.IsInputI1, data.IsIAmount1);
            oilInput2Slot.Set(data.IsInputI2, data.IsIAmount2);

            oilOutputSlot.Set(data.IsOutputItem, data.IsOAmount);
        }
    }
    private void TryCraftSelected()
    {
        if (selectedIndex < 0 || selectedIndex >= slotList.Count) return;

        if (currentTab == TabType.Powder)
        {
            CraftListUI selected = slotList[selectedIndex];
            CraftData data = selected.GetCraftData();
            if (data != null)
            {
                m_exchangeManager.Craft(data);
            }
        }
        else if (currentTab == TabType.Oil)
        {
            CraftListUI selected = slotList[selectedIndex];
            OilCraftData data = selected.GetOilData();
            if (data != null)
            {
                m_exchangeManager.OilCraft(data);
            }
        }
    }

    // 슬롯 선택 이동
    private void HandleSlotMoveInput()
    {
        // 1. 먼저 GetKeyDown 처리 (처음 누른 순간만 반응)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSlot(-1);
            holdTimer = holdDelay;
            return; //  중복 입력 방지
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSlot(1);
            holdTimer = holdDelay;
            return;
        }

        // 2. 누르고 있는 상태 처리 (Hold)
        if (Input.GetKey(KeyCode.UpArrow))
        {
            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                MoveSlot(-1);
                holdTimer = holdDelay;
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
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
        HighlightSlot();
    }


    public void InitCraftUI()
    {
        currentTab = TabType.Powder;     // 탭 초기화
        selectedIndex = 0;               // 인덱스 초기화
        SetupCraftList();           // 리스트 세팅
        SwitchTab(currentTab);           // 탭에 맞는 리스트 세팅
        HighlightSlot();                 // 첫 번째 슬롯 강조 + 스크롤 조정
    }
}
