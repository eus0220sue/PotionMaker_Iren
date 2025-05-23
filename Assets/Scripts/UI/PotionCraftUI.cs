using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionCraftUI : MonoBehaviour
{


    [SerializeField] private ExchangeManager m_exchangeManager;
    [Header("우측 제작 박스 (이미지)")]

    [SerializeField] private Slot m_Input1Slot;
    [SerializeField] private Slot m_Input2Slot;
    [SerializeField] private Slot m_OutputSlot;

    /// <summary>
    /// 디테일(우측 UI)
    /// </summary>
    [SerializeField] private Image m_potionIllust;
    [SerializeField] private TMP_Text m_potionName;


    [SerializeField] private GameObject m_potionCraftUI;

    [Header("탭 표시용 오브젝트 (이미지 등)")]
    [SerializeField] private TMP_Text NTab;
    [SerializeField] private TMP_Text ETab;
    [SerializeField] private TMP_Text MTab;
    [SerializeField] private Color m_selectedColor = Color.black;
    [SerializeField] private Color m_defaultColor = new Color32(161, 116, 37, 255);


    [Header("스크롤 리스트")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject PotionItemPrefab;

    [Header("탭별 제작 데이터")]
    [SerializeField] private List<PotionCraftData> NPotionList;
    [SerializeField] private List<PotionCraftData> EPotionList;
    [SerializeField] private List<PotionCraftData> MPotionList;

    [Header("제작화면)")]
    [SerializeField] private GameObject m_bookCraftUI;
    [SerializeField] private TMP_Text m_pNameInCraft;
    [SerializeField] private TMP_Text m_potionText;
    [SerializeField] private Image m_PIllustInCraft;

    [SerializeField] private Image m_Input1InCraft;
    [SerializeField] private Image m_Input2InCraft;
    [SerializeField] private TMP_Text m_Input1Name;
    [SerializeField] private TMP_Text m_Input2Name;





    [Header("리스트 표시 부모")]
    [SerializeField] private Transform PotionListContent;
    [SerializeField] private ScrollRect scrollRect;
    private float scrollStepY = 70f;     // 슬롯 하나 높이


    private enum TabType { Novice, Expert, Master }
    private TabType currentTab = TabType.Novice;

    private List<PotionCraftListUI> slotList = new List<PotionCraftListUI>();
    private int selectedIndex = 0;
    public int GetSelectedIndex()=>selectedIndex;
    private enum UIState { List, Craft }
    private UIState currentState = UIState.List;

    private float holdDelay = 0.2f;
    private float holdTimer = 0f;

    void Start()
    {
        m_bookCraftUI.SetActive(false);
        m_potionCraftUI.SetActive(false);
        InitPotionUI();
        SwitchTab(TabType.Novice);
        SetupCraftList();
        HighlightSlot();
        autoFindExchangeManager();

    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        // 탭 전환
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // 순서대로: Novice → Expert → Master → 다시 Novice
            switch (currentTab)
            {
                case TabType.Novice:
                    currentTab = TabType.Expert;
                    break;
                case TabType.Expert:
                    currentTab = TabType.Master;
                    break;
                case TabType.Master:
                    currentTab = TabType.Novice;
                    break;
            }

            SwitchTab(currentTab);
        }
        HandleSlotMoveInput();
       
        // Z키 기능
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentState == UIState.List)
            {
                TrySelected(); // 리스트 → 상세 정보 전환
            }
            else if (currentState == UIState.Craft)
            {
                TryCraft(); // 상세 정보에서 제작 시도
            }
        }
        if (!gameObject.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Z) && currentState == UIState.Craft)
        {
            currentState = UIState.List;
            m_bookCraftUI.SetActive(false);

            // 슬롯 초기화
            m_Input1Slot.Clear();
            m_Input2Slot.Clear();
            m_OutputSlot.Clear();

            Debug.Log("[PotionCraftUI] 리스트 모드로 복귀 - 상세 박스 초기화됨");
        }

    }
    void SwitchTab(TabType tab)
    {
        currentTab = tab;
        SetupCraftList();
        selectedIndex = 0;
        HighlightSlot();
        UpdateTabVisual();


        // 탭 UI 표시 처리도 있으면 여기에 추가
    }
    public void autoFindExchangeManager()
    {
        m_exchangeManager = FindObjectOfType<ExchangeManager>();
    }
    /// <summary>
    /// 탭 UI 색상 업데이트
    /// </summary>
    private void UpdateTabVisual()
    {
        NTab.color = (currentTab == TabType.Novice) ? m_selectedColor : m_defaultColor;
        ETab.color = (currentTab == TabType.Expert) ? m_selectedColor : m_defaultColor;
        MTab.color = (currentTab == TabType.Master) ? m_selectedColor : m_defaultColor;
    }
    /// <summary>
    /// 슬롯 하이라이트 처리
    /// </summary>
    private void HighlightSlot()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            slotList[i].SetHighlight(i == selectedIndex);
        }

        ScrollToSelected(); // 선택된 항목 자동 포커스
        UpdateDescriptionUI();

    }
    /// <summary>
    /// 스크롤뷰에서 선택된 슬롯으로 스크롤 이동
    /// </summary>
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
    /// <summary>
    /// 리스트 세팅
    /// </summary>
    private void SetupCraftList()
    {
        foreach (Transform child in PotionListContent)
            Destroy(child.gameObject);

        slotList.Clear();

        switch (currentTab)
        {
            case TabType.Novice:
                AddItemsLot(NPotionList);
                break;
            case TabType.Expert:
                AddItemsLot(EPotionList);
                break;
            case TabType.Master:
                AddItemsLot(MPotionList);
                break;
        }
    }
    /// <summary>
    /// 리스트 세팅
    /// </summary>
    /// <param name="argPotionList">포션리스트배열</param>
    public void AddItemsLot(List<PotionCraftData> argPotionList)
    {
        foreach (var data in argPotionList)
        {
            var obj = Instantiate(PotionItemPrefab, PotionListContent);
            var itemUI = obj.GetComponent<PotionCraftListUI>();
            if (itemUI != null)
            {
                itemUI.Setup(data);
                slotList.Add(itemUI);
            }
        }
    }
    /// <summary>
    /// 리스트에서 선택된 슬롯의 포션 데이터 가져오기
    /// </summary>
    void TrySelected()
    {
        var selected = GetSelectedSlot();
        if (selected == null) return;

        var data = selected.GetPotionData();
        if (data == null) return;
        // BookCraft UI 표시
        m_bookCraftUI.SetActive(true);

        // 슬롯 세팅
        m_Input1Slot.Set(data.IsInputI1, data.IsIAmount1);
        m_Input2Slot.Set(data.IsInputI2, data.IsIAmount2);
        m_OutputSlot.Set(data.IsOutputItem, data.IsOAmount);

        // 묘사(좌측) 세팅
        m_PIllustInCraft.sprite=data.IsPotionIllust;
        m_potionText.text = data.IsPotionDS;
        m_pNameInCraft.text = data.IsName;
        m_Input1InCraft.sprite = data.IsInputI1.m_itemIcon;
        m_Input2InCraft.sprite = data.IsInputI2.m_itemIcon;
        m_Input1Name.text = data.IsInputI1.m_itemName;
        m_Input2Name.text = data.IsInputI2.m_itemName;

        Debug.Log($"[TrySelected] 선택된 인덱스: {selectedIndex}");


        currentState = UIState.Craft;
        Debug.Log($"[PotionCraftUI] 상세 모드 진입: {data.IsName}");
    }


    public PotionCraftListUI GetSelectedSlot()
    {
        if (slotList.Count == 0 || selectedIndex < 0 || selectedIndex >= slotList.Count)
            return null;
        return slotList[selectedIndex];
    }
    void TryCraft()
    {
        if (selectedIndex < 0 || selectedIndex >= slotList.Count)
        {
            Debug.LogWarning("[TryCraft] 잘못된 인덱스 접근: selectedIndex = " + selectedIndex);
            return;
        }

        PotionCraftListUI selected = slotList[selectedIndex];
        PotionCraftData data = selected.GetPotionData();
        if (data == null)
        {
            Debug.LogWarning("[TryCraft] 선택된 포션 데이터가 null입니다.");
            return;
        }

        Debug.Log($"[TryCraft] 포션 제작 시도: {data.IsName}");

        // 1. 등급 확인
        if (m_exchangeManager.m_userData.IsGrade < data.IsGradeType)
        {
            Debug.LogWarning($"[TryCraft] 제작 실패: 등급 부족 (현재: {m_exchangeManager.m_userData.IsGrade}, 필요: {data.IsGradeType})");
            return;
        }

        // 2. 재료1 체크
        if (!m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1))
        {
            Debug.LogWarning($"[TryCraft] 제작 실패: 재료1 부족 - {data.IsInputI1.m_itemName} 필요: {data.IsIAmount1}");
            return;
        }

        // 3. 재료2 체크
        if (!m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI2, data.IsIAmount2))
        {
            Debug.LogWarning($"[TryCraft] 제작 실패: 재료2 부족 - {data.IsInputI2.m_itemName} 필요: {data.IsIAmount2}");
            return;
        }

        // 4. 인벤토리 공간 체크
        if (!m_exchangeManager.InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItem, data.IsOAmount))
        {
            Debug.LogWarning($"[TryCraft] 제작 실패: 인벤토리 공간 부족 (아이템: {data.IsOutputItem.m_itemName}, 수량: {data.IsOAmount})");
            return;
        }

        // 실제 제작
        m_exchangeManager.PotionCraft(data);
        Debug.Log($"[TryCraft] 제작 성공! {data.IsOutputItem.m_itemName} × {data.IsOAmount}");
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
        Debug.Log($"[MoveSlot] 현재 선택 인덱스: {selectedIndex}");
        HighlightSlot();
    }

    public void InitPotionUI()
    {
        currentTab = TabType.Novice;     // 탭 초기화
        selectedIndex = 0;               // 인덱스 초기화
        currentState = UIState.List;     // 상태 초기화

        m_bookCraftUI.SetActive(false);  // 상세 화면 비활성화 추가
        m_Input1Slot.Clear();            // 슬롯 비우기
        m_Input2Slot.Clear();
        m_OutputSlot.Clear();

        SetupCraftList();                // 리스트 세팅
        UpdateDescriptionUI();
        SwitchTab(currentTab);           // 탭에 맞는 리스트 세팅
        HighlightSlot();                 // 첫 번째 슬롯 강조 + 스크롤 조정
    }

    public void UpdateDescriptionUI()
    {
        if (selectedIndex < 0 || selectedIndex >= slotList.Count)
            return;

        if (currentTab == TabType.Novice)
        {
            var selected = GetSelectedSlot();
            var data = selected.GetPotionData();
            m_potionName.text = data.IsName;
            m_potionIllust.sprite = data.IsPotionIllust;
        }
        else if (currentTab == TabType.Expert)
        {
            var selected = GetSelectedSlot();
            var data = selected.GetPotionData();
            m_potionName.text = data.IsName;
            m_potionIllust.sprite = data.IsPotionIllust;
        }
        else if (currentTab == TabType.Master)
        {
            var selected = GetSelectedSlot();
            var data = selected.GetPotionData();
            m_potionName.text = data.IsName;
            m_potionIllust.sprite = data.IsPotionIllust;
        }
    }
}
