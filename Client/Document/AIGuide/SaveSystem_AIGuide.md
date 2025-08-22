# 存档系统技术文档

## 简介
基于PlayerPrefs的本地游戏存档系统，支持多槽位存档、自动保存和事件通知，用于持久化时间系统、背包系统、玩家数据和建筑系统等游戏进度信息。统一集成在SaveManager中，遵循项目Manager层设计规范。

## 详细接口

### SaveManager（存档管理器）
**类型**：Manager层单例类，整合完整存档功能
**位置**：`Assets/Scripts/Core/Save/SaveManager.cs`

#### 初始化和生命周期
- `Initialize()` - 初始化存档管理器（GameMain中自动调用）
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
- `PrintCurrentProgress()` - 打印当前游戏进度信息到控制台
- `DeleteCurrentSaveAndReset()` - 删除当前存档并重置游戏状态
- `ManualSave()` - 手动保存当前游戏进度

## 最佳实践

### SaveManager 使用示例

#### 基础存档操作
```csharp
// 保存游戏到默认槽位（槽位0）
bool saveSuccess = SaveManager.Instance.SaveGame();

// 从默认槽位加载游戏
bool loadSuccess = SaveManager.Instance.LoadGame();

// 检查是否有存档数据
bool hasData = SaveManager.Instance.HasSaveData(0);

// 获取存档信息
SaveInfo info = SaveManager.Instance.GetSaveInfo(0);
if (info != null)
{
    Debug.Log($"存档时间: {info.saveTime}, 第{info.clockDay}天");
}
```

#### 多槽位存档管理
```csharp
// 保存到不同槽位
SaveManager.Instance.SaveToSlot(1);
SaveManager.Instance.SaveToSlot(2);

// 获取所有存档信息
List<SaveInfo> allSaves = SaveManager.Instance.GetAllSaveInfo();
for (int i = 0; i < allSaves.Count; i++)
{
    if (allSaves[i] != null && !allSaves[i].IsEmpty)
    {
        Debug.Log($"槽位{i}: {allSaves[i].GetDisplayText()}");
    }
}

// 删除指定槽位
SaveManager.Instance.DeleteSlot(2);
```

#### 自动保存配置
```csharp
// 启用自动保存，每5分钟保存一次
SaveManager.Instance.SetAutoSave(true, 300f);

// 设置完整的存档配置
SaveManager.Instance.SetSaveConfig(
    autoLoadOnStart: true,    // 启动时自动加载
    autoSaveOnQuit: true,     // 退出时自动保存
    autoSaveInterval: 300f    // 自动保存间隔5分钟
);
```

#### 高级操作
```csharp
// 清空当前游戏数据（重置到初始状态）
SaveManager.Instance.ClearCurrentGameData();

// 清空所有存档文件
SaveManager.Instance.ClearAllSaveData();
```

### SaveView 使用示例

#### Prefab设置
```csharp
// 在Prefab中设置必要的UI组件：
// 1. 创建SaveView预制体
// 2. 添加三个Button子对象：btn_print、btn_revert、btn_save
// 3. 将SaveView脚本挂载到根GameObject上
// 4. SaveView会在Awake中自动绑定按钮事件
```

#### 手动调用SaveView功能
```csharp
// 获取SaveView实例并调用功能
SaveView saveView = FindObjectOfType<SaveView>();
if (saveView != null)
{
    // 打印当前进度
    saveView.PrintCurrentProgress();
    
    // 手动保存
    saveView.ManualSave();
    
    // 删除存档并重置
    saveView.DeleteCurrentSaveAndReset();
}
```

### SaveData 数据结构

#### 核心数据字段
```csharp
public class SaveData
{
    // 时间系统数据
    public int clockDay;                    // 当前天数
    public float dayProgress;               // 当天进度
    public TimeOfDay currentTimeOfDay;      // 当前时间段

    // 玩家状态数据
    public float currentHealth;             // 当前血量
    public float currentHunger;             // 当前饥饿度
    public float currentSanity;             // 当前理智值
    public Vector3 playerPosition;          // 玩家位置

    // 背包数据
    public List<PackageItem> packageItems;  // 背包物品列表

    // 装备数据
    public List<int> equippedItems;         // 已装备物品ID列表

    // 建筑数据
    public List<MapData> buildingData;      // 建筑数据列表

    // 存档元信息
    public string saveTime;                 // 存档时间
    public int saveVersion;                 // 存档版本
}
```

