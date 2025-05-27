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

            Debug.Log($"[퀘스트 시작] {questID}");

            // 첫 스텝의 preDia 실행
            var data = GetQuestData(questID);
            if (data != null && data.m_questSteps.Count > 0)
            {
                var firstStep = data.m_questSteps[0];
                if (firstStep.m_preDia != null)
                {
                    GManager.Instance.IsUIManager.OpenDialogueUI(firstStep.m_preDia);
                    GManager.Instance.IsDialogueManager.StartDialogue(firstStep.m_preDia);
                    Debug.Log($"[퀘스트 대화] {questID} 첫 스텝 시작 전 대화 실행");
                }
            }

            SetQuestFlag($"{questID}_Step0_Start", true);

            Debug.Log($"[QuestManager] 이벤트 호출 직전 - QuestID: '{questID}', StepIndex: 0");
            OnQuestProgressChanged?.Invoke(questID, 0);
            UpdateQuestInspectorList();
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

                Debug.Log($"[Quest Visit] 현재 맵: {currentMap}, 타겟 맵: {targetMap}");

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

                Debug.Log($"[TryTalkToNPC] 퀘스트 {questID} 현재 Step: {step?.m_stepType}, 타겟: {step?.m_targetNpcId}");

                if (step != null && step.m_stepType == QuestStepType.Talk && step.m_targetNpcId == npcID)
                {
                    AdvanceStep(questID);
                    Debug.Log($"[퀘스트 진행] {questID} - {npcID}와 대화하여 스텝 완료");
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
            Debug.Log($"[QuestManager] Deliver Step 완료 처리: {questID}");
        }
    }


    private void AdvanceStep(string questID)
    {
        var data = GetQuestData(questID);
        if (data == null) return;

        int step = GetCurrentStepIndex(questID);
        QuestStep currentStep = null;
        if (step >= 0 && step < data.m_questSteps.Count)
            currentStep = data.m_questSteps[step];

        // 현재 스텝 완료 시 afterDia 실행
        if (currentStep != null && currentStep.m_afterDia != null)
        {
            GManager.Instance.IsUIManager.OpenDialogueUI(currentStep.m_afterDia);
            Debug.Log($"[퀘스트 대화] {questID} Step {step} 완료 후 대화 실행");
        }

        if (step + 1 < data.m_questSteps.Count)
        {
            m_currentSteps[questID] = step + 1;

            Debug.Log($"[퀘스트 진행] {questID} - Step {step} 완료 → Step {step + 1} 시작");

            SetQuestFlag($"{questID}_Step{step}_Start", false);
            SetQuestFlag($"{questID}_Step{step}_Clear", true);
            SetQuestFlag($"{questID}_Step{step + 1}_Start", true);

            if (GManager.Instance.IsHUDUI != null)
            {
                GManager.Instance.IsHUDUI.UpdateQuest(questID, m_currentSteps[questID]);
                Debug.Log($"[AdvanceStep] 이벤트 호출: OnQuestProgressChanged({questID}, {m_currentSteps[questID]})");
            }

            Debug.Log($"[QuestManager] 이벤트 호출 직전 - QuestID: '{questID}', StepIndex: {m_currentSteps[questID]}");
            OnQuestProgressChanged?.Invoke(questID, m_currentSteps[questID]);
            Debug.Log("[AdvanceStep] 이벤트 호출 완료");

            var nextStep = data.m_questSteps[step + 1];
            if (nextStep.m_preDia != null)
            {
                GManager.Instance.IsUIManager.OpenDialogueUI(nextStep.m_preDia);
                Debug.Log($"[퀘스트 대화] {questID} Step {step + 1} 시작 전 대화 실행");
            }
        }
        else
        {
            Debug.Log($"[퀘스트 진행] {questID} - 마지막 Step {step} 완료 → 퀘스트 완료 예정");

            if (GManager.Instance.IsHUDUI != null)
            {
                GManager.Instance.IsHUDUI.ClearQuestUI();
            }

            if (!string.IsNullOrEmpty(currentStep?.m_nextQuestID))
            {
                Debug.Log($"[퀘스트 진행] {questID} - 다음 퀘스트 자동 시작 대기 중: {currentStep.m_nextQuestID}");
                StartCoroutine(StartNextQuestWithDelay(currentStep.m_nextQuestID, 0.1f));
            }

            CompleteQuest(questID);
        }

        UpdateQuestInspectorList();
    }

    private IEnumerator StartNextQuestWithDelay(string nextQuestID, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        StartQuest(nextQuestID);
    }

    public void CompleteQuest(string questID)
    {
        m_questStates[questID] = "Complete";

        Debug.Log($"[퀘스트 완료] {questID}");

        var data = GetQuestData(questID);
        if (data != null)
        {
            foreach (var reward in data.m_rewardItems)
            {
                InventoryManager.Instance.AddItem(reward.m_item, reward.m_amount);
            }
        }

        SetQuestFlag($"{questID}_Complete", true);
        OnQuestProgressChanged?.Invoke(questID, m_currentSteps[questID]);
        UpdateQuestInspectorList();
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
        Debug.Log($"[퀘스트 플래그] {flagName} = {value}");
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


}
