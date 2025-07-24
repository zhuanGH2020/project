# Event事件框架 - 使用说明文档
## Unity 3D游戏开发 - 背包系统事件处理框架

### 📋 项目概述

本Event事件框架基于项目`.cursorrules`规范设计，专门用于处理Unity 3D游戏中的背包系统事件，包括道具获得、消耗、装备、制作等各种操作。框架严格遵循项目现有的UnityEvent模式和Manager架构设计。

**参考实现**：基于`Assets/Script/input/InputController.cs`的事件处理方式

---

## 🏗️ 框架架构

### 核心架构图
```
EventSystemManager (事件系统管理器)
├── InventoryEventManager (背包事件管理器) ✅ 已创建
├── ItemEventManager (物品事件管理器)
├── CraftingEventManager (制作事件管理器)
└── EquipmentEventManager (装备事件管理器)

事件类型定义：
├── InventorySystemEvents (背包系统事件) ✅ 已创建
├── ItemSystemEvents (物品系统事件)
├── CraftingSystemEvents (制作系统事件)
└── EquipmentSystemEvents (装备系统事件)
```

### 已创建的文件结构
```
Assets/Script/
├── Core/
│   └── Event/
│       └── EventSystemEnums.cs ✅ (3,489 bytes) - 核心枚举定义
├── Manager/
│   ├── InventoryEventManager.cs ✅ (25,318 bytes) - 背包事件管理器
│   └── SimpleInventoryManager.cs ✅ (5,439 bytes) - 简化背包管理器示例
└── Utils/
    ├── InventorySystemEvents.cs ✅ (5,034 bytes) - 背包系统事件定义
    ├── ItemData.cs ✅ (11,044 bytes) - 物品数据类
    └── CraftingRecipeData.cs ✅ (5,932 bytes) - 制作配方数据类
```

---

## 🎯 核心组件详解

### 1. EventSystemEnums.cs - 核心枚举定义
**位置**: `Assets/Script/Core/Event/EventSystemEnums.cs`

**功能**: 定义了事件系统使用的所有枚举类型
- ✅ `ItemType` - 物品类型（食物、工具、光源、建筑等）
- ✅ `EquipmentSlotType` - 装备槽位类型
- ✅ `EventPriority` - 事件优先级
- ✅ `EventProcessingStatus` - 事件处理状态
- ✅ `InventoryOperationType` - 背包操作类型
- ✅ `CraftingStatus` - 制作系统状态
- ✅ `ItemQuality` - 物品品质等级
- ✅ `EventLogLevel` - 事件日志级别

### 2. InventoryEventManager.cs - 背包事件管理器
**位置**: `Assets/Script/Manager/InventoryEventManager.cs`

**功能**: 背包系统的核心事件管理器
- ✅ **单例模式**: 全局唯一实例，方便访问
- ✅ **事件队列**: 高性能事件处理，避免帧率波动
- ✅ **性能监控**: 事件统计和执行时间监控
- ✅ **安全调用**: 带异常处理的事件触发机制
- ✅ **日志系统**: 可配置的多级日志输出

**主要事件方法**:
```csharp
// 物品获得
TriggerItemObtained(ItemData itemData, int quantity)

// 物品消耗
TriggerItemConsumed(ItemData itemData, int quantity)

// 物品丢弃
TriggerItemDropped(ItemData itemData, int quantity, Vector3 dropPosition)

// 背包容量变化
TriggerInventoryCapacityChanged(int oldCapacity, int newCapacity)

// 背包已满
TriggerInventoryFull(ItemData attemptedItem)

// 更多事件方法...
```

### 3. InventorySystemEvents.cs - 背包系统事件定义
**位置**: `Assets/Script/Utils/InventorySystemEvents.cs`

**功能**: 定义所有背包相关的UnityEvent类型
- ✅ `ItemObtainedEvent` - 物品获得事件
- ✅ `ItemConsumedEvent` - 物品消耗事件
- ✅ `ItemDroppedEvent` - 物品丢弃事件
- ✅ `InventoryCapacityChangedEvent` - 背包容量变化事件
- ✅ `InventoryFullEvent` - 背包已满事件
- ✅ `ItemStackChangedEvent` - 物品堆叠变化事件
- ✅ **更多15种事件类型**

### 4. ItemData.cs - 物品数据类
**位置**: `Assets/Script/Utils/ItemData.cs`

**功能**: 完整的物品数据结构
- ✅ **基础信息**: ID、名称、描述、图标
- ✅ **物品属性**: 类型、品质、重量、价值、堆叠数量
- ✅ **耐久度系统**: 最大耐久度、当前耐久度、损坏检测
- ✅ **使用属性**: 是否可消耗、可装备、可交易、可丢弃
- ✅ **实用方法**: 堆叠检测、深拷贝、数据验证

### 5. SimpleInventoryManager.cs - 简化背包管理器
**位置**: `Assets/Script/Manager/SimpleInventoryManager.cs`