#### 存档信息结构
```csharp
public class SaveInfo
{
    public int slot;                // 存档槽位
    public string saveTime;         // 存档时间
    public int saveVersion;         // 存档版本
    public int clockDay;            // 游戏天数
    public float playerHealth;      // 玩家血量
    public int itemCount;           // 背包物品数量
    public int buildingCount;       // 建筑数量
    
    public bool IsEmpty { get; }    // 是否为空存档
    public string GetDisplayText()  // 获取显示文本
}
```

### 事件系统集成

#### 存档相关事件
```csharp
// 订阅存档事件
EventManager.Instance.Subscribe<GameSavedEvent>(OnGameSaved);
EventManager.Instance.Subscribe<GameLoadedEvent>(OnGameLoaded);
EventManager.Instance.Subscribe<GameSaveDeletedEvent>(OnGameSaveDeleted);

private void OnGameSaved(GameSavedEvent eventData)
{
    Debug.Log($"游戏已保存到槽位 {eventData.Slot}，时间：{eventData.SaveTime}");
    // 显示保存成功UI提示
}

private void OnGameLoaded(GameLoadedEvent eventData)
{
    Debug.Log($"游戏已从槽位 {eventData.Slot} 加载，时间：{eventData.SaveTime}");
    // 更新UI显示
}

private void OnGameSaveDeleted(GameSaveDeletedEvent eventData)
{
    Debug.Log($"槽位 {eventData.Slot} 的存档已删除");
    // 更新存档列表UI
}
```

### 与其他系统集成

#### 时间系统集成
```csharp
// SaveManager自动保存和恢复时间数据
// 时间数据通过 ClockModel.Instance 获取和设置
var clockModel = ClockModel.Instance;
saveData.clockDay = clockModel.ClockDay;
saveData.dayProgress = clockModel.DayProgress;
saveData.currentTimeOfDay = clockModel.CurrentTimeOfDay;
```

#### 背包系统集成
```csharp
// SaveManager自动保存和恢复背包数据
// 背包数据通过 PackageModel.Instance 获取和设置
var packageModel = PackageModel.Instance;
foreach (var item in packageModel.GetAllItems())
{
    saveData.packageItems.Add(item);
}
```

#### 装备系统集成
```csharp
// SaveManager通过EquipManager管理装备数据
// 保存时获取已装备物品
var equippedItemIds = EquipManager.Instance.GetAllEquippedItemIds();
foreach (var kvp in equippedItemIds)
{
    if (kvp.Value > 0)
        saveData.equippedItems.Add(kvp.Value);
}

// 加载时恢复装备状态
EquipManager.Instance.LoadEquippedItemsFromSave(saveData.equippedItems);
```

#### 建筑系统集成
```csharp
// SaveManager通过MapModel管理建筑数据
// 保存建筑数据
var mapModel = MapModel.Instance;
foreach (var mapData in mapModel.MapDataList)
{
    saveData.buildingData.Add(mapData);
}

// 加载建筑数据
mapModel.LoadBuildingsFromSave(saveData.buildingData);
```

## 注意事项

### 存档安全性
- 存档基于Unity PlayerPrefs，数据存储在本地注册表或配置文件中
- 存档数据采用JSON序列化，便于调试但不加密
- 支持版本兼容性检查，避免新旧版本冲突
- 建议在发布版本中添加存档数据校验

### 性能注意事项
- 存档操作是同步的，大量数据可能造成短暂卡顿
- 自动保存默认5分钟间隔，避免频繁保存影响性能
- `GetSaveInfo()`方法会解析存档数据，频繁调用需注意性能

### 多平台兼容性
- PlayerPrefs在不同平台有不同的存储位置
- Windows: 注册表 HKEY_CURRENT_USER\Software\[companyname]\[productname]
- Mac: ~/Library/Preferences/unity.[companyname].[productname].plist
- 跨平台存档需要额外处理

### 调试和测试
- 使用 `ClearAllSaveData()` 清空测试存档
- 使用 `GetAllSaveInfo()` 查看所有存档状态
- SaveView提供了完整的调试界面，便于测试存档功能
- 存档操作会发布事件，便于UI响应和日志记录 