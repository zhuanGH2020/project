using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包道具数据
/// </summary>
[System.Serializable]
public class PackageItem
{
    public int itemId;
    public int count;
    public float modifyTime;

    public PackageItem(int itemId, int count)
    {
        this.itemId = itemId;
        this.count = count;
        this.modifyTime = Time.time;
    }
}

/// <summary>
/// 背包数据模型 - 负责背包数据管理
/// </summary>
public class PackageModel
{
    // 单例实现
    private static PackageModel _instance;
    public static PackageModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PackageModel();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 私有构造函数，防止外部实例化
    /// </summary>
    private PackageModel()
    {
        // 初始化测试数据
        AddItem(1000, 10); // 添加10个木头
        AddItem(4001, 5);  // 添加5个铁块
    }

    // 私有字段
    private List<PackageItem> _packageItems = new List<PackageItem>();

    // 公共属性
    public List<PackageItem> PackageItems => _packageItems;

    // 公共方法
    /// <summary>
    /// 添加道具到背包
    /// </summary>
    public bool AddItem(int itemId, int count)
    {
        if (count <= 0) return false;

        // 查找已有道具
        PackageItem existingItem = _packageItems.Find(item => item.itemId == itemId);
        
        if (existingItem != null)
        {
            existingItem.count += count;
            existingItem.modifyTime = Time.time;
        }
        else
        {
            _packageItems.Add(new PackageItem(itemId, count));
        }

        // 发送道具变化事件
        EventManager.Instance.Publish(new ItemChangeEvent(itemId, count, true));
        return true;
    }

    /// <summary>
    /// 从背包移除道具
    /// </summary>
    public bool RemoveItem(int itemId, int count)
    {
        if (count <= 0) return false;

        PackageItem item = _packageItems.Find(i => i.itemId == itemId);
        if (item == null || item.count < count) return false;

        item.count -= count;
        item.modifyTime = Time.time;
        
        if (item.count <= 0)
        {
            _packageItems.Remove(item);
        }

        // 发送道具变化事件
        EventManager.Instance.Publish(new ItemChangeEvent(itemId, count, false));
        return true;
    }

    /// <summary>
    /// 获取道具数量
    /// </summary>
    public int GetItemCount(int itemId)
    {
        PackageItem item = _packageItems.Find(i => i.itemId == itemId);
        return item?.count ?? 0;
    }

    /// <summary>
    /// 检查是否有足够道具
    /// </summary>
    public bool HasEnoughItem(int itemId, int count)
    {
        return GetItemCount(itemId) >= count;
    }

    /// <summary>
    /// 获取当前背包所有道具 - 供View层调用
    /// </summary>
    public List<PackageItem> GetAllItems()
    {
        return new List<PackageItem>(_packageItems);
    }
}
