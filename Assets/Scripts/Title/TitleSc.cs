using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Video;

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
                        // 최초 퀘스트 시작
                        GManager.Instance.IsQuestManager?.StartQuest("Q_TM_0");

                        // 새로하기니까 최초 플레이 플래그 true
                        GManager.Instance.IsFirstPlay = true;

                        // 여기서 'MainGame'으로 가고 싶으면 MainGame으로!
                        SceneLoader.LoadScene("MainGame", true);
                    }
                });
                break;

            case MenuType.Continue:
                Debug.Log("이어하기는 아직 구현되지 않았습니다.");
                GManager.Instance.IsFirstPlay = false;
                SceneLoader.LoadScene("MainGame", false);
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

    public void OnNewGameButton()
    {
        Debug.Log("[메뉴] 새로하기 버튼 클릭됨!");

        UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
        StartCoroutine(LoadAndPlayIntro());
    }

    private IEnumerator LoadAndPlayIntro()
    {
        yield return new WaitForSeconds(3.0f); // 실제로는 로딩씬 완료 여부를 체크!

        var introClip = Resources.Load<VideoClip>("Video/MV_Op");
        GManager.Instance.IsVideoManager.PlayVideoRoutine(introClip);
    }

}
