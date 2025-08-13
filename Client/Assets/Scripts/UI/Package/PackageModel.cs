using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包道具数据
/// </summary>
[System.Serializable]
public class PackageItem
{
    public int uid;          // 道具实例唯一标识（时间戳）
    public int itemId;       // 道具类型ID
    public int count;        // 道具数量
    public int index;        // 格子索引
    public float modifyTime; // 修改时间

    public PackageItem(int itemId, int count, int index = -1)
    {
        this.uid = ResourceUtils.GenerateUid();
        this.itemId = itemId;
        this.count = count;
        this.index = index;
        this.modifyTime = Time.time;
    }
    
    /// <summary>
    /// 公共属性访问Uid
    /// </summary>
    public int Uid => uid;
}

/// <summary>
/// 背包数据模型 - 负责背包数据管理
/// </summary>
public class PackageModel
{
    // 背包格子数量 - 从GameSettings读取
    public static int MAX_SLOTS => GameSettings.PackageMaxSlots;
    
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

    }

    // 私有字段
    private List<PackageItem> _packageItems = new List<PackageItem>();
    private PackageItem _selectedItem; // 当前选中的物品

    // 公共属性
    public List<PackageItem> PackageItems => _packageItems;
    public PackageItem SelectedItem => _selectedItem;

    // 公共方法
    /// <summary>
    /// 查找第一个空的格子索引
    /// </summary>
    private int FindFirstEmptySlot()
    {
        bool[] occupiedSlots = new bool[MAX_SLOTS];
        
        // 标记已占用的格子
        foreach (var item in _packageItems)
        {
            if (item.index >= 0 && item.index < MAX_SLOTS)
            {
                occupiedSlots[item.index] = true;
            }
        }
        
        // 查找第一个空格子
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (!occupiedSlots[i])
            {
                return i;
            }
        }
        
        return -1; // 背包已满
    }

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
            // 查找空格子
            int emptySlot = FindFirstEmptySlot();
            if (emptySlot == -1)
            {
                Debug.LogWarning("[PackageModel] 背包已满，无法添加道具");
                return false;
            }
            
            _packageItems.Add(new PackageItem(itemId, count, emptySlot));
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
    /// 获取当前背包所有道具 - 供View层调用，按格子索引排序
    /// </summary>
    public List<PackageItem> GetAllItems()
    {
        var sortedItems = new List<PackageItem>(_packageItems);
        sortedItems.Sort((a, b) => a.index.CompareTo(b.index));
        return sortedItems;
    }

    /// <summary>
    /// 根据格子索引获取道具
    /// </summary>
    public PackageItem GetItemByIndex(int index)
    {
        return _packageItems.Find(item => item.index == index);
    }

    /// <summary>
    /// 选中背包中指定格子的物品
    /// </summary>
    public bool SelectItemByIndex(int index)
    {
        // 如果已有选中物品，先取消选中
        if (_selectedItem != null)
        {
            Debug.LogWarning("[PackageModel] 已有选中物品，请先取消选中");
            return false;
        }

        // 查找指定格子的物品
        PackageItem item = _packageItems.Find(i => i.index == index);
        if (item == null)
        {
            Debug.LogWarning($"[PackageModel] 格子 {index} 中没有物品");
            return false;
        }

        // 从背包中移除并设为选中
        _packageItems.Remove(item);
        _selectedItem = item;

        // 发送道具变化事件
        EventManager.Instance.Publish(new ItemChangeEvent(item.itemId, item.count, false));
        // 发送道具选中事件
        EventManager.Instance.Publish(new PackageItemSelectedEvent(item, true));
        Debug.Log($"[PackageModel] 选中物品: {item.itemId}, 数量: {item.count}, 原格子: {item.index}");
        
        return true;
    }

    /// <summary>
    /// 取消选中当前物品，将其放回原格子或空格子
    /// </summary>
    public bool UnselectItem()
    {
        if (_selectedItem == null)
        {
            Debug.LogWarning("[PackageModel] 没有选中的物品");
            return false;
        }

        // 检查原格子是否为空
        PackageItem originalSlotItem = _packageItems.Find(item => item.index == _selectedItem.index);
        if (originalSlotItem == null)
        {
            // 原格子为空，直接放回
            _packageItems.Add(_selectedItem);
        }
        else
        {
            // 原格子被占用，寻找新的空格子
            int emptySlot = FindFirstEmptySlot();
            if (emptySlot == -1)
            {
                Debug.LogWarning("[PackageModel] 背包已满，无法取消选中");
                return false;
            }
            _selectedItem.index = emptySlot;
            _packageItems.Add(_selectedItem);
        }

        // 发送道具变化事件
        EventManager.Instance.Publish(new ItemChangeEvent(_selectedItem.itemId, _selectedItem.count, true));
        // 发送道具取消选中事件
        EventManager.Instance.Publish(new PackageItemSelectedEvent(_selectedItem, false));
        Debug.Log($"[PackageModel] 取消选中物品: {_selectedItem.itemId}, 放入格子: {_selectedItem.index}");
        
        _selectedItem = null;
        return true;
    }

    /// <summary>
    /// 将选中的物品放到指定格子
    /// </summary>
    public bool PlaceSelectedItemToSlot(int targetIndex)
    {
        if (_selectedItem == null)
        {
            Debug.LogWarning("[PackageModel] 没有选中的物品");
            return false;
        }

        if (targetIndex < 0 || targetIndex >= MAX_SLOTS)
        {
            Debug.LogWarning($"[PackageModel] 无效的格子索引: {targetIndex}");
            return false;
        }

        // 检查目标格子是否为空
        PackageItem targetSlotItem = _packageItems.Find(item => item.index == targetIndex);
        if (targetSlotItem != null)
        {
            Debug.LogWarning($"[PackageModel] 格子 {targetIndex} 已被占用");
            return false;
        }

        // 将选中物品放到目标格子
        _selectedItem.index = targetIndex;
        _packageItems.Add(_selectedItem);

        // 发送道具变化事件
        EventManager.Instance.Publish(new ItemChangeEvent(_selectedItem.itemId, _selectedItem.count, true));
        // 发送道具取消选中事件
        EventManager.Instance.Publish(new PackageItemSelectedEvent(_selectedItem, false));
        Debug.Log($"[PackageModel] 将物品 {_selectedItem.itemId} 放到格子 {targetIndex}");
        
        _selectedItem = null;
        return true;
    }
    
    /// <summary>
    /// 检查是否有选中的物品
    /// </summary>
    public bool HasSelectedItem()
    {
        return _selectedItem != null;
    }
    
    // 清空所有道具 - 用于加载存档
    public void ClearAllItems()
    {
        _packageItems.Clear();
        _selectedItem = null; // 同时清空选中的物品
        Debug.Log("[PackageModel] All items cleared");
    }
    
    /// <summary>
    /// 直接加载道具列表 - 用于存档加载，保持原有的格子位置
    /// </summary>
    public void LoadItemsFromSave(List<PackageItem> items)
    {
        _packageItems.Clear();
        _selectedItem = null; // 清空选中的物品
        
        if (items != null)
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    _packageItems.Add(item);
                }
            }
        }
        
        Debug.Log($"[PackageModel] Loaded {_packageItems.Count} items from save");
        
        // 发送背包刷新事件，通知UI刷新
        EventManager.Instance.Publish(new PackageRefreshEvent(_packageItems.Count));
    }
    
    /// <summary>
    /// 清除选中状态（不放回背包）- 用于物品被完全消耗的情况
    /// </summary>
    public void ClearSelectedItem()
    {
        if (_selectedItem != null)
        {
            // 发送取消选中事件
            EventManager.Instance.Publish(new PackageItemSelectedEvent(_selectedItem, false));
            Debug.Log($"[PackageModel] 清除选中物品: {_selectedItem.itemId}（已完全消耗）");
            _selectedItem = null;
        }
    }
}
