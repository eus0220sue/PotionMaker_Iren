using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class QuestHUD : MonoBehaviour
{
    [SerializeField] GameObject m_QuestUIPrefab;
    [SerializeField] Transform m_QuestUIParent;
    [SerializeField] GameObject m_QuestUIOpen;
    void Start()
    {
        m_QuestUIOpen.SetActive(false);

        if (m_QuestUIParent == null)
        {
            var questHUD = GameObject.Find("QuestHUD");
            if (questHUD != null)
            {
                var questUIOpen = questHUD.transform.Find("QuestUIOpen");
                if (questUIOpen != null)
                {
                    m_QuestUIParent = questUIOpen;
                }
            }
        }
    }

    private QuestManager m_questManager;
    private Dictionary<string, QuestUIEntity> m_questUIItems = new Dictionary<string, QuestUIEntity>();

    private void OnEnable()
    {
        m_questManager = GManager.Instance.IsQuestManager;
        if (m_questManager != null)
            m_questManager.OnQuestProgressChanged += OnQuestProgressChanged;

        RefreshAllQuestUI();
    }

    private void OnDisable()
    {
        if (m_questManager != null)
            m_questManager.OnQuestProgressChanged -= OnQuestProgressChanged;
    }

    private void OnQuestProgressChanged(string questID, int stepIndex)
    {
        if (!m_questUIItems.ContainsKey(questID))
        {
            // UI가 없으면 새로 생성
            RefreshAllQuestUI();
            return;
        }
        UpdateQuestUI(questID, stepIndex);
    }

    public void RefreshAllQuestUI()
    {
        var currentQuests = m_questManager.GetAllStartedQuests();   // 진행 중만

        // 1) 진행 중이 아닌(=완료된 등) UI는 먼저 제거
        var startedSet = new HashSet<string>(currentQuests);
        var staleKeys = new List<string>(m_questUIItems.Keys);
        foreach (var id in staleKeys)
        {
            if (!startedSet.Contains(id))
            {
                if (m_questUIItems[id] != null)
                    Destroy(m_questUIItems[id].gameObject);
                m_questUIItems.Remove(id);
            }
        }

        // 2) 진행 중인 것만 생성/갱신
        foreach (var questID in currentQuests)
        {
            if (!m_questUIItems.ContainsKey(questID))
            {
                if (m_QuestUIPrefab == null || m_QuestUIParent == null)
                {
                    return;
                }
                var go = Instantiate(m_QuestUIPrefab, m_QuestUIParent);
                var uiItem = go.GetComponent<QuestUIEntity>();
                m_questUIItems.Add(questID, uiItem);
                uiItem.Initialize();
            }

            UpdateQuestUI(questID, m_questManager.GetCurrentStepIndex(questID));
        }
    }
    public void UpdateQuestUI(string questID, int stepIndex)
    {

        if (!m_questUIItems.TryGetValue(questID, out var uiItem))
        {
            return;
        }

        var questData = m_questManager.GetQuestData(questID);
        var currentStep = m_questManager.GetCurrentStep(questID);

        if (questData == null || currentStep == null)
        {
            return;
        }


        int currentAmount = 0;

        if (currentStep.m_stepType == QuestStepType.Gather || currentStep.m_stepType == QuestStepType.Craft)
        {
            currentAmount = GManager.Instance.IsInvenManager.GetItemCount(currentStep.m_targetItem);
        }

        uiItem.SetQuestTitle(questData.m_title);
        uiItem.SetQuestDescription(currentStep.m_description, currentStep.m_stepType, currentAmount, currentStep.m_requiredAmount);

    }
    public void ClearAllQuests()
    {
        foreach (var uiItem in m_questUIItems.Values)
        {
            Destroy(uiItem.gameObject);
        }
        m_questUIItems.Clear();
    }
}
