using UnityEngine;

/// <summary>
/// 可承伤物体基类
/// </summary>
public abstract class DamageableObject : ObjectBase, IDamageable
{
    [Header("基础设置")]
    [SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected float _defense = 0f;
    
    protected float _currentHealth;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public virtual float Defense => _defense;

    protected override void Awake()
    {
        base.Awake();
        _currentHealth = _maxHealth;
    }

    /// <summary>
    /// 承受伤害
    /// </summary>
    public virtual float TakeDamage(DamageInfo damageInfo)
    {
        // 计算实际伤害值
        float actualDamage = Mathf.Max(0, damageInfo.Damage - Defense);
        float oldHealth = _currentHealth;
        
        // 扣除生命值
        _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);

        // 打印日志
        Debug.Log($"[{gameObject.name}] HP: {oldHealth:F1} -> {_currentHealth:F1} (Damage: {actualDamage:F1}, Defense: {Defense:F1})");
        
        // 检查是否死亡
        if (_currentHealth <= 0)
        {
            OnDeath();
        }

        return actualDamage;
    }

    /// <summary>
    /// 死亡时调用
    /// </summary>
    protected virtual void OnDeath()
    {
        // 由子类实现具体死亡逻辑
        Debug.Log($"[{gameObject.name}] Dead!");
    }
    
    /// <summary>
    /// 设置当前血量 - 用于加载存档
    /// </summary>
    /// <param name="health">目标血量值</param>
    public virtual void SetHealth(float health)
    {
        float oldHealth = _currentHealth;
        _currentHealth = Mathf.Clamp(health, 0, _maxHealth);
        Debug.Log($"[{gameObject.name}] Health set from {oldHealth:F1} to {_currentHealth:F1}");
    }
} 