**功能**: 展示事件系统集成的完整示例
- ✅ **基础背包功能**: 添加物品、移除物品、查询数量
- ✅ **事件集成**: 完整的事件触发示例
- ✅ **重量管理**: 背包重量限制和检测
- ✅ **容量管理**: 背包容量限制和检测

---

## 🚀 快速使用指南

### Step 1: 创建事件管理器
在场景中创建一个GameObject，添加`InventoryEventManager`组件：

```csharp
// 事件管理器会自动初始化为单例
// 可通过 InventoryEventManager.Instance 访问
```

### Step 2: 创建背包管理器
添加`SimpleInventoryManager`组件来管理背包逻辑：

```csharp
public class GameController : MonoBehaviour
{
    private SimpleInventoryManager inventory;
    
    void Start()
    {
        inventory = FindObjectOfType<SimpleInventoryManager>();
    }
    
    // 添加物品示例
    public void AddTestItem()
    {
        ItemData testItem = new ItemData(1, "测试物品", ItemType.Material);
        inventory.AddItem(testItem, 5);
    }
}
```

### Step 3: 监听事件
创建UI控制器监听背包事件：

```csharp
public class InventoryUI : MonoBehaviour
{
    void Start()
    {
        var eventManager = InventoryEventManager.Instance;
        
        // 监听物品获得事件
        eventManager.onItemObtained.AddListener(OnItemObtained);
        
        // 监听背包已满事件
        eventManager.onInventoryFull.AddListener(OnInventoryFull);
    }
    
    private void OnItemObtained(ItemData item, int quantity)
    {
        Debug.Log($"获得物品: {item.GetDisplayName()} x{quantity}");
        // 更新UI显示
    }
    
    private void OnInventoryFull(ItemData attemptedItem)
    {
        Debug.Log("背包已满！");
        // 显示警告UI
    }
    
    void OnDestroy()
    {
        // 重要：移除事件监听器防止内存泄漏
        if (InventoryEventManager.Instance != null)
        {
            var eventManager = InventoryEventManager.Instance;
            eventManager.onItemObtained.RemoveListener(OnItemObtained);
            eventManager.onInventoryFull.RemoveListener(OnInventoryFull);
        }
    }
}
```

---

## 📖 完整使用示例

### 创建和使用物品

```csharp
// 创建物品数据
ItemData sword = new ItemData(101, "铁剑", ItemType.Weapon, ItemQuality.Common, 1);
ItemData potion = new ItemData(201, "生命药水", ItemType.Consumable, ItemQuality.Common, 10);

// 添加到背包
SimpleInventoryManager.Instance.AddItem(sword, 1);
SimpleInventoryManager.Instance.AddItem(potion, 5);

// 检查物品
bool hasSword = SimpleInventoryManager.Instance.HasItem(101, 1);
int potionCount = SimpleInventoryManager.Instance.GetItemQuantity(201);

// 使用物品
SimpleInventoryManager.Instance.RemoveItem(potion, 1);
```

### 事件监听完整示例

```csharp
public class CompleteEventListener : MonoBehaviour
{
    private InventoryEventManager eventManager;
    
    void Start()
    {
        eventManager = InventoryEventManager.Instance;
        SubscribeToEvents();
    }
    
    private void SubscribeToEvents()
    {
        eventManager.onItemObtained.AddListener(OnItemObtained);
        eventManager.onItemConsumed.AddListener(OnItemConsumed);
        eventManager.onItemDropped.AddListener(OnItemDropped);
        eventManager.onInventoryFull.AddListener(OnInventoryFull);
        eventManager.onInventoryWeightChanged.AddListener(OnWeightChanged);
        // ... 更多事件监听
    }
    
    private void OnItemObtained(ItemData item, int quantity)
    {
        ShowMessage($"获得 {item.GetDisplayName()} x{quantity}", Color.green);
    }
    
    private void OnItemConsumed(ItemData item, int quantity)
    {
        ShowMessage($"消耗 {item.GetDisplayName()} x{quantity}", Color.yellow);
    }
    
    private void OnItemDropped(ItemData item, int quantity, Vector3 position)
    {
        ShowMessage($"丢弃 {item.GetDisplayName()} x{quantity}", Color.red);
        CreateDropEffect(position);
    }
    
    private void OnInventoryFull(ItemData attemptedItem)
    {
        ShowWarning("背包已满！");
        PlayWarningSound();
    }
    
    private void OnWeightChanged(float oldWeight, float newWeight, float maxWeight)
    {
        UpdateWeightBar(newWeight / maxWeight);
        
        if (newWeight > maxWeight * 0.9f)
        {
            ShowWarning("背包接近超重！");
        }
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void UnsubscribeFromEvents()
    {
        if (eventManager != null)
        {
            eventManager.onItemObtained.RemoveListener(OnItemObtained);
            eventManager.onItemConsumed.RemoveListener(OnItemConsumed);
            eventManager.onItemDropped.RemoveListener(OnItemDropped);
            eventManager.onInventoryFull.RemoveListener(OnInventoryFull);
            eventManager.onInventoryWeightChanged.RemoveListener(OnWeightChanged);
        }
    }
}
```

