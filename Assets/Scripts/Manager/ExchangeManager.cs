using System;                     // ★ 이벤트(Action<int>)용
using System.Collections.Generic;
using UnityEngine;

public class ExchangeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public InventoryManager InvenManager;   // 인벤 매니저(필수)
    [SerializeField] public UserData m_userData;             // 등급 연동 O /골드 연동X

    // ──────────────────────────────────────────────────────────────────────────
    // 통화(골드)
    // ──────────────────────────────────────────────────────────────────────────
    [Header("Currency")]
    [SerializeField] private int m_gold = 0;                  // 내부 골드 저장

    public event Action<int> OnGoldChanged;                   // ★ 골드 변경 이벤트

    private void Start()
    {
        // HUD가 처음 켜질 때 값 동기화를 위해 브로드캐스트 1회
        OnGoldChanged?.Invoke(GetPlayerGold());
    }

    public int GetPlayerGold() => Mathf.Max(0, m_gold);

    public void SetPlayerGold(int amount)
    {
        amount = Mathf.Max(0, amount);
        if (m_gold == amount) return;

        m_gold = amount;

        OnGoldChanged?.Invoke(m_gold); // ★ 변경 알림
    }

    private bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (m_gold < amount) return false;
        SetPlayerGold(m_gold - amount); // ★ 직접 대입 금지
        return true;
    }

    private void AddGold(int amount)
    {
        if (amount <= 0) return;
        SetPlayerGold(m_gold + amount); // ★ 직접 대입 금지
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 가격 정책
    // 우선순위:
    // 1) ItemData의 m_buyPrice / m_sellPrice
    // 2) 인스펙터 테이블(m_itemPrices) 오버라이드
    // 3) 기본값/판매율
    // ──────────────────────────────────────────────────────────────────────────
    [Header("Pricing")]
    [Tooltip("가격표에 없거나 ItemData에 가격이 없을 때 쓰는 기본 구매가")]
    [SerializeField] private int m_defaultBuyPrice = 10;

    [Tooltip("판매가 기본 비율(판매가 미설정시 구매가 * 이 비율)")]
    [Range(0f, 1f)]
    [SerializeField] private float m_sellRate = 0.5f;

    [Serializable]
    public class ItemPriceEntry
    {
        public ItemData Item;
        public int BuyPrice = 0;   // 0이면 미설정
        public int SellPrice = -1; // -1이면 미설정(→ 구매가 * m_sellRate)
    }

    [Tooltip("개별 아이템에 대한 가격 오버라이드 테이블")]
    [SerializeField] private List<ItemPriceEntry> m_itemPrices = new();

    public int GetBuyPrice(ItemData item)
    {
        if (item == null) return m_defaultBuyPrice;

        // 1) ItemData에 구매가가 명시되어 있으면 우선
        if (item.m_buyPrice > 0)
            return item.m_buyPrice;

        // 2) 테이블 오버라이드
        var e = m_itemPrices.Find(x => x.Item == item);
        if (e != null && e.BuyPrice > 0)
            return e.BuyPrice;

        // 3) 기본값
        return m_defaultBuyPrice;
    }

    public int GetSellPrice(ItemData item)
    {
        if (item == null)
            return Mathf.Max(1, Mathf.FloorToInt(m_defaultBuyPrice * m_sellRate));

        // 1) ItemData에 판매가가 명시되어 있으면 우선
        if (item.m_sellPrice > 0)
            return item.m_sellPrice;

        // 2) 테이블 오버라이드
        var e = m_itemPrices.Find(x => x.Item == item);
        if (e != null && e.SellPrice > 0)
            return e.SellPrice;

        // 3) 구매가 * 판매율
        int buy = GetBuyPrice(item);
        return Mathf.Max(1, Mathf.FloorToInt(buy * m_sellRate));
    }

    /// <summary>
    /// 보유 골드 기준 최대 구매 가능 수량(재고/인벤 제한 미포함).
    /// 재고/인벤 제한이 있다면 호출처에서 Min 처리해줘.
    /// </summary>
    public int GetAffordableMax(ItemData item)
    {
        int unit = GetBuyPrice(item);
        if (unit <= 0) return 0;
        int gold = GetPlayerGold();
        int affordable = gold / unit;
        return Mathf.Max(0, affordable);
    }


    // ──────────────────────────────────────────────────────────────────────────
    // 구매/판매 트랜잭션
    // ──────────────────────────────────────────────────────────────────────────
    public bool TryBuy(ItemData item, int qty)
    {
        if (item == null || qty <= 0) return false;

        int unit = GetBuyPrice(item);
        if (unit <= 0)
        {
            Debug.LogWarning($"[Shop] '{item.name}' 구매가가 0 이하입니다.");
            return false;
        }

        int total = unit * qty;

        // 재화 차감
        if (!SpendGold(total))
        {
            Debug.Log($"[Shop] 골드 부족: 필요 {total}, 보유 {m_gold}");
            return false;
        }

        // 인벤 체크
        if (InvenManager == null || InvenManager.IsInventoryData == null)
        {
            Debug.LogWarning("[Shop] InvenManager/InventoryData가 연결되어 있지 않습니다.");
            AddGold(total); // 롤백
            return false;
        }
        if (!InvenManager.IsInventoryData.HasSpaceForItem(item, qty))
        {
            Debug.Log("[Shop] 인벤토리 공간 부족으로 구매 실패");
            AddGold(total); // 롤백
            return false;
        }

        InvenManager.AddItem(item, qty);
        return true;
    }

    public bool TrySell(ItemData item, int qty)
    {
        if (item == null || qty <= 0) return false;

        if (InvenManager == null || InvenManager.IsInventoryData == null)
        {
            Debug.LogWarning("[Shop] InvenManager/InventoryData가 연결되어 있지 않습니다.");
            return false;
        }

        if (!InvenManager.IsInventoryData.HasItem(item, qty))
        {
            Debug.Log("[Shop] 보유 수량이 부족합니다.");
            return false;
        }

        InvenManager.ConsumeItem(item, qty);
        int income = GetSellPrice(item) * qty;
        AddGold(income);
        return true;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 제작(기존 유지)
    // ──────────────────────────────────────────────────────────────────────────
    [Header("Craft / Grade")]
    public GradeType m_gradeType; // 등급 사용처가 있으면 유지

    public void Craft(CraftData data)
    {
        if (InvenManager == null || InvenManager.IsInventoryData == null) { Debug.LogWarning("[Craft] 인벤 미연결"); return; }

        if (!InvenManager.IsInventoryData.HasItem(data.IsInputItemData, data.IsIAmount)) return;
        if (!InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItemData, data.IsOAmount)) return;

        InvenManager.ConsumeItem(data.IsInputItemData, data.IsIAmount);
        InvenManager.AddItem(data.IsOutputItemData, data.IsOAmount);
    }

    public void OilCraft(OilCraftData data)
    {
        if (InvenManager == null || InvenManager.IsInventoryData == null) { Debug.LogWarning("[OilCraft] 인벤 미연결"); return; }

        if (!InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1)) return;
        if (!InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItem, data.IsOAmount)) return;

        InvenManager.ConsumeItem(data.IsInputI1, data.IsIAmount1);
        InvenManager.ConsumeItem(data.IsInputI2, data.IsIAmount2);
        InvenManager.AddItem(data.IsOutputItem, data.IsOAmount);
    }

    public void PotionCraft(PotionCraftData data)
    {
        if (m_userData != null && m_userData.IsGrade < data.IsGradeType)
        {
            Debug.Log("[PotionCraft] 등급이 부족합니다.");
            return;
        }
        if (InvenManager == null || InvenManager.IsInventoryData == null) { Debug.LogWarning("[PotionCraft] 인벤 미연결"); return; }

        if (!InvenManager.IsInventoryData.HasItem(data.IsInputI1, data.IsIAmount1)) return;
        if (!InvenManager.IsInventoryData.HasSpaceForItem(data.IsOutputItem, data.IsOAmount)) return;

        InvenManager.ConsumeItem(data.IsInputI1, data.IsIAmount1);
        InvenManager.ConsumeItem(data.IsInputI2, data.IsIAmount2);
        InvenManager.AddItem(data.IsOutputItem, data.IsOAmount);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        m_defaultBuyPrice = Mathf.Max(0, m_defaultBuyPrice);
        m_sellRate = Mathf.Clamp01(m_sellRate);
    }
#endif
}
