# Event事件框架技术文档
## Unity 3D游戏开发 - 背包系统事件处理

### 1. 项目概述

本文档设计了一个统一的事件框架，专门用于处理Unity 3D游戏中的背包系统事件，包括道具获得、消耗、装备、制作等各种操作。框架基于现有项目的UnityEvent模式，提供类型安全、高性能的事件处理机制。

### 2. 设计原则

#### 2.1 基于现有项目风格
参考项目中 `Assets/Script/input/InputController.cs` 和 `Assets/Script/input/MovementController.cs` 的事件处理方式：

- 使用UnityEvent系统确保Editor可视化配置
- 使用[System.Serializable]创建自定义事件类
- 提供类型安全的事件参数传递
- 支持Inspector中的事件绑定
- 使用?.Invoke()进行安全的事件调用

#### 2.2 Unity 3D游戏开发规范
- **命名约定**: 事件类使用PascalCase，事件实例使用camelCase
- **性能优化**: 缓存事件引用，避免频繁分配
- **错误处理**: 提供空值检查和异常处理
- **扩展性**: 支持动态添加和移除事件监听器

### 3. 事件系统架构

#### 3.1 核心架构图
```
EventManager (事件管理器)
├── InventoryEventDispatcher (背包事件分发器)
├── ItemEventDispatcher (物品事件分发器) 
├── CraftingEventDispatcher (制作事件分发器)
└── EventSubscriptionManager (事件订阅管理器)

事件类型：
├── InventoryEvents (背包事件)
├── ItemEvents (物品事件)
├── CraftingEvents (制作事件)
└── EquipmentEvents (装备事件)
```

#### 3.2 文件结构规划
```
Assets/Script/
├── Core/
│   └── Event/
│       ├── Base/
│       │   ├── EventManager.cs (事件管理器)
│       │   ├── BaseEventDispatcher.cs (基础事件分发器)
│       │   └── EventSubscriptionManager.cs (订阅管理器)
│       ├── Inventory/
│       │   ├── InventoryEvents.cs (背包事件定义)
│       │   ├── ItemEvents.cs (物品事件定义)
│       │   ├── InventoryEventDispatcher.cs (背包事件分发器)
│       │   └── ItemEventDispatcher.cs (物品事件分发器)
│       ├── Crafting/
│       │   ├── CraftingEvents.cs (制作事件定义)
│       │   └── CraftingEventDispatcher.cs (制作事件分发器)
│       └── Equipment/
│           ├── EquipmentEvents.cs (装备事件定义)
│           └── EquipmentEventDispatcher.cs (装备事件分发器)
```

### 4. 核心事件定义

#### 4.1 背包系统事件类
参考 `InputController.cs` 中的 `MoveInputEvent` 实现方式：

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 背包系统相关事件定义
/// 参考：Assets/Script/input/InputController.cs 的事件实现方式
/// </summary>
public static class InventoryEvents
{
    /// <summary>
    /// 物品获得事件 - 当玩家获得物品时触发
    /// </summary>
    [System.Serializable]
    public class ItemObtainedEvent : UnityEvent<ItemData, int> { }
    
    /// <summary>
    /// 物品消耗事件 - 当玩家消耗物品时触发
    /// </summary>
    [System.Serializable]
    public class ItemConsumedEvent : UnityEvent<ItemData, int> { }
    
    /// <summary>
    /// 物品丢弃事件 - 当玩家丢弃物品时触发
    /// </summary>
    [System.Serializable]
    public class ItemDroppedEvent : UnityEvent<ItemData, int, Vector3> { }
    
    /// <summary>
    /// 背包容量变化事件 - 当背包容量发生变化时触发
    /// </summary>
    [System.Serializable]
    public class InventoryCapacityChangedEvent : UnityEvent<int, int> { }
    
    /// <summary>
    /// 背包已满事件 - 当背包空间不足时触发
    /// </summary>
    [System.Serializable]
    public class InventoryFullEvent : UnityEvent<ItemData> { }
    
    /// <summary>
    /// 物品堆叠变化事件 - 当物品数量发生变化时触发
    /// </summary>
    [System.Serializable]
    public class ItemStackChangedEvent : UnityEvent<ItemData, int, int> { }
}
```

#### 4.2 物品交互事件类

```csharp
/// <summary>
/// 物品交互相关事件定义
/// </summary>
public static class ItemEvents
{
    /// <summary>
    /// 物品使用事件 - 当玩家使用物品时触发
    /// </summary>
    [System.Serializable]
    public class ItemUsedEvent : UnityEvent<ItemData, Actor> { }
    
