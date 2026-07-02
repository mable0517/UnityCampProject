using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    private SelectTile selectTile;
    public LayerMask interactionLayer;

    void Start()
    {
        selectTile = GetComponent<SelectTile>();
    }

    void Update()
    {
        if (Keyboard.current == null || selectTile == null) return;

        if (selectTile.selectorVisual != null && selectTile.selectorVisual.activeSelf && Keyboard.current.xKey.wasPressedThisFrame)
        {
            ExecuteWordAction();
        }
    }

    private void ExecuteWordAction()
    {
        if (GameManager.Instance == null || selectTile.selectorVisual == null) return;

        Vector3 cursorPosition = selectTile.selectorVisual.transform.position;
        EmotionType emotion = GameManager.Instance.currentEmotion;

        Collider2D hit = Physics2D.OverlapBox(cursorPosition, new Vector2(0.8f, 0.8f), 0f, interactionLayer);

        if (hit != null)
        {
            InteractiveObject targetObj = hit.GetComponent<InteractiveObject>();
            if (targetObj != null)
            {
                // [탈출구 및 열쇠 판정]
                if (targetObj.objectType == ObjectType.Exit)
                {
                    if (GameManager.Instance.hasKey) GameManager.Instance.ClearStage();
                    else GameManager.Instance.DisplayLog("문이 잠겨 있습니다. 열쇠가 필요합니다.");
                    return;
                }

                if (emotion == EmotionType.Fear || emotion == EmotionType.Embarassment)
                {
                    HandleTargetInteraction(emotion, targetObj);
                    return;
                }

                // [동료 적에게 X키 명령 - 비켜서기]
                if (targetObj.objectType == ObjectType.Enemy && GameManager.Instance.tamedEnemy == targetObj)
                {
                    PerformFollowerStepAside(targetObj);
                    return;
                }

                // [부하와 함께 NPC 처치]
                if (targetObj.objectType == ObjectType.NPC && GameManager.Instance.tamedEnemy != null)
                {
                    PerformFollowerAttackNPC(targetObj);
                    return;
                }

                // [괴력 밀치기 - 동료가 아닐 때만]
                if (GameManager.Instance.isSuperPowered && GameManager.Instance.tamedEnemy != targetObj &&
                   (targetObj.objectType == ObjectType.NPC || targetObj.objectType == ObjectType.Enemy))
                {
                    PerformSuperPowerPush(targetObj);
                    return;
                }

                if (emotion == EmotionType.Attract)
                {
                    HandleCloseAttract(targetObj);
                    return;
                }

                HandleTargetInteraction(emotion, targetObj);
                return;
            }
        }
        else
        {
            if (emotion == EmotionType.Attract)
            {
                PerformLongDistanceAttract(cursorPosition);
                return;
            }
        }

        HandleSelfInteraction(emotion);
    }

    private void PerformFollowerStepAside(InteractiveObject follower)
    {
        Vector3 playerToFollower = (follower.transform.position - transform.position).normalized;
        Vector3 sideDirection = new Vector3(-playerToFollower.y, playerToFollower.x, 0f);

        follower.transform.position += sideDirection * selectTile.gridSize;
        GameManager.Instance.DisplayLog("부하 적: '크르릉! (넵 알겠습니다!)'\n부하가 옆 칸으로 공손히 비켜섰습니다.");
    }

    private void PerformFollowerAttackNPC(InteractiveObject npc)
    {
        // ★ 부하가 공격할 때 경쾌한 상호작용(또는 피격) 사운드 재생!
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Interaction");

        GameManager.Instance.DisplayLog("부하 적이 포효하며 경비원(NPC)을 날려버렸습니다!\n길이 열렸습니다!");
        npc.Vanish();
    }

    private void PerformLongDistanceAttract(Vector3 cursorPosition)
    {
        Vector2 dir = (cursorPosition - transform.position).normalized;
        dir.x = Mathf.Round(dir.x);
        dir.y = Mathf.Round(dir.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(cursorPosition, dir, 15f, interactionLayer);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        InteractiveObject targetObj = null;

        foreach (var hit in hits)
        {
            InteractiveObject obj = hit.collider.GetComponent<InteractiveObject>();
            if (obj != null)
            {
                if (obj.objectType == ObjectType.Water) continue;
                targetObj = obj;
                break;
            }
        }

        if (targetObj == null || targetObj.objectType == ObjectType.Wall)
        {
            TriggerAttractTrap();
            return;
        }

        if (targetObj.isFrozen)
        {
            if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Hit");
            GameManager.Instance.DisplayLog("저 멀리 꽁꽁 얼어붙은 대상이 보이지만 끌려오지 않습니다.");
            return;
        }

        switch (targetObj.objectType)
        {
            case ObjectType.Key:
                if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Interaction");
                GameManager.Instance.DisplayLog("물 너머 있던 열쇠가 자력에 이끌려 손안으로 쏙 날아왔습니다!");
                GameManager.Instance.hasKey = true;
                targetObj.Vanish();
                break;
            case ObjectType.Fruit:
                if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Interaction");
                GameManager.Instance.DisplayLog("물 너머 있던 과일이 내 바로 앞칸으로 슝 날아왔습니다!");
                targetObj.transform.position = cursorPosition;
                break;
            case ObjectType.NPC:
                if (GameManager.Instance.isSuperPowered)
                {
                    if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Interaction");
                    GameManager.Instance.DisplayLog("괴력을 발휘해 경비원을 내 앞까지 훅 끌어당겼습니다!");
                    targetObj.transform.position = cursorPosition;
                }
                else
                {
                    if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Hit");
                    GameManager.Instance.DisplayLog("경비원이 너무 무거워서 끌려오지 않습니다. (괴력 필요)");
                }
                break;
            case ObjectType.Enemy:
                if (GameManager.Instance.tamedEnemy == targetObj)
                {
                    if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Interaction");
                    GameManager.Instance.DisplayLog("나의 부하를 내 앞으로 끌어당겼습니다.");
                    targetObj.transform.position = cursorPosition;
                }
                else
                {
                    // ★ 멀리 있는 야생 적을 코앞으로 당겼을 때도 갑툭튀(Trap) 사운드!
                    if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Trap");
                    targetObj.transform.position = cursorPosition;
                    GameManager.Instance.TriggerGameOver("으악! 물 너머에 안전하게 있던 무서운 적을 내 바로 코앞으로 끌고 와 버렸습니다! 적에게 잡아먹혔습니다!");
                }
                break;
        }
    }

    private void TriggerAttractTrap()
    {
        if (GameManager.Instance.tamedEnemy != null)
        {
            if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Hit");
            GameManager.Instance.DisplayLog("허공에 자력을 뿜었지만, 부하가 자력을 흡수해 아무 일도 일어나지 않았습니다.");
            return;
        }

        InteractiveObject enemy = FindEnemyInScene();
        if (enemy != null)
        {
            // ★ 여기에 갑툭튀 사운드 추가!
            if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Trap");
            enemy.transform.position = transform.position;
            GameManager.Instance.TriggerGameOver("허공에 끌림 마법을 시도하자 자력이 폭주했습니다!\n강력한 자력에 이끌린 적이 순식간에 날아와 당신을 덮쳤습니다!");
        }
        else
        {
            if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Hit");
            GameManager.Instance.DisplayLog("허공에 자력을 뿜었지만 주변에 반응할 적이 없습니다.");
        }
    }

    private void HandleCloseAttract(InteractiveObject target)
    {
        if (target.isFrozen) return;

        if (target.objectType == ObjectType.Key)
        {
            // ★ 열쇠 획득 사운드 추가!
            if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Interaction");
            GameManager.Instance.DisplayLog("눈앞의 열쇠를 챙겼습니다!");
            GameManager.Instance.hasKey = true;
            target.Vanish();
        }
        else if (target.objectType == ObjectType.Fruit)
        {
            // ★ 과일 당겨올 때도 사운드 추가!
            if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Interaction");
            GameManager.Instance.DisplayLog("눈앞의 과일을 내 위치로 당겨왔습니다.");
            target.transform.position = transform.position;
        }
    }

    private void HandleSelfInteraction(EmotionType emotion)
    {
        switch (emotion)
        {
            case EmotionType.Attract: TriggerAttractTrap(); break;
            case EmotionType.Fear: FearEmotionEffect.UseOnSelf(); break;
            case EmotionType.Embarassment: EmbarassmentEmotionEffect.UseOnSelf(); break;
            case EmotionType.Joy:
                if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Interaction", true);
                GameManager.Instance.DisplayLog("나 자신에게 사용했다. 기분이 무척 좋아졌다!");
                break;
            case EmotionType.Eat:
                if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Hit");
                GameManager.Instance.DisplayLog("자신의 팔을 꽉 깨물어 보았다. 아프다.");
                break;
            case EmotionType.Freeze:
                if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Hit");
                GameManager.Instance.DisplayLog("마법이 역류하여 내 몸이 얼어붙었습니다! (3초 기절)");
                GameManager.Instance.LockPlayer(3f);
                break;
        }
    }

    private InteractiveObject FindEnemyInScene()
    {
        InteractiveObject[] allObjs = Object.FindObjectsByType<InteractiveObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjs)
        {
            if (obj.objectType == ObjectType.Enemy && GameManager.Instance.tamedEnemy != obj) return obj;
        }
        return null;
    }

    private void PerformSuperPowerPush(InteractiveObject target)
    {
        Vector3 pushDirection = (target.transform.position - transform.position).normalized;
        pushDirection.x = Mathf.Round(pushDirection.x);
        pushDirection.y = Mathf.Round(pushDirection.y);

        target.transform.position += pushDirection * selectTile.gridSize;
        GameManager.Instance.DisplayLog("과일의 괴력을 발휘해 대상을 강하게 밀쳐버렸습니다!");
    }

    private void HandleTargetInteraction(EmotionType word, InteractiveObject target)
    {
        // 1. 꽁꽁 얼어붙은 상태면 마법 튕겨냄 (Hit 사운드)
        if (target.isFrozen && word != EmotionType.None)
        {
            if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Hit");
            GameManager.Instance.DisplayLog("대상이 꽁꽁 얼어붙어 있어 마법이 통하지 않습니다.");
            return;
        }

        // 2. 야생 적을 건드려서 죽게 되는 치명적 행동인지 체크 (Eat, Attract 실패 시)
        bool isDeathTrap = (word == EmotionType.Eat || word == EmotionType.Attract) && target.objectType == ObjectType.Enemy && GameManager.Instance.tamedEnemy != target;

        // 3. ★ 모든 상호작용 발생 시 무조건 사운드 1회 재생 (죽을 땐 Trap, 나머진 팀원들의 새 마법을 포함해 모두 Interaction)
        if (SoundManager.instance != null)
        {
            if (isDeathTrap) SoundManager.instance.PlaySFX("SFX_Trap");
            else SoundManager.instance.PlaySFX("SFX_Interaction");
        }

        // 4. 개별 감정(단어) 마법 효과 처리
        switch (word)
        {
            case EmotionType.Joy:
                if (target.objectType == ObjectType.Enemy) GameManager.Instance.TameEnemy(target);
                else if (target.objectType == ObjectType.Wall) GameManager.Instance.DisplayLog("벽에 예쁜 꽃이 피어났습니다! 기분 좋은 향기가 맴돕니다.");
                else if (target.objectType == ObjectType.Water) GameManager.Instance.DisplayLog("물이 영롱하게 반짝입니다! 보기만 해도 마음이 정화되는 기분입니다.");
                else if (target.objectType == ObjectType.Key) GameManager.Instance.DisplayLog("열쇠가 허공에서 기분 좋게 짤랑거리며 춤을 춥니다!");
                else if (target.objectType == ObjectType.Fruit) GameManager.Instance.DisplayLog("과일이 기쁨에 겨워 통통 튀어 오릅니다! 훨씬 먹음직스러워졌습니다.");
                else if (target.objectType == ObjectType.NPC) GameManager.Instance.DisplayLog("경비원이 기분이 좋은지 콧노래를 흥얼거립니다. (하지만 길은 비켜주지 않네요!)");
                break;

            case EmotionType.Eat:
                if (target.objectType == ObjectType.Fruit)
                {
                    GameManager.Instance.DisplayLog("신비한 과일을 먹었습니다! 몸에서 엄청난 괴력이 샘솟습니다!");
                    GameManager.Instance.isSuperPowered = true;
                    target.Vanish();
                }
                else if (target.objectType == ObjectType.Enemy)
                {
                    if (GameManager.Instance.tamedEnemy == target) GameManager.Instance.DisplayLog("나를 따르는 소중한 부하를 먹을 순 없습니다.");
                    else GameManager.Instance.TriggerGameOver("으악! 적을 먹으려다가 역으로 통째로 삼켜졌습니다!");
                }
                else
                {
                    if (target.objectType == ObjectType.Water) GameManager.Instance.DisplayLog("물을 벌컥벌컥 마셨습니다. 시원합니다!");
                    else if (target.objectType == ObjectType.Wall) GameManager.Instance.DisplayLog("벽을 핥아보았습니다. 흙먼지 맛이 납니다...");
                    else if (target.objectType == ObjectType.Key) GameManager.Instance.DisplayLog("열쇠를 깨물었다가 이빨이 부러질 뻔했습니다!");
                    else if (target.objectType == ObjectType.NPC) GameManager.Instance.DisplayLog("경비원이 기겁하며 뒤로 물러섭니다! 사람을 먹을 순 없습니다.");
                }
                break;

            case EmotionType.Freeze:
                if (target.objectType == ObjectType.Water)
                {
                    GameManager.Instance.DisplayLog("물이 꽁꽁 얼어붙어 위를 걸어갈 수 있게 되었습니다!");
                }
                else if (target.objectType == ObjectType.Enemy)
                {
                    GameManager.Instance.DisplayLog("적을 꽁꽁 얼려버렸습니다! 이제 움직이지 못합니다.");
                    if (GameManager.Instance.tamedEnemy == target) GameManager.Instance.tamedEnemy = null;
                }
                else
                {
                    if (target.objectType == ObjectType.Fruit) GameManager.Instance.DisplayLog("과일이 얼어붙어 시원한 아이스바가 되었습니다!");
                    else if (target.objectType == ObjectType.NPC) GameManager.Instance.DisplayLog("경비원이 덜덜 떨며 추워합니다. 불쌍하지만 길은 안 비켜주네요.");
                    else GameManager.Instance.DisplayLog("대상이 차갑게 얼어붙었습니다.");
                }
                target.ApplyFreeze();
                break;

            case EmotionType.Attract:
                if (target.objectType == ObjectType.Enemy && GameManager.Instance.tamedEnemy != target)
                {
                    if (SoundManager.instance != null) SoundManager.instance.PlaySFX("SFX_Trap");
                    GameManager.Instance.TriggerGameOver("끌려온 야생 적에게 잡아먹혔습니다!");
                }
                else
                {
                    // (기존의 당기기 이동 로직이 있다면 이 부분에 유지)
                }
                break;

            // --------------------------------------------------------
            // ★ 새로 추가된 팀원들의 감정 효과 (당황, 두려움)
            // --------------------------------------------------------
            case EmotionType.Embarassment: // (참고: 팀원분이 Enum에 설정한 당황 스펠링에 맞게 수정하세요. 예: Embarrassment 또는 Embarassment)
                EmbarassmentEmotionEffect.UseOnTarget(target);
                break;

            case EmotionType.Fear:
                FearEmotionEffect.UseOnTarget(target);
                break;
        }
    }
}
