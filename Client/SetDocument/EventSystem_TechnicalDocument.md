# Event事件框架技术文档（精简版）
## Unity 3D游戏开发 - 背包系统事件处理

**创建日期**：2025年1月24日  
**版本信息**：v1.0 简化版  
**基于规范**：项目`.cursorrules`规范，专注核心功能，避免过度设计

### 1. 项目概述

本文档基于项目`.cursorrules`规范设计，实现背包系统的核心事件处理功能：**获得道具**、**消耗道具**等基本操作。采用单一管理器设计，避免复杂架构。

**参考实现**：基于`Assets/Script/input/InputController.cs`的UnityEvent模式

### 2. 核心架构设计

#### 2.1 简化架构
```
InventoryEventManager (背包事件管理器)
├── 获得道具事件 (ItemObtainedEvent)
├── 消耗道具事件 (ItemConsumedEvent)  
├── 丢弃道具事件 (ItemDroppedEvent)
└── 背包已满事件 (InventoryFullEvent)
```

#### 2.2 文件结构
```
Assets/Script/
├── Manager/
│   └── InventoryEventManager.cs (背包事件管理器)
└── Utils/
    └── InventorySystemEvents.cs (背包系统事件定义)
```

### 3. 核心事件定义

```csharp
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
```

### 4. 背包事件管理器实现

```csharp
using UnityEngine;

/// <summary>
/// 背包事件管理器
/// 参考实现：Assets/Script/input/InputController.cs 的MonoBehaviour设计模式
/// 遵循项目Manager模式和Unity特定规范
/// </summary>
public class InventoryEventManager : MonoBehaviour
{
    #region 序列化字段
    
    [Header("背包事件")]
    [SerializeField, Tooltip("物品获得时触发的事件")]
    public InventorySystemEvents.ItemObtainedEvent onItemObtained;
    
    [SerializeField, Tooltip("物品消耗时触发的事件")]
    public InventorySystemEvents.ItemConsumedEvent onItemConsumed;
    
    [SerializeField, Tooltip("物品丢弃时触发的事件")]
    public InventorySystemEvents.ItemDroppedEvent onItemDropped;
    
    [SerializeField, Tooltip("背包已满时触发的事件")]
    public InventorySystemEvents.InventoryFullEvent onInventoryFull;
    
    #endregion
    
    #region 单例模式
    
    public static InventoryEventManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    #endregion
    
    #region 事件触发方法
    
    /// <summary>
    /// 触发物品获得事件
    /// </summary>
    /// <param name="itemData">获得的物品数据</param>
    /// <param name="quantity">获得数量</param>
    public void TriggerItemObtained(ItemData itemData, int quantity)
    {
        if (itemData == null || quantity <= 0)
            return;
            
        onItemObtained?.Invoke(itemData, quantity);
        
        if (Application.isEditor)
            Debug.Log($"[InventoryEvent] 获得物品: {itemData.GetDisplayName()} x{quantity}");
    }
    
    /// <summary>
    /// 触发物品消耗事件
    /// </summary>
    /// <param name="itemData">消耗的物品数据</param>
    /// <param name="quantity">消耗数量</param>
    public void TriggerItemConsumed(ItemData itemData, int quantity)
    {
        if (itemData == null || quantity <= 0)
            return;
            
        onItemConsumed?.Invoke(itemData, quantity);
        
        if (Application.isEditor)
            Debug.Log($"[InventoryEvent] 消耗物品: {itemData.GetDisplayName()} x{quantity}");
    }
    
    /// <summary>
    /// 触发物品丢弃事件
    /// </summary>
    /// <param name="itemData">丢弃的物品数据</param>
    /// <param name="quantity">丢弃数量</param>
    public void TriggerItemDropped(ItemData itemData, int quantity)
    {
        if (itemData == null || quantity <= 0)
            return;
            
        onItemDropped?.Invoke(itemData, quantity);
        
        if (Application.isEditor)
            Debug.Log($"[InventoryEvent] 丢弃物品: {itemData.GetDisplayName()} x{quantity}");
    }
    
    /// <summary>
    /// 触发背包已满事件
    /// </summary>
    /// <param name="itemData">无法添加的物品数据</param>
    public void TriggerInventoryFull(ItemData itemData)
    {
        if (itemData == null)
            return;
            
        onInventoryFull?.Invoke(itemData);
        
        if (Application.isEditor)
            Debug.LogWarning($"[InventoryEvent] 背包已满，无法添加: {itemData.GetDisplayName()}");
    }
    
    #endregion
}
```

### 5. 使用示例

#### 5.1 在背包管理器中集成

