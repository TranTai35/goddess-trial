using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 10f;

    private Transform target;

    public void Initialize(
        Transform targetTransform,
        float projectileDamage)
    {
        target = targetTransform;
        damage = projectileDamage;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime);

        transform.LookAt(
            target.position);

        if (Vector3.Distance(
            transform.position,
            target.position) < 0.3f)
        {
            PlayerStats stats =
                target.GetComponent<PlayerStats>();

            if (stats != null)
            {
                stats.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}