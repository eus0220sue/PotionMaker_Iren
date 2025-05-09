using UnityEngine;

public class InvenRefreshTester : MonoBehaviour
{
    [Tooltip("G 키를 누르면 인벤토리 UI가 강제로 갱신됩니다.")]
    public bool enableRefresh = true;

    private void Update()
    {
        if (!enableRefresh) return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (GManager.Instance != null && GManager.Instance.IsInventoryUI != null)
            {
                Debug.Log("[테스트] G 키 입력으로 인벤토리 UI 강제 갱신");
                GManager.Instance.IsInventoryUI.UpdateUI();
            }
            else
            {
                Debug.LogWarning("GManager 또는 InventoryUI가 아직 세팅되지 않았습니다.");
            }
        }
    }
}
