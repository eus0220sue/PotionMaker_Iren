using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM 소스")]
    [SerializeField] private AudioSource bgmSource;

    [Header("SFX 소스 (플레이어 + 시스템 공용)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Loop 사운드용 AudioSource (발소리 등)")]
    [SerializeField] private AudioSource loopSource;

    [Header("플레이어 사운드 목록")]
    public List<NamedSound> playerSounds;

    [Header("시스템 사운드 목록")]
    public List<NamedSound> systemSounds;

    [Header("Gathering 사운드 목록")]
    public List<NamedSound> gatherSounds;

    public float m_bgmVolume = 0.5f;
    public float m_systemVolume = 0.5f;

    private Dictionary<string, AudioClip> playerSoundDict;
    private Dictionary<string, AudioClip> systemSoundDict;
    private Dictionary<string, AudioClip> gatherSoundDict;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            playerSoundDict = playerSounds.ToDictionary(s => s.name, s => s.clip);
            systemSoundDict = systemSounds.ToDictionary(s => s.name, s => s.clip);
            gatherSoundDict = gatherSounds.ToDictionary(s => s.name, s => s.clip);
        }
    }

    public void PlayPlayerSound(string name)
    {
        if (playerSoundDict.TryGetValue(name, out var clip))
        {
            sfxSource.PlayOneShot(clip);
            Debug.Log($"[SoundManager] 플레이어 사운드 재생: {name}");
        }
        else
        {
            Debug.LogWarning($"[SoundManager] 플레이어 사운드 '{name}'이 존재하지 않음");
        }
    }


    public void PlaySystemSound(int index)
    {
        if (index < 0 || index >= systemSounds.Count)
        {
            Debug.LogWarning($"[SoundManager] 시스템 사운드 인덱스 {index} 범위 벗어남");
            return;
        }
        sfxSource.PlayOneShot(systemSounds[index].clip);
        Debug.Log($"[SoundManager] 시스템 사운드 재생: {systemSounds[index].name}");
    }
    public IEnumerator PlaySoundForDuration(AudioClip clip, float duration)
    {
        if (clip == null || sfxSource == null)
        {
            Debug.LogWarning("Clip 또는 sfxSource가 없습니다.");
            yield break;
        }

        sfxSource.clip = clip;
        sfxSource.Play();

        yield return new WaitForSeconds(duration);

        sfxSource.Stop();
        sfxSource.clip = null;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] PlayBGM: clip이 null입니다.");
            return;
        }

        if (bgmSource == null)
        {
            Debug.LogError("[SoundManager] bgmSource가 연결되어 있지 않습니다!");
            return;
        }

        Debug.Log($"[SoundManager] 재생 준비: {clip.name}");
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();

        Debug.Log($"[SoundManager]  새로운 BGM 재생 시작: {clip.name}");
    }
    public void PlayGatherSound(int index)
    {
        if (index < 0 || index >= gatherSounds.Count)
        {
            Debug.LogWarning($"[SoundManager] Gather 사운드 인덱스 {index} 범위 벗어남");
            return;
        }
        AudioClip clip = gatherSounds[index].clip;
        sfxSource.PlayOneShot(clip);
        Debug.Log($"[SoundManager] Gather 사운드 재생: {gatherSounds[index].name}");
    }
    public void StopAllSFX()
    {
        if (sfxSource.isPlaying)
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }
    }
    // 루프 재생 (발소리 등)
    public void PlayPlayerSoundLoop(int index)
    {
        if (index < 0 || index >= playerSounds.Count)
        {
            Debug.LogWarning($"[SoundManager] 시스템 사운드 인덱스 {index} 범위 벗어남");
            return;
        }

        if (loopSource.isPlaying && loopSource.clip == playerSounds[index].clip)
            return; // 이미 재생 중

        loopSource.clip = playerSounds[index].clip;
        loopSource.loop = true;
        loopSource.Play();

        Debug.Log($"[SoundManager] 시스템 사운드 루프 재생 시작: {playerSounds[index].name}");
    }

    // 루프 재생 정지
    public void StopPlayerSound(int index)
    {
        if (loopSource.isPlaying && loopSource.clip == playerSounds[index].clip)
        {
            loopSource.Stop();
            loopSource.clip = null;
            Debug.Log($"[SoundManager] 시스템 사운드 루프 정지: {playerSounds[index].name}");
        }
    }
    public void SetBGMVolume(float volume)
    {
        m_bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
            bgmSource.volume = m_bgmVolume;
        Debug.Log($"[SoundManager] BGM 볼륨 조절: {m_bgmVolume}");
    }

    public void SetSystemVolume(float volume)
    {
        m_systemVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = m_systemVolume;
        Debug.Log($"[SoundManager] 시스템 사운드 볼륨 조절: {m_systemVolume}");
    }
}

[System.Serializable]
public class NamedSound
{
    public string name;
    public AudioClip clip;
}
