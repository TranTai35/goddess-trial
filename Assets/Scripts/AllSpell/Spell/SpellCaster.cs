using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [Header("Equipped Spell")]
    public SpellBase equippedSpell;

    private PlayerController player;
    private PlayerStats stats;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();

        /*
         * Reset cooldown mỗi lần bắt đầu game.
         * Không tạo bản prefab runtime.
         */
        if (equippedSpell != null)
        {
            equippedSpell.ResetCooldown();
        }
    }

    public void CastSpell()
    {
        if (equippedSpell == null)
            return;

        if (!equippedSpell.CanCast())
        {
            Debug.Log(
                $"{equippedSpell.spellName} đang hồi chiêu. " +
                $"Còn {equippedSpell.GetRemainingCooldown():F1} giây.");

            return;
        }

        if (stats == null ||
            stats.baseStats == null)
        {
            Debug.LogError(
                "SpellCaster không tìm thấy PlayerStats.");
            return;
        }

        if (stats.baseStats.currentMana <
            equippedSpell.manaCost)
        {
            Debug.Log("Không đủ mana.");
            return;
        }

        stats.baseStats.currentMana -=
            equippedSpell.manaCost;

        equippedSpell.Cast(player);
    }
}