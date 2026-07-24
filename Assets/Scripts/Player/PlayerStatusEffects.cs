using System.Collections;
using UnityEngine;

public class PlayerStatusEffects : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerStats playerStats;

    private Coroutine stunCoroutine;
    private Coroutine slowCoroutine;
    private Coroutine burnCoroutine;

    [Header("Status VFX")]
    [SerializeField] private GameObject stunVFX;
    [SerializeField] private GameObject slowVFX;
    [SerializeField] private GameObject burnVFX;

    [Header("VFX Position")]
    [SerializeField] private float stunVFXHeight = 2.2f;
    [SerializeField] private float burnVFXHeight = 1f;

    private float normalMoveSpeed;

    // True khi Player đang bị bất kỳ hiệu ứng nào.
    private bool isEffectActive;

    private GameObject activeVFX;

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
        // Đang có một hiệu ứng thì không nhận thêm hiệu ứng khác.
        if (isEffectActive)
        {
            return;
        }

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
        if (isEffectActive)
        {
            return;
        }

        isEffectActive = true;

        stunCoroutine = StartCoroutine(
            StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        if (playerController == null)
        {
            EndCurrentEffect();
            yield break;
        }

        if (stunVFX != null)
        {
            // Đặt VFX cao trên đầu Player.
            Vector3 spawnPosition =
                playerController.transform.position +
                Vector3.up * stunVFXHeight;

            activeVFX = Instantiate(
                stunVFX,
                spawnPosition,
                Quaternion.identity,
                playerController.transform);
        }

        playerController.SetControlEnabled(false);

        yield return new WaitForSeconds(duration);

        playerController.SetControlEnabled(true);

        stunCoroutine = null;

        EndCurrentEffect();
    }

    #endregion

    #region Slow

    public void ApplySlow(
        float duration,
        float slowPercent)
    {
        if (isEffectActive)
        {
            return;
        }

        isEffectActive = true;

        slowCoroutine = StartCoroutine(
            SlowRoutine(duration, slowPercent));
    }

    private IEnumerator SlowRoutine(
        float duration,
        float slowPercent)
    {
        if (playerController == null)
        {
            EndCurrentEffect();
            yield break;
        }

        // Giữ nguyên vị trí VFX Slow như code cũ.
        if (slowVFX != null)
        {
            activeVFX = Instantiate(
                slowVFX,
                playerController.transform.position,
                Quaternion.identity,
                playerController.transform);
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

        EndCurrentEffect();
    }

    #endregion

    #region Burn

    public void ApplyBurn(
        float duration,
        float damagePerSecond)
    {
        if (isEffectActive)
        {
            return;
        }

        isEffectActive = true;

        burnCoroutine = StartCoroutine(
            BurnRoutine(
                duration,
                damagePerSecond));
    }

    private IEnumerator BurnRoutine(
        float duration,
        float damagePerSecond)
    {
        if (playerStats == null ||
            playerController == null)
        {
            EndCurrentEffect();
            yield break;
        }

        if (burnVFX != null)
        {
            // Đặt VFX ở phần thân Player.
            Vector3 spawnPosition =
                playerController.transform.position +
                Vector3.up * burnVFXHeight;

            activeVFX = Instantiate(
                burnVFX,
                spawnPosition,
                Quaternion.identity,
                playerController.transform);
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

        EndCurrentEffect();
    }

    #endregion

    private void EndCurrentEffect()
    {
        if (activeVFX != null)
        {
            Destroy(activeVFX);
            activeVFX = null;
        }

        isEffectActive = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        stunCoroutine = null;
        slowCoroutine = null;
        burnCoroutine = null;

        isEffectActive = false;

        if (activeVFX != null)
        {
            Destroy(activeVFX);
            activeVFX = null;
        }

        if (playerController != null)
        {
            playerController.SetControlEnabled(true);

            playerController.moveSpeed =
                playerController.BaseMoveSpeed;
        }
    }
}