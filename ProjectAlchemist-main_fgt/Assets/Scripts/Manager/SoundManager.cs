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

    [Header("플레이어 사운드 목록")]
    public List<NamedSound> playerSounds;

    [Header("시스템 사운드 목록")]
    public List<NamedSound> systemSounds;

    private Dictionary<string, AudioClip> playerSoundDict;
    private Dictionary<string, AudioClip> systemSoundDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            playerSoundDict = playerSounds.ToDictionary(s => s.name, s => s.clip);
            systemSoundDict = systemSounds.ToDictionary(s => s.name, s => s.clip);
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


    public void PlaySystemSound(string name)
    {
        if (systemSoundDict.TryGetValue(name, out var clip))
        {
            sfxSource.PlayOneShot(clip);
            Debug.Log($"[SoundManager] 시스템 사운드 재생: {name}");
        }
        else
        {
            Debug.LogWarning($"[SoundManager] 시스템 사운드 '{name}'이 존재하지 않음");
        }
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
}

[System.Serializable]
public class NamedSound
{
    public string name;
    public AudioClip clip;
}
