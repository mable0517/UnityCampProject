// [ExitDoor.cs] 노란 네모(탈출구) 오브젝트에 부착
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerGridMovement>() != null)
        {
            if (GameManager.Instance.hasKey)
            {
                GameManager.Instance.DisplayLog("<color=green>★ 축하합니다! 열쇠로 문을 열고 탈출했습니다! ★</color>");
            }
            else
            {
                GameManager.Instance.DisplayLog("문이 잠겨 있습니다. 노란 오각형 모양의 열쇠가 필요합니다.");
            }
        }
    }
}