using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EmbarassmentEmotionEffect
{
    private const float Duration = 6f;
    private const float ScaleRate = 0.55f;

    private static readonly HashSet<Transform> activeTargets = new HashSet<Transform>();

    public static void UseOnSelf()
    {
        if (GameManager.Instance == null) return;

        Transform player = FindPlayer();
        if (player == null) return;

        StartShrink(player, null, false, false);
        GameManager.Instance.DisplayLog("부끄러움에 몸이 작아졌습니다.");
    }

    public static void UseOnTarget(InteractiveObject target)
    {
        if (GameManager.Instance == null || target == null) return;

        if (target.objectType == ObjectType.NPC)
        {
            StartShrink(target.transform, target, true, false);
            GameManager.Instance.DisplayLog("NPC가 부끄러워하며 몸을 웅크렸습니다.");
            return;
        }

        if (target.objectType == ObjectType.Enemy)
        {
            StartShrink(target.transform, target, true, true);
            GameManager.Instance.DisplayLog("적이 부끄러워 몸집이 작아지고 위협도 약해졌습니다.");
            return;
        }

        GameManager.Instance.DisplayLog("부끄러움이 퍼졌지만 큰 변화는 없습니다.");
    }

    private static void StartShrink(Transform target, InteractiveObject interactiveObject, bool weakenCollider, bool makeHarmless)
    {
        if (target == null) return;

        if (activeTargets.Contains(target))
        {
            GameManager.Instance.DisplayLog("이미 부끄러워 몸이 작아져 있습니다.");
            return;
        }

        GameManager.Instance.StartCoroutine(ShrinkRoutine(target, interactiveObject, weakenCollider, makeHarmless));
    }

    private static IEnumerator ShrinkRoutine(Transform target, InteractiveObject interactiveObject, bool weakenCollider, bool makeHarmless)
    {
        activeTargets.Add(target);

        Vector3 originalScale = target.localScale;
        Collider2D collider = target.GetComponent<Collider2D>();
        bool originalTrigger = collider != null && collider.isTrigger;
        bool originalHarmless = interactiveObject != null && interactiveObject.isHarmless;

        target.localScale = originalScale * ScaleRate;

        if (weakenCollider && collider != null)
        {
            collider.isTrigger = true;
        }

        if (makeHarmless && interactiveObject != null)
        {
            interactiveObject.isHarmless = true;
        }

        yield return new WaitForSeconds(Duration);

        if (target != null)
        {
            target.localScale = originalScale;
        }

        if (weakenCollider && collider != null)
        {
            collider.isTrigger = originalTrigger;
        }

        if (makeHarmless && interactiveObject != null)
        {
            interactiveObject.isHarmless = originalHarmless;
        }

        activeTargets.Remove(target);
    }

    private static Transform FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player.transform;

        PlayerGridMovement movement = Object.FindFirstObjectByType<PlayerGridMovement>();
        return movement != null ? movement.transform : null;
    }
}
