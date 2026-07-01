using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WordSelectMenu : MonoBehaviour
{
    [Header("선택 UI 세팅")]
    public TextMeshProUGUI statusText;   // 상단 현황 텍스트
    public Button startGameButton;       // 게임 시작 버튼

    [Header("단어 버튼 개별 연결")]
    public Button joyButton;             // 기쁨(Joy) 버튼
    public Button eatButton;             // 먹다(Eat) 버튼
    public Button freezeButton;          // 얼리다(Freeze) 버튼
    public Button attractButton;          // 끌림(Attract) 버튼
    public Button fearButton;             // 두려움(Fear) 버튼

    [Header("버튼 시각 효과 설정")]
    public Color selectedColor = new Color(0.4f, 1f, 0.4f, 1f); // ★ 선택되었을 때의 색상 (기본값: 연한 초록색)
    public Color normalColor = Color.white;                    // ★ 선택 안 되었을 때의 기본 색상 (흰색)

    private List<EmotionType> tempSelected = new List<EmotionType>();
    private const int MaxSelectCount = 3;

    void Start()
    {
        if (startGameButton != null) startGameButton.interactable = false;
        UpdateUI();
    }

    public void ClickWordButton(string wordStr)
    {
        wordStr = wordStr.Trim();
        EmotionType clickedEmotion = EmotionType.None;

        if (wordStr == "기쁨" || wordStr.Equals("Joy", System.StringComparison.OrdinalIgnoreCase)) clickedEmotion = EmotionType.Joy;
        else if (wordStr == "먹다" || wordStr.Equals("Eat", System.StringComparison.OrdinalIgnoreCase)) clickedEmotion = EmotionType.Eat;
        else if (wordStr == "얼리다" || wordStr.Equals("Freeze", System.StringComparison.OrdinalIgnoreCase)) clickedEmotion = EmotionType.Freeze;
        else if (wordStr == "끌림" || wordStr.Equals("Attract", System.StringComparison.OrdinalIgnoreCase)) clickedEmotion = EmotionType.Attract;
        else if (wordStr == "두려움" || wordStr.Equals("Fear", System.StringComparison.OrdinalIgnoreCase)) clickedEmotion = EmotionType.Fear;

        if (clickedEmotion != EmotionType.None)
        {
            if (tempSelected.Contains(clickedEmotion))
            {
                tempSelected.Remove(clickedEmotion);
            }
            else if (tempSelected.Count < MaxSelectCount)
            {
                tempSelected.Add(clickedEmotion);
            }

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // 1. 상단 텍스트 가이드 업데이트
        if (statusText != null)
        {
            statusText.text = $"선택된 단어: ";
            for (int i = 0; i < tempSelected.Count; i++)
            {
                statusText.text += $"[{tempSelected[i]}] ";
            }
            statusText.text += $"\n({tempSelected.Count} / {MaxSelectCount})";
        }

        // 2. ★ [핵심] 단어별 버튼의 색상을 상태에 따라 실시간 변경
        UpdateButtonVisual(joyButton, EmotionType.Joy);
        UpdateButtonVisual(eatButton, EmotionType.Eat);
        UpdateButtonVisual(freezeButton, EmotionType.Freeze);
        UpdateButtonVisual(attractButton, EmotionType.Attract);
        UpdateButtonVisual(fearButton, EmotionType.Fear);

        // 3. 3개를 정확히 골랐을 때만 시작 버튼 활성화
        if (startGameButton != null)
        {
            startGameButton.interactable = (tempSelected.Count == MaxSelectCount);
        }
    }

    // 버튼의 선택 여부를 판별해 색상을 입혀주는 헬퍼 함수
    private void UpdateButtonVisual(Button btn, EmotionType emotion)
    {
        if (btn == null) return;

        // 버튼에 달린 Image 컴포넌트의 색상을 변경합니다.
        if (btn.image != null)
        {
            if (tempSelected.Contains(emotion))
            {
                btn.image.color = selectedColor; // 고른 단어는 하이라이트 색상으로!
            }
            else
            {
                btn.image.color = normalColor;   // 안 고른 단어는 기본 흰색으로!
            }
        }
    }

    public void LoadInGameScene(string sceneName)
    {
        if (tempSelected.Count == MaxSelectCount)
        {
            SceneFlowManager.Instance.SetSelectedEmotions(tempSelected);
            SceneFlowManager.Instance.LoadScene(sceneName);
        }
    }
}
