using UnityEngine;

public class EnemyTouchAttack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ★ [핵심] 만약 내가 현재 GameManager에 등록된 '동료(tamedEnemy)'라면 
            // 플레이어와 부딪히거나 겹쳐도 절대 죽이지 않고 그냥 통과시킵니다!
            InteractiveObject interactiveObject = GetComponent<InteractiveObject>();
            if (interactiveObject != null && interactiveObject.isHarmless)
            {
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.tamedEnemy == interactiveObject)
            {
                return; // 안전하게 통과! 아무 일도 일어나지 않음
            }

            // 동료가 아닌 야생 적인 상태에서 플레이어와 몸으로 부딪히면 게임오버
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver("무서운 적과 정면으로 부딪혔습니다! 적에게 잡아먹혔습니다!");
            }
        }
    }

    // 일반 물리 충돌(IsTrigger가 아닐 때)을 사용하는 경우를 위한 대비 코드
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            InteractiveObject interactiveObject = GetComponent<InteractiveObject>();
            if (interactiveObject != null && interactiveObject.isHarmless)
            {
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.tamedEnemy == interactiveObject)
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver("무서운 적과 정면으로 부딪혔습니다! 적에게 잡아먹혔습니다!");
            }
        }
    }
}
