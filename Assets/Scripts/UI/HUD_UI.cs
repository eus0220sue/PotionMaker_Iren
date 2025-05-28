using UnityEngine;
using TMPro;

public class HUD_UI : MonoBehaviour
{
    [Header("퀘스트 관련 UI")]
    [SerializeField] QuestHUD m_questHUD;

    [Header("골드 관련 UI")]
    [SerializeField] TMP_Text m_goldText;

    private int currentGold = 0;

    private void Start()
    {
        UpdateGoldUI();
    }

    public void UpdateGold(int newGold)
    {
        currentGold = newGold;
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        m_goldText.text = $"{currentGold}G";
    }

    public void UpdateQuest(string questID, int stepIndex)
    {
        m_questHUD?.UpdateQuestUI(questID, stepIndex);
    }
    public void ClearQuestUI()
    {
        // 퀘스트 UI 모두 지우기 (예: 동적 생성된 UI들 삭제, 텍스트 비우기 등)
        m_questHUD.ClearAllQuests();
    }
}
