using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIManager : MonoBehaviour
{
    public CaptainNPC captain;
    public PlayerStats player;


    [Header("Gold")]
    public TMP_Text goldText;


    // =========================================================
    // HEALTH
    // =========================================================

    [Header("Health")]
    public TMP_Text hpCost;
    public TMP_Text hpValue;

    public Button hpButton;


    // =========================================================
    // MANA
    // =========================================================

    [Header("Mana")]
    public TMP_Text manaCost;
    public TMP_Text manaValue;

    public Button manaButton;


    // =========================================================
    // DAMAGE
    // =========================================================

    [Header("Damage")]
    public TMP_Text damageCost;
    public TMP_Text damageValue;

    public Button damageButton;


    // =========================================================
    // SPEED
    // =========================================================

    [Header("Speed")]
    public TMP_Text speedCost;
    public TMP_Text speedValue;

    public Button speedButton;


    // =========================================================
    // BUTTON COLOR
    // =========================================================

    [Header("Max Upgrade Button")]

    [Tooltip("Màu Button khi đã nâng đủ 7 lần.")]
    [SerializeField]
    private Color maxUpgradeButtonColor =
        new Color(
            0.35f,
            0.35f,
            0.35f,
            1f
        );


    private void OnEnable()
    {
        RefreshUI();
    }


    // =========================================================
    // REFRESH
    // =========================================================

    public void RefreshUI()
    {
        if (captain == null ||
            player == null ||
            player.baseStats == null)
        {
            return;
        }

        if (goldText != null)
        {
            goldText.text =
                player.baseStats.gold.ToString();
        }


        UpdateStat(
            UpgradeType.MaxHealth,
            hpCost,
            hpValue,
            hpButton
        );


        UpdateStat(
            UpgradeType.MaxMana,
            manaCost,
            manaValue,
            manaButton
        );


        UpdateStat(
            UpgradeType.Damage,
            damageCost,
            damageValue,
            damageButton
        );


        UpdateStat(
            UpgradeType.MoveSpeed,
            speedCost,
            speedValue,
            speedButton
        );
    }


    // =========================================================
    // UPDATE ONE STAT
    // =========================================================

    private void UpdateStat(
        UpgradeType type,
        TMP_Text cost,
        TMP_Text value,
        Button button)
    {
        UpgradeData data =
            captain.GetUpgradeData(type);

        if (data == null)
            return;


        int upgradeCount =
            CaptainUpgradeState.GetUpgradeCount(
                type
            );


        bool isMaxed =
            CaptainUpgradeState.IsMaxed(
                type
            );


        if (isMaxed)
        {
            if (cost != null)
            {
                cost.text = "MAX";
            }

            if (value != null)
            {
                value.text =
                    $"{upgradeCount}/" +
                    $"{CaptainUpgradeState.MaxUpgradeCount}";
            }

            SetButtonMaxed(
                button,
                true
            );

            return;
        }


        if (cost != null)
        {
            cost.text =
                $"{data.GetCost()} Gold";
        }

        if (value != null)
        {
            value.text =
                $"+{data.GetValuePerLevel()} " +
                $"({upgradeCount}/" +
                $"{CaptainUpgradeState.MaxUpgradeCount})";
        }

        SetButtonMaxed(
            button,
            false
        );
    }


    // =========================================================
    // BUTTON STATE
    // =========================================================

    private void SetButtonMaxed(
        Button button,
        bool isMaxed)
    {
        if (button == null)
            return;


        ColorBlock colors =
            button.colors;


        colors.disabledColor =
            maxUpgradeButtonColor;


        button.colors =
            colors;


        button.interactable =
            !isMaxed;
    }


    // =========================================================
    // HEALTH
    // =========================================================

    public void UpgradeHealth()
    {
        captain.UpgradeStat(
            UpgradeType.MaxHealth,
            player
        );

        RefreshUI();
    }


    // =========================================================
    // MANA
    // =========================================================

    public void UpgradeMana()
    {
        captain.UpgradeStat(
            UpgradeType.MaxMana,
            player
        );

        RefreshUI();
    }


    // =========================================================
    // DAMAGE
    // =========================================================

    public void UpgradeDamage()
    {
        captain.UpgradeStat(
            UpgradeType.Damage,
            player
        );

        RefreshUI();
    }


    // =========================================================
    // SPEED
    // =========================================================

    public void UpgradeSpeed()
    {
        captain.UpgradeStat(
            UpgradeType.MoveSpeed,
            player
        );

        RefreshUI();
    }


}
