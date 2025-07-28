using UnityEngine;

/// <summary>
/// 装备基类
/// </summary>
public abstract class EquipBase : MonoBehaviour, IEquipable
{
    [Header("Basic Settings")]
    [SerializeField] protected EquipPart _equipPart;    // 装备部位
    [SerializeField] protected int _configId;           // 配置ID

    [Header("Durability Settings")]
    [SerializeField] protected float _maxDurability = 100f;
    [SerializeField] protected float _useCooldown = 0.5f;

    protected float _currentDurability;
    protected CooldownTimer _useTimer;
    protected IAttacker _owner;

    public EquipPart EquipPart => _equipPart;
    public float MaxDurability => _maxDurability;
    public float CurrentDurability => _currentDurability;
    public bool IsEquipped => _owner != null;
    public bool CanUse => _useTimer.IsReady && CurrentDurability > 0;

    protected virtual void Awake()
    {
        _currentDurability = _maxDurability;
        _useTimer = new CooldownTimer(_useCooldown);
    }

    protected virtual void Update()
    {
        _useTimer.Update();
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
        _currentDurability--;
        
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
        return 0f;
    }

    public virtual float GetDefenseBonus()
    {
        return 0f;
    }

    protected virtual void OnDurabilityDepleted()
    {
        // 由子类实现具体逻辑
    }
} 