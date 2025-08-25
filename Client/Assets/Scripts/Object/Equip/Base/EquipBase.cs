using UnityEngine;

/// <summary>
/// 装备基类
/// </summary>
public abstract class EquipBase : MonoBehaviour, IEquipable
{
    [Header("基础设置")]
    [SerializeField] protected EquipPart _equipPart;    // 装备部位
    [SerializeField] protected int _configId;           // 配置ID

    [Header("动画设置")]
    [SerializeField] protected string _animatorPath;    // 动画状态机路径

    [Header("战斗设置")]
    [SerializeField] protected float _damage = 0f;      // 攻击力
    [SerializeField] protected float _defense = 0f;     // 防御力
    [SerializeField] protected float _range = 0f;       // 攻击范围

    [Header("耐久设置")]
    [SerializeField] protected float _maxDurability = 100f;
    [SerializeField] protected float _durabilityLoss = 1f;  // 每次使用的耐久损耗
    [SerializeField] protected float _useCooldown = 0.5f;

    protected float _currentDurability;
    protected CooldownTimer _useTimer;
    protected IAttacker _owner;

    public EquipPart EquipPart => _equipPart;
    public string AnimatorPath => _animatorPath;        // 新增：获取动画路径
    public float MaxDurability => _maxDurability;
    public float CurrentDurability => _currentDurability;
    public bool IsEquipped => _owner != null;
    public bool CanUse => _useTimer.IsReady && CurrentDurability > 0;
    public float Damage => _damage;
    public float Defense => _defense;
    public float Range => _range;
    public float UseCooldown => _useCooldown;

    protected virtual void Awake()
    {
        // 基础初始化，具体数值在Init()中设置
    }

    protected virtual void Update()
    {
        if (_useTimer != null)
    {
        _useTimer.Update();
        }
    }

    /// <summary>
    /// 根据配置ID初始化装备
    /// </summary>
    public virtual void Init(int configId)
    {
        _configId = configId;
        
        // 从配置表读取装备属性
        var equipReader = ConfigManager.Instance.GetReader("Equip");
        if (equipReader == null || !equipReader.HasKey(configId))
        {
            Debug.LogError($"[EquipBase] Equipment config not found: {configId}");
            return;
        }

        // 初始化基础属性
        _equipPart = equipReader.GetValue<EquipPart>(_configId, "Type", EquipPart.None);

        // 读取动画路径
        _animatorPath = equipReader.GetValue<string>(_configId, "AnimatorPath", "");

        _damage = equipReader.GetValue<float>(_configId, "Damage", 0f);
        _defense = equipReader.GetValue<float>(_configId, "Defense", 0f);
        _maxDurability = equipReader.GetValue<float>(_configId, "Durability", 100f);
        _durabilityLoss = equipReader.GetValue<float>(_configId, "DurabilityLoss", 1f);
        _useCooldown = equipReader.GetValue<float>(_configId, "UseCooldown", 0.5f);
        _range = equipReader.GetValue<float>(_configId, "Range", 0f);

        // 用配置表的值初始化运行时数据
        _currentDurability = _maxDurability;
        _useTimer = new CooldownTimer(_useCooldown);

        Debug.Log($"[EquipBase] Initialized equipment {configId}: Part={_equipPart}, Damage={_damage}, Defense={_defense}, Durability={_maxDurability}, AnimatorPath={_animatorPath}");
    }

    public virtual void OnEquip(IAttacker owner)
    {
        _owner = owner;
        ApplyEquipEffect();
    }

    public virtual void OnUnequip()
    {
        RemoveEquipEffect();
        _owner = null;
    }

    /// <summary>
    /// 应用装备效果
    /// </summary>
    protected virtual void ApplyEquipEffect()
    {
        // 由子类实现具体效果
    }

    /// <summary>
    /// 移除装备效果
    /// </summary>
    protected virtual void RemoveEquipEffect()
    {
        // 由子类实现具体效果
    }

    public virtual void Use()
    {
        if (!CanUse) return;

        // 消耗耐久度
        _currentDurability -= _durabilityLoss;
        
        // 开始冷却
        _useTimer.StartCooldown();

        // 检查是否耐久度耗尽
        if (_currentDurability <= 0)
        {
            OnDurabilityDepleted();
        }
    }

    public virtual float GetAttackBonus()
    {
        return _damage;
    }

    public virtual float GetDefenseBonus()
    {
        return _defense;
    }

    protected virtual void OnDurabilityDepleted()
    {
        // 通知拥有者卸下装备
        if (_owner is CombatEntity combatEntity && IsEquipped)
        {
            OnUnequip();
            // 从装备列表中移除（需要CombatEntity提供RemoveEquip方法）
        }
        
        // 由子类实现具体逻辑（如特效、音效等）
    }
    
} 