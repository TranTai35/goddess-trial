using System.Collections;
using UnityEngine;

public class MeleeEnemy : EnemyController
{
    [Header("Melee")]
    public float damage = 10f;

    

    protected override IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.5f);
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // FEEDBACK KHI BỊ ĐÁNH:
                // 1. Rung camera nhẹ (0.03 magnitude cho nhẹ nhàng)
                // 2. Chớp đỏ 0.1 giây
                // KHÔNG DÙNG HIT STOP (để duration = 0, timeScale = 1)
                FeedbackManager.Instance.PlayHitFeedback(0f, 1f, 0.15f, 0.03f);
                FeedbackManager.Instance.PlayDamageFlash(0.1f);

                PlayerStats stats = hit.GetComponent<PlayerStats>();
                if (stats != null) stats.TakeDamage(damage);
                break;
            }
        }
        yield return new WaitForSeconds(0.5f);
    }
}