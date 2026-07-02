using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance;

    public List<EmotionType> selectedEmotions = new List<EmotionType>();
    public int currentStage = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;  // SceneFlowManager 인스턴스 지정
            DontDestroyOnLoad(gameObject);  
        }
        else { Destroy(gameObject); }
    }

    public void GoToSelect() { selectedEmotions.Clear(); SceneManager.LoadScene("Main"); }    // 감정 선택 화면으로 이동
    public void LoadScene(string sceneName) { SceneManager.LoadScene(sceneName); }
    public void StartStage() { SceneManager.LoadScene("Map" + currentStage); } // 특정 스테이지로 이동
    public void ClearStage() { SceneManager.LoadScene("End"); }     // 스테이지 클리어
    public void RetryStage() { SceneManager.LoadScene("Map" + currentStage); }  // 스테이지 재시도
    public void GoToTitle() { SceneManager.LoadScene("Title"); }  // 시작 화면 이동
    public void QuitGame() { Application.Quit(); }  // 게임 종료
    public void SetSelectedEmotions(List<EmotionType> emotions) { selectedEmotions = new List<EmotionType>(emotions); } // 선택한 감정 목록
}