---

## ⚡ 性能特性

### 1. 高性能事件队列
- ✅ **队列处理**: 每帧最多处理5个事件，避免性能峰值
- ✅ **优先级支持**: 支持Critical、High、Normal、Low四个优先级
- ✅ **队列大小限制**: 可配置的最大队列大小（默认100）

### 2. 智能性能监控
- ✅ **执行时间监控**: 自动记录每个事件的执行时间
- ✅ **统计报告**: 提供详细的性能统计报告
- ✅ **警告机制**: 执行时间超过5ms时自动警告

### 3. 内存优化
- ✅ **组件缓存**: 缓存Transform等常用组件
- ✅ **事件复用**: 避免频繁的内存分配
- ✅ **自动清理**: 支持事件监听器的自动管理

---

## 🔧 配置选项

### InventoryEventManager配置
```csharp
[Header("背包事件设置")]
[SerializeField, Range(10, 200)] private int maxEventQueueSize = 100;  // 最大队列大小
[SerializeField] private bool enableEventLogging = true;                // 是否启用日志
[SerializeField] private EventLogLevel logLevel = EventLogLevel.Info;   // 日志级别
```

### SimpleInventoryManager配置
```csharp
[Header("背包设置")]
[SerializeField, Range(10, 50)] private int maxCapacity = 20;      // 最大容量
[SerializeField, Range(10f, 500f)] private float maxWeight = 100f; // 最大重量
```

---

## 🐛 调试和监控

### 1. 性能报告
```csharp
// 获取性能报告
string report = InventoryEventManager.Instance.GetPerformanceReport();
Debug.Log(report);

// 输出示例:
// === 背包事件管理器性能报告 ===
// 事件队列大小: 0/100
// 
// 事件触发统计:
//   ItemObtained: 15次, 总耗时: 2.34ms, 平均: 0.156ms
//   ItemConsumed: 8次, 总耗时: 1.12ms, 平均: 0.140ms
```

### 2. 调试工具
```csharp
// 清除统计数据
InventoryEventManager.Instance.ClearEventStatistics();

// 设置日志级别
InventoryEventManager.Instance.SetLogLevel(EventLogLevel.Debug);

// 启用/禁用日志
InventoryEventManager.Instance.SetEventLogging(false);
```

---

## ✅ 部署检查清单

### 必需组件
- [ ] 场景中添加了`InventoryEventManager`组件
- [ ] 背包管理器正确引用了事件管理器
- [ ] UI组件正确订阅了相关事件
- [ ] 所有事件监听器都有对应的移除代码

### 性能检查
- [ ] 事件日志在发布版本中已禁用
- [ ] 事件队列大小设置合理（不超过200）
- [ ] 没有在Update中直接触发事件
- [ ] 事件监听器及时移除，防止内存泄漏

### 功能测试
- [ ] 物品添加/移除功能正常
- [ ] 背包容量限制生效
- [ ] 背包重量限制生效
- [ ] 所有相关事件正确触发
- [ ] UI正确响应事件更新

---

## 🔮 扩展指南

### 添加新的事件类型
1. 在`InventorySystemEvents.cs`中定义新的事件类
2. 在`InventoryEventManager.cs`中添加对应的字段和触发方法
3. 在`InitializeEvents()`方法中初始化新事件

### 创建新的事件管理器
1. 继承`MonoBehaviour`并参考`InventoryEventManager`的结构
2. 定义专用的事件类型
3. 实现相同的性能监控和队列处理机制

### 集成到现有系统
1. 在现有Manager中缓存对应的事件管理器
2. 在关键操作点触发相应事件
3. 在UI系统中监听和响应事件

---

## 📝 注意事项

### ⚠️ 重要提醒
1. **内存泄漏**: 必须在`OnDestroy`中移除所有事件监听器
2. **性能影响**: 避免在单帧内触发大量事件
3. **空值检查**: 触发事件前必须验证参数有效性
4. **日志控制**: 发布版本记得禁用详细日志

### 💡 最佳实践
1. **事件命名**: 使用清晰的事件名称和完整的XML注释
2. **参数设计**: 事件参数应包含足够的上下文信息
3. **错误处理**: 在事件处理中添加try-catch保护
4. **测试覆盖**: 为关键事件编写单元测试

---

## 📚 相关文档

- **完整技术文档**: `EventSystem_TechnicalDocument.md` (2,488行)
- **项目规范**: `.cursorrules` 
- **参考实现**: `Assets/Script/input/InputController.cs`

---

**框架版本**: v1.0  
**创建日期**: 2025年1月  
**基于规范**: Unity 3D游戏开发 Cursor Rules  
**兼容版本**: Unity 2021.3.37f1

🎉 **Event事件框架已就绪，可立即投入使用！** 