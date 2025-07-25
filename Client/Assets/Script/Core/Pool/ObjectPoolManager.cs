using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager for different types of object pools
/// </summary>
public class ObjectPoolManager<T> : MonoBehaviour where T : Enum
{
    private static ObjectPoolManager<T> instance;
    public static ObjectPoolManager<T> Instance => instance;

    // Pool configurations
    private Dictionary<T, ObjectPool> pools = new Dictionary<T, ObjectPool>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Get or create object pool by type
    /// </summary>
    public ObjectPool GetPool(T poolType)
    {
        if (pools.TryGetValue(poolType, out ObjectPool pool))
        {
            return pool;
        }

        var poolObject = new GameObject(poolType.ToString() + "Pool");
        poolObject.transform.SetParent(transform);
        pool = poolObject.AddComponent<ObjectPool>();
        pools[poolType] = pool;
        return pool;
    }

    /// <summary>
    /// Clear specified pool
    /// </summary>
    public void ClearPool(T poolType)
    {
        if (pools.TryGetValue(poolType, out ObjectPool pool))
        {
            pool.ClearAll();
            Destroy(pool.gameObject);
            pools.Remove(poolType);
        }
    }

    /// <summary>
    /// Clear all pools
    /// </summary>
    public void ClearAll()
    {
        foreach (var pool in pools.Values)
        {
            pool.ClearAll();
            Destroy(pool.gameObject);
        }
        pools.Clear();
    }

    private void OnDestroy()
    {
        ClearAll();
    }
} 