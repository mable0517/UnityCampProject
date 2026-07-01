using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private InteractiveObject interactiveData;

    void Start()
    {
        interactiveData = GetComponent<InteractiveObject>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 닿은 오브젝트가 플레이어인지 확인 (플레이어 오브젝트에 "Player" 태그가 설정되어 있어야 합니다)
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null && !GameManager.Instance.hasKey)
            {
                GameManager.Instance.hasKey = true;
                GameManager.Instance.DisplayLog("찰칵! 바닥에 놓인 열쇠를 주웠습니다.");

                // 기존 InteractiveObject의 소멸 로직 호출
                if (interactiveData != null)
                {
                    interactiveData.Vanish();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}