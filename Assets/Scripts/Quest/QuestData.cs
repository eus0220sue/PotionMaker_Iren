using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    Main,
    Sub
}
[CreateAssetMenu(menuName = "Database/QuestData")]
public class QuestData : ScriptableObject
{
    public string m_questID;
    public QuestType m_questType;
    public string m_title;
    [TextArea] public string m_summary;
    public List<QuestStep> m_questSteps;
    public List<ItemReward> m_rewardItems;
    public int m_rewardGold;
}