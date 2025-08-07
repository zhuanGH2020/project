using UnityEngine;
using System;

/// <summary>
/// 建筑物组件 - 管理建筑物的数据和行为
/// 挂载到建筑物预制体上，用于管理唯一标识、等级、交互状态等
/// </summary>
public class Building : DamageableObject
{
    [Header("建筑物基本信息")]
    [SerializeField] private int _uid = 0;                    // 唯一标识符
    [SerializeField] private int _itemId = -1;               // 对应的道具ID
    [SerializeField] private Vector2 _mapPosition;           // 地图位置
    
    [Header("建筑物状态")]
    [SerializeField] private int _level = 1;                 // 建筑物等级
    [SerializeField] private bool _canInteract = true;       // 是否可交互
    [SerializeField] private float _constructTime;          // 建造时间
    [SerializeField] private bool _isConstructed = true;     // 是否建造完成
    
    // 公共属性
    public int UID => _uid;
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
        _uid = uid > 0 ? uid : GenerateUID();
        _constructTime = Time.time;
        
        LoadBuildingConfig();
        UpdateGameObjectName();
        
        Debug.Log($"[Building] 初始化建筑物: {GetBuildingName()} (UID: {_uid})");
    }
    
    /// <summary>
    /// 生成唯一标识符（使用ResourceUtils）
    /// </summary>
    private int GenerateUID()
    {
        return ResourceUtils.GenerateUID();
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
        gameObject.name = $"{GetBuildingName()}_{_uid}";
    }
    
    /// <summary>
    /// 获取建筑物名称
    /// </summary>
    public string GetBuildingName()
    {
        return ResourceUtils.GetItemName(_itemId);
    }
    
    /// <summary>
    /// 升级建筑物
    /// </summary>
    public bool UpgradeLevel()
    {
        int maxLevel = GetMaxLevel();
        if (_level >= maxLevel)
        {
            Debug.LogWarning($"[Building] 建筑物已达到最大等级: {_level}");
            return false;
        }
        
        int oldLevel = _level;
        _level++;
        OnLevelChanged?.Invoke(this, oldLevel);
        
        Debug.Log($"[Building] 建筑物升级: {GetBuildingName()} {oldLevel} -> {_level}");
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
            Debug.Log($"[Building] 建筑物交互状态改变: {GetBuildingName()} -> {canInteract}");
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
    /// 修复建筑物
    /// </summary>
    public void Repair(float repairAmount = -1)
    {
        if (repairAmount < 0)
        {
            _currentHealth = _maxHealth; // 完全修复
        }
        else
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + repairAmount);
        }
        
        Debug.Log($"[Building] 建筑物修复: {GetBuildingName()} ({_currentHealth}/{_maxHealth})");
    }
    
    /// <summary>
    /// 建筑物被摧毁
    /// </summary>
    private void OnBuildingDestroyed()
    {
        Debug.Log($"[Building] 建筑物被摧毁: {GetBuildingName()} (UID: {_uid})");
        Demolish();
    }
    
    /// <summary>
    /// 拆除建筑物
    /// </summary>
    public void Demolish()
    {
        OnDemolished?.Invoke(this);
        MapModel.Instance.RemoveBuildingByUID(_uid);
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 获取建筑物详细信息
    /// </summary>
    public string GetBuildingInfo()
    {
        return $"建筑物: {GetBuildingName()}\n" +
               $"等级: {_level}/{GetMaxLevel()}\n" +
               $"血量: {_currentHealth:F0}/{_maxHealth:F0}\n" +
               $"位置: ({_mapPosition.x:F1}, {_mapPosition.y:F1})\n" +
               $"可交互: {(_canInteract ? "是" : "否")}\n" +
               $"UID: {_uid}";
    }
} 