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
<<<<<<< HEAD
    public GameObject EscapeKeyUIPrefab;
    public GameObject m_escapeKeyUIInstance;
    public bool escapeKeyUIOpenFlag = false;

    public bool EscapeKeyUIOpenFlag
    {
        get => escapeKeyUIOpenFlag;
        set
        {
            escapeKeyUIOpenFlag = value;
            Debug.Log($"[UIManager] EscapeKeyUIOpenFlag set to: {escapeKeyUIOpenFlag}");
        }
    }
=======

>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
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
<<<<<<< HEAD
        if (EscapeKeyUIPrefab != null && m_escapeKeyUIInstance == null)
        {
            m_escapeKeyUIInstance = Instantiate(EscapeKeyUIPrefab);
            m_escapeKeyUIInstance.SetActive(false);
            Debug.Log("[UIManager] EscapeKeyUI �ν��Ͻ� ���� �Ϸ�");
        }
        else
        {
            Debug.LogWarning("[UIManager] EscapeKeyUIPrefab�� �Ҵ���� �ʾҰų� �̹� �ν��Ͻ��� �����մϴ�.");
        }
    }

=======
    }
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1

    /// <summary>
    /// �÷��� ����
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
    /*void Awake()
    {
        if (CraftUI == null) CraftUI = GameObject.Find("CraftUI");
        if (PotionCraftUI == null) PotionCraftUI = GameObject.Find("PotionCraftUI");
        if (ShopUI == null) ShopUI = GameObject.Find("ShopUI");
        if (DialogueUI == null) DialogueUI = GameObject.Find("DialogueUI");
    }
    */

    void Update()
    {
<<<<<<< HEAD
        if (m_escapeKeyUIInstance == null)
        {
            if (EscapeKeyUIPrefab != null)
            {
                m_escapeKeyUIInstance = Instantiate(EscapeKeyUIPrefab);
                m_escapeKeyUIInstance.SetActive(false);
                Debug.Log("[UIManager] EscapeKeyUI �ν��Ͻ� ���� �Ϸ� (Update)");
            }
            else
            {
                Debug.LogWarning("[UIManager] EscapeKeyUIPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
                return;
            }
        }
        HandleBookUIOpen();
        HandleQuestUIOpen();

        // ESCŰ ���� ����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC ����");

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (sceneName == "Title")
            {
                // Ÿ��Ʋ ���� ��� TitleSc �˾� �켱 ó��
                var titleSc = FindObjectOfType<TitleSc>();
                if (titleSc != null && titleSc.IsPopupOpen())  // TitleSc�� �˾� ���� ���� üũ �޼��� �ʿ�
                {
                    titleSc.ClosePopup();  // �˾� �ݱ� �޼��� �ʿ�
                    return;  // ESC �г� ����� �ʰ� ����
                }

                // �˾� ������ ESC �г� ��� �� �÷��� ������Ʈ
                if (m_escapeKeyUIInstance.activeSelf)
                {
                    m_escapeKeyUIInstance.SetActive(false);
                    EscapeKeyUIOpenFlag = false;  // �÷��� ����
                }
                else
                {
                    m_escapeKeyUIInstance.SetActive(true);
                    EscapeKeyUIOpenFlag = true;   // �÷��� ����
                }

                return;
            }


            if (EscapeKeyUIOpenFlag)
            {
                Debug.Log("ESCâ �������� -> �ݱ�");
                HideEscapeKeyUI();
            }
            else if (!UIOpenFlag)
            {
                Debug.Log("���� UI ���� -> ESCâ �ѱ�");
                ShowEscapeKeyUI();
            }
            else
            {
                Debug.Log("�ٸ� UI �������� -> �ݱ�");
                CloseUI();
                HideEscapeKeyUI();
            }

=======
        HandleBookUIOpen();
        HandleQuestUIOpen();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUI();
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
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
        // ��ȭ ���� �� �����ִ� �ٸ� UI �ݱ�

        DialogueOpenFlag = true;

        if (DialogueUI == null)
        {
            Debug.LogError("[UIManager] DialogueUI�� ������� �ʾҽ��ϴ�.");
            return;
        }

        DialogueUI.SetActive(true);

        if (startNode == null)
        {
            Debug.LogWarning("[UIManager] startNode�� null�Դϴ�.");
            return;
        }

        GManager.Instance.IsDialogueManager.StartDialogue(startNode);
    }

<<<<<<< HEAD

=======
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
    private void HandleBookUIOpen()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // BookUI�� ���� ���������� �ݱ�, �ƴϸ� ����
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

                // ���� �ڷ�ƾ �� ���� ����
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
            // �ٸ� UI�鵵 ���� ���⿡ �߰�
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
            GManager.Instance.IsUserController.isInteracting = false; // ���ͷ��� ���� ���·� ����

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
