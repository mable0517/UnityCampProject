using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public enum WordType { None, Attract, Eat, Freeze, Joy }
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
    public List<WordType> myDeck = new List<WordType>();
    public WordType currentWord = WordType.None;

    [Header("플레이어 상태")]
    public bool isSuperPowered = false;
    public bool hasKey = false;

    [Header("부하 시스템")]
    public InteractiveObject tamedEnemy = null;
    private Transform playerTransform;
    private Vector3 lastPlayerGridPosition;
    private bool isPlayerLocked = false;
    private Coroutine logTimerCoroutine;

    [Header("씬 관리 설정")]
    [SerializeField] private string menuSceneName = "WordSelectMenuScene";

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

        if (WordDataCarrier.Instance != null && WordDataCarrier.Instance.selectedWords.Count > 0)
        {
            myDeck = new List<WordType>(WordDataCarrier.Instance.selectedWords);
            currentWord = myDeck[0];
        }
        else
        {
            myDeck = new List<WordType>() { WordType.Joy, WordType.Eat, WordType.Attract };
            currentWord = myDeck[0];
        }

        UpdateCurrentWordUI();
        DisplayLog("맵에 입장했습니다. [1, 2, 3]: 단어 변경 / [R]: 리셋");
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame && myDeck.Count > 0) ChangeActiveWord(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame && myDeck.Count > 1) ChangeActiveWord(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame && myDeck.Count > 2) ChangeActiveWord(2);

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
        SceneManager.LoadScene(menuSceneName);
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
        SceneManager.LoadScene(menuSceneName);
    }

    private void ChangeActiveWord(int index)
    {
        currentWord = myDeck[index];
        UpdateCurrentWordUI();
        DisplayLog($"장착 단어 변경 ➡️ [{currentWord}]");
    }

    private void UpdateCurrentWordUI()
    {
        if (currentWordText != null)
        {
            currentWordText.text = $"현재 단어: <color=yellow>[{currentWord}]</color>\n";
            currentWordText.text += $"보유한 단어: ";
            for (int i = 0; i < myDeck.Count; i++)
            {
                if (myDeck[i] == currentWord) currentWordText.text += $"<u><b>{myDeck[i]}</b></u> ";
                else currentWordText.text += $"{myDeck[i]} ";
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
        SceneManager.LoadScene(menuSceneName);
    }
}