    /// <summary>
    /// 物品装备事件 - 当玩家装备物品时触发
    /// </summary>
    [System.Serializable]
    public class ItemEquippedEvent : UnityEvent<ItemData, EquipmentSlot> { }
    
    /// <summary>
    /// 物品卸下事件 - 当玩家卸下装备时触发
    /// </summary>
    [System.Serializable]
    public class ItemUnequippedEvent : UnityEvent<ItemData, EquipmentSlot> { }
    
    /// <summary>
    /// 物品耐久度变化事件 - 当物品耐久度发生变化时触发
    /// </summary>
    [System.Serializable]
    public class ItemDurabilityChangedEvent : UnityEvent<ItemData, float, float> { }
    
    /// <summary>
    /// 物品损坏事件 - 当物品耐久度为0时触发
    /// </summary>
    [System.Serializable]
    public class ItemBrokenEvent : UnityEvent<ItemData> { }
}
```

#### 4.3 制作系统事件类

```csharp
/// <summary>
/// 制作系统相关事件定义
/// </summary>
public static class CraftingEvents
{
    /// <summary>
    /// 制作开始事件 - 当开始制作物品时触发
    /// </summary>
    [System.Serializable]
    public class CraftingStartedEvent : UnityEvent<CraftingRecipe> { }
    
    /// <summary>
    /// 制作完成事件 - 当制作完成时触发
    /// </summary>
    [System.Serializable]
    public class CraftingCompletedEvent : UnityEvent<ItemData, int> { }
    
    /// <summary>
    /// 制作失败事件 - 当制作失败时触发
    /// </summary>
    [System.Serializable]
    public class CraftingFailedEvent : UnityEvent<CraftingRecipe, string> { }
    
    /// <summary>
    /// 配方解锁事件 - 当解锁新配方时触发
    /// </summary>
    [System.Serializable]
    public class RecipeUnlockedEvent : UnityEvent<CraftingRecipe> { }
    
