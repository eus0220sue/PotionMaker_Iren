using UnityEngine;
using System.Collections.Generic;

public class MapBGMController : MonoBehaviour
{
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
            Debug.Log($"[Test] groupBGM before: {group.groupBGM}");

            if (group.groupBGM == null)
            {
                group.groupBGM = Resources.Load<AudioClip>("Sounds/Town_bgm"); // 경로에 맞게 수정
                Debug.Log($"[Test] groupBGM loaded manually: {group.groupBGM}");
            }
        }
    }
    public void PlayBGMForMap(GameObject mapObject)
    {
        if (mapObject == null)
        {
            Debug.LogWarning("[MapBGMController] mapObject is null. BGM 재생 실패");
            return;
        }

        Debug.Log($"[MapBGMController] 시작 - 대상: {mapObject.name}");

        foreach (var group in mapGroups)
        {
            foreach (var entry in group.maps)
            {
                if (entry.mapObject == mapObject)
                {
                    Debug.Log($"[MapBGMController] 매칭 성공: {entry.mapObject.name}");

                    AudioClip clipToPlay = group.groupBGM;

                    if (clipToPlay == null || string.IsNullOrWhiteSpace(clipToPlay.name))
                    {
                        Debug.LogWarning($"[MapBGMController] {mapObject.name}의 그룹 BGM이 null입니다.");
                        return;
                    }

                    if (clipToPlay == currentClip)
                    {
                        Debug.Log($"[MapBGMController] 동일한 BGM '{clipToPlay.name}'이 이미 재생 중입니다.");
                        return;
                    }

                    currentClip = clipToPlay;
                    SoundManager.Instance.PlayBGM(clipToPlay);

                    Debug.Log($"[MapBGMController]  BGM 재생 성공: {clipToPlay.name} (Group: {group.groupType})");
                    return;
                }
            }
        }

        Debug.LogWarning($"[MapBGMController] '{mapObject.name}'은 어떤 그룹에도 등록되지 않았습니다.");
    }

}