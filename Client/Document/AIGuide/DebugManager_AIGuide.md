# DebugManager 调试管理器

调试管理器 - 统一管理游戏开发调试功能，提供UI调试、存档管理、物品获取等开发辅助工具。

## 简介

DebugManager是项目的核心调试管理器，遵循项目管理器架构设计，提供统一的调试功能接口。它集成了UI路径调试、时间系统控制、物品获取、存档管理等多种开发调试工具，为开发和测试提供强有力的支持。

## 核心功能

### 🔧 UI调试系统
- **UI路径打印**: 可控的UI交互路径调试输出
- **调试开关控制**: 通过事件系统同步状态变化
- **InputManager集成**: 与输入系统深度集成

### ⏰ 时间系统控制  
- **时间暂停/恢复**: 控制游戏时间流逝
- **夜晚测试**: 强制设置夜晚状态进行测试
- **时间状态查询**: 获取当前时间系统状态

### 📦 物品管理功能
- **批量材料获取**: 基于GameSettings配置获取调试材料
- **指定物品获取**: 通过物品ID获取任意物品
- **事件通知系统**: 自动发布获取成功/失败通知

### 💾 存档管理功能  
- **手动保存**: 立即保存当前游戏进度
- **存档删除**: 删除存档并重置游戏状态
- **安全检查**: 完整的存档状态验证

### 📊 进度信息系统
- **综合进度查询**: 获取时间、背包、玩家、存档等全面信息
- **格式化输出**: 结构化的调试信息显示
- **实时状态监控**: 动态获取各系统当前状态

## 详细接口

### 单例访问
```csharp
// 获取DebugManager实例
DebugManager.Instance
```

### UI调试控制
```csharp
// 设置UI路径打印开关
DebugManager.Instance.SetUIPathPrintEnabled(bool enabled);

// 获取UI路径打印状态  
bool isEnabled = DebugManager.Instance.IsUIPathPrintEnabled;
```

### 时间系统控制
```csharp
// 设置时间系统开关
DebugManager.Instance.SetTimeEnabled(bool enabled);

// 获取时间系统状态
bool isTimeEnabled = DebugManager.Instance.IsTimeEnabled;

// 强制设置夜晚时间（测试用）
DebugManager.Instance.ForceSetNightTime();
```

### 物品获取功能
```csharp
// 获取配置的调试材料  
DebugManager.Instance.GetMaterials();

// 获取指定ID的物品
DebugManager.Instance.GetCustomItem(int itemId);
```

### 存档管理功能
```csharp
// 手动保存游戏
bool saveSuccess = DebugManager.Instance.ManualSave();

// 删除当前存档并重置游戏状态
bool deleteSuccess = DebugManager.Instance.DeleteCurrentSaveAndReset();
```

### 进度信息获取
```csharp
// 获取格式化的游戏进度信息
string progressInfo = DebugManager.Instance.GetCurrentProgressInfo();
Debug.Log(progressInfo);
```

## 系统依赖关系

### 核心系统依赖
```csharp
// 事件系统 - 状态通知和用户提示
EventManager.Instance.Publish(new UIPathPrintToggleEvent(enabled));
EventManager.Instance.Publish(new NoticeEvent(message));

// 时间系统 - 时间控制
ClockModel.Instance.PauseTime();
ClockModel.Instance.ResumeTime(); 
ClockModel.Instance.SetGameTime(day, progress, timeOfDay);

// 背包系统 - 物品管理
PackageModel.Instance.AddItem(itemId, count);

// 存档系统 - 数据持久化  
SaveManager.Instance.SaveGame(slotIndex);
SaveManager.Instance.DeleteSaveData(slotIndex);
SaveManager.Instance.ClearCurrentGameData();

// 物品系统 - 配置查询
ItemManager.Instance.GetItem(itemId);

// 玩家系统 - 状态查询
Player.Instance.CurrentHealth;
Player.Instance.MaxHealth;
```

### 配置依赖
```csharp
// 调试材料配置
GameSettings.DebugMaterials; // Dictionary<int, int>

// 配置表访问
ConfigManager.Instance.GetReader("Item");
```

## 最佳实践