    /// <summary>
    /// 材料不足事件 - 当制作材料不足时触发
    /// </summary>
    [System.Serializable]
    public class InsufficientMaterialsEvent : UnityEvent<CraftingRecipe, List<ItemData>> { }
}
```

### 5. 事件管理器实现

#### 5.1 EventManager核心类
参考 `MovementController.cs` 中的组件缓存和性能优化方式：

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件管理器 - 统一管理游戏中的所有事件
/// 参考：Assets/Script/input/MovementController.cs 的组件缓存方式
/// </summary>
public class EventManager : MonoBehaviour
{
    [Header("事件管理器设置")]
    [SerializeField] private bool enableEventLogging = true;
    [SerializeField] private int maxEventQueueSize = 1000;
    
    // 单例模式
    public static EventManager Instance { get; private set; }
    
    // 事件分发器缓存（参考MovementController的cachedTransform模式）
    private InventoryEventDispatcher inventoryDispatcher;
    private ItemEventDispatcher itemDispatcher;
    private CraftingEventDispatcher craftingDispatcher;
    private EquipmentEventDispatcher equipmentDispatcher;
    
    // 事件队列管理
    private Queue<System.Action> eventQueue;
    private bool isProcessingEvents;
    
    #region Unity生命周期
    
    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEventManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 缓存所有事件分发器组件（提升性能）
        CacheEventDispatchers();
    }
    
    void Update()
    {
        // 处理事件队列
        ProcessEventQueue();
    }
    
    #endregion
    
    #region 初始化方法
    
    /// <summary>
    /// 初始化事件管理器
    /// </summary>
    private void InitializeEventManager()
    {
        eventQueue = new Queue<System.Action>();
        isProcessingEvents = false;
        
        if (enableEventLogging)
            Debug.Log("[EventManager] 事件管理器初始化完成");
    }
    
    /// <summary>
    /// 缓存事件分发器组件（参考MovementController的缓存模式）
    /// </summary>
    private void CacheEventDispatchers()
    {
        // 获取或创建事件分发器
        inventoryDispatcher = GetOrCreateDispatcher<InventoryEventDispatcher>();
        itemDispatcher = GetOrCreateDispatcher<ItemEventDispatcher>();
        craftingDispatcher = GetOrCreateDispatcher<CraftingEventDispatcher>();
        equipmentDispatcher = GetOrCreateDispatcher<EquipmentEventDispatcher>();
        
        if (enableEventLogging)
            Debug.Log("[EventManager] 事件分发器缓存完成");
    }
    
    /// <summary>
    /// 获取或创建指定类型的事件分发器
    /// </summary>
    private T GetOrCreateDispatcher<T>() where T : BaseEventDispatcher
    {
        T dispatcher = GetComponent<T>();
        if (dispatcher == null)
        {
            dispatcher = gameObject.AddComponent<T>();
        }
        return dispatcher;
    }
    
    #endregion
    
    #region 公共接口方法
    
    /// <summary>
    /// 获取背包事件分发器
    /// </summary>
    public InventoryEventDispatcher GetInventoryDispatcher()
    {
        return inventoryDispatcher;
    }
    
    /// <summary>
    /// 获取物品事件分发器
    /// </summary>
    public ItemEventDispatcher GetItemDispatcher()
    {
        return itemDispatcher;
    }
    
    /// <summary>
    /// 获取制作事件分发器
    /// </summary>
    public CraftingEventDispatcher GetCraftingDispatcher()
    {
        return craftingDispatcher;
    }
    
    /// <summary>
    /// 获取装备事件分发器
    /// </summary>
    public EquipmentEventDispatcher GetEquipmentDispatcher()
    {
        return equipmentDispatcher;
    }
    
    /// <summary>
    /// 将事件添加到队列中延迟执行
    /// </summary>
    /// <param name="eventAction">要执行的事件</param>
    public void QueueEvent(System.Action eventAction)
    {
        if (eventQueue.Count < maxEventQueueSize)
        {
            eventQueue.Enqueue(eventAction);
        }
        else if (enableEventLogging)
        {
            Debug.LogWarning("[EventManager] 事件队列已满，丢弃事件");
        }
    }
    
    #endregion
    
    #region 私有方法
    
    /// <summary>
    /// 处理事件队列
    /// </summary>
    private void ProcessEventQueue()
    {
        if (isProcessingEvents || eventQueue.Count == 0)
            return;
            
        isProcessingEvents = true;
        
        try
        {
            // 每帧最多处理5个事件，避免性能问题
            int maxEventsPerFrame = 5;
            int processedEvents = 0;
            
            while (eventQueue.Count > 0 && processedEvents < maxEventsPerFrame)
            {
                System.Action eventAction = eventQueue.Dequeue();
                eventAction?.Invoke();
                processedEvents++;
            }
        }
        catch (System.Exception e)
        {
            if (enableEventLogging)
                Debug.LogError($"[EventManager] 处理事件时发生错误: {e.Message}");
        }
        finally
        {
            isProcessingEvents = false;
        }
    }
    
    #endregion
}
```

#### 5.2 基础事件分发器

```csharp
using UnityEngine;

/// <summary>
/// 基础事件分发器 - 所有事件分发器的基类
/// </summary>
public abstract class BaseEventDispatcher : MonoBehaviour
{
    [Header("分发器设置")]
    [SerializeField] protected bool enableEventLogging = true;
    
    /// <summary>
    /// 分发器名称（用于日志输出）
    /// </summary>
    protected abstract string DispatcherName { get; }
    
    /// <summary>
    /// 初始化分发器
    /// </summary>
    protected virtual void Awake()
    {
        InitializeDispatcher();
    }
    
    /// <summary>
    /// 初始化分发器的抽象方法
    /// </summary>
    protected abstract void InitializeDispatcher();
    
    /// <summary>
    /// 记录事件日志
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="message">日志消息</param>
    protected void LogEvent(string eventName, string message)
    {
        if (enableEventLogging)
        {
            Debug.Log($"[{DispatcherName}] {eventName}: {message}");
        }
    }
    
    /// <summary>
    /// 记录事件错误
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="error">错误消息</param>
    protected void LogEventError(string eventName, string error)
    {
        if (enableEventLogging)
        {
            Debug.LogError($"[{DispatcherName}] {eventName} 错误: {error}");
        }
    }
}
```

