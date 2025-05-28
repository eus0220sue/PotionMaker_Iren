using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    [Header("���� ��¿� ĵ����")]
    [SerializeField] private GameObject m_videoCanvas;
    [Header("���� �÷��̾�")]
    [SerializeField] private VideoPlayer m_videoPlayer;

    [Header("���� ����Ʈ")]
    [SerializeField] private List<VideoClip> videoClips;

    private bool m_isVideoPlaying = false;
    private bool videoEnd = false;

    void Awake()
    {
        m_videoPlayer.loopPointReached += OnVideoEnd;
        m_videoCanvas.SetActive(false);
    }

    // ���� �̸����� VideoClip ã��
    public VideoClip GetVideoClipByName(string clipName)
    {
        foreach (var clip in videoClips)
        {
            if (clip != null && clip.name == clipName)
                return clip;
        }
        Debug.LogWarning($"[VideoManager] ���� '{clipName}'��(��) ã�� �� �����ϴ�.");
        return null;
    }

    public IEnumerator PlayVideoRoutine(VideoClip clip, System.Action onComplete = null)
    {
        if (clip == null)
        {
            Debug.LogWarning("[VideoManager] ����� ������ �����ϴ�!");
            yield break;
        }
        videoEnd = false;


        yield return StartCoroutine(GManager.Instance.IsFadeInOut.FadeOut());

        m_videoPlayer.clip = clip;
        m_videoCanvas.SetActive(true);
        m_videoPlayer.Play();

        yield return StartCoroutine(GManager.Instance.IsFadeInOut.FadeIn());

        while (!videoEnd)
            yield return null;
        yield return StartCoroutine(GManager.Instance.IsFadeInOut.FadeOut());
        m_videoCanvas.SetActive(false);
        m_videoPlayer.Stop();
        yield return StartCoroutine(GManager.Instance.IsFadeInOut.FadeIn());

        onComplete?.Invoke();
    }
    private void OnVideoEnd(VideoPlayer vp)
    {
        videoEnd = true;
    }
}
