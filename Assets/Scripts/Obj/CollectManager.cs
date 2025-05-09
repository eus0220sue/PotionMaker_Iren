using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringObject : MonoBehaviour, InterAct.IInteractable
{
    [SerializeField] private GameObject player;
    [SerializeField] private float interactionDistance = 2.0f; // 상호작용 거리
    private Transform objsParent; // Objs 부모 오브젝트

    [SerializeField] private MaterialData data;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool isCollected = false;
    private bool isInRange = false; // 범위 내 진입 여부
    
    [SerializeField] private GameObject guideImage; // 인식 시 표시할 가이드 오브젝트


    /// <summary>
    /// 해당 아이템의 인덱스 (현재 사용 안함)
    /// </summary>
    //[SerializeField] private int m_index = 0;

    private void Start()
    {
        // 스프라이트 세팅
        if (data.stateSprites.Count >= 1)
        {
            spriteRenderer.sprite = data.stateSprites[0];
        }
        else
        {
        }
    }
    private void Update()
    {
        HandleDetection();

    }
    private void SetSprite(int index)
    {
        if (data.stateSprites.Count > index)
            spriteRenderer.sprite = data.stateSprites[index];
    }

    private void SetGuideVisible(bool isVisible)
    {
        if (guideImage != null)
            guideImage.SetActive(isVisible);
    }
    private void HandleDetection()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(player.transform.position, transform.position);

        // 이미 채집된 경우 → 모든 처리 무시
        if (isCollected)
        {
            SetGuideVisible(false);
            return;
        }

        // 인식 범위 안일 때
        if (distance <= interactionDistance)
        {
            if (!isInRange)
            {
                isInRange = true;
                SetSprite(1);         // 강조 스프라이트
                SetGuideVisible(true);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Collect();
            }
        }
        // 인식 범위 밖일 때
        else
        {
            if (isInRange)
            {
                isInRange = false;
                SetSprite(0);
                SetGuideVisible(false);
            }
        }
    }

    public void Interact()
    {
        // 상호작용 로직
        if (isCollected)
        {
            return;
        }
        isCollected = true;
        Collect();
    }
    public void Collect()
    {
        if (isCollected)
            return;

        isCollected = true;

        SetSprite(2);            // 채집 완료 스프라이트
        SetGuideVisible(false);  // 가이드 숨김

        if (GManager.Instance.IsinvenManager != null)
            GManager.Instance.IsinvenManager.AddItem(data.m_itemData, data.amount);
    }
}
