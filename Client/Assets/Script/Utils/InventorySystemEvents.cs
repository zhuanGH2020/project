using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static EventSystemEnums;

/// <summary>
/// 背包系统事件定义类
/// 参考实现：Assets/Script/input/InputController.cs 的UnityEvent模式
/// 遵循项目命名约定和代码风格规范
/// </summary>
public static class InventorySystemEvents
{
    /// <summary>
    /// 物品获得事件 - 当玩家获得物品时触发
    /// 参数：ItemData 物品数据，int 获得数量
    /// </summary>
    [System.Serializable]
    public class ItemObtainedEvent : UnityEvent<ItemData, int> 
    {
        
    }

    /// <summary>
    /// 物品消耗事件 - 当玩家消耗物品时触发
    /// 参数：ItemData 物品数据，int 消耗数量
    /// </summary>
    [System.Serializable]
    public class ItemConsumedEvent : UnityEvent<ItemData, int> 
    {
        
    }

    /// <summary>
    /// 物品丢弃事件 - 当玩家丢弃物品时触发
    /// 参数：ItemData 物品数据，int 丢弃数量，Vector3 丢弃位置
    /// </summary>
    [System.Serializable]
    public class ItemDroppedEvent : UnityEvent<ItemData, int, Vector3> 
    {
        
    }

    /// <summary>
    /// 背包容量变化事件 - 当背包容量发生变化时触发
    /// 参数：int 旧容量，int 新容量
    /// </summary>
    [System.Serializable]
    public class InventoryCapacityChangedEvent : UnityEvent<int, int> 
    {
        
    }

    /// <summary>
    /// 背包已满事件 - 当背包空间不足时触发
    /// 参数：ItemData 尝试添加的物品数据
    /// </summary>
    [System.Serializable]
    public class InventoryFullEvent : UnityEvent<ItemData> 
    {
        
    }

    /// <summary>
    /// 物品堆叠变化事件 - 当物品数量发生变化时触发
    /// 参数：ItemData 物品数据，int 旧数量，int 新数量
    /// </summary>
    [System.Serializable]
    public class ItemStackChangedEvent : UnityEvent<ItemData, int, int> 
    {
        
    }

    /// <summary>
    /// 背包重量变化事件 - 当背包重量发生变化时触发
    /// 参数：float 旧重量，float 新重量，float 最大重量
    /// </summary>
    [System.Serializable]
    public class InventoryWeightChangedEvent : UnityEvent<float, float, float> 
    {
        
    }

    /// <summary>
    /// 背包超重事件 - 当背包重量超过限制时触发
    /// 参数：float 当前重量，float 最大重量
    /// </summary>
    [System.Serializable]
    public class InventoryOverweightEvent : UnityEvent<float, float> 
    {
        
    }

    /// <summary>
    /// 物品移动事件 - 当物品在背包内移动时触发
    /// 参数：ItemData 物品数据，int 源槽位，int 目标槽位
    /// </summary>
    [System.Serializable]
    public class ItemMovedEvent : UnityEvent<ItemData, int, int> 
    {
        
    }

    /// <summary>
    /// 物品分割事件 - 当物品堆叠被分割时触发
    /// 参数：ItemData 物品数据，int 原数量，int 分割数量
    /// </summary>
    [System.Serializable]
    public class ItemSplitEvent : UnityEvent<ItemData, int, int> 
    {
        
    }

    /// <summary>
    /// 背包排序事件 - 当背包进行排序时触发
    /// 参数：InventoryOperationType 操作类型
    /// </summary>
    [System.Serializable]
    public class InventorySortedEvent : UnityEvent<InventoryOperationType> 
    {
        
    }

    /// <summary>
    /// 背包清空事件 - 当背包被清空时触发
    /// 参数：List&lt;ItemData&gt; 被清空的物品列表
    /// </summary>
    [System.Serializable]
    public class InventoryClearedEvent : UnityEvent<List<ItemData>> 
    {
        
    }

    /// <summary>
    /// 背包槽位锁定事件 - 当背包槽位被锁定时触发
    /// 参数：int 槽位索引，bool 是否锁定
    /// </summary>
    [System.Serializable]
    public class InventorySlotLockedEvent : UnityEvent<int, bool> 
    {
        
    }

    /// <summary>
    /// 背包操作失败事件 - 当背包操作失败时触发
    /// 参数：InventoryOperationType 操作类型，string 失败原因
    /// </summary>
    [System.Serializable]
    public class InventoryOperationFailedEvent : UnityEvent<InventoryOperationType, string> 
    {
        
    }

    /// <summary>
    /// 背包状态保存事件 - 当背包状态被保存时触发
    /// 参数：string 保存时间戳
    /// </summary>
    [System.Serializable]
    public class InventoryStateSavedEvent : UnityEvent<string> 
    {
        
    }

    /// <summary>
    /// 背包状态加载事件 - 当背包状态被加载时触发
    /// 参数：string 加载时间戳，bool 是否成功
    /// </summary>
    [System.Serializable]
    public class InventoryStateLoadedEvent : UnityEvent<string, bool> 
    {
        
    }
} 