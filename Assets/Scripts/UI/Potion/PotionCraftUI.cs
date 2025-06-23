using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PotionCraftUI : MonoBehaviour
{


    [SerializeField] ExchangeManager m_exchangeManager;
    [Header("우측 제작 박스 (이미지)")]

    [SerializeField] Slot m_Input1Slot;
    [SerializeField] Slot m_Input2Slot;
    [SerializeField] Slot m_OutputSlot;

    /// <summary>
    /// 디테일(우측 UI)
    /// </summary>
    [SerializeField] Image m_potionIllust;
    [SerializeField] TMP_Text m_potionName;


    [SerializeField] GameObject m_potionCraftUI;

    [Header("탭 표시용 오브젝트 (이미지 등)")]
    [SerializeField] TMP_Text NTab;
    [SerializeField] TMP_Text ETab;
    [SerializeField] TMP_Text MTab;
    [SerializeField] Color m_selectedColor = Color.black;
    [SerializeField] Color m_defaultColor = new Color32(161, 116, 37, 255);


    [Header("스크롤 리스트")]
    [SerializeField] Transform contentRoot;
    [SerializeField] GameObject PotionItemPrefab;

    [Header("탭별 제작 데이터")]
    [SerializeField] List<PotionCraftData> NPotionList;
    [SerializeField] List<PotionCraftData> EPotionList;
    [SerializeField] List<PotionCraftData> MPotionList;

    [Header("제작화면)")]
    [SerializeField] GameObject m_bookCraftUI;
    [SerializeField] TMP_Text m_pNameInCraft;
    [SerializeField] TMP_Text m_potionText;
    [SerializeField] Image m_PIllustInCraft;

    [SerializeField] Image m_Input1InCraft;
    [SerializeField] Image m_Input2InCraft;
    [SerializeField] TMP_Text m_Input1Name;
    [SerializeField] TMP_Text m_Input2Name;

    [SerializeField] TMP_Text m_Input1SubCate;
    [SerializeField] TMP_Text m_Input2SubCate;

    [Header("리스트 표시 부모")]
    [SerializeField] Transform PotionListContent;
    [SerializeField] ScrollRect scrollRect;


    [Header("연출용")]
    [SerializeField] GameObject completePanel;   // CompletePanel 오브젝트
    [SerializeField] Image m_resultPotion;
    [SerializeField] TMP_Text m_resultPotion_name;

    [SerializeField] GameObject[] toggleObjects; // 비활성/활성 할 4개 오브젝트
    [SerializeField] VideoClip potionMakeLv1Cutscene;



    public float scrollStepY = 70f;     // 슬롯 하나 높이


    public enum TabType { Novice, Expert, Master }
    public TabType currentTab = TabType.Novice;

    public List<PotionCraftListUI> slotList = new List<PotionCraftListUI>();
    public int selectedIndex = 0;
    public int GetSelectedIndex() => selectedIndex;
    public enum UIState { List, Craft }
    public UIState currentState = UIState.List;

    public float holdDelay = 0.2f;
    public float holdTimer = 0f;

    private bool m_inputLock = false;
    private float m_inputLockDuration = 2f;
    private float m_inputLockTimer = 0f;


    void Start()
    {
        completePanel.SetActive(false);
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

        // m_inputLock 타이머 처리
        if (GManager.Instance.IsUIManager.DialogueOpenFlag) // 여기에 실제 대화 중 플래그로!
        {
            if (!m_inputLock) // 이미 잠금 중이 아니라면
            {
                m_inputLock = true;
                m_inputLockTimer = 1f; // 2초 잠금
                Debug.Log("[LockInput] 대화 종료 후 2초 입력 막힘 시작됨!");
            }
        }
        if (m_inputLock)
        {
            m_inputLockTimer -= Time.deltaTime;
            if (m_inputLockTimer <= 0f)
            {
                m_inputLock = false;
                Debug.Log("[LockInput] 입력 막힘 해제됨!");
            }
        }

        // 탭 전환
        if (Input.GetKeyDown(KeyCode.Tab))
        {
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

        if (Input.GetKeyDown(KeyCode.Space))
        {

            if (currentState == UIState.List && !m_inputLock)
            {
                TrySelected();
            }
            else if (currentState == UIState.Craft && !m_inputLock)
            {
                Debug.Log("[SpaceKey] 스페이스바 입력됨!");
                StartCoroutine(TryCraftCoroutine());
            }
        }

        if (Input.GetKeyDown(KeyCode.Z) && currentState == UIState.Craft)
        {
            currentState = UIState.List;
            m_bookCraftUI.SetActive(false);

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
        SoundManager.Instance.PlaySystemSound(3);


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

        m_bookCraftUI.SetActive(true);

        m_Input1Slot.Set(data.IsInputI1, data.IsIAmount1);
        m_Input2Slot.Set(data.IsInputI2, data.IsIAmount2);
        m_OutputSlot.Set(data.IsOutputItem, data.IsOAmount);

        // 재료 보유 여부에 따른 딤드 처리
        bool hasInput1 = m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1);
        bool hasInput2 = m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI2, data.IsIAmount2);

        Color dimmedColor = new Color32(140, 140, 140, 255);
        Color normalColor = Color.white;

        m_Input1Slot.SetIconColor(hasInput1 ? normalColor : dimmedColor);
        m_Input2Slot.SetIconColor(hasInput2 ? normalColor : dimmedColor);

        // 둘 중 하나라도 딤드라면 출력 슬롯도 딤드 처리
        bool shouldDimOutput = !hasInput1 || !hasInput2;
        m_OutputSlot.SetIconColor(shouldDimOutput ? dimmedColor : normalColor);


        // 상세 묘사 세팅은 기존대로
        m_PIllustInCraft.sprite = data.IsPotionIllust;
        m_potionText.text = data.IsPotionDS;
        m_pNameInCraft.text = data.IsName;
        m_Input1InCraft.sprite = data.IsInputI1.m_itemIcon;
        m_Input2InCraft.sprite = data.IsInputI2.m_itemIcon;
        m_Input1Name.text = data.IsInputI1.m_itemName;
        m_Input2Name.text = data.IsInputI2.m_itemName;
        m_Input1SubCate.text = GetSubCategory(data.IsInputI1);
        m_Input2SubCate.text = GetSubCategory(data.IsInputI2);

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
    private IEnumerator TryCraftCoroutine()
    {
        if (selectedIndex < 0 || selectedIndex >= slotList.Count)
        {
            Debug.LogWarning("[TryCraft] 잘못된 인덱스 접근");
            yield break;
        }

        var selected = slotList[selectedIndex];
        var data = selected.GetPotionData();
        if (data == null)
        {
            Debug.LogWarning("[TryCraft] 선택된 포션 데이터가 null입니다.");
            yield break;
        }

        Debug.Log($"[TryCraft] 포션 제작 시도: {data.IsName}");

        // 1. 등급 확인
        if (m_exchangeManager.m_userData.IsGrade < data.IsGradeType)
        {
            GManager.Instance.IsErrorMessage.ShowErrorMessage($"제작하기 위해선 {data.IsGradeType} 등급을 달성해야 합니다.", this.transform.parent);
            yield break;
        }

        // 2. 재료1 체크
        if (!m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1))
        {
            GManager.Instance.IsErrorMessage.ShowErrorMessage($"제작 실패: 재료가 부족합니다.", this.transform.parent);
            yield break;
        }

        // 3. 재료2 체크
        if (!m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI2, data.IsIAmount2))
        {
            GManager.Instance.IsErrorMessage.ShowErrorMessage($"제작 실패: 재료가 부족합니다.", this.transform.parent);
            yield break;
        }

        // 4. 인벤토리 공간 체크
        if (!m_exchangeManager.InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItem, data.IsOAmount))
        {
            GManager.Instance.IsErrorMessage.ShowErrorMessage($"제작 실패: 인벤토리 공간이 부족합니다.", this.transform.parent);
            yield break;
        }

        // 실제 제작

        UpdateInputSlotDim(data);

        yield return StartCoroutine(PlayPotionCompleteSequence(data));


        yield return new WaitForSeconds(1f);

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
    private string GetSubCategory(ItemData item)
    {
        switch (item.IsItemType)
        {
            case ItemType.Type.Oil:
                return "에센스";
            case ItemType.Type.Material:
                return "원재료";
            case ItemType.Type.Powder:
                return "가루";
            default:
                return "기타";
        }
    }
    public IEnumerator PlayPotionCompleteSequence(PotionCraftData data)
    {
        Debug.Log("[Cutscene] 연출 시작됨!");

        foreach (var obj in toggleObjects)
            obj.SetActive(false);

        // 영상 재생
        var clip = GManager.Instance.IsVideoManager.GetVideoClipByName("PotionMake_Lv1_CutScene");
        if (clip != null)
        {
            yield return StartCoroutine(GManager.Instance.IsVideoManager.PlayVideoRoutine(clip));
        }
        else
        {
            Debug.LogWarning("[PlayPotionCompleteSequence] 영상 클립을 찾을 수 없습니다.");
        }

        // 완료 패널 표시
        m_resultPotion.sprite = data.IsPotionIllust;
        m_resultPotion_name.text = $"{data.IsName} 제작 완료!";
        completePanel.SetActive(true);

        yield return new WaitForSeconds(1f);

        completePanel.SetActive(false);
        foreach (var obj in toggleObjects)
            obj.SetActive(true);

        currentState = UIState.List;
        HighlightSlot();
        Debug.Log("[Cutscene] 연출 끝남!");

        LockInputTemporarily();

    }
    private void UpdateInputSlotDim(PotionCraftData data)
    {
        bool hasInput1 = m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1);
        bool hasInput2 = m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI2, data.IsIAmount2);

        Color dimmedColor = new Color32(140, 140, 140, 255);
        Color normalColor = Color.white;

        m_Input1Slot.SetIconColor(hasInput1 ? normalColor : dimmedColor);
        m_Input2Slot.SetIconColor(hasInput2 ? normalColor : dimmedColor);

        bool shouldDimOutput = !hasInput1 || !hasInput2;
        m_OutputSlot.SetIconColor(shouldDimOutput ? dimmedColor : normalColor);
    }
    // 대화 연출 끝난 뒤 또는 제작 연출 끝난 뒤에 호출
    private void LockInputTemporarily()
    {
        m_inputLock = true;
        m_inputLockTimer = m_inputLockDuration;
        Debug.Log($"[LockInput] 입력 막힘 시작됨! (지속시간: {m_inputLockDuration}초)");
    }

}
