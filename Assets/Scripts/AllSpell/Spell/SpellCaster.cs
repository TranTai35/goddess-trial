using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [Header("Equipped Spell")]
    public SpellBase equippedSpell;

    private PlayerController player;
    private PlayerStats stats;


    private void Awake()
    {
        player =
            GetComponent<PlayerController>();

        stats =
            GetComponent<PlayerStats>();
    }


    private void Start()
    {
        LoadSavedSpell();
    }


    // =========================================================
    // LOAD SPELL KHI PLAYER ĐƯỢC TẠO
    // =========================================================

    private void LoadSavedSpell()
    {
        if (SpellLoadoutManager.Instance != null &&
            SpellLoadoutManager.Instance.equippedUtilitySpell != null)
        {
            EquipSpell(
                SpellLoadoutManager.Instance
                    .equippedUtilitySpell
            );
        }
        else if (equippedSpell != null)
        {
            equippedSpell.ResetCooldown();
        }
    }


    // =========================================================
    // EQUIP SPELL
    // =========================================================

    public void EquipSpell(
        SpellBase spellPrefab)
    {
        equippedSpell = spellPrefab;

        if (equippedSpell != null)
        {
            equippedSpell.ResetCooldown();

            Debug.Log(
                "Player equipped Utility Spell: "
                +
                equippedSpell.spellName
            );
        }
    }


    // =========================================================
    // CAST
    // =========================================================

    public void CastSpell()
    {
        if (equippedSpell == null)
            return;


        if (!equippedSpell.CanCast())
        {
            Debug.Log(
                $"{equippedSpell.spellName} đang hồi chiêu. " +
                $"Còn {equippedSpell.GetRemainingCooldown():F1} giây."
            );

            return;
        }


        if (stats == null ||
            stats.baseStats == null)
        {
            Debug.LogError(
                "SpellCaster không tìm thấy PlayerStats."
            );

            return;
        }


        if (!stats.TrySpendMana(equippedSpell.manaCost))
        {
            Debug.Log("Không đủ mana.");
            return;
        }

        equippedSpell.Cast(player);
    }
}