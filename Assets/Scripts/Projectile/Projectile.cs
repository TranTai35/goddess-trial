using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifeTime = 5f;

    private Vector3 direction;
    private float damage;
    private float timer;
    private GameObject owner;

    private ProjectileEffectType effectType;
    private float effectDuration;
    private float effectValue;

    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }

    public void Initialize(
        Transform target,
        float projectileDamage,
        ProjectileEffectType newEffectType =
            ProjectileEffectType.None,
        float newEffectDuration = 0f,
        float newEffectValue = 0f)
    {
        damage = projectileDamage;

        effectType = newEffectType;
        effectDuration = newEffectDuration;
        effectValue = newEffectValue;

        timer = lifeTime;

        Vector3 targetPosition =
            target.position;

        targetPosition.y =
            transform.position.y;

        direction =
            (targetPosition -
             transform.position).normalized;

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    public void InitializeDirection(
        Vector3 newDirection,
        float projectileDamage,
        ProjectileEffectType newEffectType =
            ProjectileEffectType.None,
        float newEffectDuration = 0f,
        float newEffectValue = 0f)
    {
        direction =
            newDirection.normalized;

        damage =
            projectileDamage;

        effectType =
            newEffectType;

        effectDuration =
            newEffectDuration;

        effectValue =
            newEffectValue;

        timer =
            lifeTime;

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    private void Update()
    {
        transform.position +=
            direction *
            speed *
            Time.deltaTime;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            HitPlayer(other);
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            HitEnemy(other);
            return;
        }

        if (other.isTrigger)
        {
            return;
        }

        ReturnToPool();
    }

    private void HitPlayer(Collider other)
    {
        PlayerController player =
            other.GetComponent<PlayerController>();

        if (player != null &&
            player.IsInvincible)
        {
            return;
        }

        PlayerStats stats =
            other.GetComponent<PlayerStats>();

        if (stats != null)
        {
            if (FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance
                    .PlayHitFeedback(
                        0f,
                        1f,
                        0.15f,
                        0.03f);

                FeedbackManager.Instance
                    .PlayDamageFlash(0.1f);
            }

            stats.TakeDamage(damage);
        }

        PlayerStatusEffects statusEffects =
            other.GetComponent<PlayerStatusEffects>();

        if (statusEffects != null &&
            effectType != ProjectileEffectType.None)
        {
            statusEffects.ApplyEffect(
                effectType,
                effectDuration,
                effectValue);
        }

        ReturnToPool();
    }

    private void HitEnemy(Collider other)
    {
        EnemyController enemy =
            other.GetComponent<EnemyController>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        BossController boss =
            other.GetComponent<BossController>();

        if (boss != null)
        {
            boss.TakeDamage(damage);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnObject(
                gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        direction = Vector3.zero;
        damage = 0f;
        timer = 0f;
        owner = null;

        effectType =
            ProjectileEffectType.None;

        effectDuration = 0f;
        effectValue = 0f;
    }
}