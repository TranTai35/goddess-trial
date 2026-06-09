using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData baseStats;

    private void Awake()
    {
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

        Debug.Log(
            "Player HP: " +
            baseStats.currentHealth);
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