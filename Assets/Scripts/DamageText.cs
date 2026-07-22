using UnityEngine;
using TMPro;
using System.Collections;

public class DamageText : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TextMeshPro textMesh;

    [Header("Animation")]
    [SerializeField] private float duration = 1.1f;

    // Vận tốc bật lên ban đầu
    [SerializeField] private float jumpForce = 4.5f;

    // Trọng lực kéo số rơi xuống
    [SerializeField] private float gravity = 9f;

    // Tốc độ bay về phía camera
    [SerializeField] private float cameraMoveSpeed = 1.2f;

    // Khoảng cách tách các damage liên tiếp
    [SerializeField] private float spreadDistance = 0.35f;

    // Damage bắt đầu mờ từ thời điểm này
    [Range(0f, 1f)]
    [SerializeField] private float fadeStart = 0.65f;

    [Header("Size")]
    [SerializeField] private float normalFontSize = 4f;
    [SerializeField] private float criticalFontSize = 6f;

    private Coroutine animationRoutine;

    // Dùng chung để những damage liên tiếp không nằm cùng một vị trí
    private static int spawnIndex;

    public void Setup(int damage, bool isCritical)
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        textMesh.text = damage.ToString();
        textMesh.color = isCritical ? Color.red : Color.white;
        textMesh.fontSize = isCritical
            ? criticalFontSize
            : normalFontSize;

        // Reset vì object này được lấy lại từ pool
        textMesh.alpha = 1f;
        transform.localScale = Vector3.zero;

        // Cho text được ưu tiên vẽ hơn các damage text trước
        textMesh.renderer.sortingOrder = 100 + spawnIndex;

        animationRoutine = StartCoroutine(
            PlayAnimation(isCritical)
        );
    }

    private IEnumerator PlayAnimation(bool isCritical)
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            ReturnToPool();
            yield break;
        }

        Vector3 originalPosition = transform.position;

        /*
         * Damage xuất hiện theo thứ tự:
         * giữa, phải, trái, phải xa, trái xa
         *
         * 0, 1, -1, 2, -2
         */
        int slot = GetNextSpreadSlot();

        Vector3 horizontalOffset =
            mainCamera.transform.right *
            slot *
            spreadDistance;

        // Thêm một chút ngẫu nhiên để animation đỡ máy móc
        horizontalOffset +=
            mainCamera.transform.right *
            Random.Range(-0.08f, 0.08f);

        Vector3 startPosition =
            originalPosition +
            horizontalOffset;

        /*
         * Camera.forward chỉ hướng từ camera vào cảnh.
         * Muốn số bay về phía camera thì dùng -forward.
         */
        Vector3 towardCamera =
            -mainCamera.transform.forward;

        // Không cho hướng này bay quá mạnh theo chiều dọc
        towardCamera.y = 0f;
        towardCamera.Normalize();

        // Critical bật mạnh và xa hơn một chút
        float currentJumpForce = isCritical
            ? jumpForce * 1.2f
            : jumpForce;

        float currentCameraSpeed = isCritical
            ? cameraMoveSpeed * 1.2f
            : cameraMoveSpeed;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(timer / duration);

            /*
             * Công thức chuyển động theo trọng lực:
             *
             * y = vận tốc ban đầu * thời gian
             *     - 1/2 * trọng lực * thời gian²
             */
            float verticalMovement =
                currentJumpForce * timer -
                0.5f * gravity * timer * timer;

            Vector3 cameraMovement =
                towardCamera *
                currentCameraSpeed *
                timer;

            transform.position =
                startPosition +
                cameraMovement +
                Vector3.up * verticalMovement;

            // Luôn quay mặt về camera
            transform.rotation =
                mainCamera.transform.rotation;

            AnimateScale(progress, isCritical);
            AnimateFade(progress);

            yield return null;
        }

        ReturnToPool();
    }

    private void AnimateScale(
        float progress,
        bool isCritical)
    {
        float targetScale = isCritical
            ? 1.25f
            : 1f;

        float scale;

        // Phóng to nhanh khi vừa xuất hiện
        if (progress < 0.15f)
        {
            float scaleProgress =
                progress / 0.15f;

            scale = Mathf.Lerp(
                0f,
                targetScale * 1.25f,
                scaleProgress
            );
        }
        // Thu nhỏ nhẹ sau cú bật
        else if (progress < 0.3f)
        {
            float scaleProgress =
                (progress - 0.15f) / 0.15f;

            scale = Mathf.Lerp(
                targetScale * 1.25f,
                targetScale,
                scaleProgress
            );
        }
        else
        {
            scale = targetScale;
        }

        transform.localScale =
            Vector3.one * scale;
    }

    private void AnimateFade(float progress)
    {
        // Không mờ khi số vẫn đang bay lên
        if (progress < fadeStart)
        {
            textMesh.alpha = 1f;
            return;
        }

        float fadeProgress =
            Mathf.InverseLerp(
                fadeStart,
                1f,
                progress
            );

        textMesh.alpha =
            Mathf.Lerp(
                1f,
                0f,
                fadeProgress
            );
    }

    private int GetNextSpreadSlot()
    {
        int[] slots =
        {
            0,
            1,
            -1,
            2,
            -2
        };

        int slot =
            slots[spawnIndex % slots.Length];

        spawnIndex++;

        return slot;
    }

    private void ReturnToPool()
    {
        animationRoutine = null;

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnObject(
                gameObject
            );
        }
        else
        {
            Destroy(gameObject);
        }
    }
}