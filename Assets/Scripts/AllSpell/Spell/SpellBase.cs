using UnityEngine;

public abstract class SpellBase : MonoBehaviour
{
    [Header("Info")]
    public string spellName;
    public Sprite icon;
    public int manaCost = 10;

    [Header("Cooldown")]
    [Min(0f)]
    public float cooldown = 5f;

    /*
     * NonSerialized ngăn Unity lưu thời gian runtime
     * vào prefab hoặc Inspector.
     */
    [System.NonSerialized]
    private float lastCastTime;

    public void ResetCooldown()
    {
        // Cho phép sử dụng ngay khi vào game.
        lastCastTime = -cooldown;
    }

    public bool CanCast()
    {
        return Time.time >= lastCastTime + cooldown;
    }

    public float GetRemainingCooldown()
    {
        return Mathf.Max(
            0f,
            lastCastTime + cooldown - Time.time);
    }

    protected void StartCooldown()
    {
        lastCastTime = Time.time;
    }

    public abstract void Cast(PlayerController player);
}