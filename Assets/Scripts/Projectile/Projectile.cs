using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifeTime = 5f;

    [Header("Impact VFX")]
    [Tooltip("VFX phát khi projectile va chạm hoặc hết lifetime.")]
    [SerializeField] private GameObject impactVFX;

    [Tooltip("Thời gian tồn tại của VFX impact trước khi Destroy.")]
    [Min(0f)]
    [SerializeField] private float impactVFXLifeTime = 2f;

    [Tooltip("Offset vị trí VFX so với điểm va chạm.")]
    [SerializeField]
    private Vector3 impactVFXPositionOffset =
        Vector3.zero;

    [Tooltip("Offset góc xoay VFX. Dùng nếu prefab VFX bị xoay sai hướng.")]
    [SerializeField]
    private Vector3 impactVFXRotationOffset =
        Vector3.zero;

    [Tooltip("Nếu bật, VFX sẽ quay theo hướng bề mặt va chạm.")]
    [SerializeField] private bool rotateImpactVFX = true;

    [Header("Impact SFX")]
    [Tooltip("Âm thanh phát khi projectile va chạm hoặc hết lifetime.")]
    [SerializeField] private AudioClip impactSFX;

    [Range(0f, 1f)]
    [SerializeField] private float impactSFXVolume = 1f;

    // =========================================================
    // RUNTIME DATA
    // =========================================================

    private Vector3 direction;
    private float damage;
    private float timer;
    private GameObject owner;

    private ProjectileEffectType effectType;
    private float effectDuration;
    private float effectValue;

    /*
     * Tránh trường hợp projectile chạm nhiều collider
     * trong cùng một frame và Impact() bị gọi nhiều lần.
     */
    private bool hasImpacted;

    // =========================================================
    // HOMING
    // =========================================================

    private bool isHoming;
    private Transform homingTarget;
    private float homingTurnSpeed;
    private float homingDelay;
    private float homingTimer;
    private float homingTargetHeight;

    // =========================================================
    // POOL RESET
    // =========================================================

    private void OnEnable()
    {
        hasImpacted = false;
    }

    // =========================================================
    // OWNER
    // =========================================================

    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }

    // =========================================================
    // CHECK ENEMY PROJECTILE
    // =========================================================

    private bool IsOwnedByEnemy()
    {
        if (owner == null)
            return false;

        /*
         * Projectile của RangedEnemy được SetOwner(gameObject),
         * nên object owner sẽ có EnemyController.
         *
         * GetComponentInParent giúp hoạt động ngay cả khi
         * owner là child object.
         */
        return owner.GetComponentInParent<EnemyController>() != null;
    }

    // =========================================================
    // INITIALIZE NORMAL
    // =========================================================

    public void Initialize(
        Transform target,
        float projectileDamage,
        ProjectileEffectType newEffectType =
            ProjectileEffectType.None,
        float newEffectDuration = 0f,
        float newEffectValue = 0f)
    {
        ResetHoming();

        hasImpacted = false;

        damage = projectileDamage;

        effectType = newEffectType;
        effectDuration = newEffectDuration;
        effectValue = newEffectValue;

        timer = lifeTime;

        if (target == null)
        {
            direction =
                transform.right.normalized;

            if (direction == Vector3.zero)
            {
                direction = Vector3.forward;
            }

            return;
        }

        Vector3 targetPosition =
            target.position;

        targetPosition.y =
            transform.position.y;

        direction =
            (targetPosition -
             transform.position).normalized;

        RotateProjectile(direction);
    }

    // =========================================================
    // INITIALIZE DIRECTION
    // =========================================================

    public void InitializeDirection(
        Vector3 newDirection,
        float projectileDamage,
        ProjectileEffectType newEffectType =
            ProjectileEffectType.None,
        float newEffectDuration = 0f,
        float newEffectValue = 0f)
    {
        ResetHoming();

        hasImpacted = false;

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

        RotateProjectile(direction);
    }

    // =========================================================
    // INITIALIZE HOMING
    // =========================================================

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
        hasImpacted = false;

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

        isHoming =
            true;

        homingTarget =
            target;

        homingTurnSpeed =
            Mathf.Max(
                0f,
                turnSpeed
            );

        homingDelay =
            Mathf.Max(
                0f,
                delay
            );

        homingTimer =
            0f;

        homingTargetHeight =
            targetHeight;

        if (target != null)
        {
            Vector3 targetPosition =
                target.position +
                Vector3.up *
                homingTargetHeight;

            direction =
                (targetPosition -
                 transform.position).normalized;
        }
        else
        {
            direction =
                transform.right.normalized;

            if (direction == Vector3.zero)
            {
                direction =
                    Vector3.forward;
            }
        }

        RotateProjectile(direction);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (hasImpacted)
        {
            return;
        }

        UpdateHomingDirection();

        transform.position +=
            direction *
            speed *
            Time.deltaTime;

        timer -=
            Time.deltaTime;

        if (timer <= 0f)
        {
            Vector3 fallbackNormal =
                direction != Vector3.zero
                    ? -direction
                    : -transform.right;

            Impact(
                transform.position,
                fallbackNormal
            );
        }
    }

    // =========================================================
    // HOMING
    // =========================================================

    private void UpdateHomingDirection()
    {
        if (!isHoming ||
            homingTarget == null)
        {
            return;
        }

        homingTimer +=
            Time.deltaTime;

        Vector3 targetPosition =
            homingTarget.position +
            Vector3.up *
            homingTargetHeight;

        Vector3 desiredDirection =
            (targetPosition -
             transform.position).normalized;

        if (desiredDirection ==
            Vector3.zero)
        {
            return;
        }

        if (homingTimer <
            homingDelay)
        {
            return;
        }

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

    // =========================================================
    // ROTATION
    // =========================================================

    private void RotateProjectile(
        Vector3 newDirection)
    {
        if (newDirection ==
            Vector3.zero)
        {
            return;
        }

        transform.right =
            newDirection;
    }

    // =========================================================
    // COLLISION
    // =========================================================

    private void OnTriggerEnter(
        Collider other)
    {
        if (hasImpacted)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        // =====================================================
        // IGNORE OWNER
        // =====================================================

        if (owner != null)
        {
            if (other.gameObject ==
                    owner ||
                other.transform.IsChildOf(
                    owner.transform))
            {
                return;
            }
        }

        // =====================================================
        // PLAYER
        // =====================================================

        if (other.CompareTag("Player"))
        {
            HitPlayer(other);
            return;
        }

        // =====================================================
        // ENEMY / BOSS
        // =====================================================

        if (other.CompareTag("Enemy"))
        {
            HitEnemy(other);
            return;
        }

        // =====================================================
        // IGNORE OTHER TRIGGERS
        // =====================================================

        if (other.isTrigger)
        {
            return;
        }

        // =====================================================
        // ROCK / WALL / GROUND / TREE / ...
        // =====================================================

        Vector3 hitPosition =
            GetImpactPosition(other);

        Vector3 hitNormal =
            GetImpactNormal(
                hitPosition
            );

        Impact(
            hitPosition,
            hitNormal
        );
    }

    // =========================================================
    // HIT PLAYER
    // =========================================================

    private void HitPlayer(
        Collider other)
    {
        Vector3 hitPosition =
            GetImpactPosition(other);

        Vector3 hitNormal =
            GetImpactNormal(
                hitPosition
            );

        PlayerController player =
    other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            // =====================================================
            // SHIELD
            // =====================================================

            SpellCaster spellCaster =
                player.GetComponent<SpellCaster>();

            ShieldSpell shieldSpell = null;

            if (spellCaster != null &&
                spellCaster.equippedSpell != null)
            {
                shieldSpell =
                    spellCaster.equippedSpell as ShieldSpell;
            }

            if (shieldSpell != null &&
                shieldSpell.IsActiveFor(player))
            {
                // Shield chặn toàn bộ:
                // - Damage
                // - Stun
                // - Slow
                // - Burn
                shieldSpell.TryBlockDamage(player);

                Impact(
                    hitPosition,
                    hitNormal
                );

                return;
            }

            // =====================================================
            // INVINCIBILITY
            // Dash / Ultimate
            // =====================================================

            if (player.IsInvincible)
            {
                Impact(
                    hitPosition,
                    hitNormal
                );

                return;
            }
        }

        PlayerStats stats =
            other.GetComponentInParent<
                PlayerStats>();

        if (stats != null)
        {
            if (FeedbackManager.Instance !=
                null)
            {
                FeedbackManager.Instance
                    .PlayHitFeedback(
                        0f,
                        1f,
                        0.15f,
                        0.03f
                    );

                FeedbackManager.Instance
                    .PlayDamageFlash(
                        0.1f
                    );
            }

            stats.TakeDamage(
                damage
            );
        }

        PlayerStatusEffects statusEffects =
            other.GetComponentInParent<
                PlayerStatusEffects>();

        if (statusEffects != null &&
            effectType !=
                ProjectileEffectType.None)
        {
            statusEffects.ApplyEffect(
                effectType,
                effectDuration,
                effectValue
            );
        }

        Impact(
            hitPosition,
            hitNormal
        );
    }

    // =========================================================
    // HIT ENEMY / BOSS
    // =========================================================

    private void HitEnemy(
        Collider other)
    {
        Vector3 hitPosition =
            GetImpactPosition(other);

        Vector3 hitNormal =
            GetImpactNormal(
                hitPosition
            );

        /*
         * =====================================================
         * ENEMY PROJECTILE
         * =====================================================
         *
         * Nếu projectile được bắn bởi Enemy:
         *
         * - Không gây damage Enemy khác.
         * - Vẫn Impact.
         * - Vẫn VFX.
         * - Vẫn SFX.
         * - Vẫn ReturnToPool().
         */
        if (IsOwnedByEnemy())
        {
            Impact(
                hitPosition,
                hitNormal
            );

            return;
        }

        /*
         * =====================================================
         * PLAYER PROJECTILE
         * =====================================================
         *
         * Projectile của Player vẫn gây damage Enemy/Boss
         * như bình thường.
         */

        EnemyController enemy =
            other.GetComponentInParent<
                EnemyController>();

        if (enemy != null)
        {
            enemy.TakeDamage(
                damage
            );

            Impact(
                hitPosition,
                hitNormal
            );

            return;
        }

        BossController boss =
            other.GetComponentInParent<
                BossController>();

        if (boss != null)
        {
            boss.TakeDamage(
                damage
            );
        }

        Impact(
            hitPosition,
            hitNormal
        );
    }

    // =========================================================
    // IMPACT
    // =========================================================

    private void Impact(
        Vector3 position,
        Vector3 normal)
    {
        if (hasImpacted)
        {
            return;
        }

        hasImpacted =
            true;

        // =====================================================
        // VFX
        // =====================================================

        SpawnImpactVFX(
            position,
            normal
        );

        // =====================================================
        // SFX
        // =====================================================

        PlayImpactSFX(
            position
        );

        // =====================================================
        // RETURN TO POOL
        // =====================================================

        ReturnToPool();
    }

    // =========================================================
    // IMPACT POSITION
    // =========================================================

    private Vector3 GetImpactPosition(
        Collider other)
    {
        if (other == null)
        {
            return transform.position;
        }

        Vector3 closestPoint =
            other.ClosestPoint(
                transform.position
            );

        return closestPoint;
    }

    // =========================================================
    // IMPACT NORMAL
    // =========================================================

    private Vector3 GetImpactNormal(
        Vector3 hitPosition)
    {
        Vector3 normal =
            transform.position -
            hitPosition;

        if (normal.sqrMagnitude <
            0.0001f)
        {
            if (direction !=
                Vector3.zero)
            {
                normal =
                    -direction;
            }
            else
            {
                normal =
                    -transform.right;
            }
        }

        return normal.normalized;
    }

    // =========================================================
    // SPAWN VFX
    // =========================================================

    private void SpawnImpactVFX(
        Vector3 position,
        Vector3 normal)
    {
        if (impactVFX == null)
        {
            return;
        }

        Vector3 spawnPosition =
            position +
            impactVFXPositionOffset;

        Quaternion spawnRotation =
            Quaternion.identity;

        if (rotateImpactVFX &&
            normal.sqrMagnitude >
            0.0001f)
        {
            spawnRotation =
                Quaternion.LookRotation(
                    normal
                );
        }

        spawnRotation *=
            Quaternion.Euler(
                impactVFXRotationOffset
            );

        GameObject spawnedVFX =
            Instantiate(
                impactVFX,
                spawnPosition,
                spawnRotation
            );

        if (spawnedVFX == null)
        {
            return;
        }

        if (impactVFXLifeTime >
            0f)
        {
            Destroy(
                spawnedVFX,
                impactVFXLifeTime
            );
        }
    }

    // =========================================================
    // SFX
    // =========================================================

    private void PlayImpactSFX(
        Vector3 position)
    {
        if (impactSFX == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(
            impactSFX,
            position,
            impactSFXVolume
        );
    }

    // =========================================================
    // RETURN TO POOL
    // =========================================================

    private void ReturnToPool()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (PoolManager.Instance !=
            null)
        {
            PoolManager.Instance
                .ReturnObject(
                    gameObject
                );
        }
        else
        {
            gameObject.SetActive(
                false
            );
        }
    }

    // =========================================================
    // RESET HOMING
    // =========================================================

    private void ResetHoming()
    {
        isHoming =
            false;

        homingTarget =
            null;

        homingTurnSpeed =
            0f;

        homingDelay =
            0f;

        homingTimer =
            0f;

        homingTargetHeight =
            0f;
    }

    // =========================================================
    // DISABLE / RETURNED TO POOL
    // =========================================================

    private void OnDisable()
    {
        direction =
            Vector3.zero;

        damage =
            0f;

        timer =
            0f;

        owner =
            null;

        effectType =
            ProjectileEffectType.None;

        effectDuration =
            0f;

        effectValue =
            0f;

        ResetHoming();
    }
}