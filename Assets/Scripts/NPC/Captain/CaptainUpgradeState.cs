using UnityEngine;

public static class CaptainUpgradeState
{
    public const int MaxUpgradeCount = 7;

    private static int[] upgradeCounts =
        new int[System.Enum.GetValues(typeof(UpgradeType)).Length];

    /// <summary>
    /// Reset mỗi lần bắt đầu một lần Play mới.
    /// Không reset khi chuyển scene.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        upgradeCounts =
            new int[System.Enum.GetValues(typeof(UpgradeType)).Length];
    }

    public static int GetUpgradeCount(
        UpgradeType type)
    {
        int index = (int)type;

        if (index < 0 ||
            index >= upgradeCounts.Length)
        {
            return 0;
        }

        return upgradeCounts[index];
    }

    public static bool CanUpgrade(
        UpgradeType type)
    {
        return GetUpgradeCount(type) <
               MaxUpgradeCount;
    }

    public static bool RegisterUpgrade(
        UpgradeType type)
    {
        int index = (int)type;

        if (index < 0 ||
            index >= upgradeCounts.Length)
        {
            return false;
        }

        if (upgradeCounts[index] >=
            MaxUpgradeCount)
        {
            return false;
        }

        upgradeCounts[index]++;

        return true;
    }

    public static bool IsMaxed(
        UpgradeType type)
    {
        return GetUpgradeCount(type) >=
               MaxUpgradeCount;
    }

    public static void ResetAll()
    {
        for (int i = 0;
             i < upgradeCounts.Length;
             i++)
        {
            upgradeCounts[i] = 0;
        }
    }
}