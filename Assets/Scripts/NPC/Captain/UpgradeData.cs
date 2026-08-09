using UnityEngine;

public enum UpgradeType
{
    MaxHealth,
    MaxMana,
    Damage,
    MoveSpeed,
    ManaRegen
}

[System.Serializable]
public class UpgradeData
{
    public UpgradeType type;

    [Tooltip("Level dùng để tính giá và giá trị nâng.")]
    public int level = 1;

    public int baseCost;

    public void SetDefault()
    {
        /*
         * Không được reset level = 1 ở đây nữa.
         * Vì OnValidate có thể chạy nhiều lần trong Editor.
         */

        if (level < 1)
        {
            level = 1;
        }

        switch (type)
        {
            case UpgradeType.MaxHealth:
                baseCost = 100;
                break;

            case UpgradeType.MaxMana:
                baseCost = 80;
                break;

            case UpgradeType.Damage:
                baseCost = 150;
                break;

            case UpgradeType.MoveSpeed:
                baseCost = 200;
                break;

            case UpgradeType.ManaRegen:
                baseCost = 120;
                break;
        }
    }

    /// <summary>
    /// Đồng bộ level với số lần đã nâng.
    ///
    /// 0 lần nâng -> level 1
    /// 1 lần nâng -> level 2
    /// ...
    /// </summary>
    public void SetUpgradeCount(
        int upgradeCount)
    {
        level =
            Mathf.Max(0, upgradeCount) + 1;
    }

    public int GetCost()
    {
        return Mathf.RoundToInt(
            baseCost *
            Mathf.Pow(1.4f, level)
        );
    }

    public float GetValuePerLevel()
    {
        switch (type)
        {
            case UpgradeType.MaxHealth:
                return 50f + level * 10f;

            case UpgradeType.MaxMana:
                return 25f + level * 5f;

            case UpgradeType.Damage:
                return 10f + level * 2f;

            case UpgradeType.MoveSpeed:
                return 0.2f;

            case UpgradeType.ManaRegen:
                return 0.5f + level * 0.1f;

            default:
                return 0f;
        }
    }
}