### 1. UI调试使用
```csharp
// 在调试界面中控制UI路径打印
private void OnUIPathToggleChanged(bool isOn)
{
    DebugManager.Instance.SetUIPathPrintEnabled(isOn);
}

// 在InputManager中检查调试状态
if (DebugManager.Instance.IsUIPathPrintEnabled)
{
    InputUtils.PrintClickedUIPath();
}
```

### 2. 存档管理最佳实践
```csharp
// 玩家死亡时的存档处理
protected override void OnDeath()
{
    // 暂停时间并保存进度
    DebugManager.Instance.SetTimeEnabled(false);
    DebugManager.Instance.ManualSave();
    
    // 显示菜单
    UIManager.Instance.Show<MenuView>(UILayer.System);
}
```

### 3. 物品获取安全实践
```csharp
// 带验证的物品获取
private void GetCustomItem(int itemId)
{
    if (itemId <= 0)
    {
        Debug.LogWarning("无效的物品ID");
        return;
    }
    
    DebugManager.Instance.GetCustomItem(itemId);
}
```

### 4. 进度信息监控
```csharp
// 定期输出游戏状态
private void PrintCurrentProgressInfo()
{
    string progressInfo = DebugManager.Instance.GetCurrentProgressInfo();
    Debug.Log(progressInfo);
}
```

## 事件系统集成

### 发布的事件
```csharp
// UI路径打印状态变化事件
public class UIPathPrintToggleEvent : IEvent
{
    public bool IsEnabled { get; }
    
    public UIPathPrintToggleEvent(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}

// 通知事件 - 用户提示
public class NoticeEvent : IEvent
{
    public string Message { get; }
    
    public NoticeEvent(string message)
    {
        Message = message;
    }
}
```

### 事件使用示例
```csharp
// 订阅UI路径打印状态变化
EventManager.Instance.Subscribe<UIPathPrintToggleEvent>(OnUIPathToggleChanged);

private void OnUIPathToggleChanged(UIPathPrintToggleEvent eventData)
{
    // 响应UI路径调试状态变化
    UpdateUIPathDebugDisplay(eventData.IsEnabled);
}
```

## 注意事项

### 功能限制
- **槽位固定**: 存档操作固定使用槽位0
- **物品验证**: GetCustomItem会验证itemId的有效性
- **状态依赖**: 时间系统控制依赖ClockModel的正确初始化

### 性能考虑
- **进度信息获取**: GetCurrentProgressInfo会遍历所有物品，建议按需调用
- **事件发布**: 每次状态改变都会发布事件，注意避免频繁调用

### 安全注意事项
- **存档安全**: 删除存档操作不可逆，使用前需确认
- **时间系统**: 强制设置时间可能影响游戏逻辑，仅用于测试
- **物品获取**: 批量获取材料可能影响游戏平衡，仅用于调试

## DebugView集成

DebugManager与DebugView紧密集成，提供完整的UI调试界面：

```csharp
// DebugView中的典型使用
public class DebugView : BaseView
{
    // UI路径调试控制
    private void OnUIPathToggleChanged(bool isOn)
    {
        DebugManager.Instance.SetUIPathPrintEnabled(isOn);
    }
    
    // 时间系统控制
    private void OnUITimeToggleChanged(bool isOn)
    {
        DebugManager.Instance.SetTimeEnabled(isOn);
    }
    
    // 获取材料
    private void GetMaterials()
    {
        DebugManager.Instance.GetMaterials();
    }
    
    // 手动保存
    private void ManualSave()
    {
        bool saveSuccess = DebugManager.Instance.ManualSave();
        if (saveSuccess)
        {
            Debug.Log("[DebugView] Manual save completed");
        }
    }
    
    // 删除存档
    private void DeleteCurrentSave()
    {
        bool deleteSuccess = DebugManager.Instance.DeleteCurrentSaveAndReset();
        if (deleteSuccess)
        {
            Debug.Log("[DebugView] Current save deleted successfully");
        }
    }
}
```

## 架构优势

### 🎯 单一职责
每个功能模块都有明确的职责划分，便于维护和扩展

### 🔗 松耦合设计  
通过事件系统与其他组件通信，降低系统间耦合度

### 🛡️ 错误处理
完整的参数验证和异常处理，提高系统稳定性

### 📈 可扩展性
统一的管理器架构，便于添加新的调试功能

*版本: 1.0 - 统一调试管理系统* 