using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractiveObject : MonoBehaviour
{
    public ObjectType objectType;
    public bool isFrozen = false;

    [Header("Joy 기믹용 상태 변수")]
    public bool isTamed = false;       // 기쁨 마법을 맞아 마음을 연 상태
    public bool isFollowing = false;   // 플레이어를 졸졸 따라다니는 상태

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Transform playerTransform;
    private Queue<Vector3> playerPositionHistory = new Queue<Vector3>();

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // ★ 적이 플레이어를 따라다니는 로직
        if (objectType == ObjectType.Enemy && isFollowing && !isFrozen && playerTransform != null)
        {
            // 플레이어와의 거리가 벌어졌을 때만 타일 단위로 따라잡기
            if (Vector3.Distance(transform.position, playerTransform.position) > 1.1f)
            {
                // 플레이어가 밟았던 이전 위치 기록이 있다면 그곳으로 이동
                if (playerPositionHistory.Count > 0)
                {
                    Vector3 nextTargetPos = playerPositionHistory.Dequeue();
                    transform.position = nextTargetPos;

                    // TODO:여기에 적이 걸어가는 애니메이션을 트리거하세요!
                    // GetComponent<Animator>().SetTrigger("Walk");
                }
            }
        }
    }

    // 플레이어가 한 칸 움직일 때마다 플레이어 스크립트에서 이 함수를 호출해 줄 것입니다.
    public void RecordPlayerPosition(Vector3 prevPos)
    {
        if (objectType == ObjectType.Enemy && isFollowing)
        {
            playerPositionHistory.Enqueue(prevPos);
        }
    }

    public void ApplyFreeze()
    {
        isFrozen = true;
        if (spriteRenderer != null) spriteRenderer.color = Color.cyan;

        // ★ [추가된 핵심 코드] 물을 얼렸을 때만 길을 뚫어줍니다!
        if (objectType == ObjectType.Water)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true; // 물리적 충돌을 해제하여 플레이어가 위로 지나갈 수 있게 함
            }
        }
    }

    public void ApplyJoyTame()
    {
        if (objectType == ObjectType.Enemy)
        {
            isTamed = true;
            if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.6f, 0.8f); // 분홍빛으로 순해짐

            // TODO:여기에 하트가 뿅뿅 나오는 기쁨/순해짐 애니메이션이나 이펙트를 넣으세요!
            // GetComponent<Animator>().SetTrigger("Joy");
        }
    }

    public void StartFollowing(Vector3 startPos)
    {
        isFollowing = true;
        transform.position = startPos; // 플레이어 근처 칸으로 즉시 배치
        playerPositionHistory.Clear();

        // TODO:여기에 졸졸 따라오기 시작하는 애니메이션이나 눈빛 변화 연출을 넣으세요!
        // GetComponent<Animator>().SetBool("IsFollowing", true);
    }

    // ★ [핵심] 플레이어와 충돌(또는 진입)했을 때 처리하는 로직
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 적(Enemy)과 부딪혔을 때
            if (objectType == ObjectType.Enemy)
            {
                if (isFrozen) return;

                // ★ [수정 및 기획 반영] 
                // 내가 GameManager에 등록된 동료(tamedEnemy)이거나, 자체 변수가 변한 동료 상태라면
                // 부딪히거나 뚫고 지나가도 절대 죽이지 않고 안전하게 통과시킵니다!
                if ((GameManager.Instance != null && GameManager.Instance.tamedEnemy == this) || isTamed || isFollowing)
                {
                    return; // 아무 일도 일어나지 않고 안전하게 통과!
                }

                // ★ 동료가 아닌 '야생 상태의 적'과 그냥 걷다가 닿았을 때는 원래대로 게임오버가 됩니다.
                // (Attract 마법으로 당겨져서 닿았을 때의 처리는 InteractionManager에서 따로 원거리 즉사 처리를 해줍니다.)
                GameManager.Instance.TriggerGameOver("무서운 적에게 잡아 먹혔습니다!");
            }
            // 2. 탈출구(Exit)에 도착했을 때 ➡️ 열쇠 사용 확인 기믹!
            else if (objectType == ObjectType.Exit)
            {
                if (GameManager.Instance.hasKey)
                {
                    GameManager.Instance.DisplayLog("<color=green>★ 축하합니다! 열쇠로 굳게 닫힌 문을 열고 탈출에 성공했습니다! ★</color>");
                    GameManager.Instance.SetPlayerInputLock(true); // 조작 멈춤
                }
                else
                {
                    GameManager.Instance.DisplayLog("출구 문이 굳게 잠겨 있습니다. 맵 어딘가에 있는 '열쇠'가 필요합니다.");
                }
            }
        }

        // 3. ★ NPC를 마주쳤을 때 (적이 나를 따라오는 상태라면 NPC를 물리침)
        if (objectType == ObjectType.NPC)
        {
            InteractiveObject followingEnemy = FindFollowingEnemy();
            if (followingEnemy != null)
            {
                GameManager.Instance.DisplayLog("나를 따르는 적이 무서운 기세로 달려들어 경비원(NPC)을 쫓아내 버렸습니다!\n(두 대상이 모두 사라지며 길이 열렸습니다!)");

                followingEnemy.Vanish(); // 적 희생/퇴장
                this.Vanish();           // NPC 퇴장
            }
        }
    }

    private InteractiveObject FindFollowingEnemy()
    {
        InteractiveObject[] allObjs = Object.FindObjectsByType<InteractiveObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjs)
        {
            if (obj.objectType == ObjectType.Enemy && (obj.isFollowing || (GameManager.Instance != null && GameManager.Instance.tamedEnemy == obj))) return obj;
        }
        return null;
    }

    public void Vanish()
    {
        Destroy(gameObject);
    }
}
