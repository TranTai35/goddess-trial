using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;

    public float lifeTime = 5f;

    private Vector3 direction;

    private float damage;

    public void Initialize(Transform target, float damage)
    {
        this.damage = damage;

        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y;

        direction = (targetPos - transform.position).normalized;

        transform.forward = direction;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position +=
            direction *
            speed *
            Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Trúng player -> gây damage rồi hủy
        if (other.CompareTag("Player"))
        {
            PlayerStats stats =
                other.GetComponent<PlayerStats>();

            if (stats != null)
            {
                stats.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // Không phải player thì bỏ qua trigger
        if (other.isTrigger)
            return;

        // Đụng bất kỳ object vật lý nào khác -> hủy
        Destroy(gameObject);
    }
}