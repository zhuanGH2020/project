# 事件框架技术文档

## 概述
Unity项目中的**精简事件管理系统**，专注于一对多通知场景，提供类型安全、松耦合的组件间通信机制。基于发布-订阅模式实现，支持事件的订阅、发布和取消订阅功能。

## 设计理念
**简洁高效** > 过度设计  
**直接调用** > 复杂事件  
**精准使用** > 滥用事件  

- **90%的交互**：View直接调用Model，简洁高效
- **10%的通知**：一对多场景使用事件系统

## 架构设计

### 核心组件
- **EventManager**: 事件管理器，负责事件的订阅、发布和管理
- **IEvent**: 事件基础接口，所有事件必须实现此接口
- **GameEvents.cs**: 项目中所有事件的定义集合

### 设计原则
- **类型安全**: 基于泛型实现，编译时检查事件类型
- **松耦合**: 发布者和订阅者无需直接引用，通过事件系统解耦
- **统一管理**: 所有事件通过EventManager统一管理
- **异常处理**: 内置异常捕获，单个处理器错误不影响其他处理器

## 核心接口

### EventManager
**类型**: 单例类
**位置**: `Assets/Scripts/Core/Event/EventManager.cs`

#### 主要方法
```csharp
// 订阅事件
public void Subscribe<T>(Action<T> handler) where T : IEvent

// 取消订阅事件  
public void Unsubscribe<T>(Action<T> handler) where T : IEvent

// 发布事件
public void Publish<T>(T eventData) where T : IEvent

// 清空所有事件订阅
public void Clear()
```

### IEvent接口
**位置**: `Assets/Scripts/Core/Event/IEvent.cs`
```csharp
/// <summary>
/// 事件基础接口 - 所有事件类型的标记接口
/// </summary>
public interface IEvent
{
}
```

## 事件分类

### 📦 数据变化事件
#### ValueChangeEvent - 通用数值变化事件
```csharp
public class ValueChangeEvent : IEvent
{
    public string Key { get; }         // 变化的数值键名
    public object OldValue { get; }    // 旧值
    public object NewValue { get; }    // 新值
}
```

#### ItemChangeEvent - 道具变化事件
```csharp
public class ItemChangeEvent : IEvent
{
    public int ItemId { get; }         // 道具ID
    public int Count { get; }          // 变化数量
    public bool IsAdd { get; }         // 是否为增加操作
}
```

#### PackageItemSelectedEvent - 背包道具选中状态变化事件
```csharp
public class PackageItemSelectedEvent : IEvent
{
    public PackageItem SelectedItem { get; }  // 选中的道具
    public bool IsSelected { get; }           // 是否选中
}
```

### ⏰ 时间系统事件
#### TimeOfDayChangeEvent - 时间段切换事件
```csharp
public class TimeOfDayChangeEvent : IEvent
{
    public TimeOfDay PreviousTime { get; }    // 前一时间段
    public TimeOfDay CurrentTime { get; }     // 当前时间段
}
```

#### DayChangeEvent - 天数变化事件
```csharp
public class DayChangeEvent : IEvent
{
    public int PreviousDay { get; }           // 前一天
    public int CurrentDay { get; }            // 当前天数
}
```

### 🔨 制作系统事件
#### MakeTypeSelectedEvent - 制作类型选择事件
```csharp
public class MakeTypeSelectedEvent : IEvent
{
    public int TypeId { get; }                // 制作类型ID
    public string TypeName { get; }           // 制作类型名称
}
```

#### MakeMenuOpenEvent - 制作菜单打开事件
```csharp
public class MakeMenuOpenEvent : IEvent
{
    public int TypeId { get; }                // 制作类型ID
}
```

#### MakeMenuCloseEvent - 制作菜单关闭事件
```csharp
public class MakeMenuCloseEvent : IEvent
{
    // 无参数的简单关闭事件
}
```

#### MakeDetailOpenEvent - 制作详情视图打开事件
```csharp
public class MakeDetailOpenEvent : IEvent
{
    public int ItemId { get; }                // 物品ID
    public Vector2 UIPosition { get; }        // UI位置
}
```

#### MakeDetailCloseEvent - 制作详情视图关闭事件 🔥**v1.1新增**
```csharp
public class MakeDetailCloseEvent : IEvent
{
    public bool WithDelay { get; }            // 是否延迟关闭
    
    public MakeDetailCloseEvent(bool withDelay = true)
}
```

