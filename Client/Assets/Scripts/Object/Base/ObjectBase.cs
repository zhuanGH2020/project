using UnityEngine;

/// <summary>
/// Base class for all in-game objects.
/// Provides uid, type and common utilities, and auto-registers with ObjectManager.
/// Attach to any gameplay object that should be tracked by the global manager.
/// </summary>
public abstract class ObjectBase : MonoBehaviour
{
    [Header("Object Identity")]
    [SerializeField] private int _uid = 0;                 // Unique identifier
    [SerializeField] private ObjectType _objectType = ObjectType.Other; // Object category

    public int Uid => _uid;
    public ObjectType ObjectType => _objectType;
    public Vector3 Position => transform.position;

    protected virtual void Awake()
    {
        // Ensure UID exists
        if (_uid == 0)
        {
            _uid = ResourceUtils.GenerateUID();
        }
    }

    protected virtual void OnEnable()
    {
        // Lazy create manager on first access
        ObjectManager.Instance?.Register(this);
    }

    protected virtual void OnDisable()
    {
        if (ObjectManager.HasInstance)
        {
            ObjectManager.Instance.Unregister(this);
        }
    }

    public void SetUid(int uid)
    {
        if (_uid != 0 && _uid == uid) return;
        _uid = uid;
    }

    public void SetObjectType(ObjectType type)
    {
        _objectType = type;
    }

    public T GetOrAddComponent<T>() where T : Component
    {
        var c = GetComponent<T>();
        return c != null ? c : gameObject.AddComponent<T>();
    }

    public ObjectState GetObjectState()
    {
        return GetComponent<ObjectState>();
    }
} 