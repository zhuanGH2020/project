using System.Collections.Generic;
using UnityEngine;
using static EventSystemEnums;

/// <summary>
/// 简化版背包管理器
/// 展示事件系统基本使用方式
/// 遵循项目规范和Manager模式
/// </summary>
public class SimpleInventoryManager : MonoBehaviour
{
    [Header("背包设置")]
    [SerializeField, Range(10, 50)] private int maxCapacity = 20;
    [SerializeField, Range(10f, 500f)] private float maxWeight = 100f;
    
    [Header("当前状态")]
    [SerializeField] private int currentItemCount = 0;
    [SerializeField] private float currentWeight = 0f;
    
    // 私有字段
    private Dictionary<int, int> _itemQuantities; // 物品ID -> 数量
    private InventoryEventManager _eventManager;
    
    // 公共属性
    public int MaxCapacity => maxCapacity;
    public int CurrentItemCount => currentItemCount;
    public float CurrentWeight => currentWeight;
    public bool IsFull => currentItemCount >= maxCapacity;
    
    void Awake()
    {
        InitializeInventory();
    }
    
    void Start()
    {
        CacheEventManager();
    }
    
    /// <summary>
    /// 初始化背包
    /// </summary>
    private void InitializeInventory()
    {
        _itemQuantities = new Dictionary<int, int>();
        Debug.Log("[SimpleInventoryManager] 背包初始化完成");
    }
    
    /// <summary>
    /// 缓存事件管理器
    /// </summary>
    private void CacheEventManager()
    {
        _eventManager = FindObjectOfType<InventoryEventManager>();
        if (_eventManager == null)
        {
            Debug.LogError("[SimpleInventoryManager] 未找到事件管理器！");
        }
    }
    
    /// <summary>
    /// 添加物品
    /// </summary>
    /// <param name="itemData">物品数据</param>
    /// <param name="quantity">数量</param>
    /// <returns>是否成功添加</returns>
    public bool AddItem(ItemData itemData, int quantity)
    {
        if (itemData == null || quantity <= 0)
            return false;
        
        // 检查空间
        if (IsFull)
        {
            _eventManager?.TriggerInventoryFull(itemData);
            return false;
        }
        
        // 检查重量
        float addWeight = itemData.ItemWeight * quantity;
        if (currentWeight + addWeight > maxWeight)
        {
            _eventManager?.TriggerInventoryOverweight(currentWeight + addWeight, maxWeight);
            return false;
        }
        
        // 添加物品
        int itemId = itemData.ItemId;
        if (_itemQuantities.ContainsKey(itemId))
        {
            int oldQuantity = _itemQuantities[itemId];
            _itemQuantities[itemId] += quantity;
            _eventManager?.TriggerItemStackChanged(itemData, oldQuantity, _itemQuantities[itemId]);
        }
        else
        {
            _itemQuantities[itemId] = quantity;
            currentItemCount++;
        }
        
        // 更新重量
        float oldWeight = currentWeight;
        currentWeight += addWeight;
        
        // 触发事件
        _eventManager?.TriggerItemObtained(itemData, quantity);
        _eventManager?.TriggerInventoryWeightChanged(oldWeight, currentWeight, maxWeight);
        
        return true;
    }
    
    /// <summary>
    /// 移除物品
    /// </summary>
    /// <param name="itemData">物品数据</param>
    /// <param name="quantity">数量</param>
    /// <returns>实际移除的数量</returns>
    public int RemoveItem(ItemData itemData, int quantity)
    {
        if (itemData == null || quantity <= 0)
            return 0;
        
        int itemId = itemData.ItemId;
        if (!_itemQuantities.ContainsKey(itemId))
            return 0;
        
        int availableQuantity = _itemQuantities[itemId];
        int removeQuantity = Mathf.Min(availableQuantity, quantity);
        
        // 移除物品
        int oldQuantity = _itemQuantities[itemId];
        _itemQuantities[itemId] -= removeQuantity;
        
        if (_itemQuantities[itemId] <= 0)
        {
            _itemQuantities.Remove(itemId);
            currentItemCount--;
        }
        
        // 更新重量
        float oldWeight = currentWeight;
        currentWeight -= itemData.ItemWeight * removeQuantity;
        
        // 触发事件
        _eventManager?.TriggerItemConsumed(itemData, removeQuantity);
        _eventManager?.TriggerItemStackChanged(itemData, oldQuantity, _itemQuantities.ContainsKey(itemId) ? _itemQuantities[itemId] : 0);
        _eventManager?.TriggerInventoryWeightChanged(oldWeight, currentWeight, maxWeight);
        
        return removeQuantity;
    }
    
    /// <summary>
    /// 获取物品数量
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns>数量</returns>
    public int GetItemQuantity(int itemId)
    {
        return _itemQuantities.ContainsKey(itemId) ? _itemQuantities[itemId] : 0;
    }
    
    /// <summary>
    /// 检查是否拥有物品
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <param name="requiredQuantity">所需数量</param>
    /// <returns>是否拥有</returns>
    public bool HasItem(int itemId, int requiredQuantity = 1)
    {
        return GetItemQuantity(itemId) >= requiredQuantity;
    }
} 