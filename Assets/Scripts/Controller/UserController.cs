using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserController : ParentController
{
    [SerializeField] float m_moveSpeed = 5f; // 이동 속도
    public Vector2 m_input = Vector2.zero;
    public Rigidbody2D m_rb;
    public float lastMoveX = 1f; // 처음에는 왼쪽 보는 걸로 가정

    [SerializeField] Animator animator;

    private bool wasInteractKeyPressedLastFrame = false;
    private float m_interactCooldown = 0f;
    private const float INTERACT_DELAY = 0.2f;


    /// <summary>
    /// 인터렉터블인터페이스
    /// </summary>
    public IInteractableInterface currentTarget = null;
    public List<Collider2D> interactablesInRangeList = new List<Collider2D>();
    private IInteractableInterface m_focusedInteractable = null;
    /// <summary>
    /// 이동 플래그 
    /// </summary>
    public bool m_moveFlag = false;
    public bool IsMoveLock { get; private set; } = false;

    public bool isInteracting = false;
    private bool m_wasMovingLastFrame = false;


    private void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        // Rigidbody2D 설정
        m_rb.gravityScale = 0;
        m_rb.freezeRotation = true;
        m_rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        SetMoveFlag(true); // ← 최초에 한 번 이동 가능하게 설정

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
        if (GManager.Instance.IsUIManager.EscapeKeyUIOpenFlag)
        {
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
                UpdateFocus();

            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (interactablesInRangeList.Contains(other))
        {
            interactablesInRangeList.Remove(other);
            UpdateFocus();

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
    private void UpdateFocus()
    {
        // 가장 가까운 인터렉트 가능한 오브젝트 찾기
        IInteractableInterface closest = null;
        float closestDistance = float.MaxValue;

        foreach (var col in interactablesInRangeList)
        {
            var interactable = col.GetComponent<IInteractableInterface>();
            if (interactable == null) continue;

            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = interactable;
            }
        }

        if (m_focusedInteractable != closest)
        {
            if (m_focusedInteractable != null)
                m_focusedInteractable.OnFocusExit();

            m_focusedInteractable = closest;

            if (m_focusedInteractable != null)
                m_focusedInteractable.OnFocusEnter();
        }
    }
    /// <summary>
    /// 인터렉션 인터페이스 
    /// </summary>
    private void Interact()
    {
        Debug.Log("[UserController] Interact() 호출됨");

        if (isInteracting)
        {
            Debug.Log("[UserController] 이미 인터렉션 중이므로 중복 차단");
            return; // 중복 인터렉션 차단
        }

        if (GManager.Instance != null && GManager.Instance.IsUIManager != null && GManager.Instance.IsUIManager.UIOpenFlag)
        {
            Debug.Log("[UserController] UI가 열려 있어 상호작용 불가");
            return; // UI 열려있으면 인터렉션 차단
        }
        isInteracting = true;

        if (m_focusedInteractable != null)
        {
            Debug.Log($"[UserController] 상호작용 대상 있음");
            m_focusedInteractable.Interact();
        }
        else
        {
            Debug.Log("[UserController] 상호작용 대상 없음");
            isInteracting = false;
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

        //  쿨다운 중이면 입력 무시
        if (m_interactCooldown > 0f)
        {
            wasInteractKeyPressedLastFrame = isPressedNow;
            return;
        }

        //  인터렉트 중이면 입력 무시
        if (isInteracting)
        {
            wasInteractKeyPressedLastFrame = isPressedNow;
            return;
        }

        //  "떼었다가 다시 눌렀을 때"만 상호작용
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
        // 0번 사운드(발소리)를 반복 재생
        SoundManager.Instance.PlayPlayerSoundLoop(0); // SoundManager에 루프 재생 함수가 있어야 함
    }

    private void StopFootstepSound()
    {
        SoundManager.Instance.StopPlayerSound(0); // 사운드 정지 함수 필요
    }
    // 예시: 인터랙션 완료 콜백 메서드 추가
    public void OnInteractionComplete()
    {
        isInteracting = false;
    }

}
