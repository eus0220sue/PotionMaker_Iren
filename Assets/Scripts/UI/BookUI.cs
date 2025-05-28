using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookUI : MonoBehaviour
{
    public enum CategoryType { Potion, Resource }
    [SerializeField] private CategoryType m_currentCategory = CategoryType.Potion;

    [Header("���� UI")]
    [SerializeField] GameObject m_potionBookUIObj;
    [SerializeField] GameObject m_resBookUIObj;
    [SerializeField] PotionBookUI m_potionBookUI;
    [SerializeField] ResBookUI m_resBookUI;


    void Start()
    {
        gameObject.SetActive(false);

        m_potionBookUIObj.SetActive(false);
        m_resBookUIObj.SetActive(false);
        // ���� ��, ���ǵ����� Ȱ��ȭ
        SwitchCategory(CategoryType.Potion);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        // ī�װ� ��ȯ (Tab)
        if (Input.GetKeyDown(KeyCode.Tab))
            SwitchCategory();

        if (m_currentCategory == CategoryType.Potion)
        {
            // �� ��ȯ (Q/E)
            if (Input.GetKeyDown(KeyCode.Q))
                m_potionBookUI.PrevTab();
            if (Input.GetKeyDown(KeyCode.E))
                m_potionBookUI.NextTab();

            // ����/�󼼺��� �Է�
            m_potionBookUI.HandleInput();
        }
        else if (m_currentCategory == CategoryType.Resource)
        {
            // ��ᵵ�� �Է�
            m_resBookUI.HandleInput();
        }
    }

    // ī�װ� ��ȯ
    void SwitchCategory()
    {
        m_currentCategory = m_currentCategory == CategoryType.Potion ? CategoryType.Resource : CategoryType.Potion;
        SwitchCategory(m_currentCategory);
    }

    void SwitchCategory(CategoryType category)
    {
        m_currentCategory = category;

        // UI Ȱ��ȭ/��Ȱ��ȭ
        m_potionBookUIObj.SetActive(category == CategoryType.Potion);
        m_resBookUIObj.SetActive(category == CategoryType.Resource);
    }
}
