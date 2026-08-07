using UnityEngine;

public abstract class AttackSpellBase : MonoBehaviour
{
    [Header("Info")]
    public string spellName;
    public Sprite icon;
    public int manaCost = 10;
    public float attackRange = 8f;

    [Header("Cooldown")]
    [Min(0f)]
    public float cooldown = 5f;

    [Header("Cast SFX")]
    [SerializeField] protected AudioClip castSFX;

    [Range(0f, 1f)]
    [SerializeField] protected float castSFXVolume = 1f;

    // Chỉ là dữ liệu runtime, không lưu lên prefab.
    private float lastCastTime;

    protected virtual void Awake()
    {
        ResetCooldown();
    }

    public void ResetCooldown()
    {
        // Cho phép dùng ngay khi vào game.
        lastCastTime = -cooldown;
    }

    public bool CanCast()
    {
        return Time.time >=
               lastCastTime +
               cooldown;
    }

    public float GetRemainingCooldown()
    {
        return Mathf.Max(
            0f,
            lastCastTime +
            cooldown -
            Time.time
        );
    }

    protected void StartCooldown()
    {
        lastCastTime =
            Time.time;
    }

    protected void PlayCastSFX(
        Vector3 position)
    {
        if (castSFX == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(
            castSFX,
            position,
            castSFXVolume
        );
    }

    public abstract void StartAim(
        PlayerController player
    );

    public abstract void Cast(
        PlayerController player
    );

    public abstract void CancelAim();
}