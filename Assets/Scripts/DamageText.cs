using UnityEngine;
using TMPro;
using System.Collections;

public class DamageText : MonoBehaviour
{
    public TextMeshPro textMesh;

    // Tốc độ bay lên và thời gian tồn tại
    public float moveSpeed = 1.5f;
    public float fadeDuration = 1f;

    public void Setup(int damage, bool isCrit)
    {
        textMesh.text = damage.ToString();
        textMesh.color = isCrit ? Color.red : Color.white;
        textMesh.fontSize = isCrit ? 6 : 4;

        // Bắt đầu hiệu ứng bay và mờ
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        float timer = 0f;
        Color startColor = textMesh.color;
        Vector3 startPos = transform.position;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // 1. Hiệu ứng bay lên (di chuyển theo trục Y)
            transform.position = startPos + Vector3.up * (progress * moveSpeed);

            // 2. Hiệu ứng mờ dần (Alpha giảm từ 1 về 0)
            textMesh.alpha = Mathf.Lerp(1f, 0f, progress);

            yield return null;
        }

        // Sau khi xong thì xóa hoặc trả về pool
        Destroy(gameObject); // Hoặc: PoolManager.Instance.ReturnObject(this.gameObject);
    }
}