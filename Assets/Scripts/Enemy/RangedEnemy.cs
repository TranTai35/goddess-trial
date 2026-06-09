using System.Collections;
using UnityEngine;

public class RangedEnemy : EnemyController
{
    [Header("Projectile")]
    public Projectile projectilePrefab;

    public Transform firePoint;

    public float projectileDamage = 10f;

    //protected override void ChasePlayer()
    //{
    //    if (player == null)
    //        return;

    //    float distance =
    //        Vector3.Distance(
    //            transform.position,
    //            player.position);

    //    if (distance > attackRange)
    //    {
    //        agent.speed = runSpeed;

    //        animator.SetBool(WalkHash, false);
    //        animator.SetBool(RunHash, true);

    //        agent.isStopped = false;

    //        agent.SetDestination(
    //            player.position);
    //    }
    //    else
    //    {
    //        agent.isStopped = true;

    //        animator.SetBool(WalkHash, false);
    //        animator.SetBool(RunHash, false);

    //        Vector3 lookPos =
    //            player.position;

    //        lookPos.y =
    //            transform.position.y;

    //        transform.LookAt(lookPos);

    //        if (!isAttacking &&
    //            !isCoolingDown)
    //        {
    //            StartCoroutine(
    //                AttackRoutine());
    //        }
    //    }
    //}

    protected override IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.5f);

        if (projectilePrefab != null &&
            firePoint != null)
        {
            Projectile projectile =
                Instantiate(
                    projectilePrefab,
                    firePoint.position,
                    firePoint.rotation);

            projectile.Initialize(
                player,
                projectileDamage);
        }

        yield return new WaitForSeconds(0.5f);
    }
}