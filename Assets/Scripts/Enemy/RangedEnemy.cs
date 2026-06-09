using System.Collections;
using UnityEngine;

public class RangedEnemy : BaseEnemy
{
    [Header("Projectile")]
    public Projectile projectilePrefab;

    public Transform firePoint;

    public float projectileDamage = 10f;

    protected override void Chase()
    {
        float distance =
            Vector3.Distance(
                transform.position,
                player.position);

        if (distance > attackRange)
        {
            agent.speed = runSpeed;

            animator.SetBool(WalkHash, false);
            animator.SetBool(RunHash, true);

            agent.isStopped = false;

            agent.SetDestination(
                player.position);
        }
        else
        {
            agent.isStopped = true;

            transform.LookAt(
                new Vector3(
                    player.position.x,
                    transform.position.y,
                    player.position.z));

            if (!isAttacking &&
                !isCoolingDown)
            {
                StartCoroutine(
                    AttackRoutine());
            }
        }
    }

    protected override IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.5f);

        Projectile projectile =
            Instantiate(
                projectilePrefab,
                firePoint.position,
                firePoint.rotation);

        projectile.Initialize(
            player,
            projectileDamage);

        yield return new WaitForSeconds(0.5f);
    }
}