### 💾 存档系统事件
#### GameSavedEvent - 游戏保存完成事件
```csharp
public class GameSavedEvent : IEvent
{
    public int Slot { get; }                  // 存档槽位
    public string SaveTime { get; }           // 保存时间
}
```

#### GameLoadedEvent - 游戏加载完成事件
```csharp
public class GameLoadedEvent : IEvent
{
    public int Slot { get; }                  // 存档槽位
    public string SaveTime { get; }           // 保存时间
}
```

#### GameSaveDeletedEvent - 存档删除事件
```csharp
public class GameSaveDeletedEvent : IEvent
{
    public int Slot { get; }                  // 删除的槽位
}
```

### 🖱️ 输入交互事件
#### ClickOutsideUIEvent - 点击非UI区域事件
```csharp
public class ClickOutsideUIEvent : IEvent
{
    public Vector3 ClickPosition { get; }     // 点击位置(世界坐标)
}
```

#### MouseHoverEvent - 鼠标悬停事件
```csharp
public class MouseHoverEvent : IEvent
{
    public GameObject HoveredObject { get; }  // 悬停的对象
    public Vector3 HoverPosition { get; }     // 悬停位置
}
```

#### MouseHoverExitEvent - 鼠标离开悬停事件
```csharp
public class MouseHoverExitEvent : IEvent
{
    // 无参数的简单事件
}
```

#### ObjectInteractionEvent - 物体交互事件
```csharp
public class ObjectInteractionEvent : IEvent
{
    public IClickable Target { get; }         // 交互目标
    public Vector3 ClickPosition { get; }     // 点击位置
}
```

### 🗺️ 地图系统事件
#### MapDataAddedEvent - 地图数据添加事件
```csharp
public class MapDataAddedEvent : IEvent
{
    public MapData MapData { get; }           // 添加的地图数据
}
```

#### MapDataRemovedEvent - 地图数据删除事件
```csharp
public class MapDataRemovedEvent : IEvent
{
    public MapData MapData { get; }           // 删除的地图数据
}
```

#### MapDataSelectedEvent - 地图数据选中状态变化事件
```csharp
public class MapDataSelectedEvent : IEvent
{
    public MapData MapData { get; }           // 地图数据
    public bool IsSelected { get; }           // 是否选中
}
```

### 🏗️ 建筑系统事件
#### BuildingPendingPlaceEvent - 建筑物待放置事件
```csharp
public class BuildingPendingPlaceEvent : IEvent
{
    public int BuildingId { get; }            // 建筑物ID
}
```

#### BuildingPlacedEvent - 建筑物放置完成事件
```csharp
public class BuildingPlacedEvent : IEvent
{
    public int BuildingId { get; }            // 建筑物ID
    public float PosX { get; }                // X坐标
    public float PosY { get; }                // Y坐标
}
```

#### BuildingPlacementModeEvent - 建筑放置模式状态变化事件
```csharp
public class BuildingPlacementModeEvent : IEvent
{
    public bool IsInPlacementMode { get; }    // 是否在放置模式
    public int BuildingId { get; }            // 建筑物ID
}
```

### 📢 通知系统事件
#### NoticeEvent - 通知显示事件
```csharp
public class NoticeEvent : IEvent
{
    public string Message { get; }            // 通知消息
}
```

## 使用指南

### 基础使用流程

#### 1. 订阅事件
```csharp
void Start()
{
    EventManager.Instance.Subscribe<ItemChangeEvent>(OnItemChanged);
    EventManager.Instance.Subscribe<DayChangeEvent>(OnDayChanged);
}

void OnDestroy()
{
    EventManager.Instance.Unsubscribe<ItemChangeEvent>(OnItemChanged);
    EventManager.Instance.Unsubscribe<DayChangeEvent>(OnDayChanged);
}
```

#### 2. 事件处理器
```csharp
private void OnItemChanged(ItemChangeEvent eventData)
{
    Debug.Log($"道具{eventData.ItemId}变化：{(eventData.IsAdd ? "+" : "-")}{eventData.Count}");
    // 更新UI显示
    UpdateInventoryUI(eventData.ItemId, eventData.Count, eventData.IsAdd);
}

private void OnDayChanged(DayChangeEvent eventData)
{
    Debug.Log($"天数从第{eventData.PreviousDay}天变为第{eventData.CurrentDay}天");
    // 触发每日逻辑
    HandleDayChanged(eventData.CurrentDay);
}
```

