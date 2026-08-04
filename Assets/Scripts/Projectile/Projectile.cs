using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifeTime = 5f;

    private Vector3 direction;
    private Transform homingTarget;
    private bool isHoming;
    private float homingTurnSpeed;
    private float homingDelay;
    private float homingTimer;
    private float targetYOffset;
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

        Vector3 targetPosition = target.position;

        targetPosition.y = transform.position.y;

        direction =
            (targetPosition - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            // Trục X của projectile hướng về phía bay.
            transform.right = direction;
        }
    }


    public void InitializeHoming(
        Transform target,
        float projectileDamage,
        float turnSpeed,
        float delayBeforeFullHoming,
        float aimHeightOffset = 1f)
    {
        damage = projectileDamage;
        effectType = ProjectileEffectType.None;
        effectDuration = 0f;
        effectValue = 0f;
        timer = lifeTime;

        homingTarget = target;
        isHoming = target != null;
        homingTurnSpeed = Mathf.Max(0f, turnSpeed);
        homingDelay = Mathf.Max(0f, delayBeforeFullHoming);
        homingTimer = 0f;
        targetYOffset = aimHeightOffset;

        if (target != null)
        {
            Vector3 targetPosition =
                target.position + Vector3.up * targetYOffset;

            direction =
                (targetPosition - transform.position).normalized;
        }
        else
        {
            direction = transform.forward;
        }

        if (direction != Vector3.zero)
        {
            transform.right = direction;
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
        direction = newDirection.normalized;

        damage = projectileDamage;

        effectType = newEffectType;
        effectDuration = newEffectDuration;
        effectValue = newEffectValue;

        timer = lifeTime;

        if (direction != Vector3.zero)
        {
            // Trục X của projectile hướng về phía bay.
            transform.right = direction;
        }
    }

    private void Update()
    {
        if (isHoming && homingTarget != null)
        {
            homingTimer += Time.deltaTime;

            Vector3 targetPosition =
                homingTarget.position +
                Vector3.up * targetYOffset;

            Vector3 desiredDirection =
                (targetPosition - transform.position).normalized;

            if (desiredDirection != Vector3.zero)
            {
                // Lúc mới bắn chỉ đổi hướng nhẹ để viên đạn rời tay Boss tự nhiên.
                // Sau homingDelay, viên đạn quay nhanh hơn và bám theo Player.
                float currentTurnSpeed =
                    homingTimer < homingDelay
                        ? homingTurnSpeed * 0.25f
                        : homingTurnSpeed;

                direction = Vector3.RotateTowards(
                    direction,
                    desiredDirection,
                    currentTurnSpeed * Mathf.Deg2Rad * Time.deltaTime,
                    0f).normalized;

                transform.right = direction;
            }
        }

        transform.position +=
            direction * speed * Time.deltaTime;

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

        //Debug.LogWarning(player.IsInvincible);
        if (player != null &&
            player.IsInvincible)
        {
            ReturnToPool();
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
        homingTarget = null;
        isHoming = false;
        homingTurnSpeed = 0f;
        homingDelay = 0f;
        homingTimer = 0f;
        targetYOffset = 0f;
        damage = 0f;
        timer = 0f;
        owner = null;

        effectType =
            ProjectileEffectType.None;

        effectDuration = 0f;
        effectValue = 0f;
    }
}