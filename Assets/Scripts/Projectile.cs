using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifeTime = 5f;

    private Vector3 direction;
    private float damage;
    private float timer;
    private GameObject owner;

    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }

  
    public void Initialize(Transform target, float damage)
    {
        this.damage = damage;

        timer = lifeTime;

        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y;

        direction =
            (targetPos - transform.position).normalized;
        transform.right = direction;
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
        Debug.Log("Hit: " + other.name);
        if (other.gameObject == owner)
            return;
        // Trúng player
        if (other.CompareTag("Player"))
        {
            PlayerController player =
                other.GetComponent<PlayerController>();

            // Nếu đang Dash hoặc có trạng thái bất tử
            // thì đạn đi xuyên qua luôn
            if (player != null && player.IsInvincible)
            {
                return;
            }

            PlayerStats stats =
                other.GetComponent<PlayerStats>();

            if (stats != null)
            {
                stats.TakeDamage(damage);
            }

            PoolManager.Instance.ReturnObject(gameObject);
            return;        
        }else if (other.CompareTag("Enemy"))
        {
            EnemyController enemy =
                other.GetComponent<EnemyController>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            BossController boss =
            other.GetComponent<BossController>();

            if (boss != null)
            {
                boss.TakeDamage(damage);

               

               
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

    public void InitializeDirection(Vector3 direction, float damage)
    {
        this.direction = direction.normalized;
        this.damage = damage;
        timer = lifeTime;

        transform.forward = this.direction;
    }

    private void OnDisable()
    {
        // Reset lại để lần spawn sau sạch
        direction = Vector3.zero;
        damage = 0f;
        timer = 0f;
    }
}