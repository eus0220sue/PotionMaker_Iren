using System;
using System.Collections.Generic;
using UnityEngine;

public class ExchangeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public InventoryManager InvenManager;   // 인벤 매니저(필수)
    // [SerializeField] public UserData m_userData;          //  제거: SO 사용 안 함

    // ──────────────────────────────────────────────────────────────────────────
    // 통화(골드)
    // ──────────────────────────────────────────────────────────────────────────
    [Header("Currency")]
    [SerializeField] private int m_gold = 300;                  // 내부 골드 저장
    public event Action<int> OnGoldChanged;

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
        OnGoldChanged?.Invoke(m_gold);
    }

    private bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (m_gold < amount) return false;
        SetPlayerGold(m_gold - amount);
        return true;
    }

    private void AddGold(int amount)
    {
        if (amount <= 0) return;
        SetPlayerGold(m_gold + amount);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 유저 등급 (ScriptableObject 제거하고 여기서 직접 관리)
    // ──────────────────────────────────────────────────────────────────────────
    [Header("User Grade")]
    [SerializeField] private GradeType.Type m_userGrade = GradeType.Type.Novice;
    public event Action<GradeType.Type> OnGradeChanged;

    public GradeType.Type GetUserGrade() => m_userGrade;

    public void SetUserGrade(GradeType.Type grade)
    {
        if (m_userGrade == grade) return;
        m_userGrade = grade;
        OnGradeChanged?.Invoke(m_userGrade);
    }

    public bool HasRequiredGrade(GradeType.Type required) => m_userGrade >= required;

    // GManager 세이브/로드 파이프에 등급 연결
    private void OnEnable()
    {
        if (GManager.Instance != null)
        {
            GManager.Instance.OnAfterLoad += ApplyGoldAndGrade;
            GManager.Instance.OnBeforeSave += PersistGoldAndGrade;
        }
    }
    private void OnDisable()
    {
        if (GManager.Instance != null)
        {
            GManager.Instance.OnAfterLoad -= ApplyGoldAndGrade;
            GManager.Instance.OnBeforeSave -= PersistGoldAndGrade;
        }
    }
    private void ApplyGoldAndGrade()
    {
        // 파일 → 메모리
        SetPlayerGold(SaveLoad.GetInt(Keys.Gold, m_gold));
        SetUserGrade((GradeType.Type)SaveLoad.GetInt(Keys.Grade, (int)m_userGrade));
    }

    private void PersistGoldAndGrade()
    {
        // 메모리 → KeyDB
        SaveLoad.SetInt(Keys.Gold, GetPlayerGold());
        SaveLoad.SetInt(Keys.Grade, (int)GetUserGrade());
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 가격 정책
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
        public int SellPrice = -1; // -1이면 미설정( 구매가 * m_sellRate)
    }

    [Tooltip("개별 아이템에 대한 가격 오버라이드 테이블")]
    [SerializeField] private List<ItemPriceEntry> m_itemPrices = new();

    public int GetBuyPrice(ItemData item)
    {
        if (item == null) return m_defaultBuyPrice;
        if (item.m_buyPrice > 0) return item.m_buyPrice;

        var e = m_itemPrices.Find(x => x.Item == item);
        if (e != null && e.BuyPrice > 0) return e.BuyPrice;

        return m_defaultBuyPrice;
    }

    public int GetSellPrice(ItemData item)
    {
        if (item == null)
            return Mathf.Max(1, Mathf.FloorToInt(m_defaultBuyPrice * m_sellRate));

        if (item.m_sellPrice > 0) return item.m_sellPrice;

        var e = m_itemPrices.Find(x => x.Item == item);
        if (e != null && e.SellPrice > 0) return e.SellPrice;

        int buy = GetBuyPrice(item);
        return Mathf.Max(1, Mathf.FloorToInt(buy * m_sellRate));
    }

    public int GetAffordableMax(ItemData item)
    {
        int unit = GetBuyPrice(item);
        if (unit <= 0) return 0;
        int gold = GetPlayerGold();
        int affordable = gold / unit;
        return Mathf.Max(0, affordable);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 구매/판매
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

        if (!SpendGold(total))
        {
            Debug.Log($"[Shop] 골드 부족: 필요 {total}, 보유 {m_gold}");
            return false;
        }

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
        // 등급 체크: SO 대신 내부 등급 사용
        if (m_userGrade < data.IsGradeType)
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

    // 호환용 별칭 (기존 코드가 GetPlayerGrade/SetPlayerGrade를 호출해도 OK)
    public GradeType.Type GetPlayerGrade() => GetUserGrade();
    public void SetPlayerGrade(GradeType.Type grade) => SetUserGrade(grade);

    // int 버전이 필요한 코드가 있다면 함께 제공
    public int GetPlayerGradeInt() => (int)GetUserGrade();
    public void SetPlayerGradeInt(int gradeInt) => SetUserGrade((GradeType.Type)gradeInt);


#if UNITY_EDITOR
    private void OnValidate()
    {
        m_defaultBuyPrice = Mathf.Max(0, m_defaultBuyPrice);
        m_sellRate = Mathf.Clamp01(m_sellRate);
    }
#endif
}
