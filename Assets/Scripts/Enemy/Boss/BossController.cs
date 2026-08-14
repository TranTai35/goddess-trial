using System.Collections;
using System.Collections.Generic;
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

    [Header("Ending Cutscene")]
    [Tooltip("EndingCutscene sẽ được gọi sau khi Boss chết.")]
    [SerializeField]
    private EndingCutscene endingCutscene;

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

    [Range(1f, 180f)]
    [Tooltip("Góc vùng đánh phía trước Boss. Player ở sau lưng sẽ không bị trúng.")]
    public float meleeHitAngle = 100f;

    [Tooltip("Nếu bật, Player đang bất tử trong lúc dash sẽ không nhận đòn melee.")]
    public bool respectPlayerDashInvincibility = true;

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
    [Tooltip("Danh sách các loại Enemy Boss có thể triệu hồi từ PoolManager.")]
    public EnemyController[] summonPrefabs;

    [Tooltip("Prefab dự phòng nếu Summon Prefabs chưa được gắn.")]
    public EnemyController summonPrefab;

    [Tooltip("Các vị trí Enemy có thể xuất hiện.")]
    public Transform[] summonPoints;

    [Min(1)]
    [Tooltip("Tổng số Enemy được triệu hồi trong mỗi lần Boss dùng chiêu summon.")]
    public int summonCount = 6;

    [Min(0f)]
    [Tooltip("Khoảng cách thời gian giữa từng Enemy được lấy ra từ pool.")]
    public float summonInterval = 0.15f;

    [Tooltip("Chọn ngẫu nhiên điểm sinh. Nếu tắt, Boss sẽ lần lượt dùng các điểm.")]
    public bool randomizeSummonPoints = true;

    [Tooltip("Chọn ngẫu nhiên loại Enemy trong Summon Prefabs.")]
    public bool randomizeSummonPrefabs = true;

    [Header("Summon VFX")]
    [Tooltip("Prefab VFX xuất hiện tại điểm triệu hồi trước khi Enemy sinh ra.")]
    public GameObject summonVFXPrefab;

    [Tooltip("Độ lệch vị trí VFX so với Summon Point.")]
    public Vector3 summonVFXPositionOffset = Vector3.zero;

    [Tooltip("Độ lệch góc xoay VFX so với Summon Point.")]
    public Vector3 summonVFXRotationOffset = Vector3.zero;

    [Min(0f)]
    [Tooltip("Thời gian VFX xuất hiện trước khi Enemy được sinh ra.")]
    public float summonVFXLeadTime = 0.8f;

    [Min(0f)]
    [Tooltip("Thời gian giữ VFX thêm sau khi Enemy đã sinh ra.")]
    public float summonVFXPostSpawnTime = 0.1f;

    [Min(0f)]
    [Tooltip("Thời gian Boss chỉ đứng thực hiện animation triệu hồi trước khi trở lại combat.")]
    public float summonAnimationDuration = 2f;

    [Tooltip("Không cho bắt đầu thêm một đợt sinh Enemy mới khi đợt trước vẫn đang chạy.")]
    public bool preventOverlappingSummons = true;

    public int attacksBeforeSummon = 3;

    [Header("Summoned Enemy Cleanup")]
    [Min(0f)]
    [Tooltip("Khoảng cách thời gian giữa từng Enemy chết khi Boss bị tiêu diệt.")]
    public float summonedEnemyDeathInterval = 0.15f;

    private readonly List<EnemyController> summonedEnemies =
        new List<EnemyController>();

    [Header("Cooldown")]
    public float cooldownTime = 1f;
    public float cooldownMoveDistance = 3f;

    [Header("Teleport Retreat")]
    public float retreatTriggerDistance = 5f;
    public float retreatDistance = 20f;
    public float burrowDuration = 1f;

    [Header("Teleport VFX")]
    [Tooltip("VFX xuất hiện tại nơi Boss chuẩn bị dịch chuyển tới.")]
    public GameObject teleportVFXPrefab;

    [Tooltip("Độ lệch vị trí của VFX so với điểm teleport.")]
    public Vector3 teleportVFXPositionOffset = Vector3.zero;

    [Tooltip("Độ lệch góc xoay của VFX.")]
    public Vector3 teleportVFXRotationOffset = Vector3.zero;

    [Min(0f)]
    [Tooltip("Thời gian VFX hiện trước khi Boss xuất hiện.")]
    public float teleportVFXLeadTime = 0.3f;

    [Min(0f)]
    [Tooltip("Thời gian VFX còn tồn tại sau khi Boss xuất hiện.")]
    public float teleportVFXPostTime = 0.2f;

    private BossState currentState;

    private bool isDead;
    private bool isAttacking;
    private bool isCoolingDown;
    private bool isSummonSequenceRunning;

    private int attackCounter;

    // =========================================================
    // AUDIO
    // =========================================================

    private BossAudio bossAudio;

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
        // =====================================================
        // AUDIO
        // =====================================================

        bossAudio =
            GetComponent<BossAudio>();

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

    // =========================================================
    // TELEPORT
    // =========================================================

    private IEnumerator BurrowRetreatRoutine()
    {
        if (player == null)
        {
            yield break;
        }

        if (agent != null)
        {
            agent.isStopped = true;
        }

        // =====================================================
        // BOSS BẮT ĐẦU BURROW
        // =====================================================

        if (animator != null)
        {
            animator.SetTrigger(BurrowHash);
        }

        // Phát SFX teleport.
        if (bossAudio != null)
        {
            bossAudio.PlayTeleportSFX();
        }

        /*
         * Chờ phần đầu animation để Boss biến mất.
         */
        yield return new WaitForSeconds(
            burrowDuration * 0.5f
        );

        if (player == null)
        {
            yield break;
        }

        // =====================================================
        // TÍNH VỊ TRÍ TELEPORT
        // =====================================================

        Vector2 randomCircle =
            Random.insideUnitCircle;

        /*
         * Tránh trường hợp random ra vector gần bằng 0.
         */
        if (randomCircle.sqrMagnitude < 0.001f)
        {
            randomCircle =
                Vector2.right;
        }

        randomCircle.Normalize();

        Vector3 desiredPosition =
            player.position +
            new Vector3(
                randomCircle.x,
                0f,
                randomCircle.y
            ) * retreatDistance;

        /*
         * Tìm vị trí hợp lệ trên NavMesh.
         */
        if (agent == null ||
            !NavMesh.SamplePosition(
                desiredPosition,
                out NavMeshHit hit,
                5f,
                NavMesh.AllAreas
            ))
        {
            yield break;
        }

        Vector3 teleportPosition =
            hit.position;

        // =====================================================
        // HIỆN VFX Ở NƠI BOSS SẼ XUẤT HIỆN
        // =====================================================

        GameObject teleportVFX =
            null;

        if (teleportVFXPrefab != null)
        {
            Vector3 vfxPosition =
                teleportPosition +
                teleportVFXPositionOffset;

            Quaternion vfxRotation =
                Quaternion.Euler(
                    teleportVFXRotationOffset
                );

            teleportVFX =
                Instantiate(
                    teleportVFXPrefab,
                    vfxPosition,
                    vfxRotation
                );
        }

        /*
         * VFX xuất hiện trước để Player thấy
         * Boss chuẩn bị teleport tới đâu.
         */
        if (teleportVFXLeadTime > 0f)
        {
            yield return new WaitForSeconds(
                teleportVFXLeadTime
            );
        }

        // =====================================================
        // TELEPORT BOSS
        // =====================================================

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.Warp(
                teleportPosition
            );
        }
        else
        {
            transform.position =
                teleportPosition;
        }

        // =====================================================
        // VFX TỰ TẮT, KHÔNG GIỮ BOSS ĐỨNG CHỜ
        // =====================================================

        if (teleportVFX != null)
        {
            if (teleportVFXPostTime > 0f)
            {
                Destroy(
                    teleportVFX,
                    teleportVFXPostTime
                );
            }
            else
            {
                Destroy(
                    teleportVFX
                );
            }
        }

        /*
         * QUAN TRỌNG:
         *
         * Không có yield WaitForSeconds nào nữa sau Warp.
         *
         * Hàm kết thúc ngay tại đây.
         * RangedAttackRoutine sẽ tiếp tục ngay lập tức,
         * nên Boss có thể Prepare -> bắn projectile ngay.
         */
    }

    // =========================================================
    // MELEE
    // =========================================================

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

        // =====================================================
        // SLASH SFX
        // Phát đúng lúc đòn chém/VFX xảy ra.
        // =====================================================

        if (bossAudio != null)
        {
            bossAudio.PlaySlashSFX();
        }

        StartCoroutine(
            SpawnMeleeSlashVFXRoutine()
        );

        TryDealMeleeDamage();

        yield return new WaitForSeconds(
            meleeRecoveryTime
        );
    }

    private void TryDealMeleeDamage()
    {
        if (player == null)
            return;

        Vector3 toPlayer =
            player.position -
            transform.position;

        // Chỉ xét hướng trên mặt đất.
        toPlayer.y = 0f;

        float distance =
            toPlayer.magnitude;

        if (distance > meleeRange ||
            toPlayer.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angleToPlayer =
            Vector3.Angle(
                transform.forward,
                toPlayer.normalized
            );

        // Chỉ trúng trong hình nón phía trước Boss.
        if (angleToPlayer >
            meleeHitAngle * 0.5f)
        {
            return;
        }

        PlayerController playerController =
            player.GetComponentInParent<PlayerController>();

        if (respectPlayerDashInvincibility &&
            playerController != null &&
            playerController.IsInvincible)
        {
            return;
        }

        PlayerStats stats =
            player.GetComponentInParent<PlayerStats>();

        if (stats == null)
            return;

        PlayerStatusEffects statusEffects =
            player.GetComponentInParent<PlayerStatusEffects>();

        stats.TakeDamage(meleeDamage);

        if (statusEffects != null &&
            meleeStunDuration > 0f)
        {
            statusEffects.ApplyStun(
                meleeStunDuration
            );
        }
    }

    // =========================================================
    // RANGED
    // =========================================================

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

    // =========================================================
    // MELEE VFX
    // =========================================================

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

    // =========================================================
    // PROJECTILE
    // =========================================================

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

        // =====================================================
        // RANGED ATTACK SFX
        // Mỗi projectile được tạo -> phát 1 lần.
        // =====================================================

        if (bossAudio != null)
        {
            bossAudio.PlayRangedAttackSFX();
        }
    }

    // =========================================================
    // SUMMON
    // =========================================================

    private IEnumerator SummonRoutine()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.SetBool(WalkHash, false);
            animator.SetBool(RunHash, false);
            animator.SetTrigger(SpawnHash);
        }

        // =====================================================
        // CAST SFX
        // Phát ngay khi Boss bắt đầu animation triệu hồi.
        // =====================================================

        if (bossAudio != null)
        {
            bossAudio.PlayCastSFX();
        }

        // Boss chỉ đứng chờ animation triệu hồi hoàn tất.
        if (summonAnimationDuration > 0f)
        {
            yield return new WaitForSeconds(
                summonAnimationDuration
            );
        }

        // Sau khi animation xong, bắt đầu sinh Enemy ở coroutine riêng.
        // AttackRoutine không chờ coroutine này nên Boss có thể quay lại combat.
        if (!preventOverlappingSummons ||
            !isSummonSequenceRunning)
        {
            StartCoroutine(
                SummonEnemySequenceRoutine()
            );
        }
    }

    private IEnumerator SummonEnemySequenceRoutine()
    {
        isSummonSequenceRunning = true;

        if (PoolManager.Instance == null)
        {
            Debug.LogError(
                "Boss không thể triệu hồi Enemy vì scene chưa có PoolManager.",
                this
            );

            isSummonSequenceRunning = false;
            yield break;
        }

        if (summonPoints == null ||
            summonPoints.Length == 0)
        {
            Debug.LogWarning(
                "Boss chưa được gắn Summon Points.",
                this
            );

            isSummonSequenceRunning = false;
            yield break;
        }

        int amount =
            Mathf.Max(
                1,
                summonCount
            );

        for (int i = 0; i < amount; i++)
        {
            if (isDead)
                break;

            EnemyController prefab =
                GetSummonPrefab(i);

            Transform point =
                GetSummonPoint(i);

            if (prefab != null &&
                point != null)
            {
                yield return StartCoroutine(
                    SpawnEnemyWithSummonVFXRoutine(
                        prefab,
                        point
                    )
                );
            }

            if (i < amount - 1 &&
                summonInterval > 0f)
            {
                yield return new WaitForSeconds(
                    summonInterval
                );
            }
        }

        isSummonSequenceRunning = false;
    }

    private IEnumerator SpawnEnemyWithSummonVFXRoutine(
        EnemyController prefab,
        Transform spawnPoint)
    {
        GameObject summonVFX = null;

        if (summonVFXPrefab != null)
        {
            Vector3 vfxPosition =
                spawnPoint.TransformPoint(
                    summonVFXPositionOffset
                );

            Quaternion vfxRotation =
                spawnPoint.rotation *
                Quaternion.Euler(
                    summonVFXRotationOffset
                );

            summonVFX =
                Instantiate(
                    summonVFXPrefab,
                    vfxPosition,
                    vfxRotation
                );
        }

        if (summonVFXLeadTime > 0f)
        {
            yield return new WaitForSeconds(
                summonVFXLeadTime
            );
        }

        SpawnEnemyFromPool(
            prefab,
            spawnPoint
        );

        if (summonVFXPostSpawnTime > 0f)
        {
            yield return new WaitForSeconds(
                summonVFXPostSpawnTime
            );
        }

        if (summonVFX != null)
        {
            Destroy(summonVFX);
        }
    }

    private EnemyController GetSummonPrefab(
        int spawnIndex)
    {
        if (summonPrefabs != null &&
            summonPrefabs.Length > 0)
        {
            if (randomizeSummonPrefabs)
            {
                int randomIndex =
                    Random.Range(
                        0,
                        summonPrefabs.Length
                    );

                return summonPrefabs[randomIndex];
            }

            int index =
                spawnIndex %
                summonPrefabs.Length;

            return summonPrefabs[index];
        }

        return summonPrefab;
    }

    private Transform GetSummonPoint(
        int spawnIndex)
    {
        if (summonPoints == null ||
            summonPoints.Length == 0)
        {
            return null;
        }

        if (randomizeSummonPoints)
        {
            int randomIndex =
                Random.Range(
                    0,
                    summonPoints.Length
                );

            return summonPoints[randomIndex];
        }

        int index =
            spawnIndex %
                summonPoints.Length;

        return summonPoints[index];
    }

    private bool SpawnEnemyFromPool(
        EnemyController prefab,
        Transform spawnPoint)
    {
        if (prefab == null ||
            spawnPoint == null ||
            PoolManager.Instance == null)
        {
            return false;
        }

        GameObject enemyObject =
            PoolManager.Instance.GetObject(
                prefab.gameObject
            );

        if (enemyObject == null)
        {
            Debug.LogWarning(
                $"PoolManager không trả về được enemy {prefab.name}. " +
                "Hãy kiểm tra prefab đã được đăng ký trong pool.",
                this
            );

            return false;
        }

        enemyObject.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        EnemyController enemy =
            enemyObject.GetComponent<EnemyController>();

        if (enemy == null)
        {
            Debug.LogError(
                $"{prefab.name} không có EnemyController.",
                enemyObject
            );

            PoolManager.Instance.ReturnObject(
                enemyObject
            );

            return false;
        }

        enemy.ClearSpawnArea();

        enemy.OnSpawn(player);

        // Đánh dấu Enemy này thuộc về Boss hiện tại.
        enemy.SetSummonOwner(this);

        if (!summonedEnemies.Contains(enemy))
        {
            summonedEnemies.Add(enemy);
        }

        return true;
    }

    // =========================================================
    // KILL SUMMONED ENEMIES WHEN BOSS DIES
    // =========================================================

    private IEnumerator KillSummonedEnemiesRoutine()
    {
        for (int i = 0; i < summonedEnemies.Count; i++)
        {
            EnemyController enemy = summonedEnemies[i];

            if (enemy == null ||
                !enemy.gameObject.activeInHierarchy ||
                !enemy.IsSummonedBy(this) ||
                enemy.IsDead)
            {
                continue;
            }

            // Gọi đúng logic Die của Enemy:
            // dừng AI -> bật animation Die -> sau 3 giây trả về pool.
            enemy.ForceDieFromBoss();

            if (summonedEnemyDeathInterval > 0f)
            {
                yield return new WaitForSeconds(
                    summonedEnemyDeathInterval
                );
            }
        }

        summonedEnemies.Clear();
    }

    // =========================================================
    // COOLDOWN
    // =========================================================

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

    // =========================================================
    // DAMAGE
    // =========================================================

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

        GameObject obj;

        if (PoolManager.Instance != null)
        {
            obj =
                PoolManager.Instance.GetObject(
                    damageTextPrefab
                );
        }
        else
        {
            obj =
                Instantiate(
                    damageTextPrefab
                );
        }

        if (obj == null)
        {
            Debug.LogWarning(
                "Không thể lấy DamageText từ PoolManager.",
                this
            );

            return;
        }

        obj.transform.position =
            transform.position +
            damageTextOffset;

        obj.transform.rotation =
            Quaternion.identity;

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

            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.ReturnObject(
                    obj
                );
            }
            else
            {
                Destroy(obj);
            }
        }
    }

    // =========================================================
    // DIE
    // =========================================================

    private void Die()
    {
        if (isDead)
            return;

        StopAllCoroutines();

        isDead = true;
        isSummonSequenceRunning = false;
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

        // Cho các Enemy do Boss triệu hồi chết lần lượt.
        StartCoroutine(
            KillSummonedEnemiesRoutine()
        );

        StartCoroutine(
            DeathRoutine()
        );
    }

    private IEnumerator DeathRoutine()
    {
        // Chờ Boss hoàn tất death animation.
        yield return new WaitForSeconds(5f);

        // Bắt đầu Ending Cutscene sau khi Boss đã chết.
        if (endingCutscene != null)
        {
            endingCutscene.PlayEnding();
        }
        else
        {
            Debug.LogWarning(
                "BossController: Chưa gắn EndingCutscene. Boss sẽ được tắt bình thường.",
                this
            );
        }

        // Boss không còn cần tồn tại trong gameplay.
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

    // =========================================================
    // GIZMOS
    // =========================================================

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