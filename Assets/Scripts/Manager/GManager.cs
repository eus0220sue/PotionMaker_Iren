using FunkyCode.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GManager : MonoBehaviour
{
    [SerializeField]public bool TPFlag = false;
    [Header("현재 맵 그룹")]
    public GameObject currentMapGroup; // 현재 활성화된 맵을 드래그해서 등록

    [Header("페이드 컨트롤러")]
    [SerializeField] private FadeInOut m_fadeInOut;
    public FadeInOut IsFadeInOut { get { return m_fadeInOut; } }

    [Header("카메라 관련")]
    [SerializeField] private CameraBase m_cameraBase;
    public CameraBase IsCameraBase { get { return m_cameraBase; } }
    [Header("포션 제작 관련")]
    [SerializeField] private PotionCraftUI m_potionCraftUI;
    public PotionCraftUI IsPotionCraftUI { get { return m_potionCraftUI; } }
    [Header("상점 관련")]
    [SerializeField] private ShopUI m_shopUI;
    public ShopUI IsShopUI { get { return m_shopUI; } }
    [Header("사운드 관련")]
    public MapBGMController mapBGMController;
    [SerializeField] private SoundManager m_soundManager;
    public SoundManager IsSoundManager { get { return m_soundManager; } }
    /// <summary>
    /// 유저 컨트롤러
    /// </summary>
    public UserController IsUserController = null;
    /// <summary>
    /// 유저 트렌스폼
    /// </summary>
    public Transform IsUserTrans
    {
        get { return m_userObj != null ? m_userObj.transform : null; }
    }

    /// <summary>
    /// 유저 게임 오브젝트
    /// </summary>
    GameObject m_userObj = null;
    /// <summary>
    /// 인벤토리
    /// </summary>
    [SerializeField] InventoryUI m_inventoryUI = null;
    public InventoryUI IsInventoryUI { get { return m_inventoryUI; } }
    /// <summary>
    /// 인벤 매니저
    /// </summary>
    public InventoryManager IsinvenManager { get { return m_invenManager; } }
    /// <summary>
    /// 인벤토리 매니저
    /// </summary>
    [SerializeField] InventoryManager m_invenManager = null;
    /// <summary>
    /// UI매니저
    /// </summary>
    [SerializeField] UIManager m_UIManager = null;
    public UIManager IsUIManager { get { return m_UIManager; } }
    /// <summary>
    /// 제작 UI
    /// </summary>
    [SerializeField] CraftUI m_craftUI = null;
    public CraftUI IsCraftUI { get { return m_craftUI; } }
    /// <summary>
    /// 교환 매니저
    /// </summary>
    [SerializeField] ExchangeManager m_exchangeManager = null;
    public ExchangeManager IsExchangeManager { get {return  m_exchangeManager;} }
    /// <summary>
    /// 대화 매니저
    /// </summary>
    [SerializeField] DialogueManager m_dialogueManager = null;
    public DialogueManager IsDialogueManager { get { return m_dialogueManager; } }

    /// <summary>
    /// 퀘스트 매니저
    /// </summary>
    [SerializeField] QuestManager m_questManager = null;
    public QuestManager IsQuestManager { get { return m_questManager; } }

    /// <summary>
    /// 제작 UI
    /// </summary>
    [SerializeField] DialogueUI m_dialogueUI = null;
    public DialogueUI IsDialougeUI { get { return m_dialogueUI; } }


    /// <summary>
    /// 세팅 플래그
    /// 맵 전환시 false로
    /// </summary>
    public bool IsSettingFlag { get; set; } = false;

    public bool m_uIPrev = false;

    /// <summary>
    /// 싱글톤 인스턴스
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

        if (currentScene == "MainGame")
        {
            GameObject m_character = GameObject.Find("Character");

            if (m_character != null)
            {
                Setting(m_character); // 여기서 IsUserTrans 설정됨
            }
            else
            {
            }
        }
        else
        {
        }

        InitFirstMapBounds();

        if (currentMapGroup != null && mapBGMController != null)
        {
            mapBGMController.PlayBGMForMap(currentMapGroup);
        }
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
         /// 세팅
         /// </summary>
         /// <param name="argUserObj">유저 오브젝트</param>
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
        // 카메라
        m_cameraBase = FindObjectOfType<CameraBase>();
        Debug.Log($"[AutoRef] CameraBase: {(m_cameraBase != null ? "참조됨" : "NULL")}");

        // 컨트롤러
        IsUserController = FindObjectOfType<UserController>();
        Debug.Log($"[AutoRef] UserController: {(IsUserController != null ? "참조됨" : "NULL")}");

        // 맵
        currentMapGroup = GameObject.Find("MapM0_CityHall");
        Debug.Log($"[AutoRef] currentMapGroup: {(currentMapGroup != null ? "참조됨" : "NULL")}");

        // 페이드 인아웃
        m_fadeInOut = FindObjectOfType<FadeInOut>();
        Debug.Log($"[AutoRef] FadeInOut: {(m_fadeInOut != null ? "참조됨" : "NULL")}");

        // UI
        m_potionCraftUI = FindObjectOfType<PotionCraftUI>();
        Debug.Log($"[AutoRef] PotionCraftUI: {(m_potionCraftUI != null ? "참조됨" : "NULL")}");

        m_shopUI = FindObjectOfType<ShopUI>();
        Debug.Log($"[AutoRef] ShopUI: {(m_shopUI != null ? "참조됨" : "NULL")}");

        m_craftUI = FindObjectOfType<CraftUI>();
        Debug.Log($"[AutoRef] CraftUI: {(m_craftUI != null ? "참조됨" : "NULL")}");

        m_dialogueUI = FindObjectOfType<DialogueUI>();
        Debug.Log($"[AutoRef] DialogueUI: {(m_dialogueUI != null ? "참조됨" : "NULL")}");

        m_inventoryUI = GameObject.Find("Inventory")?.GetComponent<InventoryUI>();
        Debug.Log($"[AutoRef] InventoryUI: {(m_inventoryUI != null ? "참조됨" : "NULL")}");

        // 매니저
        m_invenManager = FindObjectOfType<InventoryManager>();
        Debug.Log($"[AutoRef] InventoryManager: {(m_invenManager != null ? "참조됨" : "NULL")}");

        m_UIManager = FindObjectOfType<UIManager>();
        Debug.Log($"[AutoRef] UIManager: {(m_UIManager != null ? "참조됨" : "NULL")}");

        m_exchangeManager = FindObjectOfType<ExchangeManager>();
        Debug.Log($"[AutoRef] ExchangeManager: {(m_exchangeManager != null ? "참조됨" : "NULL")}");

        m_dialogueManager = FindObjectOfType<DialogueManager>();
        Debug.Log($"[AutoRef] DialogueManager: {(m_dialogueManager != null ? "참조됨" : "NULL")}");

        // UIManager 내부 필드도 자동 연결
        if (m_UIManager != null)
        {
            m_UIManager.CraftUI = GameObject.Find("CraftUI");
            m_UIManager.PotionCraftUI = GameObject.Find("PotionCraftUI");
            m_UIManager.ShopUI = GameObject.Find("ShopUI");
            m_UIManager.DialogueUI = GameObject.Find("DialogueUI");

            Debug.Log($"[AutoRef] UIManager 내부 CraftUI: {(m_UIManager.CraftUI != null ? "참조됨" : "NULL")}");
            Debug.Log($"[AutoRef] UIManager 내부 PotionCraftUI: {(m_UIManager.PotionCraftUI != null ? "참조됨" : "NULL")}");
            Debug.Log($"[AutoRef] UIManager 내부 ShopUI: {(m_UIManager.ShopUI != null ? "참조됨" : "NULL")}");
            Debug.Log($"[AutoRef] UIManager 내부 DialogueUI: {(m_UIManager.DialogueUI != null ? "참조됨" : "NULL")}");
        }
    }

}
