// Assets/Scripts/SL/Binder/QuestBind_GM.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestEntry { public string id; public string state; public int step; }

[System.Serializable]
public class QuestSnapshot { public List<QuestEntry> list = new(); }

public class QuestBind_GM : MonoBehaviour
{
    [SerializeField] private QuestManager quest;

    void OnEnable()
    {
        if (!GManager.Instance) return;
        GManager.Instance.OnAfterLoad += Apply;   // 저장 불러온 뒤 → 월드에 적용
        GManager.Instance.OnBeforeSave += Capture; // 저장 직전   → 월드에서 수집

        Apply(); // 씬에 나중에 붙어도 즉시 반영 + HUD 갱신
    }

    void OnDisable()
    {
        if (!GManager.Instance) return;
        GManager.Instance.OnAfterLoad -= Apply;
        GManager.Instance.OnBeforeSave -= Capture;
    }

    // ─────────────────────────────────────────────────────────
    // 저장 → 월드 적용 (HUD 즉시 갱신 포함)
    // ─────────────────────────────────────────────────────────
    void Apply()
    {
        if (!quest) return;

        string json = SaveLoad.GetString(GManager.Keys.QuestsJson, "");
        var snap = string.IsNullOrEmpty(json) ? new QuestSnapshot()
                                              : JsonUtility.FromJson<QuestSnapshot>(json) ?? new QuestSnapshot();

        // 현재 상태 초기화 후, 스냅샷 반영
        quest.m_questStates.Clear();
        quest.m_currentSteps.Clear();

        if (snap.list != null)
        {
            foreach (var e in snap.list)
            {
                if (string.IsNullOrEmpty(e.id)) continue;

                string state = string.IsNullOrEmpty(e.state) ? "NotStarted" : e.state;
                int step = Mathf.Max(0, e.step);

                quest.m_questStates[e.id] = state;
                quest.m_currentSteps[e.id] = step;
            }
        }

        // ★ HUD 즉시 갱신: 진행 중(Started)인 퀘스트 하나를 골라 표시
        if (GManager.Instance?.IsHUDUI != null)
        {
            string chosenId = null;

            // (1) Started 상태인 퀘스트 중 하나 선택
            foreach (var kv in quest.m_questStates)
            {
                if (kv.Value == "Started") { chosenId = kv.Key; break; }
            }

            // (2) 없으면 Complete가 아닌 어떤 퀘스트라도 선택(선택사항)
            if (chosenId == null)
            {
                foreach (var kv in quest.m_questStates)
                {
                    if (kv.Value != "Complete") { chosenId = kv.Key; break; }
                }
            }

            // (3) 최종 선택된 퀘스트로 HUD 업데이트
            if (chosenId != null)
            {
                int stepIndex = quest.GetCurrentStepIndex(chosenId);
                GManager.Instance.IsHUDUI.UpdateQuest(chosenId, stepIndex);
            }
            // 필요하면, 진행 중인 퀘스트가 전혀 없을 때 HUD를 비우는 메서드를 호출하도록 확장 가능:
            // else { GManager.Instance.IsHUDUI.ClearQuest(); }
        }
    }

    // ─────────────────────────────────────────────────────────
    // 월드 → 저장 수집
    // ─────────────────────────────────────────────────────────
    void Capture()
    {
        if (!quest) return;

        var snap = new QuestSnapshot();

        // QuestManager의 공개 딕셔너리에서 그대로 스냅샷 만들기
        foreach (var kv in quest.m_questStates)
        {
            string id = kv.Key;
            string state = kv.Value;
            int step = quest.GetCurrentStepIndex(id);

            snap.list.Add(new QuestEntry { id = id, state = state, step = step });
        }

        SaveLoad.SetString(GManager.Keys.QuestsJson, JsonUtility.ToJson(snap));
    }
}
