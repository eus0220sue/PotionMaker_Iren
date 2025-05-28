using TMPro;
using UnityEngine;

public class QuestUIEntity : MonoBehaviour
{
    [SerializeField] TMP_Text m_QTitle;
    [SerializeField] TMP_Text m_QDescAndProgress;


    public void Initialize()
    {
        // 텍스트 초기화 등 UI 상태 리셋
        m_QTitle.text = "";
        m_QDescAndProgress.text = "";
        // 기타 초기화 작업 가능
    }

    // 예: 타입별 진행 문구
    private string GetProgressSuffix(QuestStepType stepType, int currentAmount, int requiredAmount)
    {
        switch (stepType)
        {
            case QuestStepType.Gather:
            case QuestStepType.Craft:
                return $" ({currentAmount} / {requiredAmount})";
            default:
                return "";
        }
    }


    public void SetQuestTitle(string title)
    {
        m_QTitle.text = title;
    }

    public void SetQuestDescription(string description, QuestStepType stepType, int currentAmount, int requiredAmount)
    {
        string suffix = GetProgressSuffix(stepType, currentAmount, requiredAmount);
        m_QDescAndProgress.text = description + suffix;
    }
}