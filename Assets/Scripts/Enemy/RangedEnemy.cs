using System.Collections;
using UnityEngine;

public class RangedEnemy : EnemyController
{
    [Header("Projectile")]
    public Projectile projectilePrefab;

    public Transform firePoint;

    public float projectileDamage = 10f;

    [Header("Projectile Effect")]
    public ProjectileEffectType effectType = ProjectileEffectType.None;

    public float effectDuration = 2f;

    [Range(0f, 1f)]
    public float slowPercent = 0.5f;

    public float burnDamagePerSecond = 5f;

    protected override IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.5f);

        if (projectilePrefab != null &&
            firePoint != null &&
            player != null)
        {
            GameObject obj =
                PoolManager.Instance.GetObject(
                    projectilePrefab.gameObject);

            Projectile projectile =
                obj.GetComponent<Projectile>();

            projectile.transform.position =
                firePoint.position;

            projectile.transform.rotation =
                firePoint.rotation;

            float effectValue = 0f;

            if (effectType ==
                ProjectileEffectType.Slow)
            {
                effectValue = slowPercent;
            }
            else if (effectType ==
                     ProjectileEffectType.Burn)
            {
                effectValue =
                    burnDamagePerSecond;
            }

            projectile.Initialize(
                player,
                projectileDamage,
                effectType,
                effectDuration,
                effectValue);

            projectile.SetOwner(gameObject);
        }

        yield return new WaitForSeconds(0.5f);
    }
}