using System.Collections.Generic;
using UnityEngine;

public enum SpeakerPosition
{
    Left,
    Right
}

public enum NodeType
{
    Dialogue,
    Condition,
    CutScene,
    End
}

[CreateAssetMenu(menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [Header("노드 이름")]
    public string m_nodeName;
    public string m_nodeID;
    [Header("노드 타입")]
    public NodeType m_nodeType;
    public Vector2 m_position;
    public SpeakerPosition m_speakerPosition;

    [Header("대화")]
    public string m_speakerName;
    [TextArea] public string m_dialogueText;
    public Sprite m_characterPortrait;

    [Header("다음 노드")]
    public DialogueNode m_nextNode;

    [Header("대화 선택지")]
    public List<DialogueChoice> choices;

    [Header("조건 분기용 플래그")]
    public string conditionFlag;
    public DialogueNode ifTrueNode;
    public DialogueNode ifFalseNode;

    [Header("컷신")]
    public Sprite m_cutSceneSprite;

    [Header("퀘스트 연동")]
    public string m_talkNpcID; // Talk Step 자동완성용

    public bool CheckQuestCondition(QuestDialogueCondition cond)
    {
        var qm = GManager.Instance.IsQuestManager;
        string state = qm.GetQuestState(cond.m_questId);
        if (!state.Equals(cond.m_requiredState, System.StringComparison.OrdinalIgnoreCase))
            return false;

        if (cond.m_requiredStepIndex >= 0)
        {
            int step = qm.GetCurrentStepIndex(cond.m_questId);
            if (step != cond.m_requiredStepIndex)
                return false;
        }

        return true;
    }

    public void Execute(DialogueManager manager)
    {
        switch (m_nodeType)
        {
            case NodeType.Dialogue:
                if (choices != null && choices.Count > 0)
                {
                    manager.ShowDialogueWithChoices(m_speakerName, m_dialogueText, m_characterPortrait, m_speakerPosition, choices);
                }
                else
                {
                    manager.ShowDialogue(m_speakerName, m_dialogueText, m_characterPortrait, m_speakerPosition, m_nextNode);
                }
                break;

            case NodeType.Condition:
                {
                    bool result = manager.GetFlag(conditionFlag);
                    manager.ContinueDialogue(result ? ifTrueNode : ifFalseNode);
                    break;
                }

            case NodeType.CutScene:
                manager.ShowCutScene(m_cutSceneSprite, m_nextNode);
                break;

            case NodeType.End:
                if (!string.IsNullOrEmpty(m_talkNpcID))
                {
                    var qm = GManager.Instance.IsQuestManager;

                    // Talk 처리
                    qm.TryTalkToNPC(m_talkNpcID);

                    // Deliver 처리
                    foreach (var questID in qm.GetAllStartedQuests())
                    {
                        var step = qm.GetCurrentStep(questID);
                        if (step != null &&
                            step.m_stepType == QuestStepType.Deliver &&
                            step.m_targetNpcId == m_talkNpcID)
                        {
                            if (qm.CheckDeliverCondition(questID))
                            {
                                qm.CompleteDeliverStep(questID);
                            }
                        }
                    }
                }
                manager.EndDialogue();
                break;
        }
    }
}
