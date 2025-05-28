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

        // �� ���� �� BGM ���� ó�� ���
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Ÿ��Ʋ ���� �ƴϸ� BGM ����
        if (scene.name != "TitleScene")  // Ÿ��Ʋ�� �̸� Ȯ�� �ʿ�
        {
            if (GManager.Instance?.mapBGMController != null)
            {
                GManager.Instance.mapBGMController.StopBGM();
            }
        }
        else
        {
            // Ÿ��Ʋ ������ ���ƿ��� �ٽ� BGM ���
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
                        // ���� ����Ʈ ����
                        GManager.Instance.IsQuestManager?.StartQuest("Q_TM_0");

                        // �����ϱ�ϱ� ���� �÷��� �÷��� true
                        GManager.Instance.IsFirstPlay = true;

                        // ���⼭ 'MainGame'���� ���� ������ MainGame����!
                        SceneLoader.LoadScene("MainGame", true);
                    }
                });
                break;

            case MenuType.Continue:
                Debug.Log("�̾��ϱ�� ���� �������� �ʾҽ��ϴ�.");
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
        Debug.Log("[�޴�] �����ϱ� ��ư Ŭ����!");

        UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
        StartCoroutine(LoadAndPlayIntro());
    }

    private IEnumerator LoadAndPlayIntro()
    {
        yield return new WaitForSeconds(3.0f); // �����δ� �ε��� �Ϸ� ���θ� üũ!

        var introClip = Resources.Load<VideoClip>("Video/OP_KR.ver");
        GManager.Instance.IsVideoManager.PlayVideoRoutine(introClip);
    }
    public bool IsPopupOpen()
    {
        // m_popup�� ���� ������ �˾� ���� ���·� �Ǵ�
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
