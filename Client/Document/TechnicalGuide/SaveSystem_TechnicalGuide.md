**创建日期**: 2024年12月
**版本**: 1.0
**文档类型**: 技术实现指导文档

# 存档系统技术文档

## 简介
存档系统是一个基于PlayerPrefs的本地游戏进度持久化系统，支持多槽位存档、自动保存机制和完整的事件通知。系统设计遵循项目架构规范，包含数据管理层、控制层和UI交互层，实现时间系统、背包系统和玩家数据的完整保存与加载。

## 核心设计要求

### 架构分层
```
存档系统架构
├── SaveManager (数据管理层) - 单例模式，纯工具类
├── GameSaveController (控制层) - MonoBehaviour，快捷键和生命周期管理
├── SaveView (UI交互层) - MonoBehaviour，按钮操作和每日自动保存
└── 数据结构层 - GameSaveData, SaveInfo, SavePackageItem
```

### 存储方案
- **主要存储**: Unity PlayerPrefs (跨平台兼容)
- **数据格式**: JSON序列化 (便于调试和扩展)
- **槽位管理**: 支持5个独立存档槽位 (0-4)
- **版本控制**: 存档版本号机制，支持向前兼容

## 详细接口

### SaveManager (核心管理器)
```csharp
// 单例访问
public static SaveManager Instance { get; }

// 核心存档操作
public bool SaveGame(int slot = 0)           // 保存到指定槽位
public bool LoadGame(int slot = 0)           // 从指定槽位加载
public bool HasSaveData(int slot = 0)        // 检查是否有存档
public bool DeleteSaveData(int slot = 0)     // 删除指定槽位
public void ClearAllSaveData()               // 清空所有存档

// 存档信息查询
public SaveInfo GetSaveInfo(int slot = 0)    // 获取存档信息(不加载)
public List<SaveInfo> GetAllSaveInfo()       // 获取所有槽位信息

// 自动保存管理
public void SetAutoSave(bool enabled, float intervalSeconds = 300f)
public void Update()                         // 驱动自动保存逻辑
```

### GameSaveData (存档数据结构)
```csharp
[System.Serializable]
public class GameSaveData
{
    // 时间系统数据
    public int clockDay = 1;
    public float dayProgress = 0f;
    public TimeOfDay currentTimeOfDay = TimeOfDay.Day;
    
    // 背包系统数据
    public List<SavePackageItem> packageItems = new List<SavePackageItem>();
    
    // 玩家数据
    public float currentHealth = 100f;
    public Vector3 playerPosition = Vector3.zero;
    public List<int> equippedItems = new List<int>();
    
    // 存档元信息
    public string saveTime;
    public int saveVersion = 1;
}
```

### 事件系统集成
```csharp
// 存档相关事件
public class GameSavedEvent : IEvent
public class GameLoadedEvent : IEvent
public class GameSaveDeletedEvent : IEvent
public class DayChangeEvent : IEvent        // 触发每日自动保存
```

## 实现要点

### 1. 数据收集与应用
```csharp
// 创建存档时收集数据
private GameSaveData CreateSaveData()
{
    // 收集时间系统数据
    var clockModel = ClockModel.Instance;
    saveData.clockDay = clockModel.ClockDay;
    saveData.dayProgress = clockModel.DayProgress;
    
    // 收集背包数据
    var packageModel = PackageModel.Instance;
    foreach (var item in packageModel.GetAllItems())
    {
        saveData.packageItems.Add(new SavePackageItem(item));
    }
    
    // 收集玩家数据
    var player = Player.Instance;
    saveData.currentHealth = player.CurrentHealth;
    saveData.playerPosition = player.transform.position;
}

// 加载存档时应用数据
private void ApplySaveData(GameSaveData saveData)
{
    // 应用时间系统数据
    ClockModel.Instance.SetGameTime(saveData.clockDay, saveData.dayProgress, saveData.currentTimeOfDay);
    
    // 应用背包数据
    PackageModel.Instance.ClearAllItems();
    foreach (var saveItem in saveData.packageItems)
    {
        PackageModel.Instance.AddItem(saveItem.itemId, saveItem.count);
    }
    
    // 应用玩家数据
    Player.Instance.SetHealth(saveData.currentHealth);
    Player.Instance.transform.position = saveData.playerPosition;
}
```

