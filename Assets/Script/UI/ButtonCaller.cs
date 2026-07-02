using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonCaller : MonoBehaviour
{
    public void GoToSelect()
    {
        if (SceneFlowManager.Instance != null) SceneFlowManager.Instance.GoToSelect();
        else SceneManager.LoadScene("Main");
    } // 게임 시작 버튼

    public void StartStage()
    {
        if (SceneFlowManager.Instance != null) SceneFlowManager.Instance.StartStage();
        else SceneManager.LoadScene("Map1");
    } // 스테이지 시작 버튼

    public void RetryStage()
    {
        if (SceneFlowManager.Instance != null) SceneFlowManager.Instance.RetryStage();
        else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    } // 재시작 버튼

    public void GoToTitle()
    {
        if (SceneFlowManager.Instance != null) SceneFlowManager.Instance.GoToTitle();
        else SceneManager.LoadScene("Title");
    } // 시작 화면 버튼 

    public void QuitGame()
    {
        if (SceneFlowManager.Instance != null) SceneFlowManager.Instance.QuitGame();
        else Application.Quit();
    } // 게임 종료 버튼

}
