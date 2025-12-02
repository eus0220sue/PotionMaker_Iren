using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraBoundsRegion : MonoBehaviour
{
    public string regionName = "Region";
    [Tooltip("바운드 판정용 BoxCollider2D (씬에서 크기/위치로 영역을 잡으세요)")]
    public BoxCollider2D area;

    private void Reset()
    {
        area = GetComponent<BoxCollider2D>();
        area.isTrigger = true; // 굳이 충돌은 안 써도 되지만 트리거 권장
    }

    public bool Contains(Vector3 worldPos)
    {
        if (!area) return false;
        return area.bounds.Contains(worldPos);
    }

    public (Vector2 min, Vector2 max) GetMinMax()
    {
        var b = area.bounds;
        return ((Vector2)b.min, (Vector2)b.max);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!area) area = GetComponent<BoxCollider2D>();
        if (!area) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
    }
#endif
}
