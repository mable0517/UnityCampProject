using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // 새로운 Input System 사용을 위해 추가

public class GridMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f; // 이동하는 속도
    // 기본값을 0.5f로 수정하여 0.5칸 단위로 이동하게 설정
    public float gridSize = 0.5f;
    public float moveDelay = 0.1f; // 이동 완료 후 다음 이동까지의 대기 시간 (딜레이)

    private bool isMoving = false; // 현재 이동 중인지 확인하는 플래그

    void Update()
    {
        // 이동 중이 아닐 때, 그리고 키보드가 연결되어 있을 때만 키 입력을 받음
        if (!isMoving && Keyboard.current != null)
        {
            float inputX = 0f;
            float inputY = 0f;

            // 새로운 Input System 방식으로 방향키 및 WASD 입력 받기
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) inputX = 1f;
            else if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) inputX = -1f;

            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed) inputY = 1f;
            else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed) inputY = -1f;

            // 4방향 이동만 허용 (대각선 이동 방지 로직)
            if (inputX != 0) inputY = 0;

            // 입력이 발생했다면
            if (inputX != 0 || inputY != 0)
            {
                // 현재 위치에서 입력받은 방향으로 한 칸(gridSize)만큼 떨어진 목표 위치 계산
                Vector3 targetPos = transform.position + new Vector3(inputX * gridSize, inputY * gridSize, 0f);

                // 이동 코루틴 실행
                StartCoroutine(MoveToGrid(targetPos));
            }
        }
    }

    // 부드럽게 목표 위치로 이동시키는 코루틴
    IEnumerator MoveToGrid(Vector3 targetPos)
    {
        isMoving = true; // 이동 상태 켜기 (다른 입력 차단)

        // 현재 위치가 목표 위치에 도달할 때까지 반복
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // Vector3.MoveTowards를 사용해 프레임마다 일정 속도로 목표를 향해 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null; // 다음 프레임까지 대기
        }

        // 오차를 없애기 위해 목표 위치에 정확히 안착
        transform.position = targetPos;

        // 다음 이동까지 잠깐 대기하여 한 칸씩 끊어지는 느낌을 부여
        if (moveDelay > 0f)
        {
            yield return new WaitForSeconds(moveDelay);
        }

        isMoving = false; // 이동 상태 끄기 (다시 입력 허용)
    }
}