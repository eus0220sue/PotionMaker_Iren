using System.Collections.Generic;
using UnityEngine;
using static GManager;

public class ObjAction : MonoBehaviour
{
    [Header("세이브용 고유 ID (오브젝트 아이디 적어주세요)")]
    [SerializeField] private string m_id = "Obj_Unique_ID";

    [Header("발동 조건: 해당 퀘스트의 현재 스텝이 minStep 이상이면 교체 실행")]
    [SerializeField] private string m_questID = "Q_TM_0";
    [SerializeField] private int m_minStepToUnlock = 1;

    [Header("교체 대상 (index 0: 현재 활성, index 1: 변경 후 활성)")]
    [SerializeField] private GameObject[] m_pair = new GameObject[2];

    private bool m_unlockedFromSave = false;

    // ───────────────────────────────────────────────
    // 라이프사이클
    // ───────────────────────────────────────────────
    private void Awake()
    {
        // 세이브 상태 우선 적용(깜빡임 방지)
        LoadAndApplyImmediate();
    }

    private void OnEnable()
    {
        // 세이브 적용 이후 한 번 더 최종 확인
        if (GManager.Instance != null)
            GManager.Instance.OnAfterLoad += OnAfterLoad;

        // 퀘스트 진행 변화에 반응
        var qm = GManager.Instance ? GManager.Instance.IsQuestManager : null;
        if (qm != null)
            qm.OnQuestProgressChanged += OnQuestProgressChanged;
    }

    private void OnDisable()
    {
        if (GManager.Instance != null)
            GManager.Instance.OnAfterLoad -= OnAfterLoad;

        var qm = GManager.Instance ? GManager.Instance.IsQuestManager : null;
        if (qm != null)
            qm.OnQuestProgressChanged -= OnQuestProgressChanged;
    }

    // ───────────────────────────────────────────────
    // 이벤트/적용
    // ───────────────────────────────────────────────
    private void OnAfterLoad()
    {
        LoadAndApplyImmediate(); // 저장 우선
        if (!m_unlockedFromSave)
            TryUnlockByCondition(saveIfUnlocked: true);
    }

    private void OnQuestProgressChanged(string questID, int step)
    {
        if (string.IsNullOrEmpty(m_questID)) return;
        if (questID != m_questID) return;
        if (m_unlockedFromSave) return; // 이미 해제된 상태라면 무시

        if (step >= m_minStepToUnlock)
            Unlock(applyNow: true, save: true);
    }

    private void LoadAndApplyImmediate()
    {
        m_unlockedFromSave = IsIdInOpenedSet(m_id);
        if (m_unlockedFromSave) ApplyUnlockedVisual();
        else ApplyLockedVisual();
    }

    private void TryUnlockByCondition(bool saveIfUnlocked)
    {
        var qm = GManager.Instance ? GManager.Instance.IsQuestManager : null;
        if (qm == null) return;

        int cur = qm.GetCurrentStepIndex(m_questID); // 프로젝트에 존재하는 API 기준
        if (cur >= m_minStepToUnlock)
            Unlock(applyNow: true, save: saveIfUnlocked);
    }

    private void Unlock(bool applyNow, bool save)
    {
        if (applyNow) ApplyUnlockedVisual();
        if (save)
        {
            AddIdToOpenedSet(m_id);
            GManager.Instance?.SaveNow();
        }
    }

    // ───────────────────────────────────────────────
    // 시각/활성 상태 적용
    // ───────────────────────────────────────────────
    private void ApplyLockedVisual() => SetActivePair(0); // 현재(기본) 보이기
    private void ApplyUnlockedVisual() => SetActivePair(1); // 변경 후 보이기

    private void SetActivePair(int onIndex)
    {
        if (m_pair == null || m_pair.Length < 2) return;

        for (int i = 0; i < m_pair.Length; i++)
            if (m_pair[i]) m_pair[i].SetActive(i == onIndex);
    }

    // ───────────────────────────────────────────────
    // SaveLoad 헬퍼
    // ───────────────────────────────────────────────
    private static bool IsIdInOpenedSet(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        var list = SaveLoad.GetStringList(Keys.OpenedSet);
        return list != null && list.Contains(id);
    }

    private static void AddIdToOpenedSet(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        var list = SaveLoad.GetStringList(Keys.OpenedSet) ?? new List<string>();
        if (!list.Contains(id))
        {
            list.Add(id);
            SaveLoad.SetStringList(Keys.OpenedSet, list);
        }
    }
}
