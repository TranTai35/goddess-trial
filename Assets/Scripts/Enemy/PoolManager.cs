
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Enemy Pools")]
    public PoolData[] enemyPools;

    [Header("Projectile Pools")]
    public PoolData[] projectilePools;

    [Header("UI Pools")]
    public PoolData[] uiPools;

    [Header("References")]
    public Transform player;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Wave Settings")]
    public int firstWaveCount = 10;
    public int secondWaveCount = 10;

    [Header("Level Goal")]
    public int totalKillRequired = 20;

    [Header("Respawn")]
    public float nextWaveDelay = 5f;

    private readonly Dictionary<GameObject, Queue<GameObject>> pools =
        new Dictionary<GameObject, Queue<GameObject>>();

    private readonly Dictionary<GameObject, GameObject> prefabLookup =
        new Dictionary<GameObject, GameObject>();

    private int activeEnemies;
    private int totalKilled;
    private bool secondWaveSpawned;
    private bool levelCompleted;

    private void Awake()
    {
        Instance = this;
        InitializePools(enemyPools);
        InitializePools(projectilePools);
    }

    private void Start()
    {
        SpawnWave(firstWaveCount);
    }

    private void InitializePools(PoolData[] configs)
    {
        foreach (PoolData cfg in configs)
        {
            Queue<GameObject> q = new Queue<GameObject>();

            for (int i = 0; i < cfg.prewarmCount; i++)
            {
                GameObject obj = Instantiate(cfg.prefab);
                obj.SetActive(false);
                prefabLookup[obj] = cfg.prefab;
                q.Enqueue(obj);
            }

            pools[cfg.prefab] = q;
        }
    }

    public GameObject GetObject(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
        }

        Queue<GameObject> q = pools[prefab];
        GameObject obj;

        if (q.Count > 0)
        {
            obj = q.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab);
            prefabLookup[obj] = prefab;
        }

        obj.SetActive(true);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        if (!prefabLookup.ContainsKey(obj))
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        GameObject prefab = prefabLookup[obj];
        pools[prefab].Enqueue(obj);
    }

    private void SpawnWave(int count)
    {
        activeEnemies = count;

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint =
                spawnPoints[Random.Range(0, spawnPoints.Length)];

            PoolData pool =
                enemyPools[Random.Range(0, enemyPools.Length)];

            GameObject obj = GetObject(pool.prefab);

            EnemyController enemy =
                obj.GetComponent<EnemyController>();

            enemy.transform.position = spawnPoint.position;
            enemy.transform.rotation = spawnPoint.rotation;
            enemy.OnSpawn(player);
        }

        Debug.Log($"Spawned Wave ({count})");
    }

    public void EnemyKilled(EnemyController enemy)
    {
        if (levelCompleted)
            return;

        totalKilled++;
        activeEnemies--;

        ReturnObject(enemy.gameObject);

        Debug.Log($"Killed: {totalKilled}/{totalKillRequired}");

        if (totalKilled >= totalKillRequired)
        {
            LevelCompleted();
            return;
        }

        if (activeEnemies <= 0 && !secondWaveSpawned)
        {
            secondWaveSpawned = true;
            StartCoroutine(SpawnSecondWaveRoutine());
        }
    }

    private IEnumerator SpawnSecondWaveRoutine()
    {
        yield return new WaitForSeconds(nextWaveDelay);
        SpawnWave(secondWaveCount);
    }

    private void LevelCompleted()
    {
        levelCompleted = true;
        Debug.Log("LEVEL COMPLETE!");
    }

    public int GetKillCount() => totalKilled;

    public int GetRemainingKills() => totalKillRequired - totalKilled;
}
