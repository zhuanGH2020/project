using UnityEngine;
using static EventSystemEnums;

/// <summary>
/// 物品数据类
/// 用于事件系统的物品信息传递
/// 遵循项目命名约定：PascalCase类名，camelCase私有字段
/// </summary>
[System.Serializable]
public class ItemData
{
    #region 序列化字段
    
    [Header("基础信息")]
    [SerializeField] private int itemId;
    [SerializeField] private string itemName;
    [SerializeField, TextArea(2, 4)] private string itemDescription;
    [SerializeField] private Sprite itemIcon;
    
    [Header("物品属性")]
    [SerializeField] private ItemType itemType = ItemType.Material;
    [SerializeField] private ItemQuality itemQuality = ItemQuality.Common;
    [SerializeField, Range(1, 999)] private int maxStackSize = 1;
    [SerializeField, Range(0.1f, 100f)] private float itemWeight = 1f;
    [SerializeField, Range(0, 9999)] private int itemValue = 0;
    
    [Header("耐久度系统")]
    [SerializeField] private bool hasDurability = false;
    [SerializeField, Range(1f, 1000f)] private float maxDurability = 100f;
    [SerializeField, Range(0f, 1000f)] private float currentDurability = 100f;
    
    [Header("使用属性")]
    [SerializeField] private bool isConsumable = false;
    [SerializeField] private bool isEquippable = false;
    [SerializeField] private bool isTradeable = true;
    [SerializeField] private bool isDroppable = true;
    
    #endregion
    
    #region 公共属性
    
    /// <summary>
    /// 物品唯一标识符
    /// </summary>
    public int ItemId => itemId;
    
    /// <summary>
    /// 物品名称
    /// </summary>
    public string ItemName => itemName;
    
    /// <summary>
    /// 物品描述
    /// </summary>
    public string ItemDescription => itemDescription;
    
    /// <summary>
    /// 物品图标
    /// </summary>
    public Sprite ItemIcon => itemIcon;
    
    /// <summary>
    /// 物品类型
    /// </summary>
    public ItemType ItemType => itemType;
    
    /// <summary>
    /// 物品品质
    /// </summary>
    public ItemQuality ItemQuality => itemQuality;
    
    /// <summary>
    /// 最大堆叠数量
    /// </summary>
    public int MaxStackSize => maxStackSize;
    
    /// <summary>
    /// 物品重量
    /// </summary>
    public float ItemWeight => itemWeight;
    
    /// <summary>
    /// 物品价值
    /// </summary>
    public int ItemValue => itemValue;
    
    /// <summary>
    /// 是否有耐久度系统
    /// </summary>
    public bool HasDurability => hasDurability;
    
    /// <summary>
    /// 最大耐久度
    /// </summary>
    public float MaxDurability => maxDurability;
    
    /// <summary>
    /// 当前耐久度
    /// </summary>
    public float CurrentDurability => currentDurability;
    
    /// <summary>
    /// 是否可消耗
    /// </summary>
    public bool IsConsumable => isConsumable;
    
    /// <summary>
    /// 是否可装备
    /// </summary>
    public bool IsEquippable => isEquippable;
    
    /// <summary>
    /// 是否可交易
    /// </summary>
    public bool IsTradeable => isTradeable;
    
    /// <summary>
    /// 是否可丢弃
    /// </summary>
    public bool IsDroppable => isDroppable;
    
    #endregion
    
    #region 构造函数
    
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public ItemData()
    {
        
    }
    
    /// <summary>
    /// 带参数的构造函数
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <param name="name">物品名称</param>
    /// <param name="type">物品类型</param>
    public ItemData(int id, string name, ItemType type)
    {
        itemId = id;
        itemName = name;
        itemType = type;
        currentDurability = maxDurability;
    }
    
    /// <summary>
    /// 完整参数构造函数
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <param name="name">物品名称</param>
    /// <param name="type">物品类型</param>
    /// <param name="quality">物品品质</param>
    /// <param name="stackSize">最大堆叠数量</param>
    public ItemData(int id, string name, ItemType type, ItemQuality quality, int stackSize)
    {
        itemId = id;
        itemName = name;
        itemType = type;
        itemQuality = quality;
        maxStackSize = stackSize;
        currentDurability = maxDurability;
    }
    
    #endregion
    
    #region 公共方法
    
    /// <summary>
    /// 设置当前耐久度
    /// 提供参数验证和范围限制
    /// </summary>
    /// <param name="durability">新的耐久度值</param>
    public void SetCurrentDurability(float durability)
    {
        if (hasDurability)
        {
            currentDurability = Mathf.Clamp(durability, 0f, maxDurability);
        }
    }
    
    /// <summary>
    /// 减少耐久度
    /// </summary>
    /// <param name="amount">减少的耐久度</param>
    /// <returns>是否成功减少</returns>
    public bool ReduceDurability(float amount)
    {
        if (!hasDurability || amount <= 0f)
            return false;
            
        float oldDurability = currentDurability;
        currentDurability = Mathf.Max(0f, currentDurability - amount);
        
        return currentDurability != oldDurability;
    }
    
