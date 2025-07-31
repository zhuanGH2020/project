# 存档系统技术文档

## 简介
基于PlayerPrefs的本地游戏存档系统，支持多槽位存档、自动保存和事件通知，用于持久化时间系统、背包系统和玩家数据等游戏进度信息。

## 详细接口

### SaveManager（存档管理器）
**类型**：纯数据类/工具类，使用单例模式
**位置**：`Assets/Scripts/Core/Save/SaveManager.cs`

#### 核心方法
- `SaveGame(int slot = 0)` - 保存游戏到指定槽位
- `LoadGame(int slot = 0)` - 从指定槽位加载游戏  
- `HasSaveData(int slot = 0)` - 检查指定槽位是否有存档
- `GetSaveInfo(int slot = 0)` - 获取存档信息（不加载游戏）
- `GetAllSaveInfo()` - 获取所有存档槽位信息
- `DeleteSaveData(int slot = 0)` - 删除指定槽位存档
- `ClearAllSaveData()` - 清空所有存档数据
- `SetAutoSave(bool enabled, float intervalSeconds = 300f)` - 设置自动保存
- `Update()` - 更新自动保存逻辑，需要外部调用

### GameSaveController（存档控制器）
**类型**：MonoBehaviour组件，需要挂载到GameObject上使用
**位置**：`Assets/Scripts/Manager/GameSaveController.cs`

#### 配置选项
- `Auto Load On Start` - 游戏启动时自动加载
- `Auto Save On Quit` - 应用暂停/失焦时自动保存
- `Auto Save Interval` - 自动保存间隔（60-600秒）
- `Quick Save Key` - 快速保存按键（默认F5）
- `Quick Load Key` - 快速加载按键（默认F9）
- `Show Save Info Key` - 显示存档信息按键（默认F1）

#### 公共方法
- `QuickSave()` - 快速保存到槽位0
- `QuickLoad()` - 快速从槽位0加载
- `SaveToSlot(int slot)` - 保存到指定槽位
- `LoadFromSlot(int slot)` - 从指定槽位加载
- `DeleteSlot(int slot)` - 删除指定槽位存档

### SaveView（存档UI视图）
**类型**：MonoBehaviour组件，需要挂载到GameObject上使用
**位置**：`Assets/Scripts/UI/UIDlg/SaveView.cs`

#### UI结构要求
```
SaveView GameObject
├── btn_print (Button组件) - 打印当前进度信息
├── btn_revert (Button组件) - 删除当前存档
└── btn_save (Button组件) - 手动保存进度
```

#### 核心功能
- **打印进度信息**：显示时间系统、背包系统、玩家数据和存档信息
- **删除存档**：删除槽位0的存档数据
- **手动保存**：立即保存当前游戏状态到槽位0
- **每日自动保存**：监听天数变化事件，实现每天自动保存一次

#### 特性
- 纯代码驱动，无需Inspector配置
- 自动查找子物体按钮组件
- 智能防重复：手动保存后当天不再自动保存

### 数据结构类

#### GameSaveData
存档数据主体，包含：
- `clockDay` - 当前天数
- `dayProgress` - 当天进度(0.0-1.0)
- `currentTimeOfDay` - 当前时间段
- `packageItems` - 背包道具列表
- `currentHealth` - 当前血量
- `playerPosition` - 玩家位置
- `equippedItems` - 已装备道具ID列表
- `saveTime` - 存档时间
- `saveVersion` - 存档版本号

#### SaveInfo
存档信息类，用于UI显示：
- `slot` - 存档槽位
- `saveTime` - 存档时间
- `clockDay` - 游戏天数
- `playerHealth` - 玩家血量
- `itemCount` - 道具数量
- `IsEmpty` - 是否为空槽位
- `GetDisplayText()` - 获取显示文本

### 事件系统
- `GameSavedEvent` - 游戏保存完成事件
- `GameLoadedEvent` - 游戏加载完成事件  
- `GameSaveDeletedEvent` - 存档删除事件
- `DayChangeEvent` - 天数变化事件（触发每日自动保存）

## 最佳实践

### 基础使用
```csharp
// 保存游戏
SaveManager.Instance.SaveGame(0);

// 加载游戏
SaveManager.Instance.LoadGame(0);

// 检查是否有存档
if (SaveManager.Instance.HasSaveData(0))
{
    // 执行加载逻辑
}
```

