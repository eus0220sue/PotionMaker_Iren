using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneLoader : MonoBehaviour
{
    private static Action onAfterSceneLoad;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneLoader] 씬 로드됨: {scene.name}");

        GManager.Instance?.AutoReferenceSceneObjects();

        if (scene.name == "MainGame")
        {
            GManager.Instance.InitFirstMapBounds();

            GameObject character = GameObject.Find("Character");
            if (character != null)
            {
                GManager.Instance?.Setting(character);
                Debug.Log("[SceneLoader] GManager.Setting 호출 완료");
            }
            else
            {
                Debug.LogError("[SceneLoader] MainGame에 Character가 없음");
            }
        }

        onAfterSceneLoad?.Invoke(); // 단 한 번만 실행
        onAfterSceneLoad = null;    // 실행 후 제거
    }

    public static void LoadScene(string sceneName, Action afterLoad = null)
    {
        onAfterSceneLoad = afterLoad; // 씬 로드 후 실행될 콜백 저장
        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(string sceneName)
    {
        onAfterSceneLoad = null; // 콜백 없이 로드
        SceneManager.LoadScene(sceneName);
    }
}
