using UnityEngine;

public class AttackSpellCaster : MonoBehaviour
{
    public AttackSpellBase equippedSpell;

    private PlayerController player;
    private PlayerStats stats;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();
    }

    public void StartAim()
    {
        equippedSpell?.StartAim(player);
    }

    public void CancelAim()
    {
        equippedSpell?.CancelAim();
    }

    public void CastSpell()
    {
        if (equippedSpell == null)
            return;

        if (stats.baseStats.currentMana < equippedSpell.manaCost)
            return;

        stats.baseStats.currentMana -= equippedSpell.manaCost;

        equippedSpell.Cast(player);

        equippedSpell.CancelAim();
    }
}