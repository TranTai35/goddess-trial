using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    public SpellBase equippedSpell;

    private PlayerController player;
    private PlayerStats stats;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();
    }

    public void CastSpell()
    {
        if (equippedSpell == null)
            return;

        if (stats.baseStats.currentMana <
            equippedSpell.manaCost)
        {
            Debug.Log("Not enough mana");
            return;
        }

        stats.baseStats.currentMana -=
            equippedSpell.manaCost;

        equippedSpell.Cast(player);
    }
}