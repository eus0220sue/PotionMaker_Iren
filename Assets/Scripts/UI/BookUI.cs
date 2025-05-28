using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookUI : MonoBehaviour
{
    public enum CategoryType { Potion, Resource }
    [SerializeField] private CategoryType m_currentCategory = CategoryType.Potion;

    [Header("도감 UI")]
    [SerializeField] GameObject m_potionBookUIObj;
    [SerializeField] GameObject m_resBookUIObj;
    [SerializeField] PotionBookUI m_potionBookUI;
    [SerializeField] ResBookUI m_resBookUI;


    void Start()
    {
        gameObject.SetActive(false);

        m_potionBookUIObj.SetActive(false);
        m_resBookUIObj.SetActive(false);
        // 시작 시, 포션도감만 활성화
        SwitchCategory(CategoryType.Potion);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        // 카테고리 전환 (Tab)
        if (Input.GetKeyDown(KeyCode.Tab))
            SwitchCategory();

        if (m_currentCategory == CategoryType.Potion)
        {
            // 탭 전환 (Q/E)
            if (Input.GetKeyDown(KeyCode.Q))
                m_potionBookUI.PrevTab();
            if (Input.GetKeyDown(KeyCode.E))
                m_potionBookUI.NextTab();

            // 슬롯/상세보기 입력
            m_potionBookUI.HandleInput();
        }
        else if (m_currentCategory == CategoryType.Resource)
        {
            // 재료도감 입력
            m_resBookUI.HandleInput();
        }
    }

    // 카테고리 전환
    void SwitchCategory()
    {
        m_currentCategory = m_currentCategory == CategoryType.Potion ? CategoryType.Resource : CategoryType.Potion;
        SwitchCategory(m_currentCategory);
    }

    void SwitchCategory(CategoryType category)
    {
        m_currentCategory = category;

        // UI 활성화/비활성화
        m_potionBookUIObj.SetActive(category == CategoryType.Potion);
        m_resBookUIObj.SetActive(category == CategoryType.Resource);
    }
}