```csharp
// 参考：Assets/Script/Manager/InventoryManager.cs 的实现模式
public class SimpleInventoryManager : MonoBehaviour
{
    [Header("背包设置")]
    [SerializeField, Range(10, 50)] private int maxCapacity = 20;
    
    private Dictionary<int, int> _itemQuantities;
    private InventoryEventManager _eventManager;
    
    void Start()
    {
        _itemQuantities = new Dictionary<int, int>();
        _eventManager = InventoryEventManager.Instance;
    }
    
    /// <summary>
    /// 添加物品到背包
    /// </summary>
    public bool AddItem(ItemData itemData, int quantity)
    {
        if (_itemQuantities.Count >= maxCapacity)
        {
            _eventManager.TriggerInventoryFull(itemData);
            return false;
        }
        
        if (_itemQuantities.ContainsKey(itemData.ItemId))
            _itemQuantities[itemData.ItemId] += quantity;
        else
            _itemQuantities[itemData.ItemId] = quantity;
        
        _eventManager.TriggerItemObtained(itemData, quantity);
        return true;
    }
    
    /// <summary>
    /// 从背包移除物品
    /// </summary>
    public int RemoveItem(ItemData itemData, int quantity)
    {
        if (!_itemQuantities.ContainsKey(itemData.ItemId))
            return 0;
        
        int actualRemoved = Mathf.Min(_itemQuantities[itemData.ItemId], quantity);
        _itemQuantities[itemData.ItemId] -= actualRemoved;
        
        if (_itemQuantities[itemData.ItemId] <= 0)
            _itemQuantities.Remove(itemData.ItemId);
        
        _eventManager.TriggerItemConsumed(itemData, actualRemoved);
        return actualRemoved;
    }
}
```

#### 5.2 在UI系统中监听事件

```csharp
// 参考项目UI系统MVC模式
public class InventoryUIController : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Text messageText;
    
    void Start()
    {
        var eventManager = InventoryEventManager.Instance;
        
        // 监听获得物品事件
        eventManager.onItemObtained.AddListener(OnItemObtained);
        
        // 监听背包已满事件
        eventManager.onInventoryFull.AddListener(OnInventoryFull);
    }
    
    private void OnItemObtained(ItemData itemData, int quantity)
    {
        messageText.text = $"获得 {itemData.GetDisplayName()} x{quantity}";
    }
    
    private void OnInventoryFull(ItemData itemData)
    {
        messageText.text = $"背包已满！无法获得 {itemData.GetDisplayName()}";
    }
    
    void OnDestroy()
    {
        var eventManager = InventoryEventManager.Instance;
        if (eventManager != null)
        {
            eventManager.onItemObtained.RemoveListener(OnItemObtained);
            eventManager.onInventoryFull.RemoveListener(OnInventoryFull);
        }
    }
}
```

### 6. 核心优势

#### 6.1 简洁设计
- ✅ **单一管理器**：避免复杂的多Manager架构
- ✅ **核心功能**：专注于背包系统的基本事件处理
- ✅ **易于理解**：代码逻辑清晰，便于维护

#### 6.2 符合项目规范
- ✅ **代码风格**：遵循PascalCase、XML注释等项目规范
- ✅ **Unity特性**：使用`[SerializeField]`、`[Tooltip]`等特性
- ✅ **性能优化**：基础的空值检查和调试日志优化

#### 6.3 易于集成
- ✅ **UnityEvent模式**：与现有InputController等系统保持一致
- ✅ **单例模式**：全局访问，便于系统间通信
- ✅ **Inspector配置**：可视化配置事件监听器

### 7. 部署指导

#### 7.1 快速部署步骤
1. **创建脚本文件**：按照文件结构创建相应脚本
2. **添加到场景**：创建GameObject并挂载`InventoryEventManager`
3. **配置事件**：在Inspector面板配置事件监听器
4. **集成到现有系统**：在背包管理器中调用事件触发方法

#### 7.2 测试验证
```csharp
// 简单测试代码
void TestEvents()
{
    var testItem = new ItemData(1, "测试物品", ItemType.Material);
    var eventManager = InventoryEventManager.Instance;
    
    // 测试获得物品事件
    eventManager.TriggerItemObtained(testItem, 5);
    
    // 测试消耗物品事件
    eventManager.TriggerItemConsumed(testItem, 2);
}
```

---

## 技术规范总结

本Event事件框架技术文档严格基于项目`.cursorrules`规范设计，采用**精简化设计**，具有以下特点：

### **1. 专注核心功能**
- ✅ 背包系统的获得道具、消耗道具等基本功能
- ✅ 单一`InventoryEventManager`管理器设计
- ✅ 简化的事件定义，避免过度复杂化

### **2. 符合项目规范**
- ✅ 基于`Assets/Script/input/InputController.cs`的UnityEvent模式
- ✅ 遵循Manager模式和MonoBehaviour规范
- ✅ 使用PascalCase命名、XML文档注释

### **3. 易于实现和维护**
- ✅ 代码量少，逻辑清晰
- ✅ 基础的错误处理和调试支持
- ✅ 完整的使用示例和部署指导

这个精简版Event事件框架可以直接集成到Unity 3D项目中，为背包系统提供可靠的事件处理能力！🚀