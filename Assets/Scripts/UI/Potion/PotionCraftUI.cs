using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PotionCraftUI : MonoBehaviour
{


    [SerializeField] ExchangeManager m_exchangeManager;
    [Header("���� ���� �ڽ� (�̹���)")]

    [SerializeField] Slot m_Input1Slot;
    [SerializeField] Slot m_Input2Slot;
    [SerializeField] Slot m_OutputSlot;

    /// <summary>
    /// ������(���� UI)
    /// </summary>
    [SerializeField] Image m_potionIllust;
    [SerializeField] TMP_Text m_potionName;


    [SerializeField] GameObject m_potionCraftUI;

    [Header("�� ǥ�ÿ� ������Ʈ (�̹��� ��)")]
    [SerializeField] TMP_Text NTab;
    [SerializeField] TMP_Text ETab;
    [SerializeField] TMP_Text MTab;
    [SerializeField] Color m_selectedColor = Color.black;
    [SerializeField] Color m_defaultColor = new Color32(161, 116, 37, 255);


    [Header("��ũ�� ����Ʈ")]
    [SerializeField] Transform contentRoot;
    [SerializeField] GameObject PotionItemPrefab;

    [Header("�Ǻ� ���� ������")]
    [SerializeField] List<PotionCraftData> NPotionList;
    [SerializeField] List<PotionCraftData> EPotionList;
    [SerializeField] List<PotionCraftData> MPotionList;

    [Header("����ȭ��)")]
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

    [Header("����Ʈ ǥ�� �θ�")]
    [SerializeField] Transform PotionListContent;
    [SerializeField] ScrollRect scrollRect;


    [Header("�����")]
    [SerializeField] GameObject completePanel;   // CompletePanel ������Ʈ
    [SerializeField] Image m_resultPotion;
    [SerializeField] TMP_Text m_resultPotion_name;

    [SerializeField] GameObject[] toggleObjects; // ��Ȱ��/Ȱ�� �� 4�� ������Ʈ
    [SerializeField] VideoClip potionMakeLv1Cutscene;
<<<<<<< HEAD



    public float scrollStepY = 70f;     // ���� �ϳ� ����


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
    private float m_inputLockDuration = 0.5f;
    private float m_inputLockTimer = 0f;

=======



    public float scrollStepY = 70f;     // ���� �ϳ� ����


    public enum TabType { Novice, Expert, Master }
    public TabType currentTab = TabType.Novice;

    public List<PotionCraftListUI> slotList = new List<PotionCraftListUI>();
    public int selectedIndex = 0;
    public int GetSelectedIndex() => selectedIndex;
    public enum UIState { List, Craft }
    public UIState currentState = UIState.List;

    public float holdDelay = 0.2f;
    public float holdTimer = 0f;
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1

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

        // m_inputLock Ÿ�̸� ó�� (�׻� �� ������ ����)
        if (m_inputLock)
        {
            m_inputLockTimer -= Time.deltaTime;
            if (m_inputLockTimer <= 0f)
            {
                m_inputLock = false;
            }
        }

        // �� ��ȯ
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
<<<<<<< HEAD
                StartCoroutine(TryCraftCoroutine());
            }
        }
