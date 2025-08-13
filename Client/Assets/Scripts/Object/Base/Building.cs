using UnityEngine;
using System;

/// <summary>
/// 建筑物组件 - 管理建筑物的数据和行为
/// 挂载到建筑物预制体上，用于管理唯一标识、等级、交互状态等
/// </summary>
public class Building : DamageableObject
{
    [Header("建筑物基本信息")]
    // 使用基类 ObjectBase 的 Uid，不再在本类重复序列化 _uid，避免重复序列化报错
    [SerializeField] private int _itemId = -1;               // 对应的道具ID
    [SerializeField] private Vector2 _mapPosition;           // 地图位置
    
    [Header("建筑物状态")]
    [SerializeField] private int _level = 1;                 // 建筑物等级
    [SerializeField] private bool _canInteract = true;       // 是否可交互
    [SerializeField] private float _constructTime;          // 建造时间
    [SerializeField] private bool _isConstructed = true;     // 是否建造完成
    
    // 公共属性

    public int ItemId => _itemId;
    public Vector2 MapPosition => _mapPosition;
    public int Level => _level;
    public bool CanInteract => _canInteract;
    public float ConstructTime => _constructTime;
    public bool IsConstructed => _isConstructed;
    
    // 事件
    public event Action<Building> OnDemolished;
    public event Action<Building, int> OnLevelChanged;
    public event Action<Building, bool> OnInteractStateChanged;

    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Building);
    }
    
    /// <summary>
    /// 初始化建筑物
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="mapPos">地图位置</param>
    /// <param name="uid">唯一标识符，0表示自动生成</param>
    public void Initialize(int itemId, Vector2 mapPos, int uid = 0)
    {
        _itemId = itemId;
        _mapPosition = mapPos;
        if (uid > 0)
        {
            SetUid(uid);
        }
        else if (Uid == 0)
        {
            SetUid(ResourceUtils.GenerateUid());
        }
        _constructTime = Time.time;
        
        LoadBuildingConfig();
        UpdateGameObjectName();
    }
    
    /// <summary>
    /// 加载建筑物配置
    /// </summary>
    private void LoadBuildingConfig()
    {
        var reader = ConfigManager.Instance.GetReader("Item");
        if (reader != null)
        {
            // 从配置中加载建筑物属性
            _maxHealth = reader.GetValue<float>(_itemId, "MaxHealth", 100f);
            _currentHealth = _maxHealth;
            
            // 其他配置可以在这里加载
        }
    }
    
    /// <summary>
    /// 更新GameObject名称
    /// </summary>
    private void UpdateGameObjectName()
    {
        gameObject.name = $"{GetPrefabName()}_{Uid}";
    }
    
    /// <summary>
    /// 获取建筑物名称
    /// </summary>
    public string GetBuildingName()
    {
        return ResourceUtils.GetItemName(_itemId);
    }
    
    /// <summary>
    /// 获取预制体名称
    /// </summary>
    public string GetPrefabName()
    {
        // 获取当前GameObject的名称（预制体名称），移除Unity自动添加的"(Clone)"后缀
        string currentName = gameObject.name;
        if (currentName.EndsWith("(Clone)"))
        {
            currentName = currentName.Replace("(Clone)", "");
        }
        return currentName;
    }
    
    /// <summary>
    /// 升级建筑物
    /// </summary>
    public bool UpgradeLevel()
    {
        int maxLevel = GetMaxLevel();
        if (_level >= maxLevel)
        {
            return false;
        }
        
        int oldLevel = _level;
        _level++;
        OnLevelChanged?.Invoke(this, oldLevel);
        return true;
    }
    
    /// <summary>
    /// 设置交互状态
    /// </summary>
    public void SetInteractable(bool canInteract)
    {
        if (_canInteract != canInteract)
        {
            _canInteract = canInteract;
            OnInteractStateChanged?.Invoke(this, canInteract);
        }
    }
    
    /// <summary>
    /// 获取最大等级
    /// </summary>
    public int GetMaxLevel()
    {
        var reader = ConfigManager.Instance.GetReader("Item");
        return reader?.GetValue<int>(_itemId, "MaxLevel", 5) ?? 5;
    }
    
    /// <summary>
    /// 重写死亡处理
    /// </summary>
    protected override void OnDeath()
    {
        base.OnDeath();
        OnBuildingDestroyed();
    }
    
    /// <summary>
    /// 建筑物被摧毁
    /// </summary>
    private void OnBuildingDestroyed()
    {
        Demolish();
    }
    
    /// <summary>
    /// 拆除建筑物
    /// </summary>
    public void Demolish()
    {
        OnDemolished?.Invoke(this);
        MapModel.Instance.RemoveBuildingByUid(Uid);
        Destroy(gameObject);
    }
} 