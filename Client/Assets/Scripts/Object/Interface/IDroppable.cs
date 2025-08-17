using System.Collections.Generic;

/// <summary>
/// 掉落接口 - 定义对象死亡时的掉落行为
/// </summary>
public interface IDroppable
{
    /// <summary>
    /// 获取掉落物品列表
    /// </summary>
    List<DropItem> GetDropItems();
    
    /// <summary>
    /// 处理掉落逻辑
    /// </summary>
    /// <param name="killer">杀死者，可能是玩家或其他攻击者</param>
    void ProcessDrops(IAttacker killer);
    
    /// <summary>
    /// 创建掉落物品到世界中
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <param name="count">数量</param>
    void CreateDroppedItem(int itemId, int count);
    
    /// <summary>
    /// 是否启用掉落功能
    /// </summary>
    bool IsDropEnabled { get; }
} 