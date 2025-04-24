using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "UserData", menuName = "Data/UserData", order = 1)]

public class UserData : ScriptableObject
{
    /// <summary>
    /// 소지 골드
    /// </summary>
    public int m_gold;

    /// <summary>
    /// 연금술사 등급
    /// </summary>
    [SerializeField] GradeType.Type m_grade = GradeType.Type.Novice;

    public GradeType.Type IsGrade { get { return m_grade; } }


}
