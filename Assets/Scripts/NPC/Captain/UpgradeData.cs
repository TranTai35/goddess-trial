using UnityEngine;

public enum UpgradeType
{
    MaxHealth,
    MaxMana,
    Damage,
    MoveSpeed
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
        }
    }

    public void SetUpgradeCount(int upgradeCount)
    {
        level = Mathf.Max(0, upgradeCount) + 1;
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

            default:
                return 0f;
        }
    }


}
