using UnityEngine;
using System.Collections;

public class MapPortal : MonoBehaviour
{
    [Header("이동할 목표 위치")]
    [SerializeField] private GameObject targetObject;

    [Header("현재 맵 / 다음 맵")]
    [SerializeField] private GameObject currentMapGroup;
    [SerializeField] private GameObject nextMapGroup;

    private bool isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TeleportRoutine(collision.gameObject));
        }
    }

    private IEnumerator TeleportRoutine(GameObject player)
    {
        isTeleporting = true;

        GManager.Instance.SetTPFlag(true);

        // 이동 시작할 때 입력/모션 끊기
        UserController userController = player.GetComponent<UserController>();
        if (userController != null)
        {
            userController.ResetMoveAndAnimation();
        }

        // 페이드 아웃
        yield return StartCoroutine(GManager.Instance.IsFadeInOut.FadeOut());

        // 현재 맵 비활성화
        if (currentMapGroup != null)
            currentMapGroup.SetActive(false);

        // 다음 맵 활성화
        if (nextMapGroup != null)
        {
            nextMapGroup.SetActive(true);
            Debug.Log($"[MapPortal] 다음 맵 활성화: {nextMapGroup.name}");

            // 카메라 제한 자동 계산
            SetCameraBoundsByNextMap();

            // BGM 자동 재생 (맵 이름 기준)
            GManager.Instance.mapBGMController.PlayBGMForMap(nextMapGroup);
        }
        else
        {
            Debug.LogWarning("[MapPortal] nextMapGroup이 null입니다!");
        }


        if (targetObject != null)
            player.transform.position = targetObject.transform.position;

        GManager.Instance.StartTPAfterTeleport();

        isTeleporting = false;
    }

    /// <summary>
    /// 다음 맵의 BoxCollider2D로 카메라 제한 자동 설정
    /// </summary>
    private void SetCameraBoundsByNextMap()
    {
        if (nextMapGroup == null) return;

        BoxCollider2D collider = nextMapGroup.GetComponent<BoxCollider2D>();

        if (collider != null)
        {
            Bounds bounds = collider.bounds;

            Vector2 min = bounds.min;
            Vector2 max = bounds.max;

            GManager.Instance.IsCameraBase.SetCameraBounds(min, max);
            Debug.Log($"[MapPortal] 카메라 제한 자동 설정: Min {min} / Max {max}");
        }
        else
        {
            Debug.LogWarning("[MapPortal] 다음 맵에 BoxCollider2D가 없습니다! 카메라 제한 설정 실패");
        }
    }
}
