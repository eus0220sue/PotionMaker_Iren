using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CurrentQuestInfo
{
    public QuestData Data;
    public int StepIndex;
}
[System.Serializable]
public class QuestStatus
{
    public string QuestID;
    public int StepIndex;
}

public class QuestManager : MonoBehaviour
{
    public event Action<string, int> OnQuestProgressChanged;


    [Header("퀘스트 데이터베이스")]
    [SerializeField] public QuestDB m_questDB;

    public Dictionary<string, string> m_questStates = new();
    public Dictionary<string, int> m_currentSteps = new();
    public Dictionary<string, bool> m_questFlags = new();
    [Header("현재 진행 중인 퀘스트 (읽기 전용)")]
    [SerializeField] private List<QuestStatus> m_debugQuestList = new();

    public void StartQuest(string questID)
    {
        if (!m_questStates.ContainsKey(questID))
        {
            m_questStates[questID] = "Started";
            m_currentSteps[questID] = 0;


            // 첫 스텝의 preDia 실행 (원본대로 UI + DialogueManager 둘 다 호출)
            var data = GetQuestData(questID);
            if (data != null && data.m_questSteps.Count > 0)
            {
                var firstStep = data.m_questSteps[0];
                if (firstStep != null && firstStep.m_preDia != null)
                {
                    var ui = GManager.Instance?.IsUIManager;
                    ui?.OpenDialogueUI(firstStep.m_preDia);
                    GManager.Instance?.IsDialogueManager?.StartDialogue(firstStep.m_preDia);

                }
            }

            SetQuestFlag($"{questID}_Step0_Start", true);

            // HUD/이벤트 (원본 유지)
            GManager.Instance?.IsHUDUI?.UpdateQuest(questID, 0);
            OnQuestProgressChanged?.Invoke(questID, 0);
            UpdateQuestInspectorList();

            // 시작 상태를 디스크(JSON)에 남기기(프레임 말 1회 저장)
            GManager.Instance?.SaveSoon();
        }
    }

    public void TryCompleteStep(string questID)
    {
        var step = GetCurrentStep(questID);
        if (step == null) return;

        switch (step.m_stepType)
        {
            case QuestStepType.Visit:
                string currentMap = GManager.Instance.currentMapGroup?.name;
                string targetMap = step.m_targetMapId;


                if (currentMap == targetMap)
                {
                    AdvanceStep(questID);
                }
                else
                {
                }
                break;

            case QuestStepType.Gather:
            case QuestStepType.Craft:
                if (GManager.Instance.IsInvenManager == null ||
                    GManager.Instance.IsInvenManager.IsInventoryData == null ||
                    step.m_targetItem == null)
                {
                    return;
                }

                bool hasItem = GManager.Instance.IsInvenManager.IsInventoryData.HasItem(step.m_targetItem, step.m_requiredAmount);
                if (hasItem)
                    AdvanceStep(questID);
                break;

            case QuestStepType.Talk:
            case QuestStepType.Deliver:
                // Deliver는 End 노드에서 따로 처리
                break;
        }
    }

    public void TryTalkToNPC(string npcID)
    {
        foreach (var kvp in m_questStates)
        {
            if (kvp.Value == "Started")
            {
                string questID = kvp.Key;
                var step = GetCurrentStep(questID);


                if (step != null && step.m_stepType == QuestStepType.Talk && step.m_targetNpcId == npcID)
                {
                    AdvanceStep(questID);
                    break;
                }
            }
        }
    }
    public void TryVisit()
    {
        //  Dictionary의 키 리스트를 미리 복사
        var questKeys = new List<string>(m_questStates.Keys);

        foreach (var questID in questKeys)
        {
            if (m_questStates[questID] == "Started")
            {
                var step = GetCurrentStep(questID);
                if (step != null && step.m_stepType == QuestStepType.Visit)
                {
                    TryCompleteStep(questID);
                }
            }
        }
    }
    public List<string> GetAllStartedQuests()
    {
        List<string> result = new();
        foreach (var kvp in m_questStates)
        {
            if (kvp.Value == "Started")
                result.Add(kvp.Key);
        }
        return result;
    }
    public bool CheckDeliverCondition(string questID)
    {
        var step = GetCurrentStep(questID);
        if (step == null || step.m_stepType != QuestStepType.Deliver)
            return false;

        return GManager.Instance.IsInvenManager.IsInventoryData.HasItem(step.m_targetItem, step.m_requiredAmount);
    }
    public void CompleteDeliverStep(string questID)
    {
        var step = GetCurrentStep(questID);
        if (step == null || step.m_stepType != QuestStepType.Deliver)
            return;

        if (GManager.Instance.IsInvenManager.IsInventoryData.HasItem(step.m_targetItem, step.m_requiredAmount))
        {
            GManager.Instance.IsInvenManager.RemoveItem(step.m_targetItem, step.m_requiredAmount);
            AdvanceStep(questID);
        }
    }


