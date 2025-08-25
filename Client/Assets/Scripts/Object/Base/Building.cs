using UnityEngine;
using System;

/// <summary>
/// 建筑物组件 - 管理建筑物的数据和行为
/// 挂载到建筑物预制体上，用于管理唯一标识、等级、交互状态等
/// </summary>
public class Building : DamageableObject
{
    private int _itemId = -1;               // 对应的道具ID
    private Vector2 _mapPosition;           // 地图位置
    private int _level = 1;                 // 建筑物等级
    private bool _canBuildingInteract = true; // 建筑物特有的交互状态
    private float _constructTime;           // 建造时间
    private bool _isConstructed = true;     // 是否建造完成
    
    protected float _maxHealthValue = 100f;   // 最大生命值

    // 实现DamageableObject的抽象属性
    public override float MaxHealth => _maxHealthValue;
    public override float Defense => 0f;
    public override bool CanInteract => _canBuildingInteract && CurrentHealth > 0;
    public override float GetInteractionRange() => 2f;

    // 公共属性
    public int ItemId => _itemId;
    public Vector2 MapPosition => _mapPosition;
    public int Level => _level;
    public bool CanBuildingInteract => _canBuildingInteract; // 重命名避免与抽象属性冲突
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
    
    protected virtual void LoadBuildingConfig()
    {
        var reader = ConfigManager.Instance.GetReader("Item");
        if (reader != null && reader.HasKey(_itemId))
        {
            _maxHealthValue = reader.GetValue<float>(_itemId, "MaxHealth", 100f);
        }
        else
        {
            // 如果配置不存在或没有该ID，使用默认值
            _maxHealthValue = 100f;
        }
    }
    
    private void UpdateGameObjectName()
    {
        gameObject.name = $"{GetPrefabName()}_{Uid}";
    }
    
    public string GetBuildingName()
    {
        return ResourceUtils.GetItemName(_itemId);
    }
    
    public string GetPrefabName()
    {
        string currentName = gameObject.name;
        if (currentName.EndsWith("(Clone)"))
        {
            currentName = currentName.Replace("(Clone)", "");
        }
        return currentName;
    }
    
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
    
    public void SetInteractable(bool canInteract)
    {
        if (_canBuildingInteract != canInteract)
        {
            _canBuildingInteract = canInteract;
            OnInteractStateChanged?.Invoke(this, canInteract);
        }
    }
    
    public int GetMaxLevel()
    {
        var reader = ConfigManager.Instance.GetReader("Item");
        return reader?.GetValue<int>(_itemId, "MaxLevel", 5) ?? 5;
    }
    
    protected override void OnDeath()
    {
        base.OnDeath();
        OnBuildingDestroyed();
    }
    
    private void OnBuildingDestroyed()
    {
        Demolish();
    }
    
    public virtual void Demolish()
    {
        OnDemolished?.Invoke(this);
        MapModel.Instance.RemoveBuildingByUid(Uid);
        Destroy(gameObject);
    }

    public override void OnClick(Vector3 clickPosition)
    {
        if (!CanInteract) return;

        // 触发交互事件，让InteractionManager处理寻路
        EventManager.Instance.Publish(new ObjectInteractionEvent(this, clickPosition));
    }

    /// <summary>
    /// 执行交互逻辑 - 由InteractionManager在玩家到达后调用，或直接调用
    /// 子类可重写实现自定义交互行为
    /// </summary>
    public virtual void OnInteract(Vector3 clickPosition)
    {
        // 子类重写实现具体交互逻辑
    }

    /// <summary>
    /// 玩家进入交互范围时调用 - 子类可重写实现自定义逻辑
    /// </summary>
    public virtual void OnEnterInteractionRange()
    {
        // 子类重写实现进入范围的逻辑
    }

    /// <summary>
    /// 玩家离开交互范围时调用 - 子类可重写实现自定义逻辑
    /// </summary>
    public virtual void OnLeaveInteractionRange()
    {
        // 子类重写实现离开范围的逻辑
    }
} 