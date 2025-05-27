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
                    Debug.Log("QuestUIParent �ڵ� �Ҵ� �Ϸ�");
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
            // UI�� ������ ���� ����
            RefreshAllQuestUI();
            return;
        }
        UpdateQuestUI(questID, stepIndex);
    }

    public void RefreshAllQuestUI()
    {
        var currentQuests = m_questManager.GetAllStartedQuests();
        Debug.Log($"���� ���� ���� ����Ʈ ����: {currentQuests.Count}");

        foreach (var questID in currentQuests)
        {
            if (!m_questUIItems.ContainsKey(questID))
            {
                if (m_QuestUIPrefab == null)
                {
                    Debug.LogError("m_QuestUIPrefab�� null�Դϴ�!");
                    return;
                }
                if (m_QuestUIParent == null)
                {
                    Debug.LogError("m_QuestUIParent�� null�Դϴ�!");
                    return;
                }
                var go = Instantiate(m_QuestUIPrefab, m_QuestUIParent);
                Debug.Log($"Quest UI ������ ����: {go.name} under {m_QuestUIParent.name}");
                var uiItem = go.GetComponent<QuestUIEntity>();
                m_questUIItems.Add(questID, uiItem);
                uiItem.Initialize();
            }

            UpdateQuestUI(questID, m_questManager.GetCurrentStepIndex(questID));
        }
    }

    public void UpdateQuestUI(string questID, int stepIndex)
    {
        Debug.Log($"[QuestHUD] UpdateQuestUI ȣ�� - QuestID: {questID}, Step: {stepIndex}");

        if (!m_questUIItems.TryGetValue(questID, out var uiItem))
        {
            Debug.LogWarning($"[QuestHUD] UI �׸��� ã�� ����: {questID}");
            return;
        }

        var questData = m_questManager.GetQuestData(questID);
        var currentStep = m_questManager.GetCurrentStep(questID);

        if (questData == null || currentStep == null)
        {
            Debug.LogWarning($"[QuestHUD] ����Ʈ ������ �Ǵ� ���� �����Ͱ� ����: {questID}");
            return;
        }

        // ����Ʈ Ÿ��Ʋ�� ���� ��� �α� �߰�
        Debug.Log($"����Ʈ Ÿ��Ʋ: {questData.m_title}");
        Debug.Log($"����Ʈ ����: {currentStep.m_description}");

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
