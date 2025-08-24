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

    public IEnumerator LoadRoutine(string targetScene, bool playIntro)
    {
        // 1) 메인 씬 비동기 로드 시작 (활성화 잠금)
        var op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        // 로딩바 초기화
        if (m_loadingBar) m_loadingBar.value = 0f;
        yield return null;

        // 2) 인트로: 새 게임일 때만 로딩씬에서 재생
        if (playIntro && GManager.Instance != null && GManager.Instance.IsFirstPlay)
        {
            yield return GManager.Instance.PlayIntroAndWait("Video/OP_KR.ver");
            GManager.Instance.IsFirstPlay = false;
        }

        // 3) 로딩 완료까지 대기(진행도 UI 갱신)
        while (op.progress < 0.9f)
        {
            if (m_loadingBar)
            {
                // 부드럽게 채우고 싶으면 Lerp, 즉각이면 바로 대입
                float p = op.progress / 0.9f;
                m_loadingBar.value = Mathf.MoveTowards(m_loadingBar.value, p, Time.unscaledDeltaTime);
            }
            yield return null;
        }
        if (m_loadingBar) m_loadingBar.value = 1f;

        // 4) 메인 씬 활성화
        op.allowSceneActivation = true;

        // (선택) 다음 로딩을 위해 플래그 리셋
        m_isLoading = false;
    }
}

