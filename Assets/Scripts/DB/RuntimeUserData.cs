// RuntimeUserData.cs
using System;
using UnityEngine;

[Serializable]
public class RuntimeUserData
{
    public int gold;
    public GradeType.Type grade;

    // UserData SO(기본값)로부터 런타임 데이터 초기화
    public static RuntimeUserData FromDefaults(UserData defaultsSO)
    {
        return new RuntimeUserData
        {
            gold = defaultsSO != null ? defaultsSO.m_gold : 0,
            grade = defaultsSO != null ? defaultsSO.IsGrade : GradeType.Type.Novice
        };
    }
}
