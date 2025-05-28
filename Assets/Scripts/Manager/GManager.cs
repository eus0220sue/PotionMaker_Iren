using FunkyCode.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GManager : MonoBehaviour
{
    [SerializeField]public bool TPFlag = false;
    [Header("���� �� �׷�")]
    public GameObject currentMapGroup; // ���� Ȱ��ȭ�� ���� �巡���ؼ� ���

    [Header("���̵� ��Ʈ�ѷ�")]
    [SerializeField] private FadeInOut m_fadeInOut;
    public FadeInOut IsFadeInOut { get { return m_fadeInOut; } }

    [Header("ī�޶� ����")]
    [SerializeField] private CameraBase m_cameraBase;
    public CameraBase IsCameraBase { get { return m_cameraBase; } }
    [Header("���� ���� ����")]
    [SerializeField] private PotionCraftUI m_potionCraftUI;
    public PotionCraftUI IsPotionCraftUI { get { return m_potionCraftUI; } }
    [Header("���� ����")]
    [SerializeField] ShopUI m_shopUI;
    public ShopUI IsShopUI { get { return m_shopUI; } }
    [Header("���� ����")]
    public MapBGMController mapBGMController;

    [SerializeField] private SoundManager m_soundManager;
    public SoundManager IsSoundManager { get { return m_soundManager; } }
    /// <summary>
    /// ���� ��Ʈ�ѷ�
    /// </summary>
    public UserController IsUserController = null;
    /// <summary>
    /// ���� Ʈ������
    /// </summary>
    public Transform IsUserTrans
    {
        get { return m_userObj != null ? m_userObj.transform : null; }
    }

    /// <summary>
    /// ���� ���� ������Ʈ
    /// </summary>
    GameObject m_userObj = null;
    /// <summary>
    /// �κ��丮
    /// </summary>
    [SerializeField] InventoryUI m_inventoryUI = null;
    public InventoryUI IsInventoryUI { get { return m_inventoryUI; } }
    /// <summary>
    /// �κ� �Ŵ���
    /// </summary>
    public InventoryManager IsInvenManager { get { return m_invenManager; } }
    /// <summary>
    /// �κ��丮 �Ŵ���
    /// </summary>
    [SerializeField] InventoryManager m_invenManager = null;
    /// <summary>
    /// UI�Ŵ���
    /// </summary>
    [SerializeField] UIManager m_UIManager = null;
    public UIManager IsUIManager { get { return m_UIManager; } }
    /// <summary>
    /// ���� UI
    /// </summary>
    [SerializeField] CraftUI m_craftUI = null;
    public CraftUI IsCraftUI { get { return m_craftUI; } }
    /// <summary>
    /// ��ȯ �Ŵ���
    /// </summary>
    [SerializeField] ExchangeManager m_exchangeManager = null;
    public ExchangeManager IsExchangeManager { get {return  m_exchangeManager;} }
    /// <summary>
    /// ��ȭ �Ŵ���
    /// </summary>
    [SerializeField] DialogueManager m_dialogueManager = null;
    public DialogueManager IsDialogueManager { get { return m_dialogueManager; } }

    /// <summary>
    /// ����Ʈ �Ŵ���
    /// </summary>
    [SerializeField] QuestManager m_questManager = null;
    public QuestManager IsQuestManager { get { return m_questManager; } }

    /// <summary>
    /// ���� UI
    /// </summary>
    [SerializeField] DialogueUI m_dialogueUI = null;
    public DialogueUI IsDialougeUI { get { return m_dialogueUI; } }

    /// <summary>
    /// �ε���
    /// </summary>
    [SerializeField] LoadingManager m_loadingManager = null;
    public LoadingManager IsLoadingManager { get { return m_loadingManager; } }

    [Header("����")]
    [SerializeField] VideoManager m_videoManager;
    public VideoManager IsVideoManager { get { return m_videoManager; } }

    [Header("���δ�")]
    [SerializeField] SceneLoader m_sceneLoader;
    public SceneLoader IsSceneLoader { get { return m_sceneLoader; } }

    [Header("HUD")]
    [SerializeField] HUD_UI m_hudUI;
    public HUD_UI IsHUDUI { get { return m_hudUI; } }

    [Header("HUD")]
    [SerializeField] ErrorMessage m_errorMessage;
    public ErrorMessage IsErrorMessage { get { return m_errorMessage; } }


    [Header("HUD")]
    [SerializeField] GetMessage m_getMessageUI;
    public GetMessage IsGetMessageUI { get { return m_getMessageUI; } }
    
    /// <summary>
    /// ���� �÷���
    /// �� ��ȯ�� false��
    /// </summary>
    public bool IsSettingFlag { get; set; } = false;

    public bool m_uIPrev = false;

    [SerializeField] GameObject m_introVideoObj;
    [SerializeField] VideoPlayer m_videoPlayer;
    public bool IsFirstPlay = false;

    /// <summary>
    /// �̱��� �ν��Ͻ�
    /// </summary>
    public static GManager Instance { get; private set; } = null;

    private void Awake()
    {

        if (GManager.Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (GManager.Instance.IsFirstPlay)
        {
            PlayIntroVideo();
            GManager.Instance.IsFirstPlay = false; // �� �� ����!
        }
        if (currentScene == "MainGame")
        {
            GameObject m_character = GameObject.Find("Character");

            if (m_character != null)
            {
                Setting(m_character); // ���⼭ IsUserTrans ������
            }
            else
            {
            }
        }

        InitFirstMapBounds();

        if (currentMapGroup != null && mapBGMController != null)
        {
            mapBGMController.PlayBGMForMap(currentMapGroup);
        }
    }
    void PlayIntroVideo()
    {
        m_introVideoObj.SetActive(true);
        m_videoPlayer.Play();
        StartCoroutine(WaitForIntroEnd());
    }

    IEnumerator WaitForIntroEnd()
    {
        while (m_videoPlayer.isPlaying)
            yield return null;

        m_introVideoObj.SetActive(false);
        Debug.Log("��Ʈ�� ���� ��! ���� ����!");
    }
    void Update()
    {
        if (IsUIManager == null || IsUserController == null) return;

        bool isUI = IsUIManager.UIOpenFlag;

        if (isUI != m_uIPrev)
        {
            IsUserController.SetMoveFlag(!isUI);
            m_uIPrev = isUI;
        }
    }    /// <summary>
         /// ����
         /// </summary>
         /// <param name="argUserObj">���� ������Ʈ</param>
    public void Setting(GameObject argUserObj)
    {
        m_userObj = argUserObj;
        IsSettingFlag = true;
    }


    public void SetInventoryUI(InventoryUI ui)
    {
        m_inventoryUI = ui; 
    }
    public void SetTPFlag(bool isOn)
    {
        TPFlag = isOn;
    }
    public void StartTPAfterTeleport()
    {
        if (m_fadeInOut != null)
        {
            StartCoroutine(TPAfterTeleportCoroutine());
        }
    }

    private IEnumerator TPAfterTeleportCoroutine()
    {
        yield return StartCoroutine(m_fadeInOut.FadeIn());
        SetTPFlag(false);
    }
    public void InitFirstMapBounds()
    {
        if (currentMapGroup == null || IsCameraBase == null)
        {
            return;
        }

        var collider = currentMapGroup.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            return;
        }

        Bounds bounds = collider.bounds;
        IsCameraBase.SetCameraBounds(bounds.min, bounds.max);

    }
    public void AutoReferenceSceneObjects()
    {
        // ī�޶�
        m_cameraBase = FindObjectOfType<CameraBase>();

        // ��Ʈ�ѷ�
        IsUserController = FindObjectOfType<UserController>();

        // ��
        currentMapGroup = GameObject.Find("MapM0_CityHall");

        // ���̵� �ξƿ�
        m_fadeInOut = FindObjectOfType<FadeInOut>();

        // UI
        m_potionCraftUI = FindObjectOfType<PotionCraftUI>();

        m_shopUI = FindObjectOfType<ShopUI>();

        m_craftUI = FindObjectOfType<CraftUI>();

        m_dialogueUI = FindObjectOfType<DialogueUI>();

        m_inventoryUI = GameObject.Find("Inventory")?.GetComponent<InventoryUI>();

        m_hudUI = FindObjectOfType<HUD_UI>();

        m_getMessageUI = FindAnyObjectByType<GetMessage>();

        // �Ŵ���
        m_invenManager = FindObjectOfType<InventoryManager>();

        m_UIManager = FindObjectOfType<UIManager>();

        m_exchangeManager = FindObjectOfType<ExchangeManager>();

        m_dialogueManager = FindObjectOfType<DialogueManager>();

        m_loadingManager = FindObjectOfType<LoadingManager>();
        // UIManager ���� �ʵ嵵 �ڵ� ����
        if (m_UIManager != null)
        {
            m_UIManager.CraftUI = GameObject.Find("CraftUI");
            m_UIManager.PotionCraftUI = GameObject.Find("PotionCraftUI");
            m_UIManager.ShopUI = GameObject.Find("ShopUI");
            m_UIManager.DialogueUI = GameObject.Find("DialogueUI");
            m_UIManager.BookUI = GameObject.Find("BookUI");
            m_UIManager.QuestUIOpen = GameObject.Find("QuestUIOpen");
            m_UIManager.QuestUIClosed = GameObject.Find("QuestUIClosed");
        }
    }

}
