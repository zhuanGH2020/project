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
    [SerializeField] private int _configId = 0;            // 配置表ID
    [SerializeField] private ObjectType _objectType = ObjectType.Other; // Object category

    public int Uid => _uid;
    public int ConfigId => _configId;
    public ObjectType ObjectType => _objectType;
    public Vector3 Position => transform.position;

    protected virtual void Awake()
    {
        // Ensure Uid exists
        if (_uid == 0)
        {
            _uid = ResourceUtils.GenerateUid();
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

    public void SetConfigId(int configId)
    {
        _configId = configId;
    }

    public void SetObjectType(ObjectType type)
    {
        _objectType = type;
    }

    /// <summary>
    /// 根据对象类型获取对应的配置表读取器
    /// </summary>
    /// <returns>配置表读取器，如果类型不支持或加载失败则返回null</returns>
    public ConfigReader GetConfig()
    {
        // 对于Other类型，不支持配置表
        if (_objectType == ObjectType.Other)
        {
            Debug.LogWarning($"ObjectType.Other does not support config table");
            return null;
        }

        string configName = _objectType.ToString();
        return ConfigManager.Instance.GetReader(configName);
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