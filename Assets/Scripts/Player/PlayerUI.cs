using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public PlayerStats stats;
    public SpellCaster spellCaster;

    public Slider hpBar;
    public Slider manaBar;


    public TMP_Text goldText;
    public TMP_Text diamondText;

    public Image UltimateIcon;
    public Image spellIcon;

    [Header("Fade Image")]
    public Image blackOverlay;

    private void Update()
    {
        hpBar.value =  stats.baseStats.currentHealth /stats.baseStats.maxHealth;
        SetImagePercent(hpBar.value);

        manaBar.value =stats.baseStats.currentMana / stats.baseStats.maxMana;

        goldText.text = "Gold: " + stats.baseStats.gold.ToString();

        diamondText.text = "Diamond: " + stats.baseStats.diamond.ToString();

        if (spellCaster.equippedSpell != null)
        {
            spellIcon.sprite = spellCaster.equippedSpell.icon;
        }
    }

    public void SetImagePercent(float percent)
    {
        float alpha = (1f - percent) * 0.5f;

        Color c = blackOverlay.color;
        c.a = alpha;
        blackOverlay.color = c;
    }
}