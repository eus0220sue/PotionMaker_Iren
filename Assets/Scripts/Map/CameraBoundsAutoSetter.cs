using System.Collections;
using UnityEngine;

public class CameraBoundsAutoSetter : MonoBehaviour
{
    [Header("옵션")]
    [Tooltip("구역 체크 주기(초)")]
    [SerializeField] private float m_checkInterval = 0.25f;

    [Tooltip("구역을 못 찾았을 때 플레이어 주변으로 임시 바운드를 줄 때 반경(절반크기)")]
    [SerializeField] private Vector2 m_fallbackHalfSize = new Vector2(50f, 30f);

    private CameraBase camBase;
    private Transform player;
    private CameraBoundsRegion currentRegion;

    private void OnEnable()
    {
        StartCoroutine(Co_InitThenRun());
    }

    private IEnumerator Co_InitThenRun()
    {
        // 카메라/플레이어 준비 대기
        float timeout = 5f;
        while (timeout > 0f)
        {
            camBase = GManager.Instance ? GManager.Instance.IsCameraBase : null;
            player = GManager.Instance ? GManager.Instance.IsUserTrans : null;

            if (camBase != null && player != null)
                break;

            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (camBase == null || player == null)
        {
            Debug.LogWarning("[AutoBounds] CameraBase 또는 Player를 찾지 못해 자동 지정 비활성화");
            yield break;
        }

        // 첫 적용은 즉시
        ApplyBoundsForPosition(player.position, logPrefix: "[AutoBounds/First]");

        // 이후 주기적으로 체크
        while (true)
        {
            if (player != null)
                ApplyBoundsForPosition(player.position, logPrefix: "[AutoBounds/Tick]");
            yield return new WaitForSecondsRealtime(m_checkInterval);
        }
    }

    /// <summary>
    /// 외부에서 즉시 강제 갱신하고 싶을 때 호출 (예: 이어하기 직후 저장좌표 기준으로 한 번)
    /// </summary>
    public void ForceUpdateNow(Vector3 worldPos)
    {
        if (!camBase)
            camBase = GManager.Instance ? GManager.Instance.IsCameraBase : null;

        ApplyBoundsForPosition(worldPos, logPrefix: "[AutoBounds/Force]");
    }

    private void ApplyBoundsForPosition(Vector3 worldPos, string logPrefix)
    {
        if (camBase == null)
            return;

        var regions = FindObjectsOfType<CameraBoundsRegion>(true);
        CameraBoundsRegion found = null;

        foreach (var r in regions)
        {
            if (r && r.Contains(worldPos))
            {
                found = r;
                break;
            }
        }

        if (found != null)
        {
            // 구역 변경 시에만 바운드 재설정
            if (currentRegion != found)
            {
                var (min, max) = found.GetMinMax();
                camBase.SetCameraBounds(min, max);
                currentRegion = found;

                Debug.Log($"{logPrefix} Bounds switched to '{found.regionName}' for pos={worldPos}, min={min}, max={max}");
            }
        }
        else
        {
            // 구역을 못 찾았으면 플레이어 주변에 임시 바운드 적용 (클램프 때문에 튀는 걸 방지)
            Vector2 min = (Vector2)worldPos - m_fallbackHalfSize;
            Vector2 max = (Vector2)worldPos + m_fallbackHalfSize;
            camBase.SetCameraBounds(min, max);

            // currentRegion은 null 유지 (다음에 구역 안으로 들어가면 자연스럽게 스위칭)
            Debug.LogWarning($"{logPrefix} No region contains pos={worldPos}. Fallback bounds applied around the pos (min={min}, max={max}).");
        }
    }
}
