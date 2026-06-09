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
    public float cooldownTime = 2f;

    [Header("Health")]
    public float maxHP = 100f;

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

    public virtual void OnSpawn(
        Transform targetPlayer)
    {
        StopAllCoroutines();

        player = targetPlayer;

        currentHP = maxHP;

        isDead = false;
        isAttacking = false;
        isCoolingDown = false;
        isTakingDamage = false;

        agent.enabled = true;
        agent.isStopped = false;

        animator.Rebind();
        animator.Update(0);

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

    protected IEnumerator CooldownRoutine()
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

    public virtual void TakeDamage(
        float damage)
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
    }

    private IEnumerator TakeDamageRoutine()
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

    protected virtual void Die()
    {
        if (isDead)
            return;

        StopAllCoroutines();

        isDead = true;

        currentState = State.Dead;

        agent.isStopped = true;

        animator.SetBool(
            WalkHash,
            false);

        animator.SetBool(
            RunHash,
            false);

        animator.SetTrigger(
            DieHash);

        StartCoroutine(
            ReturnToPool());
    }

    private IEnumerator ReturnToPool()
    {
        yield return new WaitForSeconds(3f);

        EnemyPoolManager
            .Instance
            .EnemyKilled(this);
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