using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Data References")]
    public PlayerStats stats;

    // Tự động lấy từ Player
    private PlayerController playerController;
    private SpellCaster spellCaster;
    private AttackSpellCaster attackCaster;

    [Header("UI Elements")]
    public Slider hpBar;
    public Slider manaBar;
    public TMP_Text goldText;
    public TMP_Text diamondText;

    [Header("Spell Icons")]
    public Image UltiIcon; // Đã có sẵn theo file code mới của bạn
    public Image spellIcon;
    public Image attackSpellIcon;

    [Header("Cooldown Texts (BỔ SUNG)")]
    public TMP_Text ultimateCooldownText;
    public TMP_Text spellCooldownText;
    public TMP_Text attackSpellCooldownText;

    [Header("Cooldown Color Settings (CÀI ĐẶT MÀU SẮC)")]
    // Bạn có thể chỉnh màu tối này trực tiếp ngoài Inspector tùy ý thích
    public Color normalColor = Color.white;
    public Color cooldownColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            spellCaster = playerObj.GetComponent<SpellCaster>();
            attackCaster = playerObj.GetComponent<AttackSpellCaster>();
        }
        else
        {
            Debug.LogError("PlayerUI: Không tìm thấy GameObject có tag 'Player'!");
        }
    }

    private void Update()
    {
        // Cập nhật chỉ số cơ bản
        if (stats != null)
        {
            hpBar.value = stats.baseStats.currentHealth / stats.baseStats.maxHealth;
            manaBar.value = stats.baseStats.currentMana / stats.baseStats.maxMana;
            goldText.text = "Gold: " + stats.baseStats.gold.ToString();
            diamondText.text = "Diamond: " + stats.baseStats.diamond.ToString();
        }

        // --- 1. Đếm ngược & Làm tối Ultimate (Chuột phải) ---
        if (playerController != null)
        {
            float ultiCD = playerController.GetUltiCooldown();
            UpdateCooldownText(ultiCD, ultimateCooldownText);

            // Cập nhật màu tối/sáng cho UltiIcon
            UpdateIconVisual(ultiCD, UltiIcon);
        }

        // --- 2. Đếm ngược & Làm tối Phép Bổ Trợ (Q) ---
        if (spellCaster != null && spellCaster.equippedSpell != null)
        {
            spellIcon.sprite = spellCaster.equippedSpell.icon;
            spellIcon.enabled = true;

            float spellCD = spellCaster.equippedSpell.GetRemainingCooldown();
            UpdateCooldownText(spellCD, spellCooldownText);

            // Cập nhật màu tối/sáng cho spellIcon
            UpdateIconVisual(spellCD, spellIcon);
        }
        else
        {
            spellIcon.enabled = false;
            if (spellCooldownText != null) spellCooldownText.text = "";
        }

        // --- 3. Đếm ngược & Làm tối Phép Tấn Công (E) ---
        if (attackCaster != null && attackCaster.equippedSpell != null)
        {
            attackSpellIcon.sprite = attackCaster.equippedSpell.icon;
            attackSpellIcon.enabled = true;

            float attackSpellCD = attackCaster.equippedSpell.GetRemainingCooldown();
            UpdateCooldownText(attackSpellCD, attackSpellCooldownText);

            // Cập nhật màu tối/sáng cho attackSpellIcon
            UpdateIconVisual(attackSpellCD, attackSpellIcon);
        }
        else
        {
            attackSpellIcon.enabled = false;
            if (attackSpellCooldownText != null) attackSpellCooldownText.text = "";
        }
    }

    // Hàm bổ trợ xử lý hiển thị chữ số đếm ngược
    private void UpdateCooldownText(float remainingTime, TMP_Text cdText)
    {
        if (cdText == null) return;

        if (remainingTime > 0f)
        {
            if (remainingTime > 1f)
                cdText.text = Mathf.CeilToInt(remainingTime).ToString();
            else
                cdText.text = remainingTime.ToString("F1");
        }
        else
        {
            cdText.text = ""; // Hồi chiêu xong thì xóa chữ
        }
    }

    // HÀM MỚI BỔ SUNG: Xử lý làm tối/sáng Icon dựa trên thời gian cooldown còn lại
    private void UpdateIconVisual(float remainingTime, Image iconImage)
    {
        if (iconImage == null) return;

        if (remainingTime > 0f)
        {
            // Nếu kỹ năng đang hồi chiêu thì chuyển sang màu tối
            iconImage.color = cooldownColor;
        }
        else
        {
            // Kỹ năng sẵn sàng thì trả về màu trắng bình thường
            iconImage.color = normalColor;
        }
    }
}