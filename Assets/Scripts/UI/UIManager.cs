using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject CraftUI;
    public GameObject PotionCraftUI;
    public GameObject ShopUI;
    public GameObject DialogueUI;
    public GameObject BookUI;
    public GameObject QuestUIOpen;
    public GameObject QuestUIClosed;
    public GameObject EscapeKeyUIPrefab;
    public GameObject m_escapeKeyUIInstance;
    public bool escapeKeyUIOpenFlag = false;

    public bool EscapeKeyUIOpenFlag
    {
        get => escapeKeyUIOpenFlag;
        set
        {
            escapeKeyUIOpenFlag = value;
        }
    }
    void Awake()
    {
        if (QuestUIOpen == null)
        {
            var parent = GameObject.Find("QuestHUD");
            if (parent != null)
            {
                QuestUIOpen = parent.transform.Find("QuestUIOpen")?.gameObject;
                QuestUIClosed = parent.transform.Find("QuestUIClosed")?.gameObject;
            }
        }
        if (EscapeKeyUIPrefab != null && m_escapeKeyUIInstance == null)
        {
            m_escapeKeyUIInstance = Instantiate(EscapeKeyUIPrefab);
            m_escapeKeyUIInstance.SetActive(false);
        }
        else
        {
        }
    }


    /// <summary>
    /// 플래그 세팅
    /// </summary>
    public bool UIOpenFlag
    {
        get
        {
            return CraftUIOpenFlag
                || PotionCraftUIOpenFlag
                || DialogueOpenFlag
                || ShopUIOpenFlag
                || BookUIOpenFlag
                || GManager.Instance.IsInventoryUI.isOpen;

        }
    }
    public bool CraftUIOpenFlag = false;
    public bool PotionCraftUIOpenFlag = false;
    public bool DialogueOpenFlag = false;
    public bool ShopUIOpenFlag = false;
    public bool BookUIOpenFlag = false;
    public bool QuestUIOpenFlag = false;


    void Update()
    {
        if (m_escapeKeyUIInstance == null)
        {
            if (EscapeKeyUIPrefab != null)
            {
                m_escapeKeyUIInstance = Instantiate(EscapeKeyUIPrefab);
                m_escapeKeyUIInstance.SetActive(false);
            }
            else
            {
                return;
            }
        }
        HandleBookUIOpen();
        HandleQuestUIOpen();

        // ESC키 누름 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (sceneName == "Title")
            {
                // 타이틀 씬일 경우 TitleSc 팝업 우선 처리
                var titleSc = FindObjectOfType<TitleSc>();
                if (titleSc != null && titleSc.IsPopupOpen())  // TitleSc에 팝업 열림 상태 체크 메서드 필요
                {
                    titleSc.ClosePopup();  // 팝업 닫기 메서드 필요
                    return;  // ESC 패널 띄우지 않고 종료
                }

                // 팝업 없으면 ESC 패널 토글 및 플래그 업데이트
                if (m_escapeKeyUIInstance.activeSelf)
                {
                    m_escapeKeyUIInstance.SetActive(false);
                    EscapeKeyUIOpenFlag = false;  // 플래그 꺼줌
                }
                else
                {
                    m_escapeKeyUIInstance.SetActive(true);
                    EscapeKeyUIOpenFlag = true;   // 플래그 켜줌
                }

                return;
            }


            if (EscapeKeyUIOpenFlag)
            {
                HideEscapeKeyUI();
            }
            else if (!UIOpenFlag)
            {
                ShowEscapeKeyUI();
            }
            else
            {
                CloseUI();
                HideEscapeKeyUI();
            }

        }
    }
    public void OpenCraftUI()
    {
        CraftUIOpenFlag = true;
        CraftUI.SetActive(true);
        GManager.Instance.IsCraftUI.InitCraftUI();
    }
    public void OpenPotionCraftUI()
    {
        PotionCraftUIOpenFlag = true;
        PotionCraftUI.SetActive(true);
        GManager.Instance.IsPotionCraftUI.InitPotionUI();

    }
    public void OpenShopUI()
    {
        ShopUIOpenFlag = true;
        ShopUI.SetActive(true);
        GManager.Instance.IsShopUI.InitShopUI();
    }
    public void OpenBookUI()
    {
        BookUIOpenFlag = true;
        BookUI.SetActive(true);
//        GManager.Instance.IsBookUI.InitBookUI();
    }

    public void OpenDialogueUI(DialogueNode startNode)
    {
        // 대화 시작 전 열려있는 다른 UI 닫기

        DialogueOpenFlag = true;

        if (DialogueUI == null)
        {
            return;
        }

        DialogueUI.SetActive(true);

        if (startNode == null)
        {
            return;
        }

        GManager.Instance.IsDialogueManager.StartDialogue(startNode);
    }


    private void HandleBookUIOpen()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // BookUI가 현재 열려있으면 닫기, 아니면 열기
            if (!BookUIOpenFlag)
            {
                OpenBookUI();
            }
            else
            {
                BookUIOpenFlag=false;
                BookUI.SetActive(false);
                return;
            }
        }
    }


    public void OpenQuestUI()
    {
        QuestUIOpenFlag = true;
        QuestUIOpen.SetActive(true);
        QuestUIClosed.SetActive(false);
    }
    public void HandleQuestUIOpen()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if ((!QuestUIOpenFlag))
            {
                OpenQuestUI();
            }
            else
            {
                QuestUIOpenFlag = false;
                QuestUIOpen.SetActive(false);
                QuestUIClosed.SetActive(true);
                return;
            }
        }
    }
    public void CloseUI()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CraftUIOpenFlag)
            {
                CraftUIOpenFlag = false;
                CraftUI.SetActive(false);

                // 제작 코루틴 및 사운드 중지
                var craftUIComp = CraftUI.GetComponent<CraftUI>();
                if (craftUIComp != null)
                {
                    craftUIComp.StopCraftCoroutine();
                }
            }
            if (PotionCraftUIOpenFlag)
            {
                PotionCraftUIOpenFlag = false;
                PotionCraftUI.SetActive(false);
            }
            if (ShopUIOpenFlag)
            {
                ShopUIOpenFlag = false;
                ShopUI.SetActive(false);
            }
            // 다른 UI들도 추후 여기에 추가
            if (DialogueOpenFlag)
            {
                DialogueOpenFlag = false;
                DialogueUI.SetActive(false);
                GManager.Instance.IsDialogueManager.EndDialogue();

            }
            if (BookUIOpenFlag)
            {
                BookUIOpenFlag = false;
                BookUI.SetActive(false);
            }
            GManager.Instance.IsUserController.isInteracting = false; // 인터렉션 가능 상태로 복원

        }
    }
    public void CloseAllUIExceptDialogue()
    {
        if (CraftUIOpenFlag)
        {
            CraftUIOpenFlag = false;
            CraftUI.SetActive(false);
            var craftUIComp = CraftUI.GetComponent<CraftUI>();
            if (craftUIComp != null)
            {
                craftUIComp.StopCraftCoroutine();
            }
        }

        if (PotionCraftUIOpenFlag)
        {
            PotionCraftUIOpenFlag = false;
            PotionCraftUI.SetActive(false);
        }

        if (ShopUIOpenFlag)
        {
            ShopUIOpenFlag = false;
            ShopUI.SetActive(false);
        }

        if (BookUIOpenFlag)
        {
            BookUIOpenFlag = false;
            BookUI.SetActive(false);
        }

        if (QuestUIOpenFlag)
        {
            QuestUIOpenFlag = false;
            QuestUIOpen.SetActive(false);
            QuestUIClosed.SetActive(true);
        }
    }
    public void ShowEscapeKeyUI()
    {
        if (m_escapeKeyUIInstance != null)
        {
            m_escapeKeyUIInstance.SetActive(true);
            EscapeKeyUIOpenFlag = true;
        }
    }

    public void HideEscapeKeyUI()
    {
        if (m_escapeKeyUIInstance != null)
        {
            m_escapeKeyUIInstance.SetActive(false);
            EscapeKeyUIOpenFlag = false;
        }
    }
}
