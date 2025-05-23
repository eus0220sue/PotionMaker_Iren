using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/QuestDB")]
public class QuestDB : ScriptableObject
{
    [System.Serializable]
    public class QuestEntry
    {
        public string questId;
        public QuestData questData;
    }

    [System.Serializable]
    public class QuestGroup
    {
        public string groupName; // 예: "Village", "Forest"
        public List<QuestEntry> quests;
    }

    [Header("그룹별 퀘스트")]
    public List<QuestGroup> questGroups;

    /// 퀘스트 ID로 전체 그룹에서 탐색
    public QuestData GetQuestById(string questId)
    {
        foreach (var group in questGroups)
        {
            foreach (var entry in group.quests)
            {
                if (entry.questId == questId)
                    return entry.questData;
            }
        }
        return null;
    }

    /// 특정 그룹의 모든 퀘스트 반환
    public List<QuestData> GetQuestsByGroup(string groupName)
    {
        return questGroups
            .FirstOrDefault(g => g.groupName == groupName)?.quests
            .Where(e => e.questData != null)
            .Select(e => e.questData)
            .ToList() ?? new List<QuestData>();
    }
}
