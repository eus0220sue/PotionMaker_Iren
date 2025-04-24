using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftUI : MonoBehaviour
{
    [SerializeField] private ExchangeManager m_exchangeManager;

    [SerializeField] private TMP_Text tabLabelText;

    [SerializeField] private Slot podwerInputSlot;
    [SerializeField] private Slot podwerOutputSlot;
    [SerializeField] private Slot oilInput1Slot;
    [SerializeField] private Slot oilInput2Slot;
    [SerializeField] private Slot oilOutputSlot;

    [SerializeField] private GameObject m_podwerBox;
    [SerializeField] private GameObject m_oilBox;
    [SerializeField] private GameObject m_craftUI;

    [Header("탭 표시용 오브젝트 (이미지 등)")]
    [SerializeField] private GameObject powderTabObject;
    [SerializeField] private GameObject oilTabObject;

    [Header("스크롤 리스트")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject craftItemPrefab;

    [Header("탭별 제작 데이터")]
    [SerializeField] private List<CraftData> powderCraftList;
    [SerializeField] private List<OilCraftData> oilCraftList;

    [Header("리스트 표시 부모")]
    [SerializeField] private Transform craftListContent;
    [SerializeField] private ScrollRect scrollRect;

    private float scrollStepY = 100f;     // 슬롯 하나 높이
    private enum TabType { Powder, Oil }
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
        if (Input.GetKeyDown(KeyCode.Z))
        {
            TryCraftSelected();
        }
    }
    void SwitchTab(TabType tab)
    {
        currentTab = tab;
        m_podwerBox.SetActive(tab == TabType.Powder);
        m_oilBox.SetActive(tab == TabType.Oil);

        tabLabelText.text = (tab == TabType.Powder) ? "분쇄기" : "추출기";

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
                Debug.Log($"[CraftUI] 분쇄 제작 완료: {data.IsOutputItemData.m_itemName}");
            }
        }
        else if (currentTab == TabType.Oil)
        {
            CraftListUI selected = slotList[selectedIndex];
            OilCraftData data = selected.GetOilData();
            if (data != null)
            {
                m_exchangeManager.OilCraft(data);
                Debug.Log($"[CraftUI] 추출 제작 완료: {data.IsOutputItem.m_itemName}");
            }
        }
    }

    // 슬롯 선택 이동
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