=======
                StartCoroutine(TryCraftCoroutine()); // �ڷ�ƾ���� ���� �õ�
            }
        }

        if (!gameObject.activeSelf) return;
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1

        if (Input.GetKeyDown(KeyCode.Z) && currentState == UIState.Craft)
        {
            currentState = UIState.List;
            m_bookCraftUI.SetActive(false);

            m_Input1Slot.Clear();
            m_Input2Slot.Clear();
            m_OutputSlot.Clear();

            Debug.Log("[PotionCraftUI] ����Ʈ ���� ���� - �� �ڽ� �ʱ�ȭ��");
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


        // �� UI ǥ�� ó���� ������ ���⿡ �߰�
    }
    public void autoFindExchangeManager()
    {
        m_exchangeManager = FindObjectOfType<ExchangeManager>();
    }
    /// <summary>
    /// �� UI ���� ������Ʈ
    /// </summary>
    private void UpdateTabVisual()
    {
        NTab.color = (currentTab == TabType.Novice) ? m_selectedColor : m_defaultColor;
        ETab.color = (currentTab == TabType.Expert) ? m_selectedColor : m_defaultColor;
        MTab.color = (currentTab == TabType.Master) ? m_selectedColor : m_defaultColor;
    }
    /// <summary>
    /// ���� ���̶���Ʈ ó��
    /// </summary>
    private void HighlightSlot()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            slotList[i].SetHighlight(i == selectedIndex);
        }

        ScrollToSelected(); // ���õ� �׸� �ڵ� ��Ŀ��
        UpdateDescriptionUI();

    }
    /// <summary>
    /// ��ũ�Ѻ信�� ���õ� �������� ��ũ�� �̵�
    /// </summary>
    private void ScrollToSelected()
    {
        if (scrollRect == null || slotList.Count == 0) return;

        int visibleMidIndex = 2; // 0���� �����ϹǷ� 3��°�� �ε��� 2
        float viewportHeight = scrollRect.viewport.rect.height;
        float contentHeight = scrollRect.content.rect.height;
        float maxScrollY = contentHeight - viewportHeight;

        float scrollY = 0f;

        if (selectedIndex > visibleMidIndex)
        {
            scrollY = (selectedIndex - visibleMidIndex) * scrollStepY;
            scrollY = Mathf.Min(scrollY, maxScrollY); // �ʹ� �������� �ʵ��� ����
        }

        scrollRect.content.anchoredPosition = new Vector2(
            scrollRect.content.anchoredPosition.x,
            scrollY
        );
    }
    /// <summary>
    /// ����Ʈ ����
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
    /// ����Ʈ ����
    /// </summary>
    /// <param name="argPotionList">���Ǹ���Ʈ�迭</param>
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
    /// ����Ʈ���� ���õ� ������ ���� ������ ��������
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

        // ��� ���� ���ο� ���� ���� ó��
        bool hasInput1 = m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1);
        bool hasInput2 = m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI2, data.IsIAmount2);

        Color dimmedColor = new Color32(140, 140, 140, 255);
        Color normalColor = Color.white;

        m_Input1Slot.SetIconColor(hasInput1 ? normalColor : dimmedColor);
        m_Input2Slot.SetIconColor(hasInput2 ? normalColor : dimmedColor);

        // �� �� �ϳ��� ������ ��� ���Ե� ���� ó��
        bool shouldDimOutput = !hasInput1 || !hasInput2;
        m_OutputSlot.SetIconColor(shouldDimOutput ? dimmedColor : normalColor);


        // �� ���� ������ �������
        m_PIllustInCraft.sprite = data.IsPotionIllust;
        m_potionText.text = data.IsPotionDS;
        m_pNameInCraft.text = data.IsName;
        m_Input1InCraft.sprite = data.IsInputI1.m_itemIcon;
        m_Input2InCraft.sprite = data.IsInputI2.m_itemIcon;
        m_Input1Name.text = data.IsInputI1.m_itemName;
        m_Input2Name.text = data.IsInputI2.m_itemName;
        m_Input1SubCate.text = GetSubCategory(data.IsInputI1);
        m_Input2SubCate.text = GetSubCategory(data.IsInputI2);

        Debug.Log($"[TrySelected] ���õ� �ε���: {selectedIndex}");

        currentState = UIState.Craft;
        Debug.Log($"[PotionCraftUI] �� ��� ����: {data.IsName}");
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
            Debug.LogWarning("[TryCraft] �߸��� �ε��� ����");
            yield break;
        }

        var selected = slotList[selectedIndex];
        var data = selected.GetPotionData();
        if (data == null)
        {
            Debug.LogWarning("[TryCraft] ���õ� ���� �����Ͱ� null�Դϴ�.");
            yield break;
        }

        Debug.Log($"[TryCraft] ���� ���� �õ�: {data.IsName}");

        // 1. ��� Ȯ��
        if (m_exchangeManager.m_userData.IsGrade < data.IsGradeType)
        {
            GManager.Instance.IsErrorMessage.ShowErrorMessage($"�����ϱ� ���ؼ� {data.IsGradeType} ����� �޼��ؾ� �մϴ�.", this.transform.parent);
            yield break;
        }

        // 2. ���1 üũ
        if (!m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1))
        {
            GManager.Instance.IsErrorMessage.ShowErrorMessage($"���� ����: ��ᰡ �����մϴ�.", this.transform.parent);
            yield break;
        }

        // 3. ���2 üũ
        if (!m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI2, data.IsIAmount2))
        {
            GManager.Instance.IsErrorMessage.ShowErrorMessage($"���� ����: ��ᰡ �����մϴ�.", this.transform.parent);
            yield break;
        }

        // 4. �κ��丮 ���� üũ
        if (!m_exchangeManager.InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItem, data.IsOAmount))
        {
            GManager.Instance.IsErrorMessage.ShowErrorMessage($"���� ����: �κ��丮 ������ �����մϴ�.", this.transform.parent);
            yield break;
        }

        // ���� ����

        UpdateInputSlotDim(data);

        yield return StartCoroutine(PlayPotionCompleteSequence(data));


        yield return new WaitForSeconds(2f);

        m_exchangeManager.PotionCraft(data);
        Debug.Log($"[TryCraft] ���� ����! {data.IsOutputItem.m_itemName} �� {data.IsOAmount}");
        UpdateInputSlotDim(data);
        PlayPotionCompleteSequence();
    }


    // ���� ���� �̵�
    private void HandleSlotMoveInput()
    {
        // 1. ���� GetKeyDown ó�� (ó�� ���� ������ ����)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSlot(-1);
            holdTimer = holdDelay;
            return; //  �ߺ� �Է� ����
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSlot(1);
            holdTimer = holdDelay;
            return;
        }

        // 2. ������ �ִ� ���� ó�� (Hold)
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
        Debug.Log($"[MoveSlot] ���� ���� �ε���: {selectedIndex}");
        HighlightSlot();
    }

    public void InitPotionUI()
    {
        currentTab = TabType.Novice;     // �� �ʱ�ȭ
        selectedIndex = 0;               // �ε��� �ʱ�ȭ
        currentState = UIState.List;     // ���� �ʱ�ȭ

        m_bookCraftUI.SetActive(false);  // �� ȭ�� ��Ȱ��ȭ �߰�
        m_Input1Slot.Clear();            // ���� ����
        m_Input2Slot.Clear();
        m_OutputSlot.Clear();

        SetupCraftList();                // ����Ʈ ����
        UpdateDescriptionUI();
        SwitchTab(currentTab);           // �ǿ� �´� ����Ʈ ����
        HighlightSlot();                 // ù ��° ���� ���� + ��ũ�� ����
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
                return "������";
            case ItemType.Type.Material:
                return "�����";
            case ItemType.Type.Powder:
                return "����";
            default:
                return "��Ÿ";
        }
    }
