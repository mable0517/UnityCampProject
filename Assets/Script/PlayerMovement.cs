using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFreeMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer; // 스프라이트를 뒤집기 위해 추가
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // 컴포넌트 가져오기

        if (rb != null) rb.gravityScale = 0f;
    }

    void Update()
    {
        movement = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) movement.x = 1f;
            else if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) movement.x = -1f;

            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed) movement.y = 1f;
            else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed) movement.y = -1f;
        }

        movement = movement.normalized;

        if (anim != null)
        {
            anim.SetFloat("Speed", movement.sqrMagnitude);
        }

        // 수정된 부분: Scale을 건드리지 않고 이미지 방향만 뒤집기
        if (spriteRenderer != null)
        {
            if (movement.x > 0)
                spriteRenderer.flipX = false; // 오른쪽 볼 때 원본 유지
            else if (movement.x < 0)
                spriteRenderer.flipX = true;  // 왼쪽 볼 때 좌우 반전
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}