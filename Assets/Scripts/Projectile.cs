using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifeTime = 5f;

    private Vector3 direction;
    private float damage;
    private float timer;

    public void Initialize(Transform target, float damage)
    {
        this.damage = damage;

        timer = lifeTime;

        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y;

        direction =
            (targetPos - transform.position).normalized;

        transform.forward = direction;
    }

    private void Update()
    {
        transform.position +=
            direction *
            speed *
            Time.deltaTime;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            PoolManager.Instance.ReturnObject(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Trúng player
        if (other.CompareTag("Player"))
        {
            PlayerStats stats =
                other.GetComponent<PlayerStats>();

            if (stats != null)
            {
                stats.TakeDamage(damage);
            }

            PoolManager.Instance.ReturnObject(gameObject);
            return;
        }

        // Bỏ qua trigger khác
        if (other.isTrigger)
            return;

        // Đụng tường hoặc object khác
        PoolManager.Instance.ReturnObject(gameObject);
    }

    private void OnDisable()
    {
        // Reset lại để lần spawn sau sạch
        direction = Vector3.zero;
        damage = 0f;
        timer = 0f;
    }
}