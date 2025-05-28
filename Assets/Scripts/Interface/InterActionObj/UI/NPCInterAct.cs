using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class QuestDialogueCondition
{
    public string m_questId;
    public string m_requiredState = "Started";
    public int m_requiredStepIndex = -1; // -1�̸� ����
}
[Serializable]
public class FlagDialogueEntry
{
    public string flagName;           // MQ_0_Step3_Clear ��
    public DialogueNode dialogue;     // �ش� �÷��װ� true�� �� ������ ���
}


[Serializable]
public class QuestDialogueEntry
{
    [Header("����Ʈ ����")]
    public List<QuestDialogueCondition> questConditions = new();

    [Header("�÷��� ����")]
    public List<string> requiredFlags = new(); // MQ_0_Step1_Clear ���� Ŀ���� �÷��� �̸�

    [Header("������ ���")]
    public DialogueNode startNode;
}

public class NPCInterAct : MonoBehaviour, IInteractableInterface
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightSprite;
    [SerializeField] private GameObject guideUI;
    [Header("NPC ���� ID")]
    public string m_npcID;
    [Header("����Ʈ ���Ǻ� ���")]
    [SerializeField] private List<QuestDialogueEntry> m_questDialogues;

    [Header("�÷��� ���Ǻ� ���")]
    [SerializeField] private List<FlagDialogueEntry> m_flagDialogues;

    [Header("�⺻ ���")]
    [SerializeField] private DialogueNode m_defaultNode;

    public void Interact()
    {
        Debug.Log($"[NPCInterAct] Interact called on NPC {m_npcID}");

        // 1. ����Ʈ ���� �ֿ켱
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

        // 2. ����Ʈ ���ǿ� �����ϴ� ���� ���� ���� �÷��� ���� Ȯ��
        foreach (var entry in m_flagDialogues)
        {
            bool flagSet = GManager.Instance.IsQuestManager.GetQuestFlag(entry.flagName);

            if (flagSet)
            {
                GManager.Instance.IsUIManager.OpenDialogueUI(entry.dialogue);
                return;
            }
        }

        // 3. �⺻ ���
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

