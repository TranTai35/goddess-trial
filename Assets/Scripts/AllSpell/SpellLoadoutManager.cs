using UnityEngine;

public class SpellLoadoutManager : MonoBehaviour
{
    public static SpellLoadoutManager Instance;

    [Header("Current Equipped Spells")]
    public SpellBase equippedUtilitySpell;
    public AttackSpellBase equippedAttackSpell;


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
    // SAVE UTILITY SPELL
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
    // SAVE ATTACK SPELL
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
}