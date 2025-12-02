using UnityEngine;
using System.Collections.Generic;

public class MapBGMController : MonoBehaviour
{

    [Header("Title BGM")]
    public AudioClip titleBGM;

    [SerializeField] private AudioSource bgmSource;  // 여기에 AudioSource 추가

    [System.Serializable]
    public class MapEntry
    {
        public GameObject mapObject;
        public AudioClip overrideBGM; // null이면 그룹 BGM 사용
    }

    [System.Serializable]
    public class MapGroupEntry
    {
        public MapGroupType groupType;
        public AudioClip groupBGM;
        public List<MapEntry> maps; // 해당 그룹에 속한 맵들
    }

    [Header("그룹별 맵 & BGM 매핑")]
    public List<MapGroupEntry> mapGroups;

    private AudioClip currentClip;

    void Start()
    {
        if (mapGroups != null && mapGroups.Count > 0)
        {
            var group = mapGroups[0];

            if (group.groupBGM == null)
            {
                group.groupBGM = Resources.Load<AudioClip>("Sounds/Town_bgm"); // 경로에 맞게 수정
            }
        }
    }

    public void PlayTitleBGM()
    {
        if (titleBGM != null)
        {
            SoundManager.Instance.PlayBGM(titleBGM);
        }
        else
        {
        }
    }
    public void PlayBGMForMap(GameObject mapObject)
    {
        if (mapObject == null)
        {
            return;
        }


        foreach (var group in mapGroups)
        {
            foreach (var entry in group.maps)
            {
                if (entry.mapObject == mapObject)
                {

                    AudioClip clipToPlay = group.groupBGM;

                    if (clipToPlay == null || string.IsNullOrWhiteSpace(clipToPlay.name))
                    {
                        return;
                    }
                    if (currentClip == clipToPlay && bgmSource.isPlaying)
                    {
                        return;
                    }

                    //  같은 BGM이지만 멈춘 상태라면 다시 Play
                    currentClip = clipToPlay;
                    SoundManager.Instance.PlayBGM(clipToPlay);

                    return;
                }
            }
        }

    }
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }
    }
    public void SetupMapsAfterSceneLoad()
    {
        // M0 그룹 처리
        var m0Group = mapGroups.Find(g => g.groupType == MapGroupType.M0_Town);
        if (m0Group != null)
        {
            m0Group.maps.Clear();
            var m0Maps = GameObject.FindGameObjectsWithTag("M0");
            foreach (var obj in m0Maps)
            {
                MapEntry entry = new MapEntry();
                entry.mapObject = obj;
                entry.overrideBGM = null; // 필요 시 개별 BGM 지정 가능
                m0Group.maps.Add(entry);
            }
        }

        // M1 그룹 처리
        var m1Group = mapGroups.Find(g => g.groupType == MapGroupType.M1_Forest);
        if (m1Group != null)
        {
            m1Group.maps.Clear();
            var m1Maps = GameObject.FindGameObjectsWithTag("M1");
            foreach (var obj in m1Maps)
            {
                MapEntry entry = new MapEntry();
                entry.mapObject = obj;
                entry.overrideBGM = null;
                m1Group.maps.Add(entry);
            }
        }

        // M2 그룹 처리
        var m2Group = mapGroups.Find(g => g.groupType == MapGroupType.M2_Lake);
        if (m2Group != null)
        {
            m2Group.maps.Clear();
            var m2Maps = GameObject.FindGameObjectsWithTag("M2");
            foreach (var obj in m2Maps)
            {
                MapEntry entry = new MapEntry();
                entry.mapObject = obj;
                entry.overrideBGM = null;
                m2Group.maps.Add(entry);
            }
        }

    }


}
