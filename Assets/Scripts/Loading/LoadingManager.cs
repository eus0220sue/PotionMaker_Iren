using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] Slider m_loadingBar;
    private bool m_isLoading = false;

    public void StartLoading(string targetScene, bool playIntro)
    {
        if (m_isLoading)
        {
            Debug.LogWarning("[LoadingManager] 이미 로딩 중입니다!");
            return;
        }
        m_isLoading = true;

        StartCoroutine(LoadRoutine(targetScene, playIntro));
    }


    private IEnumerator LoadRoutine(string targetScene, bool playIntro)
    {
        var asyncOp = SceneManager.LoadSceneAsync(targetScene);
        asyncOp.allowSceneActivation = false;

        float fakeDuration = 4f; // 페이크 로딩 시간
        float elapsed = 0f;

        while (elapsed < fakeDuration)
        {
            elapsed += Time.deltaTime;
            float fakeProgress = Mathf.Clamp01(elapsed / fakeDuration);
            m_loadingBar.value = fakeProgress;

            yield return null;
        }

        // 페이크 로딩 완료되면 프로그래스 바 1로 세팅
        m_loadingBar.value = 1f;

        // 실제 씬이 로드 완료될 때까지 대기
        while (asyncOp.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        if (playIntro)
        {
            var introClip = Resources.Load<VideoClip>("Video/MV_Op");
            yield return StartCoroutine(GManager.Instance.IsVideoManager.PlayVideoRoutine(introClip));
        }

        asyncOp.allowSceneActivation = true;
        m_isLoading = false;
    }
}


