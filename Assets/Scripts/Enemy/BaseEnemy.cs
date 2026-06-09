using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseEnemy : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform player;

    protected NavMeshAgent agent;

    [Header("Patrol")]
    public float patrolRadius = 4f;
    public float walkSpeed = 2f;

    protected Vector3 patrolPointA;
    protected Vector3 patrolPointB;
    protected Vector3 currentPatrolTarget;
    protected bool goingToA;

    [Header("Detection")]
    public float detectRadius = 10f;

    [Header("Combat")]
    public float runSpeed = 4.5f;
    public float attackRange = 2f;
    public float cooldownTime = 2f;

    [Header("Health")]
    public float maxHP = 100;
    protected float currentHP;

    protected bool isDead;
    protected bool isAttacking;
    protected bool isCoolingDown;
    protected bool isTakingDamage;

    protected Coroutine damageRoutine;

    protected readonly int WalkHash = Animator.StringToHash("Walk");
    protected readonly int RunHash = Animator.StringToHash("Run");
    protected readonly int AttackHash = Animator.StringToHash("Attack");
    protected readonly int DamageHash = Animator.StringToHash("Take Damage");
    protected readonly int DieHash = Animator.StringToHash("Die");

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject p =
                GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }
    }

    protected virtual void OnEnable()
    {
        currentHP = maxHP;
        isDead = false;
        isAttacking = false;
        isCoolingDown = false;
        isTakingDamage = false;

        CreatePatrolPoints();
    }

    protected virtual void Update()
    {
        if (isDead || isTakingDamage)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position);

        if (distance <= detectRadius)
            Chase();
        else
            Patrol();
    }

    #region Patrol

    protected void CreatePatrolPoints()
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

        } while (
            Vector3.Distance(
                patrolPointA,
                patrolPointB) < 5f);

        currentPatrolTarget =
            patrolPointB;
    }

    protected Vector3 GetRandomNavMeshPoint(
        Vector3 center,
        float radius)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPos =
                center +
                Random.insideUnitSphere * radius;

            randomPos.y = center.y;

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

    protected virtual void Patrol()
    {
        agent.speed = walkSpeed;

        animator.SetBool(WalkHash, true);
        animator.SetBool(RunHash, false);

        agent.isStopped = false;

        agent.SetDestination(currentPatrolTarget);

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

    #endregion

    #region Chase

    protected virtual void Chase()
    {
        agent.speed = runSpeed;

        animator.SetBool(WalkHash, false);
        animator.SetBool(RunHash, true);

        agent.isStopped = false;

        agent.SetDestination(player.position);

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

    protected IEnumerator AttackRoutine()
    {
        isAttacking = true;

        agent.isStopped = true;

        animator.SetTrigger(
            AttackHash);

        yield return Attack();

        isAttacking = false;

        yield return StartCoroutine(
            CooldownRoutine());
    }

    protected abstract IEnumerator Attack();

    #endregion

    #region Damage

    public virtual void TakeDamage(
        float damage)
    {
        if (isDead)
            return;

        currentHP -= damage;

        if (currentHP <= 0)
        {
            //Die();
            return;
        }
    }

    protected IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;

        yield return new WaitForSeconds(
            cooldownTime);

        isCoolingDown = false;
    }

    //protected virtual void Die()
    //{
    //    isDead = true;

    //    agent.isStopped = true;

    //    animator.SetBool(WalkHash, false);
    //    animator.SetBool(RunHash, false);

    //    animator.SetTrigger(DieHash);

    //    StartCoroutine(ReturnToPool());
    //}

    //protected IEnumerator ReturnToPool()
    //{
    //    yield return new WaitForSeconds(3f);

    //    EnemyPoolManager.Instance
    //        .EnemyKilled(this);
    //}

    #endregion
}