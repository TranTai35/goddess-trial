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
    // LOAD SPELL KHI SANG SCENE MỚI
    // =========================================================

    private void LoadSavedSpell()
    {
        if (SpellLoadoutManager.Instance != null &&
            SpellLoadoutManager.Instance.equippedAttackSpell != null)
        {
            EquipSpell(
                SpellLoadoutManager.Instance
                    .equippedAttackSpell
            );
        }
        else if (equippedSpell != null)
        {
            AttackSpellBase defaultSpell =
                equippedSpell;

            equippedSpell = null;

            EquipSpell(defaultSpell);
        }
    }


    // =========================================================
    // EQUIP ATTACK SPELL
    // =========================================================

    public void EquipSpell(
        AttackSpellBase spellPrefab)
    {
        if (spellPrefab == null)
            return;


        // -----------------------------------------------------
        // HỦY SPELL RUNTIME CŨ
        // -----------------------------------------------------

        if (equippedSpell != null &&
            equippedSpell != spellPrefabReference)
        {
            equippedSpell.CancelAim();

            Destroy(
                equippedSpell.gameObject
            );
        }


        // -----------------------------------------------------
        // LƯU PREFAB GỐC
        // -----------------------------------------------------

        spellPrefabReference =
            spellPrefab;


        // -----------------------------------------------------
        // TẠO RUNTIME SPELL MỚI
        // -----------------------------------------------------

        equippedSpell =
            Instantiate(
                spellPrefabReference,
                transform
            );


        equippedSpell.name =
            spellPrefabReference.name +
            "_Runtime";


        equippedSpell.ResetCooldown();


        equippedSpell.gameObject.hideFlags =
            HideFlags.DontSave;


        Debug.Log(
            "Player equipped Attack Spell: "
            +
            equippedSpell.spellName
        );
    }


    // =========================================================
    // START AIM
    // =========================================================

    public void StartAim()
    {
        if (equippedSpell == null)
            return;


        if (!equippedSpell.CanCast())
            return;


        equippedSpell.StartAim(player);
    }


    // =========================================================
    // CANCEL AIM
    // =========================================================

    public void CancelAim()
    {
        equippedSpell?.CancelAim();
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


            equippedSpell.CancelAim();

            return;
        }


        if (stats == null ||
            stats.baseStats == null)
        {
            Debug.LogError(
                "AttackSpellCaster: Không tìm thấy PlayerStats."
            );


            equippedSpell.CancelAim();

            return;
        }


        if (stats.baseStats.currentMana <
            equippedSpell.manaCost)
        {
            Debug.Log(
                "Không đủ mana!"
            );


            equippedSpell.CancelAim();

            return;
        }


        stats.baseStats.currentMana -=
            equippedSpell.manaCost;


        equippedSpell.Cast(player);

        equippedSpell.CancelAim();
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (equippedSpell != null &&
            equippedSpell != spellPrefabReference)
        {
            Destroy(
                equippedSpell.gameObject
            );
        }
    }
}