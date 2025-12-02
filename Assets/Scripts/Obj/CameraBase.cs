using UnityEngine;

public class CameraBase : MonoBehaviour
{
    [Header("카메라 이동 제한 범위")]
    public Vector2 minPos;
    public Vector2 maxPos;

    // ▼ 디버그 ON/OFF 토글 (원하면 인스펙터에서 끌 수 있음)
    [Header("Debug")]
    [SerializeField] private bool m_debugLog = true;

    private Transform target;
    private Camera cam;

    private void Start()
    {
        target = GManager.Instance.IsUserTrans;
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null && GManager.Instance.IsUserTrans != null)
        {
            target = GManager.Instance.IsUserTrans;
        }

        if (!GManager.Instance.IsSettingFlag) return;
        if (target == null || cam == null) return;

        Vector3 targetPos = target.position;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float mapWidth = maxPos.x - minPos.x;
        float mapHeight = maxPos.y - minPos.y;

        float clampedX = (mapWidth > camWidth * 2f) ? Mathf.Clamp(targetPos.x, minPos.x + camWidth, maxPos.x - camWidth)
                                                      : (minPos.x + maxPos.x) * 0.5f;
        float clampedY = (mapHeight > camHeight * 2f) ? Mathf.Clamp(targetPos.y, minPos.y + camHeight, maxPos.y - camHeight)
                                                      : (minPos.y + maxPos.y) * 0.5f;

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    public void SetCameraBounds(Vector2 min, Vector2 max)
    {
        minPos = min;
        maxPos = max;
    }

    // ====== 추가: 이어하기용 유틸 ======

    public void SetTarget(Transform t)
    {
        target = t;
        if (cam == null) cam = Camera.main;
        
    }

    /// <summary>월드 좌표로 카메라 즉시 스냅(경계 클램프 포함)</summary>
    public void SnapToWorld(Vector3 worldPos)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        // ★ 바운드가 아직 유효하지 않으면 클램프 없이 즉시 스냅
        if (!HasValidBounds())
        {
            Vector3 pos = new Vector3(worldPos.x, worldPos.y, transform.position.z);
            transform.position = pos;
            return;
        }

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float mapWidth = maxPos.x - minPos.x;
        float mapHeight = maxPos.y - minPos.y;

        float clampedX = (mapWidth > camWidth * 2f) ? Mathf.Clamp(worldPos.x, minPos.x + camWidth, maxPos.x - camWidth)
                                                      : (minPos.x + maxPos.x) * 0.5f;
        float clampedY = (mapHeight > camHeight * 2f) ? Mathf.Clamp(worldPos.y, minPos.y + camHeight, maxPos.y - camHeight)
                                                      : (minPos.y + maxPos.y) * 0.5f;

        Vector3 finalPos = new Vector3(clampedX, clampedY, transform.position.z);

        if (m_debugLog)

        transform.position = finalPos;
    }

    /// <summary>세이브 파일에 저장된 플레이어 좌표로 즉시 스냅</summary>
    public void SnapToSavedPos()
    {
        Vector3 fallback = transform.position;
        Vector3 pos = SaveLoad.GetVector3(Keys.Pos, fallback);
        if (m_debugLog)
        SnapToWorld(pos);
    }

    /// <summary>현재 타겟 위치로 즉시 스냅(새로하기에도 안전)</summary>
    public void SnapToTargetNow()
    {
        if (target != null)
        {
          
            SnapToWorld(target.position);
        }
        else if (m_debugLog)
        {
        }
    }
    public bool HasValidBounds()
    {
        return (maxPos.x > minPos.x) && (maxPos.y > minPos.y);
    }
}
