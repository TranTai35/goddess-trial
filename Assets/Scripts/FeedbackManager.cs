using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("Camera Settings")]
    public Transform cameraTransform; // Kéo Main Camera vào đây
    public Image damageFlashPanel;
    private Vector3 _originalPosition;

    void Awake()
    {
        Instance = this;
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    // Hàm gọi chung cho cả game
    public void PlayHitFeedback(float hitStopDuration, float timeScale, float shakeDuration, float shakeMagnitude)
    {
        StartCoroutine(HitStopRoutine(hitStopDuration, timeScale));
        StartCoroutine(CameraShakeRoutine(shakeDuration, shakeMagnitude));

    }

    private IEnumerator HitStopRoutine(float duration, float timeScale)
    {
        float originalTimeScale = Time.timeScale;

        // 1. Hạ TimeScale mượt mà trong 0.02 giây đầu (Ease-in)
        float transitionTime = 0.02f;
        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            Time.timeScale = Mathf.Lerp(originalTimeScale, timeScale, elapsed / transitionTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 2. Giữ ở mức low-speed trong thời gian bạn muốn
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);

        // 3. Phục hồi TimeScale mượt mà (Ease-out)
        elapsed = 0f;
        while (elapsed < transitionTime)
        {
            Time.timeScale = Mathf.Lerp(timeScale, 1f, elapsed / transitionTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;
    }

    private IEnumerator CameraShakeRoutine(float duration, float magnitude)
    {
        _originalPosition = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            cameraTransform.localPosition = new Vector3(_originalPosition.x + x, _originalPosition.y + y, _originalPosition.z);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cameraTransform.localPosition = _originalPosition;
    }

    public void PlayDamageFlash(float duration)
    {
        StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        if (damageFlashPanel == null) yield break;

        // Lấy màu gốc (thường là trong suốt)
        Color originalColor = new Color(1, 0, 0, 0);
        // Màu đỏ nhạt lúc bắt đầu (alpha 0.3)
        Color flashColor = new Color(1, 0, 0, 0.3f);

        float elapsed = 0f;

        // Tăng dần Alpha lên một chút rồi giảm dần về 0
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Sin((elapsed / duration) * Mathf.PI); // Hàm Sin tạo đường cong mềm mại
            damageFlashPanel.color = Color.Lerp(originalColor, flashColor, t);
            yield return null;
        }

        damageFlashPanel.color = originalColor;
    }
}