#### 3. 发布事件
```csharp
// 发布道具变化事件
EventManager.Instance.Publish(new ItemChangeEvent(itemId, count, true));

// 发布天数变化事件
EventManager.Instance.Publish(new DayChangeEvent(previousDay, currentDay));

// 发布通知事件
EventManager.Instance.Publish(new NoticeEvent("制作成功！"));
```

### 高级使用模式

#### 1. UI组件事件驱动
```csharp
public class MakeMenuView : BaseView
{
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<MakeMenuOpenEvent>(OnMakeMenuOpen);
        EventManager.Instance.Subscribe<MakeMenuCloseEvent>(OnMakeMenuClose);
        EventManager.Instance.Subscribe<ClickOutsideUIEvent>(OnClickOutside);
    }

    private void OnMakeMenuOpen(MakeMenuOpenEvent eventData)
    {
        ShowMakeMenu(eventData.TypeId);
    }

    private void OnClickOutside(ClickOutsideUIEvent eventData)
    {
        if (gameObject.activeInHierarchy)
        {
            CloseMakeMenu();
        }
    }
}
```

#### 2. Model层事件发布
```csharp
public class PackageModel
{
    public void AddItem(int itemId, int count)
    {
        // 业务逻辑
        // ...
        
        // 发布事件通知UI更新
        EventManager.Instance.Publish(new ItemChangeEvent(itemId, count, true));
    }
}
```

#### 3. 延迟关闭机制（制作系统）
```csharp
// 制作成功后立即关闭（无延迟）
EventManager.Instance.Publish(new MakeDetailCloseEvent(false));

// 悬停离开时延迟关闭（默认延迟）
EventManager.Instance.Publish(new MakeDetailCloseEvent());
```

## 最佳实践

### 🎯 事件系统使用原则

#### ✅ 适合用事件的场景（一对多通知）

```csharp
// 时间变化需要通知多个系统
ClockModel → DayChangeEvent → {UI, 植物系统, 建筑系统, 存档系统}

// 玩家血量变化需要通知多个UI
Player → PlayerHealthChangeEvent → {血条UI, 死亡界面, 音效系统}

// UI状态变化需要通知多个监听者
UIManager → UIShowEvent → {输入系统, 暂停系统, 音效系统}
```

#### ❌ 不适合用事件的场景（一对一直接调用）

```csharp
// ✅ 直接调用更简洁
btn_cook.onClick → CookingModel.Instance.Cook()

// ❌ 没必要的事件复杂化
btn_cook.onClick → Event → CookingModel.OnEvent()
```

### ✅ 推荐做法

#### 1. 事件命名规范
- 使用动词过去式或名词形式：`ItemChangeEvent`、`DayChangeEvent`
- 包含系统前缀：`Make`、`Package`、`Building`等
- 明确表达事件含义：`OpenEvent`、`CloseEvent`、`SelectedEvent`

#### 2. 事件数据设计
```csharp
// ✅ 携带必要的上下文信息
public class ItemChangeEvent : IEvent
{
    public int ItemId { get; }           // 明确的数据
    public int Count { get; }            // 具体的变化量
    public bool IsAdd { get; }           // 操作类型
}

// ✅ 提供前后状态对比
public class TimeOfDayChangeEvent : IEvent
{
    public TimeOfDay PreviousTime { get; } // 前一状态
    public TimeOfDay CurrentTime { get; }  // 当前状态
}
```

#### 3. 异常安全的订阅管理
```csharp
public class ExampleView : MonoBehaviour
{
    void Start()
    {
        // 订阅事件
        SubscribeEvents();
    }

    void OnDestroy()
    {
        // 确保取消订阅，避免内存泄漏
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<ItemChangeEvent>(OnItemChanged);
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<ItemChangeEvent>(OnItemChanged);
    }
}
```

#### 4. 空值检查和防御性编程
```csharp
private void OnMakeDetailOpen(MakeDetailOpenEvent eventData)
{
    if (eventData == null || eventData.ItemId <= 0)
    {
        return; // 防御性检查
    }
    
    ShowMakeDetail(eventData.ItemId, eventData.UIPosition);
}
```

