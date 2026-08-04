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

    // Homing
    private bool isHoming;
    private Transform homingTarget;
    private float homingTurnSpeed;
    private float homingDelay;
    private float homingTimer;
    private float homingTargetHeight;

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
        ResetHoming();

        damage = projectileDamage;

        effectType = newEffectType;
        effectDuration = newEffectDuration;
        effectValue = newEffectValue;

        timer = lifeTime;

        if (target == null)
        {
            direction = transform.forward;
            return;
        }

        Vector3 targetPosition = target.position;

        // Projectile thường bay ngang.
        targetPosition.y = transform.position.y;

        direction =
            (targetPosition - transform.position).normalized;

        RotateProjectile(direction);
    }

    public void InitializeDirection(
        Vector3 newDirection,
        float projectileDamage,
        ProjectileEffectType newEffectType =
            ProjectileEffectType.None,
        float newEffectDuration = 0f,
        float newEffectValue = 0f)
    {
        ResetHoming();

        direction = newDirection.normalized;
        damage = projectileDamage;

        effectType = newEffectType;
        effectDuration = newEffectDuration;
        effectValue = newEffectValue;

        timer = lifeTime;

        RotateProjectile(direction);
    }

    public void InitializeHoming(
        Transform target,
        float projectileDamage,
        float turnSpeed,
        float delay,
        float targetHeight,
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

        isHoming = true;
        homingTarget = target;
        homingTurnSpeed = Mathf.Max(0f, turnSpeed);
        homingDelay = Mathf.Max(0f, delay);
        homingTimer = 0f;
        homingTargetHeight = targetHeight;

        if (target != null)
        {
            Vector3 targetPosition =
                target.position +
                Vector3.up * homingTargetHeight;

            direction =
                (targetPosition - transform.position).normalized;
        }
        else
        {
            direction = transform.forward.normalized;
        }

        RotateProjectile(direction);
    }

    private void Update()
    {
        UpdateHomingDirection();

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

    private void UpdateHomingDirection()
    {
        if (!isHoming || homingTarget == null)
            return;

        homingTimer += Time.deltaTime;

        Vector3 targetPosition =
            homingTarget.position +
            Vector3.up * homingTargetHeight;

        Vector3 desiredDirection =
            (targetPosition - transform.position).normalized;

        if (desiredDirection == Vector3.zero)
            return;

        // Trong thời gian homingDelay, projectile vẫn tiếp tục bay
        // theo hướng khởi đầu.
        if (homingTimer < homingDelay)
            return;

        float maxRadians =
            homingTurnSpeed *
            Mathf.Deg2Rad *
            Time.deltaTime;

        direction =
            Vector3.RotateTowards(
                direction,
                desiredDirection,
                maxRadians,
                0f
            ).normalized;

        RotateProjectile(direction);
    }

    private void RotateProjectile(Vector3 newDirection)
    {
        if (newDirection == Vector3.zero)
            return;

        // Projectile hiện tại dùng trục X làm hướng bay.
        transform.right = newDirection;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner != null)
        {
            // Bỏ qua toàn bộ collider thuộc object chủ projectile.
            if (other.gameObject == owner ||
                other.transform.IsChildOf(owner.transform))
            {
                return;
            }
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
            other.GetComponentInParent<PlayerController>();

        if (player != null &&
            player.IsInvincible)
        {
            ReturnToPool();
            return;
        }

        PlayerStats stats =
            other.GetComponentInParent<PlayerStats>();

        if (stats != null)
        {
            if (FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayHitFeedback(
                    0f,
                    1f,
                    0.15f,
                    0.03f
                );

                FeedbackManager.Instance.PlayDamageFlash(
                    0.1f
                );
            }

            stats.TakeDamage(damage);
        }

        PlayerStatusEffects statusEffects =
            other.GetComponentInParent<PlayerStatusEffects>();

        if (statusEffects != null &&
            effectType != ProjectileEffectType.None)
        {
            statusEffects.ApplyEffect(
                effectType,
                effectDuration,
                effectValue
            );
        }

        ReturnToPool();
    }

    private void HitEnemy(Collider other)
    {
        EnemyController enemy =
            other.GetComponentInParent<EnemyController>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            ReturnToPool();
            return;
        }

        BossController boss =
            other.GetComponentInParent<BossController>();

        if (boss != null)
        {
            boss.TakeDamage(damage);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnObject(
                gameObject
            );
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void ResetHoming()
    {
        isHoming = false;
        homingTarget = null;
        homingTurnSpeed = 0f;
        homingDelay = 0f;
        homingTimer = 0f;
        homingTargetHeight = 0f;
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

        ResetHoming();
    }
}