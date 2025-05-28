using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class ResBookUI : MonoBehaviour
{
    [SerializeField] List<ItemData> m_resourceList;
    [SerializeField] Image[] m_resIcon;
    [SerializeField] TMP_Text[] m_nameTexts;
    [SerializeField] TMP_Text[] m_infoTexts;

    private int currentPage = 0;
    private const int itemsPerPage = 6;

    void Start() => UpdatePage();

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if ((currentPage + 1) * itemsPerPage < m_resourceList.Count)
            {
                currentPage++;
                UpdatePage();
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdatePage();
            }
        }
    }

    private void UpdatePage()
    {
        int start = currentPage * itemsPerPage;
        for (int i = 0; i < itemsPerPage; i++)
        {
            int idx = start + i;
            if (idx < m_resourceList.Count)
            {
                m_nameTexts[i].text = m_resourceList[idx].m_itemName;
                m_infoTexts[i].text = m_resourceList[idx].m_description;
                m_resIcon[i].sprite = m_resourceList[idx].m_itemIcon; // 이미지 추가
            }
            else
            {
                m_nameTexts[i].text = "";
                m_infoTexts[i].text = "";
                m_resIcon[i].sprite = null; // 이미지 비우기
            }
        }
    }

    private string GetSubCategory(ItemData item)
    {
        return item.m_description; // 아이템 데이터의 Description을 그대로 반환
    }

}
