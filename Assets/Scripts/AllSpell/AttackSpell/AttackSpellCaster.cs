using UnityEngine;

public class AttackSpellCaster : MonoBehaviour
{
    [Header("Attack Spell Prefab")]
    public AttackSpellBase equippedSpell;

    private PlayerController player;
    private PlayerStats stats;
    private AttackSpellBase spellPrefabReference;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();
        EquipSpell(equippedSpell);
    }

    public void EquipSpell(AttackSpellBase prefab)
    {
        if (equippedSpell != null && equippedSpell != spellPrefabReference)
            Destroy(equippedSpell.gameObject);

        equippedSpell = null;
        spellPrefabReference = prefab;

        if (prefab == null)
            return;

        equippedSpell = Instantiate(prefab, transform);
        equippedSpell.name = prefab.name + "_Runtime";
        equippedSpell.ResetCooldown();
        equippedSpell.gameObject.hideFlags = HideFlags.DontSave;
    }

    public void StartAim()
    {
        if (equippedSpell == null || !equippedSpell.CanCast())
            return;

        equippedSpell.StartAim(player);
    }

    public void CancelAim()
    {
        equippedSpell?.CancelAim();
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
            equippedSpell.CancelAim();
            return;
        }

        if (stats == null || stats.baseStats == null)
        {
            Debug.LogError("AttackSpellCaster: Không tìm thấy PlayerStats.");
            equippedSpell.CancelAim();
            return;
        }

        if (stats.baseStats.currentMana < equippedSpell.manaCost)
        {
            Debug.Log("Không đủ mana!");
            equippedSpell.CancelAim();
            return;
        }

        stats.baseStats.currentMana -= equippedSpell.manaCost;
        equippedSpell.Cast(player);
        equippedSpell.CancelAim();
    }

    private void OnDestroy()
    {
        if (equippedSpell != null && equippedSpell != spellPrefabReference)
            Destroy(equippedSpell.gameObject);
    }
}
