using UnityEngine;
using System.Collections; // Nhớ thêm thư viện này

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData baseStats;
    private Animator animator;
    private const string TakeDamageTrigger = "TakeDamage";

    // Biến quản lý trạng thái nhận sát thương
    private bool isTakingDamage = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        baseStats.currentHealth = baseStats.maxHealth;
        baseStats.currentMana = baseStats.maxMana;
    }

    // ... code RegenerateMana giữ nguyên ...

    public void TakeDamage(float damage)
    {
        PlayerController player = GetComponent<PlayerController>();

        if (player != null && player.IsInvincible)
            return;

        baseStats.currentHealth -= damage;
        Debug.Log("Player HP: " + baseStats.currentHealth);

        // Logic sửa lại: Nhận bất kỳ sát thương nào cũng kiểm tra trigger
        if (!isTakingDamage && animator != null)
        {
            StartCoroutine(PlayDamageAnimationRoutine());
        }

        if (baseStats.currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator PlayDamageAnimationRoutine()
    {
        isTakingDamage = true; // Khóa không cho bật animation thêm

        animator.SetTrigger(TakeDamageTrigger); // Bật animation

        yield return new WaitForSeconds(1f); // Chờ 1 giây

        isTakingDamage = false; // Mở khóa cho lần sau
    }

    private void Die()
    {
        baseStats.currentHealth = 0;
        Debug.Log("PLAYER DEAD");
    }
}