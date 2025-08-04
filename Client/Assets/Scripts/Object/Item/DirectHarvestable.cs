using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 可直接采集物 - 点击即可收获的物品（如草、花等）
/// 具体参数通过配置表查询
/// </summary>
public class DirectHarvestable : HarvestableObject
{
    [Header("Direct Harvest Settings")]
    [SerializeField] private int _itemId = 0;     // 物品ID，用于查表获取具体配置

    protected override void Awake()
    {
        base.Awake();
        
        // 设置为0生命值，表示不可破坏
        _maxHealth = 0;
        _currentHealth = 0;
        
        // 从配置表加载采集设置
        LoadHarvestConfig();
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
            Debug.LogWarning($"[DirectHarvestable] Config not found for item {_itemId}");
            return;
        }

        // 从配置表读取掉落物品
        int dropItemId = configReader.GetValue<int>(_itemId, "DropItemId", _itemId);
        int dropCount = configReader.GetValue<int>(_itemId, "DropCount", 1);
        float dropRate = configReader.GetValue<float>(_itemId, "DropRate", 1.0f);

        // 配置掉落
        _drops.Clear();
        _drops.Add(new DropItem(dropItemId, dropCount, dropCount, dropRate));

        // 设置采集参数
        _harvestTime = configReader.GetValue<float>(_itemId, "HarvestTime", 0f);
        _destroyAfterHarvest = configReader.GetValue<bool>(_itemId, "DestroyAfterHarvest", true);
        _interactionRange = configReader.GetValue<float>(_itemId, "InteractionRange", 2f);
        
        // 设置动作类型
        _actionType = configReader.GetValue<ActionType>(_itemId, "ActionType", ActionType.Pull);

        Debug.Log($"[DirectHarvestable] Loaded config for item {_itemId}: drops {dropItemId}x{dropCount}, action: {_actionType}");
    }

    protected override void OnHarvestComplete(IAttacker harvester)
    {
        Debug.Log($"[DirectHarvestable] Item {_itemId} harvested by {harvester?.GetType().Name}");
    }

    public override float TakeDamage(DamageInfo damageInfo)
    {
        // 直接采集物不可被伤害
        return 0;
    }

    /// <summary>
    /// 设置物品ID并重新加载配置
    /// </summary>
    public void SetItemId(int itemId)
    {
        _itemId = itemId;
        LoadHarvestConfig();
    }

    /// <summary>
    /// 获取当前物品ID
    /// </summary>
    public int ItemId => _itemId;

    /// <summary>
    /// 设置掉落数量（用于掉落物）
    /// </summary>
    public void SetDropCount(int count)
    {
        // 清空原有掉落列表
        _drops.Clear();
        // 添加指定数量的掉落物
        _drops.Add(new DropItem(_itemId, count));
    }
} 