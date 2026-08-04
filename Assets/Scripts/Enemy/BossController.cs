using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    private enum BossState
    {
        Idle,
        Combat,
        Attacking,
        Cooldown,
        Dead
    }

    [Header("References")]
    public Animator animator;
    public NavMeshAgent agent;
    public Transform player;

    [Header("Boss HP")]
    public float maxHP = 300f;

    [Header("Boss UI")]
    public string bossDisplayName = "Boss";

    [Tooltip("Bật thanh máu khi Player đi vào detectRange.")]
    public bool showHealthBarWhenDetected = true;

    [Header("Boss Damage Feedback")]
    [Tooltip("Dùng cùng prefab DamageText đang gắn cho Enemy.")]
    public GameObject damageTextPrefab;

    [Tooltip("Độ cao DamageText so với tâm Boss.")]
    public Vector3 damageTextOffset = new Vector3(0f, 3f, 0f);

    private float currentHP;
    private bool hasShownHealthBar;

    [Header("Detection")]
    public float detectRange = 100f;

    [Header("Movement")]
    public float runSpeed = 8f;
    public float walkSpeed = 2f;

    [Header("Melee")]
    public float meleeRange = 15f;
    public float meleeDamage = 25f;

    [Tooltip("Thời gian Player bị khóa điều khiển khi trúng đòn đánh gần của Boss.")]
    [Min(0f)]
    public float meleeStunDuration = 1.2f;

    [Header("Ranged")]
    public float projectileDamage = 20f;
    public Projectile projectilePrefab;
    public Transform firePoint;

    [Header("Ranged Burst & Homing")]
    [Min(1)]
    public int projectileBurstCount = 3;

    public float projectileBurstInterval = 0.18f;
    public float projectileHomingTurnSpeed = 160f;
    public float projectileHomingDelay = 0.2f;
    public float projectileTargetHeight = 1f;

    [Header("Melee VFX")]
    public GameObject meleeSlashVFX;
    public Transform meleeVFXSpawnPoint;
    public Vector3 meleeVFXLocalPositionOffset;
    public Vector3 meleeVFXLocalRotationOffset;
    public float meleeVFXSpreadAngle = 18f;
    public float meleeVFXInterval = 0.06f;
    public float meleeVFXLifeTime = 2f;

    [Header("Melee Timing")]
    [Tooltip("Thời gian từ lúc bắt đầu animation Slash Attack tới lúc tay hoặc vũ khí chạm Player.")]
    public float meleeImpactDelay = 0.55f;

    [Tooltip("Thời gian chờ sau thời điểm va chạm để animation đánh gần hoàn tất.")]
    public float meleeRecoveryTime = 0.65f;

    [Header("Summon")]
    public EnemyController summonPrefab;
    public Transform[] summonPoints;
    public int attacksBeforeSummon = 3;

    [Header("Cooldown")]
    public float cooldownTime = 1f;
    public float cooldownMoveDistance = 3f;

    [Header("Teleport Retreat")]
    public float retreatTriggerDistance = 5f;
    public float retreatDistance = 20f;
    public float burrowDuration = 1.5f;

    private BossState currentState;

    private bool isDead;
    private bool isAttacking;
    private bool isCoolingDown;

    private int attackCounter;

    private readonly int WalkHash =
        Animator.StringToHash("Walk");

    private readonly int RunHash =
        Animator.StringToHash("Run");

    private readonly int PrepareHash =
        Animator.StringToHash("Prepare");

    private readonly int SlashHash =
        Animator.StringToHash("Slash Attack");

    private readonly int SpawnHash =
        Animator.StringToHash("SpawnDemon");

    private readonly int DamageHash =
        Animator.StringToHash("Take Damage");

    private readonly int DieHash =
        Animator.StringToHash("Die");

    private readonly int BurrowHash =
        Animator.StringToHash("Burrow");

    private void Start()
    {
        currentHP = maxHP;

        currentState = BossState.Idle;

        if (animator != null)
        {
            animator.SetBool(WalkHash, false);
        }

        if (!showHealthBarWhenDetected &&
            BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.ShowBossHealth(
                this,
                bossDisplayName,
                currentHP,
                maxHP
            );

            hasShownHealthBar = true;
        }
    }

    private void Update()
    {
        if (isDead || player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (showHealthBarWhenDetected &&
            !hasShownHealthBar &&
            distance <= detectRange)
        {
            ShowHealthBar();
        }

        if (distance > detectRange)
        {
            if (animator != null)
            {
                animator.SetBool(WalkHash, false);
                animator.SetBool(RunHash, false);
            }

            return;
        }

        if (!isAttacking &&
            !isCoolingDown)
        {
            StartCoroutine(
                AttackRoutine()
            );
        }
    }

    private void ShowHealthBar()
    {
        if (BossHealthUI.Instance == null)
            return;

        BossHealthUI.Instance.ShowBossHealth(
            this,
            bossDisplayName,
            currentHP,
            maxHP
        );

        hasShownHealthBar = true;
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        currentState = BossState.Attacking;

        bool useMelee =
            Random.value > 0.5f;

        if (attackCounter >= attacksBeforeSummon)
        {
            attackCounter = 0;

            yield return StartCoroutine(
                SummonRoutine()
            );
        }
        else
        {
            if (useMelee)
            {
                yield return StartCoroutine(
                    MeleeAttackRoutine()
                );
            }
            else
            {
                yield return StartCoroutine(
                    RangedAttackRoutine()
                );
            }

            attackCounter++;
        }

        isAttacking = false;

        StartCoroutine(
            CooldownRoutine()
        );
    }

    private IEnumerator BurrowRetreatRoutine()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }

        if (animator != null)
        {
            animator.SetTrigger(BurrowHash);
        }

        yield return new WaitForSeconds(
            burrowDuration * 0.5f
        );

        Vector2 randomCircle =
            Random.insideUnitCircle.normalized;

        Vector3 targetPos =
            player.position +
            new Vector3(
                randomCircle.x,
                0f,
                randomCircle.y
            ) * retreatDistance;

        if (agent != null &&
            NavMesh.SamplePosition(
                targetPos,
                out NavMeshHit hit,
                5f,
                NavMesh.AllAreas
            ))
        {
            agent.Warp(hit.position);
        }

        yield return new WaitForSeconds(
            burrowDuration * 0.5f
        );
    }

    private IEnumerator MeleeAttackRoutine()
    {
        if (agent == null || animator == null)
            yield break;

        agent.speed = runSpeed;
        agent.isStopped = false;

        animator.SetBool(WalkHash, false);
        animator.SetBool(RunHash, true);

        while (player != null)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    player.position
                );

            if (distance <= meleeRange)
                break;

            Vector3 dir =
                (transform.position -
                 player.position).normalized;

            Vector3 targetPos =
                player.position +
                dir * 1.2f;

            agent.SetDestination(targetPos);

            yield return null;
        }

        if (player == null)
            yield break;

        agent.isStopped = true;
        animator.SetBool(RunHash, false);

        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        animator.SetTrigger(SlashHash);

        yield return new WaitForSeconds(
            meleeImpactDelay
        );

        StartCoroutine(
            SpawnMeleeSlashVFXRoutine()
        );

        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                meleeRange
            );

        foreach (Collider hit in hits)
        {
            PlayerStats stats =
                hit.GetComponentInParent<PlayerStats>();

            if (stats == null)
                continue;

            PlayerStatusEffects statusEffects =
                hit.GetComponentInParent<PlayerStatusEffects>();

            stats.TakeDamage(meleeDamage);

            if (statusEffects != null &&
                meleeStunDuration > 0f)
            {
                statusEffects.ApplyStun(
                    meleeStunDuration
                );
            }

            break;
        }

        yield return new WaitForSeconds(
            meleeRecoveryTime
        );
    }

    private IEnumerator RangedAttackRoutine()
    {
        if (player == null)
            yield break;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance < retreatTriggerDistance)
        {
            yield return StartCoroutine(
                BurrowRetreatRoutine()
            );
        }

        if (agent != null)
        {
            agent.isStopped = true;
        }

        if (animator != null)
        {
            animator.SetBool(RunHash, false);
            animator.SetTrigger(PrepareHash);
        }

        float prepareTime = 1f;
        float timer = 0f;

        while (timer < prepareTime &&
               player != null)
        {
            timer += Time.deltaTime;

            Vector3 lookPos = player.position;
            lookPos.y = transform.position.y;

            Vector3 dir =
                (lookPos -
                 transform.position).normalized;

            if (dir != Vector3.zero)
            {
                Quaternion targetRot =
                    Quaternion.LookRotation(dir);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        10f * Time.deltaTime
                    );
            }

            yield return null;
        }

        yield return StartCoroutine(
            SpawnProjectileBurstRoutine()
        );

        yield return new WaitForSeconds(0.8f);
    }

    private IEnumerator SpawnProjectileBurstRoutine()
    {
        int count =
            Mathf.Max(
                1,
                projectileBurstCount
            );

        for (int i = 0; i < count; i++)
        {
            SpawnProjectile();

            if (i < count - 1)
            {
                yield return new WaitForSeconds(
                    projectileBurstInterval
                );
            }
        }
    }

    private IEnumerator SpawnMeleeSlashVFXRoutine()
    {
        if (meleeSlashVFX == null)
            yield break;

        Transform spawnPoint =
            meleeVFXSpawnPoint != null
                ? meleeVFXSpawnPoint
                : transform;

        float[] angleOffsets =
        {
            -meleeVFXSpreadAngle,
            0f,
            meleeVFXSpreadAngle
        };

        foreach (float angleOffset in angleOffsets)
        {
            Vector3 worldPosition =
                spawnPoint.TransformPoint(
                    meleeVFXLocalPositionOffset
                );

            Quaternion worldRotation =
                spawnPoint.rotation *
                Quaternion.Euler(
                    meleeVFXLocalRotationOffset
                ) *
                Quaternion.Euler(
                    0f,
                    0f,
                    angleOffset
                );

            GameObject vfx =
                Instantiate(
                    meleeSlashVFX,
                    worldPosition,
                    worldRotation
                );

            if (meleeVFXLifeTime > 0f)
            {
                Destroy(
                    vfx,
                    meleeVFXLifeTime
                );
            }

            if (meleeVFXInterval > 0f)
            {
                yield return new WaitForSeconds(
                    meleeVFXInterval
                );
            }
        }
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab == null ||
            firePoint == null ||
            player == null)
        {
            return;
        }

        GameObject obj;

        if (PoolManager.Instance != null)
        {
            obj =
                PoolManager.Instance.GetObject(
                    projectilePrefab.gameObject
                );
        }
        else
        {
            obj =
                Instantiate(
                    projectilePrefab.gameObject
                );
        }

        if (obj == null)
            return;

        Projectile projectile =
            obj.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogError(
                "Projectile prefab không có component Projectile.",
                obj
            );

            return;
        }

        projectile.transform.position =
            firePoint.position;

        projectile.transform.rotation =
            firePoint.rotation;

        projectile.InitializeHoming(
            player,
            projectileDamage,
            projectileHomingTurnSpeed,
            projectileHomingDelay,
            projectileTargetHeight
        );

        projectile.SetOwner(gameObject);
    }

    private IEnumerator SummonRoutine()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }

        if (animator != null)
        {
            animator.SetTrigger(SpawnHash);
        }

        yield return new WaitForSeconds(2f);

        if (summonPrefab != null &&
            summonPoints != null)
        {
            foreach (Transform point in summonPoints)
            {
                if (point == null)
                    continue;

                EnemyController enemy =
                    Instantiate(
                        summonPrefab,
                        point.position,
                        point.rotation
                    );

                enemy.OnSpawn(player);
            }
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;
        currentState = BossState.Cooldown;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = walkSpeed;
        }

        if (animator != null)
        {
            animator.SetBool(WalkHash, true);
        }

        Vector3 randomDir =
            Random.insideUnitSphere;

        randomDir.y = 0f;

        Vector3 targetPos =
            transform.position +
            randomDir.normalized *
            cooldownMoveDistance;

        if (agent != null)
        {
            agent.SetDestination(targetPos);
        }

        yield return new WaitForSeconds(
            cooldownTime
        );

        if (animator != null)
        {
            animator.SetBool(WalkHash, false);
        }

        isCoolingDown = false;
        currentState = BossState.Combat;
    }

    public void TakeDamage(
        float damage,
        bool isCritical = false)
    {
        if (isDead)
            return;

        currentHP -= damage;

        currentHP =
            Mathf.Clamp(
                currentHP,
                0f,
                maxHP
            );

        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.ShowBossHealth(
                this,
                bossDisplayName,
                currentHP,
                maxHP
            );

            hasShownHealthBar = true;
        }

        Debug.Log(
            $"Boss HP: {currentHP}/{maxHP}"
        );

        ShowDamageText(
            damage,
            isCritical
        );

        if (currentHP <= 0f)
        {
            Die();
            return;
        }

        if (damage >= 30f &&
            animator != null)
        {
            animator.SetTrigger(DamageHash);
        }
    }

    private void ShowDamageText(
        float damage,
        bool isCritical)
    {
        if (damageTextPrefab == null)
            return;

        GameObject obj =
            Instantiate(
                damageTextPrefab,
                transform.position +
                damageTextOffset,
                Quaternion.identity
            );

        DamageText damageText =
            obj.GetComponentInChildren<DamageText>(
                true
            );

        if (damageText != null)
        {
            damageText.Setup(
                Mathf.RoundToInt(damage),
                isCritical
            );
        }
        else
        {
            Debug.LogWarning(
                "Damage Text Prefab của Boss không có component DamageText.",
                damageTextPrefab
            );
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        StopAllCoroutines();

        isDead = true;
        currentState = BossState.Dead;

        // Ẩn thanh máu ngay khi Boss hết HP.
        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.HideBossHealth(
                this
            );
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.SetBool(WalkHash, false);
            animator.SetBool(RunHash, false);
            animator.SetTrigger(DieHash);
        }

        StartCoroutine(
            DeathRoutine()
        );
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(5f);

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (isDead &&
            BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.HideBossHealth(
                this
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectRange
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            meleeRange
        );
    }
}
