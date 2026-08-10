using System.Collections.Generic;
using UnityEngine;

public class CaptainNPC : NPC
{
    [SerializeField]
    private List<UpgradeData> upgrades;


    private void Awake()
    {
        SyncUpgradeLevels();
    }

    private void OnValidate()
    {
        if (upgrades == null)
            return;

        foreach (UpgradeData data in upgrades)
        {
            if (data == null)
                continue;

            data.SetDefault();
        }
    }

    private void SyncUpgradeLevels()
    {
        if (upgrades == null)
            return;

        foreach (UpgradeData data in upgrades)
        {
            if (data == null)
                continue;

            int upgradeCount =
                CaptainUpgradeState.GetUpgradeCount(
                    data.type
                );

            data.SetUpgradeCount(
                upgradeCount
            );
        }
    }

    public UpgradeData GetUpgradeData(
        UpgradeType type)
    {
        if (upgrades == null)
            return null;

        UpgradeData data =
            upgrades.Find(
                x => x != null &&
                     x.type == type
            );

        if (data != null)
        {
            int upgradeCount =
                CaptainUpgradeState.GetUpgradeCount(
                    type
                );

            data.SetUpgradeCount(
                upgradeCount
            );
        }

        return data;
    }

    public bool UpgradeStat(
        UpgradeType type,
        PlayerStats player)
    {
        if (player == null ||
            player.baseStats == null)
        {
            return false;
        }

        if (!CaptainUpgradeState.CanUpgrade(type))
        {
            Debug.Log(
                $"{type} đã đạt giới hạn " +
                $"{CaptainUpgradeState.MaxUpgradeCount} lần nâng."
            );

            return false;
        }

        UpgradeData data =
            GetUpgradeData(type);

        if (data == null)
            return false;

        int cost =
            data.GetCost();

        if (player.baseStats.gold < cost)
        {
            Debug.Log(
                $"Không đủ Gold để nâng {type}."
            );

            return false;
        }

        player.baseStats.gold -= cost;

        float value =
            data.GetValuePerLevel();

        switch (type)
        {
            case UpgradeType.MaxHealth:

                player.baseStats.maxHealth +=
                    value;

                // Khi nâng Max Health thì hồi đầy luôn
                player.baseStats.currentHealth =
                    player.baseStats.maxHealth;

                break;


            case UpgradeType.MaxMana:

                player.baseStats.maxMana +=
                    value;

                // Khi nâng Max Mana thì hồi đầy luôn
                player.baseStats.currentMana =
                    player.baseStats.maxMana;

                break;


            case UpgradeType.Damage:

                player.baseStats.damage +=
                    value;

                break;


            case UpgradeType.MoveSpeed:

                player.baseStats.moveSpeed +=
                    value;

                PlayerController controller =
                    player.GetComponent<PlayerController>();

                if (controller != null)
                {
                    controller.UpdateMoveSpeed();
                }

                break;
        }

        CaptainUpgradeState.RegisterUpgrade(
            type
        );

        int newUpgradeCount =
            CaptainUpgradeState.GetUpgradeCount(
                type
            );

        data.SetUpgradeCount(
            newUpgradeCount
        );

        Debug.Log(
            $"{type}: đã nâng " +
            $"{newUpgradeCount}/" +
            $"{CaptainUpgradeState.MaxUpgradeCount}"
        );

        return true;
    }


}
