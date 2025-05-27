using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    [SerializeField] private Image m_fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (m_fadeImage != null)
        {
            // 게임 시작할 때 fadeImage를 투명하게 초기화
            Color color = m_fadeImage.color;
            color.a = 0f; // 알파를 0으로
            m_fadeImage.color = color;
        }
        else
        {
        }
    }

    public IEnumerator FadeOut(float duration = 1f)
    {
        float timer = 0f;
        Color color = m_fadeImage.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / duration);
            m_fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        m_fadeImage.color = color;
    }


    public IEnumerator FadeIn(float duration = 1f)
    {
        float timer = 0f;
        Color color = m_fadeImage.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / duration);
            m_fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        m_fadeImage.color = color;
    }

    public IEnumerator LoadSceneWithFade(string sceneName)
    {
        //  FadeOut 시작
        yield return StartCoroutine(FadeOut());

        //  실제 씬 전환
        SceneManager.LoadScene(sceneName);

        //  FadeIn 시작
        yield return StartCoroutine(FadeIn());
    }

}
