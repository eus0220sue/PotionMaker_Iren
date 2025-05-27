using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialData", menuName = "Data/MaterialData", order = 1)]

public class MaterialData : ScriptableObject
{
    public int gatherSoundIndex;

    public string m_MaterialName;
    /// <summary>
    /// 필드 오브젝트 스프라이트 인덱스
    /// 0: 채집 전/ 1: 채집 후
    /// </summary>
    public List<Sprite> stateSprites;
    /// <summary>
    /// 가이드 UI 이름
    /// </summary>
    public string m_objName = null;

    /// <summary>
    /// 아이템 데이터
    /// </summary>
    public ItemData m_itemData;

    /// <summary>
    /// 채집량
    /// </summary>
    public int amount = 1;
}
