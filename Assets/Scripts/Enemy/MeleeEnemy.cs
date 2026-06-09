using System.Collections;
using UnityEngine;

public class MeleeEnemy : BaseEnemy
{
    public float damage = 10;

    protected override IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.5f);

        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                attackRange);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            PlayerStats stats =
                hit.GetComponent<PlayerStats>();

            if (stats != null)
            {
                stats.TakeDamage(damage);
            }

            break;
        }

        yield return new WaitForSeconds(0.5f);
    }
}