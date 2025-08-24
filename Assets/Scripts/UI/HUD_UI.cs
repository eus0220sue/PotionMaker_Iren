using UnityEngine;
using TMPro;

public class HUD_UI : MonoBehaviour
{
    [Header("퀘스트 관련 UI")]
    [SerializeField] private QuestHUD m_questHUD;

    [Header("골드 관련 UI")]
    [SerializeField] private TMP_Text m_goldText;

    [Header("Managers")]
    private ExchangeManager m_exchangeManager; // ★ 추가

    private int currentGold = 0;

    private void OnEnable()
    {
        // 매니저 자동 주입(인스펙터가 비어있으면 GManager 통해 찾기)
        if (m_exchangeManager == null)
            m_exchangeManager = GManager.Instance?.IsExchangeManager;

        // 이벤트 구독 + 현재 값으로 즉시 동기화
        if (m_exchangeManager != null)
        {
            m_exchangeManager.OnGoldChanged -= HandleGoldChanged; // 중복구독 방지
            m_exchangeManager.OnGoldChanged += HandleGoldChanged;
            UpdateGold(m_exchangeManager.GetPlayerGold()); // ★ 초기 동기화
        }
        else
        {
            // 매니저 못찾았으면 0G 표기(필요 시 지연 등록 로직 추가 가능)
            UpdateGold(0);
        }
    }

    private void OnDisable()
    {
        if (m_exchangeManager != null)
            m_exchangeManager.OnGoldChanged -= HandleGoldChanged;
    }

    // 이벤트 콜백
    private void HandleGoldChanged(int newGold)
    {
        UpdateGold(newGold);
    }

    // 외부에서 직접 세팅할 때도 사용 가능
    public void UpdateGold(int newGold)
    {
        currentGold = Mathf.Max(0, newGold);
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (m_goldText != null)
            m_goldText.text = $"{currentGold}G"; // 필요하면 $"{currentGold:n0} G"
    }

    public void UpdateQuest(string questID, int stepIndex)
    {
        m_questHUD?.UpdateQuestUI(questID, stepIndex);
    }

    public void ClearQuestUI()
    {
        m_questHUD?.ClearAllQuests();
    }
}