    private void AdvanceStep(string questID)
    {
        var data = GetQuestData(questID);
        if (data == null) return;

        int step = GetCurrentStepIndex(questID);
        QuestStep currentStep = (step >= 0 && step < data.m_questSteps.Count) ? data.m_questSteps[step] : null;

        // 현재 스텝 완료 후 대화
        if (currentStep != null && currentStep.m_afterDia != null)
        {
            var ui = GManager.Instance?.IsUIManager;
            ui?.OpenDialogueUI(currentStep.m_afterDia);
            GManager.Instance?.IsDialogueManager?.StartDialogue(currentStep.m_afterDia);

        }

        // ─────────────────────────────────────────────────────────
        // 아직 마지막 스텝이 아니면, 스텝만 advance
        // ─────────────────────────────────────────────────────────
        if (step + 1 < data.m_questSteps.Count)
        {
            m_currentSteps[questID] = step + 1;


            SetQuestFlag($"{questID}_Step{step}_Start", false);
            SetQuestFlag($"{questID}_Step{step}_Clear", true);
            SetQuestFlag($"{questID}_Step{step + 1}_Start", true);

            // HUD/이벤트 (원본 유지)
            GManager.Instance?.IsHUDUI?.UpdateQuest(questID, m_currentSteps[questID]);
            OnQuestProgressChanged?.Invoke(questID, m_currentSteps[questID]);

            // ★ 중간 진행도 JSON에 남기기(프레임 말 1회 저장)
            GManager.Instance?.SaveSoon();

            // 다음 스텝 preDia (원본처럼 UI + DialogueManager 둘 다 호출)
            var nextStep = data.m_questSteps[step + 1];
            if (nextStep != null && nextStep.m_preDia != null)
            {
                var ui = GManager.Instance?.IsUIManager;
                ui?.OpenDialogueUI(nextStep.m_preDia);
                GManager.Instance?.IsDialogueManager?.StartDialogue(nextStep.m_preDia);

            }
        }
        // ─────────────────────────────────────────────────────────
        // 마지막 스텝 완료 → 퀘스트 완료 + (있다면) 다음 퀘스트 즉시 시작
        // ─────────────────────────────────────────────────────────
        else
        {
            string nextId = currentStep?.m_nextQuestID;

            // 1) 현재 퀘스트 완료
            CompleteQuest(questID);
            GManager.Instance.IsHUDUI?.ClearQuestUI();

            // 2) 다음 퀘스트가 있으면 즉시 시작
            if (!string.IsNullOrEmpty(nextId))
            {
                StartQuest(nextId);

                // (필요 시 유지) 다음 퀘스트 첫 스텝 preDia & HUD 갱신
                var nextData = GetQuestData(nextId);
                QuestStep firstStep = (nextData != null && nextData.m_questSteps.Count > 0) ? nextData.m_questSteps[0] : null;
                if (firstStep?.m_preDia != null)
                {
                    var ui = GManager.Instance?.IsUIManager;
                    ui?.OpenDialogueUI(firstStep.m_preDia);
                    GManager.Instance?.IsDialogueManager?.StartDialogue(firstStep.m_preDia);
                }

                GManager.Instance?.IsHUDUI?.UpdateQuest(nextId, GetCurrentStepIndex(nextId));
            }
            else
            {
                // 이어갈 퀘스트가 없으면 HUD 비움
                GManager.Instance?.IsHUDUI?.ClearQuestUI();
            }

            // 3) 마지막에 저장 (프레임 말 1회로 합쳐짐)
            GManager.Instance?.SaveSoon();
        }

        UpdateQuestInspectorList();
    }

