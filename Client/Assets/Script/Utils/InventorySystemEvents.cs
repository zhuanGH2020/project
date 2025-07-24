using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 背包系统事件定义类
/// 参考实现：Assets/Script/input/InputController.cs 的UnityEvent模式
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
    /// 参数：ItemData 物品数据，int 丢弃数量
    /// </summary>
    [System.Serializable]
    public class ItemDroppedEvent : UnityEvent<ItemData, int> 
    {
        
    }

    /// <summary>
    /// 背包已满事件 - 当背包容量不足时触发
    /// 参数：ItemData 尝试添加的物品数据
    /// </summary>
    [System.Serializable]
    public class InventoryFullEvent : UnityEvent<ItemData> 
    {
        
    }
} 