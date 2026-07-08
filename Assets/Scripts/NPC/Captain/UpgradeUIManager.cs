using TMPro;
using UnityEngine;

public class UpgradeUIManager : MonoBehaviour
{
    public CaptainNPC captain;
    public PlayerStats player;

    [Header("Gold")]
    public TMP_Text goldText;

    [Header("Health")]
    //public TMP_Text hpLevel;
    public TMP_Text hpCost;
    public TMP_Text hpValue;

    [Header("Mana")]
    //public TMP_Text manaLevel;
    public TMP_Text manaCost;
    public TMP_Text manaValue;

    [Header("Mana Regen")]
    //public TMP_Text regenLevel;
    public TMP_Text regenCost;
    public TMP_Text regenValue;

    [Header("Damage")]
    //public TMP_Text damageLevel;
    public TMP_Text damageCost;
    public TMP_Text damageValue;

    [Header("Speed")]
    //public TMP_Text speedLevel;
    public TMP_Text speedCost;
    public TMP_Text speedValue;

   

    private void OnEnable()
    {
        Debug.Log("UpgradeUIManager Enabled");
        RefreshUI();
    }

    public void RefreshUI()
    {
        goldText.text =
            player.baseStats.gold.ToString();

        UpdateStat(
            UpgradeType.MaxHealth,
            hpCost,
            hpValue);

        UpdateStat(
            UpgradeType.MaxMana,
            manaCost,
            manaValue);

        UpdateStat(
            UpgradeType.Damage,
            damageCost,
            damageValue);

        UpdateStat(
            UpgradeType.MoveSpeed,
            speedCost,
            speedValue);

        UpdateStat(
            UpgradeType.ManaRegen,
            regenCost,
            regenValue);
    }

    private void UpdateStat(
        UpgradeType type,
        TMP_Text cost,
        TMP_Text value)
    {
        UpgradeData data =
            captain.GetUpgradeData(type);

        cost.text =
            $"{data.GetCost()} Gold";

        value.text =
            $"+{data.GetValuePerLevel()}";
    }

    public void UpgradeHealth()
    {
        
        captain.UpgradeStat(
            UpgradeType.MaxHealth,
            player);

        RefreshUI();
    }

    public void UpgradeMana()
    {
        captain.UpgradeStat(
            UpgradeType.MaxMana,
            player);

        RefreshUI();
    }

    public void UpgradeDamage()
    {
        captain.UpgradeStat(
            UpgradeType.Damage,
            player);

        RefreshUI();
    }

    public void UpgradeSpeed()
    {
        captain.UpgradeStat(
            UpgradeType.MoveSpeed,
            player);

        RefreshUI();
    }

    public void UpgradeManaRegen()
    {
        captain.UpgradeStat(
            UpgradeType.ManaRegen,
            player);

        RefreshUI();
    }
}