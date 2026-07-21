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
    public AnimationCurve hitStopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float shakeDampening = 4f;

    void Awake()
    {
        Instance = this;
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    // Hàm gọi chung cho các hiệu ứng
    public void PlayHitFeedback(float hitStopDuration, float timeScale, float shakeDuration, float shakeMagnitude)
    {
        StopAllCoroutines();
        StartCoroutine(HitStopRoutine(hitStopDuration, timeScale));
        StartCoroutine(DelayedShake(0.05f, shakeDuration, shakeMagnitude));
    }

    // --- HIỆU ỨNG DỪNG HÌNH (HIT STOP) ---
    private IEnumerator HitStopRoutine(float duration, float targetTimeScale)
    {
        float originalTimeScale = Time.timeScale;
        float elapsed = 0f;
        float transitionTime = 0.05f;

        while (elapsed < transitionTime)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(originalTimeScale, targetTimeScale, hitStopCurve.Evaluate(elapsed / transitionTime));
            yield return null;
        }

        yield return new WaitForSecondsRealtime(duration);

        elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(targetTimeScale, 1f, hitStopCurve.Evaluate(elapsed / transitionTime));
            yield return null;
        }
        Time.timeScale = 1f;
    }

    // --- HIỆU ỨNG RUNG CAMERA (CAMERA SHAKE) ---
    private IEnumerator DelayedShake(float delay, float duration, float magnitude)
    {
        yield return new WaitForSecondsRealtime(delay);
        _originalPosition = cameraTransform.localPosition;
        float elapsed = 0f;
        float randomSeed = Random.Range(0f, 1000f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float percent = elapsed / duration;
            float damper = 1f - Mathf.Clamp01(shakeDampening * percent - (shakeDampening - 1));

            float x = (Mathf.PerlinNoise(elapsed * 20f, randomSeed) - 0.5f) * 2f * magnitude * damper;
            float y = (Mathf.PerlinNoise(randomSeed, elapsed * 20f) - 0.5f) * 2f * magnitude * damper;

            cameraTransform.localPosition = _originalPosition + new Vector3(x, y, 0);
            yield return null;
        }
        cameraTransform.localPosition = _originalPosition;
    }

    // --- HIỆU ỨNG CHỚP ĐỎ (DAMAGE FLASH) ---
    public void PlayDamageFlash(float duration)
    {
        StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        if (damageFlashPanel == null) yield break;

        Color originalColor = new Color(1, 0, 0, 0); // Màu đỏ trong suốt (alpha 0)
        Color flashColor = new Color(1, 0, 0, 0.4f); // Màu đỏ đậm hơn (alpha 0.4)

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            // Dùng Sin để tạo độ "nảy" sáng lên rồi tắt dần
            float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
            damageFlashPanel.color = Color.Lerp(originalColor, flashColor, t);
            yield return null;
        }
        damageFlashPanel.color = originalColor;
    }
}