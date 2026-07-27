using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Respawn Cost")]
    public int goldCost = 100;
    public int diamondCost = 20;

    [Header("Respawn Scene")]
    public string respawnScene = "Village";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RespawnPlayer(PlayerStats playerStats)
    {
        // Trừ vàng
        playerStats.baseStats.gold =
            Mathf.Max(
                0,
                playerStats.baseStats.gold - goldCost);

        // Trừ kim cương
        playerStats.baseStats.diamond =
            Mathf.Max(
                0,
                playerStats.baseStats.diamond - diamondCost);

        // Lưu tiền sau khi đã trừ phí. Không lưu stat tạm trong level.
        PersistentPlayerState.EnsureExists()
            .SaveCurrencyFrom(playerStats);

        // Hồi HP
        playerStats.baseStats.currentHealth =
            playerStats.baseStats.maxHealth;

        // Hồi Mana
        playerStats.baseStats.currentMana =
            playerStats.baseStats.maxMana;

        // Chuyển scene
        SceneManager.LoadScene(respawnScene);
    }
}