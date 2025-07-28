using UnityEngine;

/// <summary>
/// 伤害信息
/// </summary>
public struct DamageInfo
{
    /// <summary>
    /// 伤害值
    /// </summary>
    public float Damage;

    /// <summary>
    /// 伤害类型
    /// </summary>
    public DamageType Type;

    /// <summary>
    /// 击中点
    /// </summary>
    public Vector3 HitPoint;

    /// <summary>
    /// 伤害来源
    /// </summary>
    public IAttacker Source;

    /// <summary>
    /// 伤害方向
    /// </summary>
    public Vector3 Direction;
} 