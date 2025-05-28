using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    [Header("영상 출력용 캔버스")]
    [SerializeField] private GameObject m_videoCanvas;
    [Header("비디오 플레이어")]
    [SerializeField] private VideoPlayer m_videoPlayer;

    [Header("영상 리스트")]
    [SerializeField] private List<VideoClip> videoClips;

    private bool m_isVideoPlaying = false;
    private bool videoEnd = false;

    void Awake()
    {
        m_videoPlayer.loopPointReached += OnVideoEnd;
        m_videoCanvas.SetActive(false);
    }

    // 영상 이름으로 VideoClip 찾기
    public VideoClip GetVideoClipByName(string clipName)
    {
        foreach (var clip in videoClips)
        {
            if (clip != null && clip.name == clipName)
                return clip;
        }
        Debug.LogWarning($"[VideoManager] 영상 '{clipName}'을(를) 찾을 수 없습니다.");
        return null;
    }

    public IEnumerator PlayVideoRoutine(VideoClip clip, System.Action onComplete = null)
    {
        if (clip == null)
        {
            Debug.LogWarning("[VideoManager] 재생할 비디오가 없습니다!");
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