    /// <summary>
    /// 修复耐久度
    /// </summary>
    /// <param name="amount">修复的耐久度</param>
    /// <returns>是否成功修复</returns>
    public bool RepairDurability(float amount)
    {
        if (!hasDurability || amount <= 0f || currentDurability >= maxDurability)
            return false;
            
        float oldDurability = currentDurability;
        currentDurability = Mathf.Min(maxDurability, currentDurability + amount);
        
        return currentDurability != oldDurability;
    }
    
    /// <summary>
    /// 获取耐久度百分比
    /// 用于UI显示和游戏逻辑判断
    /// </summary>
    /// <returns>耐久度百分比 (0-1)</returns>
    public float GetDurabilityPercentage()
    {
        if (!hasDurability || maxDurability <= 0f)
            return 1f;
            
        return currentDurability / maxDurability;
    }
    
    /// <summary>
    /// 检查物品是否已损坏
    /// </summary>
    /// <returns>是否已损坏</returns>
    public bool IsBroken()
    {
        return hasDurability && currentDurability <= 0f;
    }
    
    /// <summary>
    /// 检查物品是否可以堆叠
    /// </summary>
    /// <param name="otherItem">要堆叠的物品</param>
    /// <returns>是否可以堆叠</returns>
    public bool CanStackWith(ItemData otherItem)
    {
        if (otherItem == null || itemId != otherItem.itemId)
            return false;
            
        // 有耐久度的物品通常不能堆叠
        if (hasDurability || otherItem.hasDurability)
            return false;
            
        return maxStackSize > 1;
    }
    
    /// <summary>
    /// 获取物品的显示名称（包含品质信息）
    /// </summary>
    /// <returns>带品质的物品名称</returns>
    public string GetDisplayName()
    {
        string qualityPrefix = GetQualityPrefix();
        return string.IsNullOrEmpty(qualityPrefix) ? itemName : $"{qualityPrefix}{itemName}";
    }
    
    /// <summary>
    /// 获取品质前缀
    /// </summary>
    /// <returns>品质前缀字符串</returns>
    private string GetQualityPrefix()
    {
        switch (itemQuality)
        {
            case ItemQuality.Common:
                return "";
            case ItemQuality.Uncommon:
                return "[优质] ";
            case ItemQuality.Rare:
                return "[稀有] ";
            case ItemQuality.Epic:
                return "[史诗] ";
            case ItemQuality.Legendary:
                return "[传说] ";
            default:
                return "";
        }
    }
    
    /// <summary>
    /// 创建物品数据的深拷贝
    /// 避免引用问题
    /// </summary>
    /// <returns>物品数据副本</returns>
    public ItemData CreateCopy()
    {
        ItemData copy = new ItemData();
        copy.itemId = itemId;
        copy.itemName = itemName;
        copy.itemDescription = itemDescription;
        copy.itemIcon = itemIcon;
        copy.itemType = itemType;
        copy.itemQuality = itemQuality;
        copy.maxStackSize = maxStackSize;
        copy.itemWeight = itemWeight;
        copy.itemValue = itemValue;
        copy.hasDurability = hasDurability;
        copy.maxDurability = maxDurability;
        copy.currentDurability = currentDurability;
        copy.isConsumable = isConsumable;
        copy.isEquippable = isEquippable;
        copy.isTradeable = isTradeable;
        copy.isDroppable = isDroppable;
        
        return copy;
    }
    
    /// <summary>
    /// 验证物品数据的有效性
    /// 用于数据完整性检查
    /// </summary>
    /// <returns>验证结果和错误信息</returns>
    public (bool isValid, string errorMessage) ValidateData()
    {
        if (itemId <= 0)
            return (false, "物品ID必须大于0");
            
        if (string.IsNullOrEmpty(itemName))
            return (false, "物品名称不能为空");
            
        if (maxStackSize <= 0)
            return (false, "最大堆叠数量必须大于0");
            
        if (itemWeight < 0f)
            return (false, "物品重量不能为负数");
            
        if (hasDurability && maxDurability <= 0f)
            return (false, "有耐久度的物品最大耐久度必须大于0");
            
        if (hasDurability && currentDurability < 0f)
            return (false, "当前耐久度不能为负数");
            
        return (true, "");
    }
    
    #endregion
    
    #region 重写方法
    
    /// <summary>
    /// 重写ToString方法，用于调试
    /// </summary>
    /// <returns>物品信息字符串</returns>
    public override string ToString()
    {
        return $"ItemData[ID:{itemId}, Name:{itemName}, Type:{itemType}, Quality:{itemQuality}]";
    }
    
    /// <summary>
    /// 重写Equals方法
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>是否相等</returns>
    public override bool Equals(object obj)
    {
        if (obj is ItemData other)
        {
            return itemId == other.itemId;
        }
        return false;
    }
    
    /// <summary>
    /// 重写GetHashCode方法
    /// </summary>
    /// <returns>哈希码</returns>
    public override int GetHashCode()
    {
        return itemId.GetHashCode();
    }
    
    #endregion
} 