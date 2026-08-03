using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnArea : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera gameplayCamera;

    [Header("Area Radius")]
    [Min(1f)]
    [SerializeField] private float activationRadius = 45f;

    [Min(1f)]
    [SerializeField] private float despawnRadius = 55f;

    [Min(0f)]
    [SerializeField] private float offScreenDespawnDelay = 2f;

    [Header("Hidden Spawn")]
    [Min(0.05f)]
    [SerializeField] private float hiddenSpawnCheckInterval = 0.2f;

    [Range(0f, 0.5f)]
    [Tooltip("Vùng đệm ngoài mép camera. 0.08 nghĩa là không spawn cả ở sát mép màn hình.")]
    [SerializeField] private float spawnPointVisiblePadding = 0f;

    [Min(0f)]
    [SerializeField] private float minimumSpawnDistanceFromPlayer = 0f;

    [Min(0.1f)]
    [SerializeField] private float spawnPointOccupiedRadius = 1.5f;

    [SerializeField] private LayerMask enemyLayer;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Enemy Types In This Area")]
    [Tooltip("Bạn tự chọn loại enemy và số lượng của từng loại cho khu vực này.")]
    [SerializeField] private EnemySpawnEntry[] enemyTypes;

    [Header("Wave Settings")]
    [Min(1)]
    [SerializeField] private int maxWaves = 1;

    [Min(0f)]
    [SerializeField] private float nextWaveDelay = 3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<EnemyController> activeEnemies =
        new List<EnemyController>();

    // aliveByType: số con loại đó chưa bị giết, gồm cả con đang ở scene và chưa spawn.
    private int[] aliveByType;

    // loadedByType: số con loại đó hiện đang được lấy ra khỏi pool và có trong scene.
    private int[] loadedByType;

    private int currentWaveIndex;
    private bool areaActivated;
    private bool waitingForNextWave;
    private bool areaCompleted;

    private float nextWaveTimer;
    private float hiddenSpawnCheckTimer;
    private float offScreenTimer;

    public bool AreaCompleted => areaCompleted;
    public int CurrentWaveNumber => currentWaveIndex + 1;

    private void Awake()
    {
        ValidateSettings();
        InitializeWaveCounts();
    }

    private void Start()
    {
        FindReferences();
    }

    private void Update()
    {
        if (areaCompleted)
            return;

        if (player == null)
        {
            FindReferences();

            if (player == null)
                return;
        }

        RemoveNullEnemies();

        float playerSqrDistance =
            (player.position - transform.position).sqrMagnitude;

        float activationSqr =
            activationRadius * activationRadius;

        float despawnSqr =
            despawnRadius * despawnRadius;

        /*
         * Xử lý thời gian chờ wave tiếp theo.
         */
        HandleNextWave(
            playerSqrDistance,
            activationSqr);

        /*
         * Khi player vừa bước vào Activation Radius,
         * kích hoạt area và kiểm tra spawn ngay lập tức.
         */
        if (!areaActivated &&
            !waitingForNextWave &&
            playerSqrDistance <= activationSqr)
        {
            areaActivated = true;

            hiddenSpawnCheckTimer =
                hiddenSpawnCheckInterval;

            TrySpawnHiddenEnemies();
        }

        /*
         * Sau khi area đã kích hoạt, tiếp tục kiểm tra định kỳ.
         *
         * Những enemy chưa spawn sẽ xuất hiện khi có
         * SpawnPoint mới khuất camera và hợp lệ.
         */
        if (areaActivated &&
            !waitingForNextWave &&
            !areaCompleted)
        {
            hiddenSpawnCheckTimer -= Time.deltaTime;

            if (hiddenSpawnCheckTimer <= 0f)
            {
                hiddenSpawnCheckTimer =
                    hiddenSpawnCheckInterval;

                TrySpawnHiddenEnemies();
            }
        }

        /*
         * Player ra ngoài Despawn Radius:
         * chỉ thu hồi enemy khi chúng đều khuất camera
         * đủ thời gian.
         */
        if (areaActivated &&
            activeEnemies.Count > 0 &&
            playerSqrDistance > despawnSqr)
        {
            HandlePossibleUnload();
        }
        else
        {
            offScreenTimer = 0f;
        }
    }

    private void FindReferences()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
            else if (PoolManager.Instance != null)
                player = PoolManager.Instance.player;
        }

        if (gameplayCamera == null)
            gameplayCamera = Camera.main;
    }

    private void InitializeWaveCounts()
    {
        int typeCount = enemyTypes != null ? enemyTypes.Length : 0;

        aliveByType = new int[typeCount];
        loadedByType = new int[typeCount];

        ResetCountsForNewWave();
    }

    private void ResetCountsForNewWave()
    {
        if (enemyTypes == null)
            return;

        for (int i = 0; i < enemyTypes.Length; i++)
        {
            EnemySpawnEntry entry = enemyTypes[i];

            if (entry == null || entry.prefab == null)
            {
                aliveByType[i] = 0;
                loadedByType[i] = 0;
                continue;
            }

            aliveByType[i] = Mathf.Max(0, entry.amount);
            loadedByType[i] = 0;
        }
    }

    private void TrySpawnHiddenEnemies()
    {
        if (!areaActivated || areaCompleted || waitingForNextWave)
            return;

        if (PoolManager.Instance == null)
            return;

        if (GetTotalAliveEnemies() <= 0)
        {
            CompleteCurrentWave();
            return;
        }

        List<Transform> hiddenPoints = GetAvailableHiddenSpawnPoints();

        if (hiddenPoints.Count == 0)
            return;

        ShuffleSpawnPoints(hiddenPoints);

        int pointIndex = 0;

        for (int typeIndex = 0; typeIndex < enemyTypes.Length; typeIndex++)
        {
            EnemySpawnEntry entry = enemyTypes[typeIndex];

            if (entry == null || entry.prefab == null)
                continue;

            int needToLoad = aliveByType[typeIndex] - loadedByType[typeIndex];

            while (needToLoad > 0 && pointIndex < hiddenPoints.Count)
            {
                Transform spawnPoint = hiddenPoints[pointIndex];
                pointIndex++;

                if (SpawnEnemy(entry.prefab, typeIndex, spawnPoint))
                {
                    loadedByType[typeIndex]++;
                    needToLoad--;
                }
            }

            if (pointIndex >= hiddenPoints.Count)
                break;
        }
    }

    private bool SpawnEnemy(
    GameObject prefab,
    int typeIndex,
    Transform spawnPoint)
    {
        if (prefab == null ||
            spawnPoint == null)
        {
            return false;
        }

        /*
         * PoolManager đặt enemy vào đúng SpawnPoint
         * trước khi bật GameObject.
         */
        GameObject enemyObject =
            PoolManager.Instance.GetObject(
                prefab,
                spawnPoint.position,
                spawnPoint.rotation);

        if (enemyObject == null)
            return false;

        EnemyController enemy =
            enemyObject.GetComponent<EnemyController>();

        if (enemy == null)
        {
            Debug.LogError(
                $"{prefab.name} không có EnemyController.");

            PoolManager.Instance.ReturnObject(
                enemyObject);

            return false;
        }

        enemy.SetSpawnArea(
            this,
            typeIndex);

        enemy.OnSpawn(player);

        activeEnemies.Add(enemy);

        Log(
            $"Spawn {prefab.name} tại {spawnPoint.name}, " +
            $"vị trí {spawnPoint.position}.");

        return true;
    }

    private List<Transform> GetAvailableHiddenSpawnPoints()
    {
        List<Transform> result = new List<Transform>();

        if (spawnPoints == null)
            return result;

        float minimumSqrDistance =
            minimumSpawnDistanceFromPlayer * minimumSpawnDistanceFromPlayer;

        foreach (Transform point in spawnPoints)
        {
            if (point == null)
                continue;

            if (IsSpawnPointVisible(point))
                continue;

            if (IsSpawnPointOccupied(point))
                continue;

            if (player != null && minimumSpawnDistanceFromPlayer > 0f)
            {
                float playerSqrDistance =
                    (point.position - player.position).sqrMagnitude;

                if (playerSqrDistance < minimumSqrDistance)
                    continue;
            }

            result.Add(point);
        }

        return result;
    }

    private bool IsSpawnPointVisible(Transform spawnPoint)
    {
        if (spawnPoint == null)
            return false;

        if (gameplayCamera == null)
            gameplayCamera = Camera.main;

        // Không tìm thấy camera thì không spawn để tránh xuất hiện sai.
        if (gameplayCamera == null)
            return true;

        Vector3 pointPosition =
            spawnPoint.position + Vector3.up * 1f;

        Vector3 viewportPoint =
            gameplayCamera.WorldToViewportPoint(pointPosition);

        float padding = spawnPointVisiblePadding;

        /*
         * Điểm nằm phía sau camera:
         * camera chắc chắn không thấy.
         */
        if (viewportPoint.z <= 0f)
            return false;

        /*
         * Điểm nằm ngoài khung hình:
         * camera không thấy nên được phép spawn.
         */
        bool insideViewport =
            viewportPoint.x >= -padding &&
            viewportPoint.x <= 1f + padding &&
            viewportPoint.y >= -padding &&
            viewportPoint.y <= 1f + padding;

        if (!insideViewport)
            return false;

        /*
         * Điểm nằm trong khung camera nhưng có thể đang bị
         * tường, đá hoặc địa hình che khuất.
         */
        Vector3 cameraPosition =
            gameplayCamera.transform.position;

        Vector3 direction =
            pointPosition - cameraPosition;

        float distance =
            direction.magnitude;

        if (distance <= 0.01f)
            return true;

        direction.Normalize();

        /*
         * Raycast từ camera tới spawn point.
         * Nếu chạm vật cản trước khi tới điểm thì camera
         * thực tế không nhìn thấy spawn point.
         */
        if (Physics.Raycast(
            cameraPosition,
            direction,
            out RaycastHit hit,
            distance,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            /*
             * Nếu vật bị raycast trúng không phải spawn point
             * hoặc child của spawn point thì điểm đang bị che.
             */
            if (hit.transform != spawnPoint &&
                !hit.transform.IsChildOf(spawnPoint))
            {
                return false;
            }
        }

        /*
         * Điểm nằm trong camera và không bị che.
         */
        return true;
    }

    private bool IsSpawnPointOccupied(Transform spawnPoint)
    {
        Collider[] hits = Physics.OverlapSphere(
            spawnPoint.position,
            spawnPointOccupiedRadius,
            enemyLayer,
            QueryTriggerInteraction.Ignore);

        return hits.Length > 0;
    }


    /// <summary>
    /// Chỉ dùng để test nhanh trong Editor hoặc bản Build.
    /// Thu hồi toàn bộ enemy đang hoạt động và đánh dấu toàn bộ area/wave đã hoàn thành
    /// để Portal có thể sử dụng ngay.
    /// </summary>
    public void ForceCompleteForTesting()
    {
        if (areaCompleted)
            return;

        // Thu hồi các enemy đang được area này quản lý ngay lập tức.
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyController enemy = activeEnemies[i];

            if (enemy == null)
                continue;

            enemy.ClearSpawnArea();

            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.ReturnObject(enemy.gameObject);
            }
            else
            {
                enemy.gameObject.SetActive(false);
            }
        }

        activeEnemies.Clear();

        // Xóa cả enemy chưa spawn và các wave còn lại.
        if (aliveByType != null)
        {
            for (int i = 0; i < aliveByType.Length; i++)
                aliveByType[i] = 0;
        }

        if (loadedByType != null)
        {
            for (int i = 0; i < loadedByType.Length; i++)
                loadedByType[i] = 0;
        }

        currentWaveIndex = maxWaves;
        areaActivated = false;
        waitingForNextWave = false;
        areaCompleted = true;

        nextWaveTimer = 0f;
        hiddenSpawnCheckTimer = 0f;
        offScreenTimer = 0f;

        Log("[BUILD TEST] Shift + R: khu vực đã được hoàn thành cưỡng bức.");
    }

    public void NotifyEnemyKilled(
        EnemyController enemy,
        int enemyTypeIndex)
    {
        if (enemy == null)
            return;

        if (!activeEnemies.Remove(enemy))
            return;

        if (IsValidTypeIndex(enemyTypeIndex))
        {
            aliveByType[enemyTypeIndex] =
                Mathf.Max(0, aliveByType[enemyTypeIndex] - 1);

            loadedByType[enemyTypeIndex] =
                Mathf.Max(0, loadedByType[enemyTypeIndex] - 1);
        }

        if (PoolManager.Instance != null)
            PoolManager.Instance.ReturnObject(enemy.gameObject);

        Log($"Enemy bị tiêu diệt. Wave còn {GetTotalAliveEnemies()} con.");

        if (GetTotalAliveEnemies() <= 0)
            CompleteCurrentWave();
    }

    private void CompleteCurrentWave()
    {
        areaActivated = false;
        waitingForNextWave = false;
        activeEnemies.Clear();
        offScreenTimer = 0f;

        currentWaveIndex++;

        if (currentWaveIndex >= maxWaves)
        {
            areaCompleted = true;
            Log("Đã tiêu diệt hết toàn bộ khu vực.");
            return;
        }

        ResetCountsForNewWave();

        waitingForNextWave = true;
        nextWaveTimer = nextWaveDelay;

        Log($"Hoàn thành wave {currentWaveIndex}. Chờ wave tiếp theo.");
    }

    private void HandleNextWave(
        float playerSqrDistance,
        float activationSqr)
    {
        if (!waitingForNextWave)
            return;

        // Chỉ đếm thời gian khi player còn ở gần area.
        if (playerSqrDistance > activationSqr)
            return;

        nextWaveTimer -= Time.deltaTime;

        if (nextWaveTimer > 0f)
            return;

        waitingForNextWave = false;
        areaActivated = true;
        hiddenSpawnCheckTimer = 0f;

        Log($"Bắt đầu wave {currentWaveIndex + 1}/{maxWaves}.");
    }

    private void HandlePossibleUnload()
    {
        if (IsAnyEnemyVisible())
        {
            offScreenTimer = 0f;
            return;
        }

        offScreenTimer += Time.deltaTime;

        if (offScreenTimer < offScreenDespawnDelay)
            return;

        UnloadAliveEnemies();
    }

    private bool IsAnyEnemyVisible()
    {
        if (gameplayCamera == null)
            gameplayCamera = Camera.main;

        // Không có camera thì không thu hồi để tránh enemy biến mất sai.
        if (gameplayCamera == null)
            return true;

        Plane[] cameraPlanes =
            GeometryUtility.CalculateFrustumPlanes(gameplayCamera);

        foreach (EnemyController enemy in activeEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>();

            foreach (Renderer enemyRenderer in renderers)
            {
                if (enemyRenderer == null || !enemyRenderer.enabled)
                    continue;

                if (GeometryUtility.TestPlanesAABB(
                    cameraPlanes,
                    enemyRenderer.bounds))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void UnloadAliveEnemies()
    {
        if (PoolManager.Instance == null)
            return;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyController enemy = activeEnemies[i];

            if (enemy == null)
                continue;

            int typeIndex = enemy.SpawnTypeIndex;

            if (IsValidTypeIndex(typeIndex))
            {
                loadedByType[typeIndex] =
                    Mathf.Max(0, loadedByType[typeIndex] - 1);
            }

            enemy.ClearSpawnArea();
            PoolManager.Instance.ReturnObject(enemy.gameObject);
        }

        activeEnemies.Clear();
        areaActivated = false;
        offScreenTimer = 0f;

        Log($"Đã thu hồi enemy. Wave vẫn còn {GetTotalAliveEnemies()} con.");
    }

    private int GetTotalAliveEnemies()
    {
        if (aliveByType == null)
            return 0;

        int total = 0;

        foreach (int amount in aliveByType)
            total += amount;

        return total;
    }

    private bool IsValidTypeIndex(int index)
    {
        return index >= 0 &&
               aliveByType != null &&
               loadedByType != null &&
               index < aliveByType.Length &&
               index < loadedByType.Length;
    }

    private void RemoveNullEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
                activeEnemies.RemoveAt(i);
        }
    }

    private void ShuffleSpawnPoints(List<Transform> points)
    {
        for (int i = points.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            Transform temp = points[i];
            points[i] = points[randomIndex];
            points[randomIndex] = temp;
        }
    }

    private void ValidateSettings()
    {
        if (despawnRadius <= activationRadius)
        {
            despawnRadius = activationRadius + 10f;

            Debug.LogWarning(
                $"{name}: Despawn Radius phải lớn hơn Activation Radius. " +
                "Đã tự động tăng Despawn Radius.");
        }

        maxWaves = Mathf.Max(1, maxWaves);
        hiddenSpawnCheckInterval = Mathf.Max(0.05f, hiddenSpawnCheckInterval);
    }

    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log($"{name}: {message}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, despawnRadius);

        if (spawnPoints == null)
            return;

        Gizmos.color = Color.cyan;

        foreach (Transform point in spawnPoints)
        {
            if (point == null)
                continue;

            Gizmos.DrawWireSphere(point.position, spawnPointOccupiedRadius);
        }
    }
}
