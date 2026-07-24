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

    private float currentHP;

    [Header("Detection")]
    public float detectRange = 100f;

    [Header("Movement")]
    public float runSpeed = 8f;
    public float walkSpeed = 2f;

    [Header("Melee")]
    public float meleeRange = 15f;
    public float meleeDamage = 25f;

    [Header("Ranged")]
    public float projectileDamage = 20f;
    public Projectile projectilePrefab;
    public Transform firePoint;


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

        if (player == null &&
            PoolManager.Instance != null)
        {
            player =
                PoolManager.Instance.player;
        }

        currentState =
            BossState.Idle;

        animator.SetBool( WalkHash,false);
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position);

        if (distance > detectRange)
        {
            animator.SetBool(
                WalkHash,
                false);

            animator.SetBool(
                RunHash,
                false);

            return;
        }

        if (!isAttacking &&
            !isCoolingDown)
        {

            StartCoroutine(
                AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        currentState =
            BossState.Attacking;

        bool useMelee =
            Random.value > 0.5f;
        if(attackCounter >= attacksBeforeSummon)
        {
            attackCounter = 0;

            yield return StartCoroutine(
                SummonRoutine());
        }
        else
        {
            if (useMelee)
            {
                yield return StartCoroutine(
                    MeleeAttackRoutine());
            }
            else
            {
                yield return StartCoroutine(
                    RangedAttackRoutine());
            }
            attackCounter++;
        }

        isAttacking = false;

        StartCoroutine(
            CooldownRoutine());
    }

    private IEnumerator BurrowRetreatRoutine()
    {
        agent.isStopped = true;

        animator.SetTrigger(BurrowHash);

        // Chờ animation chui xuống đất
        yield return new WaitForSeconds(
            burrowDuration * 0.5f);

        Vector2 randomCircle =
    Random.insideUnitCircle.normalized;

        Vector3 targetPos =
            player.position +
            new Vector3(
                randomCircle.x,
                0,
                randomCircle.y)
            * retreatDistance;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            targetPos,
            out hit,
            5f,
            NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        // Chờ animation trồi lên hoàn tất
        yield return new WaitForSeconds(
            burrowDuration * 0.5f);
    }

    private IEnumerator MeleeAttackRoutine()
    {
        agent.speed = runSpeed;
        agent.isStopped = false;

        animator.SetBool(WalkHash, false);
        animator.SetBool(RunHash, true);

        // CHASE cho tới khi vào range
        while (true)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= meleeRange)
                break;

            Vector3 dir =
                (transform.position - player.position).normalized;

            Vector3 targetPos =
                player.position + dir * 1.2f;

            agent.SetDestination(targetPos);

            yield return null;
        }

        // STOP và ATTACK
        agent.isStopped = true;
        animator.SetBool(RunHash, false);

        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        animator.SetTrigger(SlashHash);

        yield return new WaitForSeconds(0.6f);

        Collider[] hits =
            Physics.OverlapSphere(transform.position, meleeRange);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            PlayerStats stats = hit.GetComponent<PlayerStats>();

            if (stats != null)
                stats.TakeDamage(meleeDamage);

            break;
        }

        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator RangedAttackRoutine()
    {
        float distance =
        Vector3.Distance(
            transform.position,
            player.position);

        if (distance < retreatTriggerDistance)
        {
            yield return StartCoroutine(
                BurrowRetreatRoutine());
        }

        agent.isStopped = true;

        animator.SetBool(RunHash, false);
        animator.SetTrigger(PrepareHash);

        float prepareTime = 1f;
        float timer = 0f;

        while (timer < prepareTime)
        {
            timer += Time.deltaTime;

            Vector3 lookPos = player.position;
            lookPos.y = transform.position.y;

            Vector3 dir = (lookPos - transform.position).normalized;

            if (dir != Vector3.zero)
            {
                Quaternion targetRot =
                    Quaternion.LookRotation(dir);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        10f * Time.deltaTime);
            }

            yield return null;
        }

        SpawnProjectile();

        yield return new WaitForSeconds(0.8f);
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab == null ||
            firePoint == null)
            return;

        GameObject obj =
            PoolManager.Instance.GetObject(
                projectilePrefab.gameObject);

        Projectile projectile =
            obj.GetComponent<Projectile>();

        projectile.transform.position =
            firePoint.position;

        projectile.transform.rotation =
            firePoint.rotation;

        projectile.Initialize(
            player,
            projectileDamage);

        projectile.SetOwner(
            gameObject);
    }

    private IEnumerator SummonRoutine()
    {
        agent.isStopped = true;

        animator.SetTrigger(
            SpawnHash);

        yield return new WaitForSeconds(
            2f);

        foreach (Transform point
                 in summonPoints)
        {
            EnemyController enemy =
                Instantiate(
                    summonPrefab,
                    point.position,
                    point.rotation);

            enemy.OnSpawn(
                player);
        }

        yield return new WaitForSeconds(
            1f);
    }

    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;

        currentState =
            BossState.Cooldown;

        agent.isStopped = false;
        agent.speed = walkSpeed;

        animator.SetBool(
            WalkHash,
            true);

        Vector3 randomDir =
            Random.insideUnitSphere;

        randomDir.y = 0;

        Vector3 targetPos =
            transform.position +
            randomDir.normalized *
            cooldownMoveDistance;

        agent.SetDestination(
            targetPos);

        yield return new WaitForSeconds(
            cooldownTime);

        animator.SetBool(
            WalkHash,
            false);

        isCoolingDown = false;

        currentState =
            BossState.Combat;
    }

    public void TakeDamage(
        float damage)
    {
        if (isDead)
            return;

        currentHP -= damage;

        Debug.Log(
            "Boss HP: " +
            currentHP);

        if (damage >= 30f)
        {
            animator.SetTrigger(
                DamageHash);
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        StopAllCoroutines();

        isDead = true;

        currentState =
            BossState.Dead;

        agent.isStopped = true;
        agent.ResetPath();

        animator.SetBool(
            WalkHash,
            false);

        animator.SetBool(
            RunHash,
            false);

        animator.SetTrigger(
            DieHash);

        StartCoroutine(
            DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(
            5f);

        gameObject.SetActive(
            false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectRange);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            meleeRange);
    }
}