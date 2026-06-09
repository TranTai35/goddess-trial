using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance;

    [Header("References")]
    public EnemyController enemyPrefab;
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

    private Queue<EnemyController> pool = new Queue<EnemyController>();
    private int activeEnemies;
    private int totalKilled;
    private bool secondWaveSpawned;
    private bool levelCompleted;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnWave(firstWaveCount);
    }

    private EnemyController GetEnemy()
    {
        if (pool.Count > 0)
        {
            EnemyController pooledEnemy = pool.Dequeue();
            pooledEnemy.gameObject.SetActive(true);
            return pooledEnemy;
        }

        EnemyController newEnemy = Instantiate(enemyPrefab);
        return newEnemy;
    }

    private void SpawnWave(int count)
    {
        activeEnemies = count;

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            EnemyController enemy = GetEnemy();

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

        enemy.gameObject.SetActive(false);
        pool.Enqueue(enemy);

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
        Debug.Log($"Second wave in {nextWaveDelay}s");
        yield return new WaitForSeconds(nextWaveDelay);

        SpawnWave(secondWaveCount);
    }

    private void LevelCompleted()
    {
        levelCompleted = true;
        Debug.Log("LEVEL COMPLETE!");

        // Ví dụ bổ sung logic sau khi thắng:
        // winPanel.SetActive(true);
        // SceneManager.LoadScene(nextScene);
        // portal.SetActive(true);
    }

    public int GetKillCount()
    {
        return totalKilled;
    }

    public int GetRemainingKills()
    {
        return totalKillRequired - totalKilled;
    }
}