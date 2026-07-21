using UnityEngine;

public abstract class AttackSpellBase : MonoBehaviour
{
    [Header("Info")]
    public string spellName;
    public Sprite icon;
    public int manaCost = 10;
    public float attackRange = 8f;

    [Header("Cooldown")]
    public float cooldown = 5f;
    public float lastCastTime = -100f;

    protected virtual void Awake()
    {
        // Khởi tạo để có thể dùng chiêu ngay khi vừa vào game mà không bị kẹt cooldown
        lastCastTime = -cooldown;
    }

    // Hàm kiểm tra xem chiêu thức đã hồi xong chưa
    public bool CanCast()
    {
        return Time.time >= lastCastTime + cooldown;
    }

    // Hàm tính toán số giây còn lại để gửi cho PlayerUI hiển thị số đếm ngược
    public float GetRemainingCooldown()
    {
        return Mathf.Max(0f, lastCastTime + cooldown - Time.time);
    }

    // Hàm kích hoạt tính thời gian hồi chiêu (Sẽ gọi trong hàm Cast của các phép con)
    protected void StartCooldown()
    {
        lastCastTime = Time.time;
    }

    // --- Các hàm bắt buộc các phép con (như EnergyBlastSpell) phải viết logic ---
    public abstract void StartAim(PlayerController player);

    public abstract void Cast(PlayerController player);

    public abstract void CancelAim();
}