using System.Collections.Generic;
using UnityEngine;

public class SortingManager : MonoBehaviour
{
    public float detectionRadius = 4.0f;
    private Transform playerTransform;
    private SpriteRenderer playerRenderer;

    private HashSet<Transform> childTransforms = new HashSet<Transform>();
    private Dictionary<Transform, SpriteRenderer> childRenderers = new Dictionary<Transform, SpriteRenderer>();

    void Start()
    {
        GameObject player = GameObject.Find("Character");
        if (player == null)
        {
            return;
        }

        playerTransform = player.transform;
        playerRenderer = player.GetComponent<SpriteRenderer>();

        if (playerRenderer == null)
        {
            return;
        }

        UpdateChildList(); // 자식 초기화
    }

    void Update()
    {
        UpdateSortingOrders(); // ✨ 매 프레임마다 정렬 갱신
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
                    childRenderer.sortingOrder = playerRenderer.sortingOrder + 0;
                }
                else
                {
                    childRenderer.sortingOrder = playerRenderer.sortingOrder - 1;
                }
            }
        }
    }
}
