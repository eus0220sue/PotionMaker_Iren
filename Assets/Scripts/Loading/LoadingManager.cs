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
            Debug.LogWarning("[LoadingManager] �̹� �ε� ���Դϴ�!");
            return;
        }
        m_isLoading = true;

        StartCoroutine(LoadRoutine(targetScene, playIntro));
    }


    private IEnumerator LoadRoutine(string targetScene, bool playIntro)
    {
        var asyncOp = SceneManager.LoadSceneAsync(targetScene);
        asyncOp.allowSceneActivation = false;

        float fakeDuration = 4f; // ����ũ �ε� �ð�
        float elapsed = 0f;

        while (elapsed < fakeDuration)
        {
            elapsed += Time.deltaTime;
            float fakeProgress = Mathf.Clamp01(elapsed / fakeDuration);
            m_loadingBar.value = fakeProgress;

            yield return null;
        }

        // ����ũ �ε� �Ϸ�Ǹ� ���α׷��� �� 1�� ����
        m_loadingBar.value = 1f;

        // ���� ���� �ε� �Ϸ�� ������ ���
        while (asyncOp.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        if (playIntro)
        {
<<<<<<< HEAD
            var introClip = Resources.Load<VideoClip>("Video/OP_KR.ver");
=======
            var introClip = Resources.Load<VideoClip>("Video/MV_Op");
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
            yield return StartCoroutine(GManager.Instance.IsVideoManager.PlayVideoRoutine(introClip));
        }

        asyncOp.allowSceneActivation = true;
        m_isLoading = false;
    }
}


