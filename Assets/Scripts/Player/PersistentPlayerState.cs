using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lưu trạng thái Player xuyên scene trong một phiên chơi.
/// - permanent stats: chỉ số đã nâng ở Captain.
/// - currency: vàng/kim cương luôn được giữ, kể cả chết hoặc thoát level.
/// - spell: phép đã mua và phép đang trang bị.
/// </summary>
public class PersistentPlayerState : MonoBehaviour
{
    public static PersistentPlayerState Instance { get; private set; }

    public const string VillageSceneName = "Village";

    private bool initialized;

    private float permanentMaxHealth;
    private float permanentMaxMana;
    private float permanentManaRegen;
    private float permanentDamage;
    private float permanentMoveSpeed;

    private int gold;
    private int diamond;

    private SpellBase equippedUtilitySpellPrefab;
    private AttackSpellBase equippedAttackSpellPrefab;

    private readonly HashSet<string> boughtUtilitySpells = new HashSet<string>();
    private readonly HashSet<string> boughtAttackSpells = new HashSet<string>();

    public static PersistentPlayerState EnsureExists()
    {
        if (Instance != null)
            return Instance;

        GameObject stateObject = new GameObject("PersistentPlayerState");
        return stateObject.AddComponent<PersistentPlayerState>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeFrom(PlayerStatsData stats)
    {
        if (initialized || stats == null)
            return;

        permanentMaxHealth = stats.maxHealth;
        permanentMaxMana = stats.maxMana;
        permanentManaRegen = stats.manaRegen;
        permanentDamage = stats.damage;
        permanentMoveSpeed = stats.moveSpeed;

        gold = stats.gold;
        diamond = stats.diamond;
        initialized = true;
    }

    public void ApplyPersistentState(PlayerStats player)
    {
        if (player == null || player.baseStats == null)
            return;

        InitializeFrom(player.baseStats);

        PlayerStatsData stats = player.baseStats;
        stats.maxHealth = permanentMaxHealth;
        stats.maxMana = permanentMaxMana;
        stats.manaRegen = permanentManaRegen;
        stats.damage = permanentDamage;
        stats.moveSpeed = permanentMoveSpeed;
        stats.gold = gold;
        stats.diamond = diamond;
        stats.currentHealth = stats.maxHealth;
        stats.currentMana = stats.maxMana;
    }

    public void ApplyEquippedSpells(GameObject playerObject)
    {
        if (playerObject == null)
            return;

        SpellCaster utilityCaster = playerObject.GetComponent<SpellCaster>();
        if (utilityCaster != null)
            utilityCaster.EquipSpell(equippedUtilitySpellPrefab);

        AttackSpellCaster attackCaster = playerObject.GetComponent<AttackSpellCaster>();
        if (attackCaster != null)
            attackCaster.EquipSpell(equippedAttackSpellPrefab);
    }

    /// <summary>
    /// Chỉ gọi sau khi nâng cấp ở Captain hoặc trước khi rời Village.
    /// Không gọi trong level, vì nâng cấp trong level chỉ là tạm thời.
    /// </summary>
    public void SavePermanentStatsFrom(PlayerStats player)
    {
        if (player == null || player.baseStats == null)
            return;

        InitializeFrom(player.baseStats);

        PlayerStatsData stats = player.baseStats;
        permanentMaxHealth = stats.maxHealth;
        permanentMaxMana = stats.maxMana;
        permanentManaRegen = stats.manaRegen;
        permanentDamage = stats.damage;
        permanentMoveSpeed = stats.moveSpeed;

        SaveCurrencyFrom(player);
    }

    public void SaveCurrencyFrom(PlayerStats player)
    {
        if (player == null || player.baseStats == null)
            return;

        InitializeFrom(player.baseStats);
        gold = player.baseStats.gold;
        diamond = player.baseStats.diamond;
    }

    public void SetEquippedUtilitySpell(SpellBase prefab)
    {
        equippedUtilitySpellPrefab = prefab;
    }

    public void SetEquippedAttackSpell(AttackSpellBase prefab)
    {
        equippedAttackSpellPrefab = prefab;
    }

    public void MarkSpellBought(string spellName, SpellType type)
    {
        if (string.IsNullOrWhiteSpace(spellName))
            return;

        if (type == SpellType.Utility)
            boughtUtilitySpells.Add(spellName);
        else
            boughtAttackSpells.Add(spellName);
    }

    public bool IsSpellBought(string spellName, SpellType type)
    {
        if (string.IsNullOrWhiteSpace(spellName))
            return false;

        return type == SpellType.Utility
            ? boughtUtilitySpells.Contains(spellName)
            : boughtAttackSpells.Contains(spellName);
    }
}
