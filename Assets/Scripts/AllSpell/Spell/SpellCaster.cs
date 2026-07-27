using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [Header("Equipped Spell Prefab")]
    public SpellBase equippedSpell;

    private PlayerController player;
    private PlayerStats stats;
    private SpellBase spellPrefabReference;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();
        EquipSpell(equippedSpell);
    }

    public void EquipSpell(SpellBase prefab)
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

        if (stats == null || stats.baseStats == null)
        {
            Debug.LogError("SpellCaster không tìm thấy PlayerStats.");
            return;
        }

        if (stats.baseStats.currentMana < equippedSpell.manaCost)
        {
            Debug.Log("Không đủ mana.");
            return;
        }

        stats.baseStats.currentMana -= equippedSpell.manaCost;
        equippedSpell.Cast(player);
    }

    private void OnDestroy()
    {
        if (equippedSpell != null && equippedSpell != spellPrefabReference)
            Destroy(equippedSpell.gameObject);
    }
}
