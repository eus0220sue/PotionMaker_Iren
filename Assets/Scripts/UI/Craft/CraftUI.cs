using System.Collections;
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

    [Header("�� ǥ�ÿ� ������Ʈ (�̹��� ��)")]
    [SerializeField] GameObject powderTabObject;
    [SerializeField] GameObject oilTabObject;

    [Header("��ũ�� ����Ʈ")]
    [SerializeField] Transform contentRoot;
    [SerializeField] GameObject craftItemPrefab;

    [Header("�Ǻ� ���� ������")]
    [SerializeField] List<CraftData> powderCraftList;
    [SerializeField] List<OilCraftData> oilCraftList;

    [Header("����Ʈ ǥ�� �θ�")]
    [SerializeField] Transform craftListContent;
    [SerializeField] ScrollRect scrollRect;

    private float scrollStepY = 100f;     // ���� �ϳ� ����
    public enum TabType { Powder, Oil }
    private TabType currentTab = TabType.Powder;
    private List<CraftListUI> slotList = new List<CraftListUI>();
    private int selectedIndex = 0;

    private float holdDelay = 0.2f;
    private float holdTimer = 0f;

    Color32 m_dimmedColor = new Color32(140, 140, 140, 255);
    Color32 m_normalColor = new Color32(255, 255, 255, 255); // ��������(��� ����)
    private Coroutine craftCoroutine = null;

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
        // �� ��ȯ
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentTab = (currentTab == TabType.Powder) ? TabType.Oil : TabType.Powder;
            SwitchTab(currentTab);
        }
        // ���� ���� (SpaceŰ)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (craftCoroutine == null)
                StartCraftCoroutine(TryCraftSelectedCoroutine());
        }

    }
    void SwitchTab(TabType tab)
    {
        currentTab = tab;
        bool isPowder = (tab == TabType.Powder);

        m_podwerBox.SetActive(isPowder);
        m_oilBox.SetActive(!isPowder);

        tabLabelText.text = isPowder ? "�м��" : "�����";

        // Image ������Ʈ ��������
        var powderImage = powderTabObject.GetComponent<Image>();
        var oilImage = oilTabObject.GetComponent<Image>();

        if (powderImage != null)
            powderImage.color = isPowder ? m_normalColor : m_dimmedColor;

        if (oilImage != null)
            oilImage.color = isPowder ? m_dimmedColor : m_normalColor;

        SoundManager.Instance.PlaySystemSound(2);
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
                    itemUI.Setup(data); // CraftData�� Setup
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
                    itemUI.Setup(data); // OilCraftData�� Setup
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
    private IEnumerator TryCraftSelectedCoroutine()
    {
        if (selectedIndex < 0 || selectedIndex >= slotList.Count)
            yield break;

        if (currentTab == TabType.Powder)
        {
            CraftListUI selected = slotList[selectedIndex];
            CraftData data = selected.GetCraftData();
            if (data == null)
            {
                GManager.Instance.IsErrorMessage.ShowErrorMessage("���õ� ���� �����Ͱ� �����ϴ�.", this.transform.parent);
                yield break;
            }

            // ���1 üũ
            if (!m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputItemData, data.IsIAmount))
            {
                GManager.Instance.IsErrorMessage.ShowErrorMessage("���� ����: ��ᰡ �����մϴ�", this.transform.parent);
                yield break;
            }

            // �κ��丮 ���� üũ
            if (!m_exchangeManager.InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItemData, data.IsOAmount))
            {
                GManager.Instance.IsErrorMessage.ShowErrorMessage("���� ����: �κ��丮 ������ �����մϴ�.", this.transform.parent);
                yield break;
            }

            // ���� �� ���� ��� �� ����
            var clip = SoundManager.Instance.systemSounds[6].clip; // ���� ���� ����
            yield return StartCoroutine(SoundManager.Instance.PlaySoundForDuration(clip, 3f));
            m_exchangeManager.Craft(data);
            UpdateInputSlotDim();

            craftCoroutine = null;

        }
        else if (currentTab == TabType.Oil)
        {
            CraftListUI selected = slotList[selectedIndex];
            OilCraftData data = selected.GetOilData();
            if (data == null)
            {
                GManager.Instance.IsErrorMessage.ShowErrorMessage("���õ� ���� �����Ͱ� �����ϴ�.", this.transform.parent);
                yield break;
            }
            // ���1 üũ
            if (!m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1))
            {
                GManager.Instance.IsErrorMessage.ShowErrorMessage($"���� ����: ��ᰡ �����մϴ�", this.transform.parent);
                yield break;
            }

            // ���2 üũ
            if (!m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI2, data.IsIAmount2))
            {
                GManager.Instance.IsErrorMessage.ShowErrorMessage($"���� ����: ��ᰡ �����մϴ�", this.transform.parent);
                yield break;
            }

            // �κ��丮 ���� üũ
            if (!m_exchangeManager.InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItem, data.IsOAmount))
            {
                GManager.Instance.IsErrorMessage.ShowErrorMessage("���� ����: �κ��丮 ������ �����մϴ�.", this.transform.parent);
                yield break;
            }

            // ���� �� ���� ��� �� ����
            var clip = SoundManager.Instance.systemSounds[5].clip; // ���� ���� ����
            yield return StartCoroutine(SoundManager.Instance.PlaySoundForDuration(clip, 3f));
            m_exchangeManager.OilCraft(data);
            UpdateInputSlotDim();

            craftCoroutine = null;

        }
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
        HighlightSlot();
    }
    public void InitCraftUI()
    {
        currentTab = TabType.Powder;     // �� �ʱ�ȭ
        selectedIndex = 0;               // �ε��� �ʱ�ȭ
        SetupCraftList();           // ����Ʈ ����
        SwitchTab(currentTab);           // �ǿ� �´� ����Ʈ ����
        HighlightSlot();                 // ù ��° ���� ���� + ��ũ�� ����
    }
    public void StartCraftCoroutine(IEnumerator coroutine)
    {
        if (craftCoroutine != null)
            StopCoroutine(craftCoroutine);
        craftCoroutine = StartCoroutine(coroutine);
    }
    public void StopCraftCoroutine()
    {
        if (craftCoroutine != null)
        {
            StopCoroutine(craftCoroutine);
            craftCoroutine = null;
        }
        // ���嵵 ����
        SoundManager.Instance.StopAllSFX();
    }

    private void UpdateInputSlotDim()
    {
        if (currentTab == TabType.Powder)
        {
            var data = powderCraftList[selectedIndex];
            if (data == null) return;

            bool hasInput = m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputItemData, data.IsIAmount);
            Color dimmedColor = new Color32(140, 140, 140, 255);
            Color normalColor = new Color32(255, 255, 255, 255);

            podwerInputSlot.SetIconColor(hasInput ? normalColor : dimmedColor);
            podwerOutputSlot.SetIconColor(hasInput ? normalColor : dimmedColor);
        }
        else if (currentTab == TabType.Oil)
        {
            var data = oilCraftList[selectedIndex];
            if (data == null) return;

            bool hasInput1 = m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1);
            bool hasInput2 = m_exchangeManager.InvenManager.IsInventoryData.HasItem(data.IsInputI2, data.IsIAmount2);
            Color dimmedColor = new Color32(140, 140, 140, 255);
            Color normalColor = new Color32(255, 255, 255, 255);

            oilInput1Slot.SetIconColor(hasInput1 ? normalColor : dimmedColor);
            oilInput2Slot.SetIconColor(hasInput2 ? normalColor : dimmedColor);

            bool shouldDimOutput = !hasInput1 || !hasInput2;
            oilOutputSlot.SetIconColor(shouldDimOutput ? dimmedColor : normalColor);
        }
    }

}
