using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 진행 상황을 SaveLoad(KeyDB) ←→ 월드(QuestManager) 사이에 동기화한다.
/// - 로드 직후(GManager.OnAfterLoad): JSON → QuestManager 복원 + HUD 갱신
/// - 저장 직전(GManager.OnBeforeSave): QuestManager → JSON 스냅샷 기록
/// - 새 게임(IsFirstPlay=true)이고 저장된 퀘스트가 없으면 m_initialQuestId를 자동 시작
/// </summary>
public class QuestBind_GM : MonoBehaviour
{
    [Header("Quest Manager")]
    [SerializeField] private QuestManager quest;   // 씬의 QuestManager 참조

    [Header("초기 퀘스트(새 게임일 때 자동 시작)")]
    [SerializeField] private string m_initialQuestId = "Q_TM_0";

    // ----- DTO -----
    [Serializable]
    private class QuestEntry
    {
        public string id;
        public string state; // "NotStarted" / "Started" / "Complete"
        public int step;     // 현재 스텝 인덱스 (0~)
    }

    [Serializable]
    private class QuestSnapshot
    {
        public List<QuestEntry> list = new();
    }

    private void Awake()
    {
        if (!quest) quest = FindObjectOfType<QuestManager>(true);
    }

    private void OnEnable()
    {
        StartCoroutine(Co_Subscribe());
    }

    private IEnumerator Co_Subscribe()
    {
        while (GManager.Instance == null) yield return null;

        // 중복 구독 방지 후 재구독
        GManager.Instance.OnAfterLoad -= Apply;
        GManager.Instance.OnBeforeSave -= Capture;

        GManager.Instance.OnAfterLoad += Apply;    // 로드 → 월드 복원
        GManager.Instance.OnBeforeSave += Capture;  // 저장 직전 → 스냅샷 기록

        // 씬에 뒤늦게 붙어도 즉시 1회 반영
        Apply();
    }

    private void OnDisable()
    {
        if (GManager.Instance == null) return;
        GManager.Instance.OnAfterLoad -= Apply;
        GManager.Instance.OnBeforeSave -= Capture;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // LOAD → 월드에 적용
    // ─────────────────────────────────────────────────────────────────────────────
    public void Apply()
    {
        if (!quest) return;

        // 저장 JSON 읽기 (이어하기용)
        string json = SaveLoad.GetString(Keys.QuestsJson, "");  // Keys.QuestsJson == "quest.all"

        // ★ 새로하기: 1회성 플래그가 켜져 있으면 저장 유무와 무관하게 처음 퀘스트로 강제 초기화
        if (GManager.Instance != null && GManager.Instance.ForceQuestResetOnce)
        {
            // 내부 상태 초기화
            quest.m_questStates?.Clear();
            quest.m_currentSteps?.Clear();

            // 시작 퀘스트 선택
            string startId = string.IsNullOrEmpty(m_initialQuestId) ? "Q_TM_0" : m_initialQuestId;

            // 강제 시작
            quest.StartQuest(startId);

            // 1회성 플래그 OFF (소비)
            GManager.Instance.ForceQuestResetOnce = false;

            TryRefreshHUD();
            return;
        }

        // ─────────────────────────────────────────────────────────────
        // 이어하기: 저장된 스냅샷을 그대로 복원
        // ─────────────────────────────────────────────────────────────
        QuestSnapshot snap = string.IsNullOrEmpty(json)
            ? new QuestSnapshot()
            : (JsonUtility.FromJson<QuestSnapshot>(json) ?? new QuestSnapshot());

        // 내부 상태 초기화
        quest.m_questStates?.Clear();
        quest.m_currentSteps?.Clear();

        int restored = 0;
        if (snap.list != null)
        {
            foreach (var e in snap.list)
            {
                if (e == null || string.IsNullOrEmpty(e.id)) continue;
                string state = string.IsNullOrEmpty(e.state) ? "NotStarted" : e.state;
                int step = Mathf.Max(0, e.step);

                quest.m_questStates[e.id] = state;
                quest.m_currentSteps[e.id] = step;
                restored++;
            }
        }

        TryRefreshHUD();
    }



    // ─────────────────────────────────────────────────────────────────────────────
    // SAVE ← 월드에서 수집
    // ─────────────────────────────────────────────────────────────────────────────
    public void Capture()
    {
        if (!quest) return;

        var snap = new QuestSnapshot();

        if (quest.m_questStates != null)
        {
            foreach (var kv in quest.m_questStates)
            {
                string id = kv.Key;
                string state = string.IsNullOrEmpty(kv.Value) ? "NotStarted" : kv.Value;

                int step = 0;
                try { step = quest.GetCurrentStepIndex(id); } catch { /* 무시 */ }

                snap.list.Add(new QuestEntry { id = id, state = state, step = Mathf.Max(0, step) });
            }
        }

        string outJson = JsonUtility.ToJson(snap);
        SaveLoad.SetString(Keys.QuestsJson, outJson);   // "quest.all"로 기록
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // HUD 보정
    // ─────────────────────────────────────────────────────────────────────────────
    private void TryRefreshHUD()
    {
        var hud = GManager.Instance?.IsHUDUI;
        if (hud == null || quest == null || quest.m_questStates == null) return;

        //  먼저 UI 전부 리빌드(Started/미완료 퀘 항목들을 프리팹으로 생성)
        hud.RefreshAllQuestUI();

        // 어떤 퀘를 최전면에 보여줄지 선택
        string chosenId = null;

        // 1) Started 우선
        foreach (var kv in quest.m_questStates)
            if (kv.Value == "Started") { chosenId = kv.Key; break; }

        // 2) 없으면 '미완료(any != Complete)' 중 하나
        if (chosenId == null)
            foreach (var kv in quest.m_questStates)
                if (kv.Value != "Complete") { chosenId = kv.Key; break; }

        // (선택) 3) 전부 Complete라면 그 중 하나라도 보여주고 싶으면 주석 해제
        // if (chosenId == null)
        //     foreach (var kv in quest.m_questStates)
        //         if (kv.Value == "Complete") { chosenId = kv.Key; break; }

        if (!string.IsNullOrEmpty(chosenId))
        {
            int step = 0;
            try { step = quest.GetCurrentStepIndex(chosenId); } catch { }
            hud.UpdateQuest(chosenId, step);  // 이제 UI 항목이 존재하므로 경고 없음
        }
        else
        {
            hud.ClearQuestUI();
        }
    }
}