    public void CompleteQuest(string questID)
    {
        m_questStates[questID] = "Complete";


        var data = GetQuestData(questID);
        if (data != null)
        {
            foreach (var reward in data.m_rewardItems)
            {
                if (reward?.m_item != null && reward.m_amount > 0)
                {
                    InventoryManager.Instance.AddItem(reward.m_item, reward.m_amount);
                }
            }
        }

        SetQuestFlag($"{questID}_Complete", true);
        OnQuestProgressChanged?.Invoke(questID, GetCurrentStepIndex(questID));
        UpdateQuestInspectorList();
        GManager.Instance?.SaveSoon();

    }

    public QuestData GetQuestData(string questID)
    {
        return m_questDB.GetQuestById(questID);
    }

    public int GetCurrentStepIndex(string questID)
    {
        return m_currentSteps.TryGetValue(questID, out var step) ? step : 0;
    }

    public QuestStep GetCurrentStep(string questID)
    {
        var data = GetQuestData(questID);
        if (data == null) return null;

        int index = GetCurrentStepIndex(questID);
        if (index >= 0 && index < data.m_questSteps.Count)
            return data.m_questSteps[index];

        return null;
    }

    public void SetQuestFlag(string flagName, bool value)
    {
        m_questFlags[flagName] = value;
    }

    public bool GetQuestFlag(string flagName)
    {
        return m_questFlags.TryGetValue(flagName, out var result) && result;
    }

    public string GetQuestState(string questID)
    {
        return m_questStates.TryGetValue(questID, out var state) ? state : "NotStarted";
    }
    //현재 메인퀘스트 정보
    public CurrentQuestInfo GetCurrentMainQuestInfo()
    {
        foreach (var kvp in m_questStates)
        {
            if (kvp.Value == "Started")
            {
                var data = GetQuestData(kvp.Key);
                if (data != null && data.m_questType == QuestType.Main)
                {
                    return new CurrentQuestInfo
                    {
                        Data = data,
                        StepIndex = GetCurrentStepIndex(kvp.Key)
                    };
                }
            }
        }
        return null;
    }
    //현재 서브 퀘스트 정보
    public List<CurrentQuestInfo> GetCurrentSubQuestInfos()
    {
        var result = new List<CurrentQuestInfo>();
        foreach (var kvp in m_questStates)
        {
            if (kvp.Value == "Started")
            {
                var data = GetQuestData(kvp.Key);
                if (data != null && data.m_questType == QuestType.Sub)
                {
                    result.Add(new CurrentQuestInfo
                    {
                        Data = data,
                        StepIndex = GetCurrentStepIndex(kvp.Key)
                    });
                }
            }
        }
        return result;
    }
    private void UpdateQuestInspectorList()
    {
        m_debugQuestList.Clear();

        foreach (var kvp in m_questStates)
        {
            if (kvp.Value == "Started")
            {
                m_debugQuestList.Add(new QuestStatus
                {
                    QuestID = kvp.Key,
                    StepIndex = GetCurrentStepIndex(kvp.Key)
                });
            }
        }
    }
    public void TryCompleteStepAll()
    {
        foreach (var kvp in m_questStates)
        {
            if (kvp.Value == "Started")
            {
                TryCompleteStep(kvp.Key);
            }
        }
    }

    // QuestManager.cs 안에 추가 (대사/컷신 없이 상태만 세움)
    public void StartQuestSilently(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return;
        if (!m_currentSteps.ContainsKey(questID))
        {
            m_currentSteps[questID] = 0;
            SetQuestFlag($"{questID}_Step0_Start", true);
            OnQuestProgressChanged?.Invoke(questID, 0);
            GManager.Instance?.IsHUDUI?.UpdateQuest(questID, 0);
        }
    }

    public void SetCurrentStepSilently(string questID, int stepIndex)
    {
        if (string.IsNullOrEmpty(questID)) return;
        m_currentSteps[questID] = Mathf.Max(0, stepIndex);
        // 플래그 정합(간단 버전): 해당 stepIndex의 Start만 true로 보장
        for (int s = 0; s <= stepIndex; s++)
            SetQuestFlag($"{questID}_Step{s}_Start", s == stepIndex);

        OnQuestProgressChanged?.Invoke(questID, m_currentSteps[questID]);
        GManager.Instance?.IsHUDUI?.UpdateQuest(questID, m_currentSteps[questID]);
    }


}
