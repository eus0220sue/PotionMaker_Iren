using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class QuestDialogueCondition
{
    public string m_questId;
    public string m_requiredState = "Started";
    public int m_requiredStepIndex = -1; // -1이면 무시
}
[Serializable]
public class FlagDialogueEntry
{
    public string flagName;           // MQ_0_Step3_Clear 등
    public DialogueNode dialogue;     // 해당 플래그가 true일 때 보여줄 대사
}


[Serializable]
public class QuestDialogueEntry
{
    [Header("퀘스트 조건")]
    public List<QuestDialogueCondition> questConditions = new();

    [Header("플래그 조건")]
    public List<string> requiredFlags = new(); // MQ_0_Step1_Clear 같은 커스텀 플래그 이름

    [Header("실행할 대사")]
    public DialogueNode startNode;
}

public class NPCInterAct : MonoBehaviour, IInteractableInterface
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightSprite;
    [SerializeField] private GameObject guideUI;
    [Header("NPC 고유 ID")]
    public string m_npcID;
    [Header("퀘스트 조건별 대사")]
    [SerializeField] private List<QuestDialogueEntry> m_questDialogues;

    [Header("플래그 조건별 대사")]
    [SerializeField] private List<FlagDialogueEntry> m_flagDialogues;

    [Header("기본 대사")]
    [SerializeField] private DialogueNode m_defaultNode;

    public void Interact()
    {
        Debug.Log($"[NPCInterAct] Interact called on NPC {m_npcID}");

        // 1. 퀘스트 조건 최우선
        foreach (var entry in m_questDialogues)
        {
            bool allQuestConditionsMet = entry.questConditions.All(IsQuestConditionMet);
            bool allFlagsMet = entry.requiredFlags.All(flag => GManager.Instance.IsQuestManager.GetQuestFlag(flag));


            if (allQuestConditionsMet && allFlagsMet)
            {
                GManager.Instance.IsUIManager.OpenDialogueUI(entry.startNode);
                return;
            }
        }

        // 2. 퀘스트 조건에 부합하는 것이 없을 때만 플래그 조건 확인
        foreach (var entry in m_flagDialogues)
        {
            bool flagSet = GManager.Instance.IsQuestManager.GetQuestFlag(entry.flagName);

            if (flagSet)
            {
                GManager.Instance.IsUIManager.OpenDialogueUI(entry.dialogue);
                return;
            }
        }

        // 3. 기본 대사
        if (m_defaultNode != null)
        {
            GManager.Instance.IsUIManager.OpenDialogueUI(m_defaultNode);
        }
        else
        {
        }
    }

    private bool IsQuestConditionMet(QuestDialogueCondition cond)
    {
        var qm = GManager.Instance.IsQuestManager;

        string state = qm.GetQuestState(cond.m_questId);
        if (!state.Equals(cond.m_requiredState, StringComparison.OrdinalIgnoreCase))
            return false;

        if (cond.m_requiredStepIndex >= 0)
        {
            int step = qm.GetCurrentStepIndex(cond.m_questId);
            if (step != cond.m_requiredStepIndex)
                return false;
        }

        return true;
    }

    public void OnFocusEnter()
    {
        SetHighlight(true);
    }

    public void OnFocusExit()
    {
        SetHighlight(false);
    }
    public void SetHighlight(bool active)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = active ? highlightSprite : normalSprite;

        if (guideUI != null)
            guideUI.SetActive(active);
    }
}

