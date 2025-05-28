using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM �ҽ�")]
    [SerializeField] private AudioSource bgmSource;

    [Header("SFX �ҽ� (�÷��̾� + �ý��� ����)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Loop ����� AudioSource (�߼Ҹ� ��)")]
    [SerializeField] private AudioSource loopSource;

    [Header("�÷��̾� ���� ���")]
    public List<NamedSound> playerSounds;

    [Header("�ý��� ���� ���")]
    public List<NamedSound> systemSounds;

    [Header("Gathering ���� ���")]
    public List<NamedSound> gatherSounds;

<<<<<<< HEAD
    public float m_bgmVolume = 0.5f;
    public float m_systemVolume = 0.5f;

=======
>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
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
            Debug.Log($"[SoundManager] �÷��̾� ���� ���: {name}");
        }
        else
        {
            Debug.LogWarning($"[SoundManager] �÷��̾� ���� '{name}'�� �������� ����");
        }
    }


    public void PlaySystemSound(int index)
    {
        if (index < 0 || index >= systemSounds.Count)
        {
            Debug.LogWarning($"[SoundManager] �ý��� ���� �ε��� {index} ���� ���");
            return;
        }
        sfxSource.PlayOneShot(systemSounds[index].clip);
        Debug.Log($"[SoundManager] �ý��� ���� ���: {systemSounds[index].name}");
    }
    public IEnumerator PlaySoundForDuration(AudioClip clip, float duration)
    {
        if (clip == null || sfxSource == null)
        {
            Debug.LogWarning("Clip �Ǵ� sfxSource�� �����ϴ�.");
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
            Debug.LogWarning("[SoundManager] PlayBGM: clip�� null�Դϴ�.");
            return;
        }

        if (bgmSource == null)
        {
            Debug.LogError("[SoundManager] bgmSource�� ����Ǿ� ���� �ʽ��ϴ�!");
            return;
        }

        Debug.Log($"[SoundManager] ��� �غ�: {clip.name}");
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();

        Debug.Log($"[SoundManager]  ���ο� BGM ��� ����: {clip.name}");
    }
    public void PlayGatherSound(int index)
    {
        if (index < 0 || index >= gatherSounds.Count)
        {
            Debug.LogWarning($"[SoundManager] Gather ���� �ε��� {index} ���� ���");
            return;
        }
        AudioClip clip = gatherSounds[index].clip;
        sfxSource.PlayOneShot(clip);
        Debug.Log($"[SoundManager] Gather ���� ���: {gatherSounds[index].name}");
    }
    public void StopAllSFX()
    {
        if (sfxSource.isPlaying)
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }
    }
    // ���� ��� (�߼Ҹ� ��)
    public void PlayPlayerSoundLoop(int index)
    {
        if (index < 0 || index >= playerSounds.Count)
        {
            Debug.LogWarning($"[SoundManager] �ý��� ���� �ε��� {index} ���� ���");
            return;
        }

        if (loopSource.isPlaying && loopSource.clip == playerSounds[index].clip)
            return; // �̹� ��� ��

        loopSource.clip = playerSounds[index].clip;
        loopSource.loop = true;
        loopSource.Play();

        Debug.Log($"[SoundManager] �ý��� ���� ���� ��� ����: {playerSounds[index].name}");
    }

    // ���� ��� ����
    public void StopPlayerSound(int index)
    {
        if (loopSource.isPlaying && loopSource.clip == playerSounds[index].clip)
        {
            loopSource.Stop();
            loopSource.clip = null;
            Debug.Log($"[SoundManager] �ý��� ���� ���� ����: {playerSounds[index].name}");
        }
    }
<<<<<<< HEAD
    public void SetBGMVolume(float volume)
    {
        m_bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
            bgmSource.volume = m_bgmVolume;
        Debug.Log($"[SoundManager] BGM ���� ����: {m_bgmVolume}");
    }

    public void SetSystemVolume(float volume)
    {
        m_systemVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = m_systemVolume;
        Debug.Log($"[SoundManager] �ý��� ���� ���� ����: {m_systemVolume}");
    }
=======


>>>>>>> 642329f552b3543e6b6f0ae4156dbb3ba21693b1
}

[System.Serializable]
public class NamedSound
{
    public string name;
    public AudioClip clip;
}
