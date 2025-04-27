using FunkyCode.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Header("사운드 관련")]
    public MapBGMController mapBGMController;
    [SerializeField] private SoundManager m_soundManager;
    public SoundManager IsSoundManager { get { return m_soundManager; } }

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
    /// 세팅 플래그
    /// 맵 전환시 false로
    /// </summary>
    public bool IsSettingFlag { get; set; } = false;
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
    /// <summary>
    /// 세팅
    /// </summary>
    /// <param name="argUserObj">유저 오브젝트</param>
    public void Setting(GameObject argUserObj)
    {
        m_userObj = argUserObj;
        IsSettingFlag = true;
    }
    /// <summary>
    /// 유저 컨트롤러
    /// </summary>
    public UserController IsUserController { get; private set; } = null;
    /// <summary>
    /// 유저 트렌스폼
    /// </summary>
    public Transform IsUserTrans
    {
        get { return m_userObj != null ? m_userObj.transform : null; }
    }

    private void Start()
    {
        //  Start()에서 자동으로 Setting() 호출
        GameObject m_character = GameObject.Find("Character");
        if (m_character != null)
        {
            Setting(m_character);
        }
        InitFirstMapBounds();
        Debug.Log($"[GManager] mapBGMController 연결 상태: {(mapBGMController != null ? "정상" : "null")}");

        if (currentMapGroup != null && mapBGMController != null)
        {
            mapBGMController.PlayBGMForMap(currentMapGroup);
        }
        else
        {
            Debug.LogWarning("[GManager] 초기 맵이나 mapBGMController가 설정되지 않음");
        }
    }
    public void SetInventoryUI(InventoryUI ui)
    {
        m_inventoryUI = ui; 
    }

    public void SetTPFlag(bool isOn)
    {
        TPFlag = isOn;
        Debug.Log($"[TPFlag] 상태 변경됨 → {(isOn ? "ON (이동 불가)" : "OFF (이동 가능)")}");
    }
    public void StartTPAfterTeleport()
    {
        if (m_fadeInOut != null)
        {
            StartCoroutine(TPAfterTeleportCoroutine());
        }
        else
        {
            Debug.LogError("[GManager] FadeFadeInOut 연결 안 됨!");
        }
    }

    private IEnumerator TPAfterTeleportCoroutine()
    {
        Debug.Log("[GManager] 페이드 인 시작");

        // 바로 FadeIn 시작
        yield return StartCoroutine(m_fadeInOut.FadeIn());

        Debug.Log("[GManager] 페이드 인 완료, TPFlag OFF");

        SetTPFlag(false);
    }
    private void InitFirstMapBounds()
    {
        if (currentMapGroup == null)
        {
            Debug.LogWarning("[GManager] 시작할 때 currentMapGroup이 연결되어 있지 않습니다.");
            return;
        }

        BoxCollider2D collider = currentMapGroup.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            Debug.LogWarning("[GManager] 시작할 때 currentMapGroup에 BoxCollider2D가 없습니다.");
            return;
        }

        Bounds bounds = collider.bounds;
        Vector2 min = bounds.min;
        Vector2 max = bounds.max;

        if (IsCameraBase != null)
        {
            IsCameraBase.SetCameraBounds(min, max);
            Debug.Log($"[GManager] 게임 시작 시 카메라 제한 자동 설정: Min {min} / Max {max}");
        }
        else
        {
            Debug.LogError("[GManager] CameraBase 연결이 되어 있지 않습니다.");
        }
    }
}
