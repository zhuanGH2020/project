using UnityEngine;

/// <summary>
/// 可装备接口
/// </summary>
public interface IEquipable
{
    /// <summary>
    /// 最大耐久度
    /// </summary>
    float MaxDurability { get; }

    /// <summary>
    /// 当前耐久度
    /// </summary>
    float CurrentDurability { get; }

    /// <summary>
    /// 是否已装备
    /// </summary>
    bool IsEquipped { get; }

    /// <summary>
    /// 是否可以使用(CD检查)
    /// </summary>
    bool CanUse { get; }

    /// <summary>
    /// 装备时调用
    /// </summary>
    /// <param name="owner">装备者</param>
    void OnEquip(IAttacker owner);

    /// <summary>
    /// 卸下时调用
    /// </summary>
    void OnUnequip();

    /// <summary>
    /// 使用装备
    /// </summary>
    void Use();

    /// <summary>
    /// 获取攻击加成
    /// </summary>
    float GetAttackBonus();

    /// <summary>
    /// 获取防御加成
    /// </summary>
    float GetDefenseBonus();
} 