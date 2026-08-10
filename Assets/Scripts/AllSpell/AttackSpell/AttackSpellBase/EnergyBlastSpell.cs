using UnityEngine;

public class EnergyBlastSpell : AttackSpellBase
{
    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Projectile Damage")]
    [SerializeField]
    private float projectileDamage = 30f;

    [Header("Range Indicator")]
    public GameObject rangeIndicatorPrefab;

    private GameObject currentRangeIndicator;

    private PlayerController currentPlayer;


    public override void StartAim(
        PlayerController player)
    {
        currentPlayer = player;

        /*
         * Nhấn E chỉ bắt đầu ngắm.
         * Chưa cast nên chưa phát SFX.
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

        currentPlayer = null;
    }


    public override void Cast(
        PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        if (Camera.main == null)
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
         * Click nhưng không lấy được điểm
         * thì không cast.
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


        direction.y = 0f;


        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }


        direction.Normalize();


        // =====================================================
        // CAST THÀNH CÔNG
        // =====================================================

        StartCooldown();


        PlayCastSFX(
            player.transform.position
        );


        Debug.Log(
            "Energy Blast Cast - Damage: " +
            projectileDamage
        );


        // =====================================================
        // SPAWN PROJECTILE
        // =====================================================

        Vector3 spawnPos =
            player.transform.position +
            Vector3.up * 1f +
            direction * 1.5f;


        Quaternion rotation =
            Quaternion.LookRotation(
                direction
            );


        GameObject obj;


        if (PoolManager.Instance != null)
        {
            obj =
                PoolManager.Instance
                    .GetObject(
                        projectilePrefab,
                        spawnPos,
                        rotation
                    );
        }
        else
        {
            obj =
                Instantiate(
                    projectilePrefab,
                    spawnPos,
                    rotation
                );
        }


        if (obj == null)
        {
            CancelAim();

            return;
        }


        // =====================================================
        // PROJECTILE
        // =====================================================

        Projectile projectile =
            obj.GetComponent<Projectile>();


        if (projectile != null)
        {
            projectile.SetOwner(
                player.gameObject
            );


            /*
             * Damage của Energy Blast
             * lấy trực tiếp từ projectileDamage
             * trong Inspector.
             */
            projectile.InitializeDirection(
                direction,
                projectileDamage
            );
        }


        // =====================================================
        // HIDE INDICATOR
        // =====================================================

        CancelAim();
    }
}