using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 需要工具采集物 - 需要使用工具破坏后才能收获的物品（如树木、矿石等）
/// 具体参数通过配置表查询
/// </summary>
public class ToolRequiredHarvestable : HarvestableObject
{
    [Header("Tool Required Harvest Settings")]
    [SerializeField] private int _itemId = 0;                    // 物品ID，用于查表获取具体配置
    [SerializeField] private GameObject _normalState;            // 正常状态模型
    [SerializeField] private GameObject _damagedState;           // 受损状态模型
    [SerializeField] private GameObject _harvestedState;         // 可采集状态模型（如倒下的树）

    protected override void Awake()
    {
        base.Awake();
        
        // 从配置表加载采集设置
        LoadHarvestConfig();
        UpdateVisual();
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
            Debug.LogWarning($"[ToolRequiredHarvestable] Config not found for item {_itemId}");
            return;
        }

        // 从配置表读取基础属性
        _maxHealth = configReader.GetValue<float>(_itemId, "MaxHealth", 100f);
        _currentHealth = _maxHealth;

        // 从配置表读取掉落配置
        int dropItemId = configReader.GetValue<int>(_itemId, "DropItemId", _itemId);
        int minDropCount = configReader.GetValue<int>(_itemId, "MinDropCount", 1);
        int maxDropCount = configReader.GetValue<int>(_itemId, "MaxDropCount", 3);
        float dropRate = configReader.GetValue<float>(_itemId, "DropRate", 1.0f);

        // 配置掉落
        _drops.Clear();
        _drops.Add(new DropItem(dropItemId, minDropCount, maxDropCount, dropRate));

        // 设置采集参数
        _harvestTime = configReader.GetValue<float>(_itemId, "HarvestTime", 0f);
        _destroyAfterHarvest = configReader.GetValue<bool>(_itemId, "DestroyAfterHarvest", false);
        _interactionRange = configReader.GetValue<float>(_itemId, "InteractionRange", 3f);
        
        // 设置工具要求
        _requiresTool = configReader.GetValue<bool>(_itemId, "RequiresTool", true);
        _requiredToolType = configReader.GetValue<ToolType>(_itemId, "RequiredToolType", ToolType.Axe);

        // 设置动作类型
        _actionType = configReader.GetValue<ActionType>(_itemId, "ActionType", ActionType.Chop);

        Debug.Log($"[ToolRequiredHarvestable] Loaded config for item {_itemId}: HP {_maxHealth}, requires {_requiredToolType}, action: {_actionType}");
    }

    public override float TakeDamage(DamageInfo damageInfo)
    {
        // 只能被物理伤害影响
        if (damageInfo.Type != DamageType.Physical)
            return 0;

        float damage = base.TakeDamage(damageInfo);
        UpdateVisual();
        return damage;
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        Debug.Log($"[ToolRequiredHarvestable] Item {_itemId} destroyed and ready for harvest");
    }

    protected override void OnHarvestComplete(IAttacker harvester)
    {
        Debug.Log($"[ToolRequiredHarvestable] Item {_itemId} harvested by {harvester?.GetType().Name}");
    }

    /// <summary>
    /// 更新视觉表现
    /// </summary>
    private void UpdateVisual()
    {
        if (_currentHealth <= 0)
        {
            SetVisualState(false, true);  // 显示可采集状态
        }
        else
        {
            SetVisualState(true, false);  // 显示正常状态
        }
    }

    private void SetVisualState(bool normal, bool harvested)
    {
        if (_normalState) _normalState.SetActive(normal);
        if (_harvestedState) _harvestedState.SetActive(harvested);
    }

    // 重写交互条件：只有破坏后才能交互采集
    public override bool CanInteract => _currentHealth <= 0 && !_isHarvested && !_isBeingHarvested;
    public override bool CanHarvest => _currentHealth <= 0 && !_isHarvested && !_isBeingHarvested;

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
    /// 设置健康值（用于存档加载）
    /// </summary>
    public override void SetHealth(float health)
    {
        base.SetHealth(health);
        UpdateVisual();
    }

    // 公开属性
    public int ItemId => _itemId;
    public float HealthPercentage => _maxHealth > 0 ? (_currentHealth / _maxHealth) : 0f;
} 