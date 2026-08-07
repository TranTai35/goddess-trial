using System.Collections;
using UnityEngine;

public class ShieldSpell : SpellBase
{
    [Header("Shield Settings")]
    [Min(0f)]
    public float duration = 5f;

    public GameObject shieldVFX;

    private Coroutine activeRoutine;
    private GameObject activeVFX;

    public override void Cast(
        PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        StartCooldown();

        /*
         * PHÁT SFX NGAY KHI SHIELD ĐƯỢC DÙNG.
         */
        PlayCastSFX(
            player.transform.position
        );

        /*
         * Nếu còn shield cũ thì dừng và xóa trước,
         * tránh nhiều VFX chồng lên nhau.
         */
        if (activeRoutine != null)
        {
            player.StopCoroutine(
                activeRoutine
            );

            activeRoutine = null;
        }

        RemoveShieldVFX();

        activeRoutine =
            player.StartCoroutine(
                ShieldRoutine(player)
            );
    }

    private IEnumerator ShieldRoutine(
        PlayerController player)
    {
        Debug.Log(
            "Shield ON"
        );

        if (shieldVFX != null)
        {
            activeVFX = Instantiate(
                shieldVFX,
                player.transform.position,
                Quaternion.identity,
                player.transform
            );
        }

        yield return
            new WaitForSeconds(
                duration
            );

        Debug.Log(
            "Shield OFF"
        );

        RemoveShieldVFX();

        activeRoutine =
            null;
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