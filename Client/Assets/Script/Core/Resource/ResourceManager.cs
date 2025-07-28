using UnityEngine;
using System.Collections.Generic;

public class ResourceManager
{
    private static ResourceManager _instance;
    public static ResourceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ResourceManager();
            }
            return _instance;
        }
    }

    private Dictionary<string, ResourceCache> _cacheDict = new Dictionary<string, ResourceCache>();

    private ResourceManager() { }

    public T Load<T>(string path) where T : UnityEngine.Object
    {
        if (_cacheDict.TryGetValue(path, out ResourceCache cache))
        {
            cache.Retain();
            return cache.Resource as T;
        }

        T resource = Resources.Load<T>(path);
        if (resource != null)
        {
            _cacheDict[path] = new ResourceCache(resource, path);
            Debug.Log($"[ResourceManager] Load resource: {path}");
            return resource;
        }
        
        Debug.LogError($"[ResourceManager] Failed to load: {path}");
        return null;
    }

    public void Release(UnityEngine.Object obj)
    {
        if (obj == null) return;

        string keyToRemove = null;
        ResourceCache cacheToCheck = null;

        foreach (var pair in _cacheDict)
        {
            if (ReferenceEquals(pair.Value.Resource, obj))
            {
                keyToRemove = pair.Key;
                cacheToCheck = pair.Value;
                break;
            }
        }

        if (cacheToCheck != null)
        {
            if (cacheToCheck.Release())
            {
                _cacheDict.Remove(keyToRemove);
                Debug.Log($"[ResourceManager] Unload resource: {keyToRemove}");
            }
        }
    }

    public void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
    }
} 