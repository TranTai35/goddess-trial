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

        // BỔ SUNG KIỂM TRA: Nếu đang hồi chiêu thì huỷ bỏ, không bắn, không trừ mana
        if (!equippedSpell.CanCast())
        {
            Debug.Log($"{equippedSpell.spellName} đang hồi chiêu!");
            return;
        }

        if (stats.baseStats.currentMana < equippedSpell.manaCost)
        {
            Debug.Log("Không đủ mana!");
            return;
        }

        stats.baseStats.currentMana -= equippedSpell.manaCost;

        equippedSpell.Cast(player);
        equippedSpell.CancelAim();
    }
}