### 使用MonoBehaviour组件
```csharp
// 在场景中创建空GameObject，挂载GameSaveController组件
// 配置Inspector中的参数：
// - 启用自动加载和自动保存
// - 设置合适的自动保存间隔
// - 配置快捷键

// 通过代码调用存档功能
GetComponent<GameSaveController>().SaveToSlot(1);
GetComponent<GameSaveController>().LoadFromSlot(1);
```

### 获取存档信息
```csharp
// 获取单个存档信息
SaveInfo info = SaveManager.Instance.GetSaveInfo(0);
if (info != null && !info.IsEmpty)
{
    Debug.Log($"存档：{info.GetDisplayText()}");
}

// 获取所有存档信息
var allSaves = SaveManager.Instance.GetAllSaveInfo();
foreach (var save in allSaves)
{
    if (save != null && !save.IsEmpty)
    {
        // 显示存档信息
    }
}
```

### 订阅存档事件
```csharp
void Start()
{
    EventManager.Instance.Subscribe<GameSavedEvent>(OnGameSaved);
    EventManager.Instance.Subscribe<GameLoadedEvent>(OnGameLoaded);
}

private void OnGameSaved(GameSavedEvent eventData)
{
    Debug.Log($"游戏已保存到槽位 {eventData.Slot}");
    // 显示保存成功UI提示
}

private void OnGameLoaded(GameLoadedEvent eventData)
{
    Debug.Log($"游戏已从槽位 {eventData.Slot} 加载");
    // 显示加载成功UI提示
}
```

### 自定义自动保存
```csharp
void Start()
{
    // 设置每2分钟自动保存一次
    SaveManager.Instance.SetAutoSave(true, 120f);
}

void Update()
{
    // 必须调用Update来驱动自动保存
    SaveManager.Instance.Update();
}
```

### 使用SaveView组件
```csharp
// 在UI中创建SaveView GameObject，添加SaveView脚本
// 创建三个子物体Button：btn_print、btn_revert、btn_save
// SaveView会自动查找并绑定按钮事件

// SaveView提供的功能：
// - btn_print: 在控制台打印详细的游戏进度信息
// - btn_revert: 删除当前存档数据
// - btn_save: 手动保存当前游戏状态
// - 自动功能: 每天变化时自动保存一次
```

### 订阅天数变化事件
```csharp
void Start()
{
    EventManager.Instance.Subscribe<DayChangeEvent>(OnDayChanged);
}

private void OnDayChanged(DayChangeEvent eventData)
{
    Debug.Log($"天数从第{eventData.PreviousDay}天变为第{eventData.CurrentDay}天");
    // 可在此处添加其他每日重置逻辑
}
```

## 注意事项

### 组件挂载要求
- `GameSaveController` 需要挂载到GameObject上使用
- `SaveView` 需要挂载到UI GameObject上使用
- `SaveManager` 是纯工具类，无需挂载
- 确保在场景中只有一个`GameSaveController`实例
- `SaveView`可以有多个实例，但通常一个即可满足需求

### 数据持久化范围
当前系统支持保存：
- 时间系统：天数、进度、时间段
- 背包系统：所有道具ID和数量
- 玩家数据：血量、位置、装备

### 性能考虑
- 使用PlayerPrefs存储，数据量适中时性能良好
- 自动保存间隔建议不低于60秒
- 存档数据使用JSON序列化，支持调试

### 扩展方法
系统为现有类添加了以下方法：
- `ClockModel.SetGameTime()` - 设置游戏时间
- `PackageModel.ClearAllItems()` - 清空背包
- `DamageableObject.SetHealth()` - 设置血量

### 新增事件
系统为时间系统添加了新事件：
- `DayChangeEvent` - 天数变化时触发，包含前一天和当前天数信息
- 该事件在`ClockModel.UpdateTimeProgress()`中发布
- 用于触发每日自动保存和其他周期性游戏逻辑

## 其他需要补充的关键点

### 存档版本兼容
- 系统支持存档版本控制
- 加载时会检查版本兼容性
- 支持向前兼容，建议定期更新版本号

### 错误处理
- 所有存档操作都有完整的异常处理
- 失败时会输出详细的错误日志
- 支持存档数据损坏时的恢复机制

### 多平台支持
- 基于Unity PlayerPrefs，天然支持跨平台
- Windows: 注册表存储
- Mac: Plist文件存储  
- 移动平台: 系统偏好设置存储

创建日期：2024-12-19
更新日期：2024-12-19
版本：1.1.0 