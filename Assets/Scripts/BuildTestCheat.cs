using UnityEngine;

/// <summary>
/// Phím tắt test toàn game: giữ Shift và nhấn R để hoàn thành toàn bộ enemy area
/// trong scene hiện tại. Script tự tạo khi game chạy nên hoạt động cả trong bản Build.
/// </summary>
public class BuildTestCheat : MonoBehaviour
{
    private static BuildTestCheat instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateAutomatically()
    {
        if (instance != null)
            return;

        GameObject cheatObject = new GameObject("Build Test Cheat");
        instance = cheatObject.AddComponent<BuildTestCheat>();
        DontDestroyOnLoad(cheatObject);
    }

    private void Update()
    {
        bool holdingShift =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        if (holdingShift && Input.GetKeyDown(KeyCode.R))
        {
            DefeatAllEnemies();
        }
    }

    private void DefeatAllEnemies()
    {
        int completedAreaCount = 0;
        int looseEnemyCount = 0;
        int bossCount = 0;

        // Hoàn thành cả enemy đang spawn, enemy chưa spawn và những wave còn lại.
        EnemySpawnArea[] areas = FindObjectsOfType<EnemySpawnArea>();

        foreach (EnemySpawnArea area in areas)
        {
            if (area == null)
                continue;

            area.ForceCompleteForTesting();
            completedAreaCount++;
        }

        // Diệt enemy được đặt thủ công, không nằm trong EnemySpawnArea.
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();

        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            enemy.TakeDamage(float.MaxValue);
            looseEnemyCount++;
        }

        // Diệt boss trong scene nếu có.
        BossController[] bosses = FindObjectsOfType<BossController>();

        foreach (BossController boss in bosses)
        {
            if (boss == null || !boss.gameObject.activeInHierarchy)
                continue;

            boss.TakeDamage(float.MaxValue);
            bossCount++;
        }

        Debug.Log(
            $"[BUILD TEST] Shift + R hoàn tất: {completedAreaCount} area, " +
            $"{looseEnemyCount} enemy rời và {bossCount} boss.");
    }
}
