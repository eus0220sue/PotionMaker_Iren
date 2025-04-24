using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringObject : MonoBehaviour, InterAct.IInteractable
{
    [SerializeField] private GameObject player;
    [SerializeField] private float interactionDistance = 1.0f; // 상호작용 거리
    private Transform objsParent; // Objs 부모 오브젝트

    [SerializeField] private MaterialData data;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool isCollected = false;

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
        if (isCollected) return;

        if (player == null)
        {
            return;
        }

        // 거리 체크
        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (distance <= interactionDistance && Input.GetKeyDown(KeyCode.Space))
        {
            Collect();
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
        {
            return;
        }
        isCollected = true;
        // 스프라이트 변경
        if (data.stateSprites.Count >= 2)
        {
            spriteRenderer.sprite = data.stateSprites[1];
        }
        else
        {
        }

        // 인벤토리 추가
        if (GManager.Instance.IsinvenManager != null)
        {
            GManager.Instance.IsinvenManager.AddItem(data.m_itemData, data.amount);
        }
        else
        {
        }

    }
}
