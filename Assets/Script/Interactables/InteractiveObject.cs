using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractiveObject : MonoBehaviour
{
    public ObjectType objectType;
    public bool isFrozen = false;

    public bool isHarmless = false;

    [Header("Joy 기믹용 상태 변수")]
    public bool isTamed = false;
    public bool isFollowing = false;

    [Header("얼음 에셋")]
    public Sprite frozenSprite;

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
        if (objectType == ObjectType.Enemy && isFollowing && playerTransform != null)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) > 1.1f)
            {
                if (playerPositionHistory.Count > 0)
                {
                    Vector3 nextTargetPos = playerPositionHistory.Dequeue();
                    transform.position = nextTargetPos;
                }
            }
        }
    }

    public void RecordPlayerPosition(Vector3 prevPos)
    {
        if (objectType == ObjectType.Enemy && isFollowing) playerPositionHistory.Enqueue(prevPos);
    }

    public void ApplyFreeze()
    {
        isFrozen = true;
        if (spriteRenderer != null)
        {
            if (frozenSprite != null)
            {
                spriteRenderer.sprite = frozenSprite;
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.color = Color.cyan;
            }
        }

        if (objectType == ObjectType.Water)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }
    }

    public void ApplyJoyTame()
    {
        if (objectType == ObjectType.Enemy)
        {
            isTamed = true;
            if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.6f, 0.8f);
        }
    }

    public void StartFollowing(Vector3 startPos)
    {
        isFollowing = true;
        transform.position = startPos;
        playerPositionHistory.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (objectType == ObjectType.Enemy)
            {
                if ((GameManager.Instance != null && GameManager.Instance.tamedEnemy == this) || isTamed || isFollowing) return;

                // 야생 적과 부딪혀 사망 (피격 사운드)
                if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Hit");
                GameManager.Instance.TriggerGameOver("무서운 적에게 잡아 먹혔습니다!");
            }
            else if (objectType == ObjectType.Exit)
            {
                if (GameManager.Instance.hasKey)
                {
                    if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Interaction");
                    GameManager.Instance.DisplayLog("<color=green>★ 축하합니다! 열쇠로 굳게 닫힌 문을 열고 탈출에 성공했습니다! ★</color>");
                    GameManager.Instance.SetPlayerInputLock(true);
                }
                else
                {
                    // 열쇠 없이 막힘 (잘못된 플레이)
                    if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Hit");
                    GameManager.Instance.DisplayLog("출구 문이 굳게 잠겨 있습니다. 맵 어딘가에 있는 '열쇠'가 필요합니다.");
                }
            }
        }

        if (objectType == ObjectType.NPC)
        {
            InteractiveObject followingEnemy = FindFollowingEnemy();
            if (followingEnemy != null)
            {
                if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Interaction");
                GameManager.Instance.DisplayLog("나를 따르는 적이 무서운 기세로 달려들어 경비원(NPC)을 쫓아내 버렸습니다!");
                followingEnemy.Vanish();
                this.Vanish();
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