#### 5.3 背包事件分发器实现

```csharp
using UnityEngine;

/// <summary>
/// 背包事件分发器 - 处理所有背包相关事件
/// 参考：Assets/Script/input/InputController.cs 的事件处理方式
/// </summary>
public class InventoryEventDispatcher : BaseEventDispatcher
{
    [Header("背包事件")]
    public InventoryEvents.ItemObtainedEvent onItemObtained;
    public InventoryEvents.ItemConsumedEvent onItemConsumed;
    public InventoryEvents.ItemDroppedEvent onItemDropped;
    public InventoryEvents.InventoryCapacityChangedEvent onInventoryCapacityChanged;
    public InventoryEvents.InventoryFullEvent onInventoryFull;
    public InventoryEvents.ItemStackChangedEvent onItemStackChanged;
    
    protected override string DispatcherName => "InventoryEventDispatcher";
    
    protected override void InitializeDispatcher()
    {
        // 初始化所有事件（参考InputController的事件初始化）
        if (onItemObtained == null)
            onItemObtained = new InventoryEvents.ItemObtainedEvent();
            
        if (onItemConsumed == null)
            onItemConsumed = new InventoryEvents.ItemConsumedEvent();
            
        if (onItemDropped == null)
            onItemDropped = new InventoryEvents.ItemDroppedEvent();
            
        if (onInventoryCapacityChanged == null)
            onInventoryCapacityChanged = new InventoryEvents.InventoryCapacityChangedEvent();
            
        if (onInventoryFull == null)
            onInventoryFull = new InventoryEvents.InventoryFullEvent();
            
        if (onItemStackChanged == null)
            onItemStackChanged = new InventoryEvents.ItemStackChangedEvent();
            
        LogEvent("Initialize", "背包事件分发器初始化完成");
    }
    
    #region 背包事件触发方法
    
    /// <summary>
    /// 触发物品获得事件
    /// </summary>
    /// <param name="itemData">获得的物品数据</param>
    /// <param name="quantity">获得数量</param>
    public void TriggerItemObtained(ItemData itemData, int quantity)
    {
        if (itemData == null)
        {
            LogEventError("ItemObtained", "物品数据为空");
            return;
        }
        
        LogEvent("ItemObtained", $"获得物品: {itemData.itemName} x{quantity}");
        onItemObtained?.Invoke(itemData, quantity);
    }
    
    /// <summary>
    /// 触发物品消耗事件
    /// </summary>
    /// <param name="itemData">消耗的物品数据</param>
    /// <param name="quantity">消耗数量</param>
    public void TriggerItemConsumed(ItemData itemData, int quantity)
    {
        if (itemData == null)
        {
            LogEventError("ItemConsumed", "物品数据为空");
            return;
        }
        
        LogEvent("ItemConsumed", $"消耗物品: {itemData.itemName} x{quantity}");
        onItemConsumed?.Invoke(itemData, quantity);
    }
    
    /// <summary>
    /// 触发物品丢弃事件
    /// </summary>
    /// <param name="itemData">丢弃的物品数据</param>
    /// <param name="quantity">丢弃数量</param>
    /// <param name="dropPosition">丢弃位置</param>
    public void TriggerItemDropped(ItemData itemData, int quantity, Vector3 dropPosition)
    {
        if (itemData == null)
        {
            LogEventError("ItemDropped", "物品数据为空");
            return;
        }
        
        LogEvent("ItemDropped", $"丢弃物品: {itemData.itemName} x{quantity} 在位置 {dropPosition}");
        onItemDropped?.Invoke(itemData, quantity, dropPosition);
    }
    
    /// <summary>
    /// 触发背包容量变化事件
    /// </summary>
    /// <param name="oldCapacity">原容量</param>
    /// <param name="newCapacity">新容量</param>
    public void TriggerInventoryCapacityChanged(int oldCapacity, int newCapacity)
    {
        LogEvent("InventoryCapacityChanged", $"背包容量变化: {oldCapacity} -> {newCapacity}");
        onInventoryCapacityChanged?.Invoke(oldCapacity, newCapacity);
    }
    
    /// <summary>
    /// 触发背包已满事件
    /// </summary>
    /// <param name="attemptedItem">尝试添加的物品</param>
    public void TriggerInventoryFull(ItemData attemptedItem)
    {
        LogEvent("InventoryFull", $"背包已满，无法添加物品: {attemptedItem?.itemName ?? "未知物品"}");
        onInventoryFull?.Invoke(attemptedItem);
    }
    
    /// <summary>
    /// 触发物品堆叠变化事件
    /// </summary>
    /// <param name="itemData">物品数据</param>
    /// <param name="oldQuantity">原数量</param>
    /// <param name="newQuantity">新数量</param>
    public void TriggerItemStackChanged(ItemData itemData, int oldQuantity, int newQuantity)
    {
        if (itemData == null)
        {
            LogEventError("ItemStackChanged", "物品数据为空");
            return;
        }
        
        LogEvent("ItemStackChanged", $"物品堆叠变化: {itemData.itemName} {oldQuantity} -> {newQuantity}");
        onItemStackChanged?.Invoke(itemData, oldQuantity, newQuantity);
    }
    
    #endregion
}
```

