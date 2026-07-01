using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // 새로운 Input System 패키지 유지

public class PlayerGridMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;      // 이동하는 속도
    public float gridSize = 1f;       // ★ 요청하신 대로 1칸 단위 이동으로 변경!
    public float moveDelay = 0.1f;    // 꾹 누르고 있을 때 확실하게 한 칸씩 끊어지게 하는 딜레이

    private bool isMoving = false;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 그리드 이동 시 물리 충돌체(Rigidbody2D)가 밀어내는 것을 방지하기 위해 
        // Rigidbody2D가 있다면 이 스크립트가 켜질 때 Is Kinematic을 켜주는 것이 좋습니다.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }
    }

    void Update()
    {
        // 이미 이동 중이 아닐 때만 다음 키 입력을 받음
        if (!isMoving && Keyboard.current != null)
        {
            Vector3 targetPosition = transform.position;
            bool hasInput = false;

            // 1. 좌우 입력 확인
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                targetPosition += Vector3.right * gridSize;
                hasInput = true;
                if (spriteRenderer != null) spriteRenderer.flipX = false; // Scale 안 건드리고 우측 조준
            }
            else if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                targetPosition += Vector3.left * gridSize;
                hasInput = true;
                if (spriteRenderer != null) spriteRenderer.flipX = true;  // Scale 안 건드리고 좌측 조준
            }
            // 2. 상하 입력 확인 (그리드 이동이므로 대각선 이동을 막기 위해 else if 처리)
            else if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            {
                targetPosition += Vector3.up * gridSize;
                hasInput = true;
            }
            else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            {
                targetPosition += Vector3.down * gridSize;
                hasInput = true;
            }

            // 입력이 감지되었다면 이동 코루틴 실행
            if (hasInput)
            {
                StartCoroutine(MoveToGrid(targetPosition));
            }
        }

        // 3. 애니메이터에 상태 전달 (이동 중일 때 Run, 멈췄을 때 Idle)
        if (anim != null)
        {
            anim.SetFloat("Speed", isMoving ? 1f : 0f);
        }
    }

    // 부드럽게 목표 그리드로 이동시키는 코루틴
    IEnumerator MoveToGrid(Vector3 targetPos)
    {
        isMoving = true;

        // 목표 위치에 완전히 도달할 때까지 프레임마다 이동
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 정확한 그리드 좌표에 안착
        transform.position = targetPos;

        // [중요 기준] 이동 완료 후 잠깐 대기하여, 키를 꾹 눌러도 1칸씩 확실히 끊어지는 손맛 부여
        if (moveDelay > 0f)
        {
            yield return new WaitForSeconds(moveDelay);
        }

        isMoving = false;
    }
}