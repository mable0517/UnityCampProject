using UnityEngine;
using UnityEngine.InputSystem;

public class SelectTile : MonoBehaviour
{
    [Header("타일 선택 설정")]
    public GameObject selectorVisual; // 1단계에서 만든 TileSelector 오브젝트 연결
    public float gridSize = 1f;       // 이동 기준과 동일한 1칸

    private Vector2 facingDirection = Vector2.down; // 기본 바라보는 방향
    private bool isSelectorActive = false;          // 선택기 활성화 상태

    void Start()
    {
        // 처음에는 커서가 꺼져 있도록 설정
        if (selectorVisual != null) selectorVisual.SetActive(false);
    }

    void Update()
    {
        // 1. Z 또는 Space 키를 눌러 선택기 켜기/끄기 (토글)
        if (Keyboard.current != null && (Keyboard.current.zKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            isSelectorActive = !isSelectorActive; // 상태 반전
            if (selectorVisual != null) selectorVisual.SetActive(isSelectorActive);
        }

        // 2. 선택기가 켜져 있을 때만 로직 실행
        if (isSelectorActive)
        {
            UpdateFacingDirection();
            UpdateSelectorPosition();
        }
    }

    private void UpdateFacingDirection()
    {
        if (Keyboard.current == null) return;

        // 상하좌우 입력 감지
        if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            facingDirection = Vector2.right;
        else if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            facingDirection = Vector2.left;
        else if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            facingDirection = Vector2.up;
        else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            facingDirection = Vector2.down;
    }

    private void UpdateSelectorPosition()
    {
        if (selectorVisual == null) return;

        // 플레이어 위치 기준 앞 1칸 계산
        Vector3 targetPos = transform.position + (Vector3)facingDirection * gridSize;

        // 좌표 스냅(정렬)
        targetPos.x = Mathf.Round(targetPos.x / gridSize) * gridSize;
        targetPos.y = Mathf.Round(targetPos.y / gridSize) * gridSize;
        targetPos.z = 0f; // 2D이므로 Z축 고정

        selectorVisual.transform.position = targetPos;
    }
}