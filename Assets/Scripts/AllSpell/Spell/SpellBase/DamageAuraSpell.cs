using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAuraSpell : SpellBase
{
    [Header("Aura Settings")]
    public float duration = 5f;
    public GameObject auraVFX;

    [Header("Damage Settings")]
    public int damagePerSecond = 5;      // Gây đúng 5 sát thương mỗi giây
    public float damageRadius = 4f;
    public LayerMask enemyLayer;

    public override void Cast(PlayerController player)
    {
        StartCooldown();
        player.StartCoroutine(AuraRoutine(player));
    }

    private IEnumerator AuraRoutine(PlayerController player)
    {
        Debug.Log("Damage Aura ĐÃ BẬT!");

        GameObject vfx = null;
        if (auraVFX != null)
        {
            Vector3 spawnPosition = player.transform.position + new Vector3(0f, 1f, 0f);
            vfx = Instantiate(
                auraVFX,
                spawnPosition,
                Quaternion.identity,
                player.transform
            );
        }

        float elapsed = 0f;
        float tickInterval = 1f; // Chờ đúng 1 giây quét 1 lần

        while (elapsed < duration)
        {
            // Quét tìm các Collider quanh Player dựa trên Layer Quái
            Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, damageRadius, enemyLayer);

            foreach (Collider col in hitColliders)
            {
                // LẤY TRỰC TIẾP SCRIPT ENEMY CỦA BẠN Ở ĐÂY:
                EnemyController enemy = col.GetComponent<EnemyController>();

                if (enemy != null)
                {
                    // Gọi hàm TakeDamage có sẵn trong EnemyController của bạn
                    enemy.TakeDamage(damagePerSecond);
                    Debug.Log($"Gây {damagePerSecond} dame lên {col.name}. Máu còn lại được xử lý trong EnemyController.");
                }
            }

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        Debug.Log("Damage Aura ĐÃ HẾT THỜI GIAN!");

        if (vfx != null)
        {
            Destroy(vfx);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}