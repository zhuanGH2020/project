using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Object pool object wrapper
/// </summary>
public class PooledObject
{
    public GameObject GameObject { get; set; }
    public float LastUsedTime { get; set; }
} 

/// <summary>
/// Generic object pool that can manage multiple prefab types
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [Header("Pool Configuration")]
    [SerializeField] private int maxPoolSize = 100;        // Maximum size for each prefab pool
    [SerializeField] private float maxUnusedTime = 30f;    // Maximum time an object can stay unused in pool

    // Pool dictionary for each prefab type
    private Dictionary<string, Queue<PooledObject>> objectPools = new Dictionary<string, Queue<PooledObject>>();
    private Dictionary<string, int> activeCounts = new Dictionary<string, int>();

    /// <summary>
    /// Get an object from pool
    /// </summary>
    public GameObject Get(string prefabPath)
    {
        EnsurePoolExists(prefabPath);

        var pool = objectPools[prefabPath];
        GameObject obj = null;

        while (pool.Count > 0 && obj == null)
        {
            var pooledObj = pool.Dequeue();
            if (pooledObj.GameObject != null)
            {
                obj = pooledObj.GameObject;
                obj.SetActive(true);
                pooledObj.LastUsedTime = Time.time;
            }
            else
            {
                activeCounts[prefabPath]--;
            }
        }

        if (obj != null)
        {
            activeCounts[prefabPath]++;
        }

        return obj;
    }

    /// <summary>
    /// Return an object to pool
    /// </summary>
    public void Return(GameObject obj, string prefabPath)
    {
        if (obj == null) return;

        EnsurePoolExists(prefabPath);

        var pool = objectPools[prefabPath];

        if (pool.Count >= maxPoolSize)
        {
            Destroy(obj);
            activeCounts[prefabPath]--;
            return;
        }

        ResetObject(obj);

        pool.Enqueue(new PooledObject 
        { 
            GameObject = obj,
            LastUsedTime = Time.time
        });
    }

    /// <summary>
    /// Ensure pool exists for the prefab
    /// </summary>
    private void EnsurePoolExists(string prefabPath)
    {
        if (!objectPools.ContainsKey(prefabPath))
        {
            objectPools[prefabPath] = new Queue<PooledObject>();
            activeCounts[prefabPath] = 0;
        }
    }

    /// <summary>
    /// Reset object state
    /// </summary>
    private void ResetObject(GameObject obj)
    {
        obj.GetComponents<MonoBehaviour>().ToList().ForEach(mb => mb.StopAllCoroutines());

        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        obj.SetActive(false);
    }

    /// <summary>
    /// Cleanup unused objects
    /// </summary>
    public void Cleanup()
    {
        var currentTime = Time.time;

        foreach (var prefabPath in objectPools.Keys.ToList())
        {
            var pool = objectPools[prefabPath];
            var objectsToKeep = new Queue<PooledObject>();

            while (pool.Count > 0)
            {
                var pooledObj = pool.Dequeue();
                
                if (pooledObj.GameObject == null || 
                    (currentTime - pooledObj.LastUsedTime > maxUnusedTime))
                {
                    if (pooledObj.GameObject != null)
                    {
                        Destroy(pooledObj.GameObject);
                    }
                    activeCounts[prefabPath]--;
                }
                else
                {
                    objectsToKeep.Enqueue(pooledObj);
                }
            }

            objectPools[prefabPath] = objectsToKeep;
        }
    }

    /// <summary>
    /// Preload objects into pool
    /// </summary>
    public void Preload(string prefabPath, GameObject prefab, int count)
    {
        count = Mathf.Min(count, maxPoolSize);
        EnsurePoolExists(prefabPath);

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab);
            Return(obj, prefabPath);
        }
    }

    /// <summary>
    /// Clear specific prefab pool
    /// </summary>
    public void Clear(string prefabPath)
    {
        if (objectPools.TryGetValue(prefabPath, out var pool))
        {
            while (pool.Count > 0)
            {
                var pooledObj = pool.Dequeue();
                if (pooledObj.GameObject != null)
                {
                    Destroy(pooledObj.GameObject);
                }
            }
            activeCounts[prefabPath] = 0;
        }
    }

    /// <summary>
    /// Clear all pools
    /// </summary>
    public void ClearAll()
    {
        foreach (var pool in objectPools.Values)
        {
            while (pool.Count > 0)
            {
                var pooledObj = pool.Dequeue();
                if (pooledObj.GameObject != null)
                {
                    Destroy(pooledObj.GameObject);
                }
            }
        }
        objectPools.Clear();
        activeCounts.Clear();
    }

    private void OnDestroy()
    {
        ClearAll();
    }
} 