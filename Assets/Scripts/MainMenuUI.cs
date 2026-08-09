using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject settingPanel;

    [SerializeField]
    private GameObject newGamePanel;


    [Header("Player Data")]
    [Tooltip(
        "Kéo PlayerStatsData ScriptableObject của Player vào đây."
    )]
    [SerializeField]
    private PlayerStatsData playerStatsData;


    [Header("Scene")]
    [SerializeField]
    private string newGameSceneName =
        "Village";


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        Time.timeScale = 1f;


        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }


        if (newGamePanel != null)
        {
            newGamePanel.SetActive(false);
        }
    }


    // =========================================================
    // NEW GAME BUTTON
    // =========================================================

    public void NewGame()
    {
        /*
         * Không xóa save ngay.
         *
         * Chỉ hiện bảng xác nhận.
         */
        if (newGamePanel != null)
        {
            newGamePanel.SetActive(true);
            newGamePanel.transform.SetAsLastSibling();
        }
    }


    // =========================================================
    // NEW GAME - NO
    // =========================================================

    public void NewGameNo()
    {
        if (newGamePanel != null)
        {
            newGamePanel.SetActive(false);
        }
    }


    // =========================================================
    // NEW GAME - YES
    // =========================================================

    public void NewGameYes()
    {
        if (playerStatsData == null)
        {
            Debug.LogError(
                "MainMenuUI: Chưa gắn PlayerStatsData."
            );

            return;
        }


        /*
         * Tạo game mới.
         *
         * Save cũ sẽ bị xóa và ghi đè bằng
         * save mới.
         */
        SaveGameManager.CreateNewGame(
            playerStatsData
        );


        Time.timeScale =
            1f;


        SceneManager.LoadScene(
            newGameSceneName
        );
    }


    // =========================================================
    // LOAD GAME
    // =========================================================

    public void LoadGame()
    {
        /*
         * CHƯA TỪNG NEW GAME
         *
         * Load Game sẽ mở panel New Game.
         */
        if (!SaveGameManager.HasSave())
        {
            NewGame();

            return;
        }


        if (playerStatsData == null)
        {
            Debug.LogError(
                "MainMenuUI: Chưa gắn PlayerStatsData."
            );

            return;
        }


        // =====================================================
        // LOAD PLAYER DATA
        // =====================================================

        SaveGameManager.LoadStats(
            playerStatsData
        );


        // =====================================================
        // GET LAST SCENE
        // =====================================================

        string savedScene =
            SaveGameManager.GetSavedScene();


        if (string.IsNullOrEmpty(savedScene))
        {
            savedScene =
                newGameSceneName;
        }


        Time.timeScale =
            1f;


        SceneManager.LoadScene(
            savedScene
        );
    }


    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
        }
    }


    public void CloseSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
    }


    // =========================================================
    // EXIT
    // =========================================================

    public void ExitGame()
    {
        Application.Quit();
    }
}