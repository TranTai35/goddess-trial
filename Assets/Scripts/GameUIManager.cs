using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject deathPanel;


    [Header("Stats")]
    public PlayerStats playerStats;


    private bool isPaused;
    private bool isDead;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }


        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }


        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        HandlePause();

        CheckPlayerDeath();
    }


    // =========================================================
    // PAUSE
    // =========================================================

    private void HandlePause()
    {
        if (isDead)
            return;


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }


    public void PauseGame()
    {
        isPaused = true;


        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }


        Time.timeScale = 0f;
    }


    public void ResumeGame()
    {
        isPaused = false;


        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }


        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }


        Time.timeScale = 1f;
    }


    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }


    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }


    // =========================================================
    // EXIT TO MENU + SAVE
    // =========================================================

    public void ExitToMenu()
    {
        Time.timeScale = 1f;


        /*
         * SAVE GAME trước khi về MainMenu.
         */
        if (
            playerStats != null &&
            playerStats.baseStats != null
        )
        {
            string sceneName =
                SceneManager.GetActiveScene().name;


            SaveGameManager.SaveGame(
                playerStats.baseStats,
                sceneName
            );
        }


        SceneManager.LoadScene(
            "MainMenu"
        );
    }


    // =========================================================
    // EXIT TO VILLAGE
    // =========================================================

    public void ExitToVillage()
    {
        Time.timeScale = 1f;


        if (playerStats != null)
        {
            PlayerRunState.ResetToFull(
                playerStats.baseStats
            );


            /*
             * Về Village cũng cập nhật save.
             */
            SaveGameManager.SaveGame(
                playerStats.baseStats,
                "Village"
            );
        }


        SceneManager.LoadScene(
            "Village"
        );
    }


    // =========================================================
    // PLAYER DEATH
    // =========================================================

    private void CheckPlayerDeath()
    {
        if (isDead)
            return;


        if (
            playerStats == null ||
            playerStats.baseStats == null
        )
        {
            return;
        }


        if (
            playerStats.baseStats.currentHealth
            <=
            0
        )
        {
            ShowDeathUI();
        }
    }


    public void ShowDeathUI()
    {
        isDead = true;


        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }


        Animator animator =
            playerStats.GetComponent<Animator>();


        if (animator != null)
        {
            animator.SetTrigger(
                "Die"
            );
        }


        Time.timeScale = 0f;
    }


    // =========================================================
    // RESPAWN
    // =========================================================

    public void Respawn()
    {
        Time.timeScale = 1f;


        GameManager.Instance
            .RespawnPlayer(
                playerStats
            );


        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }


        isDead = false;
    }
}