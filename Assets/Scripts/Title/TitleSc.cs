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

        if (GManager.Instance != null && GManager.Instance.mapBGMController != null)
        {
            GManager.Instance.mapBGMController.PlayTitleBGM();
        }

        // 씬 변경 시 BGM 끄기 처리 등록
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 타이틀 씬이 아니면 BGM 끄기
        if (scene.name != "TitleScene")  // 타이틀씬 이름 확인 필요
        {
            if (GManager.Instance?.mapBGMController != null)
            {
                GManager.Instance.mapBGMController.StopBGM();
            }
        }
        else
        {
            // 타이틀 씬으로 돌아오면 다시 BGM 재생
            if (GManager.Instance?.mapBGMController != null)
            {
                GManager.Instance.mapBGMController.PlayTitleBGM();
            }
        }
    }


    void Update()
    {
        if (GManager.Instance.IsUIManager.EscapeKeyUIOpenFlag)
        {
            return;
        }
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

        var introClip = Resources.Load<VideoClip>("Video/OP_KR.ver");
        GManager.Instance.IsVideoManager.PlayVideoRoutine(introClip);
    }
    public bool IsPopupOpen()
    {
        // m_popup이 켜져 있으면 팝업 열림 상태로 판단
        return m_popup != null && m_popup.activeSelf;
    }

    public void ClosePopup()
    {
        if (m_popup != null)
        {
            m_popup.SetActive(false);
            m_boxOpenFlag = false;
            m_newGamePopup.gameObject.SetActive(false);
            m_quitPopup.gameObject.SetActive(false);
        }
    }
}
