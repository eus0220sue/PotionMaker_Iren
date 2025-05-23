using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSc : MonoBehaviour
{
    [SerializeField] GameObject[] m_menuItems;
    [SerializeField] RectTransform m_selected;
    [SerializeField] GameObject m_popup;

    [SerializeField] PopUp m_newGamePopup;
    [SerializeField] PopUp m_quitPopup;

    public int selectedIndex = 0;
    public bool m_boxOpenFlag = false;

    public readonly Color defaultColor = new Color32(200, 200, 200, 255);
    public readonly Color highlightColor = new Color32(255, 255, 255, 255);

    private enum MenuType { NewGame, Continue, Exit }

    void Start()
    {
        selectedIndex = 0;
        UpdateMenuHighlight();
    }

    void Update()
    {
        if (m_boxOpenFlag)
            return;

        HandleArrowInput();
        HandleSelection();
    }

    void HandleArrowInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedIndex = (selectedIndex - 1 + m_menuItems.Length) % m_menuItems.Length;
            UpdateMenuHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedIndex = (selectedIndex + 1) % m_menuItems.Length;
            UpdateMenuHighlight();
        }
    }

    void HandleSelection()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        m_boxOpenFlag = true;
        m_popup.SetActive(true);
        switch ((MenuType)selectedIndex)
        {
            case MenuType.NewGame:
                m_newGamePopup.gameObject.SetActive(true);
                m_newGamePopup.Open(result =>
                {
                    if (result)
                    {
                        GManager.Instance.IsQuestManager.StartQuest("MQ_0");
                        if (GManager.Instance == null)
                        {
                            Debug.LogError("[Test] GManager.Instance == null");
                        }
                        else if (GManager.Instance.IsQuestManager == null)
                        {
                            Debug.LogError("[Test] GManager.Instance.IsQuestManager == null");
                        }
                        else
                        {
                            Debug.Log($"[Test] GManager.Instance.IsQuestManager OK: {GManager.Instance.IsQuestManager.name}");
                            GManager.Instance.IsQuestManager.StartQuest("MQ_0");
                        }

                        SceneLoader.LoadScene("MainGame", afterLoad: () =>
                        {
                            GameObject character = GameObject.Find("Character");
                            GameObject map = GameObject.Find("MapM0_CityHall");

                            if (character != null)
                                GManager.Instance.Setting(character);

                            if (map != null)
                            {
                                GManager.Instance.currentMapGroup = map;

                                BoxCollider2D col = map.GetComponent<BoxCollider2D>();
                                if (col != null && GManager.Instance.IsCameraBase != null)
                                {
                                    Bounds b = col.bounds;
                                    GManager.Instance.IsCameraBase.SetCameraBounds(b.min, b.max);
                                }
                            }
                        });
                    }
                });
                break;


            case MenuType.Continue:
                Debug.Log("이어하기는 아직 구현되지 않았습니다.");
                m_boxOpenFlag = false;
                break;

            case MenuType.Exit:
                m_quitPopup.gameObject.SetActive(true);
                m_quitPopup.Open(result =>
                {
                    if (result)
                        Application.Quit();
                });
                break;
        }
    }

    void UpdateMenuHighlight()
    {
        for (int i = 0; i < m_menuItems.Length; i++)
        {
            var img = m_menuItems[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == selectedIndex) ? highlightColor : defaultColor;
        }

        var rect = m_menuItems[selectedIndex].GetComponent<RectTransform>();
        if (rect != null && m_selected != null)
            m_selected.anchoredPosition = rect.anchoredPosition + new Vector2(0, -50f);
    }
}
