using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Respawn Cost")]
    public int goldCost = 30;
    public int diamondCost = 10;

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

        // Kết thúc lượt chơi và hồi đầy HP/Mana.
        PlayerRunState.ResetToFull(
            playerStats.baseStats);

        // Chuyển scene
        SceneManager.LoadScene(respawnScene);
    }
}