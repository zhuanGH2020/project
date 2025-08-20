# 存档系统技术文档

## 简介
基于PlayerPrefs的本地游戏存档系统，支持多槽位存档、自动保存和事件通知，用于持久化时间系统、背包系统、玩家数据和建筑系统等游戏进度信息。统一集成在SaveModel中，遵循项目Model层设计规范。

## 详细接口

### SaveModel（存档模型）
**类型**：Model层单例类，整合完整存档功能
**位置**：`Assets/Scripts/Core/Save/SaveModel.cs`

#### 初始化和生命周期
- `Initialize()` - 初始化存档模型（GameMain中自动调用）
- `Update()` - 更新自动保存逻辑（GameMain中自动调用）
- `Cleanup()` - 清理资源（GameMain中自动调用）
- `OnApplicationPause(bool pauseStatus)` - 应用暂停处理

#### 核心存档方法
- `SaveGame(int slot = 0)` - 保存游戏到指定槽位
- `LoadGame(int slot = 0)` - 从指定槽位加载游戏  
- `HasSaveData(int slot = 0)` - 检查指定槽位是否有存档
- `GetSaveInfo(int slot = 0)` - 获取存档信息（不加载游戏）
- `GetAllSaveInfo()` - 获取所有存档槽位信息
- `DeleteSaveData(int slot = 0)` - 删除指定槽位存档
- `ClearAllSaveData()` - 清空所有存档数据

#### 便捷方法
- `QuickSave()` - 快速保存到槽位0
- `QuickLoad()` - 快速从槽位0加载
- `SaveToSlot(int slot)` - 保存到指定槽位
- `LoadFromSlot(int slot)` - 从指定槽位加载
- `DeleteSlot(int slot)` - 删除指定槽位

#### 配置方法
- `SetAutoSave(bool enabled, float intervalSeconds = 300f)` - 设置自动保存
- `SetSaveConfig(bool autoLoadOnStart, bool autoSaveOnQuit, float autoSaveInterval)` - 设置存档配置

### SaveView（存档UI视图）
**类型**：MonoBehaviour组件，需要挂载到GameObject上使用
**位置**：`Assets/Scripts/Core/Save/SaveView.cs`

#### UI结构要求
```
SaveView GameObject
├── btn_print (Button组件) - 打印当前进度信息
├── btn_revert (Button组件) - 删除当前存档
└── btn_save (Button组件) - 手动保存进度
```

#### 核心功能
- **打印进度信息**：显示时间系统、背包系统、玩家数据、建筑系统和存档信息
- **删除存档**：删除槽位0的存档数据
- **手动保存**：立即保存当前游戏状态到槽位0
- **每日自动保存**：监听天数变化事件，实现每天自动保存一次

#### 特性
- 纯代码驱动，无需Inspector配置
- 自动查找子物体按钮组件
- 智能防重复：手动保存后当天不再自动保存

### 数据结构类

#### SaveData 🔥**v1.2更新**
存档数据主体，包含：

**时间系统**
- `clockDay` - 当前天数
- `dayProgress` - 当天进度(0.0-1.0)
- `currentTimeOfDay` - 当前时间段

**背包系统**
- `packageItems` - 背包道具列表

**玩家数据**
- `currentHealth` - 当前血量
- `playerPosition` - 玩家位置
- `equippedItems` - 已装备道具ID列表

**建筑系统** 🔥**v1.2新增**
- `buildingData` - 建筑物数据列表

**存档信息**
- `saveTime` - 存档时间
- `saveVersion` - 存档版本号

#### SaveInfo 🔥**v1.2更新**
存档信息类，用于UI显示：
- `slot` - 存档槽位
- `saveTime` - 存档时间
- `saveVersion` - 存档版本
- `clockDay` - 游戏天数
- `playerHealth` - 玩家血量
- `itemCount` - 道具数量
- `buildingCount` - 建筑数量 🔥**v1.2新增**
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
SaveModel.Instance.SaveGame(0);

// 加载游戏  
SaveModel.Instance.LoadGame(0);

// 检查是否有存档
if (SaveModel.Instance.HasSaveData(0))
{
    // 执行加载逻辑
}

// 便捷方法
SaveModel.Instance.QuickSave();    // 快速保存到槽位0
SaveModel.Instance.QuickLoad();    // 快速从槽位0加载
```

### 系统初始化
```csharp
// 在GameMain中统一初始化（已自动处理）
void Start()
{
    // 其他系统初始化...
    var clockModel = ClockModel.Instance;
    var packageModel = PackageModel.Instance;
    var mapModel = MapModel.Instance; // 建筑系统
    
    // 初始化存档模型
    var saveModel = SaveModel.Instance;
    saveModel.Initialize();
}

