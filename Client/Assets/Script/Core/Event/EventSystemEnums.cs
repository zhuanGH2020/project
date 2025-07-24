using UnityEngine;

/// <summary>
/// 事件系统核心枚举定义
/// 遵循项目命名约定：PascalCase枚举名
/// 基于项目游戏设计需求定义
/// </summary>
public class EventSystemEnums
{
    /// <summary>
    /// 物品类型枚举
    /// 基于项目游戏设计需求定义
    /// </summary>
    public enum ItemType
    {
        Food = 0,       // 食物类
        Tool = 1,       // 工具类
        Light = 2,      // 光源类
        Building = 3,   // 建筑类
        Clothing = 4,   // 衣物类
        Weapon = 5,     // 武器类
        Summon = 6,     // 召唤类
        Material = 7,   // 材料类
        Equipment = 8,  // 装备类
        Consumable = 9  // 消耗品类
    }
} 