### 6. 事件使用示例

#### 6.1 背包系统集成示例

```csharp
using UnityEngine;

/// <summary>
/// 背包管理器 - 集成事件系统的示例
/// 参考：Assets/Script/input/MovementController.cs 的事件接收方式
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("背包设置")]
    [SerializeField, Range(10, 100)] private int maxCapacity = 30;
    
    // 事件分发器引用（缓存以提升性能）
    private InventoryEventDispatcher inventoryEventDispatcher;
    
    // 背包数据
    private Dictionary<int, InventorySlot> inventory;
    private int currentCapacity;
    
    void Start()
    {
        // 初始化背包
        InitializeInventory();
        
        // 缓存事件分发器
        CacheEventDispatcher();
    }
    
    /// <summary>
    /// 初始化背包系统
    /// </summary>
    private void InitializeInventory()
    {
        inventory = new Dictionary<int, InventorySlot>();
        currentCapacity = 0;
    }
    
    /// <summary>
    /// 缓存事件分发器（参考MovementController的组件缓存）
    /// </summary>
    private void CacheEventDispatcher()
    {
        if (EventManager.Instance != null)
        {
            inventoryEventDispatcher = EventManager.Instance.GetInventoryDispatcher();
        }
        else
        {
            Debug.LogError("[InventoryManager] EventManager未找到！");
        }
    }
    
    /// <summary>
    /// 添加物品到背包
    /// </summary>
    /// <param name="itemData">物品数据</param>
    /// <param name="quantity">添加数量</param>
    /// <returns>是否成功添加</returns>
    public bool AddItem(ItemData itemData, int quantity)
    {
        // 检查背包空间
        if (currentCapacity >= maxCapacity)
        {
            // 触发背包已满事件
            inventoryEventDispatcher?.TriggerInventoryFull(itemData);
            return false;
        }
        
        // 添加物品逻辑...
        // （这里省略具体实现）
        
        // 更新容量
        int oldCapacity = currentCapacity;
        currentCapacity++;
        
        // 触发相关事件
        inventoryEventDispatcher?.TriggerItemObtained(itemData, quantity);
        inventoryEventDispatcher?.TriggerInventoryCapacityChanged(oldCapacity, currentCapacity);
        
        return true;
    }
    
    /// <summary>
    /// 从背包中移除物品
    /// </summary>
    /// <param name="itemData">物品数据</param>
    /// <param name="quantity">移除数量</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveItem(ItemData itemData, int quantity)
    {
        // 移除物品逻辑...
        // （这里省略具体实现）
        
        // 触发物品消耗事件
        inventoryEventDispatcher?.TriggerItemConsumed(itemData, quantity);
        
        return true;
    }
    
    /// <summary>
    /// 丢弃物品
    /// </summary>
    /// <param name="itemData">物品数据</param>
    /// <param name="quantity">丢弃数量</param>
    public void DropItem(ItemData itemData, int quantity)
    {
        Vector3 dropPosition = transform.position + Vector3.forward * 2f;
        
        // 执行丢弃逻辑...
        // （这里省略具体实现）
        
        // 触发丢弃事件
        inventoryEventDispatcher?.TriggerItemDropped(itemData, quantity, dropPosition);
    }
}
```

