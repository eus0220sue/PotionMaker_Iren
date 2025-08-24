using FunkyCode.Buffers;
using System; // Action 이벤트용
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GManager : MonoBehaviour
{

    // === [세이브 슬롯 / 오토세이브 옵션] ===
    [Header("Save/Load")]
    [SerializeField] private string saveFile = null;   // 비워두면 save_slot0.json

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
    [SerializeField] ShopUI m_shopUI;
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
    public InventoryManager IsInvenManager { get { return m_invenManager; } }
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
    /// 로딩씬
    /// </summary>
    [SerializeField] LoadingManager m_loadingManager = null;
    public LoadingManager IsLoadingManager { get { return m_loadingManager; } }

    [Header("영상")]
    [SerializeField] VideoManager m_videoManager;
    public VideoManager IsVideoManager { get { return m_videoManager; } }

    [Header("씬로더")]
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
    /// 세팅 플래그
    /// 맵 전환시 false로
    /// </summary>
    public bool IsSettingFlag { get; set; } = false;

    public bool m_uIPrev = false;

    [SerializeField] GameObject m_introVideoObj;
    [SerializeField] VideoPlayer m_videoPlayer;
    public bool IsFirstPlay = false;

    public event Action OnAfterLoad;   // 로드 후, 씬 오브젝트에 적용
    public event Action OnBeforeSave;  // 저장 직전, 씬 오브젝트가 상태를 키로 쓰기
                                       // 로딩 UI가 듣게 할 수 있는 진행도 이벤트 (0~1)
    public event Action<float> OnLoadingProgress;

    // 내부: 최종 씬이 로드될 때만 OnAfterLoad/인트로를 실행하기 위한 가드
    private string _pendingFinalScene = null;

    // 내부 상태
    private float _pendingFadeInSec = 0.6f;
    private bool _waitFadeInAfterReady = false;


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

        InitFirstMapBounds();

        if (currentMapGroup != null && mapBGMController != null)
        {
            mapBGMController.PlayBGMForMap(currentMapGroup);
        }
    }
    // 1) 기존 PlayIntroVideo / WaitForIntroEnd 전부 대체
    public void PlayIntroVideo(string clipResPath = "Video/OP_KR.ver")
    {
        StartCoroutine(Co_PlayIntroVideo(clipResPath));
    }

    private IEnumerator Co_PlayIntroVideo(string clipResPath)
    {
        // VideoManager 찾아서(비활성 포함) 활성화 보장
        var vm = IsVideoManager ?? FindObjectOfType<VideoManager>(true);
        if (vm == null)
        {
            Debug.LogWarning("[GManager] VideoManager를 찾지 못해 인트로 스킵");
            yield break;
        }
        if (!vm.gameObject.activeSelf) vm.gameObject.SetActive(true);

        // Resources/Video/OP_KR.ver 같은 경로에서 클립 로드
        var clip = Resources.Load<UnityEngine.Video.VideoClip>(clipResPath);
        if (clip == null)
        {
            Debug.LogWarning($"[GManager] 인트로 클립을 찾지 못했습니다: Resources/{clipResPath}");
            yield break;
        }

        // 코루틴은 GManager가 실행 → VideoManager 비활성이어도 OK
        yield return vm.PlayVideoRoutine(clip);

        Debug.Log("인트로 영상 끝! 게임 시작!");
    }
    public IEnumerator PlayIntroAndWait(string clipResPath = "Video/OP_KR.ver")
    {
        var vm = IsVideoManager ?? FindObjectOfType<VideoManager>(true);
        if (vm == null) yield break;

        if (!vm.gameObject.activeSelf) vm.gameObject.SetActive(true);

        var clip = Resources.Load<UnityEngine.Video.VideoClip>(clipResPath);
        if (clip == null) yield break;

        // 코루틴은 GManager(활성)에서 실행 → VideoManager 비활성이어도 OK
        yield return vm.PlayVideoRoutine(clip);
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

        // 컨트롤러
        IsUserController = FindObjectOfType<UserController>();

        // 맵
        currentMapGroup = GameObject.Find("MapM0_CityHall");

        // 페이드 인아웃
        m_fadeInOut = FindObjectOfType<FadeInOut>();

        // UI
        m_potionCraftUI = FindObjectOfType<PotionCraftUI>();

        m_shopUI = FindObjectOfType<ShopUI>();

        m_craftUI = FindObjectOfType<CraftUI>();

        m_dialogueUI = FindObjectOfType<DialogueUI>();

        m_inventoryUI = GameObject.Find("Inventory")?.GetComponent<InventoryUI>();

        m_hudUI = FindObjectOfType<HUD_UI>();

        m_getMessageUI = FindAnyObjectByType<GetMessage>();

        // 매니저
        m_invenManager = FindObjectOfType<InventoryManager>();

        m_UIManager = FindObjectOfType<UIManager>();

        m_exchangeManager = FindObjectOfType<ExchangeManager>();

        m_dialogueManager = FindObjectOfType<DialogueManager>();

        m_loadingManager = FindObjectOfType<LoadingManager>();

        m_videoManager = FindObjectOfType<VideoManager>(true);

        // UIManager 내부 필드도 자동 연결
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
    /// <summary>
    /// 세이브 키 목록
    /// </summary>
    public static class Keys
    {
        public const string Scene = "scene.current";
        public const string Gold = "player.gold";
        public const string Grade = "player.grade";    // (int)GradeType.Type
        public const string Pos = "player.pos";
        public const string Rot = "player.rot";

        public const string ActiveQuest = "quest.active";
        public const string QuestsJson = "quest.all";

        public const string OpenedSet = "map.opened";
        public const string DestroyedSet = "map.destroyed";
        public const string PickedSet = "map.picked";
    }

    /// <summary>
    /// 2) “처음하기 / 이어하기 / 저장” API 추가
    /// </summary>
    /// <param name="firstSceneName"></param>
    /// <param name="spawnPos"></param>
    /// <param name="spawnRot"></param>
    // 처음하기: SO 기본값→키 세팅, 씬 이동 후 OnAfterLoad
    public IEnumerator StartNewWithLoading(
        string loadingScene, string targetScene,
        Vector3 spawnPos, Quaternion spawnRot,
        float fadeOutSec = 0.6f, float fadeInSec = 0.6f)
    {
        // 1) 새게임 키 초기화
        SaveLoad.NewEmpty();
        SaveLoad.SetInt(Keys.Gold, 0);
        SaveLoad.SetInt(Keys.Grade, (int)GradeType.Type.Novice);
        SaveLoad.SetVector3(Keys.Pos, spawnPos);
        SaveLoad.SetQuaternion(Keys.Rot, spawnRot);
        SaveLoad.SetStringList(Keys.OpenedSet, new List<string>());
        SaveLoad.SetStringList(Keys.DestroyedSet, new List<string>());
        SaveLoad.SetStringList(Keys.PickedSet, new List<string>());
        SaveLoad.SetString(Keys.QuestsJson, "");
        SaveLoad.SetString(Keys.Scene, targetScene);

        // 2) 페이드 아웃
        if (IsFadeInOut != null) yield return IsFadeInOut.FadeOut(fadeOutSec);

        // 3) 최종 씬에서만 후처리하도록 가드
        _pendingFinalScene = targetScene;
        SceneManager.sceneLoaded += OnSceneLoadedThenApply;

        // 4)  기존 SceneLoader 파이프라인만 사용 (로딩씬 진입→LoadingManager가 비동기 로드)
        SceneLoader.LoadScene(targetScene, true);

        // (여기서는 기다릴 필요 없음; 로딩/전환은 SceneLoader/LoadingManager가 처리)
        // 페이드 인은 로딩 파이프라인/최종씬 연출 정책에 맞게 해당 측에서 처리
    }

    // 이어하기: JSON 로드→저장된 씬으로 이동→OnAfterLoad
    public bool ContinueGame()
    {
        if (!SaveLoad.Load(saveFile)) return false;

        string target = SaveLoad.GetString(Keys.Scene, "");
        if (!string.IsNullOrEmpty(target) &&
            target != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        {
            SceneManager.sceneLoaded += OnSceneLoadedThenApply;
            SceneManager.LoadScene(target);
        }
        else
        {
            OnAfterLoad?.Invoke();
        }
        return true;
    }
    private void OnSceneLoadedThenApply(Scene s, LoadSceneMode m)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedThenApply;

        // 로딩씬을 경유하는 동안엔 최종 씬이 아니면 패스
        if (!string.IsNullOrEmpty(_pendingFinalScene) && s.name != _pendingFinalScene)
        {
            SceneManager.sceneLoaded += OnSceneLoadedThenApply;
            return;
        }
        _pendingFinalScene = null;

        AutoReferenceSceneObjects(); // 씬 객체 캐싱(플레이어/카메라/UI 등)

        // 저장 적용(바인더들이 OnAfterLoad 구독해서 수행)
        OnAfterLoad?.Invoke();

        //  월드 준비 완료까지 기다렸다가 페이드 인
        if (_waitFadeInAfterReady)
            StartCoroutine(Co_WaitWorldReadyThenFadeIn());
    }
    public bool WasLastSaveOk { get; private set; } = false;
    public System.DateTime? LastSaveTime { get; private set; } = null;
    public event System.Action<bool> OnSaved; // true=성공, false=실패

    // 저장: 바인더가 OnBeforeSave에서 키에 상태를 써넣고, 파일 저장
    public void SaveNow()
    {
        bool ok = true;
        try
        {
            OnBeforeSave?.Invoke();
            SaveLoad.SetString(Keys.Scene, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            SaveLoad.Save(saveFile); // 여기서 예외 나면 catch로
        }
        catch (System.Exception e)
        {
            ok = false;
            Debug.LogError($"[Save] 실패: {e}");
        }
        finally
        {
            WasLastSaveOk = ok;
            if (ok) LastSaveTime = System.DateTime.Now;
            OnSaved?.Invoke(ok);
        }
    }

    /// <summary>
    /// 3) 편의 API (골드/등급/좌표) 추가
    /// </summary>
    /// <returns></returns>
    // 골드/등급
    public int GetGold() => SaveLoad.GetInt(Keys.Gold, 0);
    public void SetGold(int v) => SaveLoad.SetInt(Keys.Gold, Mathf.Max(0, v));
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        int g = GetGold(); if (g < amount) return false;
        SetGold(g - amount); return true;
    }
    public void AddGold(int amount) { if (amount > 0) SetGold(GetGold() + amount); }

    public GradeType.Type GetGrade() => (GradeType.Type)SaveLoad.GetInt(Keys.Grade, (int)GradeType.Type.Novice);
    public void SetGrade(GradeType.Type g) => SaveLoad.SetInt(Keys.Grade, (int)g);

    // 플레이어 위치
    public Vector3 GetPlayerPos() => SaveLoad.GetVector3(Keys.Pos, Vector3.zero);
    public Quaternion GetPlayerRot() => SaveLoad.GetQuaternion(Keys.Rot, Quaternion.identity);
    public void MarkPlayer(Transform t)
    {
        if (!t) return;
        SaveLoad.SetVector3(Keys.Pos, t.position);
        SaveLoad.SetQuaternion(Keys.Rot, t.rotation);
    }
    // GManager 클래스 안 아무 곳에 추가
    public bool HasSave()
    {
        string file = string.IsNullOrEmpty(saveFile) ? "save_slot0.json" : saveFile;
        string path = System.IO.Path.Combine(Application.persistentDataPath, file);
        return System.IO.File.Exists(path);
    }
    public IEnumerator ContinueWithLoadingBlocking(float fadeOutSec = 0.6f, float fadeInSec = 0.6f)
    {
        // 1) 저장 불러오기(메모리로만), 목표 씬 이름 얻기
        if (!SaveLoad.Load(saveFile))
        {
            Debug.LogWarning("[Continue] 저장 파일을 불러오지 못했습니다.");
            yield break;
        }
        string targetScene = SaveLoad.GetString(Keys.Scene, SceneManager.GetActiveScene().name);

        // 2) 페이드 아웃(완전 블랙 유지)
        if (IsFadeInOut != null)
            yield return IsFadeInOut.FadeOut(fadeOutSec);

        // 3) 최종 씬에서만 후처리하기 위한 가드 + 페이드인 지연 플래그
        _pendingFinalScene = targetScene;
        _pendingFadeInSec = fadeInSec;
        _waitFadeInAfterReady = true;
        SceneManager.sceneLoaded += OnSceneLoadedThenApply;

        // 4) 기존 로딩 파이프라인 사용 (로딩씬 → 비동기 로드)
        SceneLoader.LoadScene(targetScene, true);

        // 여기서부터는 OnSceneLoadedThenApply 안에서 마무리(FadeIn)됨
    }
    // 플레이어/카메라 준비까지 대기 후 페이드 인
    private IEnumerator Co_WaitWorldReadyThenFadeIn()
    {
        // 바인더들의 Awake/Start 1~2프레임 보장
        yield return null;
        yield return null;

        // 기준 데이터
        Vector3 savedPos = GetPlayerPos();

        float timeout = 3f; // 최대 대기(초)
        float t = 0f;

        while (t < timeout)
        {
            bool playerOk = (IsUserController != null && IsUserController.gameObject.activeInHierarchy);
            bool posOk = false;
            if (playerOk)
            {
                var p = IsUserController.transform.position;
                posOk = (Vector3.SqrMagnitude(p - savedPos) <= 0.01f); // 약 0.1m 이내
            }

            bool camOk = (Camera.main != null && Camera.main.isActiveAndEnabled);

            if (playerOk && posOk && camOk)
                break; // 준비 완료

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // 1프레임 더 안정화
        yield return null;

        if (IsFadeInOut != null)
            yield return IsFadeInOut.FadeIn(_pendingFadeInSec);

        _waitFadeInAfterReady = false;
    }
}