// 可选：自定义配置
void Start()
{
    var saveModel = SaveModel.Instance;
    saveModel.SetSaveConfig(autoLoadOnStart: true, autoSaveOnQuit: true, autoSaveInterval: 180f); // 3分钟
    saveModel.Initialize();
}
```

### 获取存档信息
```csharp
// 获取单个存档信息
SaveInfo info = SaveModel.Instance.GetSaveInfo(0);
if (info != null && !info.IsEmpty)
{
    Debug.Log($"存档：{info.GetDisplayText()}");
    Debug.Log($"建筑数量：{info.buildingCount}"); // v1.2新增
}

// 获取所有存档信息
var allSaves = SaveModel.Instance.GetAllSaveInfo();
foreach (var save in allSaves)
{
    if (save != null && !save.IsEmpty)
    {
        // 显示存档信息，包括建筑数量
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
    SaveModel.Instance.SetAutoSave(true, 120f);
}

void Update()
{
    // 必须调用Update来驱动自动保存
    SaveModel.Instance.Update();
}
```

### 使用SaveView组件
```csharp
// 在UI中创建SaveView GameObject，添加SaveView脚本
// 创建三个子物体Button：btn_print、btn_revert、btn_save
// SaveView会自动查找并绑定按钮事件

// SaveView提供的功能：
// - btn_print: 在控制台打印详细的游戏进度信息（包括建筑数据）
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
- `SaveView` 需要挂载到UI GameObject上使用
- `SaveModel` 是单例类，无需挂载
- `SaveView`可以有多个实例，但通常一个即可满足需求

### 数据持久化范围 🔥**v1.2更新**
当前系统支持保存：
- **时间系统**：天数、进度、时间段
- **背包系统**：所有道具ID和数量
- **玩家数据**：血量、位置、装备
- **建筑系统**：所有已放置的建筑物数据 🔥**v1.2新增**

### 性能考虑
- 使用PlayerPrefs存储，数据量适中时性能良好
- 自动保存间隔建议不低于60秒
- 存档数据使用JSON序列化，支持调试
- 建筑数据增加了存档大小，建议监控性能

### 扩展方法
系统为现有类添加了以下方法：
- `ClockModel.SetGameTime()` - 设置游戏时间
- `PackageModel.ClearAllItems()` - 清空背包
- `DamageableObject.SetHealth()` - 设置血量
- `MapModel.LoadBuildingData()` - 加载建筑数据 🔥**v1.2新增**

### 新增事件
系统为时间系统添加了新事件：
- `DayChangeEvent` - 天数变化时触发，包含前一天和当前天数信息
- 该事件在`ClockModel.UpdateTimeProgress()`中发布
- 用于触发每日自动保存和其他周期性游戏逻辑

## 建筑系统集成 🔥**v1.2新增**

### 建筑数据保存
```csharp
// 建筑数据自动保存
// SaveModel会自动收集MapModel中的所有建筑数据
var mapModel = MapModel.Instance;
saveData.buildingData.Clear();
foreach (var mapData in mapModel.MapDataList)
{
    saveData.buildingData.Add(mapData);
}
```

### 建筑数据加载
```csharp
// 建筑数据自动加载
// SaveModel会自动将建筑数据恢复到MapModel
var mapModel = MapModel.Instance;
mapModel.ClearMapData();
foreach (var building in saveData.buildingData)
{
    mapModel.AddMapDataDirect(building);
}
```

### 建筑存档信息
```csharp
// SaveInfo中新增建筑数量统计
SaveInfo info = SaveModel.Instance.GetSaveInfo(0);
Debug.Log($"存档中包含 {info.buildingCount} 个建筑物");
```

## 其他需要补充的关键点

### 存档版本兼容
- 系统支持存档版本控制
- 加载时会检查版本兼容性
- 支持向前兼容，建议定期更新版本号
- v1.2版本新增建筑数据支持

### 错误处理
- 所有存档操作都有完整的异常处理
- 失败时会输出详细的错误日志
- 支持存档数据损坏时的恢复机制
- 建筑数据加载失败时的回退机制

### 多平台支持
- 基于Unity PlayerPrefs，天然支持跨平台
- Windows: 注册表存储
- Mac: Plist文件存储  
- 移动平台: 系统偏好设置存储

## 版本历史
- **v1.0**: 基础存档系统实现
- **v1.1**: 完善自动保存和事件系统
- **v1.2**: 新增建筑系统数据支持 🔥

---

创建日期：2024-12-19  
更新日期：2024-12-19  
版本：1.2.0 