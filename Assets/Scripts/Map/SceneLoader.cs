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
<<<<<<< HEAD
        DontDestroyOnLoad(this.gameObject);

    }
    public static void LoadScene(string sceneName)
    {
        if (GManager.Instance?.mapBGMController != null)
            GManager.Instance.mapBGMController.StopBGM();

        GManager.Instance.StartCoroutine(GManager.Instance.IsFadeInOut.LoadSceneWithFade(sceneName));
=======

        DontDestroyOnLoad(gameObject);
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
<<<<<<< HEAD
        if (GManager.Instance != null)
        {
            GManager.Instance.AutoReferenceSceneObjects();
            if (GManager.Instance?.mapBGMController != null)
                GManager.Instance.mapBGMController.StopBGM();
            // ���� BGM ��� (Title, MainGame ��)
            switch (scene.name)
            {
                case "Title":
                    GManager.Instance.mapBGMController.PlayTitleBGM();
                    break;
                case "MainGame":
                    // �ڷ�ƾ�� �ƴ϶� �ٷ� ����
                    SetupMainGame();

                    break;
                default:
                    Debug.Log("[SceneLoader] BGM �̼��� ��: " + scene.name);
                    break;
=======

        if (GManager.Instance != null)
        {
            GManager.Instance.AutoReferenceSceneObjects();
        }
        StartCoroutine(DelayedStart(scene));
    }

    private IEnumerator DelayedStart(Scene scene)
    {
        yield return null;  // �� ������Ʈ�� ��� �ε�ǰ� Ȱ��ȭ�� ������ �� ������ ���

        if (scene.name == "LoadingScene")
        {
            GManager.Instance.IsLoadingManager.StartLoading(targetScene, playIntro);
        }
        else if (scene.name == "MainGame")
        {

            // �� ã��
            GameObject map = GameObject.Find("MapM0_CityHall"); // ���� �°� �̸� ����
            if (map != null)
            {
                BoxCollider2D mapCollider = map.GetComponent<BoxCollider2D>();
                if (mapCollider != null && GManager.Instance.IsCameraBase != null)
                {
                    var bounds = mapCollider.bounds;
                    GManager.Instance.IsCameraBase.SetCameraBounds(bounds.min, bounds.max);
                }

            }


            // ĳ���� ã�� �� ����
            var character = GameObject.Find("Character");
            if (character != null)
            {
                GManager.Instance.Setting(character);
            }


            // onAfterSceneLoad �׼��� ������ ȣ��
            if (onAfterSceneLoad != null)
            {
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
            }
            onAfterSceneLoad?.Invoke();
            onAfterSceneLoad = null;
        }
        else
        {
            onAfterSceneLoad?.Invoke();
            onAfterSceneLoad = null;
        }
<<<<<<< HEAD

        StartCoroutine(DelayedStart(scene));
    }

    private void SetupMainGame()
    {

        if (GManager.Instance?.mapBGMController != null)
        {
            GManager.Instance.mapBGMController.SetupMapsAfterSceneLoad();

            var mapObj = GameObject.Find("MapM0_CityHall");
            if (mapObj != null)
            {
                GManager.Instance.mapBGMController.PlayBGMForMap(mapObj);
            }
        }
    }

    private IEnumerator DelayedStart(Scene scene)
    {
        yield return null;  // �� ������Ʈ�� ��� �ε�ǰ� Ȱ��ȭ�� ������ �� ������ ���

        if (scene.name == "LoadingScene")
        {
            GManager.Instance.IsLoadingManager.StartLoading(targetScene, playIntro);
        }
        else if (scene.name == "MainGame")
        {

            // �� ã��
            GameObject map = GameObject.Find("MapM0_CityHall"); // ���� �°� �̸� ����
            if (map != null)
            {
                BoxCollider2D mapCollider = map.GetComponent<BoxCollider2D>();
                if (mapCollider != null && GManager.Instance.IsCameraBase != null)
                {
                    var bounds = mapCollider.bounds;
                    GManager.Instance.IsCameraBase.SetCameraBounds(bounds.min, bounds.max);
                }

            }


            // ĳ���� ã�� �� ����
            var character = GameObject.Find("Character");
            if (character != null)
            {
                GManager.Instance.Setting(character);
            }
        }
        else
        {
            onAfterSceneLoad?.Invoke();
            onAfterSceneLoad = null;
        }
=======
    }

    public static void LoadScene(string sceneName, bool isPlayIntro, Action afterLoad = null)
    {
        targetScene = sceneName;
        playIntro = isPlayIntro;
        onAfterSceneLoad = afterLoad;

        GManager.Instance.StartCoroutine(GManager.Instance.IsFadeInOut.LoadSceneWithFade("LoadingScene"));
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
    }

    public static void LoadScene(string sceneName, bool isPlayIntro, Action afterLoad = null)
    {
        targetScene = sceneName;
        playIntro = isPlayIntro;
        onAfterSceneLoad = afterLoad;

        GManager.Instance.StartCoroutine(GManager.Instance.IsFadeInOut.LoadSceneWithFade("LoadingScene"));
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
