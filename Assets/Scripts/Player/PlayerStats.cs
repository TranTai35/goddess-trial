using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData baseStats;

    private Animator animator;

    private const string TakeDamageTrigger = "TakeDamage";

    // =========================================================
    // DAMAGE FEEDBACK
    // =========================================================

    [Header("Damage Feedback")]

    [Tooltip("Màu player khi nhận damage")]
    public Color damageColor = Color.red;

    [Tooltip("Thời gian giữ màu đỏ")]
    public float damageFlashDuration = 0.15f;


    private bool isTakingDamage = false;

    // Danh sách material trên cơ thể Player
    private List<Material> playerMaterials =
        new List<Material>();

    // Lưu màu gốc của từng material
    private List<Color> originalColors =
        new List<Color>();


    private void Awake()
    {
        animator = GetComponent<Animator>();

        baseStats.currentHealth =
            baseStats.maxHealth;

        baseStats.currentMana =
            baseStats.maxMana;

        CachePlayerMaterials();
    }


    // =========================================================
    // CACHE MATERIAL
    // =========================================================

    private void CachePlayerMaterials()
    {
        playerMaterials.Clear();
        originalColors.Clear();


        // -----------------------------
        // SKINNED MESH
        // Ví dụ:
        // Body
        // Cloak
        // ...
        // -----------------------------

        SkinnedMeshRenderer[] skinnedRenderers =
            GetComponentsInChildren<SkinnedMeshRenderer>(true);


        foreach (
            SkinnedMeshRenderer renderer
            in skinnedRenderers
        )
        {
            Material[] materials =
                renderer.materials;


            foreach (Material material in materials)
            {
                AddMaterial(material);
            }
        }


        // -----------------------------
        // NORMAL MESH
        // Ví dụ:
        // Hair
        // Head
        // Backpack
        // Weapon
        // ...
        // -----------------------------

        MeshRenderer[] meshRenderers =
            GetComponentsInChildren<MeshRenderer>(true);


        foreach (
            MeshRenderer renderer
            in meshRenderers
        )
        {
            Material[] materials =
                renderer.materials;


            foreach (Material material in materials)
            {
                AddMaterial(material);
            }
        }
    }


    private void AddMaterial(Material material)
    {
        if (material == null)
            return;


        // Tránh lưu material trùng
        if (playerMaterials.Contains(material))
            return;


        // Shader có property màu hay không
        if (
            material.HasProperty("_BaseColor")
            ||
            material.HasProperty("_Color")
        )
        {
            playerMaterials.Add(material);

            originalColors.Add(
                material.color
            );
        }
    }


    // =========================================================
    // TAKE DAMAGE
    // =========================================================

    public void TakeDamage(float damage)
    {
        PlayerController player =
            GetComponent<PlayerController>();


        // Nếu đang bất tử khi dash
        if (
            player != null
            &&
            player.IsInvincible
        )
        {
            return;
        }


        baseStats.currentHealth -= damage;


        Debug.Log(
            "Player HP: "
            +
            baseStats.currentHealth
        );


        // =====================================================
        // DAMAGE FEEDBACK
        // =====================================================

        if (
            !isTakingDamage
            &&
            animator != null
        )
        {
            StartCoroutine(
                PlayDamageFeedbackRoutine()
            );
        }


        // =====================================================
        // DIE
        // =====================================================

        if (baseStats.currentHealth <= 0)
        {
            Die();
        }
    }


    // =========================================================
    // DAMAGE FEEDBACK ROUTINE
    // =========================================================

    private IEnumerator PlayDamageFeedbackRoutine()
    {
        isTakingDamage = true;


        // -----------------------------------------------------
        // 1. BẬT ANIMATION TAKE DAMAGE
        // -----------------------------------------------------

        animator.SetTrigger(
            TakeDamageTrigger
        );


        // -----------------------------------------------------
        // 2. PLAYER BIẾN THÀNH MÀU ĐỎ
        // -----------------------------------------------------

        SetPlayerColor(
            damageColor
        );


        // Giữ màu đỏ trong một khoảng ngắn
        yield return new WaitForSeconds(
            damageFlashDuration
        );


        // -----------------------------------------------------
        // 3. TRẢ PLAYER VỀ MÀU GỐC
        // -----------------------------------------------------

        RestorePlayerColor();


        // Animation TakeDamage của bạn hiện đang
        // khóa damage animation trong khoảng 1 giây.
        float remainingTime =
            1f - damageFlashDuration;


        if (remainingTime > 0f)
        {
            yield return new WaitForSeconds(
                remainingTime
            );
        }


        isTakingDamage = false;
    }


    // =========================================================
    // SET RED
    // =========================================================

    private void SetPlayerColor(Color color)
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


            material.color = color;
        }
    }


    // =========================================================
    // RESTORE ORIGINAL COLOR
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
        baseStats.currentHealth = 0;

        // Phòng trường hợp player chết
        // đúng lúc đang đỏ
        RestorePlayerColor();

        Debug.Log("PLAYER DEAD");
    }
}