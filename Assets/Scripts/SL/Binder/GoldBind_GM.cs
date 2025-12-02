using UnityEngine;

[DefaultExecutionOrder(-50)]
public class GoldBind_GM : MonoBehaviour
{
    public int CurrentGold => SaveLoad.GetInt(Keys.Gold, 0);

    private void OnEnable()
    {
        if (GManager.Instance) GManager.Instance.OnAfterLoad += OnAfterLoad;
    }
    private void OnDisable()
    {
        if (GManager.Instance) GManager.Instance.OnAfterLoad -= OnAfterLoad;
    }

    private void OnAfterLoad()
    {
        // HUD 갱신만 필요하다면 여기서
        GManager.Instance?.IsHUDUI?.UpdateGold(CurrentGold);
    }

    // 외부에서 돈 변경 시 호출
    public void AddGold(int delta)
    {
        int g = Mathf.Max(0, CurrentGold + delta);
        SaveLoad.SetInt(Keys.Gold, g);
        GManager.Instance?.IsHUDUI?.UpdateGold(g);
    }

    public void SetGold(int value)
    {
        int g = Mathf.Max(0, value);
        SaveLoad.SetInt(Keys.Gold, g);
        GManager.Instance?.IsHUDUI?.UpdateGold(g);
        GManager.Instance?.SaveNow();
    }
}
