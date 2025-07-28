using UnityEngine;

/// <summary>
/// 攻击者接口
/// </summary>
public interface IAttacker
{
    /// <summary>
    /// 基础攻击力
    /// </summary>
    float BaseAttack { get; }

    /// <summary>
    /// 是否可以攻击(CD检查)
    /// </summary>
    bool CanAttack { get; }

    /// <summary>
    /// 执行攻击
    /// </summary>
    /// <param name="target">攻击目标</param>
    void PerformAttack(IDamageable target);
} 