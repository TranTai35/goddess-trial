using UnityEngine;

/// <summary>
/// Lưu HP/Mana của một lượt chơi khi Player đi qua
/// nhiều scene chiến đấu.
/// </summary>
public static class PlayerRunState
{
    public static bool IsRunActive
    {
        get;
        private set;
    }


    private static float savedHealth;
    private static float savedMana;


    // =========================================================
    // START NEW RUN - FULL HP/MANA
    // =========================================================

    public static void StartNewRun(
        PlayerStatsData stats)
    {
        if (stats == null)
        {
            return;
        }

        IsRunActive = true;

        savedHealth =
            stats.maxHealth;

        savedMana =
            stats.maxMana;

        ApplyTo(stats);
    }


    // =========================================================
    // START FROM CURRENT HP/MANA
    // =========================================================

    /// <summary>
    /// Dùng khi Load Game.
    ///
    /// Ví dụ save:
    /// HP = 70
    /// Mana = 35
    ///
    /// thì bắt đầu run từ chính 70/35,
    /// không hồi đầy.
    /// </summary>
    public static void StartFromCurrent(
        PlayerStatsData stats)
    {
        if (stats == null)
        {
            return;
        }

        IsRunActive = true;

        savedHealth =
            Mathf.Clamp(
                stats.currentHealth,
                0f,
                stats.maxHealth
            );

        savedMana =
            Mathf.Clamp(
                stats.currentMana,
                0f,
                stats.maxMana
            );

        ApplyTo(stats);
    }


    // =========================================================
    // RESTORE
    // =========================================================

    public static void RestoreOrStart(
        PlayerStatsData stats)
    {
        if (stats == null)
        {
            return;
        }

        if (!IsRunActive)
        {
            StartNewRun(stats);
            return;
        }

        ApplyTo(stats);
    }


    // =========================================================
    // SAVE CURRENT RUN
    // =========================================================

    public static void Save(
        PlayerStatsData stats)
    {
        if (stats == null ||
            !IsRunActive)
        {
            return;
        }

        savedHealth =
            Mathf.Clamp(
                stats.currentHealth,
                0f,
                stats.maxHealth
            );

        savedMana =
            Mathf.Clamp(
                stats.currentMana,
                0f,
                stats.maxMana
            );
    }


    // =========================================================
    // RESET FULL
    // =========================================================

    public static void ResetToFull(
        PlayerStatsData stats)
    {
        IsRunActive = false;

        if (stats == null)
        {
            savedHealth = 0f;
            savedMana = 0f;

            return;
        }

        stats.currentHealth =
            stats.maxHealth;

        stats.currentMana =
            stats.maxMana;

        savedHealth =
            stats.currentHealth;

        savedMana =
            stats.currentMana;
    }


    // =========================================================
    // APPLY
    // =========================================================

    private static void ApplyTo(
        PlayerStatsData stats)
    {
        stats.currentHealth =
            Mathf.Clamp(
                savedHealth,
                0f,
                stats.maxHealth
            );

        stats.currentMana =
            Mathf.Clamp(
                savedMana,
                0f,
                stats.maxMana
            );
    }
}