using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)] // 최대한 일찍
public class InventoryBind_GM : MonoBehaviour
{
    [Header("인벤 매니저(프로젝트 타입에 맞게 할당)")]
    [SerializeField] private InventoryManager m_inventory;

    [Header("아이템 DB(식별자 → ItemData 매핑)")]
    [SerializeField] private List<ItemData> m_itemDatabase = new();

    private Dictionary<string, ItemData> _id2Item;
    private bool _subscribed = false;

    [Serializable] private class SlotDTO { public string id; public int qty; public int index; }
    [Serializable] private class InvenDTO { public List<SlotDTO> slots = new(); public int capacity; }

    private void Awake()
    {
        AutoWireInventory();
        BuildLookup();
        Debug.Log($"[InventoryBind] Awake: m_inventory={(m_inventory ? m_inventory.name : "null")}, itemDB={m_itemDatabase.Count}");
    }

    private void OnEnable()
    {
        if (GManager.Instance != null)
        {
            GManager.Instance.OnBeforeSave -= SaveSnapshot;  // 중복 방지
            GManager.Instance.OnBeforeSave += SaveSnapshot;

            GManager.Instance.OnAfterLoad -= LoadAndApply;
            GManager.Instance.OnAfterLoad += LoadAndApply;
        }
    }

    private void OnDisable()
    {
        if (GManager.Instance != null)
        {
            GManager.Instance.OnBeforeSave -= SaveSnapshot;
            GManager.Instance.OnAfterLoad -= LoadAndApply;
        }
    }


    private void AutoWireInventory()
    {
        if (!m_inventory)
            m_inventory = GManager.Instance ? GManager.Instance.IsInvenManager : null;
        if (!m_inventory)
            m_inventory = FindObjectOfType<InventoryManager>(true);
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;

        if (GManager.Instance != null)
        {
            GManager.Instance.OnBeforeSave -= SaveSnapshot;
            GManager.Instance.OnBeforeSave += SaveSnapshot;

            GManager.Instance.OnAfterLoad -= LoadAndApply;
            GManager.Instance.OnAfterLoad += LoadAndApply;

            _subscribed = true;
            Debug.Log("[InventoryBind] Subscribed to GManager events immediately");
        }
        else
        {
            StartCoroutine(Co_Subscribe());
        }
    }

    private IEnumerator Co_Subscribe()
    {
        while (GManager.Instance == null) yield return null;

        TrySubscribe(); // 위 로직 재사용
    }

    private void BuildLookup()
    {
        _id2Item = new Dictionary<string, ItemData>(StringComparer.Ordinal);
        foreach (var it in m_itemDatabase)
        {
            if (!it) continue;
            var id = GetItemId(it);
            if (string.IsNullOrEmpty(id)) continue;
            if (!_id2Item.ContainsKey(id)) _id2Item.Add(id, it);
        }
    }

    private static string GetItemId(ItemData it)
    {
        var byID = GetFieldString(it, "m_itemID");
        var byId = GetFieldString(it, "m_itemId");
        if (!string.IsNullOrEmpty(byID)) return byID;
        if (!string.IsNullOrEmpty(byId)) return byId;
        return it.m_itemName; // 폴백
    }
    private static string GetFieldString(ItemData it, string field)
    {
        var f = it.GetType().GetField(field);
        if (f != null && f.FieldType == typeof(string)) return (string)f.GetValue(it);
        return null;
    }

    // ─────────────────────────────────────────────
    // 저장: 슬롯 인덱스까지 함께 기록
    // ─────────────────────────────────────────────
    public void SaveSnapshot()
    {
        AutoWireInventory(); // 혹시 씬 교체 후 끊겼을 수 있어 재확인

        if (!m_inventory)
        {
            Debug.LogWarning("[InventoryBind] SaveSnapshot: m_inventory가 null 입니다. (바인딩 확인 필요)");
            return;
        }
        var data = m_inventory.IsInventoryData;
        if (data?.slots == null)
        {
            Debug.LogWarning("[InventoryBind] SaveSnapshot: InventoryData/slots가 null 입니다.");
            return;
        }

        var dto = new InvenDTO { capacity = data.slots.Length };
        for (int i = 0; i < data.slots.Length; i++)
        {
            var s = data.slots[i];
            if (s?.itemData == null || s.quantity <= 0) continue;

            var id = GetItemId(s.itemData);
            dto.slots.Add(new SlotDTO { id = id, qty = s.quantity, index = i });
        }

        string json = JsonUtility.ToJson(dto);
        SaveLoad.SetString(Keys.InvenJson, json);
        Debug.Log($"[InventoryBind] SaveSnapshot: key='{Keys.InvenJson}', capacity={dto.capacity}, slotCount={dto.slots.Count}");
    }

