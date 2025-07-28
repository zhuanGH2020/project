using UnityEngine;

/// <summary>
/// 可砍伐的树
/// </summary>
public class Tree : DamageableObject
{
    [Header("Tree Settings")]
    [SerializeField] private GameObject _normalState;    // 正常状态模型
    [SerializeField] private GameObject _damagedState;   // 受损状态模型
    [SerializeField] private GameObject _deadState;      // 倒下状态模型
    [SerializeField] private float _damageStateThreshold = 0.5f;  // 受损状态阈值

    protected override void Awake()
    {
        base.Awake();
        UpdateVisual();
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

    /// <summary>
    /// 更新视觉表现
    /// </summary>
    private void UpdateVisual()
    {
        if (_currentHealth <= 0)
        {
            SetVisualState(false, false, true);
        }
        else if (_currentHealth <= _maxHealth * _damageStateThreshold)
        {
            SetVisualState(false, true, false);
        }
        else
        {
            SetVisualState(true, false, false);
        }
    }

    private void SetVisualState(bool normal, bool damaged, bool dead)
    {
        if (_normalState) _normalState.SetActive(normal);
        if (_damagedState) _damagedState.SetActive(damaged);
        if (_deadState) _deadState.SetActive(dead);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        // 可以在这里添加掉落物品等逻辑
    }
} 