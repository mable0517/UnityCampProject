using System.Collections;
using UnityEngine;

public static class FearEmotionEffect
{
    private const float SelfDuration = 6f;
    private const float SlowRate = 0.5f;
    private const float VisionRate = 1.35f;
    private const float NpcFearDuration = 6f;
    private const float NpcSightRange = 5f;
    private const float NpcFleeSpeed = 2f;
    private const float EnemyStunDuration = 3f;

    private static bool selfFearActive;

    public static void UseOnSelf()
    {
        if (GameManager.Instance == null) return;

        if (selfFearActive)
        {
            GameManager.Instance.DisplayLog("이미 두려움에 몸이 굳어 있습니다.");
            return;
        }

        GameManager.Instance.StartCoroutine(SelfFearRoutine());
    }

    public static void UseOnTarget(InteractiveObject target)
    {
        if (GameManager.Instance == null || target == null) return;

        if (target.objectType == ObjectType.NPC)
        {
            GameManager.Instance.StartCoroutine(NpcFleeRoutine(target));
            GameManager.Instance.DisplayLog("NPC가 겁에 질렸습니다. 당신을 보면 도망치려 합니다.");
            return;
        }

        if (target.objectType == ObjectType.Enemy)
        {
            GameManager.Instance.StartCoroutine(EnemyStunRoutine(target));
            GameManager.Instance.DisplayLog("적이 공포에 질려 잠시 굳어버렸습니다.");
            return;
        }

        GameManager.Instance.DisplayLog("두려움이 퍼졌지만 큰 변화는 없습니다.");
    }

    private static IEnumerator SelfFearRoutine()
    {
        selfFearActive = true;

        PlayerGridMovement movement = Object.FindFirstObjectByType<PlayerGridMovement>();
        Camera mainCamera = Camera.main;

        float originalSpeed = movement != null ? movement.moveSpeed : 0f;
        float originalCameraSize = mainCamera != null ? mainCamera.orthographicSize : 0f;

        if (movement != null) movement.moveSpeed = originalSpeed * SlowRate;
        if (mainCamera != null) mainCamera.orthographicSize = originalCameraSize * VisionRate;

        GameManager.Instance.DisplayLog("두려움이 몰려옵니다. 더 멀리 보이지만 몸이 굳어 움직임이 느려집니다.");

        yield return new WaitForSeconds(SelfDuration);

        if (movement != null) movement.moveSpeed = originalSpeed;
        if (mainCamera != null) mainCamera.orthographicSize = originalCameraSize;

        selfFearActive = false;
    }

    private static IEnumerator NpcFleeRoutine(InteractiveObject npc)
    {
        Transform player = FindPlayer();
        float endTime = Time.time + NpcFearDuration;

        while (npc != null && player != null && Time.time < endTime)
        {
            Vector3 direction = npc.transform.position - player.position;

            if (direction.magnitude <= NpcSightRange)
            {
                npc.transform.position += direction.normalized * NpcFleeSpeed * Time.deltaTime;
            }

            yield return null;
        }
    }

    private static IEnumerator EnemyStunRoutine(InteractiveObject enemy)
    {
        if (enemy == null) yield break;

        bool wasFrozen = enemy.isFrozen;
        SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        enemy.isFrozen = true;
        if (spriteRenderer != null && !wasFrozen) spriteRenderer.color = Color.gray;

        yield return new WaitForSeconds(EnemyStunDuration);

        if (enemy == null || wasFrozen) yield break;

        enemy.isFrozen = false;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    private static Transform FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player.transform;

        PlayerGridMovement movement = Object.FindFirstObjectByType<PlayerGridMovement>();
        return movement != null ? movement.transform : null;
    }
}
