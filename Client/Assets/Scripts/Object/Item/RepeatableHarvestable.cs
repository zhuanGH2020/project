using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 可重复采集物 - 可多次收获，会重新生长的物品（如浆果丛、果树等）
/// 具体参数通过配置表查询
/// </summary>
public class RepeatableHarvestable : HarvestableObject
{
    [Header("Repeatable Harvest Settings")]
    [SerializeField] private int _itemId = 0;                    // 物品ID，用于查表获取具体配置
    [SerializeField] private GameObject _harvestableVisual;      // 可采集状态的视觉表现

    private int _currentHarvestCount;        // 当前可采集数量
    private int _maxHarvestCount;            // 最大采集数量
    private float _regrowTime;               // 重新生长时间
    private float _regrowTimer;              // 重新生长计时器
    private bool _isRegrowing;               // 是否正在重新生长

    protected override void Awake()
    {
        base.Awake();
        
        // 设置为0生命值，表示不可破坏
        _maxHealth = 0;
        _currentHealth = 0;
        
        // 从配置表加载采集设置
        LoadHarvestConfig();
        UpdateVisual();
    }

    private void Update()
    {
        if (_isRegrowing)
        {
            _regrowTimer += Time.deltaTime;
            if (_regrowTimer >= _regrowTime)
            {
                Regrow();
            }
        }
    }

    /// <summary>
    /// 从配置表加载采集配置
    /// </summary>
    private void LoadHarvestConfig()
    {
        if (_itemId <= 0) return;

        var configReader = ConfigManager.Instance.GetReader("Source");
        if (configReader == null || !configReader.HasKey(_itemId))
        {
            Debug.LogWarning($"[RepeatableHarvestable] Config not found for item {_itemId}");
            return;
        }

        // 从配置表读取采集参数
        int dropItemId = configReader.GetValue<int>(_itemId, "DropItemId", _itemId);
        int dropCount = configReader.GetValue<int>(_itemId, "DropCount", 1);
        float dropRate = configReader.GetValue<float>(_itemId, "DropRate", 1.0f);
        
        _maxHarvestCount = configReader.GetValue<int>(_itemId, "MaxHarvestCount", 3);
        _regrowTime = configReader.GetValue<float>(_itemId, "RegrowTime", 300f);

        // 初始化采集数量
        _currentHarvestCount = _maxHarvestCount;

        // 配置掉落
        _drops.Clear();
        _drops.Add(new DropItem(dropItemId, dropCount, dropCount, dropRate));

        // 设置采集参数
        _harvestTime = configReader.GetValue<float>(_itemId, "HarvestTime", 0f);
        _destroyAfterHarvest = false; // 重复采集物不销毁
        _interactionRange = configReader.GetValue<float>(_itemId, "InteractionRange", 2f);
        
        // 设置动作类型
        _actionType = configReader.GetValue<ActionType>(_itemId, "ActionType", ActionType.Pick);

        Debug.Log($"[RepeatableHarvestable] Loaded config for item {_itemId}: max harvest {_maxHarvestCount}, regrow time {_regrowTime}s, action: {_actionType}");
    }

    protected override void OnHarvestComplete(IAttacker harvester)
    {
        // 减少可采集数量
        _currentHarvestCount--;
        UpdateVisual();

        Debug.Log($"[RepeatableHarvestable] Item {_itemId} harvested by {harvester?.GetType().Name}, remaining: {_currentHarvestCount}");

        // 如果采集完了，开始重新生长
        if (_currentHarvestCount <= 0)
        {
            StartRegrow();
        }
    }

    private void StartRegrow()
    {
        _isRegrowing = true;
        _regrowTimer = 0f;
        Debug.Log($"[RepeatableHarvestable] Starting regrow timer ({_regrowTime}s)");
    }

    private void Regrow()
    {
        _isRegrowing = false;
        _currentHarvestCount = _maxHarvestCount;
        UpdateVisual();
        Debug.Log($"[RepeatableHarvestable] Regrowth complete, harvest count restored to {_maxHarvestCount}");
    }

    private void UpdateVisual()
    {
        if (_harvestableVisual != null)
        {
            _harvestableVisual.SetActive(_currentHarvestCount > 0);
        }
    }

    // 重写交互条件：只有有可采集物时才能交互
    public override bool CanInteract => _currentHarvestCount > 0 && !_isBeingHarvested;
    public override bool CanHarvest => _currentHarvestCount > 0 && !_isBeingHarvested;

    public override float TakeDamage(DamageInfo damageInfo)
    {
        // 重复采集物不可被伤害
        return 0;
    }

    /// <summary>
    /// 设置物品ID并重新加载配置
    /// </summary>
    public void SetItemId(int itemId)
    {
        _itemId = itemId;
        LoadHarvestConfig();
        UpdateVisual();
    }

    /// <summary>
    /// 设置采集数量（用于存档加载）
    /// </summary>
    public void SetHarvestCount(int count)
    {
        _currentHarvestCount = Mathf.Clamp(count, 0, _maxHarvestCount);
        UpdateVisual();
    }

    // 公开属性用于存档
    public int ItemId => _itemId;
    public int CurrentHarvestCount => _currentHarvestCount;
    public int MaxHarvestCount => _maxHarvestCount;
    public bool IsRegrowing => _isRegrowing;
    public float RegrowProgress => _isRegrowing ? (_regrowTimer / _regrowTime) : 0f;
} 