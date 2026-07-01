using UnityEngine;

public class ButtonCaller : MonoBehaviour
{
    public void GoToSelect() { SceneFlowManager.Instance.GoToSelect(); } // 게임 시작 버튼
    public void StartStage() { SceneFlowManager.Instance.StartStage(); } // 스테이지 시작 버튼
    public void RetryStage() { SceneFlowManager.Instance.RetryStage(); } // 재시작 버튼
    public void GoToTitle() { SceneFlowManager.Instance.GoToTitle(); } // 시작 화면 버튼 
    public void QuitGame() { SceneFlowManager.Instance.QuitGame(); } // 게임 종료 버튼

}
