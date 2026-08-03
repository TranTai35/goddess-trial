using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Giữ lại để BossController và các script cũ có thể lấy Player.")]
    public Transform player;

    [Header("Enemy Pools")]
    public PoolData[] enemyPools;

    [Header("Projectile Pools")]
    public PoolData[] projectilePools;

    [Header("UI Pools")]
    public PoolData[] uiPools;

    private readonly Dictionary<GameObject, Queue<GameObject>> pools =
        new Dictionary<GameObject, Queue<GameObject>>();

    private readonly Dictionary<GameObject, GameObject> prefabLookup =
        new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializePools(enemyPools);
        InitializePools(projectilePools);
        InitializePools(uiPools);
    }

    private void InitializePools(PoolData[] configs)
    {
        if (configs == null)
            return;

        foreach (PoolData config in configs)
        {
            if (config == null || config.prefab == null)
                continue;

            if (!pools.TryGetValue(config.prefab, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                pools.Add(config.prefab, queue);
            }

            int amount = Mathf.Max(0, config.prewarmCount);

            for (int i = 0; i < amount; i++)
            {
                GameObject pooledObject = CreateObject(config.prefab);
                pooledObject.SetActive(false);
                queue.Enqueue(pooledObject);
            }
        }
    }

    private GameObject CreateObject(GameObject prefab)
    {
        GameObject pooledObject = Instantiate(prefab);

        NavMeshAgent agent =
            pooledObject.GetComponent<NavMeshAgent>();

        if (agent != null)
            agent.enabled = false;

        pooledObject.SetActive(false);

        prefabLookup[pooledObject] = prefab;

        return pooledObject;
    }

    private GameObject CreateObject(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation)
    {
        GameObject pooledObject =
            Instantiate(prefab, position, rotation);

        NavMeshAgent agent =
            pooledObject.GetComponent<NavMeshAgent>();

        if (agent != null)
            agent.enabled = false;

        prefabLookup[pooledObject] = prefab;

        return pooledObject;
    }

    public GameObject GetObject(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("PoolManager.GetObject nhận prefab null.");
            return null;
        }

        if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools.Add(prefab, queue);
        }

        GameObject pooledObject =
            queue.Count > 0
                ? queue.Dequeue()
                : CreateObject(prefab);

        pooledObject.SetActive(true);
        return pooledObject;
    }
    public GameObject GetObject(
    GameObject prefab,
    Vector3 position,
    Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError(
                "PoolManager.GetObject nhận prefab null.");

            return null;
        }

        if (!pools.TryGetValue(
            prefab,
            out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools.Add(prefab, queue);
        }

        GameObject pooledObject;

        if (queue.Count > 0)
        {
            pooledObject = queue.Dequeue();

            pooledObject.transform.SetPositionAndRotation(
                position,
                rotation);
        }
        else
        {
            pooledObject = CreateObject(
                prefab,
                position,
                rotation);
        }

        pooledObject.SetActive(true);

        return pooledObject;
    }
    public void ReturnObject(GameObject pooledObject)
    {
        if (pooledObject == null)
            return;

        if (!prefabLookup.TryGetValue(pooledObject, out GameObject prefab))
        {
            Debug.LogWarning(
                $"{pooledObject.name} không được tạo bởi PoolManager nên sẽ bị Destroy.");

            Destroy(pooledObject);
            return;
        }

        pooledObject.SetActive(false);

        if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools.Add(prefab, queue);
        }

        queue.Enqueue(pooledObject);
    }
}
