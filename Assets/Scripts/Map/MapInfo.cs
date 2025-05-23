using UnityEngine;

public class MapInfo : MonoBehaviour
{
    [Header("이 맵이 속한 그룹")]
    public MapGroupType groupType;

    [Header("이 맵 전용 BGM (비워두면 그룹 BGM 사용)")]
    public AudioClip overrideBGM;

    [Header("이 맵의 내부 이름 (저장, 로딩, 퀘스트용 등)")]
    public string mapID;

    [Header("이 맵에서 표시할 이름 (UI용 등)")]
    public string displayName;

    // 향후 퀘스트/날씨/지역 이벤트에 쓰일 확장 필드도 여기에 추가 가능
}
