using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    private static string targetScene;
    private static Action onAfterSceneLoad;
    private static bool playIntro;

    void Awake()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        DontDestroyOnLoad(gameObject);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (GManager.Instance != null)
        {
            GManager.Instance.AutoReferenceSceneObjects();
        }
        StartCoroutine(DelayedStart(scene));
    }

    private IEnumerator DelayedStart(Scene scene)
    {
        yield return null;  // 씬 오브젝트가 모두 로드되고 활성화될 때까지 한 프레임 대기

        if (scene.name == "LoadingScene")
        {
            GManager.Instance.IsLoadingManager.StartLoading(targetScene, playIntro);
        }
        else if (scene.name == "MainGame")
        {

            // 맵 찾기
            GameObject map = GameObject.Find("MapM0_CityHall"); // 씬에 맞게 이름 조정
            if (map != null)
            {
                BoxCollider2D mapCollider = map.GetComponent<BoxCollider2D>();
                if (mapCollider != null && GManager.Instance.IsCameraBase != null)
                {
                    var bounds = mapCollider.bounds;
                    GManager.Instance.IsCameraBase.SetCameraBounds(bounds.min, bounds.max);
                }

            }


            // 캐릭터 찾기 및 세팅
            var character = GameObject.Find("Character");
            if (character != null)
            {
                GManager.Instance.Setting(character);
            }


            // onAfterSceneLoad 액션이 있으면 호출
            if (onAfterSceneLoad != null)
            {
            }
            onAfterSceneLoad?.Invoke();
            onAfterSceneLoad = null;
        }
        else
        {
            onAfterSceneLoad?.Invoke();
            onAfterSceneLoad = null;
        }
    }

    public static void LoadScene(string sceneName, bool isPlayIntro, Action afterLoad = null)
    {
        targetScene = sceneName;
        playIntro = isPlayIntro;
        onAfterSceneLoad = afterLoad;

        GManager.Instance.StartCoroutine(GManager.Instance.IsFadeInOut.LoadSceneWithFade("LoadingScene"));
    }
}
