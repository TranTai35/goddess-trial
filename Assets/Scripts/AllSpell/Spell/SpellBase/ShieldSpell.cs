using System.Collections;
using UnityEngine;

public class ShieldSpell : SpellBase
{
    [Header("Shield Settings")]
    [Min(0f)]
    public float duration = 5f;

    public GameObject shieldVFX;

    [Header("Shield Hit SFX")]
    [Tooltip("Âm thanh phát mỗi khi shield chặn được một đòn đánh/projectile.")]
    [SerializeField] private AudioClip shieldHitSFX;

    [Range(0f, 1f)]
    [SerializeField] private float shieldHitSFXVolume = 1f;

    [Tooltip("Khoảng nghỉ rất ngắn để nhiều collider của cùng một đòn không làm âm thanh bị chồng quá dày.")]
    [Min(0f)]
    [SerializeField] private float shieldHitSFXCooldown = 0.05f;

    private Coroutine activeRoutine;
    private GameObject activeVFX;
    private PlayerController activePlayer;
    private float lastShieldHitSFXTime = -999f;

    public override void Cast(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        StartCooldown();

        // Tiếng cast shield vẫn dùng Cast SFX từ SpellBase.
        PlayCastSFX(player.transform.position);

        // Nếu còn shield cũ thì dừng và xóa trước, tránh chồng nhiều VFX.
        if (activeRoutine != null && activePlayer != null)
        {
            activePlayer.StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        RemoveShieldVFX();

        activePlayer = player;
        lastShieldHitSFXTime = -999f;

        activeRoutine = player.StartCoroutine(ShieldRoutine(player));
    }

    public bool IsActiveFor(PlayerController player)
    {
        return player != null &&
               activePlayer == player &&
               activeRoutine != null;
    }

    public bool TryBlockDamage(PlayerController player)
    {
        if (!IsActiveFor(player))
        {
            return false;
        }

        PlayShieldHitSFX(player.transform.position);
        return true;
    }

    private IEnumerator ShieldRoutine(PlayerController player)
    {
        Debug.Log("Shield ON");

        if (shieldVFX != null)
        {
            activeVFX = Instantiate(
                shieldVFX,
                player.transform.position,
                Quaternion.identity,
                player.transform
            );
        }

        yield return new WaitForSeconds(duration);

        Debug.Log("Shield OFF");

        RemoveShieldVFX();

        activeRoutine = null;
        activePlayer = null;
    }

    private void PlayShieldHitSFX(Vector3 position)
    {
        if (shieldHitSFX == null)
        {
            return;
        }

        if (Time.time < lastShieldHitSFXTime + shieldHitSFXCooldown)
        {
            return;
        }

        lastShieldHitSFXTime = Time.time;

        AudioSource.PlayClipAtPoint(
            shieldHitSFX,
            position,
            shieldHitSFXVolume
        );
    }

    private void RemoveShieldVFX()
    {
        if (activeVFX == null)
        {
            return;
        }

        Destroy(activeVFX);
        activeVFX = null;
    }
}