#### 6.2 UI系统事件监听示例

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包UI控制器 - 监听背包事件的示例
/// 参考：Assets/Script/input/MovementController.cs 的OnMoveInput事件接收方式
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Text capacityText;
    [SerializeField] private Text messageText;
    [SerializeField] private GameObject fullWarningPanel;
    
    void Start()
    {
        // 订阅背包事件（类似MovementController订阅输入事件）
        SubscribeToInventoryEvents();
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        UnsubscribeFromInventoryEvents();
    }
    
    /// <summary>
    /// 订阅背包事件
    /// </summary>
    private void SubscribeToInventoryEvents()
    {
        if (EventManager.Instance != null)
        {
            var dispatcher = EventManager.Instance.GetInventoryDispatcher();
            
            // 添加事件监听器（参考MovementController.OnMoveInput的绑定方式）
            dispatcher.onItemObtained.AddListener(OnItemObtained);
            dispatcher.onItemConsumed.AddListener(OnItemConsumed);
            dispatcher.onInventoryCapacityChanged.AddListener(OnInventoryCapacityChanged);
            dispatcher.onInventoryFull.AddListener(OnInventoryFull);
        }
    }
    
    /// <summary>
    /// 取消订阅背包事件
    /// </summary>
    private void UnsubscribeFromInventoryEvents()
    {
        if (EventManager.Instance != null)
        {
            var dispatcher = EventManager.Instance.GetInventoryDispatcher();
            
            dispatcher.onItemObtained.RemoveListener(OnItemObtained);
            dispatcher.onItemConsumed.RemoveListener(OnItemConsumed);
            dispatcher.onInventoryCapacityChanged.RemoveListener(OnInventoryCapacityChanged);
            dispatcher.onInventoryFull.RemoveListener(OnInventoryFull);
        }
    }
    
    #region 事件处理方法
    
    /// <summary>
    /// 处理物品获得事件（类似MovementController.OnMoveInput的事件处理）
    /// </summary>
    /// <param name="itemData">获得的物品</param>
    /// <param name="quantity">获得数量</param>
    private void OnItemObtained(ItemData itemData, int quantity)
    {
        // 显示获得物品的消息
        if (messageText != null)
        {
            messageText.text = $"获得 {itemData.itemName} x{quantity}";
            
            // 2秒后清除消息
            Invoke(nameof(ClearMessage), 2f);
        }
    }
    
    /// <summary>
    /// 处理物品消耗事件
    /// </summary>
    /// <param name="itemData">消耗的物品</param>
    /// <param name="quantity">消耗数量</param>
    private void OnItemConsumed(ItemData itemData, int quantity)
    {
        if (messageText != null)
        {
            messageText.text = $"消耗 {itemData.itemName} x{quantity}";
            Invoke(nameof(ClearMessage), 2f);
        }
    }
    
    /// <summary>
    /// 处理背包容量变化事件
    /// </summary>
    /// <param name="oldCapacity">原容量</param>
    /// <param name="newCapacity">新容量</param>
    private void OnInventoryCapacityChanged(int oldCapacity, int newCapacity)
    {
        if (capacityText != null)
        {
            capacityText.text = $"背包: {newCapacity}/30";
        }
    }
    
    /// <summary>
    /// 处理背包已满事件
    /// </summary>
    /// <param name="attemptedItem">尝试添加的物品</param>
    private void OnInventoryFull(ItemData attemptedItem)
    {
        // 显示背包已满警告
        if (fullWarningPanel != null)
        {
            fullWarningPanel.SetActive(true);
            
            // 3秒后隐藏警告
            Invoke(nameof(HideFullWarning), 3f);
        }
        
        if (messageText != null)
        {
            messageText.text = "背包已满！";
            Invoke(nameof(ClearMessage), 2f);
        }
    }
    
    /// <summary>
    /// 清除消息文本
    /// </summary>
    private void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
    
    /// <summary>
    /// 隐藏背包已满警告
    /// </summary>
    private void HideFullWarning()
    {
        if (fullWarningPanel != null)
        {
            fullWarningPanel.SetActive(false);
        }
    }
    
    #endregion
}
```

### 7. 数据类型定义

#### 7.1 物品数据类

```csharp
using UnityEngine;

/// <summary>
/// 物品数据类 - 用于事件系统的物品信息传递
/// </summary>
[System.Serializable]
public class ItemData
{
    [Header("基础信息")]
    public int itemId;
    public string itemName;
    public string description;
    public Sprite icon;
    
