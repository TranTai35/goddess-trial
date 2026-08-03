using System.Collections;
using System.Collections.Generic;
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

    private void Start()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        deathPanel.SetActive(false);
    }

    private void Update()
    {
        HandlePause();
        CheckPlayerDeath();
      
    }

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

        pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);

        settingsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }

    public void ExitToVillage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Village");
    }

    private void CheckPlayerDeath()
    {
        if (isDead)
            return;

        if (playerStats.baseStats.currentHealth <= 0)
        {
            ShowDeathUI();
        }
    }

    public void ShowDeathUI()
    {
        isDead = true;

        deathPanel.SetActive(true);

        Animator animator =
     playerStats.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
    }


    public void Respawn()
    {
        Time.timeScale = 1f;

        GameManager.Instance
            .RespawnPlayer(playerStats);

        deathPanel.SetActive(false);

        isDead = false;
    }
}