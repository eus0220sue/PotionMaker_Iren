using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader _instance;

    private static string targetScene;
    private static Action onAfterSceneLoad;
    private static bool playIntro;

    void Awake()
    {
        // 싱글톤 & 루트에 DontDestroyOnLoad 적용 (경고 방지)
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        var rootGO = (transform.parent == null) ? gameObject : transform.root.gameObject;
        if (rootGO != gameObject)
        DontDestroyOnLoad(rootGO);
    }

    public static void LoadScene(string sceneName)
    {
        if (GManager.Instance?.mapBGMController != null)
            GManager.Instance.mapBGMController.StopBGM();

        GManager.Instance.StartCoroutine(GManager.Instance.IsFadeInOut.LoadSceneWithFade(sceneName));
    }

    public static void LoadScene(string sceneName, bool isPlayIntro, Action afterLoad = null)
    {
        targetScene = sceneName;
        playIntro = isPlayIntro;
        onAfterSceneLoad = afterLoad;

        if (GManager.Instance?.mapBGMController != null)
            GManager.Instance.mapBGMController.StopBGM();

        GManager.Instance.StartCoroutine(GManager.Instance.IsFadeInOut.LoadSceneWithFade("LoadingScene"));
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GManager.Instance != null)
        {
            GManager.Instance.AutoReferenceSceneObjects();

            if (GManager.Instance?.mapBGMController != null)
                GManager.Instance.mapBGMController.StopBGM();

            switch (scene.name)
            {
                case "Title":
                    GManager.Instance.mapBGMController.PlayTitleBGM();
                    break;

                case "MainGame":
                    SetupMainGame();
                    break;

                default:
                    break;
            }
        }

        StartCoroutine(DelayedStart(scene));
    }

    private void SetupMainGame()
    {
        if (GManager.Instance?.mapBGMController != null)
        {
            GManager.Instance.mapBGMController.SetupMapsAfterSceneLoad();

            // 기존: 특정 맵 이름으로 BGM 매칭
            var mapObj = GameObject.Find("MapM0_CityHall");
            if (mapObj != null)
            {
                GManager.Instance.mapBGMController.PlayBGMForMap(mapObj);
            }
        }
    }

    private IEnumerator DelayedStart(Scene scene)
    {
        // 씬 오브젝트 준비 대기
        yield return null;

        if (scene.name == "LoadingScene")
        {
            GManager.Instance.IsLoadingManager.StartLoading(targetScene, playIntro);
        }
        else if (scene.name == "MainGame")
        {
            // 1) 저장 좌표 불러오기
            var savedPos = SaveLoad.GetVector3(Keys.Pos, Vector3.zero);

            // 2) 카메라 바운드: AutoSetter가 있으면 우선 사용
            var autoSetter = FindObjectOfType<CameraBoundsAutoSetter>(true);
            if (autoSetter != null)
            {
                autoSetter.ForceUpdateNow(savedPos);
            }
            else
            {
                // 없으면 저장좌표가 포함된 Region 찾기 → 바운드 적용
                ApplyBoundsForPosition(savedPos);
            }

            // 3) 캐릭터 찾기 및 GManager에 세팅
            var character = GameObject.Find("Character");
            if (character != null)
            {
                GManager.Instance.Setting(character);
            }

            // 나머지 after-load 훅
            onAfterSceneLoad?.Invoke();
            onAfterSceneLoad = null;
        }
        else
        {
            onAfterSceneLoad?.Invoke();
            onAfterSceneLoad = null;
        }
    }

    /// <summary>
    /// 저장 좌표를 포함하는 CameraBoundsRegion을 찾아 카메라 바운드 지정.
    /// 없으면 저장 좌표 주변으로 임시 바운드 적용.
    /// </summary>
    private void ApplyBoundsForPosition(Vector3 worldPos)
    {
        var camBase = GManager.Instance?.IsCameraBase;
        if (camBase == null)
        {
            return;
        }

        var regions = FindObjectsOfType<CameraBoundsRegion>(true);
        CameraBoundsRegion chosen = null;
        foreach (var r in regions)
        {
            if (r != null && r.Contains(worldPos))
            {
                chosen = r;
                break;
            }
        }

        if (chosen != null)
        {
            var (min, max) = chosen.GetMinMax();
            camBase.SetCameraBounds(min, max);
        }
        else
        {
            Vector2 half = new Vector2(50f, 30f);
            Vector2 min = (Vector2)worldPos - half;
            Vector2 max = (Vector2)worldPos + half;
            camBase.SetCameraBounds(min, max);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
