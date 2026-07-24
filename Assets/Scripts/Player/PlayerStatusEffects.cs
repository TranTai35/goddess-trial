using System.Collections;
using UnityEngine;

public class PlayerStatusEffects : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerStats playerStats;

    private Coroutine stunCoroutine;
    private Coroutine slowCoroutine;
    private Coroutine burnCoroutine;

    private float normalMoveSpeed;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        if (playerController != null)
        {
            normalMoveSpeed = playerController.moveSpeed;
        }
    }

    public void ApplyEffect(
        ProjectileEffectType effectType,
        float duration,
        float effectValue)
    {
        switch (effectType)
        {
            case ProjectileEffectType.Stun:
                ApplyStun(duration);
                break;

            case ProjectileEffectType.Slow:
                ApplySlow(duration, effectValue);
                break;

            case ProjectileEffectType.Burn:
                ApplyBurn(duration, effectValue);
                break;
        }
    }

    #region Stun

    public void ApplyStun(float duration)
    {
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(
            StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        if (playerController == null)
        {
            yield break;
        }

        playerController.SetControlEnabled(false);

        yield return new WaitForSeconds(duration);

        playerController.SetControlEnabled(true);

        stunCoroutine = null;
    }

    #endregion

    #region Slow

    public void ApplySlow(
        float duration,
        float slowPercent)
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        slowCoroutine = StartCoroutine(
            SlowRoutine(duration, slowPercent));
    }

    private IEnumerator SlowRoutine(float duration,float slowPercent)
    {
        if (playerController == null)
        {
            yield break;
        }

        normalMoveSpeed =
            playerController.BaseMoveSpeed;

        slowPercent =
            Mathf.Clamp01(slowPercent);

        playerController.moveSpeed =
            normalMoveSpeed *
            (1f - slowPercent);

        yield return new WaitForSeconds(duration);

        playerController.moveSpeed =
            playerController.BaseMoveSpeed;

        slowCoroutine = null;
    }

    #endregion

    #region Burn

    public void ApplyBurn(
        float duration,
        float damagePerSecond)
    {
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }

        burnCoroutine = StartCoroutine(
            BurnRoutine(
                duration,
                damagePerSecond));
    }

    private IEnumerator BurnRoutine(
        float duration,
        float damagePerSecond)
    {
        if (playerStats == null)
        {
            yield break;
        }

        float elapsedTime = 0f;
        float damageInterval = 0.5f;

        while (elapsedTime < duration)
        {
            yield return new WaitForSeconds(
                damageInterval);

            playerStats.TakeDamage(
                damagePerSecond *
                damageInterval);

            elapsedTime += damageInterval;
        }

        burnCoroutine = null;
    }

    #endregion

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.SetControlEnabled(true);

            if (normalMoveSpeed > 0f)
            {
                playerController.moveSpeed =
                    normalMoveSpeed;
            }
        }
    }
}