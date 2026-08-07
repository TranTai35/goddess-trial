using UnityEngine;

public class EnergyBlastSpell
    : AttackSpellBase
{
    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Range Indicator")]
    public GameObject rangeIndicatorPrefab;

    private GameObject currentRangeIndicator;

    private PlayerController currentPlayer;

    public override void StartAim(
        PlayerController player)
    {
        currentPlayer =
            player;

        /*
         * KHÔNG phát SFX tại đây.
         *
         * Nhấn E chỉ là bắt đầu ngắm.
         */

        if (currentRangeIndicator == null)
        {
            currentRangeIndicator =
                Instantiate(
                    rangeIndicatorPrefab,
                    player.transform
                );
        }

        currentRangeIndicator
            .SetActive(true);

        currentRangeIndicator
            .transform.localPosition =
                new Vector3(
                    0f,
                    0.05f,
                    0f
                );

        currentRangeIndicator
            .transform.localRotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f
                );

        currentRangeIndicator
            .transform.localScale =
                new Vector3(
                    attackRange * 2f,
                    attackRange * 2f,
                    1f
                );
    }

    public override void CancelAim()
    {
        if (currentRangeIndicator != null)
        {
            currentRangeIndicator
                .SetActive(false);
        }

        currentPlayer =
            null;
    }

    public override void Cast(
        PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        Ray ray =
            Camera.main.ScreenPointToRay(
                Input.mousePosition
            );

        Plane plane =
            new Plane(
                Vector3.up,
                player.transform.position
            );

        /*
         * Nếu click nhưng không lấy được điểm,
         * không cast, không cooldown và cũng không phát SFX.
         */
        if (!plane.Raycast(
            ray,
            out float enter))
        {
            return;
        }

        Vector3 mousePoint =
            ray.GetPoint(enter);

        Vector3 direction =
            mousePoint -
            player.transform.position;

        direction.y =
            0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }

        direction.Normalize();

        /*
         * TỚI ĐÂY MỚI XÁC NHẬN
         * ATTACK SPELL THẬT SỰ ĐƯỢC CAST.
         */
        StartCooldown();

        /*
         * PHÁT SFX TẠI THỜI ĐIỂM CLICK CHỌN HƯỚNG.
         */
        PlayCastSFX(
            player.transform.position
        );

        Debug.Log(
            "Energy Blast Cast"
        );

        Vector3 spawnPos =
            player.transform.position +
            Vector3.up * 1f +
            direction * 1.5f;

        GameObject obj;

        if (PoolManager.Instance != null)
        {
            obj =
                PoolManager.Instance
                    .GetObject(
                        projectilePrefab,
                        spawnPos,
                        Quaternion.LookRotation(
                            direction
                        )
                    );
        }
        else
        {
            obj =
                Instantiate(
                    projectilePrefab,
                    spawnPos,
                    Quaternion.LookRotation(
                        direction
                    )
                );
        }

        if (obj == null)
        {
            return;
        }

        Projectile projectile =
            obj.GetComponent<
                Projectile>();

        if (projectile != null)
        {
            projectile.SetOwner(
                player.gameObject
            );

            PlayerStats stats =
                player.GetComponent<
                    PlayerStats>();

            float projectileDamage =
                0f;

            if (stats != null &&
                stats.baseStats != null)
            {
                projectileDamage =
                    stats.baseStats.damage;
            }

            projectile
                .InitializeDirection(
                    direction,
                    projectileDamage
                );
        }

        /*
         * Sau khi bắn xong thì ẩn indicator.
         */
        CancelAim();
    }
}