    [Header("物品属性")]
    public ItemType itemType;
    public int maxStackSize = 1;
    public float weight = 1f;
    public int value = 0;
    
    [Header("耐久度")]
    public bool hasDurability = false;
    public float maxDurability = 100f;
    public float currentDurability = 100f;
    
    [Header("使用属性")]
    public bool isConsumable = false;
    public bool isEquippable = false;
}

/// <summary>
/// 物品类型枚举
/// </summary>
public enum ItemType
{
    Food,       // 食物类
    Tool,       // 工具类
    Light,      // 光源类
    Building,   // 建筑类
    Clothing,   // 衣物类
    Weapon,     // 武器类
    Summon,     // 召唤类
    Material,   // 材料类
    Equipment   // 装备类
}

/// <summary>
/// 装备槽位枚举
/// </summary>
public enum EquipmentSlot
{
    Helmet,     // 头盔
    Armor,      // 护甲
    Weapon,     // 武器
    Accessory   // 饰品
}
```

#### 7.2 制作配方数据类

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 制作配方数据类
/// </summary>
[System.Serializable]
public class CraftingRecipe
{
    [Header("配方信息")]
    public int recipeId;
    public string recipeName;
    public string description;
    
    [Header("制作结果")]
    public ItemData resultItem;
    public int resultQuantity = 1;
    
    [Header("所需材料")]
    public List<CraftingMaterial> requiredMaterials;
    
    [Header("制作条件")]
    public bool requiresCraftingTable = false;
    public float craftingTime = 1f;
    public int requiredLevel = 1;
}

/// <summary>
/// 制作材料数据类
/// </summary>
[System.Serializable]
public class CraftingMaterial
{
    public ItemData material;
    public int quantity;
}
```

### 8. 性能优化和最佳实践

#### 8.1 性能优化建议

1. **事件缓存**：缓存事件分发器引用，避免频繁查找
2. **事件队列**：使用事件队列处理高频事件，避免帧率波动
3. **监听器管理**：及时移除不需要的事件监听器，防止内存泄漏
4. **事件合并**：对于频繁触发的事件，考虑合并处理

#### 8.2 错误处理机制

```csharp
/// <summary>
/// 安全的事件触发方法
/// </summary>
public static class EventUtils
{
    /// <summary>
    /// 安全触发事件（带异常处理）
    /// </summary>
    /// <param name="eventAction">要触发的事件</param>
    /// <param name="eventName">事件名称（用于日志）</param>
    public static void SafeInvokeEvent(System.Action eventAction, string eventName)
    {
        try
        {
            eventAction?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[EventUtils] 触发事件 {eventName} 时发生错误: {e.Message}");
        }
    }
    
    /// <summary>
    /// 安全触发带参数的事件
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="eventAction">要触发的事件</param>
    /// <param name="parameter">事件参数</param>
    /// <param name="eventName">事件名称</param>
    public static void SafeInvokeEvent<T>(System.Action<T> eventAction, T parameter, string eventName)
    {
        try
        {
            eventAction?.Invoke(parameter);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[EventUtils] 触发事件 {eventName} 时发生错误: {e.Message}");
        }
    }
}
```

#### 8.3 调试和监控工具

```csharp
/// <summary>
/// 事件调试工具
/// </summary>
[System.Serializable]
public class EventDebugger
{
    [Header("调试设置")]
    public bool enableEventCounting = true;
    public bool showDetailedLogs = false;
    
    // 事件计数器
    private Dictionary<string, int> eventCounts = new Dictionary<string, int>();
    
    /// <summary>
    /// 记录事件触发次数
    /// </summary>
    /// <param name="eventName">事件名称</param>
    public void RecordEvent(string eventName)
    {
        if (!enableEventCounting) return;
        
        if (eventCounts.ContainsKey(eventName))
        {
            eventCounts[eventName]++;
        }
        else
        {
            eventCounts[eventName] = 1;
        }
        
        if (showDetailedLogs)
        {
            Debug.Log($"[EventDebugger] {eventName} 触发次数: {eventCounts[eventName]}");
        }
    }
    
    /// <summary>
    /// 获取事件统计信息
    /// </summary>
    /// <returns>事件统计字典</returns>
    public Dictionary<string, int> GetEventStatistics()
    {
        return new Dictionary<string, int>(eventCounts);
    }
    
    /// <summary>
    /// 清除事件统计
    /// </summary>
    public void ClearStatistics()
    {
        eventCounts.Clear();
    }
}
```

