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
        if (isDead)
            return;


        isDead = true;


        // =========================================================
        // KHÓA PLAYER
        // =========================================================

        if (playerStats != null)
        {
            PlayerController playerController =
                playerStats.GetComponent<PlayerController>();


            if (playerController != null)
            {
                playerController.SetControlEnabled(
                    false
                );
            }
        }


        // =========================================================
        // KHÓA FEEDBACK
        // =========================================================

        if (FeedbackManager.Instance != null)
        {
            FeedbackManager.Instance.LockGameTime();
        }


        // =========================================================
        // DEATH UI
        // =========================================================

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }


        // =========================================================
        // DEATH ANIMATION
        // =========================================================

        Animator animator =
            playerStats != null
            ? playerStats.GetComponent<Animator>()
            : null;


        if (animator != null)
        {
            animator.SetBool(
                "Moving",
                false
            );

            animator.SetBool(
                "IsAttacking",
                false
            );

            animator.SetTrigger(
                "Die"
            );
        }


        // =========================================================
        // FREEZE GAME
        // =========================================================

        Time.timeScale = 0f;
    }


    // =========================================================
    // RESPAWN
    // =========================================================

    public void Respawn()
    {
        if (FeedbackManager.Instance != null)
        {
            FeedbackManager.Instance.UnlockGameTime();
        }


        Time.timeScale = 1f;


        if (playerStats != null)
        {
            PlayerController playerController =
                playerStats.GetComponent<PlayerController>();


            if (playerController != null)
            {
                playerController.SetControlEnabled(
                    true
                );
            }
        }


        isDead = false;


        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }


        if (
            GameManager.Instance != null &&
            playerStats != null
        )
        {
            GameManager.Instance.RespawnPlayer(
                playerStats
            );
        }
    }
}