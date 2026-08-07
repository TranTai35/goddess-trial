using System.Collections;
using UnityEngine;

public class DamageAuraSpell : SpellBase
{
    [Header("Aura Settings")]
    [Min(0f)]
    public float duration = 5f;

    public GameObject auraVFX;

    [Header("Damage Settings")]
    public int damagePerSecond = 5;

    [Min(0f)]
    public float damageRadius = 4f;

    public LayerMask enemyLayer;

    private Coroutine activeRoutine;
    private GameObject activeVFX;

    public override void Cast(
        PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        StartCooldown();

        /*
         * PHÁT SFX NGAY KHI NHẤN Q
         * và spell thật sự được cast.
         */
        PlayCastSFX(
            player.transform.position
        );

        /*
         * Ngăn nhiều aura tồn tại cùng lúc.
         */
        if (activeRoutine != null)
        {
            player.StopCoroutine(
                activeRoutine
            );

            activeRoutine = null;
        }

        RemoveAuraVFX();

        activeRoutine =
            player.StartCoroutine(
                AuraRoutine(player)
            );
    }

    private IEnumerator AuraRoutine(
        PlayerController player)
    {
        Debug.Log(
            "Damage Aura ĐÃ BẬT!"
        );

        if (auraVFX != null)
        {
            Vector3 spawnPosition =
                player.transform.position +
                Vector3.up;

            activeVFX = Instantiate(
                auraVFX,
                spawnPosition,
                Quaternion.identity,
                player.transform
            );
        }

        float elapsed = 0f;

        const float tickInterval =
            1f;

        while (elapsed < duration)
        {
            Collider[] hitColliders =
                Physics.OverlapSphere(
                    player.transform.position,
                    damageRadius,
                    enemyLayer,
                    QueryTriggerInteraction.Ignore
                );

            foreach (
                Collider col
                in hitColliders)
            {
                EnemyController enemy =
                    col.GetComponentInParent<
                        EnemyController>();

                if (enemy != null)
                {
                    enemy.TakeDamage(
                        damagePerSecond
                    );
                }
            }

            float waitTime =
                Mathf.Min(
                    tickInterval,
                    duration - elapsed
                );

            yield return
                new WaitForSeconds(
                    waitTime
                );

            elapsed +=
                waitTime;
        }

        Debug.Log(
            "Damage Aura ĐÃ HẾT THỜI GIAN!"
        );

        RemoveAuraVFX();

        activeRoutine =
            null;
    }

    private void RemoveAuraVFX()
    {
        if (activeVFX == null)
        {
            return;
        }

        Destroy(activeVFX);

        activeVFX = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            damageRadius
        );
    }
}