### 9. 集成指导

#### 9.1 与现有系统集成

1. **与Actor系统集成**：背包事件可以影响角色状态和行为
2. **与UI系统集成**：事件驱动的UI更新机制
3. **与制作系统集成**：制作完成后触发背包事件
4. **与存档系统集成**：重要事件的持久化保存

#### 9.2 扩展指导

1. **添加新事件类型**：遵循现有的事件定义模式
2. **自定义事件分发器**：继承BaseEventDispatcher
3. **事件过滤器**：根据条件过滤特定事件
4. **事件回放系统**：记录和重放事件序列

### 10. 测试和验证

#### 10.1 单元测试示例

```csharp
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

/// <summary>
/// 背包事件系统单元测试
/// </summary>
public class InventoryEventTests
{
    private EventManager eventManager;
    private InventoryEventDispatcher inventoryDispatcher;
    
    [SetUp]
    public void Setup()
    {
        // 创建测试用的EventManager
        GameObject testObject = new GameObject("TestEventManager");
        eventManager = testObject.AddComponent<EventManager>();
        inventoryDispatcher = eventManager.GetInventoryDispatcher();
    }
    
    [TearDown]
    public void Teardown()
    {
        // 清理测试对象
        if (eventManager != null)
        {
            Object.DestroyImmediate(eventManager.gameObject);
        }
    }
    
    [Test]
    public void TestItemObtainedEvent()
    {
        // 准备测试数据
        ItemData testItem = new ItemData
        {
            itemId = 1,
            itemName = "测试物品",
            itemType = ItemType.Material
        };
        
        bool eventTriggered = false;
        ItemData receivedItem = null;
        int receivedQuantity = 0;
        
        // 添加事件监听器
        inventoryDispatcher.onItemObtained.AddListener((item, quantity) =>
        {
            eventTriggered = true;
            receivedItem = item;
            receivedQuantity = quantity;
        });
        
        // 触发事件
        inventoryDispatcher.TriggerItemObtained(testItem, 5);
        
        // 验证结果
        Assert.IsTrue(eventTriggered, "物品获得事件未触发");
        Assert.AreEqual(testItem, receivedItem, "接收到的物品数据不正确");
        Assert.AreEqual(5, receivedQuantity, "接收到的数量不正确");
    }
    
    [Test]
    public void TestInventoryFullEvent()
    {
        ItemData testItem = new ItemData
        {
            itemId = 2,
            itemName = "背包已满测试物品"
        };
        
        bool eventTriggered = false;
        
        inventoryDispatcher.onInventoryFull.AddListener((item) =>
        {
            eventTriggered = true;
        });
        
        inventoryDispatcher.TriggerInventoryFull(testItem);
        
        Assert.IsTrue(eventTriggered, "背包已满事件未触发");
    }
}
```

### 11. 部署和维护

#### 11.1 部署检查清单

- [ ] EventManager已添加到场景中
- [ ] 所有事件分发器正确初始化
- [ ] UI组件正确订阅了相关事件
- [ ] 事件日志功能可以正常关闭（发布版本）
- [ ] 性能测试通过（高频事件不影响帧率）

#### 11.2 维护建议

1. **定期检查事件监听器**：确保没有内存泄漏
2. **监控事件触发频率**：避免性能问题
3. **更新事件文档**：保持技术文档的时效性
4. **版本兼容性**：新增事件时保持向后兼容

---

## 总结

本事件框架基于Unity 3D游戏开发的最佳实践，参考了项目中现有的输入系统实现方式，提供了完整的背包系统事件处理解决方案。框架具有以下特点：

1. **类型安全**：使用强类型事件参数，避免运行时错误
2. **高性能**：组件缓存、事件队列等优化措施
3. **易扩展**：模块化设计，易于添加新的事件类型
4. **易调试**：完整的日志和监控系统
5. **易集成**：与现有项目风格保持一致

通过这个事件框架，可以实现松耦合的系统间通信，提高代码的可维护性和扩展性。 