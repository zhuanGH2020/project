using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 可承伤物体基类 - 提供基础生命值管理、点击接口和掉落系统
/// 具体交互逻辑完全由子类实现
/// </summary>
public abstract class DamageableObject : ObjectBase, IDamageable, IClickable, IDroppable
{
    protected float _currentHealth;
    
    // 掉落系统相关
    protected List<DropItem> _drops = new List<DropItem>();
    protected bool _enableDrop = false;  // 默认关闭掉落功能
    protected IAttacker _lastAttacker;   // 记录最后攻击者用于掉落

    // IDamageable 基础实现
    public abstract float MaxHealth { get; }
    public float CurrentHealth => _currentHealth;
    public abstract float Defense { get; }

    // IClickable 基础实现
    public abstract bool CanInteract { get; }
    public abstract float GetInteractionRange();
    
    // IDroppable 基础实现
    public virtual List<DropItem> GetDropItems() => _drops;
    public virtual bool IsDropEnabled => _enableDrop;

    protected override void Awake()
    {
        base.Awake();
        _currentHealth = MaxHealth;
    }

    public virtual float TakeDamage(DamageInfo damageInfo)
    {
        float actualDamage = Mathf.Max(0, damageInfo.Damage - Defense);
        _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);
        
        // 记录攻击者用于掉落处理
        _lastAttacker = damageInfo.Source;

        if (_currentHealth <= 0)
        {
            OnDeath();
        }

        return actualDamage;
    }

    protected virtual void OnDeath()
    {
        // 处理死亡掉落
        if (IsDropEnabled && _lastAttacker != null)
        {
            ProcessDrops(_lastAttacker);
        }
        
        // 子类重写实现其他死亡逻辑
        OnDeathCustom();
    }
    
    /// <summary>
    /// 子类可重写的死亡逻辑，在掉落处理之后执行
    /// </summary>
    protected virtual void OnDeathCustom()
    {
        // 子类重写实现死亡逻辑
    }

    public virtual void ProcessDrops(IAttacker killer)
    {
        if (!IsDropEnabled) return;
        
        foreach (var drop in _drops)
        {
            int actualCount = drop.GetActualDropCount();
            if (actualCount > 0)
            {
                // 直接在地图创建掉落物
                CreateDroppedItem(drop.itemId, actualCount);
            }
        }
    }

    public virtual void CreateDroppedItem(int itemId, int count)
    {
        // 从Item.csv获取物品配置
        var itemConfig = ConfigManager.Instance.GetReader("Item");
        if (itemConfig == null || !itemConfig.HasKey(itemId))
        {
            Debug.LogError($"[DamageableObject] Item config not found for ID: {itemId}");
            return;
        }

        string prefabPath = itemConfig.GetValue<string>(itemId, "PrefabPath", "");
        Vector3 dropPosition = transform.position + Vector3.up * 0.5f + GetRandomOffset();
        GameObject droppedItemGO;

        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError($"[DamageableObject] No prefab path configured for item {itemId}");
            return;
        }

        // 使用配置的预制体
        GameObject prefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[DamageableObject] Prefab not found at path: {prefabPath} for item {itemId}");
            return;
        }

        droppedItemGO = Object.Instantiate(prefab, dropPosition, Quaternion.identity);

        // 确保有HarvestableObject组件
        var harvestable = droppedItemGO.GetComponent<HarvestableObject>();
        if (harvestable == null)
        {
            harvestable = droppedItemGO.AddComponent<HarvestableObject>();
        }

        // 初始化掉落物
        harvestable.Init(itemId);
        harvestable.SetDropCount(count);
        
        // 确保有碰撞器
        if (droppedItemGO.GetComponent<Collider>() == null)
        {
            droppedItemGO.AddComponent<SphereCollider>();
        }
    }

    /// <summary>
    /// 获取随机偏移，避免掉落物重叠
    /// </summary>
    private Vector3 GetRandomOffset()
    {
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float distance = Random.Range(0.5f, 1.5f);
        return new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance);
    }

    public virtual void SetHealth(float health)
    {
        _currentHealth = Mathf.Clamp(health, 0, MaxHealth);
    }

    public virtual void OnClick(Vector3 clickPosition)
    {
        
    }
    
    /// <summary>
    /// 启用掉落功能
    /// </summary>
    public virtual void EnableDrop()
    {
        _enableDrop = true;
    }
    
    /// <summary>
    /// 禁用掉落功能
    /// </summary>
    public virtual void DisableDrop()
    {
        _enableDrop = false;
    }
    
    /// <summary>
    /// 添加掉落物品
    /// </summary>
    public virtual void AddDrop(DropItem dropItem)
    {
        _drops.Add(dropItem);
    }
    
    /// <summary>
    /// 添加掉落物品（简化方法）
    /// </summary>
    public virtual void AddDrop(int itemId, int minCount, int maxCount, float dropRate = 1.0f)
    {
        _drops.Add(new DropItem(itemId, minCount, maxCount, dropRate));
    }
    
    /// <summary>
    /// 清除所有掉落物品
    /// </summary>
    public virtual void ClearDrops()
    {
        _drops.Clear();
    }
} 