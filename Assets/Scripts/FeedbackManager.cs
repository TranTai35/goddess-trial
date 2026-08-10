using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public Image damageFlashPanel;

    private Vector3 _originalPosition;

    [Header("Curves & Settings")]
    public AnimationCurve hitStopCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float shakeDampening = 4f;

    // Nếu game đang pause/death thì Feedback không được
    // tự ý bật Time.timeScale trở lại.
    private bool lockTimeScale = false;


    private void Awake()
    {
        Instance = this;

        if (cameraTransform == null &&
            Camera.main != null)
        {
            cameraTransform =
                Camera.main.transform;
        }
    }


    // =========================================================
    // HIT FEEDBACK
    // =========================================================

    public void PlayHitFeedback(
        float hitStopDuration,
        float timeScale,
        float shakeDuration,
        float shakeMagnitude)
    {
        /*
         * Nếu game đã bị khóa bởi Death/Pause
         * thì không chạy hit stop nữa.
         */
        if (!lockTimeScale)
        {
            StartCoroutine(
                HitStopRoutine(
                    hitStopDuration,
                    timeScale
                )
            );
        }

        StartCoroutine(
            DelayedShake(
                0.05f,
                shakeDuration,
                shakeMagnitude
            )
        );
    }


    // =========================================================
    // HIT STOP
    // =========================================================

    private IEnumerator HitStopRoutine(
        float duration,
        float targetTimeScale)
    {
        /*
         * Nếu duration <= 0 và target = 1
         * thì không cần thay đổi Time.timeScale.
         *
         * Đây chính là trường hợp MeleeEnemy hiện tại.
         */
        if (
            duration <= 0f &&
            Mathf.Approximately(
                targetTimeScale,
                1f
            )
        )
        {
            yield break;
        }


        float originalTimeScale =
            Time.timeScale;

        float elapsed = 0f;
        float transitionTime = 0.05f;


        while (
            elapsed < transitionTime &&
            !lockTimeScale
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;

            Time.timeScale =
                Mathf.Lerp(
                    originalTimeScale,
                    targetTimeScale,
                    hitStopCurve.Evaluate(
                        elapsed /
                        transitionTime
                    )
                );

            yield return null;
        }


        if (lockTimeScale)
            yield break;


        yield return
            new WaitForSecondsRealtime(
                duration
            );


        elapsed = 0f;


        while (
            elapsed < transitionTime &&
            !lockTimeScale
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;

            Time.timeScale =
                Mathf.Lerp(
                    targetTimeScale,
                    1f,
                    hitStopCurve.Evaluate(
                        elapsed /
                        transitionTime
                    )
                );

            yield return null;
        }


        if (!lockTimeScale)
        {
            Time.timeScale = 1f;
        }
    }


    // =========================================================
    // CAMERA SHAKE
    // =========================================================

    private IEnumerator DelayedShake(
        float delay,
        float duration,
        float magnitude)
    {
        yield return
            new WaitForSecondsRealtime(
                delay
            );


        if (cameraTransform == null)
            yield break;


        _originalPosition =
            cameraTransform.localPosition;


        float elapsed = 0f;

        float randomSeed =
            Random.Range(
                0f,
                1000f
            );


        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float percent =
                elapsed /
                duration;


            float damper =
                1f -
                Mathf.Clamp01(
                    shakeDampening *
                    percent -
                    (shakeDampening - 1)
                );


            float x =
                (
                    Mathf.PerlinNoise(
                        elapsed * 20f,
                        randomSeed
                    ) -
                    0.5f
                ) *
                2f *
                magnitude *
                damper;


            float y =
                (
                    Mathf.PerlinNoise(
                        randomSeed,
                        elapsed * 20f
                    ) -
                    0.5f
                ) *
                2f *
                magnitude *
                damper;


            cameraTransform.localPosition =
                _originalPosition +
                new Vector3(
                    x,
                    y,
                    0f
                );


            yield return null;
        }


        cameraTransform.localPosition =
            _originalPosition;
    }


    // =========================================================
    // DAMAGE FLASH
    // =========================================================

    public void PlayDamageFlash(
        float duration)
    {
        StartCoroutine(
            FlashRoutine(
                duration
            )
        );
    }


    private IEnumerator FlashRoutine(
        float duration)
    {
        if (damageFlashPanel == null)
            yield break;


        Color originalColor =
            new Color(
                1f,
                0f,
                0f,
                0f
            );


        Color flashColor =
            new Color(
                1f,
                0f,
                0f,
                0.4f
            );


        float elapsed = 0f;


        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Sin(
                    (elapsed / duration) *
                    Mathf.PI
                );


            damageFlashPanel.color =
                Color.Lerp(
                    originalColor,
                    flashColor,
                    t
                );


            yield return null;
        }


        damageFlashPanel.color =
            originalColor;
    }


    // =========================================================
    // LOCK / UNLOCK TIME SCALE
    // =========================================================

    public void LockGameTime()
    {
        lockTimeScale = true;

        /*
         * Dừng toàn bộ feedback đang chạy.
         * Quan trọng: không cho HitStopRoutine
         * bật Time.timeScale = 1 trở lại.
         */
        StopAllCoroutines();


        if (cameraTransform != null)
        {
            cameraTransform.localPosition =
                _originalPosition;
        }


        if (damageFlashPanel != null)
        {
            damageFlashPanel.color =
                new Color(
                    1f,
                    0f,
                    0f,
                    0f
                );
        }
    }


    public void UnlockGameTime()
    {
        lockTimeScale = false;
    }
}