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
    [SerializeField] private string saveFile = "save_slot0"; // 인스펙터에서 슬롯 지정

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
    private bool _isContinueFlow = false;
    private bool _pendingNewGameClear = false;

    /// <summary>
    /// 싱글톤 인스턴스
    /// </summary>
    public static GManager Instance { get; private set; } = null;
    public event System.Action OnAfterLoadLate; // 씬/매니저/UI 초기화가 끝난 직후 알림
    private Coroutine _saveCoalescer;

    private void Awake()
    {

        if (GManager.Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AutoWireBinds();
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

    private IEnumerator Co_PostLoadRefresh()
    {
        yield return null; // 씬 내 Start/Init 대기
        yield return null;


        if (_pendingNewGameClear)
        {
            var invMgr = FindObjectOfType<InventoryManager>(true);
            invMgr?.ClearAllSlots();               
            _pendingNewGameClear = false;
        }

        var invBind = FindObjectOfType<InventoryBind_GM>(true);
        invBind?.LoadAndApply();

        // 판매탭 비주얼(안전겸 한 번 더)
        ShopUI shop = null;
        float t = 0f, timeout = 3f;
        while (t < timeout)
        {
            shop = FindObjectOfType<ShopUI>(true);
            if (shop != null) break;
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        if (shop != null)
        {
            shop.UpdateSellUI();
        }

        OnAfterLoadLate?.Invoke();
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
            yield break;
        }
        if (!vm.gameObject.activeSelf) vm.gameObject.SetActive(true);

        // Resources/Video/OP_KR.ver 같은 경로에서 클립 로드
        var clip = Resources.Load<UnityEngine.Video.VideoClip>(clipResPath);
        if (clip == null)
        {
            yield break;
        }

        // 코루틴은 GManager가 실행 → VideoManager 비활성이어도 OK
        yield return vm.PlayVideoRoutine(clip);

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
    private string GetSaveFileName()
    {
        var name = string.IsNullOrEmpty(saveFile) ? "save_slot0" : saveFile;
        if (!name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            name += ".json";
        return name;
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
    public bool ForceQuestResetOnce { get; set; } = false;  // 새로하기 직후 1회 강제 초기화 플래그


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
        SaveLoad.NewEmpty();
        SaveLoad.SetInt(Keys.Gold, 300);
        SaveLoad.SetInt(Keys.Grade, (int)GradeType.Type.Novice);
        SaveLoad.SetVector3(Keys.Pos, spawnPos);
        SaveLoad.SetQuaternion(Keys.Rot, spawnRot);
        SaveLoad.SetStringList(Keys.OpenedSet, new List<string>());
        SaveLoad.SetStringList(Keys.DestroyedSet, new List<string>());
        SaveLoad.SetStringList(Keys.PickedSet, new List<string>());
        SaveLoad.SetString(Keys.QuestsJson, "");
        SaveLoad.SetString(Keys.Scene, targetScene);

        // ★ 추가
        IsFirstPlay = true;
        ForceQuestResetOnce = true;
        _pendingNewGameClear = true;
        _isContinueFlow = false;
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
        _isContinueFlow = true;
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
        AutoWireBinds();

        // 저장 적용(바인더들이 OnAfterLoad 구독해서 수행)
        OnAfterLoad?.Invoke();

        //  월드 준비 완료까지 기다렸다가 페이드 인
        if (_waitFadeInAfterReady)
            StartCoroutine(Co_WaitWorldReadyThenFadeIn());
        StartCoroutine(Co_PostLoadRefresh());
        RefreshInventoryVisualsAfterLoad();
        StartCoroutine(Co_ApplySavedPlayerTransformWhenReady());
        if (_isContinueFlow)
        {
            // 바인더 적용이 끝났더라도, 플레이어/바운드/카메라가 준비될 때까지 잠깐 대기 후 스냅
            StartCoroutine(Co_SnapCameraAfterBounds());
        }
    }

    private IEnumerator Co_SnapCameraAfterBounds()
    {
        // 이어하기에서만 의미 있음
        if (!_isContinueFlow) yield break;

        float timeout = 3f;
        // CameraBase, Player 준비 기다림(+ 가능하면 바운드까지)
        while (timeout > 0f)
        {
            bool hasCam = (IsCameraBase != null);
            bool hasUser = (IsUserTrans != null);
            bool hasBounds = false;

            if (hasCam)
            {
                // CameraBase.cs에 HasValidBounds() 추가해두었죠. (없으면 이 줄은 제거해도 됩니다)
                hasBounds = IsCameraBase.HasValidBounds();
            }

            // 바운드가 아직 없어도 SnapToWorld가 언클램프 스냅을 해주도록 되어 있으니, cam+user만 준비되면 진행
            if (hasCam && hasUser) break;

            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        var t = IsUserTrans;
        if (IsCameraBase == null)
        {
            _isContinueFlow = false;
            yield break;
        }

        // 저장된 좌표 로드
        var savedPos = SaveLoad.GetVector3(Keys.Pos, t ? t.position : Vector3.zero);
        var savedRot = SaveLoad.GetQuaternion(Keys.Rot, t ? t.rotation : Quaternion.identity);

        // 타겟 지정
        IsCameraBase.SetTarget(t);

        // 바운드가 준비되었다면 클램프, 아니면 언클램프 스냅( CameraBase.SnapToWorld 내부에서 처리 )
        IsCameraBase.SnapToWorld(savedPos);

        // 1회성 플래그 해제
        _isContinueFlow = false;
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
            // ★ 저장 직전 최신 좌표 강제 기록
            var t = ResolvePlayerTransform();
            if (t != null)
            {
                MarkPlayer(t);
            }
            else
            {
            }

            OnBeforeSave?.Invoke(); // 바인더들이 키에 상태 쓰는 지점

            var file = GetSaveFileName();            //  파일명 통일
            SaveLoad.Save(file);                     //  존재하지 않는 API 호출 제거

        }
        catch (System.Exception e)
        {
            ok = false;
        }
        finally
        {
            WasLastSaveOk = ok;
            if (ok) LastSaveTime = System.DateTime.Now;
            OnSaved?.Invoke(ok);
            FindObjectOfType<InventoryBind_GM>(true)?.SaveSnapshot();

        }
    }
    private Transform ResolvePlayerTransform()
    {
        if (IsUserTrans != null) return IsUserTrans;

        // 1) UserController 기준
        var uc = FindObjectOfType<UserController>(true);
        if (uc != null)
        {
            m_userObj = uc.gameObject;      // ★ 프로퍼티가 아니라 내부 필드에 세팅
            return uc.transform;
        }

        // 2) Player 태그
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
        {
            m_userObj = go;                  // ★ 내부 필드에 세팅
            return go.transform;
        }

        return null;
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
        string file = GetSaveFileName();
        string path = System.IO.Path.Combine(Application.persistentDataPath, file);
        return System.IO.File.Exists(path);
    }
    public IEnumerator ContinueWithLoadingBlocking(float fadeOutSec = 0.6f, float fadeInSec = 0.6f)
    {
        _isContinueFlow = true;
        // 1) 저장 불러오기(메모리로만), 목표 씬 이름 얻기
        if (!SaveLoad.Load(saveFile))
        {
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

    public void RefreshInventoryVisualsAfterLoad()
    {
        StartCoroutine(Co_RefreshInventoryVisuals());
    }

    private IEnumerator Co_ApplySavedPlayerTransformWhenReady()
    {
        // 바인더 Awake/Start 보장
        yield return null;

        Vector3 savedPos = GetPlayerPos();
        Quaternion savedRot = GetPlayerRot();

        // 플레이어 등장 대기
        float timeout = 3f, t = 0f;
        while (t < timeout && (IsUserController == null || !IsUserController.gameObject.activeInHierarchy))
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        var trans = IsUserController ? IsUserController.transform : null;
        if (trans != null)
        {
            trans.SetPositionAndRotation(savedPos, savedRot);
        }
    }



    private System.Collections.IEnumerator Co_RefreshInventoryVisuals()
    {
        // 씬 안의 Awake/Start가 다 돌 시간을 1~2프레임 준다
        yield return null;
        yield return null;

        // 1) 인벤 스냅샷을 메모리에 적용 (한 번 더 안전하게)
        var invBind = UnityEngine.Object.FindObjectOfType<InventoryBind_GM>(true);
        invBind?.LoadAndApply();

        // 2) 인벤토리 UI(가 있으면) → 강제 리프레시
        //    (InventoryManager에서 평소엔 Add/Remove/Consume 때 UpdateUI()를 호출하지만,
        //     이어하기 직후엔 수동으로 한 번 쏴주는 게 안전)
        IsInventoryUI?.UpdateUI();

        // 3) 상점 판매 탭은 인벤 미러링이니 같이 갱신
        var shop = UnityEngine.Object.FindObjectOfType<ShopUI>(true);
        shop?.UpdateSellUI();

    }

    // 프레임 끝(또는 짧은 딜레이 후) 1회만 SaveNow() 실행
    public void SaveSoon(float delaySec = 0f)
    {
        if (_saveCoalescer != null) return;               // 이미 예약되어 있으면 중복 예약 방지
        _saveCoalescer = StartCoroutine(Co_SaveSoon(delaySec));
    }

    private IEnumerator Co_SaveSoon(float delaySec)
    {
        if (delaySec > 0f) yield return new WaitForSeconds(delaySec);
        yield return null;                                 // 프레임 끝까지 대기 → 여러 요청을 1회로 합침
        SaveNow();                                         // ★ 실제 저장 (OnBeforeSave → Binder.Capture 호출됨)
        _saveCoalescer = null;
    }

    // 앱 종료/일시정지 시 안전 저장
    private void OnApplicationQuit()
    {
        SaveNow();
    }

    private PlayerBind_GM _playerBind;
    private InventoryBind_GM _inventoryBind;
    private QuestBind_GM _questBind;

    private void AutoWireBinds()
    {
        _playerBind = FindObjectOfType<PlayerBind_GM>(true);
        _inventoryBind = FindObjectOfType<InventoryBind_GM>(true);
        _questBind = FindObjectOfType<QuestBind_GM>(true);

        // 중복구독 방지 후 구독
        if (_playerBind != null)
        {
            OnBeforeSave -= _playerBind.Capture; OnBeforeSave += _playerBind.Capture;
            OnAfterLoad -= _playerBind.Apply; OnAfterLoad += _playerBind.Apply;
        }

        if (_inventoryBind != null)
        {
            OnBeforeSave -= _inventoryBind.SaveSnapshot; OnBeforeSave += _inventoryBind.SaveSnapshot;
            OnAfterLoad -= _inventoryBind.LoadAndApply; OnAfterLoad += _inventoryBind.LoadAndApply;
        }

        if (_questBind != null)
        {
            OnBeforeSave -= _questBind.Capture; OnBeforeSave += _questBind.Capture;
            OnAfterLoad -= _questBind.Apply; OnAfterLoad += _questBind.Apply;
        }
    }

}
