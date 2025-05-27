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
