using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringObject : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float interactionDistance = 2.0f; // ��ȣ�ۿ� �Ÿ�
    private Transform objsParent; // Objs �θ� ������Ʈ

    [SerializeField] private MaterialData data;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool isCollected = false;
    private bool isInRange = false; // ���� �� ���� ����
    
    [SerializeField] private GameObject guideImage; // �ν� �� ǥ���� ���̵� ������Ʈ

    private void Start()
    {
        // ��������Ʈ ����
        if (data.stateSprites.Count >= 1)
        {
            spriteRenderer.sprite = data.stateSprites[0];
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

        // �̹� ä���� ��� �� ��� ó�� ����
        if (isCollected)
        {
            SetGuideVisible(false);
            return;
        }

        // �ν� ���� ���� ��
        if (distance <= interactionDistance)
        {
            if (!isInRange)
            {
                isInRange = true;
                SetSprite(1);         // ���� ��������Ʈ
                SetGuideVisible(true);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Collect();
            }
        }
        // �ν� ���� ���� ��
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


    public void Collect()
    {
        if (isCollected)
            return;

        isCollected = true;

        SetSprite(2);            // ä�� �Ϸ� ��������Ʈ
        SetGuideVisible(false);  // ���̵� ����

        // gatherSoundIndex�� 0 �̻��̰� gatherSounds ����Ʈ ���� ���� �� ���
        if (data.gatherSoundIndex >= 0 && data.gatherSoundIndex < SoundManager.Instance.gatherSounds.Count)
        {
            SoundManager.Instance.PlayGatherSound(data.gatherSoundIndex);
        }

        if (GManager.Instance.IsInvenManager != null)
            GManager.Instance.IsInvenManager.AddItem(data.m_itemData, data.amount);
    }

    public void ResetCollectState()
    {
        isCollected = false;
        isInRange = false;

        // ��������Ʈ�� �⺻ ���·� �ǵ�����
        SetSprite(0);
        SetGuideVisible(false);
    }

}