### 2. PlayerPrefs存储实现
```csharp
// 存储键名约定
private const string SAVE_KEY_PREFIX = "GameSave_";
private const string SAVE_VERSION_KEY = "SaveVersion_";
private const string SAVE_TIME_KEY = "SaveTime_";

// 保存实现
string jsonData = JsonUtility.ToJson(saveData, false);
PlayerPrefs.SetString(GetSaveKey(slot), jsonData);
PlayerPrefs.SetInt(GetVersionKey(slot), CURRENT_SAVE_VERSION);
PlayerPrefs.SetString(GetTimeKey(slot), saveData.saveTime);
PlayerPrefs.Save();

// 加载实现
string jsonData = PlayerPrefs.GetString(GetSaveKey(slot));
GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
```

### 3. 每日自动保存机制
```csharp
// 在ClockModel中发布天数变化事件
private void UpdateTimeProgress()
{
    if (_dayProgress >= 1f)
    {
        int previousDay = _clockDay;
        _clockDay = Mathf.Min(_clockDay + 1, _maxDays);
        
        if (_clockDay != previousDay)
        {
            EventManager.Instance.Publish(new DayChangeEvent(previousDay, _clockDay));
        }
    }
}

// 在SaveView中响应天数变化
private void OnDayChanged(DayChangeEvent eventData)
{
    if (eventData.CurrentDay != _lastSavedDay)
    {
        SaveManager.Instance.SaveGame(0);
        _lastSavedDay = eventData.CurrentDay;
    }
}
```

## 最佳实践

### 1. UI组件实现
```csharp
// SaveView组件 - 自动查找按钮并绑定事件
private void InitializeButtons()
{
    _btnPrint = transform.Find("btn_print")?.GetComponent<Button>();
    _btnRevert = transform.Find("btn_revert")?.GetComponent<Button>();
    _btnSave = transform.Find("btn_save")?.GetComponent<Button>();
    
    _btnPrint?.onClick.AddListener(OnPrintButtonClick);
    _btnRevert?.onClick.AddListener(OnRevertButtonClick);
    _btnSave?.onClick.AddListener(OnSaveButtonClick);
}

// UI结构约定
SaveView GameObject
├── btn_print (Button) - 打印进度信息
├── btn_revert (Button) - 删除存档
└── btn_save (Button) - 手动保存
```

### 2. 错误处理模式
```csharp
public bool SaveGame(int slot = 0)
{
    if (!IsValidSlot(slot))
    {
        Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
        return false;
    }
    
    try
    {
        // 执行保存逻辑
        return true;
    }
    catch (Exception e)
    {
        Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
        return false;
    }
}
```

### 3. 事件通知机制
```csharp
// 保存完成后发布事件
EventManager.Instance.Publish(new GameSavedEvent(slot, saveData.saveTime));

// 订阅事件进行UI更新
private void OnGameSaved(GameSavedEvent eventData)
{
    // 更新UI状态或显示提示
}
```

## 扩展接口要求

### 现有系统需要添加的方法
```csharp
// ClockModel.cs
public void SetGameTime(int day, float progress, TimeOfDay timeOfDay)

// PackageModel.cs  
public void ClearAllItems()

// DamageableObject.cs (Player继承)
public virtual void SetHealth(float health)
```

## 约束条件

### 命名规范
- 类名使用PascalCase: `SaveManager`, `GameSaveData`
- 私有字段使用camelCase+下划线: `_lastSavedDay`, `_btnPrint`
- 常量使用UPPER_SNAKE_CASE: `SAVE_KEY_PREFIX`, `MAX_SAVE_SLOTS`

### 注释规范
- 公共类和复杂方法使用XML注释 `/// <summary>`
- 简单方法和私有方法使用单行注释 `//`
- 事件类使用简洁的单行注释

### 性能要求
- PlayerPrefs适用于中小型数据量(建议单个存档小于1MB)
- 自动保存间隔不低于60秒
- 使用引用计数避免重复序列化

### 兼容性要求
- 支持存档版本升级机制
- 向前兼容，新版本能读取旧存档
- 跨平台PlayerPrefs兼容性 