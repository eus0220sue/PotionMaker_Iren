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

    // TitleSc 클래스의 필드 영역에 추가
    [SerializeField] private TMPro.TMP_Text m_continueText;     // 이어하기 항목의 TMP_Text
    [SerializeField] private Color m_colorEnabled = Color.white;
    [SerializeField] private Color m_colorDisabled = new Color(1f, 1f, 1f, 0.4f);

    private bool m_canContinue = false;


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
        //추가: 저장 파일 유무에 따라 '이어하기' 선택 가능/불가 갱신
        RefreshContinueAvailability();
        UpdateMenuHighlight();
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
    // 이어하기 선택 가능 여부(저장 파일 있는지) ->Start()에서 1회 갱신
    private bool IsSelectable(int index)
    {
        var mt = (MenuType)index;
        if (mt == MenuType.Continue) return m_canContinue; // 세이브 없으면 false
        return true;
    }

    // 좌/우 이동 시 '선택 불가' 항목을 건너뛰며 순환
    private int GetNextSelectable(int dir)
    {
        int count = m_menuItems.Length;
        int idx = selectedIndex;
        for (int i = 0; i < count; i++)
        {
            idx = (idx + dir + count) % count;
            if (IsSelectable(idx)) return idx;
        }
        return selectedIndex; // (모두 불가인 경우 -> 사실상 발생 X)
    }

    // 세이브 유무 확인 & 이어하기 텍스트 회색 처리 & 커서 보정
    private void RefreshContinueAvailability()
    {
        m_canContinue = (GManager.Instance && GManager.Instance.HasSave());

        if (m_continueText)
            m_continueText.color = m_canContinue ? m_colorEnabled : m_colorDisabled;

        // 현재 커서가 선택 불가 항목이면, 다음 선택 가능한 항목으로 이동
        if (!IsSelectable(selectedIndex))
            selectedIndex = GetNextSelectable(+1);
    }

    void HandleArrowInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedIndex = GetNextSelectable(-1); // ← 선택 불가 건너뛰기
            UpdateMenuHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedIndex = GetNextSelectable(+1); // ← 선택 불가 건너뛰기
            UpdateMenuHighlight();
        }
    }

    void HandleSelection()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        // 선택 불가면 리턴(에러 SFX 등을 여기서 재생해도 됨)
        if (!IsSelectable(selectedIndex))
            return;

        switch ((MenuType)selectedIndex)
        {
            case MenuType.NewGame:
                // (기존 그대로)
                m_boxOpenFlag = true;
                if (m_popup) m_popup.SetActive(true);
                m_newGamePopup.gameObject.SetActive(true);

                m_newGamePopup.Open(result =>
                {
                    if (result)
                    {
                        GManager.Instance.IsQuestManager?.StartQuest("Q_TM_0");
                        GManager.Instance.IsFirstPlay = true;
                        StartCoroutine(GManager.Instance.StartNewWithLoading(
                            "LoadingScene",           // 로딩 씬 이름
                            "MainGame",               // 최종 씬 이름
                            new Vector3(-70f, -60f, 0f),// 시작 위치 (원하면 Vector3.zero)
                            Quaternion.identity       // 시작 회전
                        ));
                    }
                    ClosePopup();
                });
                break;
            case MenuType.Continue:
                {
                    // 혹시라도 세이브가 사라진 경우 대비(보통은 IsSelectable로 이미 걸러짐)
                    if (!GManager.Instance || !GManager.Instance.HasSave())
                    {
                        RefreshContinueAvailability();
                        UpdateMenuHighlight();
                        break;
                    }

                    // 이어하기는 인트로 재생 대상이 아님
                    GManager.Instance.IsFirstPlay = false;

                    // 입력 잠금(TitleSc.Update에서 m_boxOpenFlag 체크 중)
                    m_boxOpenFlag = true;

                    // 로딩씬 경유 + 최종 씬에서 '월드 준비 완료'까지 블랙 유지 후 페이드인
                    StartCoroutine(GManager.Instance.ContinueWithLoadingBlocking(
                        fadeOutSec: 0.6f,
                        fadeInSec: 0.6f
                    ));
                    break;
                }

            case MenuType.Exit:
                // (기존 그대로)
                m_boxOpenFlag = true;
                if (m_popup) m_popup.SetActive(true);
                m_quitPopup.gameObject.SetActive(true);

                m_quitPopup.Open(result =>
                {
                    if (result) Application.Quit();
                    ClosePopup();
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
