using UnityEngine;

public class EnergyBlastSpell : AttackSpellBase
{
    public GameObject projectilePrefab;

    public GameObject rangeIndicatorPrefab;

    private GameObject currentRangeIndicator;

    private PlayerController currentPlayer;

    public override void StartAim(PlayerController player)
    {
        currentPlayer = player;

        if (currentRangeIndicator == null)
        {
            currentRangeIndicator = Instantiate(
                rangeIndicatorPrefab,
                player.transform);
        }

        currentRangeIndicator.SetActive(true);

        currentRangeIndicator.transform.localPosition =
            new Vector3(0, 0.05f, 0);

        currentRangeIndicator.transform.localRotation =
            Quaternion.Euler(90f, 0f, 0f);

        currentRangeIndicator.transform.localScale =
            new Vector3(
                attackRange * 2f,
                attackRange * 2f,
                1f);
    }
    public override void CancelAim()
    {
        if (currentRangeIndicator != null)
        {
            currentRangeIndicator.SetActive(false);
        }
    }

    public override void Cast(PlayerController player)
    {
        // BỔ SUNG: Bắt đầu tính thời gian hồi chiêu ngay khi bắn chiêu thành công
        StartCooldown();

        Debug.Log("Click Cast");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, player.transform.position);

        if (!plane.Raycast(ray, out float enter)) return;

        Vector3 mousePoint = ray.GetPoint(enter);
        Vector3 direction = mousePoint - player.transform.position;
        direction.y = 0;
        direction.Normalize();

        Vector3 spawnPos = player.transform.position + Vector3.up * 1f + direction * 1.5f;

        GameObject obj;

        if (PoolManager.Instance != null)
        {
            obj = PoolManager.Instance.GetObject(
                projectilePrefab,
                spawnPos,
                Quaternion.LookRotation(direction)
            );
        }
        else
        {
            obj = Instantiate(
                projectilePrefab,
                spawnPos,
                Quaternion.LookRotation(direction)
            );
        }

        Projectile projectile =
            obj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetOwner(
                player.gameObject
            );

            projectile.InitializeDirection(
                direction,
                player.GetComponent<PlayerStats>()
                    .baseStats.damage
            );
        }
    }
}