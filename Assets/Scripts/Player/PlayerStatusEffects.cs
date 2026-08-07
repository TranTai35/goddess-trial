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

    [Header("Status SFX")]
    [SerializeField] private AudioClip stunSFX;
    [SerializeField] private AudioClip slowSFX;
    [SerializeField] private AudioClip burnSFX;

    [Header("Status SFX Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float stunSFXVolume = 0.3f;

    [Range(0f, 1f)]
    [SerializeField] private float slowSFXVolume = 0.05f;

    [Range(0f, 1f)]
    [SerializeField] private float burnSFXVolume = 0.05f;

    [Header("VFX Position")]
    [SerializeField] private float stunVFXHeight = 2.2f;
    [SerializeField] private float slowVFXHeight = 0f;
    [SerializeField] private float burnVFXHeight = 1f;

    private float normalMoveSpeed;

    // True khi Player đang bị bất kỳ hiệu ứng nào.
    private bool isEffectActive;

    private GameObject activeVFX;

    // AudioSource riêng cho Slow.
    private AudioSource slowAudioSource;

    // AudioSource riêng cho Burn.
    private AudioSource burnAudioSource;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        playerController =
            GetComponent<PlayerController>();

        playerStats =
            GetComponent<PlayerStats>();

        // =====================================================
        // SLOW AUDIO SOURCE
        // =====================================================

        slowAudioSource =
            gameObject.AddComponent<AudioSource>();

        slowAudioSource.playOnAwake = false;

        /*
         * Nếu muốn tiếng Slow chạy liên tục trong thời gian
         * bị Slow thì để true.
         */
        slowAudioSource.loop = true;

        // 2D sound, không phụ thuộc khoảng cách camera.
        slowAudioSource.spatialBlend = 0f;

        slowAudioSource.clip = null;

        // =====================================================
        // BURN AUDIO SOURCE
        // =====================================================

        burnAudioSource =
            gameObject.AddComponent<AudioSource>();

        burnAudioSource.playOnAwake = false;

        burnAudioSource.loop = true;

        // 2D sound, không phụ thuộc khoảng cách camera.
        burnAudioSource.spatialBlend = 0f;

        burnAudioSource.clip = null;
    }

    private void Start()
    {
        if (playerController != null)
        {
            normalMoveSpeed =
                playerController.moveSpeed;
        }
    }

    // =========================================================
    // APPLY EFFECT
    // =========================================================

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
                ApplySlow(
                    duration,
                    effectValue
                );
                break;

            case ProjectileEffectType.Burn:
                ApplyBurn(
                    duration,
                    effectValue
                );
                break;
        }
    }

    // =========================================================
    // STUN
    // =========================================================

    #region Stun

    public void ApplyStun(float duration)
    {
        if (isEffectActive)
        {
            return;
        }

        isEffectActive = true;

        stunCoroutine =
            StartCoroutine(
                StunRoutine(duration)
            );
    }

    private IEnumerator StunRoutine(
        float duration)
    {
        if (playerController == null)
        {
            EndCurrentEffect();
            yield break;
        }

        // =========================
        // VFX
        // =========================

        if (stunVFX != null)
        {
            Vector3 spawnPosition =
                playerController.transform.position +
                Vector3.up *
                stunVFXHeight;

            activeVFX =
                Instantiate(
                    stunVFX,
                    spawnPosition,
                    Quaternion.identity,
                    playerController.transform
                );
        }

        // =========================
        // SFX
        // =========================

        /*
         * Stun chỉ phát một lần.
         */
        PlayOneShotStatusSFX(
            stunSFX,
            stunSFXVolume
        );

        // =========================
        // STATE
        // =========================

        playerController
            .SetControlEnabled(false);

        yield return
            new WaitForSeconds(
                duration
            );

        playerController
            .SetControlEnabled(true);

        stunCoroutine =
            null;

        EndCurrentEffect();
    }

    #endregion

    // =========================================================
    // SLOW
    // =========================================================

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

        slowCoroutine =
            StartCoroutine(
                SlowRoutine(
                    duration,
                    slowPercent
                )
            );
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

        // =========================
        // VFX
        // =========================

        if (slowVFX != null)
        {
            Vector3 spawnPosition =
                playerController.transform.position +
                Vector3.up *
                slowVFXHeight;

            activeVFX =
                Instantiate(
                    slowVFX,
                    spawnPosition,
                    Quaternion.identity,
                    playerController.transform
                );
        }

        // =========================
        // SFX
        // =========================

        /*
         * Phát Slow SFX bằng AudioSource riêng.
         *
         * Khi Slow hết thì StopSlowSFX()
         * sẽ tắt tiếng ngay.
         */
        PlaySlowSFX();

        // =========================
        // STATE
        // =========================

        normalMoveSpeed =
            playerController.BaseMoveSpeed;

        slowPercent =
            Mathf.Clamp01(
                slowPercent
            );

        playerController.moveSpeed =
            normalMoveSpeed *
            (1f - slowPercent);

        yield return
            new WaitForSeconds(
                duration
            );

        // Trả tốc độ về bình thường.
        playerController.moveSpeed =
            playerController.BaseMoveSpeed;

        // Tắt sound Slow ngay khi hết Slow.
        StopSlowSFX();

        slowCoroutine =
            null;

        EndCurrentEffect();
    }

    #endregion

    // =========================================================
    // BURN
    // =========================================================

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

        burnCoroutine =
            StartCoroutine(
                BurnRoutine(
                    duration,
                    damagePerSecond
                )
            );
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

        // =========================
        // VFX
        // =========================

        if (burnVFX != null)
        {
            Vector3 spawnPosition =
                playerController.transform.position +
                Vector3.up *
                burnVFXHeight;

            activeVFX =
                Instantiate(
                    burnVFX,
                    spawnPosition,
                    Quaternion.identity,
                    playerController.transform
                );
        }

        // =========================
        // BURN SFX
        // =========================

        PlayBurnSFX();

        // =========================
        // DAMAGE OVER TIME
        // =========================

        float elapsedTime =
            0f;

        float damageInterval =
            0.5f;

        while (elapsedTime < duration)
        {
            float waitTime =
                Mathf.Min(
                    damageInterval,
                    duration - elapsedTime
                );

            yield return
                new WaitForSeconds(
                    waitTime
                );

            playerStats.TakeDamage(
                damagePerSecond *
                waitTime
            );

            elapsedTime +=
                waitTime;
        }

        // Tắt sound Burn ngay khi hết Burn.
        StopBurnSFX();

        burnCoroutine =
            null;

        EndCurrentEffect();
    }

    #endregion

    // =========================================================
    // STUN ONE SHOT AUDIO
    // =========================================================

    private void PlayOneShotStatusSFX(
        AudioClip clip,
        float volume)
    {
        if (clip == null)
        {
            return;
        }

        float finalVolume =
            Mathf.Clamp01(
                volume
            );

        AudioSource.PlayClipAtPoint(
            clip,
            transform.position,
            finalVolume
        );
    }

    // =========================================================
    // SLOW AUDIO
    // =========================================================

    private void PlaySlowSFX()
    {
        if (slowSFX == null ||
            slowAudioSource == null)
        {
            return;
        }

        slowAudioSource.Stop();

        slowAudioSource.clip =
            slowSFX;

        /*
         * Slow sound sẽ loop cho tới khi hết effect.
         */
        slowAudioSource.loop =
            true;

        slowAudioSource.volume =
            Mathf.Clamp01(
                slowSFXVolume
            );

        slowAudioSource.Play();
    }

    private void StopSlowSFX()
    {
        if (slowAudioSource == null)
        {
            return;
        }

        slowAudioSource.Stop();

        slowAudioSource.clip =
            null;

        slowAudioSource.loop =
            false;
    }

    // =========================================================
    // BURN AUDIO
    // =========================================================

    private void PlayBurnSFX()
    {
        if (burnSFX == null ||
            burnAudioSource == null)
        {
            return;
        }

        burnAudioSource.Stop();

        burnAudioSource.clip =
            burnSFX;

        burnAudioSource.loop =
            true;

        burnAudioSource.volume =
            Mathf.Clamp01(
                burnSFXVolume
            );

        burnAudioSource.Play();
    }

    private void StopBurnSFX()
    {
        if (burnAudioSource == null)
        {
            return;
        }

        burnAudioSource.Stop();

        burnAudioSource.clip =
            null;

        burnAudioSource.loop =
            false;
    }

    // =========================================================
    // END EFFECT
    // =========================================================

    private void EndCurrentEffect()
    {
        // =========================
        // REMOVE VFX
        // =========================

        if (activeVFX != null)
        {
            Destroy(activeVFX);

            activeVFX =
                null;
        }

        // =========================
        // STOP STATUS SOUNDS
        // =========================

        /*
         * Chỉ có một effect được chạy cùng lúc,
         * nên gọi cả hai cũng không có vấn đề.
         */
        StopSlowSFX();
        StopBurnSFX();

        isEffectActive =
            false;
    }

    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        StopAllCoroutines();

        stunCoroutine =
            null;

        slowCoroutine =
            null;

        burnCoroutine =
            null;

        isEffectActive =
            false;

        // =========================
        // REMOVE VFX
        // =========================

        if (activeVFX != null)
        {
            Destroy(activeVFX);

            activeVFX =
                null;
        }

        // =========================
        // STOP STATUS SOUNDS
        // =========================

        StopSlowSFX();
        StopBurnSFX();

        // =========================
        // RESET PLAYER
        // =========================

        if (playerController != null)
        {
            playerController
                .SetControlEnabled(true);

            playerController.moveSpeed =
                playerController.BaseMoveSpeed;
        }
    }
}