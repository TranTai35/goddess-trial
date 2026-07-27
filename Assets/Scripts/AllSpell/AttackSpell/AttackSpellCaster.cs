using UnityEngine;

public class AttackSpellCaster : MonoBehaviour
{
    [Header("Attack Spell Prefab")]
    [Tooltip(
        "Kéo prefab có component AttackSpellBase vào đây. " +
        "Khi game bắt đầu, hệ thống tạo một bản runtime riêng.")]
    public AttackSpellBase equippedSpell;

    private PlayerController player;
    private PlayerStats stats;

    private AttackSpellBase spellPrefabReference;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        stats = GetComponent<PlayerStats>();

        CreateRuntimeSpell();
    }

    private void CreateRuntimeSpell()
    {
        if (equippedSpell == null)
            return;

        // Lưu prefab gốc.
        spellPrefabReference = equippedSpell;

        /*
         * Tạo bản runtime riêng.
         * Cooldown sẽ không còn ghi vào prefab.
         */
        equippedSpell = Instantiate(
            spellPrefabReference,
            transform);

        equippedSpell.name =
            spellPrefabReference.name + "_Runtime";

        equippedSpell.ResetCooldown();

        equippedSpell.gameObject.hideFlags =
            HideFlags.DontSave;
    }

    public void StartAim()
    {
        if (equippedSpell == null)
            return;

        /*
         * Không hiện chế độ aim nếu chiêu đang hồi.
         */
        if (!equippedSpell.CanCast())
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

        if (stats == null ||
            stats.baseStats == null)
        {
            Debug.LogError(
                "AttackSpellCaster: Không tìm thấy PlayerStats.");

            equippedSpell.CancelAim();
            return;
        }

        if (stats.baseStats.currentMana <
            equippedSpell.manaCost)
        {
            Debug.Log("Không đủ mana!");

            equippedSpell.CancelAim();
            return;
        }

        stats.baseStats.currentMana -=
            equippedSpell.manaCost;

        equippedSpell.Cast(player);
        equippedSpell.CancelAim();
    }

    private void OnDestroy()
    {
        /*
         * Chỉ hủy bản runtime.
         */
        if (equippedSpell != null &&
            equippedSpell != spellPrefabReference)
        {
            Destroy(equippedSpell.gameObject);
        }
    }
}