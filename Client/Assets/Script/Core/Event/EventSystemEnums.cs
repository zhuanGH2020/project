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

    /// <summary>
    /// 装备槽位类型枚举
    /// 支持项目装备系统设计
    /// </summary>
    public enum EquipmentSlotType
    {
        None = 0,       // 无装备槽
        Helmet = 1,     // 头盔槽
        Armor = 2,      // 护甲槽
        Weapon = 3,     // 武器槽
        Accessory = 4,  // 饰品槽
        Tool = 5        // 工具槽
    }

    /// <summary>
    /// 事件优先级枚举
    /// 用于事件队列处理的优先级管理
    /// </summary>
    public enum EventPriority
    {
        Low = 0,        // 低优先级
        Normal = 1,     // 普通优先级
        High = 2,       // 高优先级
        Critical = 3    // 关键优先级
    }

    /// <summary>
    /// 事件处理状态枚举
    /// 用于跟踪事件处理结果
    /// </summary>
    public enum EventProcessingStatus
    {
        Pending = 0,    // 等待处理
        Processing = 1, // 正在处理
        Completed = 2,  // 处理完成
        Failed = 3,     // 处理失败
        Cancelled = 4   // 已取消
    }

    /// <summary>
    /// 背包操作类型枚举
    /// 用于区分不同的背包操作
    /// </summary>
    public enum InventoryOperationType
    {
        Add = 0,        // 添加物品
        Remove = 1,     // 移除物品
        Move = 2,       // 移动物品
        Stack = 3,      // 堆叠物品
        Split = 4,      // 分割物品
        Sort = 5        // 整理背包
    }

    /// <summary>
    /// 制作系统状态枚举
    /// 用于跟踪制作过程状态
    /// </summary>
    public enum CraftingStatus
    {
        Idle = 0,           // 空闲状态
        Preparing = 1,      // 准备中
        Crafting = 2,       // 制作中
        Completed = 3,      // 制作完成
        Failed = 4,         // 制作失败
        Cancelled = 5       // 制作取消
    }

    /// <summary>
    /// 物品品质枚举
    /// 用于区分物品品质等级
    /// </summary>
    public enum ItemQuality
    {
        Common = 0,     // 普通品质
        Uncommon = 1,   // 不常见品质
        Rare = 2,       // 稀有品质
        Epic = 3,       // 史诗品质
        Legendary = 4   // 传说品质
    }

    /// <summary>
    /// 事件日志级别枚举
    /// 用于控制事件日志输出级别
    /// </summary>
    public enum EventLogLevel
    {
        None = 0,       // 不输出日志
        Error = 1,      // 仅错误日志
        Warning = 2,    // 警告及以上
        Info = 3,       // 信息及以上
        Debug = 4,      // 调试及以上
        Verbose = 5     // 详细日志
    }
} 