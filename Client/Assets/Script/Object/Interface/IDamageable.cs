using UnityEngine;

/// <summary>
/// 可承伤接口
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 最大生命值
    /// </summary>
    float MaxHealth { get; }

    /// <summary>
    /// 当前生命值
    /// </summary>
    float CurrentHealth { get; }

    /// <summary>
    /// 防御值
    /// </summary>
    float Defense { get; }

    /// <summary>
    /// 承受伤害
    /// </summary>
    /// <param name="damageInfo">伤害信息</param>
    /// <returns>实际伤害值</returns>
    float TakeDamage(DamageInfo damageInfo);
} 