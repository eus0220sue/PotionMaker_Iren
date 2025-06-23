using UnityEngine;
using System.Collections.Generic;

public class MapBGMController : MonoBehaviour
{

    [Header("Title BGM")]
    public AudioClip titleBGM;

    [SerializeField] private AudioSource bgmSource;  // ���⿡ AudioSource �߰�

    [System.Serializable]
    public class MapEntry
    {
        public GameObject mapObject;
        public AudioClip overrideBGM; // null�̸� �׷� BGM ���
    }

    [System.Serializable]
    public class MapGroupEntry
    {
        public MapGroupType groupType;
        public AudioClip groupBGM;
        public List<MapEntry> maps; // �ش� �׷쿡 ���� �ʵ�
    }

    [Header("�׷캰 �� & BGM ����")]
    public List<MapGroupEntry> mapGroups;

    private AudioClip currentClip;

    void Start()
    {
        if (mapGroups != null && mapGroups.Count > 0)
        {
            var group = mapGroups[0];
            Debug.Log($"[Test] groupBGM before: {group.groupBGM}");

            if (group.groupBGM == null)
            {
                group.groupBGM = Resources.Load<AudioClip>("Sounds/Town_bgm"); // ��ο� �°� ����
                Debug.Log($"[Test] groupBGM loaded manually: {group.groupBGM}");
            }
        }
    }
    public void PlayTitleBGM()
    {
        if (titleBGM != null)
        {
            SoundManager.Instance.PlayBGM(titleBGM);
            Debug.Log("[MapBGMController] Ÿ��Ʋ BGM ��� ����");
        }
        else
        {
            Debug.LogWarning("[MapBGMController] Ÿ��Ʋ BGM�� �������� ����");
        }
    }
    public void PlayBGMForMap(GameObject mapObject)
    {
        if (mapObject == null)
        {
            Debug.LogWarning("[MapBGMController] mapObject is null. BGM ��� ����");
            return;
        }

        Debug.Log($"[MapBGMController] ���� - ���: {mapObject.name}");

        foreach (var group in mapGroups)
        {
            foreach (var entry in group.maps)
            {
                if (entry.mapObject == mapObject)
                {
                    Debug.Log($"[MapBGMController] ��Ī ����: {entry.mapObject.name}");

                    AudioClip clipToPlay = group.groupBGM;

                    if (clipToPlay == null || string.IsNullOrWhiteSpace(clipToPlay.name))
                    {
                        Debug.LogWarning($"[MapBGMController] {mapObject.name}�� �׷� BGM�� null�Դϴ�.");
                        return;
                    }
                    if (currentClip == clipToPlay && bgmSource.isPlaying)
                    {
                        Debug.Log($"[MapBGMController] ������ BGM '{clipToPlay.name}'�� �̹� ��� ���Դϴ�. ��� ����.");
                        return;
                    }

                    //  ���� BGM������ ���� ���¶�� �ٽ� Play
                    currentClip = clipToPlay;
                    SoundManager.Instance.PlayBGM(clipToPlay);

                    Debug.Log($"[MapBGMController] BGM ��� ����: {clipToPlay.name} (Group: {group.groupType})");
                    return;
                }
            }
        }

        Debug.LogWarning($"[MapBGMController] '{mapObject.name}'�� � �׷쿡�� ��ϵ��� �ʾҽ��ϴ�.");
    }
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            Debug.Log("[MapBGMController] BGM ����");
        }
    }
    public void SetupMapsAfterSceneLoad()
    {
        // M0 �׷� ó��
        var m0Group = mapGroups.Find(g => g.groupType == MapGroupType.M0_Town);
        if (m0Group != null)
        {
            m0Group.maps.Clear();
            var m0Maps = GameObject.FindGameObjectsWithTag("M0");
            foreach (var obj in m0Maps)
            {
                MapEntry entry = new MapEntry();
                entry.mapObject = obj;
                entry.overrideBGM = null; // �ʿ� �� ���� BGM ���� ����
                m0Group.maps.Add(entry);
            }
        }

        // M1 �׷� ó��
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

        // M2 �׷� ó��
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
