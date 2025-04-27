using System.Collections.Generic;
using UnityEngine;

public class ParentSortingManager : MonoBehaviour
{
    public float detectionRadius = 5f; // 플레이어 감지 반경
    private Transform playerTransform;
    private SpriteRenderer playerRenderer;

    private HashSet<Transform> childTransforms = new HashSet<Transform>(); // 중복 방지
    private Dictionary<Transform, SpriteRenderer> childRenderers = new Dictionary<Transform, SpriteRenderer>();

    void Start()
    {
        GameObject player = GameObject.Find("Character");
        if (player == null)
        {
            Debug.LogError(" [ParentSortingManager] Player 오브젝트를 찾을 수 없습니다!");
            return;
        }

        playerTransform = player.transform;
        playerRenderer = player.GetComponent<SpriteRenderer>();

        if (playerRenderer == null)
        {
            Debug.LogError(" [ParentSortingManager] Player에 SpriteRenderer가 없습니다!");
            return;
        }

        // 최초 자식 오브젝트 초기화
        UpdateChildList();

        // 0.2초마다 정렬 업데이트 (성능 최적화)
        InvokeRepeating(nameof(UpdateSortingOrders), 0, 0.2f);
        InvokeRepeating(nameof(UpdateChildList), 0, 1f); // 1초마다 자식 리스트 갱신
    }

    private void UpdateChildList()
    {
        foreach (Transform child in transform)
        {
            if (!childTransforms.Contains(child)) // 중복 등록 방지
            {
                SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
                if (childRenderer != null)
                {
                    childTransforms.Add(child);
                    childRenderers[child] = childRenderer;
                }
            }
        }

        // 삭제된 오브젝트 정리
        childTransforms.RemoveWhere(child => child == null);
        List<Transform> toRemove = new List<Transform>();
        foreach (var pair in childRenderers)
        {
            if (pair.Key == null)
            {
                toRemove.Add(pair.Key);
            }
        }
        foreach (var key in toRemove)
        {
            childRenderers.Remove(key);
        }
    }

    private void UpdateSortingOrders()
    {
        if (playerTransform == null) return;

        foreach (var child in childTransforms)
        {
            if (child == null || !childRenderers.ContainsKey(child)) continue;

            SpriteRenderer childRenderer = childRenderers[child];
            float distance = Vector2.Distance(playerTransform.position, child.position);

            if (distance <= detectionRadius)
            {
                if (playerTransform.position.y > child.position.y)
                {
                    childRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
                }
                else
                {
                    childRenderer.sortingOrder = playerRenderer.sortingOrder - 1;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 모든 자식의 감지 반경 표시
        Gizmos.color = Color.green;
        foreach (Transform child in childTransforms)
        {
            if (child != null)
            {
                Gizmos.DrawWireSphere(child.position, detectionRadius);
            }
        }
    }
}
