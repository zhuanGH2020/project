using UnityEngine;

/// <summary>
/// 可采集物体 - 支持直接点击采集和工具采集
/// 使用Drop.csv配置系统进行掉落管理
/// </summary>
public class HarvestableObject : Building
{
    [SerializeField] private ParticleSystem _harvestEffect;
    
    private int _requiredWeaponId;
    private bool _anyWeapon;
    private bool _isHarvested;
    private bool _isBeingHarvested;

    public override bool CanInteract => !_isHarvested && !_isBeingHarvested && base.CanInteract;
    public override float GetInteractionRange() => 2f;

    public bool IsHarvested => _isHarvested;



    public void Init(int itemId)
    {
        // 使用Building的Initialize方法设置itemId
        Initialize(itemId, Vector2.zero);
        LoadDropConfig();
        SetHealth(1f); // 可采集物体固定血量为1
    }

    /// <summary>
    /// 重写建筑物配置加载 - 可采集物品不需要从Item表查询MaxHealth
    /// </summary>
    protected override void LoadBuildingConfig()
    {
        // 可采集物品不从Item表加载MaxHealth，固定使用默认值
        // 血量会在Init方法中设置为1
        _maxHealthValue = 100f; // 设置默认值，但实际不会用到
    }

    private void LoadDropConfig()
    {
        var dropConfig = ConfigManager.Instance.GetReader("Drop");
        if (dropConfig == null || !dropConfig.HasKey(ItemId))
        {
            Debug.LogError($"Drop config not found for item: {ItemId}");
            return;
        }

        // 读取基础配置
        _requiredWeaponId = dropConfig.GetValue<int>(ItemId, "RequiredWeaponId", 0);
        _anyWeapon = dropConfig.GetValue<bool>(ItemId, "AnyWeapon", false);

        // 配置掉落物品
        ClearDrops();
        for (int i = 1; i <= 5; i++)
        {
            int dropItemId = dropConfig.GetValue<int>(ItemId, $"DropItemId{i}", 0);
            if (dropItemId > 0)
            {
                int dropCount = dropConfig.GetValue<int>(ItemId, $"DropCount{i}", 1);
                float dropChance = dropConfig.GetValue<float>(ItemId, $"DropChance{i}", 1.0f);
                AddDrop(dropItemId, dropCount, dropCount, dropChance);
            }
        }
        EnableDrop();
    }

    /// <summary>
    /// 重写Building的交互逻辑 - 实现采集功能
    /// </summary>
    public override void OnInteract(Vector3 clickPosition)
    {
        if (!CanInteract) return;

        if (CanHarvest())
        {
            PerformHarvest();
        }
        else
        {
            ShowHarvestHint();
        }
    }

    private bool CanHarvest()
    {
        var player = Player.Instance;
        if (player == null) return false;

        // 需要特定工具
        if (_requiredWeaponId > 0)
        {
            return HasRequiredWeapon(player, _requiredWeaponId);
        }
        
        // 需要任意武器
        if (_anyWeapon)
        {
            return HasAnyWeapon(player);
        }
        
        // 直接采集
        return true;
    }

    private void PerformHarvest()
    {
        _isBeingHarvested = true;
        _isHarvested = true;

        PlayHarvestEffect();
        
        // 直接将ItemId对应的物品添加到背包
        var success = PackageModel.Instance.AddItem(ItemId, 1);
        if (success)
        {
            // 发送通知事件
            EventManager.Instance.Publish(new NoticeEvent($"获得了 {GetBuildingName()}"));
        }
        else
        {
            // 背包已满，显示提示
            EventManager.Instance.Publish(new NoticeEvent("背包已满！"));
        }

        Destroy(gameObject, GetDestroyDelay());
    }

    private void ShowHarvestHint()
    {
        // 移除Debug.Log，符合项目日志规范
        if (_requiredWeaponId > 0)
        {
            var weaponName = GetWeaponName(_requiredWeaponId);
            // TODO: 通过UI系统显示提示：需要使用{weaponName}才能采集
        }
        else if (_anyWeapon)
        {
            // TODO: 通过UI系统显示提示：需要装备武器才能攻击
        }
    }

    private bool HasRequiredWeapon(Player player, int weaponId)
    {
        // TODO: 检查玩家是否装备了指定武器
        // 示例实现：return player.GetEquippedWeaponId() == weaponId;
        return true; // 临时返回true用于测试，待装备系统完成后实现
    }

    private bool HasAnyWeapon(Player player)
    {
        // TODO: 检查玩家是否装备了任意武器
        // 示例实现：return player.HasEquippedWeapon();
        return true; // 临时返回true用于测试，待装备系统完成后实现
    }

    private string GetWeaponName(int weaponId)
    {
        var toolConfig = ConfigManager.Instance.GetReader("Tool");
        if (toolConfig?.HasKey(weaponId) == true)
        {
            return toolConfig.GetValue<string>(weaponId, "Name", "工具");
        }
        return "未知工具";
    }

    private void PlayHarvestEffect()
    {
        if (_harvestEffect != null)
        {
            _harvestEffect.Play();
        }
    }

    private float GetDestroyDelay()
    {
        return _harvestEffect != null ? _harvestEffect.main.duration : 0f;
    }

    // 工具采集支持：被工具攻击时的处理
    public override float TakeDamage(DamageInfo damageInfo)
    {
        // 只有需要工具的物体才能被工具伤害
        if (_requiredWeaponId == 0 && !_anyWeapon) return 0;
        
        // 检查工具是否符合要求
        if (!IsValidWeaponDamage(damageInfo)) return 0;

        float damage = base.TakeDamage(damageInfo);
        
        // 生命值归零时自动采集
        if (CurrentHealth <= 0 && !_isHarvested)
        {
            PerformHarvest();
        }
        
        return damage;
    }

    private bool IsValidWeaponDamage(DamageInfo damageInfo)
    {
        // TODO: 检查伤害来源是否是合适的工具
        return damageInfo.Type == DamageType.Physical;
    }

    /// <summary>
    /// 设置掉落数量（用于掉落物品）
    /// </summary>
    public void SetDropCount(int count)
    {
        if (ItemId > 0)
        {
            ClearDrops();
            AddDrop(ItemId, count, count, 1.0f);
        }
    }
} 