    // ─────────────────────────────────────────────
    // 로드: 같은 슬롯 인덱스에 그대로 복원
    // ─────────────────────────────────────────────
    public void LoadAndApply()
    {
        AutoWireInventory();

        if (!m_inventory)
        {
            Debug.LogWarning("[InventoryBind] LoadAndApply: m_inventory가 null 입니다.");
            return;
        }
        var data = m_inventory.IsInventoryData;
        if (data == null || data.slots == null)
        {
            Debug.LogWarning("[InventoryBind] LoadAndApply: InventoryData/slots가 null 입니다.");
            return;
        }

        // 1) 초기화
        for (int i = 0; i < data.slots.Length; i++)
        {
            if (data.slots[i] == null) continue;
            data.slots[i].itemData = null;
            data.slots[i].quantity = 0;
        }

        // 2) JSON 읽기
        string json = SaveLoad.GetString(Keys.InvenJson, "");
        Debug.Log($"[InventoryBind] LoadAndApply: key='{Keys.InvenJson}', jsonLength={json?.Length ?? 0}");
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("[InventoryBind] 저장된 인벤 없음 → 빈 인벤으로 유지");
            StartCoroutine(Co_DeferredUIRefresh());
            return;
        }

        var dto = JsonUtility.FromJson<InvenDTO>(json);
        if (dto == null)
        {
            Debug.LogWarning("[InventoryBind] JSON 파싱 실패");
            StartCoroutine(Co_DeferredUIRefresh());
            return;
        }

        int FindFirstEmpty()
        {
            for (int i = 0; i < data.slots.Length; i++)
            {
                var s = data.slots[i];
                if (s == null) continue;
                if (s.itemData == null || s.quantity <= 0) return i;
            }
            return -1;
        }

        int applied = 0, repacked = 0, skipped = 0;
        foreach (var s in dto.slots)
        {
            if (s == null || string.IsNullOrEmpty(s.id) || s.qty <= 0) { skipped++; continue; }

            if (!_id2Item.TryGetValue(s.id, out var item))
            {
                Debug.LogWarning($"[InventoryBind] 아이템 ID 매핑 실패: {s.id}");
                skipped++;
                continue;
            }

            if (s.index < 0 || s.index >= data.slots.Length)
            {
                int free = FindFirstEmpty();
                if (free >= 0)
                {
                    data.slots[free].itemData = item;
                    data.slots[free].quantity = s.qty;
                    repacked++;
                }
                else skipped++;
                continue;
            }

            var dst = data.slots[s.index];
            if (dst.itemData == null || dst.quantity == 0)
            {
                dst.itemData = item;
                dst.quantity = s.qty;
                applied++;
            }
            else
            {
                int free = FindFirstEmpty();
                if (free >= 0)
                {
                    data.slots[free].itemData = item;
                    data.slots[free].quantity = s.qty;
                    repacked++;
                }
                else skipped++;
            }
        }

        Debug.Log($"[InventoryBind] 인벤 적용 완료: placed={applied}, repacked={repacked}, skipped={skipped} / capacity={data.slots.Length}");
        StartCoroutine(Co_DeferredUIRefresh());
    }

    private IEnumerator Co_DeferredUIRefresh()
    {
        // 슬롯 UI가 초기화될 시간을 한두 프레임 주기
        yield return null;
        yield return null;

        var shop = FindObjectOfType<ShopUI>(true);
        if (shop != null)
        {
            shop.UpdateSellUI();
            Debug.Log("[InventoryBind] 비주얼 갱신 완료: ShopUI.UpdateSellUI()");
        }
        else
        {
            Debug.LogWarning("[InventoryBind] ShopUI를 찾지 못해 비주얼 갱신 생략");
        }
    }

    // ─────────────────────────────────────────────
    // 호환용 별칭 (GManager가 Capture/Apply를 호출해도 동작)
    // ─────────────────────────────────────────────
    public void Capture() => SaveSnapshot();
    public void Apply() => LoadAndApply();
}
