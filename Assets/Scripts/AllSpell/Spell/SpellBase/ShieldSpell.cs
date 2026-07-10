using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSpell : SpellBase
{
    public float duration = 5f;

    public GameObject shieldVFX;

    public override void Cast(PlayerController player)
    {
        
        StartCooldown();

        player.StartCoroutine(
            ShieldRoutine(player));
    }

    private IEnumerator ShieldRoutine(
        PlayerController player)
    {
        Debug.Log("Shield ON");

        GameObject vfx = null;

        if (shieldVFX != null)
        {
            vfx = Instantiate(
                shieldVFX,
                player.transform.position,
                Quaternion.identity,
                player.transform
            );
        }

        yield return new WaitForSeconds(duration);

        Debug.Log("Shield OFF");

        if (vfx != null)
        {
            Destroy(vfx);
        }
    }
}