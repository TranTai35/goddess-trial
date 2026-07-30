using System.Collections.Generic;
using UnityEngine;

public class MagicianNPC : NPC
{
    [Header("--- TRANG SPELL (BỔ TRỢ) ---")]
    public List<SpellLearnData> utilitySpells;
    public List<SpellItemUI> utilityUIItems;   // Kéo các nút của trang Spell vào đây

    [Header("--- TRANG ATTACK SPELL (TẤN CÔNG) ---")]
    public List<SpellLearnData> attackSpells;
    public List<SpellItemUI> attackUIItems;    // Kéo các nút của trang Attack vào đây

    private PlayerController player;
    private PlayerStats playerStats;
    private SpellCaster spellCaster;
    private AttackSpellCaster attackSpellCaster;

    protected override void Awake()
    {
        base.Awake();
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            spellCaster = player.GetComponent<SpellCaster>();
            attackSpellCaster = player.GetComponent<AttackSpellCaster>();
        }
    }

    private void Start()
    {
        // 1. Khởi tạo dữ liệu cho trang Spell bổ trợ
        for (int i = 0; i < utilityUIItems.Count; i++)
        {
            if (i < utilitySpells.Count)
            {
                utilityUIItems[i].Setup(i, SpellType.Utility, utilitySpells[i], this);
            }
        }

        // 2. Khởi tạo dữ liệu cho trang Attack Spell
        for (int i = 0; i < attackUIItems.Count; i++)
        {
            if (i < attackSpells.Count)
            {
                attackUIItems[i].Setup(i, SpellType.Attack, attackSpells[i], this);
            }
        }

        RefreshAllUI();
    }

    // Làm mới toàn bộ UI của cả 2 trang
    public void RefreshAllUI()
    {
        // Làm mới trang Spell bổ trợ
        for (int i = 0; i < utilitySpells.Count; i++)
        {
            if (i >= utilityUIItems.Count) break;
            bool isEquipped = CheckIfEquipped(utilitySpells[i], SpellType.Utility);
            utilityUIItems[i].UpdateVisuals(utilitySpells[i].isBought, isEquipped);
        }

        // Làm mới trang Attack Spell
        for (int i = 0; i < attackSpells.Count; i++)
        {
            if (i >= attackUIItems.Count) break;
            bool isEquipped = CheckIfEquipped(attackSpells[i], SpellType.Attack);
            attackUIItems[i].UpdateVisuals(attackSpells[i].isBought, isEquipped);
        }
    }

    private bool CheckIfEquipped(SpellLearnData data, SpellType type)
    {
        if (type == SpellType.Utility && spellCaster.equippedSpell != null && data.utilitySpellPrefab != null)
        {
            return spellCaster.equippedSpell.spellName == data.utilitySpellPrefab.spellName;
        }
        else if (type == SpellType.Attack && attackSpellCaster.equippedSpell != null && data.attackSpellPrefab != null)
        {
            return attackSpellCaster.equippedSpell.spellName == data.attackSpellPrefab.spellName;
        }
        return false;
    }

    // Khi click, hàm này dựa vào SpellType để biết cần xử lý danh sách nào
    public void OnSpellButtonClicked(int index, SpellType type)
    {
        // Chọn đúng danh sách dữ liệu dựa theo loại phép
        List<SpellLearnData> targetDataList = (type == SpellType.Utility) ? utilitySpells : attackSpells;

        if (index < 0 || index >= targetDataList.Count) return;
        SpellLearnData data = targetDataList[index];

        if (!data.isBought)
        {
            // Logic mua phép
            if (playerStats.baseStats.diamond >= data.diamondCost)
            {
                playerStats.baseStats.diamond -= data.diamondCost;
                data.isBought = true;
                EquipSpellToPlayer(data, type);
            }
            else
            {
                Debug.Log("Không đủ kim cương!");
            }
        }
        else
        {
            // Đã mua rồi thì chỉ trang bị thôi
            EquipSpellToPlayer(data, type);
        }

        // Cập nhật lại màu sắc toàn bộ các nút bấm
        RefreshAllUI();
    }

    private void EquipSpellToPlayer(
     SpellLearnData data,
     SpellType type)
    {
        GameObject playerObj =
            GameObject.FindGameObjectWithTag(
                "Player"
            );


        if (playerObj == null)
            return;


        // =========================================================
        // UTILITY SPELL
        // =========================================================

        if (type == SpellType.Utility &&
            data.utilitySpellPrefab != null)
        {
            SpellCaster utilityCaster =
                playerObj.GetComponent<SpellCaster>();


            if (utilityCaster != null)
            {
                // Trang bị cho Player hiện tại
                utilityCaster.EquipSpell(
                    data.utilitySpellPrefab
                );


                // Lưu để sang scene khác vẫn còn
                if (SpellLoadoutManager.Instance != null)
                {
                    SpellLoadoutManager.Instance
                        .SetUtilitySpell(
                            data.utilitySpellPrefab
                        );
                }


                Debug.Log(
                    $"Đã trang bị Utility Spell: {data.spellName}"
                );
            }
        }


        // =========================================================
        // ATTACK SPELL
        // =========================================================

        else if (type == SpellType.Attack &&
                 data.attackSpellPrefab != null)
        {
            AttackSpellCaster attackCaster =
                playerObj.GetComponent<AttackSpellCaster>();


            if (attackCaster != null)
            {
                // Trang bị cho Player hiện tại
                attackCaster.EquipSpell(
                    data.attackSpellPrefab
                );


                // Lưu để sang scene khác vẫn còn
                if (SpellLoadoutManager.Instance != null)
                {
                    SpellLoadoutManager.Instance
                        .SetAttackSpell(
                            data.attackSpellPrefab
                        );
                }


                Debug.Log(
                    $"Đã trang bị Attack Spell: {data.spellName}"
                );
            }
        }
    }
}