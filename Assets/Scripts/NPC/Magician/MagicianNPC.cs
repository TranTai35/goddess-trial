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
        // ==========================================
        // KHÔI PHỤC SPELL ĐÃ MUA
        // ==========================================

        RestorePurchasedSpells();


        // ==========================================
        // SETUP UTILITY UI
        // ==========================================

        for (int i = 0;
             i < utilityUIItems.Count;
             i++)
        {
            if (i < utilitySpells.Count)
            {
                utilityUIItems[i].Setup(
                    i,
                    SpellType.Utility,
                    utilitySpells[i],
                    this
                );
            }
        }


        // ==========================================
        // SETUP ATTACK UI
        // ==========================================

        for (int i = 0;
             i < attackUIItems.Count;
             i++)
        {
            if (i < attackSpells.Count)
            {
                attackUIItems[i].Setup(
                    i,
                    SpellType.Attack,
                    attackSpells[i],
                    this
                );
            }
        }


        RefreshAllUI();
    }

    private void RestorePurchasedSpells()
    {
        if (SpellLoadoutManager.Instance == null)
            return;


        // Utility
        for (int i = 0;
             i < utilitySpells.Count;
             i++)
        {
            utilitySpells[i].isBought =
                SpellLoadoutManager.Instance
                    .IsSpellBought(
                        utilitySpells[i],
                        SpellType.Utility
                    );
        }


        // Attack
        for (int i = 0;
             i < attackSpells.Count;
             i++)
        {
            attackSpells[i].isBought =
                SpellLoadoutManager.Instance
                    .IsSpellBought(
                        attackSpells[i],
                        SpellType.Attack
                    );
        }
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

    private bool CheckIfEquipped(
    SpellLearnData data,
    SpellType type)
    {
        if (SpellLoadoutManager.Instance == null)
            return false;


        if (type == SpellType.Utility)
        {
            return SpellLoadoutManager.Instance
                .IsUtilityEquipped(data);
        }


        if (type == SpellType.Attack)
        {
            return SpellLoadoutManager.Instance
                .IsAttackEquipped(data);
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


                // LƯU SPELL ĐÃ MUA
                if (SpellLoadoutManager.Instance != null)
                {
                    SpellLoadoutManager.Instance
                        .MarkSpellBought(
                            data,
                            type
                        );
                }


                EquipSpellToPlayer(
                    data,
                    type
                );
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


        // ==========================================
        // UTILITY
        // ==========================================

        if (type == SpellType.Utility &&
            data.utilitySpellPrefab != null)
        {
            SpellCaster utilityCaster =
                playerObj.GetComponent<SpellCaster>();


            if (utilityCaster != null)
            {
                utilityCaster.EquipSpell(
                    data.utilitySpellPrefab
                );
            }


            if (SpellLoadoutManager.Instance != null)
            {
                SpellLoadoutManager.Instance
                    .SetUtilitySpell(
                        data.utilitySpellPrefab
                    );
            }
        }


        // ==========================================
        // ATTACK
        // ==========================================

        else if (type == SpellType.Attack &&
                 data.attackSpellPrefab != null)
        {
            AttackSpellCaster attackCaster =
                playerObj.GetComponent<
                    AttackSpellCaster
                >();


            if (attackCaster != null)
            {
                attackCaster.EquipSpell(
                    data.attackSpellPrefab
                );
            }


            if (SpellLoadoutManager.Instance != null)
            {
                SpellLoadoutManager.Instance
                    .SetAttackSpell(
                        data.attackSpellPrefab
                    );
            }
        }
    }
}