<<<<<<< HEAD
    public IEnumerator PlayPotionCompleteSequence(PotionCraftData data)
    {
        foreach (var obj in toggleObjects)
            obj.SetActive(false);

        // ���� ���
        var clip = GManager.Instance.IsVideoManager.GetVideoClipByName("PotionMake_Lv1_CutScene");
        if (clip != null)
        {
=======
    public void PlayPotionCompleteSequence()
    {
        // 1. toggleObjects 4���� ��Ȱ��ȭ
        foreach (var obj in toggleObjects)
            obj.SetActive(false);

        // 2. ���� ��� �ڷ�ƾ ���� (�� ��ũ��Ʈ ���� ������Ʈ�� Ȱ��ȭ ���� ����)
        StartCoroutine(PlayVideoAndShowCompletePanel());
    }

    private IEnumerator PlayVideoAndShowCompletePanel()
    {
        // ���� Ŭ�� ��������
        var clip = GManager.Instance.IsVideoManager.GetVideoClipByName("PotionMake_Lv1_CutScene");
        if (clip != null)
        {
            // ���� ���
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
            yield return StartCoroutine(GManager.Instance.IsVideoManager.PlayVideoRoutine(clip));
        }
        else
        {
<<<<<<< HEAD
            Debug.LogWarning("[PlayPotionCompleteSequence] ���� Ŭ���� ã�� �� �����ϴ�.");
        }

        // �Ϸ� �г� ǥ��
        m_resultPotion.sprite = data.IsPotionIllust;
        m_resultPotion_name.text = $"{data.IsName} ���� �Ϸ�!";
        completePanel.SetActive(true);

        yield return new WaitForSeconds(2f);

        completePanel.SetActive(false);
        foreach (var obj in toggleObjects)
            obj.SetActive(true);

        currentState = UIState.List;
        HighlightSlot();
        LockInputTemporarily();

=======
            Debug.LogWarning("[PlayVideoAndShowCompletePanel] ���� Ŭ���� ã�� �� �����ϴ�.");
        }

        // 3. ���� ���� �� �Ϸ� �г� Ȱ��ȭ
        completePanel.SetActive(true);

        // 4. �����̽� Ű �Է� ���
        yield return new WaitForSeconds(2f);

        // 5. �Ϸ� �г� ��Ȱ��ȭ �� UI ����
        completePanel.SetActive(false);

        m_potionCraftUI.SetActive(true);
        // �� UI�� �ʿ�� Ȱ��ȭ
        m_bookCraftUI.SetActive(false);

        foreach (var obj in toggleObjects)
            obj.SetActive(true);

        // 6. ���� �ʱ�ȭ
        currentState = UIState.List;
        HighlightSlot();
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
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
<<<<<<< HEAD
    // ��ȭ ���� ���� �� �Ǵ� ���� ���� ���� �ڿ� ȣ��
    private void LockInputTemporarily()
    {
        m_inputLock = true;
        m_inputLockTimer = m_inputLockDuration;
    }
=======
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1

}
