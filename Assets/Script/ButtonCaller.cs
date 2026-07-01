using UnityEngine;

public class ButtonCaller : MonoBehaviour
{
    public void GoToSelect() { GameManager.Instance.GoToSelect(); } // 게임 시작 버튼
    public void StartStage() { GameManager.Instance.StartStage(); } // 스테이지 시작 버튼
    public void RetryStage() { GameManager.Instance.RetryStage(); } // 재시작 버튼
    public void GoToTitle() { GameManager.Instance.GoToTitle(); } // 시작 화면 버튼 
    public void QuitGame() { GameManager.Instance.QuitGame(); } // 게임 종료 버튼

}
