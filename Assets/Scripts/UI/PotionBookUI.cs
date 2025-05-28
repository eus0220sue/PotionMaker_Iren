using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PotionBookUI : MonoBehaviour
{
    public enum TabType { Novice, Expert, Master }
    public TabType currentTab = TabType.Novice;

    [Header("탭 관련")]
    [SerializeField] TMP_Text NTab;
    [SerializeField] TMP_Text ETab;
    [SerializeField] TMP_Text MTab;
    [SerializeField] Color m_selectedColor = Color.black;
    [SerializeField] Color m_defaultColor = new Color32(161, 116, 37, 255);

    [Header("리스트")]
    [SerializeField] List<PotionCraftData> NPotionList;
    [SerializeField] List<PotionCraftData> EPotionList;
    [SerializeField] List<PotionCraftData> MPotionList;
    [SerializeField] Transform PotionListContent;
    [SerializeField] GameObject PotionItemPrefab;
    [SerializeField] ScrollRect scrollRect;
    public float scrollStepY = 70f;

    [Header("미리보기")]
    [SerializeField] TMP_Text m_potionName;
    [SerializeField] Image m_potionIllust;

    [Header("상세 UI")]
    [SerializeField] GameObject m_bookDetailUI;
    [SerializeField] Image m_potionDetail;
    [SerializeField] TMP_Text m_pName;
    [SerializeField] TMP_Text m_potionText;
    [SerializeField] Image m_Input1Img;
    [SerializeField] Image m_Input2Img;
    [SerializeField] TMP_Text m_Input1Name;
    [SerializeField] TMP_Text m_Input2Name;
    [SerializeField] TMP_Text m_Input1SubCate;
    [SerializeField] TMP_Text m_Input2SubCate;

    private List<PotionCraftListUI> slotList = new();
    private int selectedIndex = 0;

    void Start()
    {
        m_bookDetailUI.SetActive(false);
        SetupList();
        HighlightSlot();
        UpdateTabVisual();
    }

    public void NextTab()
    {
        currentTab = currentTab switch
        {
            TabType.Novice => TabType.Expert,
            TabType.Expert => TabType.Master,
            _ => TabType.Novice
        };
        RefreshTab();
    }

    public void PrevTab()
    {
        currentTab = currentTab switch
        {
            TabType.Novice => TabType.Master,
            TabType.Expert => TabType.Novice,
            _ => TabType.Expert
        };
        RefreshTab();
    }

    void RefreshTab()
    {
        SetupList();
        selectedIndex = 0;
        HighlightSlot();
        UpdateTabVisual();
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            MoveSlot(-1);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            MoveSlot(1);
        else if (Input.GetKeyDown(KeyCode.Space))
            ShowDetail();
        else if (Input.GetKeyDown(KeyCode.Z))
            HideDetail();
    }

    void MoveSlot(int dir)
    {
        selectedIndex = Mathf.Clamp(selectedIndex + dir, 0, slotList.Count - 1);
        HighlightSlot();
    }

    void HighlightSlot()
    {
        for (int i = 0; i < slotList.Count; i++)
            slotList[i].SetHighlight(i == selectedIndex);
        ScrollToSelected();
        UpdatePreviewUI();
    }

    void SetupList()
    {
        foreach (Transform child in PotionListContent)
            Destroy(child.gameObject);
        slotList.Clear();

        var list = currentTab switch
        {
            TabType.Novice => NPotionList,
            TabType.Expert => EPotionList,
            TabType.Master => MPotionList,
            _ => NPotionList
        };

        foreach (var data in list)
        {
            var obj = Instantiate(PotionItemPrefab, PotionListContent);
            var slot = obj.GetComponent<PotionCraftListUI>();
            slot.Setup(data);
            slotList.Add(slot);
        }
    }

    void ScrollToSelected()
    {
        if (scrollRect == null || slotList.Count == 0) return;

        int midIndex = 2;
        float viewportH = scrollRect.viewport.rect.height;
        float contentH = scrollRect.content.rect.height;
        float maxY = contentH - viewportH;

        float scrollY = selectedIndex > midIndex
            ? Mathf.Min((selectedIndex - midIndex) * scrollStepY, maxY)
            : 0;

        scrollRect.content.anchoredPosition = new Vector2(
            scrollRect.content.anchoredPosition.x, scrollY);
    }

    void UpdateTabVisual()
    {
        NTab.color = currentTab == TabType.Novice ? m_selectedColor : m_defaultColor;
        ETab.color = currentTab == TabType.Expert ? m_selectedColor : m_defaultColor;
        MTab.color = currentTab == TabType.Master ? m_selectedColor : m_defaultColor;
    }

    void UpdatePreviewUI()
    {
        if (selectedIndex < 0 || selectedIndex >= slotList.Count) return;
        var data = slotList[selectedIndex].GetPotionData();
        m_potionName.text = data.IsName;
        m_potionIllust.sprite = data.IsPotionIllust;
    }

    public void ShowDetail()
    {
        if (selectedIndex < 0 || selectedIndex >= slotList.Count) return;
        var data = slotList[selectedIndex].GetPotionData();
        m_potionDetail.sprite = data.IsPotionIllust;
        m_pName.text = data.IsName;
        m_potionText.text = data.IsPotionDS;
        m_Input1Img.sprite = data.IsInputI1.m_itemIcon;
        m_Input2Img.sprite = data.IsInputI2.m_itemIcon;
        m_Input1Name.text = data.IsInputI1.m_itemName;
        m_Input2Name.text = data.IsInputI2.m_itemName;
        m_Input1SubCate.text = GetSubCategory(data.IsInputI1);
        m_Input2SubCate.text = GetSubCategory(data.IsInputI2);
        m_bookDetailUI.SetActive(true);
    }

    public void HideDetail() => m_bookDetailUI.SetActive(false);

    string GetSubCategory(ItemData item) => item.IsItemType switch
    {
        ItemType.Type.Oil => "에센스",
        ItemType.Type.Material => "원재료",
        ItemType.Type.Powder => "가루",
        _ => "기타"
    };
}
