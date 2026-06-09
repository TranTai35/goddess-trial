using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCMove : MonoBehaviour
{
    [Header("Movement")]
    public float wanderRadius = 10f;
    public float waitTime = 2f;

    [Header("Animation")]
    public Animator animator;

    [Header("Stuck Handling")]
    public float maxMoveTime = 5f;

    private float moveTimer;

    private NavMeshAgent agent;
    private float timer;

    private const string IsMoving = "Moving";

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();

        SetNewDestination();
    }

    private void Update()
    {
        bool isMoving = !agent.pathPending && agent.remainingDistance > agent.stoppingDistance + 0.2f;
        animator.SetBool(IsMoving, isMoving);

        // Nếu đang đi tới destination
        if (isMoving)
        {
            moveTimer += Time.deltaTime;

            // ❗ Đi quá lâu mà chưa tới -> reset destination
            if (moveTimer >= maxMoveTime)
            {
                moveTimer = 0f;
                agent.ResetPath();      // dừng lại
                SetNewDestination();    // chọn điểm mới
                return;
            }
        }
        else
        {
            moveTimer = 0f; // reset khi đứng yên / đã tới nơi
        }

        // Đến nơi rồi thì đợi rồi đi tiếp
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            timer += Time.deltaTime;

            if (timer >= waitTime)
            {
                timer = 0f;
                SetNewDestination();
            }
        }
    }

    private void SetNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere.normalized * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(
                randomDirection,
                out NavMeshHit hit,
                10f,
                NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
