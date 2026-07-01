using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGridMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;   // 플레이어 이동 속도

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // 현재 입력 방향
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Rigidbody 권장 설정
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        //-------------------------
        // 입력 받기
        //-------------------------
        moveInput = Vector2.zero;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveInput.x = 1;

        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveInput.x = -1;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            moveInput.y = 1;

        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            moveInput.y = -1;

        // 대각선 속도 보정
        moveInput = moveInput.normalized;

        //-------------------------
        // 캐릭터 방향
        //-------------------------
        if (spriteRenderer != null)
        {
            if (moveInput.x > 0)
                spriteRenderer.flipX = false;

            else if (moveInput.x < 0)
                spriteRenderer.flipX = true;
        }

        //-------------------------
        // 애니메이션
        //-------------------------
        if (anim != null)
        {
            anim.SetFloat("Speed", moveInput.magnitude);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}