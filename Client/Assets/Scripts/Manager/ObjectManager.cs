using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global object registry. Manages create/remove/find of gameplay objects.
/// This is a lightweight non-Mono singleton to avoid scene dependencies.
/// </summary>
public class ObjectManager
{
    private static ObjectManager _instance;
    public static ObjectManager Instance => _instance ??= new ObjectManager();
    public static bool HasInstance => _instance != null;

    private readonly Dictionary<int, ObjectBase> _uidToObject = new Dictionary<int, ObjectBase>();
    private readonly Dictionary<ObjectType, HashSet<ObjectBase>> _typeToObjects = new Dictionary<ObjectType, HashSet<ObjectBase>>();

    private ObjectManager() { }

    public void Register(ObjectBase obj)
    {
        if (obj == null) return;
        
        // Check for Uid conflicts
        if (_uidToObject.ContainsKey(obj.Uid) && _uidToObject[obj.Uid] != obj)
        {
            Debug.LogWarning($"[ObjectManager] Uid conflict detected: {obj.Uid} for {obj.name}. Generating new Uid.");
            obj.SetUid(ResourceUtils.GenerateUid());
        }
        
        _uidToObject[obj.Uid] = obj;

        if (!_typeToObjects.TryGetValue(obj.ObjectType, out var set))
        {
            set = new HashSet<ObjectBase>();
            _typeToObjects[obj.ObjectType] = set;
        }
        set.Add(obj);
    }

    public void Unregister(ObjectBase obj)
    {
        if (obj == null) return;
        _uidToObject.Remove(obj.Uid);
        if (_typeToObjects.TryGetValue(obj.ObjectType, out var set))
        {
            set.Remove(obj);
            if (set.Count == 0)
            {
                _typeToObjects.Remove(obj.ObjectType);
            }
        }
    }

    public ObjectBase FindByUid(int uid)
    {
        _uidToObject.TryGetValue(uid, out var obj);
        return obj;
    }

    public T FindByUid<T>(int uid) where T : ObjectBase
    {
        return FindByUid(uid) as T;
    }

    public IEnumerable<ObjectBase> FindAllByType(ObjectType type)
    {
        if (_typeToObjects.TryGetValue(type, out var set))
        {
            return set;
        }
        return System.Array.Empty<ObjectBase>();
    }

    public IEnumerable<T> FindAllByType<T>(ObjectType type) where T : ObjectBase
    {
        foreach (var obj in FindAllByType(type))
        {
            if (obj is T t) yield return t;
        }
    }

    /// <summary>
    /// Get total registered object count
    /// </summary>
    public int GetTotalObjectCount()
    {
        return _uidToObject.Count;
    }

    /// <summary>
    /// Get object count by type
    /// </summary>
    public int GetObjectCountByType(ObjectType type)
    {
        return _typeToObjects.TryGetValue(type, out var set) ? set.Count : 0;
    }

    /// <summary>
    /// Clear all registered objects (use with caution, typically for scene transitions)
    /// </summary>
    public void ClearAll()
    {
        _uidToObject.Clear();
        _typeToObjects.Clear();
        Debug.Log("[ObjectManager] All objects cleared");
    }

    /// <summary>
    /// Remove null references (cleanup destroyed objects that weren't properly unregistered)
    /// </summary>
    public void CleanupNullReferences()
    {
        var toRemove = new List<int>();
        foreach (var kvp in _uidToObject)
        {
            if (kvp.Value == null)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var uid in toRemove)
        {
            _uidToObject.Remove(uid);
        }

        // Clean type dictionaries
        var typesToRemove = new List<ObjectType>();
        foreach (var kvp in _typeToObjects)
        {
            kvp.Value.RemoveWhere(obj => obj == null);
            if (kvp.Value.Count == 0)
            {
                typesToRemove.Add(kvp.Key);
            }
        }

        foreach (var type in typesToRemove)
        {
            _typeToObjects.Remove(type);
        }

        if (toRemove.Count > 0)
        {
            Debug.Log($"[ObjectManager] Cleaned up {toRemove.Count} null references");
        }
    }
} 