using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    private enum State
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
    public float damage = 10f;

    [Header("Health")]
    public float maxHP = 100f;
    public float currentHP;

    private NavMeshAgent agent;
    private State currentState;
    private Vector3 patrolPointA;
    private Vector3 patrolPointB;
    private Vector3 currentPatrolTarget;
    private Coroutine damageRoutine;

    private bool goingToA;
    private bool isDead;
    private bool isAttacking;
    private bool isCoolingDown;
    private bool isTakingDamage;

    // Animator Hashes
    private readonly int WalkHash = Animator.StringToHash("Walk");
    private readonly int RunHash = Animator.StringToHash("Run");
    private readonly int AttackHash = Animator.StringToHash("Attack");
    private readonly int CooldownHash = Animator.StringToHash("Cooldown");
    private readonly int DamageHash = Animator.StringToHash("Take Damage");
    private readonly int DieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void OnSpawn(Transform targetPlayer)
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
        agent.Warp(transform.position);

        animator.Rebind();
        animator.Update(0f);

        CreatePatrolPoints();

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectRadius)
        {
            currentState = State.Chase;
            animator.SetBool(WalkHash, false);
            animator.SetBool(RunHash, true);
        }
        else
        {
            currentState = State.Patrol;
            animator.SetBool(WalkHash, true);
            animator.SetBool(RunHash, false);
        }

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

        CheckPlayerDetection();
    }

    private void CreatePatrolPoints()
    {
        patrolPointA = GetRandomNavMeshPoint(transform.position, patrolRadius);

        do
        {
            patrolPointB = GetRandomNavMeshPoint(transform.position, patrolRadius);
        }
        while (Vector3.Distance(patrolPointA, patrolPointB) < 5f);

        goingToA = false;
        currentPatrolTarget = patrolPointB;
    }

    private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPos = center + new Vector3(Random.Range(-radius, radius), 0f, Random.Range(-radius, radius));

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return center;
    }

    private void Patrol()
    {
        agent.speed = walkSpeed;
        agent.isStopped = false;
        agent.SetDestination(currentPatrolTarget);

        animator.SetBool(WalkHash, true);
        animator.SetBool(RunHash, false);

        if (Vector3.Distance(transform.position, currentPatrolTarget) < 1f)
        {
            goingToA = !goingToA;
            currentPatrolTarget = goingToA ? patrolPointA : patrolPointB;
        }
    }

    private void CheckPlayerDetection()
    {
        if (player == null || isAttacking || isCoolingDown || isTakingDamage)
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        currentState = (distance <= detectRadius) ? State.Chase : State.Patrol;
    }

    private void ChasePlayer()
    {
        if (player == null)
            return;

        agent.speed = runSpeed;
        agent.isStopped = false;

        animator.SetBool(WalkHash, false);
        animator.SetBool(RunHash, true);

        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 targetPos = player.position + dir * 1f;
        agent.SetDestination(targetPos);

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange && !isAttacking && !isCoolingDown)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        currentState = State.Attack;
        agent.isStopped = true;

        animator.SetBool(RunHash, false);

        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        animator.SetTrigger(AttackHash);

        yield return new WaitForSeconds(0.5f);
        DealDamage();

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;

        StartCoroutine(CooldownRoutine());
    }

    private void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            PlayerStats stats = hit.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage);
            }
            break;
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;
        animator.SetTrigger(CooldownHash);

        yield return new WaitForSeconds(cooldownTime);

        if (isDead)
            yield break;

        isCoolingDown = false;

        float distance = Vector3.Distance(transform.position, player.position);
        currentState = (distance <= detectRadius) ? State.Chase : State.Patrol;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        PlayTakeDamageAnimation();
    }

    private void PlayTakeDamageAnimation()
    {
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
        }
        damageRoutine = StartCoroutine(TakeDamageRoutine());
    }

    private IEnumerator TakeDamageRoutine()
    {
        isTakingDamage = true;
        agent.isStopped = true;

        animator.ResetTrigger(DamageHash);
        animator.SetTrigger(DamageHash);

        yield return new WaitForSeconds(0.5f);

        if (isDead)
            yield break;

        isTakingDamage = false;
        agent.isStopped = false;

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            currentState = (distance <= detectRadius) ? State.Chase : State.Patrol;
        }

        damageRoutine = null;
    }

    private void Die()
    {
        if (isDead)
            return;

        StopAllCoroutines();
        isDead = true;
        currentState = State.Dead;
        agent.isStopped = true;

        animator.SetBool(WalkHash, false);
        animator.SetBool(RunHash, false);
        animator.SetTrigger(DieHash);

        StartCoroutine(ReturnToPool());
    }

    private IEnumerator ReturnToPool()
    {
        yield return new WaitForSeconds(3f);
        EnemyPoolManager.Instance.EnemyKilled(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}