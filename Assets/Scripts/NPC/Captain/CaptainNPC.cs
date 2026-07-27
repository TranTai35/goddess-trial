using System.Collections.Generic;
using UnityEngine;

public class CaptainNPC : NPC
{
    [SerializeField]
    private List<UpgradeData> upgrades;

    private void OnValidate()
    {
        foreach (UpgradeData data in upgrades)
        {
            data.SetDefault();
        }
    }
    public UpgradeData GetUpgradeData(
        UpgradeType type)
    {
        return upgrades.Find(
            x => x.type == type);
    }

    public bool UpgradeStat(
        UpgradeType type,
        PlayerStats player)
    {
        UpgradeData data =
            GetUpgradeData(type);

        if (data == null)
            return false;

        int cost =
            data.GetCost();

        if (player.baseStats.gold < cost)
            return false;

        player.baseStats.gold -= cost;

        float value =
            data.GetValuePerLevel();

        switch (type)
        {
            case UpgradeType.MaxHealth:
                player.baseStats.maxHealth += value;
                break;

            case UpgradeType.MaxMana:
                player.baseStats.maxMana += value;
                break;

            case UpgradeType.Damage:
                player.baseStats.damage += value;
                break;

            case UpgradeType.MoveSpeed:
                player.baseStats.moveSpeed += value;

                PlayerController controller = player.GetComponent<PlayerController>();

                controller.UpdateMoveSpeed();
                break;

            case UpgradeType.ManaRegen:
                player.baseStats.manaRegen += value;
                break;
        }

        data.level++;

        // Upgrade ở Captain là chỉ số vĩnh viễn.
        PersistentPlayerState.EnsureExists()
            .SavePermanentStatsFrom(player);

        return true;
    }
}