### ❌ 避免的做法

#### 1. 过度使用事件系统
```csharp
// ❌ 一对一交互不需要事件
private void OnButtonClick()
{
    EventManager.Instance.Publish(new ButtonClickEvent());
}

// ✅ 直接调用更简洁
private void OnButtonClick()
{
    SomeModel.Instance.DoSomething();
}
```

#### 2. 循环事件引用
```csharp
// ❌ 避免在事件处理器中发布相同类型的事件
private void OnItemChanged(ItemChangeEvent eventData)
{
    // 这可能导致无限循环
    EventManager.Instance.Publish(new ItemChangeEvent(...));
}
```

#### 3. 忘记取消订阅
```csharp
// ❌ 没有在OnDestroy中取消订阅
void Start()
{
    EventManager.Instance.Subscribe<ItemChangeEvent>(OnItemChanged);
    // 忘记在OnDestroy中取消订阅 -> 内存泄漏
}
```

#### 4. 事件处理器中的重逻辑
```csharp
// ❌ 在事件处理器中执行耗时操作
private void OnDayChanged(DayChangeEvent eventData)
{
    // 避免在这里执行复杂计算或IO操作
    for (int i = 0; i < 10000; i++)
    {
        // 复杂计算...
    }
}
```

## 系统优势

### 🎯 架构优势
1. **解耦设计**: 组件间无需直接引用，降低代码耦合度
2. **类型安全**: 编译时检查，减少运行时错误
3. **统一管理**: 所有事件通过EventManager集中管理
4. **扩展性强**: 新增事件类型无需修改现有代码

### 🛡️ 可靠性优势
5. **异常隔离**: 单个处理器异常不影响其他处理器
6. **内存安全**: 自动垃圾回收，避免内存泄漏风险
7. **调试友好**: 事件流向清晰，便于问题定位

### ⚡ 性能优势
8. **延迟执行**: 支持异步事件处理
9. **批量处理**: 可以累积事件批量处理
10. **订阅管理**: 智能的订阅者列表管理

## 与其他系统集成

### InputManager集成
```csharp
// InputManager发布点击外部UI事件
EventManager.Instance.Publish(new ClickOutsideUIEvent(mouseWorldPos));

// UI组件订阅并响应
EventManager.Instance.Subscribe<ClickOutsideUIEvent>(OnClickOutsideUI);
```

### SaveSystem集成
```csharp
// SaveModel发布保存完成事件
EventManager.Instance.Publish(new GameSavedEvent(slot, saveTime));

// SaveView订阅天数变化实现每日自动保存
EventManager.Instance.Subscribe<DayChangeEvent>(OnDayChanged);
```

### MakeSystem集成
```csharp
// 制作详情关闭支持延迟控制
EventManager.Instance.Publish(new MakeDetailCloseEvent(false)); // 立即关闭
EventManager.Instance.Publish(new MakeDetailCloseEvent(true));  // 延迟关闭
```

## 当前项目架构

### 简化版MV模式 + 直接调用

```
View (直接调用) → Model
  ↓                ↓
各种Manager ← ← ← 各种Manager
  ↓
真实业务逻辑
```

### 具体使用方式

```csharp
// ✅ View开发：直接调用Manager和Model
public class CookingView : BaseView
{
    void Start()
    {
        // 直接调用Manager获取服务
        var sprite = LoadResource<Sprite>("icon");
        var config = GetConfig("Recipe");
        
        // 订阅事件（仅在需要时）
        SubscribeEvent<DayChangeEvent>(OnDayChanged);
    }
    
    private void OnCookButtonClick()
    {
        // 直接调用Model业务逻辑
        CookingModel.Instance.Cook();
    }
    
    private void OnDestroy()
    {
        // 手动清理事件订阅
        UnsubscribeEvent<DayChangeEvent>(OnDayChanged);
    }
}
```

## 版本历史
- **v1.0**: 基础事件系统实现
- **v1.1**: 新增MakeDetailCloseEvent延迟控制机制
- **v1.2**: 完善建筑系统事件支持
- **v2.0**: 架构简化，专注于一对多通知场景 🔥

---

创建日期：2024-12-19  
更新日期：2024-12-19  
版本：2.0.0
