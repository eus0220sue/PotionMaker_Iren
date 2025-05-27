using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserController : ParentController
{
    [SerializeField] float m_moveSpeed = 5f; // �̵� �ӵ�
    public Vector2 m_input = Vector2.zero;
    public Rigidbody2D m_rb;
    public float lastMoveX = 1f; // ó������ ���� ���� �ɷ� ����

    [SerializeField] Animator animator;

    private bool wasInteractKeyPressedLastFrame = false;
    private float m_interactCooldown = 0f;
    private const float INTERACT_DELAY = 0.2f;


    /// <summary>
    /// ���ͷ��ͺ��������̽�
    /// </summary>
    public IInteractableInterface currentTarget = null;

    public List<Collider2D> interactablesInRangeList = new List<Collider2D>();
    /// <summary>
    /// �̵� �÷��� 
    /// </summary>
    public bool m_moveFlag = false;
    public bool IsMoveLock { get; private set; } = false;

    public bool isInteracting = false;
    private bool m_wasMovingLastFrame = false;


    private void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        // Rigidbody2D ����
        m_rb.gravityScale = 0;
        m_rb.freezeRotation = true;
        m_rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        SetMoveFlag(true); // �� ���ʿ� �� �� �̵� �����ϰ� ����

    }

    void Update()
    {
        if (GManager.Instance != null && GManager.Instance.TPFlag) return;
        if (m_interactCooldown > 0f)
            m_interactCooldown -= Time.deltaTime;

        InputBuffer();
        Move();
    }
    private void FixedUpdate()
    {
        if (!m_moveFlag)
        {
            m_rb.velocity = Vector2.zero;
            return;
        }

        m_rb.velocity = m_input * m_moveSpeed;
    }

    public override void Move()
    {
        if (!m_moveFlag)
        {
            m_input = Vector2.zero;
            animator.SetBool("isMove", false);
            StopFootstepSound();
            return;
        }
        if (GManager.Instance == null)
        {
            Debug.LogError("[UserController] GManager.Instance�� null�Դϴ�.");
            return;
        }

        if (GManager.Instance.IsInventoryUI == null)
        {
            Debug.LogError("[UserController] IsInventoryUI�� null�Դϴ�.");
            return;
        }

        if (GManager.Instance.IsInventoryUI.isOpen)
        {
            Debug.Log("[UserController] �κ��丮 UI�� ���� �־� �̵� �Ұ�.");
            m_input = Vector2.zero;
            m_rb.velocity = Vector2.zero;
            animator.SetBool("isMove", false);
            return;
        }

        if (GManager.Instance.IsUIManager.UIOpenFlag)
        {
            m_input = Vector2.zero;
            m_rb.velocity = Vector2.zero;
            animator.SetBool("isMove", false);
            return;
        }

        if (GManager.Instance.TPFlag)
        {
            m_input = Vector2.zero;
            m_rb.velocity = Vector2.zero;
            animator.SetBool("isMove", false);
            return;
        }

        // �Ʒ��� �Է� ���� ó��
        m_input.x = Input.GetAxisRaw("Horizontal");
        m_input.y = Input.GetAxisRaw("Vertical");
        m_input = m_input.normalized;

        bool isMove = m_input.magnitude > 0.1f;
        animator.SetBool("isMove", isMove);

        if (isMove)
        {
            if (Mathf.Abs(m_input.x) > 0.1f)
            {
                lastMoveX = m_input.x > 0 ? 1f : -1f;
            }
            animator.SetFloat("moveX", lastMoveX);
        }
        if (isMove != m_wasMovingLastFrame)
        {
            if (isMove)
            {
                PlayFootstepSound();
            }
            else
            {
                StopFootstepSound();
            }
            m_wasMovingLastFrame = isMove;
        }
    }
    /// <summary>
    /// �浹üũ
    /// </summary>
    /// <param name="other">�浹�� �ݶ��̴�</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractableInterface>();
        if (interactable != null)
        {
            if (!interactablesInRangeList.Contains(other))
            {
                interactablesInRangeList.Add(other);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (interactablesInRangeList.Contains(other))
        {
            interactablesInRangeList.Remove(other);
        }
    }
    public void SetMoveFlag(bool m_moveFlag)
    {
        this.m_moveFlag = m_moveFlag;

        if (!m_moveFlag)
        {
            m_input = Vector2.zero;          
            m_rb.velocity = Vector2.zero;     
            animator.SetBool("isMove", false);
        }
    }
    /// <summary>
    /// ���ͷ��� �������̽� 
    /// </summary>
    private void Interact()
    {
        if (isInteracting) return; // �ߺ� ���ͷ��� ����

        // UI�� ���� ������ ��ȣ�ۿ� ����
        if (GManager.Instance != null && GManager.Instance.IsUIManager != null)
        {
            if (GManager.Instance.IsUIManager.UIOpenFlag)
            {
                return;
            }
        }

        isInteracting = true;

        if (interactablesInRangeList.Count == 0)
        {
            return;
        }

        foreach (var col in interactablesInRangeList)
        {

            var target = col.GetComponent<IInteractableInterface>();
            if (target != null)
            {
                target.Interact();
                break;
            }
        }
    }

    public void ResetMoveAndAnimation()
    {
        m_input = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool("isMove", false);
        }
    }

    public void InputBuffer()
    {
        bool isPressedNow = Input.GetKey(KeyCode.Space);

        //  ��ٿ� ���̸� �Է� ����
        if (m_interactCooldown > 0f)
        {
            wasInteractKeyPressedLastFrame = isPressedNow;
            return;
        }

        //  ���ͷ�Ʈ ���̸� �Է� ����
        if (isInteracting)
        {
            wasInteractKeyPressedLastFrame = isPressedNow;
            return;
        }

        //  "�����ٰ� �ٽ� ������ ��"�� ��ȣ�ۿ�
        if (!wasInteractKeyPressedLastFrame && isPressedNow)
        {
            Interact();
        }

        wasInteractKeyPressedLastFrame = isPressedNow;
    }

    public void StartInteractCooldown()
    {
        m_interactCooldown = INTERACT_DELAY;
    }

    private void PlayFootstepSound()
    {
        // 0�� ����(�߼Ҹ�)�� �ݺ� ���
        SoundManager.Instance.PlayPlayerSoundLoop(0); // SoundManager�� ���� ��� �Լ��� �־�� ��
    }

    private void StopFootstepSound()
    {
        SoundManager.Instance.StopPlayerSound(0); // ���� ���� �Լ� �ʿ�
    }

}
