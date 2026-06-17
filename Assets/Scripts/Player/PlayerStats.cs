using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData baseStats;

    private Animator animator;

    private const string TakeDamageTrigger = "TakeDamage";

    private void Awake()
    {
        animator = GetComponent<Animator>();

        baseStats.currentHealth = baseStats.maxHealth;
        baseStats.currentMana = baseStats.maxMana;
    }



    public void TakeDamage(float damage)
    {
        PlayerController player =
            GetComponent<PlayerController>();

        if (player != null &&
            player.IsInvincible)
        {
            return;
        }

        baseStats.currentHealth -= damage;

        if (animator != null && damage >= 20)
        {
            animator.SetTrigger(TakeDamageTrigger);
        }

        Debug.Log(
            "Player HP: " +
            baseStats.currentHealth);

        if (baseStats.currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        baseStats.currentHealth = 0;

        Debug.Log("PLAYER DEAD");

        // TODO:
        // Play animation chết
        // Game Over UI
        // Respawn
    }
}