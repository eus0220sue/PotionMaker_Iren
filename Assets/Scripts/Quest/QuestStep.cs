using UnityEngine;

[System.Serializable]
public class QuestStep
{
    public string m_description;
    public QuestStepType m_stepType;

    [Header("공통 필드")]
    public int m_requiredAmount;

    [Header("아이템 관련")]
    public ItemData m_targetItem;

    [Header("NPC 관련")]
    public string m_targetNpcId;

    [Header("맵 관련")]
    public string m_targetMapId;
}

public enum QuestStepType
{
    Visit,
    Gather,
    Craft,
    Deliver,
    Talk,
    End
}
