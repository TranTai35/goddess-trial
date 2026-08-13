using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData baseStats;

    private Animator animator;

    private const string TakeDamageTrigger =
        "TakeDamage";


    // =========================================================
    // DAMAGE FEEDBACK
    // =========================================================

    [Header("Damage Feedback")]

    [Tooltip("Màu player khi nhận damage")]
    public Color damageColor =
        Color.red;

    [Tooltip("Thời gian giữ màu đỏ")]
    public float damageFlashDuration =
        0.15f;


    private bool isTakingDamage =
        false;


    private List<Material> playerMaterials =
        new List<Material>();


    private List<Color> originalColors =
        new List<Color>();


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        animator =
            GetComponent<Animator>();


        InitializeRunStats();


        CachePlayerMaterials();
    }


    // =========================================================
    // RUN HP / MANA PERSISTENCE
    // =========================================================

    private void InitializeRunStats()
    {
        if (baseStats == null)
        {
            Debug.LogError(
                "PlayerStats chưa được gắn PlayerStatsData."
            );

            return;
        }


        string currentScene =
            SceneManager.GetActiveScene().name;


        // =========================================================
        // VILLAGE
        // =========================================================

        if (currentScene == "Village")
        {
            /*
             * Khi trở về Village:
             * - kết thúc run cũ
             * - hồi đầy HP/Mana
             */
            PlayerRunState.ResetToFull(
                baseStats
            );

            return;
        }


        // =========================================================
        // BATTLE SCENE
        // =========================================================

        /*
         * Nếu chưa có một run đang hoạt động.
         */
        if (!PlayerRunState.IsRunActive)
        {
            /*
             * Trường hợp PlayerStatsData còn giữ HP = 0
             * từ lần test/chết trước.
             *
             * Không được bắt đầu run với HP = 0.
             */
            if (baseStats.currentHealth <= 0f)
            {
                PlayerRunState.StartNewRun(
                    baseStats
                );

                return;
            }


            /*
             * Nếu HP hiện tại vẫn hợp lệ,
             * có thể đây là dữ liệu vừa Load Game.
             *
             * Ví dụ save đang có:
             * HP = 70
             * Mana = 40
             *
             * thì bắt đầu run từ 70/40.
             */
            PlayerRunState.StartFromCurrent(
                baseStats
            );

            return;
        }


        // =========================================================
        // ĐANG TRONG CÙNG MỘT RUN
        // =========================================================

        /*
         * Ví dụ:
         *
         * Level1_1:
         * HP 100 -> còn 65
         *
         * chuyển Level1_2
         *
         * => Player vẫn còn 65 HP.
         */
        PlayerRunState.RestoreOrStart(
            baseStats
        );
    }


    // =========================================================
    // SPEND MANA
    // =========================================================

    public bool TrySpendMana(
        float amount)
    {
        if (
            baseStats == null ||
            amount < 0f
        )
        {
            return false;
        }


        if (
            baseStats.currentMana <
            amount
        )
        {
            return false;
        }


        baseStats.currentMana -=
            amount;


        PlayerRunState.Save(
            baseStats
        );


        return true;
    }


    // =========================================================
    // HEAL HP PERCENT
    // =========================================================

    public void HealPercent(
        float percent)
    {
        if (baseStats == null)
            return;


        percent =
            Mathf.Max(
                0f,
                percent
            );


        float healAmount =
            baseStats.maxHealth *
            percent;


        float oldHealth =
            baseStats.currentHealth;


        baseStats.currentHealth =
            Mathf.Clamp(
                baseStats.currentHealth +
                healAmount,
                0f,
                baseStats.maxHealth
            );


        PlayerRunState.Save(
            baseStats
        );


        Debug.Log(
            $"Heal HP: {oldHealth} -> " +
            $"{baseStats.currentHealth}"
        );
    }


    // =========================================================
    // RESTORE MANA PERCENT
    // =========================================================

    public void RestoreManaPercent(
        float percent)
    {
        if (baseStats == null)
            return;


        percent =
            Mathf.Max(
                0f,
                percent
            );


        float manaAmount =
            baseStats.maxMana *
            percent;


        float oldMana =
            baseStats.currentMana;


        baseStats.currentMana =
            Mathf.Clamp(
                baseStats.currentMana +
                manaAmount,
                0f,
                baseStats.maxMana
            );


        PlayerRunState.Save(
            baseStats
        );


        Debug.Log(
            $"Restore Mana: {oldMana} -> " +
            $"{baseStats.currentMana}"
        );
    }


    // =========================================================
    // SAVE
    // =========================================================

    public void SaveRunStats()
    {
        PlayerRunState.Save(
            baseStats
        );
    }


    // =========================================================
    // CACHE MATERIAL
    // =========================================================

    private void CachePlayerMaterials()
    {
        playerMaterials.Clear();

        originalColors.Clear();


        SkinnedMeshRenderer[] skinnedRenderers =
            GetComponentsInChildren<SkinnedMeshRenderer>(
                true
            );


        foreach (
            SkinnedMeshRenderer renderer
            in skinnedRenderers
        )
        {
            Material[] materials =
                renderer.materials;


            foreach (
                Material material
                in materials
            )
            {
                AddMaterial(
                    material
                );
            }
        }


        MeshRenderer[] meshRenderers =
            GetComponentsInChildren<MeshRenderer>(
                true
            );


        foreach (
            MeshRenderer renderer
            in meshRenderers
        )
        {
            Material[] materials =
                renderer.materials;


            foreach (
                Material material
                in materials
            )
            {
                AddMaterial(
                    material
                );
            }
        }
    }


    // =========================================================
    // ADD MATERIAL
    // =========================================================

    private void AddMaterial(
        Material material)
    {
        if (material == null)
            return;


        if (
            playerMaterials.Contains(
                material
            )
        )
        {
            return;
        }


        if (
            material.HasProperty("_BaseColor")
            ||
            material.HasProperty("_Color")
        )
        {
            playerMaterials.Add(
                material
            );


            originalColors.Add(
                material.color
            );
        }
    }


    // =========================================================
    // TAKE DAMAGE
    // =========================================================

    public void TakeDamage(
        float damage)
    {
        PlayerController player =
            GetComponent<PlayerController>();


        // =========================================================
        // SHIELD BLOCK
        // =========================================================
        // Shield có trạng thái riêng. Khi đang bật:
        // - không trừ HP
        // - phát SFX shield hit
        // - không chạy damage feedback đỏ/animation
        if (player != null)
        {
            SpellCaster spellCaster =
                GetComponent<SpellCaster>();

            if (spellCaster != null &&
                spellCaster.equippedSpell is ShieldSpell shieldSpell &&
                shieldSpell.TryBlockDamage(player))
            {
                return;
            }
        }


        // Dash / Ultimate invincibility vẫn hoạt động như cũ.
        if (
            player != null &&
            player.IsInvincible
        )
        {
            return;
        }


        baseStats.currentHealth =
            Mathf.Max(
                0f,
                baseStats.currentHealth -
                damage
            );


        PlayerRunState.Save(
            baseStats
        );


        Debug.Log(
            "Player HP: " +
            baseStats.currentHealth
        );


        if (
            !isTakingDamage &&
            animator != null
        )
        {
            StartCoroutine(
                PlayDamageFeedbackRoutine()
            );
        }


        if (
            baseStats.currentHealth <= 0
        )
        {
            Die();
        }
    }


    // =========================================================
    // DAMAGE FEEDBACK
    // =========================================================

    private IEnumerator PlayDamageFeedbackRoutine()
    {
        isTakingDamage =
            true;


        animator.SetTrigger(
            TakeDamageTrigger
        );


        SetPlayerColor(
            damageColor
        );


        yield return new WaitForSeconds(
            damageFlashDuration
        );


        RestorePlayerColor();


        float remainingTime =
            1f -
            damageFlashDuration;


        if (remainingTime > 0f)
        {
            yield return new WaitForSeconds(
                remainingTime
            );
        }


        isTakingDamage =
            false;
    }


    // =========================================================
    // SET COLOR
    // =========================================================

    private void SetPlayerColor(
        Color color)
    {
        for (
            int i = 0;
            i < playerMaterials.Count;
            i++
        )
        {
            Material material =
                playerMaterials[i];


            if (material == null)
                continue;


            material.color =
                color;
        }
    }


    // =========================================================
    // RESTORE COLOR
    // =========================================================

    private void RestorePlayerColor()
    {
        for (
            int i = 0;
            i < playerMaterials.Count;
            i++
        )
        {
            Material material =
                playerMaterials[i];


            if (material == null)
                continue;


            material.color =
                originalColors[i];
        }
    }


    // =========================================================
    // DIE
    // =========================================================

    private void Die()
    {
        baseStats.currentHealth =
            0;


        PlayerRunState.Save(
            baseStats
        );


        RestorePlayerColor();


        Debug.Log(
            "PLAYER DEAD"
        );
    }
}