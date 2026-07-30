using System.Collections.Generic;
using UnityEngine;

public class SpellLoadoutManager : MonoBehaviour
{
    public static SpellLoadoutManager Instance;

    [Header("Current Equipped Spells")]
    public SpellBase equippedUtilitySpell;
    public AttackSpellBase equippedAttackSpell;

    // Những spell đã mua trong phiên chơi hiện tại
    private HashSet<string> purchasedSpells =
        new HashSet<string>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // EQUIPPED UTILITY
    // =========================================================

    public void SetUtilitySpell(
        SpellBase spellPrefab)
    {
        equippedUtilitySpell = spellPrefab;

        Debug.Log(
            "Saved Utility Spell: " +
            (spellPrefab != null
                ? spellPrefab.spellName
                : "None")
        );
    }


    // =========================================================
    // EQUIPPED ATTACK
    // =========================================================

    public void SetAttackSpell(
        AttackSpellBase spellPrefab)
    {
        equippedAttackSpell = spellPrefab;

        Debug.Log(
            "Saved Attack Spell: " +
            (spellPrefab != null
                ? spellPrefab.spellName
                : "None")
        );
    }


    // =========================================================
    // PURCHASED SPELL
    // =========================================================

    public void MarkSpellBought(
        SpellLearnData data,
        SpellType type)
    {
        if (data == null)
            return;

        string id =
            GetSpellID(data, type);

        purchasedSpells.Add(id);

        Debug.Log(
            "Saved purchased spell: " +
            id
        );
    }


    public bool IsSpellBought(
        SpellLearnData data,
        SpellType type)
    {
        if (data == null)
            return false;

        string id =
            GetSpellID(data, type);

        return purchasedSpells.Contains(id);
    }


    // =========================================================
    // SPELL ID
    // =========================================================

    private string GetSpellID(
        SpellLearnData data,
        SpellType type)
    {
        return type.ToString()
            + ":"
            + data.spellName;
    }


    // =========================================================
    // CHECK EQUIPPED
    // =========================================================

    public bool IsUtilityEquipped(
        SpellLearnData data)
    {
        if (data == null ||
            data.utilitySpellPrefab == null ||
            equippedUtilitySpell == null)
        {
            return false;
        }

        return
            equippedUtilitySpell.spellName
            ==
            data.utilitySpellPrefab.spellName;
    }


    public bool IsAttackEquipped(
        SpellLearnData data)
    {
        if (data == null ||
            data.attackSpellPrefab == null ||
            equippedAttackSpell == null)
        {
            return false;
        }

        return
            equippedAttackSpell.spellName
            ==
            data.attackSpellPrefab.spellName;
    }
}