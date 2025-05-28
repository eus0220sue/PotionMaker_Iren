using UnityEngine;
using TMPro;

public class HUD_UI : MonoBehaviour
{
    [Header("����Ʈ ���� UI")]
    [SerializeField] QuestHUD m_questHUD;

    [Header("��� ���� UI")]
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
        // ����Ʈ UI ��� ����� (��: ���� ������ UI�� ����, �ؽ�Ʈ ���� ��)
        m_questHUD.ClearAllQuests();
    }
}
