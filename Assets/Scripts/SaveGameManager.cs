using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveGameManager
{
    // =========================================================
    // SAVE KEYS
    // =========================================================

    private const string HasSaveKey =
        "SAVE_HAS_DATA";

    private const string SceneKey =
        "SAVE_SCENE";

    private const string MaxHealthKey =
        "SAVE_MAX_HEALTH";

    private const string CurrentHealthKey =
        "SAVE_CURRENT_HEALTH";

    private const string MaxManaKey =
        "SAVE_MAX_MANA";

    private const string CurrentManaKey =
        "SAVE_CURRENT_MANA";

    private const string DamageKey =
        "SAVE_DAMAGE";

    private const string MoveSpeedKey =
        "SAVE_MOVE_SPEED";

    private const string GoldKey =
        "SAVE_GOLD";

    private const string DiamondKey =
        "SAVE_DIAMOND";


    // =========================================================
    // DEFAULT NEW GAME VALUES
    // =========================================================

    public const float DefaultMaxHealth = 100f;
    public const float DefaultMaxMana = 100f;
    public const float DefaultDamage = 10f;
    public const float DefaultMoveSpeed = 5f;

    public const int DefaultGold = 500;
    public const int DefaultDiamond = 200;

    public const string DefaultScene = "Village";


    // =========================================================
    // HAS SAVE
    // =========================================================

    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(
            HasSaveKey,
            0
        ) == 1;
    }


    // =========================================================
    // SAVE GAME
    // =========================================================

    public static void SaveGame(
        PlayerStatsData stats,
        string sceneName)
    {
        if (stats == null)
        {
            Debug.LogWarning(
                "SaveGameManager: PlayerStatsData = null."
            );

            return;
        }


        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName =
                SceneManager.GetActiveScene().name;
        }


        PlayerPrefs.SetInt(
            HasSaveKey,
            1
        );


        PlayerPrefs.SetString(
            SceneKey,
            sceneName
        );


        // =====================================================
        // PLAYER STATS
        // =====================================================

        PlayerPrefs.SetFloat(
            MaxHealthKey,
            stats.maxHealth
        );


        PlayerPrefs.SetFloat(
            CurrentHealthKey,
            stats.currentHealth
        );


        PlayerPrefs.SetFloat(
            MaxManaKey,
            stats.maxMana
        );


        PlayerPrefs.SetFloat(
            CurrentManaKey,
            stats.currentMana
        );


        PlayerPrefs.SetFloat(
            DamageKey,
            stats.damage
        );


        PlayerPrefs.SetFloat(
            MoveSpeedKey,
            stats.moveSpeed
        );


        PlayerPrefs.SetInt(
            GoldKey,
            stats.gold
        );


        PlayerPrefs.SetInt(
            DiamondKey,
            stats.diamond
        );


        // =====================================================
        // CAPTAIN UPGRADE COUNTS
        // =====================================================

        foreach (
            UpgradeType type
            in System.Enum.GetValues(
                typeof(UpgradeType)
            )
        )
        {
            PlayerPrefs.SetInt(
                GetCaptainUpgradeKey(type),
                CaptainUpgradeState.GetUpgradeCount(type)
            );
        }


        PlayerPrefs.Save();


        Debug.Log(
            $"SAVE GAME: Scene = {sceneName}, " +
            $"Gold = {stats.gold}, " +
            $"Diamond = {stats.diamond}"
        );
    }


    // =========================================================
    // LOAD STATS
    // =========================================================

    public static void LoadStats(
        PlayerStatsData stats)
    {
        if (!HasSave())
        {
            Debug.LogWarning(
                "SaveGameManager: Không có save để load."
            );

            return;
        }


        if (stats == null)
        {
            Debug.LogWarning(
                "SaveGameManager: PlayerStatsData = null."
            );

            return;
        }


        stats.maxHealth =
            PlayerPrefs.GetFloat(
                MaxHealthKey,
                DefaultMaxHealth
            );


        stats.currentHealth =
            PlayerPrefs.GetFloat(
                CurrentHealthKey,
                stats.maxHealth
            );


        stats.maxMana =
            PlayerPrefs.GetFloat(
                MaxManaKey,
                DefaultMaxMana
            );


        stats.currentMana =
            PlayerPrefs.GetFloat(
                CurrentManaKey,
                stats.maxMana
            );


        stats.damage =
            PlayerPrefs.GetFloat(
                DamageKey,
                DefaultDamage
            );


        stats.moveSpeed =
            PlayerPrefs.GetFloat(
                MoveSpeedKey,
                DefaultMoveSpeed
            );


        stats.gold =
            PlayerPrefs.GetInt(
                GoldKey,
                DefaultGold
            );


        stats.diamond =
            PlayerPrefs.GetInt(
                DiamondKey,
                DefaultDiamond
            );


        // =====================================================
        // CAPTAIN UPGRADES
        // =====================================================

        CaptainUpgradeState.ResetAll();


        foreach (
            UpgradeType type
            in System.Enum.GetValues(
                typeof(UpgradeType)
            )
        )
        {
            int count =
                PlayerPrefs.GetInt(
                    GetCaptainUpgradeKey(type),
                    0
                );


            CaptainUpgradeState.SetUpgradeCount(
                type,
                count
            );
        }


        Debug.Log(
            "Save Game stats đã được load."
        );
    }


    // =========================================================
    // GET SAVED SCENE
    // =========================================================

    public static string GetSavedScene()
    {
        if (!HasSave())
        {
            return DefaultScene;
        }


        return PlayerPrefs.GetString(
            SceneKey,
            DefaultScene
        );
    }


    // =========================================================
    // CREATE NEW GAME
    // =========================================================

    public static void CreateNewGame(
        PlayerStatsData stats)
    {
        /*
         * Xóa save cũ trước.
         */
        DeleteSave();


        if (stats == null)
        {
            Debug.LogError(
                "SaveGameManager: Không thể New Game vì stats = null."
            );

            return;
        }


        // =====================================================
        // RESET PLAYER
        // =====================================================

        stats.maxHealth =
            DefaultMaxHealth;

        stats.currentHealth =
            DefaultMaxHealth;


        stats.maxMana =
            DefaultMaxMana;

        stats.currentMana =
            DefaultMaxMana;


        stats.damage =
            DefaultDamage;


        stats.moveSpeed =
            DefaultMoveSpeed;


        stats.gold =
            DefaultGold;


        stats.diamond =
            DefaultDiamond;


        // =====================================================
        // RESET CAPTAIN
        // =====================================================

        CaptainUpgradeState.ResetAll();


        // =====================================================
        // RESET BATTLE RUN
        // =====================================================

        PlayerRunState.ResetToFull(
            stats
        );


        /*
         * Lưu ngay một save mới ở Village.
         */
        SaveGame(
            stats,
            DefaultScene
        );


        Debug.Log(
            "NEW GAME CREATED."
        );
    }


    // =========================================================
    // DELETE SAVE
    // =========================================================

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(
            HasSaveKey
        );


        PlayerPrefs.DeleteKey(
            SceneKey
        );


        PlayerPrefs.DeleteKey(
            MaxHealthKey
        );


        PlayerPrefs.DeleteKey(
            CurrentHealthKey
        );


        PlayerPrefs.DeleteKey(
            MaxManaKey
        );


        PlayerPrefs.DeleteKey(
            CurrentManaKey
        );


        PlayerPrefs.DeleteKey(
            DamageKey
        );


        PlayerPrefs.DeleteKey(
            MoveSpeedKey
        );


        PlayerPrefs.DeleteKey(
            GoldKey
        );


        PlayerPrefs.DeleteKey(
            DiamondKey
        );


        foreach (
            UpgradeType type
            in System.Enum.GetValues(
                typeof(UpgradeType)
            )
        )
        {
            PlayerPrefs.DeleteKey(
                GetCaptainUpgradeKey(type)
            );
        }


        PlayerPrefs.Save();


        CaptainUpgradeState.ResetAll();


        Debug.Log(
            "Save cũ đã được xóa."
        );
    }


    // =========================================================
    // CAPTAIN KEY
    // =========================================================

    private static string GetCaptainUpgradeKey(
        UpgradeType type)
    {
        return
            "SAVE_CAPTAIN_" +
            type.ToString();
    }
}