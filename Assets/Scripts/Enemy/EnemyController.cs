using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    protected enum State
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }

    [Header("References")]
    public Animator animator;
    public Transform player;

    [Header("Patrol")]
    public float patrolRadius = 4f;
    public float walkSpeed = 2f;

    [Header("Detection")]
    public float detectRadius = 10f;

    [Header("Combat")]
    public float runSpeed = 4.5f;
    public float attackRange = 2f;
    public float cooldownTime = 1f;

    [Header("Health")]
    public float maxHP = 100f;

    public GameObject damageTextPrefab;

    protected float currentHP;

    protected NavMeshAgent agent;
    protected State currentState;

    protected bool isDead;
    protected bool isAttacking;
    protected bool isCoolingDown;
    protected bool isTakingDamage;

    private bool goingToA;

    private Vector3 patrolPointA;
    private Vector3 patrolPointB;
    private Vector3 currentPatrolTarget;

    private Coroutine damageRoutine;

    private EnemySpawnArea spawnArea;
    private int spawnTypeIndex = -1;

    // Boss sở hữu Enemy này nếu Enemy được Boss triệu hồi.
    private BossController summonOwner;

    public int SpawnTypeIndex => spawnTypeIndex;
    public bool IsDead => isDead;

    protected readonly int WalkHash =
        Animator.StringToHash("Walk");

    protected readonly int RunHash =
        Animator.StringToHash("Run");

    protected readonly int AttackHash =
        Animator.StringToHash("Attack");

    protected readonly int CooldownHash =
        Animator.StringToHash("Cooldown");

    protected readonly int DamageHash =
        Animator.StringToHash("Take Damage");

    protected readonly int DieHash =
        Animator.StringToHash("Die");

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetSpawnArea(
        EnemySpawnArea area,
        int enemyTypeIndex)
    {
        spawnArea = area;
        spawnTypeIndex = enemyTypeIndex;
    }

    public void ClearSpawnArea()
    {
        spawnArea = null;
        spawnTypeIndex = -1;
    }

    public void SetSummonOwner(BossController owner)
    {
        summonOwner = owner;
    }

    public bool IsSummonedBy(BossController owner)
    {
        return summonOwner == owner;
    }

    public virtual void OnSpawn(Transform targetPlayer)
    {
        StopAllCoroutines();

        player = targetPlayer;

        // Mỗi lần lấy từ pool ra thì bỏ owner cũ.
        // Nếu được Boss triệu hồi, BossController sẽ gắn owner lại sau OnSpawn().
        summonOwner = null;

        currentHP = maxHP;

        isDead = false;
        isAttacking = false;
        isCoolingDown = false;
        isTakingDamage = false;

        agent.enabled = false;

        if (NavMesh.SamplePosition(
            transform.position,
            out NavMeshHit spawnHit,
            3f,
            NavMesh.AllAreas))
        {
            transform.position = spawnHit.position;

            agent.enabled = true;
        }
        else
        {
            Debug.LogWarning(
                $"{name}: SpawnPoint không nằm gần NavMesh tại " +
                $"{transform.position}.");

            return;
        }

        agent.isStopped = false;
        agent.ResetPath();

        animator.Rebind();
        animator.Update(0f);

        animator.SetBool(WalkHash, false);
        animator.SetBool(RunHash, false);

        CreatePatrolPoints();

        float distance =
            Vector3.Distance(
                transform.position,
                player.position);

        currentState =
            distance <= detectRadius
            ? State.Chase
            : State.Patrol;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (isDead || isTakingDamage)
            return;

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                ChasePlayer();
                break;
        }

        CheckDetection();
    }

    #region PATROL

    protected virtual void Patrol()
    {
        agent.speed = walkSpeed;
        agent.isStopped = false;

        animator.SetBool(WalkHash, true);
        animator.SetBool(RunHash, false);

        agent.SetDestination(
            currentPatrolTarget);

        if (Vector3.Distance(
                transform.position,
                currentPatrolTarget) < 1f)
        {
            goingToA = !goingToA;

            currentPatrolTarget =
                goingToA
                ? patrolPointA
                : patrolPointB;
        }
    }

    private void CreatePatrolPoints()
    {
        patrolPointA =
            GetRandomNavMeshPoint(
                transform.position,
                patrolRadius);

        do
        {
            patrolPointB =
                GetRandomNavMeshPoint(
                    transform.position,
                    patrolRadius);
        }
        while (
            Vector3.Distance(
                patrolPointA,
                patrolPointB) < 5f);

        goingToA = false;
        currentPatrolTarget = patrolPointB;
    }

    private Vector3 GetRandomNavMeshPoint(
        Vector3 center,
        float radius)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPos =
                center +
                new Vector3(
                    Random.Range(-radius, radius),
                    0,
                    Random.Range(-radius, radius));

            if (NavMesh.SamplePosition(
                randomPos,
                out NavMeshHit hit,
                2f,
                NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return center;
    }

    #endregion

    #region CHASE

    protected virtual void ChasePlayer()
    {
        if (player == null)
            return;

        agent.speed = runSpeed;
        agent.isStopped = false;

        animator.SetBool(WalkHash, false);
        animator.SetBool(RunHash, true);

        Vector3 dir =
            (transform.position -
             player.position).normalized;

        Vector3 targetPos =
            player.position +
            dir * 1f;

        agent.SetDestination(targetPos);

        float distance =
            Vector3.Distance(
                transform.position,
                player.position);

        if (distance <= attackRange &&
            !isAttacking &&
            !isCoolingDown)
        {
            StartCoroutine(
                AttackRoutine());
        }
    }

    #endregion

    #region ATTACK

    protected virtual IEnumerator Attack()
    {
        yield return null;
    }

    protected virtual IEnumerator AttackRoutine()
    {
        isAttacking = true;

        currentState = State.Attack;

        agent.isStopped = true;

        animator.SetBool(
            RunHash,
            false);

        Vector3 lookPos =
            player.position;

        lookPos.y =
            transform.position.y;

        transform.LookAt(
            lookPos);

        animator.SetTrigger(
            AttackHash);

        yield return StartCoroutine(
            Attack());

        isAttacking = false;

        StartCoroutine(
            CooldownRoutine());
    }

    protected virtual IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;

        animator.SetTrigger(
            CooldownHash);

        yield return new WaitForSeconds(
            cooldownTime);

        if (isDead)
            yield break;

        isCoolingDown = false;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position);

        currentState =
            distance <= detectRadius
            ? State.Chase
            : State.Patrol;
    }

    #endregion

    #region DAMAGE

    public virtual void TakeDamage(float damage, bool isCritical = false)
    {
        if (isDead)
            return;

        currentHP -= damage;
        Debug.Log("HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        if (damageRoutine != null)
        {
            StopCoroutine(
                damageRoutine);
        }

        damageRoutine =
            StartCoroutine(
                TakeDamageRoutine());

        if (damageTextPrefab != null)
        {
            // Lấy object từ pool
            GameObject obj = PoolManager.Instance.GetObject(damageTextPrefab);

            // Đặt vị trí trên đầu quái
            obj.transform.position = transform.position + Vector3.up * 2f;

            // Đặt rotation nhìn về phía Camera (hoặc để mặc định)
            obj.transform.rotation = Quaternion.identity;

            // Gọi hàm setup
            DamageText dt = obj.GetComponent<DamageText>();
            dt.Setup((int)damage, isCritical);
        }


    }

    protected virtual IEnumerator TakeDamageRoutine()
    {
        isTakingDamage = true;

        agent.isStopped = true;

        animator.SetTrigger(
            DamageHash);

        yield return new WaitForSeconds(
            0.5f);

        if (isDead)
            yield break;

        isTakingDamage = false;

        agent.isStopped = false;
        agent.ResetPath();

        currentState =
            Vector3.Distance(
                transform.position,
                player.position)
            <= detectRadius
            ? State.Chase
            : State.Patrol;
    }

    #endregion

    #region DEATH

    public void ForceDieFromBoss()
    {
        if (isDead || !gameObject.activeInHierarchy)
            return;

        Die();
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        StopAllCoroutines();

        isDead = true;

        currentState = State.Dead;

        agent.isStopped = true;
        agent.ResetPath();

        animator.SetBool(WalkHash, false);
        animator.SetBool(RunHash, false);

        animator.SetTrigger(DieHash);

        StartCoroutine(ReturnToPool());
    }

    private IEnumerator ReturnToPool()
    {
        yield return new WaitForSeconds(3f);

        EnemySpawnArea ownerArea = spawnArea;
        int ownerTypeIndex = spawnTypeIndex;

        summonOwner = null;
        ClearSpawnArea();

        if (ownerArea != null)
        {
            ownerArea.NotifyEnemyKilled(
                this,
                ownerTypeIndex);
        }
        else if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnObject(gameObject);
        }
    }

    #endregion

    private void CheckDetection()
    {
        if (player == null ||
            isAttacking ||
            isCoolingDown)
            return;

        currentState =
            Vector3.Distance(
                transform.position,
                player.position)
            <= detectRadius
            ? State.Chase
            : State.Patrol;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        isDead = false;
        isAttacking = false;
        isCoolingDown = false;
        isTakingDamage = false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            transform.position,
            patrolRadius);
    }
}