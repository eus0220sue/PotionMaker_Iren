using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserController : ParentController
{
    [SerializeField] private float m_moveSpeed = 5f; // 이동 속도
    private Vector2 m_input = Vector2.zero;
    private Rigidbody2D m_rb;
    private int wallLayer; // 벽 레이어 저장
    private float lastMoveX = 1f; // 처음에는 왼쪽 보는 걸로 가정
    [SerializeField] private Animator animator;
    /// <summary>
    /// 인터렉터블인터페이스
    /// </summary>
    private IInteractableInterface currentTarget = null;

    private List<Collider2D> interactablesInRangeList = new List<Collider2D>();
    /// <summary>
    /// 이동 플래그 
    /// </summary>
    public bool m_moveFlag = false;
    public bool IsMoveLock { get; private set; } = false;

    public bool isInteracting = false;


    private void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        wallLayer = LayerMask.NameToLayer("Wall"); // "Wall" 레이어 가져오기
        // Rigidbody2D 설정
        m_rb.gravityScale = 0;
        m_rb.freezeRotation = true;
        m_rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        SetMoveFlag(true); // ← 최초에 한 번 이동 가능하게 설정

    }

    void Update()
    {
        if (GManager.Instance != null && GManager.Instance.TPFlag) return;
        Interact();
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
            return;
        }
        if (GManager.Instance == null)
        {
            Debug.LogError("[UserController] GManager.Instance가 null입니다.");
            return;
        }

        if (GManager.Instance.IsInventoryUI == null)
        {
            Debug.LogError("[UserController] IsInventoryUI가 null입니다.");
            return;
        }

        if (GManager.Instance.IsInventoryUI.isOpen)
        {
            Debug.Log("[UserController] 인벤토리 UI가 열려 있어 이동 불가.");
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

        // 아래는 입력 정상 처리
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
    }
    /// <summary>
    /// 충돌체크
    /// </summary>
    /// <param name="other">충돌한 콜라이더</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractableInterface>();
        if (interactable != null)
        {
            if (!interactablesInRangeList.Contains(other))
            {
                interactablesInRangeList.Add(other);
                Debug.Log($"[TriggerEnter] 리스트에 추가됨: {other.gameObject.name}");
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
    /// 인터렉션 인터페이스 
    /// </summary>
    private void Interact()
    {
        if (isInteracting) return; // 중복 인터렉션 차단
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isInteracting = true;

            if (interactablesInRangeList.Count == 0)
            {
                Debug.Log($"[Interact] 대상 확인 실패:");
                isInteracting = false;
                return;
            }

            foreach (var col in interactablesInRangeList)
            {
                Debug.Log($"[Interact] 대상 확인 중: {col.gameObject.name}");

                var target = col.GetComponent<IInteractableInterface>();
                if (target != null)
                {
                    Debug.Log($"[Interact] 상호작용 시도: {col.gameObject.name}");
                    target.Interact();
                    break;
                }
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
}
