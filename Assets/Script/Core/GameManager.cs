using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public enum ObjectType { Wall, Fruit, Exit, Key, Water, Enemy, NPC }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI 설정")]
    public GameObject logPanel;
    public TextMeshProUGUI logText;
    public TextMeshProUGUI currentWordText;
    public float logDisplayTime = 1.5f; // ★ 이벤트 창이 떠 있는 시간 (인스펙터에서 수정 가능)

    [Header("가져온 단어 목록")]
    public List<EmotionType> selectedEmotions = new List<EmotionType>();
    public EmotionType currentEmotion = EmotionType.None;

    [Header("플레이어 상태")]
    public bool isSuperPowered = false;
    public bool hasKey = false;

    [Header("부하 시스템")]
    public InteractiveObject tamedEnemy = null;
    private Transform playerTransform;
    private Vector3 lastPlayerGridPosition;
    private bool isPlayerLocked = false;
    private Coroutine logTimerCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (logPanel != null) logPanel.SetActive(false);
        if (logText != null)
        {
            logText.text = "";
            logText.gameObject.SetActive(false);
        }

        var playerMovement = Object.FindFirstObjectByType<PlayerGridMovement>();
        if (playerMovement != null)
        {
            playerTransform = playerMovement.transform;
            lastPlayerGridPosition = playerTransform.position;
        }

        if (SceneFlowManager.Instance != null && SceneFlowManager.Instance.selectedEmotions.Count > 0)
        {
            selectedEmotions = new List<EmotionType>(SceneFlowManager.Instance.selectedEmotions);
            currentEmotion = selectedEmotions[0];
        }
        else
        {
            selectedEmotions = new List<EmotionType>() { EmotionType.Joy, EmotionType.Eat, EmotionType.Attract };
            currentEmotion = selectedEmotions[0];
        }

        UpdateCurrentWordUI();
        DisplayLog("맵에 입장했습니다. [1, 2, 3]: 단어 변경 / [R]: 리셋");
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame && selectedEmotions.Count > 0) ChangeActiveEmotion(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame && selectedEmotions.Count > 1) ChangeActiveEmotion(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame && selectedEmotions.Count > 2) ChangeActiveEmotion(2);

        if (Keyboard.current.rKey.wasPressedThisFrame) ResetToMenuScene();

        HandleFollowerMovement();
    }

    private void HandleFollowerMovement()
    {
        if (tamedEnemy == null || playerTransform == null) return;

        if (Vector3.Distance(playerTransform.position, lastPlayerGridPosition) >= 0.8f)
        {
            tamedEnemy.transform.position = lastPlayerGridPosition;
            lastPlayerGridPosition = playerTransform.position;
        }
    }

    public void TameEnemy(InteractiveObject enemy)
    {
        tamedEnemy = enemy;
        lastPlayerGridPosition = playerTransform.position;

        // ★ [통과 가능 처리] 동료가 된 적의 물리 충돌을 Trigger로 바꿔 길막 방지
        Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
        if (enemyCollider != null)
        {
            enemyCollider.isTrigger = true;
        }

        DisplayLog("적이 기쁨에 취해 당신의 부하가 되었습니다!\n이제 길을 막지 않고 졸졸 따라다닙니다.");
    }

    public void ClearStage()
    {
        StartCoroutine(ClearRoutine());
    }

    private IEnumerator ClearRoutine()
    {
        SetPlayerInputLock(true);
        DisplayLog("<color=green>🎉 축하합니다! 열쇠로 탈출구를 열고 스테이지를 클리어했습니다!</color>");
        yield return new WaitForSeconds(3.5f);
        SceneFlowManager.Instance.ClearStage();
    }

    public void DisplayLog(string message)
    {
        if (logText != null)
        {
            logText.gameObject.SetActive(true);
            logText.text = message;
        }
        if (logPanel != null) logPanel.SetActive(true);

        if (logTimerCoroutine != null) StopCoroutine(logTimerCoroutine);
        // ★ 3초에서 변수(기본 1.5초)로 단축
        logTimerCoroutine = StartCoroutine(LogAutoCloseRoutine(logDisplayTime));
    }

    private IEnumerator LogAutoCloseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseLog();
    }

    private void CloseLog()
    {
        if (logPanel != null) logPanel.SetActive(false);
        if (logText != null) logText.gameObject.SetActive(false);
    }

    public void SetPlayerInputLock(bool lockState)
    {
        var movement = Object.FindFirstObjectByType<PlayerGridMovement>();
        var selector = Object.FindFirstObjectByType<SelectTile>();

        if (movement != null)
        {
            movement.enabled = !lockState;
            if (lockState)
            {
                var rb = movement.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }
        if (selector != null) selector.enabled = !lockState;
    }

    public void ResetToMenuScene()
    {
        SceneFlowManager.Instance.GoToSelect();
    }

    private void ChangeActiveEmotion(int index)
    {
        currentEmotion = selectedEmotions[index];
        UpdateCurrentWordUI();
        DisplayLog($"장착 감정 변경 ➡️ [{currentEmotion}]");
    }

    private void UpdateCurrentWordUI()
    {
        if (currentWordText != null)
        {
            currentWordText.text = $"현재 감정: <color=yellow>[{currentEmotion}]</color>\n";
            currentWordText.text += $"보유한 감정: ";
            for (int i = 0; i < selectedEmotions.Count; i++)
            {
                if (selectedEmotions[i] == currentEmotion) currentWordText.text += $"<u><b>{selectedEmotions[i]}</b></u> ";
                else currentWordText.text += $"{selectedEmotions[i]} ";
            }
        }
    }

    public void LockPlayer(float duration)
    {
        if (isPlayerLocked) return;
        StartCoroutine(LockPlayerRoutine(duration));
    }

    private IEnumerator LockPlayerRoutine(float duration)
    {
        isPlayerLocked = true;
        SetPlayerInputLock(true);
        yield return new WaitForSeconds(duration);
        SetPlayerInputLock(false);
        isPlayerLocked = false;
    }

    public void TriggerGameOver(string message)
    {
        StartCoroutine(GameOverRoutine(message));
    }

    private IEnumerator GameOverRoutine(string message)
    {
        SetPlayerInputLock(true);
        if (logTimerCoroutine != null) StopCoroutine(logTimerCoroutine);

        DisplayLog($"<color=red>{message}</color>\n3초 후 단어 선택창으로 이동합니다...");
        yield return new WaitForSeconds(3f);
        SceneFlowManager.Instance.GoToSelect();
    }
}
