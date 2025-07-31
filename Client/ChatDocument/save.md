# 设计玩家游戏进度存储方案
_Exported on 2025/7/31 at GMT+8 22:09:30 from Cursor (1.1.6)_

---

**User**

我需要设计一个功能方案，用来存储玩家本地的游戏进度，包括时间天数，背包里的道具数量

---

**Cursor**

我需要了解项目中现有的背包系统和时间系统的数据结构，以便设计一个合适的本地存档功能。让我先查看相关的代码。

基于我对项目现有数据结构的分析，我来为您设计一个本地游戏进度存档系统。

## 本地存档系统设计方案

### 核心需求分析

通过分析项目代码，我发现需要存储的关键数据包括：

1. **时间系统数据**（参考`ClockModel.cs`）：
   - 当前天数
   - 当天进度
   - 时间段状态

2. **背包系统数据**（参考`PackageModel.cs`）：
   - 道具ID和数量列表

3. **玩家数据**（参考`Player.cs`, `CombatEntity.cs`, `DamageableObject.cs`）：
   - 当前血量
   - 玩家位置
   - 当前装备

### 设计方案

#### 1. 存档数据结构类

```csharp
/// <summary>
/// 游戏存档数据 - 存储所有需要持久化的游戏进度
/// </summary>
[System.Serializable]
public class GameSaveData
{   
    [Header("时间系统")]
    public int clockDay = 1;                    // 当前天数
    public float dayProgress = 0f;              // 当天进度 (0.0-1.0)
    public TimeOfDay currentTimeOfDay = TimeOfDay.Day; // 当前时间段
    
    [Header("背包系统")]
    public List<SavePackageItem> packageItems = new List<SavePackageItem>(); // 背包道具列表
    
    [Header("玩家数据")]
    public float currentHealth = 100f;          // 当前血量
    public Vector3 playerPosition = Vector3.zero; // 玩家位置
    public List<int> equippedItems = new List<int>(); // 已装备的道具ID列表
    
    [Header("存档信息")]
    public string saveTime;                     // 存档时间
    public int saveVersion = 1;                 // 存档版本号
}

/// <summary>
/// 可序列化的背包道具数据
/// </summary>
[System.Serializable]
public class SavePackageItem
{
    public int itemId;
    public int count;
    public float modifyTime;
    
    public SavePackageItem() { }
    
    public SavePackageItem(PackageItem item)
    {
        itemId = item.itemId;
        count = item.count;
        modifyTime = item.modifyTime;
    }
    
    public PackageItem ToPackageItem()
    {
        return new PackageItem(itemId, count) { modifyTime = modifyTime };
    }
}
```

#### 2. 存档管理器

```csharp
/// <summary>
/// 游戏存档管理器 - 负责游戏进度的保存和加载
/// </summary>
public class SaveManager
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveManager();
            return _instance;
        }
    }
    
    private const string SAVE_KEY = "GameSaveData";
    private const int CURRENT_SAVE_VERSION = 1;
    
    private SaveManager() { }
    
    /// <summary>
    /// 保存当前游戏进度
    /// </summary>
    public bool SaveGame()
    {
        try
        {
            GameSaveData saveData = CreateSaveData();
            string jsonData = JsonUtility.ToJson(saveData, true);
            
            PlayerPrefs.SetString(SAVE_KEY, jsonData);
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveManager] Game saved successfully at {saveData.saveTime}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 加载游戏进度
    /// </summary>
    public bool LoadGame()
    {
        try
        {
            if (!HasSaveData())
            {
                Debug.LogWarning("[SaveManager] No save data found");
                return false;
            }
            
            string jsonData = PlayerPrefs.GetString(SAVE_KEY);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            if (saveData == null)
            {
                Debug.LogError("[SaveManager] Failed to parse save data");
                return false;
            }
            
            ApplySaveData(saveData);
            Debug.Log($"[SaveManager] Game loaded successfully from {saveData.saveTime}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查是否存在存档数据
    /// </summary>
    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
    
    /// <summary>
    /// 删除存档数据
    /// </summary>
    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Save data deleted");
    }
    
    /// <summary>
    /// 获取存档信息（不加载游戏）
    /// </summary>
    public GameSaveData GetSaveInfo()
    {
        if (!HasSaveData()) return null;
        
        try
        {
            string jsonData = PlayerPrefs.GetString(SAVE_KEY);
            return JsonUtility.FromJson<GameSaveData>(jsonData);
        }
        catch (System.Exception)
        {
            return null;
        }
    }
    
    /// <summary>
    /// 创建当前游戏状态的存档数据
    /// </summary>
    private GameSaveData CreateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        
        // 保存时间系统数据
        var clockModel = ClockModel.Instance;
        saveData.clockDay = clockModel.ClockDay;
        saveData.dayProgress = clockModel.DayProgress;
        saveData.currentTimeOfDay = clockModel.CurrentTimeOfDay;
        
        // 保存背包数据
        var packageModel = PackageModel.Instance;
        saveData.packageItems.Clear();
        foreach (var item in packageModel.GetAllItems())
        {
            saveData.packageItems.Add(new SavePackageItem(item));
        }
        
        // 保存玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            saveData.currentHealth = player.CurrentHealth;
            saveData.playerPosition = player.transform.position;
            
            // 保存装备ID列表（需要添加获取装备ID的方法）
            saveData.equippedItems = GetPlayerEquippedItems(player);
        }
        
        // 保存存档信息
        saveData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        saveData.saveVersion = CURRENT_SAVE_VERSION;
        
        return saveData;
    }
    
    /// <summary>
    /// 应用存档数据到游戏系统
    /// </summary>
    private void ApplySaveData(GameSaveData saveData)
    {
        // 应用时间系统数据（需要为ClockModel添加设置方法）
        ApplyClockData(saveData);
        
        // 应用背包数据
        ApplyPackageData(saveData);
        
        // 应用玩家数据
        ApplyPlayerData(saveData);
    }
    
    /// <summary>
    /// 应用时间系统数据
    /// </summary>
    private void ApplyClockData(GameSaveData saveData)
    {
        // 需要为ClockModel添加数据设置方法
        var clockModel = ClockModel.Instance;
        // clockModel.SetGameTime(saveData.clockDay, saveData.dayProgress, saveData.currentTimeOfDay);
    }
    
    /// <summary>
    /// 应用背包数据
    /// </summary>
    private void ApplyPackageData(GameSaveData saveData)
    {
        var packageModel = PackageModel.Instance;
        
        // 清空当前背包（需要为PackageModel添加清空方法）
        // packageModel.ClearAllItems();
        
        // 加载存档中的道具
        foreach (var saveItem in saveData.packageItems)
        {
            packageModel.AddItem(saveItem.itemId, saveItem.count);
        }
    }
    
    /// <summary>
    /// 应用玩家数据
    /// </summary>
    private void ApplyPlayerData(GameSaveData saveData)
    {
        var player = Player.Instance;
        if (player == null) return;
        
        // 设置血量（需要为Player添加设置血量方法）
        // player.SetHealth(saveData.currentHealth);
        
        // 设置位置
        player.transform.position = saveData.playerPosition;
        
        // 重新装备道具
        foreach (int equipId in saveData.equippedItems)
        {
            player.Equip(equipId);
        }
    }
    
    /// <summary>
    /// 获取玩家当前装备的道具ID列表
    /// </summary>
    private List<int> GetPlayerEquippedItems(Player player)
    {
        // 需要为CombatEntity添加获取装备ID列表的方法
        // return player.GetEquippedItemIds();
        return new List<int>(); // 暂时返回空列表
    }
}
```

#### 3. 需要为现有系统添加的辅助方法

**为ClockModel添加的方法：**
```csharp
// 需要添加到ClockModel.cs中
public void SetGameTime(int day, float progress, TimeOfDay timeOfDay)
{
    _clockDay = Mathf.Clamp(day, 1, _maxDays);
    _dayProgress = Mathf.Clamp01(progress);
    _currentTimeOfDay = timeOfDay;
    UpdateTargetAmbientIntensity();
    RenderSettings.ambientIntensity = _targetAmbientIntensity;
}
```

**为PackageModel添加的方法：**
```csharp
// 需要添加到PackageModel.cs中
public void ClearAllItems()
{
    _packageItems.Clear();
}

public void LoadPackageData(List<PackageItem> items)
{
    _packageItems = new List<PackageItem>(items);
}
```

**为CombatEntity添加的方法：**
```csharp
// 需要添加到CombatEntity.cs中
public void SetHealth(float health)
{
    _currentHealth = Mathf.Clamp(health, 0, _maxHealth);
}

public List<int> GetEquippedItemIds()
{
    List<int> equipIds = new List<int>();
    foreach (var equip in _equips)
    {
        equipIds.Add(equip.EquipId); // 需要为EquipBase添加EquipId属性
    }
    return equipIds;
}
```

#### 4. 使用示例

```csharp
/// <summary>
/// 游戏管理器示例 - 展示如何使用存档系统
/// </summary>
public class GameManagerExample : MonoBehaviour
{
    void Start()
    {
        // 游戏启动时自动加载存档
        if (SaveManager.Instance.HasSaveData())
        {
            SaveManager.Instance.LoadGame();
        }
    }
    
    void Update()
    {
        // 按F5保存游戏
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveManager.Instance.SaveGame();
        }
        
        // 按F9加载游戏
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveManager.Instance.LoadGame();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // 应用暂停时自动保存
        if (pauseStatus)
        {
            SaveManager.Instance.SaveGame();
        }
    }
}
```

### 特点说明

1. **基于现有架构**：完全遵循项目的单例模式和命名规范
2. **可扩展性**：轻松添加新的存档数据类型
3. **版本控制**：支持存档版本管理，便于未来升级
4. **错误处理**：包含完整的异常处理机制
5. **自动保存**：支持应用暂停时自动保存
6. **存档信息**：可查看存档详细信息而不加载游戏

这个设计方案能够满足您存储时间天数和背包道具数量的需求，同时为将来扩展更多存档功能提供了良好的基础。

```csharp
/// <summary>
/// 游戏存档数据 - 存储所有需要持久化的游戏进度
/// </summary>
[System.Serializable]
public class GameSaveData
{   
    [Header("时间系统")]
    public int clockDay = 1;                    // 当前天数
    public float dayProgress = 0f;              // 当天进度 (0.0-1.0)
    public TimeOfDay currentTimeOfDay = TimeOfDay.Day; // 当前时间段
    
    [Header("背包系统")]
    public List<SavePackageItem> packageItems = new List<SavePackageItem>(); // 背包道具列表
    
    [Header("玩家数据")]
    public float currentHealth = 100f;          // 当前血量
    public Vector3 playerPosition = Vector3.zero; // 玩家位置
    public List<int> equippedItems = new List<int>(); // 已装备的道具ID列表
    
    [Header("存档信息")]
    public string saveTime;                     // 存档时间
    public int saveVersion = 1;                 // 存档版本号
}

/// <summary>
/// 可序列化的背包道具数据
/// </summary>
[System.Serializable]
public class SavePackageItem
{
    public int itemId;
    public int count;
    public float modifyTime;
    
    public SavePackageItem() { }
    
    public SavePackageItem(PackageItem item)
    {
        itemId = item.itemId;
        count = item.count;
        modifyTime = item.modifyTime;
    }
    
    public PackageItem ToPackageItem()
    {
        return new PackageItem(itemId, count) { modifyTime = modifyTime };
    }
}
```

```csharp
/// <summary>
/// 游戏存档管理器 - 负责游戏进度的保存和加载
/// </summary>
public class SaveManager
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveManager();
            return _instance;
        }
    }
    
    private const string SAVE_KEY = "GameSaveData";
    private const int CURRENT_SAVE_VERSION = 1;
    
    private SaveManager() { }
    
    /// <summary>
    /// 保存当前游戏进度
    /// </summary>
    public bool SaveGame()
    {
        try
        {
            GameSaveData saveData = CreateSaveData();
            string jsonData = JsonUtility.ToJson(saveData, true);
            
            PlayerPrefs.SetString(SAVE_KEY, jsonData);
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveManager] Game saved successfully at {saveData.saveTime}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 加载游戏进度
    /// </summary>
    public bool LoadGame()
    {
        try
        {
            if (!HasSaveData())
            {
                Debug.LogWarning("[SaveManager] No save data found");
                return false;
            }
            
            string jsonData = PlayerPrefs.GetString(SAVE_KEY);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            if (saveData == null)
            {
                Debug.LogError("[SaveManager] Failed to parse save data");
                return false;
            }
            
            ApplySaveData(saveData);
            Debug.Log($"[SaveManager] Game loaded successfully from {saveData.saveTime}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查是否存在存档数据
    /// </summary>
    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
    
    /// <summary>
    /// 删除存档数据
    /// </summary>
    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Save data deleted");
    }
    
    /// <summary>
    /// 获取存档信息（不加载游戏）
    /// </summary>
    public GameSaveData GetSaveInfo()
    {
        if (!HasSaveData()) return null;
        
        try
        {
            string jsonData = PlayerPrefs.GetString(SAVE_KEY);
            return JsonUtility.FromJson<GameSaveData>(jsonData);
        }
        catch (System.Exception)
        {
            return null;
        }
    }
    
    /// <summary>
    /// 创建当前游戏状态的存档数据
    /// </summary>
    private GameSaveData CreateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        
        // 保存时间系统数据
        var clockModel = ClockModel.Instance;
        saveData.clockDay = clockModel.ClockDay;
        saveData.dayProgress = clockModel.DayProgress;
        saveData.currentTimeOfDay = clockModel.CurrentTimeOfDay;
        
        // 保存背包数据
        var packageModel = PackageModel.Instance;
        saveData.packageItems.Clear();
        foreach (var item in packageModel.GetAllItems())
        {
            saveData.packageItems.Add(new SavePackageItem(item));
        }
        
        // 保存玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            saveData.currentHealth = player.CurrentHealth;
            saveData.playerPosition = player.transform.position;
            
            // 保存装备ID列表（需要添加获取装备ID的方法）
            saveData.equippedItems = GetPlayerEquippedItems(player);
        }
        
        // 保存存档信息
        saveData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        saveData.saveVersion = CURRENT_SAVE_VERSION;
        
        return saveData;
    }
    
    /// <summary>
    /// 应用存档数据到游戏系统
    /// </summary>
    private void ApplySaveData(GameSaveData saveData)
    {
        // 应用时间系统数据（需要为ClockModel添加设置方法）
        ApplyClockData(saveData);
        
        // 应用背包数据
        ApplyPackageData(saveData);
        
        // 应用玩家数据
        ApplyPlayerData(saveData);
    }
    
    /// <summary>
    /// 应用时间系统数据
    /// </summary>
    private void ApplyClockData(GameSaveData saveData)
    {
        // 需要为ClockModel添加数据设置方法
        var clockModel = ClockModel.Instance;
        // clockModel.SetGameTime(saveData.clockDay, saveData.dayProgress, saveData.currentTimeOfDay);
    }
    
    /// <summary>
    /// 应用背包数据
    /// </summary>
    private void ApplyPackageData(GameSaveData saveData)
    {
        var packageModel = PackageModel.Instance;
        
        // 清空当前背包（需要为PackageModel添加清空方法）
        // packageModel.ClearAllItems();
        
        // 加载存档中的道具
        foreach (var saveItem in saveData.packageItems)
        {
            packageModel.AddItem(saveItem.itemId, saveItem.count);
        }
    }
    
    /// <summary>
    /// 应用玩家数据
    /// </summary>
    private void ApplyPlayerData(GameSaveData saveData)
    {
        var player = Player.Instance;
        if (player == null) return;
        
        // 设置血量（需要为Player添加设置血量方法）
        // player.SetHealth(saveData.currentHealth);
        
        // 设置位置
        player.transform.position = saveData.playerPosition;
        
        // 重新装备道具
        foreach (int equipId in saveData.equippedItems)
        {
            player.Equip(equipId);
        }
    }
    
    /// <summary>
    /// 获取玩家当前装备的道具ID列表
    /// </summary>
    private List<int> GetPlayerEquippedItems(Player player)
    {
        // 需要为CombatEntity添加获取装备ID列表的方法
        // return player.GetEquippedItemIds();
        return new List<int>(); // 暂时返回空列表
    }
}
```

```csharp
// 需要添加到ClockModel.cs中
public void SetGameTime(int day, float progress, TimeOfDay timeOfDay)
{
    _clockDay = Mathf.Clamp(day, 1, _maxDays);
    _dayProgress = Mathf.Clamp01(progress);
    _currentTimeOfDay = timeOfDay;
    UpdateTargetAmbientIntensity();
    RenderSettings.ambientIntensity = _targetAmbientIntensity;
}
```

```csharp
// 需要添加到PackageModel.cs中
public void ClearAllItems()
{
    _packageItems.Clear();
}

public void LoadPackageData(List<PackageItem> items)
{
    _packageItems = new List<PackageItem>(items);
}
```

```csharp
// 需要添加到CombatEntity.cs中
public void SetHealth(float health)
{
    _currentHealth = Mathf.Clamp(health, 0, _maxHealth);
}

public List<int> GetEquippedItemIds()
{
    List<int> equipIds = new List<int>();
    foreach (var equip in _equips)
    {
        equipIds.Add(equip.EquipId); // 需要为EquipBase添加EquipId属性
    }
    return equipIds;
}
```

```csharp
/// <summary>
/// 游戏管理器示例 - 展示如何使用存档系统
/// </summary>
public class GameManagerExample : MonoBehaviour
{
    void Start()
    {
        // 游戏启动时自动加载存档
        if (SaveManager.Instance.HasSaveData())
        {
            SaveManager.Instance.LoadGame();
        }
    }
    
    void Update()
    {
        // 按F5保存游戏
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveManager.Instance.SaveGame();
        }
        
        // 按F9加载游戏
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveManager.Instance.LoadGame();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // 应用暂停时自动保存
        if (pauseStatus)
        {
            SaveManager.Instance.SaveGame();
        }
    }
}
```

---

**User**

本地保存的文件是json格式吗？还是有别的推荐，介绍一下功能框架

---

**Cursor**

基于Unity游戏开发的最佳实践，我来为您详细介绍几种本地存档方案及其优缺点，并推荐一个完整的存档功能框架。

## 本地存档格式方案对比

### 1. PlayerPrefs方案（我之前的设计）
```csharp
// 优点：Unity内置，跨平台，简单易用
// 缺点：存储在注册表/系统偏好中，不适合大量数据
PlayerPrefs.SetString(SAVE_KEY, jsonData);
```

### 2. JSON文件方案
```csharp
// 优点：人类可读，易于调试，跨平台兼容
// 缺点：明文存储，玩家可轻易修改，文件较大
string filePath = Path.Combine(Application.persistentDataPath, "save.json");
File.WriteAllText(filePath, jsonData);
```

### 3. 二进制文件方案
```csharp
// 优点：文件小，加载快，不易被修改
// 缺点：不可读，调试困难
BinaryFormatter formatter = new BinaryFormatter();
FileStream stream = new FileStream(filePath, FileMode.Create);
formatter.Serialize(stream, saveData);
```

### 4. 加密JSON方案（推荐）
```csharp
// 优点：安全性高，支持调试模式，文件大小适中
// 缺点：实现复杂度中等
string encryptedData = EncryptionHelper.Encrypt(jsonData);
File.WriteAllText(filePath, encryptedData);
```

## 推荐的完整存档功能框架

基于项目的规范和实际需求，我推荐使用**加密JSON方案**，以下是完整的实现：

```csharp
// 优点：Unity内置，跨平台，简单易用
// 缺点：存储在注册表/系统偏好中，不适合大量数据
PlayerPrefs.SetString(SAVE_KEY, jsonData);
```

```csharp
// 优点：人类可读，易于调试，跨平台兼容
// 缺点：明文存储，玩家可轻易修改，文件较大
string filePath = Path.Combine(Application.persistentDataPath, "save.json");
File.WriteAllText(filePath, jsonData);
```

```csharp
// 优点：文件小，加载快，不易被修改
// 缺点：不可读，调试困难
BinaryFormatter formatter = new BinaryFormatter();
FileStream stream = new FileStream(filePath, FileMode.Create);
formatter.Serialize(stream, saveData);
```

```csharp
// 优点：安全性高，支持调试模式，文件大小适中
// 缺点：实现复杂度中等
string encryptedData = EncryptionHelper.Encrypt(jsonData);
File.WriteAllText(filePath, encryptedData);
```

## 完整的存档功能框架设计

### 1. 核心工具类 - 加密助手

```csharp
using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// 加密辅助工具类 - 提供简单的字符串加密/解密功能
/// </summary>
public static class EncryptionHelper
{
    private const string ENCRYPTION_KEY = "GameSave2024Key!"; // 16字符密钥
    
    /// <summary>
    /// 加密字符串
    /// </summary>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;
            
        try
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV();
                
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                    Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                    Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
                    return Convert.ToBase64String(result);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[EncryptionHelper] Encryption failed: {e.Message}");
            return plainText; // 加密失败返回原文
        }
    }
    
    /// <summary>
    /// 解密字符串
    /// </summary>
    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;
            
        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                
                byte[] iv = new byte[16];
                byte[] encrypted = new byte[encryptedBytes.Length - 16];
                Array.Copy(encryptedBytes, 0, iv, 0, 16);
                Array.Copy(encryptedBytes, 16, encrypted, 0, encrypted.Length);
                aes.IV = iv;
                
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[EncryptionHelper] Decryption failed: {e.Message}");
            return encryptedText; // 解密失败返回原文
        }
    }
}
```

### 2. 文件操作助手

```csharp
using System.IO;
using UnityEngine;

/// <summary>
/// 文件操作助手 - 提供安全的文件读写功能
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// 获取存档文件目录
    /// </summary>
    public static string GetSaveDirectory()
    {
        string saveDir = Path.Combine(Application.persistentDataPath, "SaveData");
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        return saveDir;
    }
    
    /// <summary>
    /// 安全写入文件
    /// </summary>
    public static bool WriteFile(string fileName, string content, bool encrypt = true)
    {
        try
        {
            string filePath = Path.Combine(GetSaveDirectory(), fileName);
            string finalContent = encrypt ? EncryptionHelper.Encrypt(content) : content;
            
            // 写入临时文件
            string tempPath = filePath + ".tmp";
            File.WriteAllText(tempPath, finalContent, System.Text.Encoding.UTF8);
            
            // 原子操作：重命名临时文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.Move(tempPath, filePath);
            
            Debug.Log($"[FileHelper] File saved: {fileName}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileHelper] Failed to write file {fileName}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 安全读取文件
    /// </summary>
    public static string ReadFile(string fileName, bool decrypt = true)
    {
        try
        {
            string filePath = Path.Combine(GetSaveDirectory(), fileName);
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            string content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            return decrypt ? EncryptionHelper.Decrypt(content) : content;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileHelper] Failed to read file {fileName}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    public static bool FileExists(string fileName)
    {
        string filePath = Path.Combine(GetSaveDirectory(), fileName);
        return File.Exists(filePath);
    }
    
    /// <summary>
    /// 删除文件
    /// </summary>
    public static bool DeleteFile(string fileName)
    {
        try
        {
            string filePath = Path.Combine(GetSaveDirectory(), fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[FileHelper] File deleted: {fileName}");
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileHelper] Failed to delete file {fileName}: {e.Message}");
            return false;
        }
    }
}
```

### 3. 完整的存档管理器（改进版）

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档管理器 - 支持多种存储方式的完整存档系统
/// </summary>
public class SaveManager
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveManager();
            return _instance;
        }
    }
    
    // 存档配置
    private const string DEFAULT_SAVE_FILE = "gamesave.dat";
    private const string BACKUP_SAVE_FILE = "gamesave_backup.dat";
    private const int CURRENT_SAVE_VERSION = 1;
    private const int MAX_SAVE_SLOTS = 5; // 支持5个存档槽
    
    /// <summary>
    /// 存储方式枚举
    /// </summary>
    public enum SaveFormat
    {
        EncryptedJSON,   // 加密JSON（推荐）
        PlainJSON,       // 明文JSON（调试用）
        PlayerPrefs,     // Unity PlayerPrefs
        Binary           // 二进制格式
    }
    
    [Header("存档设置")]
    [SerializeField] private SaveFormat _saveFormat = SaveFormat.EncryptedJSON;
    [SerializeField] private bool _enableAutoSave = true;
    [SerializeField] private float _autoSaveInterval = 300f; // 5分钟自动保存
    [SerializeField] private bool _createBackup = true;
    
    private float _lastAutoSaveTime;
    
    private SaveManager() 
    {
        _lastAutoSaveTime = Time.time;
    }
    
    /// <summary>
    /// 更新自动保存（需要在GameManager中调用）
    /// </summary>
    public void Update()
    {
        if (_enableAutoSave && Time.time - _lastAutoSaveTime >= _autoSaveInterval)
        {
            AutoSave();
            _lastAutoSaveTime = Time.time;
        }
    }
    
    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    public bool SaveGame(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
            return false;
        }
        
        try
        {
            GameSaveData saveData = CreateSaveData();
            string fileName = GetSaveFileName(slot);
            
            bool success = false;
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                    success = SaveAsEncryptedJSON(fileName, saveData);
                    break;
                case SaveFormat.PlainJSON:
                    success = SaveAsPlainJSON(fileName, saveData);
                    break;
                case SaveFormat.PlayerPrefs:
                    success = SaveAsPlayerPrefs(slot, saveData);
                    break;
                case SaveFormat.Binary:
                    success = SaveAsBinary(fileName, saveData);
                    break;
            }
            
            if (success && _createBackup && slot == 0)
            {
                CreateBackup();
            }
            
            Debug.Log($"[SaveManager] Game saved to slot {slot} using {_saveFormat} format");
            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 从指定槽位加载游戏
    /// </summary>
    public bool LoadGame(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
            return false;
        }
        
        try
        {
            string fileName = GetSaveFileName(slot);
            GameSaveData saveData = null;
            
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                    saveData = LoadFromEncryptedJSON(fileName);
                    break;
                case SaveFormat.PlainJSON:
                    saveData = LoadFromPlainJSON(fileName);
                    break;
                case SaveFormat.PlayerPrefs:
                    saveData = LoadFromPlayerPrefs(slot);
                    break;
                case SaveFormat.Binary:
                    saveData = LoadFromBinary(fileName);
                    break;
            }
            
            if (saveData == null)
            {
                Debug.LogWarning($"[SaveManager] No save data found in slot {slot}");
                return false;
            }
            
            ApplySaveData(saveData);
            Debug.Log($"[SaveManager] Game loaded from slot {slot} - {saveData.saveTime}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查指定槽位是否有存档
    /// </summary>
    public bool HasSaveData(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS) return false;
        
        switch (_saveFormat)
        {
            case SaveFormat.EncryptedJSON:
            case SaveFormat.PlainJSON:
            case SaveFormat.Binary:
                return FileHelper.FileExists(GetSaveFileName(slot));
            case SaveFormat.PlayerPrefs:
                return PlayerPrefs.HasKey(GetPlayerPrefsKey(slot));
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 获取存档信息（不加载游戏）
    /// </summary>
    public GameSaveData GetSaveInfo(int slot = 0)
    {
        if (!HasSaveData(slot)) return null;
        
        try
        {
            string fileName = GetSaveFileName(slot);
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                    return LoadFromEncryptedJSON(fileName);
                case SaveFormat.PlainJSON:
                    return LoadFromPlainJSON(fileName);
                case SaveFormat.PlayerPrefs:
                    return LoadFromPlayerPrefs(slot);
                case SaveFormat.Binary:
                    return LoadFromBinary(fileName);
                default:
                    return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    public bool DeleteSaveData(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS) return false;
        
        try
        {
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                case SaveFormat.PlainJSON:
                case SaveFormat.Binary:
                    return FileHelper.DeleteFile(GetSaveFileName(slot));
                case SaveFormat.PlayerPrefs:
                    PlayerPrefs.DeleteKey(GetPlayerPrefsKey(slot));
                    PlayerPrefs.Save();
                    return true;
                default:
                    return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to delete save data: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 自动保存
    /// </summary>
    private void AutoSave()
    {
        SaveGame(0); // 自动保存到槽位0
        Debug.Log("[SaveManager] Auto save completed");
    }
    
    /// <summary>
    /// 创建备份
    /// </summary>
    private void CreateBackup()
    {
        try
        {
            string originalFile = GetSaveFileName(0);
            if (_saveFormat == SaveFormat.PlayerPrefs)
            {
                // PlayerPrefs备份需要特殊处理
                string backupKey = GetPlayerPrefsKey(0) + "_backup";
                string originalData = PlayerPrefs.GetString(GetPlayerPrefsKey(0));
                PlayerPrefs.SetString(backupKey, originalData);
                PlayerPrefs.Save();
            }
            else
            {
                string originalPath = System.IO.Path.Combine(FileHelper.GetSaveDirectory(), originalFile);
                string backupPath = System.IO.Path.Combine(FileHelper.GetSaveDirectory(), BACKUP_SAVE_FILE);
                
                if (System.IO.File.Exists(originalPath))
                {
                    System.IO.File.Copy(originalPath, backupPath, true);
                }
            }
            Debug.Log("[SaveManager] Backup created");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to create backup: {e.Message}");
        }
    }
    
    // === 不同格式的保存方法 ===
    
    private bool SaveAsEncryptedJSON(string fileName, GameSaveData saveData)
    {
        string jsonData = JsonUtility.ToJson(saveData, true);
        return FileHelper.WriteFile(fileName, jsonData, true);
    }
    
    private bool SaveAsPlainJSON(string fileName, GameSaveData saveData)
    {
        string jsonData = JsonUtility.ToJson(saveData, true);
        return FileHelper.WriteFile(fileName, jsonData, false);
    }
    
    private bool SaveAsPlayerPrefs(int slot, GameSaveData saveData)
    {
        string jsonData = JsonUtility.ToJson(saveData, false);
        string key = GetPlayerPrefsKey(slot);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.Save();
        return true;
    }
    
    private bool SaveAsBinary(string fileName, GameSaveData saveData)
    {
        // 注意：Unity 2018+建议使用JsonUtility而不是BinaryFormatter
        // 这里提供一个简化的二进制实现
        string jsonData = JsonUtility.ToJson(saveData, false);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
        string base64Data = Convert.ToBase64String(bytes);
        return FileHelper.WriteFile(fileName, base64Data, false);
    }
    
    // === 不同格式的加载方法 ===
    
    private GameSaveData LoadFromEncryptedJSON(string fileName)
    {
        string jsonData = FileHelper.ReadFile(fileName, true);
        if (string.IsNullOrEmpty(jsonData)) return null;
        

```csharp
using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// 加密辅助工具类 - 提供简单的字符串加密/解密功能
/// </summary>
public static class EncryptionHelper
{
    private const string ENCRYPTION_KEY = "GameSave2024Key!"; // 16字符密钥
    
    /// <summary>
    /// 加密字符串
    /// </summary>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;
            
        try
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV();
                
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                    Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                    Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
                    return Convert.ToBase64String(result);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[EncryptionHelper] Encryption failed: {e.Message}");
            return plainText; // 加密失败返回原文
        }
    }
    
    /// <summary>
    /// 解密字符串
    /// </summary>
    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;
            
        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                
                byte[] iv = new byte[16];
                byte[] encrypted = new byte[encryptedBytes.Length - 16];
                Array.Copy(encryptedBytes, 0, iv, 0, 16);
                Array.Copy(encryptedBytes, 16, encrypted, 0, encrypted.Length);
                aes.IV = iv;
                
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[EncryptionHelper] Decryption failed: {e.Message}");
            return encryptedText; // 解密失败返回原文
        }
    }
}
```

```csharp
using System.IO;
using UnityEngine;

/// <summary>
/// 文件操作助手 - 提供安全的文件读写功能
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// 获取存档文件目录
    /// </summary>
    public static string GetSaveDirectory()
    {
        string saveDir = Path.Combine(Application.persistentDataPath, "SaveData");
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        return saveDir;
    }
    
    /// <summary>
    /// 安全写入文件
    /// </summary>
    public static bool WriteFile(string fileName, string content, bool encrypt = true)
    {
        try
        {
            string filePath = Path.Combine(GetSaveDirectory(), fileName);
            string finalContent = encrypt ? EncryptionHelper.Encrypt(content) : content;
            
            // 写入临时文件
            string tempPath = filePath + ".tmp";
            File.WriteAllText(tempPath, finalContent, System.Text.Encoding.UTF8);
            
            // 原子操作：重命名临时文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.Move(tempPath, filePath);
            
            Debug.Log($"[FileHelper] File saved: {fileName}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileHelper] Failed to write file {fileName}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 安全读取文件
    /// </summary>
    public static string ReadFile(string fileName, bool decrypt = true)
    {
        try
        {
            string filePath = Path.Combine(GetSaveDirectory(), fileName);
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            string content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            return decrypt ? EncryptionHelper.Decrypt(content) : content;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileHelper] Failed to read file {fileName}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    public static bool FileExists(string fileName)
    {
        string filePath = Path.Combine(GetSaveDirectory(), fileName);
        return File.Exists(filePath);
    }
    
    /// <summary>
    /// 删除文件
    /// </summary>
    public static bool DeleteFile(string fileName)
    {
        try
        {
            string filePath = Path.Combine(GetSaveDirectory(), fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[FileHelper] File deleted: {fileName}");
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileHelper] Failed to delete file {fileName}: {e.Message}");
            return false;
        }
    }
}
```

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档管理器 - 支持多种存储方式的完整存档系统
/// </summary>
public class SaveManager
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveManager();
            return _instance;
        }
    }
    
    // 存档配置
    private const string DEFAULT_SAVE_FILE = "gamesave.dat";
    private const string BACKUP_SAVE_FILE = "gamesave_backup.dat";
    private const int CURRENT_SAVE_VERSION = 1;
    private const int MAX_SAVE_SLOTS = 5; // 支持5个存档槽
    
    /// <summary>
    /// 存储方式枚举
    /// </summary>
    public enum SaveFormat
    {
        EncryptedJSON,   // 加密JSON（推荐）
        PlainJSON,       // 明文JSON（调试用）
        PlayerPrefs,     // Unity PlayerPrefs
        Binary           // 二进制格式
    }
    
    [Header("存档设置")]
    [SerializeField] private SaveFormat _saveFormat = SaveFormat.EncryptedJSON;
    [SerializeField] private bool _enableAutoSave = true;
    [SerializeField] private float _autoSaveInterval = 300f; // 5分钟自动保存
    [SerializeField] private bool _createBackup = true;
    
    private float _lastAutoSaveTime;
    
    private SaveManager() 
    {
        _lastAutoSaveTime = Time.time;
    }
    
    /// <summary>
    /// 更新自动保存（需要在GameManager中调用）
    /// </summary>
    public void Update()
    {
        if (_enableAutoSave && Time.time - _lastAutoSaveTime >= _autoSaveInterval)
        {
            AutoSave();
            _lastAutoSaveTime = Time.time;
        }
    }
    
    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    public bool SaveGame(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
            return false;
        }
        
        try
        {
            GameSaveData saveData = CreateSaveData();
            string fileName = GetSaveFileName(slot);
            
            bool success = false;
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                    success = SaveAsEncryptedJSON(fileName, saveData);
                    break;
                case SaveFormat.PlainJSON:
                    success = SaveAsPlainJSON(fileName, saveData);
                    break;
                case SaveFormat.PlayerPrefs:
                    success = SaveAsPlayerPrefs(slot, saveData);
                    break;
                case SaveFormat.Binary:
                    success = SaveAsBinary(fileName, saveData);
                    break;
            }
            
            if (success && _createBackup && slot == 0)
            {
                CreateBackup();
            }
            
            Debug.Log($"[SaveManager] Game saved to slot {slot} using {_saveFormat} format");
            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 从指定槽位加载游戏
    /// </summary>
    public bool LoadGame(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
            return false;
        }
        
        try
        {
            string fileName = GetSaveFileName(slot);
            GameSaveData saveData = null;
            
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                    saveData = LoadFromEncryptedJSON(fileName);
                    break;
                case SaveFormat.PlainJSON:
                    saveData = LoadFromPlainJSON(fileName);
                    break;
                case SaveFormat.PlayerPrefs:
                    saveData = LoadFromPlayerPrefs(slot);
                    break;
                case SaveFormat.Binary:
                    saveData = LoadFromBinary(fileName);
                    break;
            }
            
            if (saveData == null)
            {
                Debug.LogWarning($"[SaveManager] No save data found in slot {slot}");
                return false;
            }
            
            ApplySaveData(saveData);
            Debug.Log($"[SaveManager] Game loaded from slot {slot} - {saveData.saveTime}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查指定槽位是否有存档
    /// </summary>
    public bool HasSaveData(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS) return false;
        
        switch (_saveFormat)
        {
            case SaveFormat.EncryptedJSON:
            case SaveFormat.PlainJSON:
            case SaveFormat.Binary:
                return FileHelper.FileExists(GetSaveFileName(slot));
            case SaveFormat.PlayerPrefs:
                return PlayerPrefs.HasKey(GetPlayerPrefsKey(slot));
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 获取存档信息（不加载游戏）
    /// </summary>
    public GameSaveData GetSaveInfo(int slot = 0)
    {
        if (!HasSaveData(slot)) return null;
        
        try
        {
            string fileName = GetSaveFileName(slot);
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                    return LoadFromEncryptedJSON(fileName);
                case SaveFormat.PlainJSON:
                    return LoadFromPlainJSON(fileName);
                case SaveFormat.PlayerPrefs:
                    return LoadFromPlayerPrefs(slot);
                case SaveFormat.Binary:
                    return LoadFromBinary(fileName);
                default:
                    return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    public bool DeleteSaveData(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS) return false;
        
        try
        {
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                case SaveFormat.PlainJSON:
                case SaveFormat.Binary:
                    return FileHelper.DeleteFile(GetSaveFileName(slot));
                case SaveFormat.PlayerPrefs:
                    PlayerPrefs.DeleteKey(GetPlayerPrefsKey(slot));
                    PlayerPrefs.Save();
                    return true;
                default:
                    return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to delete save data: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 自动保存
    /// </summary>
    private void AutoSave()
    {
        SaveGame(0); // 自动保存到槽位0
        Debug.Log("[SaveManager] Auto save completed");
    }
    
    /// <summary>
    /// 创建备份
    /// </summary>
    private void CreateBackup()
    {
        try
        {
            string originalFile = GetSaveFileName(0);
            if (_saveFormat == SaveFormat.PlayerPrefs)
            {
                // PlayerPrefs备份需要特殊处理
                string backupKey = GetPlayerPrefsKey(0) + "_backup";
                string originalData = PlayerPrefs.GetString(GetPlayerPrefsKey(0));
                PlayerPrefs.SetString(backupKey, originalData);
                PlayerPrefs.Save();
            }
            else
            {
                string originalPath = System.IO.Path.Combine(FileHelper.GetSaveDirectory(), originalFile);
                string backupPath = System.IO.Path.Combine(FileHelper.GetSaveDirectory(), BACKUP_SAVE_FILE);
                
                if (System.IO.File.Exists(originalPath))
                {
                    System.IO.File.Copy(originalPath, backupPath, true);
                }
            }
            Debug.Log("[SaveManager] Backup created");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to create backup: {e.Message}");
        }
    }
    
    // === 不同格式的保存方法 ===
    
    private bool SaveAsEncryptedJSON(string fileName, GameSaveData saveData)
    {
        string jsonData = JsonUtility.ToJson(saveData, true);
        return FileHelper.WriteFile(fileName, jsonData, true);
    }
    
    private bool SaveAsPlainJSON(string fileName, GameSaveData saveData)
    {
        string jsonData = JsonUtility.ToJson(saveData, true);
        return FileHelper.WriteFile(fileName, jsonData, false);
    }
    
    private bool SaveAsPlayerPrefs(int slot, GameSaveData saveData)
    {
        string jsonData = JsonUtility.ToJson(saveData, false);
        string key = GetPlayerPrefsKey(slot);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.Save();
        return true;
    }
    
    private bool SaveAsBinary(string fileName, GameSaveData saveData)
    {
        // 注意：Unity 2018+建议使用JsonUtility而不是BinaryFormatter
        // 这里提供一个简化的二进制实现
        string jsonData = JsonUtility.ToJson(saveData, false);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
        string base64Data = Convert.ToBase64String(bytes);
        return FileHelper.WriteFile(fileName, base64Data, false);
    }
    
    // === 不同格式的加载方法 ===
    
    private GameSaveData LoadFromEncryptedJSON(string fileName)
    {
        string jsonData = FileHelper.ReadFile(fileName, true);
        if (string.IsNullOrEmpty(jsonData)) return null;
        
```

```csharp
using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// 加密辅助工具类 - 提供简单的字符串加密/解密功能
/// </summary>
public static class EncryptionHelper
{
    private const string ENCRYPTION_KEY = "GameSave2024Key!"; // 16字符密钥
    
    /// <summary>
    /// 加密字符串
    /// </summary>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;
            
        try
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV();
                
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                    Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                    Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
                    return Convert.ToBase64String(result);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[EncryptionHelper] Encryption failed: {e.Message}");
            return plainText; // 加密失败返回原文
        }
    }
    
    /// <summary>
    /// 解密字符串
    /// </summary>
    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;
            
        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                
                byte[] iv = new byte[16];
                byte[] encrypted = new byte[encryptedBytes.Length - 16];
                Array.Copy(encryptedBytes, 0, iv, 0, 16);
                Array.Copy(encryptedBytes, 16, encrypted, 0, encrypted.Length);
                aes.IV = iv;
                
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[EncryptionHelper] Decryption failed: {e.Message}");
            return encryptedText; // 解密失败返回原文
        }
    }
}
```

```csharp
using System.IO;
using UnityEngine;

/// <summary>
/// 文件操作助手 - 提供安全的文件读写功能
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// 获取存档文件目录
    /// </summary>
    public static string GetSaveDirectory()
    {
        string saveDir = Path.Combine(Application.persistentDataPath, "SaveData");
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        return saveDir;
    }
    
    /// <summary>
    /// 安全写入文件
    /// </summary>
    public static bool WriteFile(string fileName, string content, bool encrypt = true)
    {
        try
        {
            string filePath = Path.Combine(GetSaveDirectory(), fileName);
            string finalContent = encrypt ? EncryptionHelper.Encrypt(content) : content;
            
            // 写入临时文件
            string tempPath = filePath + ".tmp";
            File.WriteAllText(tempPath, finalContent, System.Text.Encoding.UTF8);
            
            // 原子操作：重命名临时文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.Move(tempPath, filePath);
            
            Debug.Log($"[FileHelper] File saved: {fileName}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileHelper] Failed to write file {fileName}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 安全读取文件
    /// </summary>
    public static string ReadFile(string fileName, bool decrypt = true)
    {
        try
        {
            string filePath = Path.Combine(GetSaveDirectory(), fileName);
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            string content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            return decrypt ? EncryptionHelper.Decrypt(content) : content;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileHelper] Failed to read file {fileName}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    public static bool FileExists(string fileName)
    {
        string filePath = Path.Combine(GetSaveDirectory(), fileName);
        return File.Exists(filePath);
    }
    
    /// <summary>
    /// 删除文件
    /// </summary>
    public static bool DeleteFile(string fileName)
    {
        try
        {
            string filePath = Path.Combine(GetSaveDirectory(), fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[FileHelper] File deleted: {fileName}");
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileHelper] Failed to delete file {fileName}: {e.Message}");
            return false;
        }
    }
}
```

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档管理器 - 支持多种存储方式的完整存档系统
/// </summary>
public class SaveManager
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveManager();
            return _instance;
        }
    }
    
    // 存档配置
    private const string DEFAULT_SAVE_FILE = "gamesave.dat";
    private const string BACKUP_SAVE_FILE = "gamesave_backup.dat";
    private const int CURRENT_SAVE_VERSION = 1;
    private const int MAX_SAVE_SLOTS = 5; // 支持5个存档槽
    
    /// <summary>
    /// 存储方式枚举
    /// </summary>
    public enum SaveFormat
    {
        EncryptedJSON,   // 加密JSON（推荐）
        PlainJSON,       // 明文JSON（调试用）
        PlayerPrefs,     // Unity PlayerPrefs
        Binary           // 二进制格式
    }
    
    [Header("存档设置")]
    [SerializeField] private SaveFormat _saveFormat = SaveFormat.EncryptedJSON;
    [SerializeField] private bool _enableAutoSave = true;
    [SerializeField] private float _autoSaveInterval = 300f; // 5分钟自动保存
    [SerializeField] private bool _createBackup = true;
    
    private float _lastAutoSaveTime;
    
    private SaveManager() 
    {
        _lastAutoSaveTime = Time.time;
    }
    
    /// <summary>
    /// 更新自动保存（需要在GameManager中调用）
    /// </summary>
    public void Update()
    {
        if (_enableAutoSave && Time.time - _lastAutoSaveTime >= _autoSaveInterval)
        {
            AutoSave();
            _lastAutoSaveTime = Time.time;
        }
    }
    
    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    public bool SaveGame(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
            return false;
        }
        
        try
        {
            GameSaveData saveData = CreateSaveData();
            string fileName = GetSaveFileName(slot);
            
            bool success = false;
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                    success = SaveAsEncryptedJSON(fileName, saveData);
                    break;
                case SaveFormat.PlainJSON:
                    success = SaveAsPlainJSON(fileName, saveData);
                    break;
                case SaveFormat.PlayerPrefs:
                    success = SaveAsPlayerPrefs(slot, saveData);
                    break;
                case SaveFormat.Binary:
                    success = SaveAsBinary(fileName, saveData);
                    break;
            }
            
            if (success && _createBackup && slot == 0)
            {
                CreateBackup();
            }
            
            Debug.Log($"[SaveManager] Game saved to slot {slot} using {_saveFormat} format");
            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 从指定槽位加载游戏
    /// </summary>
    public bool LoadGame(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
            return false;
        }
        
        try
        {
            string fileName = GetSaveFileName(slot);
            GameSaveData saveData = null;
            
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                    saveData = LoadFromEncryptedJSON(fileName);
                    break;
                case SaveFormat.PlainJSON:
                    saveData = LoadFromPlainJSON(fileName);
                    break;
                case SaveFormat.PlayerPrefs:
                    saveData = LoadFromPlayerPrefs(slot);
                    break;
                case SaveFormat.Binary:
                    saveData = LoadFromBinary(fileName);
                    break;
            }
            
            if (saveData == null)
            {
                Debug.LogWarning($"[SaveManager] No save data found in slot {slot}");
                return false;
            }
            
            ApplySaveData(saveData);
            Debug.Log($"[SaveManager] Game loaded from slot {slot} - {saveData.saveTime}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查指定槽位是否有存档
    /// </summary>
    public bool HasSaveData(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS) return false;
        
        switch (_saveFormat)
        {
            case SaveFormat.EncryptedJSON:
            case SaveFormat.PlainJSON:
            case SaveFormat.Binary:
                return FileHelper.FileExists(GetSaveFileName(slot));
            case SaveFormat.PlayerPrefs:
                return PlayerPrefs.HasKey(GetPlayerPrefsKey(slot));
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 获取存档信息（不加载游戏）
    /// </summary>
    public GameSaveData GetSaveInfo(int slot = 0)
    {
        if (!HasSaveData(slot)) return null;
        
        try
        {
            string fileName = GetSaveFileName(slot);
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                    return LoadFromEncryptedJSON(fileName);
                case SaveFormat.PlainJSON:
                    return LoadFromPlainJSON(fileName);
                case SaveFormat.PlayerPrefs:
                    return LoadFromPlayerPrefs(slot);
                case SaveFormat.Binary:
                    return LoadFromBinary(fileName);
                default:
                    return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    public bool DeleteSaveData(int slot = 0)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS) return false;
        
        try
        {
            switch (_saveFormat)
            {
                case SaveFormat.EncryptedJSON:
                case SaveFormat.PlainJSON:
                case SaveFormat.Binary:
                    return FileHelper.DeleteFile(GetSaveFileName(slot));
                case SaveFormat.PlayerPrefs:
                    PlayerPrefs.DeleteKey(GetPlayerPrefsKey(slot));
                    PlayerPrefs.Save();
                    return true;
                default:
                    return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to delete save data: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 自动保存
    /// </summary>
    private void AutoSave()
    {
        SaveGame(0); // 自动保存到槽位0
        Debug.Log("[SaveManager] Auto save completed");
    }
    
    /// <summary>
    /// 创建备份
    /// </summary>
    private void CreateBackup()
    {
        try
        {
            string originalFile = GetSaveFileName(0);
            if (_saveFormat == SaveFormat.PlayerPrefs)
            {
                // PlayerPrefs备份需要特殊处理
                string backupKey = GetPlayerPrefsKey(0) + "_backup";
                string originalData = PlayerPrefs.GetString(GetPlayerPrefsKey(0));
                PlayerPrefs.SetString(backupKey, originalData);
                PlayerPrefs.Save();
            }
            else
            {
                string originalPath = System.IO.Path.Combine(FileHelper.GetSaveDirectory(), originalFile);
                string backupPath = System.IO.Path.Combine(FileHelper.GetSaveDirectory(), BACKUP_SAVE_FILE);
                
                if (System.IO.File.Exists(originalPath))
                {
                    System.IO.File.Copy(originalPath, backupPath, true);
                }
            }
            Debug.Log("[SaveManager] Backup created");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to create backup: {e.Message}");
        }
    }
    
    // === 不同格式的保存方法 ===
    
    private bool SaveAsEncryptedJSON(string fileName, GameSaveData saveData)
    {
        string jsonData = JsonUtility.ToJson(saveData, true);
        return FileHelper.WriteFile(fileName, jsonData, true);
    }
    
    private bool SaveAsPlainJSON(string fileName, GameSaveData saveData)
    {
        string jsonData = JsonUtility.ToJson(saveData, true);
        return FileHelper.WriteFile(fileName, jsonData, false);
    }
    
    private bool SaveAsPlayerPrefs(int slot, GameSaveData saveData)
    {
        string jsonData = JsonUtility.ToJson(saveData, false);
        string key = GetPlayerPrefsKey(slot);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.Save();
        return true;
    }
    
    private bool SaveAsBinary(string fileName, GameSaveData saveData)
    {
        // 注意：Unity 2018+建议使用JsonUtility而不是BinaryFormatter
        // 这里提供一个简化的二进制实现
        string jsonData = JsonUtility.ToJson(saveData, false);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
        string base64Data = Convert.ToBase64String(bytes);
        return FileHelper.WriteFile(fileName, base64Data, false);
    }
    
    // === 不同格式的加载方法 ===
    
    private GameSaveData LoadFromEncryptedJSON(string fileName)
    {
        string jsonData = FileHelper.ReadFile(fileName, true);
        if (string.IsNullOrEmpty(jsonData)) return null;
```

---

**User**

就用 PlayerPrefs方案

---

**Cursor**

基于您的选择，我为您设计一个完整的基于PlayerPrefs的存档系统。PlayerPrefs是Unity内置的跨平台本地存储方案，简单可靠，非常适合存储游戏进度数据。

## 基于PlayerPrefs的完整存档功能框架

### 1. 存档数据结构类

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档数据 - 基于PlayerPrefs的轻量级存档方案
/// </summary>
[System.Serializable]
public class GameSaveData
{   
    [Header("时间系统")]
    public int clockDay = 1;                    // 当前天数
    public float dayProgress = 0f;              // 当天进度 (0.0-1.0)
    public TimeOfDay currentTimeOfDay = TimeOfDay.Day; // 当前时间段
    
    [Header("背包系统")]
    public List<SavePackageItem> packageItems = new List<SavePackageItem>(); // 背包道具列表
    
    [Header("玩家数据")]
    public float currentHealth = 100f;          // 当前血量
    public Vector3 playerPosition = Vector3.zero; // 玩家位置
    public List<int> equippedItems = new List<int>(); // 已装备的道具ID列表
    
    [Header("存档信息")]
    public string saveTime;                     // 存档时间
    public int saveVersion = 1;                 // 存档版本号
}

/// <summary>
/// 可序列化的背包道具数据
/// </summary>
[System.Serializable]
public class SavePackageItem
{
    public int itemId;
    public int count;
    public float modifyTime;
    
    public SavePackageItem() { }
    
    public SavePackageItem(PackageItem item)
    {
        itemId = item.itemId;
        count = item.count;
        modifyTime = item.modifyTime;
    }
    
    public PackageItem ToPackageItem()
    {
        return new PackageItem(itemId, count) { modifyTime = modifyTime };
    }
}
```

### 2. 核心存档管理器

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档管理器 - 基于PlayerPrefs的本地存档系统
/// 支持多槽位存档，自动保存，版本兼容等功能
/// </summary>
public class SaveManager
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveManager();
            return _instance;
        }
    }
    
    // 存档配置常量
    private const string SAVE_KEY_PREFIX = "GameSave_";
    private const string SAVE_VERSION_KEY = "SaveVersion_";
    private const string SAVE_TIME_KEY = "SaveTime_";
    private const int CURRENT_SAVE_VERSION = 1;
    private const int MAX_SAVE_SLOTS = 5;
    
    // 自动保存设置
    private bool _enableAutoSave = true;
    private float _autoSaveInterval = 300f; // 5分钟
    private float _lastAutoSaveTime;
    
    private SaveManager() 
    {
        _lastAutoSaveTime = Time.time;
    }
    
    /// <summary>
    /// 更新自动保存逻辑 - 需要在GameManager中定期调用
    /// </summary>
    public void Update()
    {
        if (_enableAutoSave && Time.time - _lastAutoSaveTime >= _autoSaveInterval)
        {
            AutoSave();
            _lastAutoSaveTime = Time.time;
        }
    }
    
    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    /// <param name="slot">存档槽位 (0-4)</param>
    /// <returns>保存是否成功</returns>
    public bool SaveGame(int slot = 0)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}. Must be 0-{MAX_SAVE_SLOTS - 1}");
            return false;
        }
        
        try
        {
            GameSaveData saveData = CreateSaveData();
            string jsonData = JsonUtility.ToJson(saveData, false);
            
            // 保存主要数据
            string saveKey = GetSaveKey(slot);
            PlayerPrefs.SetString(saveKey, jsonData);
            
            // 保存版本信息
            PlayerPrefs.SetInt(GetVersionKey(slot), CURRENT_SAVE_VERSION);
            
            // 保存时间戳
            PlayerPrefs.SetString(GetTimeKey(slot), saveData.saveTime);
            
            // 立即写入磁盘
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveManager] Game saved to slot {slot} at {saveData.saveTime}");
            
            // 发送保存完成事件
            EventManager.Instance.Publish(new GameSavedEvent(slot, saveData.saveTime));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save game to slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 从指定槽位加载游戏
    /// </summary>
    /// <param name="slot">存档槽位 (0-4)</param>
    /// <returns>加载是否成功</returns>
    public bool LoadGame(int slot = 0)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
            return false;
        }
        
        if (!HasSaveData(slot))
        {
            Debug.LogWarning($"[SaveManager] No save data found in slot {slot}");
            return false;
        }
        
        try
        {
            string saveKey = GetSaveKey(slot);
            string jsonData = PlayerPrefs.GetString(saveKey);
            
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError($"[SaveManager] Save data is empty in slot {slot}");
                return false;
            }
            
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            if (saveData == null)
            {
                Debug.LogError($"[SaveManager] Failed to parse save data in slot {slot}");
                return false;
            }
            
            // 版本兼容性检查
            int saveVersion = PlayerPrefs.GetInt(GetVersionKey(slot), 1);
            if (saveVersion > CURRENT_SAVE_VERSION)
            {
                Debug.LogWarning($"[SaveManager] Save version {saveVersion} is newer than current {CURRENT_SAVE_VERSION}");
            }
            
            ApplySaveData(saveData);
            Debug.Log($"[SaveManager] Game loaded from slot {slot} - {saveData.saveTime}");
            
            // 发送加载完成事件
            EventManager.Instance.Publish(new GameLoadedEvent(slot, saveData.saveTime));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load game from slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查指定槽位是否有存档数据
    /// </summary>
    public bool HasSaveData(int slot = 0)
    {
        if (!IsValidSlot(slot)) return false;
        return PlayerPrefs.HasKey(GetSaveKey(slot));
    }
    
    /// <summary>
    /// 获取存档信息（不加载游戏）
    /// </summary>
    public SaveInfo GetSaveInfo(int slot = 0)
    {
        if (!HasSaveData(slot)) return null;
        
        try
        {
            string saveTime = PlayerPrefs.GetString(GetTimeKey(slot), "Unknown");
            int saveVersion = PlayerPrefs.GetInt(GetVersionKey(slot), 1);
            string jsonData = PlayerPrefs.GetString(GetSaveKey(slot));
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            return new SaveInfo
            {
                slot = slot,
                saveTime = saveTime,
                saveVersion = saveVersion,
                clockDay = saveData?.clockDay ?? 1,
                playerHealth = saveData?.currentHealth ?? 100f,
                itemCount = saveData?.packageItems?.Count ?? 0
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to get save info for slot {slot}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 获取所有存档槽位的信息
    /// </summary>
    public List<SaveInfo> GetAllSaveInfo()
    {
        List<SaveInfo> saveInfos = new List<SaveInfo>();
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            SaveInfo info = GetSaveInfo(i);
            saveInfos.Add(info); // null值也添加，表示空槽位
        }
        return saveInfos;
    }
    
    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    public bool DeleteSaveData(int slot = 0)
    {
        if (!IsValidSlot(slot)) return false;
        
        try
        {
            PlayerPrefs.DeleteKey(GetSaveKey(slot));
            PlayerPrefs.DeleteKey(GetVersionKey(slot));
            PlayerPrefs.DeleteKey(GetTimeKey(slot));
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveManager] Save data deleted from slot {slot}");
            
            // 发送删除完成事件
            EventManager.Instance.Publish(new GameSaveDeletedEvent(slot));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to delete save data from slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 清空所有存档数据
    /// </summary>
    public void ClearAllSaveData()
    {
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            DeleteSaveData(i);
        }
        Debug.Log("[SaveManager] All save data cleared");
    }
    
    /// <summary>
    /// 设置自动保存
    /// </summary>
    public void SetAutoSave(bool enabled, float intervalSeconds = 300f)
    {
        _enableAutoSave = enabled;
        _autoSaveInterval = intervalSeconds;
        Debug.Log($"[SaveManager] Auto save: {enabled}, Interval: {intervalSeconds}s");
    }
    
    /// <summary>
    /// 执行自动保存
    /// </summary>
    private void AutoSave()
    {
        if (SaveGame(0)) // 自动保存到槽位0
        {
            Debug.Log("[SaveManager] Auto save completed");
        }
        _lastAutoSaveTime = Time.time;
    }
    
    /// <summary>
    /// 创建当前游戏状态的存档数据
    /// </summary>
    private GameSaveData CreateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        
        // 保存时间系统数据
        var clockModel = ClockModel.Instance;
        saveData.clockDay = clockModel.ClockDay;
        saveData.dayProgress = clockModel.DayProgress;
        saveData.currentTimeOfDay = clockModel.CurrentTimeOfDay;
        
        // 保存背包数据
        var packageModel = PackageModel.Instance;
        saveData.packageItems.Clear();
        foreach (var item in packageModel.GetAllItems())
        {
            saveData.packageItems.Add(new SavePackageItem(item));
        }
        
        // 保存玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            saveData.currentHealth = player.CurrentHealth;
            saveData.playerPosition = player.transform.position;
            // saveData.equippedItems = GetPlayerEquippedItems(player); // 需要实现
        }
        
        // 保存存档信息
        saveData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        saveData.saveVersion = CURRENT_SAVE_VERSION;
        
        return saveData;
    }
    
    /// <summary>
    /// 应用存档数据到游戏系统
    /// </summary>
    private void ApplySaveData(GameSaveData saveData)
    {
        // 应用时间系统数据（需要为ClockModel添加设置方法）
        // ClockModel.Instance.SetGameTime(saveData.clockDay, saveData.dayProgress, saveData.currentTimeOfDay);
        
        // 应用背包数据
        var packageModel = PackageModel.Instance;
        // packageModel.ClearAllItems(); // 需要实现
        foreach (var saveItem in saveData.packageItems)
        {
            packageModel.AddItem(saveItem.itemId, saveItem.count);
        }
        
        // 应用玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            // player.SetHealth(saveData.currentHealth); // 需要实现
            player.transform.position = saveData.playerPosition;
            
            // 重新装备道具
            foreach (int equipId in saveData.equippedItems)
            {
                player.Equip(equipId);
            }
        }
    }
    
    // === 辅助方法 ===
    
    private bool IsValidSlot(int slot)
    {
        return slot >= 0 && slot < MAX_SAVE_SLOTS;
    }
    
    private string GetSaveKey(int slot)
    {
        return $"{SAVE_KEY_PREFIX}{slot}";
    }
    
    private string GetVersionKey(int slot)
    {
        return $"{SAVE_VERSION_KEY}{slot}";
    }
    
    private string GetTimeKey(int slot)
    {
        return $"{SAVE_TIME_KEY}{slot}";
    }
}
```

### 3. 存档信息类

```csharp
/// <summary>
/// 存档信息 - 用于显示存档列表而不完整加载游戏数据
/// </summary>
[System.Serializable]
public class SaveInfo
{
    public int slot;              // 存档槽位
    public string saveTime;       // 存档时间
    public int saveVersion;       // 存档版本
    public int clockDay;          // 游戏天数
    public float playerHealth;    // 玩家血量
    public int itemCount;         // 道具数量
    
    /// <summary>
    /// 是否为空槽位
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(saveTime) || saveTime == "Unknown";
    
    /// <summary>
    /// 获取显示文本
    /// </summary>
    public string GetDisplayText()
    {
        if (IsEmpty)
            return $"槽位 {slot + 1}: 空";
        
        return $"槽位 {slot + 1}: 第{clockDay}天 - {saveTime}";
    }
}
```

### 4. 存档相关事件

```csharp
// 需要添加到 Assets/Scripts/Core/Event/GameEvents.cs 中

/// <summary>
/// 游戏保存完成事件
/// </summary>
public class GameSavedEvent : IEvent
{
    public int Slot { get; }
    public string SaveTime { get; }
    
    public GameSavedEvent(int slot, string saveTime)
    {
        Slot = slot;
        SaveTime = saveTime;
    }
}

/// <summary>
/// 游戏加载完成事件
/// </summary>
public class GameLoadedEvent : IEvent
{
    public int Slot { get; }
    public string SaveTime { get; }
    
    public GameLoadedEvent(int slot, string saveTime)
    {
        Slot = slot;
        SaveTime = saveTime;
    }
}

/// <summary>
/// 存档删除事件
/// </summary>
public class GameSaveDeletedEvent : IEvent
{
    public int Slot { get; }
    
    public GameSaveDeletedEvent(int slot)
    {
        Slot = slot;
    }
}
```

### 5. 使用示例

```csharp
/// <summary>
/// 游戏管理器 - 展示存档系统的使用方法
/// 此脚本需要挂载到GameObject上使用
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("存档设置")]
    [SerializeField] private bool _autoLoadOnStart = true;
    [SerializeField] private bool _autoSaveOnQuit = true;
    
    void Start()
    {
        // 游戏启动时自动加载存档
        if (_autoLoadOnStart && SaveManager.Instance.HasSaveData(0))
        {
            SaveManager.Instance.LoadGame(0);
        }
        
        // 设置自动保存（5分钟间隔）
        SaveManager.Instance.SetAutoSave(true, 300f);
    }
    
    void Update()
    {
        // 更新自动保存逻辑
        SaveManager.Instance.Update();
        
        // 快捷键操作
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveManager.Instance.SaveGame(0);
            Debug.Log("手动保存完成");
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveManager.Instance.LoadGame(0);
            Debug.Log("手动加载完成");
        }
        
        // 显示所有存档信息（测试用）
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowAllSaveInfo();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // 应用暂停时自动保存
        if (pauseStatus && _autoSaveOnQuit)
        {
            SaveManager.Instance.SaveGame(0);
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        // 应用失去焦点时自动保存
        if (!hasFocus && _autoSaveOnQuit)
        {
            SaveManager.Instance.SaveGame(0);
        }
    }
    
    /// <summary>
    /// 显示所有存档信息（调试用）
    /// </summary>
    private void ShowAllSaveInfo()
    {
        var saveInfos = SaveManager.Instance.GetAllSaveInfo();
        Debug.Log("=== 存档信息 ===");
        foreach (var info in saveInfos)
        {
            if (info != null)
            {
                Debug.Log(info.GetDisplayText());
            }
        }
    }
}
```

### 6. 需要为现有系统添加的方法

**为`ClockModel.cs`添加：**
```12:15:Assets/Scripts/UI/Clock/ClockModel.cs
// ... existing code ...
/// <summary>
/// 设置游戏时间 - 用于加载存档
/// </summary>
public void SetGameTime(int day, float progress, TimeOfDay timeOfDay)
{
    _clockDay = Mathf.Clamp(day, 1, _maxDays);
    _dayProgress = Mathf.Clamp01(progress);
    _currentTimeOfDay = timeOfDay;
    UpdateTargetAmbientIntensity();
    RenderSettings.ambientIntensity = _targetAmbientIntensity;
}
// ... existing code ...
```

**为`PackageModel.cs`添加：**
```50:55:Assets/Scripts/UI/Package/PackageModel.cs
// ... existing code ...
/// <summary>
/// 清空所有道具 - 用于加载存档
/// </summary>
public void ClearAllItems()
{
    _packageItems.Clear();
}
// ... existing code ...
```

## 框架特点

1. **简单可靠**：基于Unity内置PlayerPrefs，跨平台兼容
2. **多槽位支持**：支持5个独立存档槽位
3. **自动保存**：可配置的自动保存功能
4. **版本管理**：支持存档版本控制和兼容性检查
5. **事件通知**：完整的事件系统集成
6. **错误处理**：完善的异常处理和日志记录
7. **易于扩展**：遵循项目架构规范，便于后期扩展

这个基于PlayerPrefs的存档系统完全满足您存储时间天数和背包道具数量的需求，同时提供了完整的存档管理功能。

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档数据 - 基于PlayerPrefs的轻量级存档方案
/// </summary>
[System.Serializable]
public class GameSaveData
{   
    [Header("时间系统")]
    public int clockDay = 1;                    // 当前天数
    public float dayProgress = 0f;              // 当天进度 (0.0-1.0)
    public TimeOfDay currentTimeOfDay = TimeOfDay.Day; // 当前时间段
    
    [Header("背包系统")]
    public List<SavePackageItem> packageItems = new List<SavePackageItem>(); // 背包道具列表
    
    [Header("玩家数据")]
    public float currentHealth = 100f;          // 当前血量
    public Vector3 playerPosition = Vector3.zero; // 玩家位置
    public List<int> equippedItems = new List<int>(); // 已装备的道具ID列表
    
    [Header("存档信息")]
    public string saveTime;                     // 存档时间
    public int saveVersion = 1;                 // 存档版本号
}

/// <summary>
/// 可序列化的背包道具数据
/// </summary>
[System.Serializable]
public class SavePackageItem
{
    public int itemId;
    public int count;
    public float modifyTime;
    
    public SavePackageItem() { }
    
    public SavePackageItem(PackageItem item)
    {
        itemId = item.itemId;
        count = item.count;
        modifyTime = item.modifyTime;
    }
    
    public PackageItem ToPackageItem()
    {
        return new PackageItem(itemId, count) { modifyTime = modifyTime };
    }
}
```

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档管理器 - 基于PlayerPrefs的本地存档系统
/// 支持多槽位存档，自动保存，版本兼容等功能
/// </summary>
public class SaveManager
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveManager();
            return _instance;
        }
    }
    
    // 存档配置常量
    private const string SAVE_KEY_PREFIX = "GameSave_";
    private const string SAVE_VERSION_KEY = "SaveVersion_";
    private const string SAVE_TIME_KEY = "SaveTime_";
    private const int CURRENT_SAVE_VERSION = 1;
    private const int MAX_SAVE_SLOTS = 5;
    
    // 自动保存设置
    private bool _enableAutoSave = true;
    private float _autoSaveInterval = 300f; // 5分钟
    private float _lastAutoSaveTime;
    
    private SaveManager() 
    {
        _lastAutoSaveTime = Time.time;
    }
    
    /// <summary>
    /// 更新自动保存逻辑 - 需要在GameManager中定期调用
    /// </summary>
    public void Update()
    {
        if (_enableAutoSave && Time.time - _lastAutoSaveTime >= _autoSaveInterval)
        {
            AutoSave();
            _lastAutoSaveTime = Time.time;
        }
    }
    
    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    /// <param name="slot">存档槽位 (0-4)</param>
    /// <returns>保存是否成功</returns>
    public bool SaveGame(int slot = 0)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}. Must be 0-{MAX_SAVE_SLOTS - 1}");
            return false;
        }
        
        try
        {
            GameSaveData saveData = CreateSaveData();
            string jsonData = JsonUtility.ToJson(saveData, false);
            
            // 保存主要数据
            string saveKey = GetSaveKey(slot);
            PlayerPrefs.SetString(saveKey, jsonData);
            
            // 保存版本信息
            PlayerPrefs.SetInt(GetVersionKey(slot), CURRENT_SAVE_VERSION);
            
            // 保存时间戳
            PlayerPrefs.SetString(GetTimeKey(slot), saveData.saveTime);
            
            // 立即写入磁盘
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveManager] Game saved to slot {slot} at {saveData.saveTime}");
            
            // 发送保存完成事件
            EventManager.Instance.Publish(new GameSavedEvent(slot, saveData.saveTime));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save game to slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 从指定槽位加载游戏
    /// </summary>
    /// <param name="slot">存档槽位 (0-4)</param>
    /// <returns>加载是否成功</returns>
    public bool LoadGame(int slot = 0)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
            return false;
        }
        
        if (!HasSaveData(slot))
        {
            Debug.LogWarning($"[SaveManager] No save data found in slot {slot}");
            return false;
        }
        
        try
        {
            string saveKey = GetSaveKey(slot);
            string jsonData = PlayerPrefs.GetString(saveKey);
            
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError($"[SaveManager] Save data is empty in slot {slot}");
                return false;
            }
            
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            if (saveData == null)
            {
                Debug.LogError($"[SaveManager] Failed to parse save data in slot {slot}");
                return false;
            }
            
            // 版本兼容性检查
            int saveVersion = PlayerPrefs.GetInt(GetVersionKey(slot), 1);
            if (saveVersion > CURRENT_SAVE_VERSION)
            {
                Debug.LogWarning($"[SaveManager] Save version {saveVersion} is newer than current {CURRENT_SAVE_VERSION}");
            }
            
            ApplySaveData(saveData);
            Debug.Log($"[SaveManager] Game loaded from slot {slot} - {saveData.saveTime}");
            
            // 发送加载完成事件
            EventManager.Instance.Publish(new GameLoadedEvent(slot, saveData.saveTime));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load game from slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查指定槽位是否有存档数据
    /// </summary>
    public bool HasSaveData(int slot = 0)
    {
        if (!IsValidSlot(slot)) return false;
        return PlayerPrefs.HasKey(GetSaveKey(slot));
    }
    
    /// <summary>
    /// 获取存档信息（不加载游戏）
    /// </summary>
    public SaveInfo GetSaveInfo(int slot = 0)
    {
        if (!HasSaveData(slot)) return null;
        
        try
        {
            string saveTime = PlayerPrefs.GetString(GetTimeKey(slot), "Unknown");
            int saveVersion = PlayerPrefs.GetInt(GetVersionKey(slot), 1);
            string jsonData = PlayerPrefs.GetString(GetSaveKey(slot));
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            return new SaveInfo
            {
                slot = slot,
                saveTime = saveTime,
                saveVersion = saveVersion,
                clockDay = saveData?.clockDay ?? 1,
                playerHealth = saveData?.currentHealth ?? 100f,
                itemCount = saveData?.packageItems?.Count ?? 0
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to get save info for slot {slot}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 获取所有存档槽位的信息
    /// </summary>
    public List<SaveInfo> GetAllSaveInfo()
    {
        List<SaveInfo> saveInfos = new List<SaveInfo>();
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            SaveInfo info = GetSaveInfo(i);
            saveInfos.Add(info); // null值也添加，表示空槽位
        }
        return saveInfos;
    }
    
    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    public bool DeleteSaveData(int slot = 0)
    {
        if (!IsValidSlot(slot)) return false;
        
        try
        {
            PlayerPrefs.DeleteKey(GetSaveKey(slot));
            PlayerPrefs.DeleteKey(GetVersionKey(slot));
            PlayerPrefs.DeleteKey(GetTimeKey(slot));
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveManager] Save data deleted from slot {slot}");
            
            // 发送删除完成事件
            EventManager.Instance.Publish(new GameSaveDeletedEvent(slot));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to delete save data from slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 清空所有存档数据
    /// </summary>
    public void ClearAllSaveData()
    {
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            DeleteSaveData(i);
        }
        Debug.Log("[SaveManager] All save data cleared");
    }
    
    /// <summary>
    /// 设置自动保存
    /// </summary>
    public void SetAutoSave(bool enabled, float intervalSeconds = 300f)
    {
        _enableAutoSave = enabled;
        _autoSaveInterval = intervalSeconds;
        Debug.Log($"[SaveManager] Auto save: {enabled}, Interval: {intervalSeconds}s");
    }
    
    /// <summary>
    /// 执行自动保存
    /// </summary>
    private void AutoSave()
    {
        if (SaveGame(0)) // 自动保存到槽位0
        {
            Debug.Log("[SaveManager] Auto save completed");
        }
        _lastAutoSaveTime = Time.time;
    }
    
    /// <summary>
    /// 创建当前游戏状态的存档数据
    /// </summary>
    private GameSaveData CreateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        
        // 保存时间系统数据
        var clockModel = ClockModel.Instance;
        saveData.clockDay = clockModel.ClockDay;
        saveData.dayProgress = clockModel.DayProgress;
        saveData.currentTimeOfDay = clockModel.CurrentTimeOfDay;
        
        // 保存背包数据
        var packageModel = PackageModel.Instance;
        saveData.packageItems.Clear();
        foreach (var item in packageModel.GetAllItems())
        {
            saveData.packageItems.Add(new SavePackageItem(item));
        }
        
        // 保存玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            saveData.currentHealth = player.CurrentHealth;
            saveData.playerPosition = player.transform.position;
            // saveData.equippedItems = GetPlayerEquippedItems(player); // 需要实现
        }
        
        // 保存存档信息
        saveData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        saveData.saveVersion = CURRENT_SAVE_VERSION;
        
        return saveData;
    }
    
    /// <summary>
    /// 应用存档数据到游戏系统
    /// </summary>
    private void ApplySaveData(GameSaveData saveData)
    {
        // 应用时间系统数据（需要为ClockModel添加设置方法）
        // ClockModel.Instance.SetGameTime(saveData.clockDay, saveData.dayProgress, saveData.currentTimeOfDay);
        
        // 应用背包数据
        var packageModel = PackageModel.Instance;
        // packageModel.ClearAllItems(); // 需要实现
        foreach (var saveItem in saveData.packageItems)
        {
            packageModel.AddItem(saveItem.itemId, saveItem.count);
        }
        
        // 应用玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            // player.SetHealth(saveData.currentHealth); // 需要实现
            player.transform.position = saveData.playerPosition;
            
            // 重新装备道具
            foreach (int equipId in saveData.equippedItems)
            {
                player.Equip(equipId);
            }
        }
    }
    
    // === 辅助方法 ===
    
    private bool IsValidSlot(int slot)
    {
        return slot >= 0 && slot < MAX_SAVE_SLOTS;
    }
    
    private string GetSaveKey(int slot)
    {
        return $"{SAVE_KEY_PREFIX}{slot}";
    }
    
    private string GetVersionKey(int slot)
    {
        return $"{SAVE_VERSION_KEY}{slot}";
    }
    
    private string GetTimeKey(int slot)
    {
        return $"{SAVE_TIME_KEY}{slot}";
    }
}
```

```csharp
/// <summary>
/// 存档信息 - 用于显示存档列表而不完整加载游戏数据
/// </summary>
[System.Serializable]
public class SaveInfo
{
    public int slot;              // 存档槽位
    public string saveTime;       // 存档时间
    public int saveVersion;       // 存档版本
    public int clockDay;          // 游戏天数
    public float playerHealth;    // 玩家血量
    public int itemCount;         // 道具数量
    
    /// <summary>
    /// 是否为空槽位
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(saveTime) || saveTime == "Unknown";
    
    /// <summary>
    /// 获取显示文本
    /// </summary>
    public string GetDisplayText()
    {
        if (IsEmpty)
            return $"槽位 {slot + 1}: 空";
        
        return $"槽位 {slot + 1}: 第{clockDay}天 - {saveTime}";
    }
}
```

```csharp
// 需要添加到 Assets/Scripts/Core/Event/GameEvents.cs 中

/// <summary>
/// 游戏保存完成事件
/// </summary>
public class GameSavedEvent : IEvent
{
    public int Slot { get; }
    public string SaveTime { get; }
    
    public GameSavedEvent(int slot, string saveTime)
    {
        Slot = slot;
        SaveTime = saveTime;
    }
}

/// <summary>
/// 游戏加载完成事件
/// </summary>
public class GameLoadedEvent : IEvent
{
    public int Slot { get; }
    public string SaveTime { get; }
    
    public GameLoadedEvent(int slot, string saveTime)
    {
        Slot = slot;
        SaveTime = saveTime;
    }
}

/// <summary>
/// 存档删除事件
/// </summary>
public class GameSaveDeletedEvent : IEvent
{
    public int Slot { get; }
    
    public GameSaveDeletedEvent(int slot)
    {
        Slot = slot;
    }
}
```

```csharp
/// <summary>
/// 游戏管理器 - 展示存档系统的使用方法
/// 此脚本需要挂载到GameObject上使用
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("存档设置")]
    [SerializeField] private bool _autoLoadOnStart = true;
    [SerializeField] private bool _autoSaveOnQuit = true;
    
    void Start()
    {
        // 游戏启动时自动加载存档
        if (_autoLoadOnStart && SaveManager.Instance.HasSaveData(0))
        {
            SaveManager.Instance.LoadGame(0);
        }
        
        // 设置自动保存（5分钟间隔）
        SaveManager.Instance.SetAutoSave(true, 300f);
    }
    
    void Update()
    {
        // 更新自动保存逻辑
        SaveManager.Instance.Update();
        
        // 快捷键操作
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveManager.Instance.SaveGame(0);
            Debug.Log("手动保存完成");
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveManager.Instance.LoadGame(0);
            Debug.Log("手动加载完成");
        }
        
        // 显示所有存档信息（测试用）
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowAllSaveInfo();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // 应用暂停时自动保存
        if (pauseStatus && _autoSaveOnQuit)
        {
            SaveManager.Instance.SaveGame(0);
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        // 应用失去焦点时自动保存
        if (!hasFocus && _autoSaveOnQuit)
        {
            SaveManager.Instance.SaveGame(0);
        }
    }
    
    /// <summary>
    /// 显示所有存档信息（调试用）
    /// </summary>
    private void ShowAllSaveInfo()
    {
        var saveInfos = SaveManager.Instance.GetAllSaveInfo();
        Debug.Log("=== 存档信息 ===");
        foreach (var info in saveInfos)
        {
            if (info != null)
            {
                Debug.Log(info.GetDisplayText());
            }
        }
    }
}
```

```plaintext
// ... existing code ...
/// <summary>
/// 设置游戏时间 - 用于加载存档
/// </summary>
public void SetGameTime(int day, float progress, TimeOfDay timeOfDay)
{
    _clockDay = Mathf.Clamp(day, 1, _maxDays);
    _dayProgress = Mathf.Clamp01(progress);
    _currentTimeOfDay = timeOfDay;
    UpdateTargetAmbientIntensity();
    RenderSettings.ambientIntensity = _targetAmbientIntensity;
}
// ... existing code ...
```

```plaintext
// ... existing code ...
/// <summary>
/// 清空所有道具 - 用于加载存档
/// </summary>
public void ClearAllItems()
{
    _packageItems.Clear();
}
// ... existing code ...
```

---

**User**

@project.mdc 按规范生成代码

---

**Cursor**

我将按照项目规范为您生成完整的存档系统代码。首先让我查看项目中现有的架构实现方式作为参考。

```csharp
   private static ConfigManager _instance;
   public static ConfigManager Instance
   {
       get
       {
           if (_instance == null)
               _instance = new ConfigManager();
           return _instance;
       }
   }
```

```csharp
   private static EventManager _instance;
   public static EventManager Instance => _instance ??= new EventManager();
```

现在我按照项目规范为您生成完整的存档系统代码。

## 1. 存档数据结构类

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档数据 - 存储所有需要持久化的游戏进度
/// 包含时间系统、背包系统、玩家数据等核心游戏状态
/// </summary>
[System.Serializable]
public class GameSaveData
{   
    [Header("时间系统")]
    public int clockDay = 1;                    // 当前天数
    public float dayProgress = 0f;              // 当天进度 (0.0-1.0)
    public TimeOfDay currentTimeOfDay = TimeOfDay.Day; // 当前时间段
    
    [Header("背包系统")]
    public List<SavePackageItem> packageItems = new List<SavePackageItem>(); // 背包道具列表
    
    [Header("玩家数据")]
    public float currentHealth = 100f;          // 当前血量
    public Vector3 playerPosition = Vector3.zero; // 玩家位置
    public List<int> equippedItems = new List<int>(); // 已装备的道具ID列表
    
    [Header("存档信息")]
    public string saveTime;                     // 存档时间
    public int saveVersion = 1;                 // 存档版本号
}

/// <summary>
/// 可序列化的背包道具数据
/// 用于在存档中保存PackageItem的信息
/// </summary>
[System.Serializable]
public class SavePackageItem
{
    public int itemId;
    public int count;
    public float modifyTime;
    
    public SavePackageItem() { }
    
    public SavePackageItem(PackageItem item)
    {
        itemId = item.itemId;
        count = item.count;  
        modifyTime = item.modifyTime;
    }
    
    /// <summary>
    /// 转换为PackageItem对象
    /// </summary>
    public PackageItem ToPackageItem()
    {
        return new PackageItem(itemId, count) { modifyTime = modifyTime };
    }
}

/// <summary>
/// 存档信息 - 用于显示存档列表而不完整加载游戏数据
/// 提供存档的基本信息供UI展示使用
/// </summary>
[System.Serializable]
public class SaveInfo
{
    public int slot;              // 存档槽位
    public string saveTime;       // 存档时间
    public int saveVersion;       // 存档版本
    public int clockDay;          // 游戏天数
    public float playerHealth;    // 玩家血量
    public int itemCount;         // 道具数量
    
    /// <summary>
    /// 是否为空槽位
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(saveTime) || saveTime == "Unknown";
    
    /// <summary>
    /// 获取显示文本
    /// </summary>
    public string GetDisplayText()
    {
        if (IsEmpty)
            return $"槽位 {slot + 1}: 空";
        
        return $"槽位 {slot + 1}: 第{clockDay}天 - {saveTime}";
    }
}
```

## 2. 存档管理器

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档管理器 - 基于PlayerPrefs的本地存档系统
/// 提供多槽位存档、自动保存、版本兼容等功能
/// 负责管理游戏进度的保存和加载，支持时间系统和背包系统的数据持久化
/// </summary>
public class SaveManager
{
    private static SaveManager _instance;
    
    /// <summary>
    /// 单例实例
    /// </summary>
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveManager();
            return _instance;
        }
    }
    
    // 存档配置常量
    private const string SAVE_KEY_PREFIX = "GameSave_";
    private const string SAVE_VERSION_KEY = "SaveVersion_";
    private const string SAVE_TIME_KEY = "SaveTime_";
    private const int CURRENT_SAVE_VERSION = 1;
    private const int MAX_SAVE_SLOTS = 5;
    
    // 自动保存设置
    private bool _enableAutoSave = true;
    private float _autoSaveInterval = 300f; // 5分钟
    private float _lastAutoSaveTime;
    
    /// <summary>
    /// 私有构造函数，确保单例模式
    /// </summary>
    private SaveManager() 
    {
        _lastAutoSaveTime = Time.time;
    }
    
    /// <summary>
    /// 更新自动保存逻辑 - 需要在GameManager中定期调用
    /// </summary>
    public void Update()
    {
        if (_enableAutoSave && Time.time - _lastAutoSaveTime >= _autoSaveInterval)
        {
            AutoSave();
            _lastAutoSaveTime = Time.time;
        }
    }
    
    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    /// <param name="slot">存档槽位 (0-4)</param>
    /// <returns>保存是否成功</returns>
    public bool SaveGame(int slot = 0)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}. Must be 0-{MAX_SAVE_SLOTS - 1}");
            return false;
        }
        
        try
        {
            GameSaveData saveData = CreateSaveData();
            string jsonData = JsonUtility.ToJson(saveData, false);
            
            // 保存主要数据
            string saveKey = GetSaveKey(slot);
            PlayerPrefs.SetString(saveKey, jsonData);
            
            // 保存版本信息
            PlayerPrefs.SetInt(GetVersionKey(slot), CURRENT_SAVE_VERSION);
            
            // 保存时间戳
            PlayerPrefs.SetString(GetTimeKey(slot), saveData.saveTime);
            
            // 立即写入磁盘
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveManager] Game saved to slot {slot} at {saveData.saveTime}");
            
            // 发送保存完成事件
            EventManager.Instance.Publish(new GameSavedEvent(slot, saveData.saveTime));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save game to slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 从指定槽位加载游戏
    /// </summary>
    /// <param name="slot">存档槽位 (0-4)</param>
    /// <returns>加载是否成功</returns>
    public bool LoadGame(int slot = 0)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
            return false;
        }
        
        if (!HasSaveData(slot))
        {
            Debug.LogWarning($"[SaveManager] No save data found in slot {slot}");
            return false;
        }
        
        try
        {
            string saveKey = GetSaveKey(slot);
            string jsonData = PlayerPrefs.GetString(saveKey);
            
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError($"[SaveManager] Save data is empty in slot {slot}");
                return false;
            }
            
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            if (saveData == null)
            {
                Debug.LogError($"[SaveManager] Failed to parse save data in slot {slot}");
                return false;
            }
            
            // 版本兼容性检查
            int saveVersion = PlayerPrefs.GetInt(GetVersionKey(slot), 1);
            if (saveVersion > CURRENT_SAVE_VERSION)
            {
                Debug.LogWarning($"[SaveManager] Save version {saveVersion} is newer than current {CURRENT_SAVE_VERSION}");
            }
            
            ApplySaveData(saveData);
            Debug.Log($"[SaveManager] Game loaded from slot {slot} - {saveData.saveTime}");
            
            // 发送加载完成事件
            EventManager.Instance.Publish(new GameLoadedEvent(slot, saveData.saveTime));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load game from slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查指定槽位是否有存档数据
    /// </summary>
    public bool HasSaveData(int slot = 0)
    {
        if (!IsValidSlot(slot)) return false;
        return PlayerPrefs.HasKey(GetSaveKey(slot));
    }
    
    /// <summary>
    /// 获取存档信息（不加载游戏）
    /// </summary>
    public SaveInfo GetSaveInfo(int slot = 0)
    {
        if (!HasSaveData(slot)) return null;
        
        try
        {
            string saveTime = PlayerPrefs.GetString(GetTimeKey(slot), "Unknown");
            int saveVersion = PlayerPrefs.GetInt(GetVersionKey(slot), 1);
            string jsonData = PlayerPrefs.GetString(GetSaveKey(slot));
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            return new SaveInfo
            {
                slot = slot,
                saveTime = saveTime,
                saveVersion = saveVersion,
                clockDay = saveData?.clockDay ?? 1,
                playerHealth = saveData?.currentHealth ?? 100f,
                itemCount = saveData?.packageItems?.Count ?? 0
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to get save info for slot {slot}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 获取所有存档槽位的信息
    /// </summary>
    public List<SaveInfo> GetAllSaveInfo()
    {
        List<SaveInfo> saveInfos = new List<SaveInfo>();
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            SaveInfo info = GetSaveInfo(i);
            saveInfos.Add(info); // null值也添加，表示空槽位
        }
        return saveInfos;
    }
    
    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    public bool DeleteSaveData(int slot = 0)
    {
        if (!IsValidSlot(slot)) return false;
        
        try
        {
            PlayerPrefs.DeleteKey(GetSaveKey(slot));
            PlayerPrefs.DeleteKey(GetVersionKey(slot));
            PlayerPrefs.DeleteKey(GetTimeKey(slot));
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveManager] Save data deleted from slot {slot}");
            
            // 发送删除完成事件
            EventManager.Instance.Publish(new GameSaveDeletedEvent(slot));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to delete save data from slot {slot}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 清空所有存档数据
    /// </summary>
    public void ClearAllSaveData()
    {
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            DeleteSaveData(i);
        }
        Debug.Log("[SaveManager] All save data cleared");
    }
    
    /// <summary>
    /// 设置自动保存
    /// </summary>
    /// <param name="enabled">是否启用自动保存</param>
    /// <param name="intervalSeconds">自动保存间隔（秒）</param>
    public void SetAutoSave(bool enabled, float intervalSeconds = 300f)
    {
        _enableAutoSave = enabled;
        _autoSaveInterval = intervalSeconds;
        Debug.Log($"[SaveManager] Auto save: {enabled}, Interval: {intervalSeconds}s");
    }
    
    /// <summary>
    /// 执行自动保存
    /// </summary>
    private void AutoSave()
    {
        if (SaveGame(0)) // 自动保存到槽位0
        {
            Debug.Log("[SaveManager] Auto save completed");
        }
        _lastAutoSaveTime = Time.time;
    }
    
    /// <summary>
    /// 创建当前游戏状态的存档数据
    /// </summary>
    private GameSaveData CreateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        
        // 保存时间系统数据
        var clockModel = ClockModel.Instance;
        saveData.clockDay = clockModel.ClockDay;
        saveData.dayProgress = clockModel.DayProgress;
        saveData.currentTimeOfDay = clockModel.CurrentTimeOfDay;
        
        // 保存背包数据
        var packageModel = PackageModel.Instance;
        saveData.packageItems.Clear();
        foreach (var item in packageModel.GetAllItems())
        {
            saveData.packageItems.Add(new SavePackageItem(item));
        }
        
        // 保存玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            saveData.currentHealth = player.CurrentHealth;
            saveData.playerPosition = player.transform.position;
            // 注意：装备系统需要后续实现GetEquippedItemIds方法
            // saveData.equippedItems = GetPlayerEquippedItems(player);
        }
        
        // 保存存档信息
        saveData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        saveData.saveVersion = CURRENT_SAVE_VERSION;
        
        return saveData;
    }
    
    /// <summary>
    /// 应用存档数据到游戏系统
    /// </summary>
    private void ApplySaveData(GameSaveData saveData)
    {
        // 应用时间系统数据（需要为ClockModel添加设置方法）
        // ClockModel.Instance.SetGameTime(saveData.clockDay, saveData.dayProgress, saveData.currentTimeOfDay);
        
        // 应用背包数据
        var packageModel = PackageModel.Instance;
        // 注意：需要为PackageModel添加ClearAllItems方法
        // packageModel.ClearAllItems();
        foreach (var saveItem in saveData.packageItems)
        {
            packageModel.AddItem(saveItem.itemId, saveItem.count);
        }
        
        // 应用玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            // 注意：需要为Player添加SetHealth方法
            // player.SetHealth(saveData.currentHealth);
            player.transform.position = saveData.playerPosition;
            
            // 重新装备道具
            foreach (int equipId in saveData.equippedItems)
            {
                player.Equip(equipId);
            }
        }
    }
    
    // === 辅助方法 ===
    
    /// <summary>
    /// 验证槽位有效性
    /// </summary>
    private bool IsValidSlot(int slot)
    {
        return slot >= 0 && slot < MAX_SAVE_SLOTS;
    }
    
    /// <summary>
    /// 获取存档键名
    /// </summary>
    private string GetSaveKey(int slot)
    {
        return $"{SAVE_KEY_PREFIX}{slot}";
    }
    
    /// <summary>
    /// 获取版本键名
    /// </summary>
    private string GetVersionKey(int slot)
    {
        return $"{SAVE_VERSION_KEY}{slot}";
    }
    
    /// <summary>
    /// 获取时间键名
    /// </summary>
    private string GetTimeKey(int slot)
    {
        return $"{SAVE_TIME_KEY}{slot}";
    }
}
```

## 3. 添加存档相关事件

```csharp
/// <summary>
/// 制作菜单打开事件
/// </summary>
public class MakeMenuOpenEvent : IEvent
{
    public int TypeId { get; }

    public MakeMenuOpenEvent(int typeId)
    {
        TypeId = typeId;
    }
}

->

/// <summary>
/// 制作菜单打开事件
/// </summary>
public class MakeMenuOpenEvent : IEvent
{
    public int TypeId { get; }

    public MakeMenuOpenEvent(int typeId)
    {
        TypeId = typeId;
    }
}

/// <summary>
/// 游戏保存完成事件
/// </summary>
public class GameSavedEvent : IEvent
{
    public int Slot { get; }
    public string SaveTime { get; }
    
    public GameSavedEvent(int slot, string saveTime)
    {
        Slot = slot;
        SaveTime = saveTime;
    }
}

/// <summary>
/// 游戏加载完成事件
/// </summary>
public class GameLoadedEvent : IEvent
{
    public int Slot { get; }
    public string SaveTime { get; }
    
    public GameLoadedEvent(int slot, string saveTime)
    {
        Slot = slot;
        SaveTime = saveTime;
    }
}

/// <summary>
/// 存档删除事件
/// </summary>
public class GameSaveDeletedEvent : IEvent
{
    public int Slot { get; }
    
    public GameSaveDeletedEvent(int slot)
    {
        Slot = slot;
    }
}
```

## 4. MonoBehaviour控制器组件

```csharp
using UnityEngine;

/// <summary>
/// 游戏存档控制器 - MonoBehaviour组件
/// 负责驱动SaveManager的更新逻辑和提供存档快捷键操作
/// 需要挂载到GameObject上使用
/// </summary>
public class GameSaveController : MonoBehaviour
{
    [Header("存档设置")]
    [SerializeField] private bool _autoLoadOnStart = true;
    [SerializeField] private bool _autoSaveOnQuit = true;
    [SerializeField] [Range(60f, 600f)] private float _autoSaveInterval = 300f; // 5分钟
    
    [Header("快捷键设置")]
    [SerializeField] private KeyCode _quickSaveKey = KeyCode.F5;
    [SerializeField] private KeyCode _quickLoadKey = KeyCode.F9;
    [SerializeField] private KeyCode _showSaveInfoKey = KeyCode.F1;
    
    void Start()
    {
        // 初始化SaveManager
        var saveManager = SaveManager.Instance;
        
        // 设置自动保存
        saveManager.SetAutoSave(true, _autoSaveInterval);
        
        // 游戏启动时自动加载存档
        if (_autoLoadOnStart && saveManager.HasSaveData(0))
        {
            bool loadSuccess = saveManager.LoadGame(0);
            Debug.Log($"[GameSaveController] Auto load on start: {(loadSuccess ? "Success" : "Failed")}");
        }
        
        // 订阅存档事件
        EventManager.Instance.Subscribe<GameSavedEvent>(OnGameSaved);
        EventManager.Instance.Subscribe<GameLoadedEvent>(OnGameLoaded);
        EventManager.Instance.Subscribe<GameSaveDeletedEvent>(OnGameSaveDeleted);
    }
    
    void Update()
    {
        // 驱动SaveManager的自动保存逻辑
        SaveManager.Instance.Update();
        
        // 处理快捷键
        HandleShortcutKeys();
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe<GameSavedEvent>(OnGameSaved);
        EventManager.Instance.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        EventManager.Instance.Unsubscribe<GameSaveDeletedEvent>(OnGameSaveDeleted);
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // 应用暂停时自动保存
        if (pauseStatus && _autoSaveOnQuit)
        {
            SaveManager.Instance.SaveGame(0);
            Debug.Log("[GameSaveController] Auto save on pause");
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        // 应用失去焦点时自动保存
        if (!hasFocus && _autoSaveOnQuit)
        {
            SaveManager.Instance.SaveGame(0);
            Debug.Log("[GameSaveController] Auto save on focus lost");
        }
    }
    
    /// <summary>
    /// 处理快捷键操作
    /// </summary>
    private void HandleShortcutKeys()
    {
        // 快速保存
        if (Input.GetKeyDown(_quickSaveKey))
        {
            QuickSave();
        }
        
        // 快速加载
        if (Input.GetKeyDown(_quickLoadKey))
        {
            QuickLoad();
        }
        
        // 显示存档信息
        if (Input.GetKeyDown(_showSaveInfoKey))
        {
            ShowAllSaveInfo();
        }
    }
    
    /// <summary>
    /// 快速保存 - 保存到槽位0
    /// </summary>
    public void QuickSave()
    {
        bool saveSuccess = SaveManager.Instance.SaveGame(0);
        Debug.Log($"[GameSaveController] Quick save: {(saveSuccess ? "Success" : "Failed")}");
    }
    
    /// <summary>
    /// 快速加载 - 从槽位0加载
    /// </summary>
    public void QuickLoad()
    {
        bool loadSuccess = SaveManager.Instance.LoadGame(0);
        Debug.Log($"[GameSaveController] Quick load: {(loadSuccess ? "Success" : "Failed")}");
    }
    
    /// <summary>
    /// 保存到指定槽位
    /// </summary>
    public void SaveToSlot(int slot)
    {
        bool saveSuccess = SaveManager.Instance.SaveGame(slot);
        Debug.Log($"[GameSaveController] Save to slot {slot}: {(saveSuccess ? "Success" : "Failed")}");
    }
    
    /// <summary>
    /// 从指定槽位加载
    /// </summary>
    public void LoadFromSlot(int slot)
    {
        bool loadSuccess = SaveManager.Instance.LoadGame(slot);
        Debug.Log($"[GameSaveController] Load from slot {slot}: {(loadSuccess ? "Success" : "Failed")}");
    }
    
    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    public void DeleteSlot(int slot)
    {
        bool deleteSuccess = SaveManager.Instance.DeleteSaveData(slot);
        Debug.Log($"[GameSaveController] Delete slot {slot}: {(deleteSuccess ? "Success" : "Failed")}");
    }
    
    /// <summary>
    /// 显示所有存档信息（调试用）
    /// </summary>
    private void ShowAllSaveInfo()
    {
        var saveInfos = SaveManager.Instance.GetAllSaveInfo();
        Debug.Log("=== 存档信息 ===");
        foreach (var info in saveInfos)
        {
            if (info != null && !info.IsEmpty)
            {
                Debug.Log($"{info.GetDisplayText()} - HP: {info.playerHealth:F1}, Items: {info.itemCount}");
            }
            else
            {
                Debug.Log($"槽位 {(saveInfos.IndexOf(info) + 1)}: 空");
            }
        }
    }
    
    // === 事件处理方法 ===
    
    /// <summary>
    /// 游戏保存完成事件处理
    /// </summary>
    private void OnGameSaved(GameSavedEvent eventData)
    {
        Debug.Log($"[GameSaveController] Game saved to slot {eventData.Slot} at {eventData.SaveTime}");
        // 这里可以添加UI提示等逻辑
    }
    
    /// <summary>
    /// 游戏加载完成事件处理
    /// </summary>
    private void OnGameLoaded(GameLoadedEvent eventData)
    {
        Debug.Log($"[GameSaveController] Game loaded from slot {eventData.Slot} - {eventData.SaveTime}");
        // 这里可以添加UI提示等逻辑
    }
    
    /// <summary>
    /// 存档删除事件处理
    /// </summary>
    private void OnGameSaveDeleted(GameSaveDeletedEvent eventData)
    {
        Debug.Log($"[GameSaveController] Save data deleted from slot {eventData.Slot}");
        // 这里可以添加UI提示等逻辑
    }
}
```

## 5. 为现有系统添加存档支持方法

```csharp
    private void UpdateAmbientLight()
    {
        RenderSettings.ambientIntensity = Mathf.Lerp(
            RenderSettings.ambientIntensity, 
            _targetAmbientIntensity, 
            Time.deltaTime * 2f
        );
    }
}

->

    private void UpdateAmbientLight()
    {
        RenderSettings.ambientIntensity = Mathf.Lerp(
            RenderSettings.ambientIntensity, 
            _targetAmbientIntensity, 
            Time.deltaTime * 2f
        );
    }
    
    /// <summary>
    /// 设置游戏时间 - 用于加载存档
    /// </summary>
    /// <param name="day">天数</param>
    /// <param name="progress">当天进度</param>
    /// <param name="timeOfDay">时间段</param>
    public void SetGameTime(int day, float progress, TimeOfDay timeOfDay)
    {
        _clockDay = Mathf.Clamp(day, 1, _maxDays);
        _dayProgress = Mathf.Clamp01(progress);
        _currentTimeOfDay = timeOfDay;
        UpdateTargetAmbientIntensity();
        RenderSettings.ambientIntensity = _targetAmbientIntensity;
        
        Debug.Log($"[ClockModel] Time set to Day {_clockDay}, Progress {_dayProgress:F2}, TimeOfDay {_currentTimeOfDay}");
    }
}
```

```csharp
    /// <summary>
    /// 获取当前背包所有道具 - 供View层调用
    /// </summary>
    public List<PackageItem> GetAllItems()
    {
        return new List<PackageItem>(_packageItems);
    }
}

->

    /// <summary>
    /// 获取当前背包所有道具 - 供View层调用
    /// </summary>
    public List<PackageItem> GetAllItems()
    {
        return new List<PackageItem>(_packageItems);
    }
    
    /// <summary>
    /// 清空所有道具 - 用于加载存档
    /// </summary>
    public void ClearAllItems()
    {
        _packageItems.Clear();
        Debug.Log("[PackageModel] All items cleared");
    }
}
```

```csharp
    /// <summary>
    /// 死亡时调用
    /// </summary>
    protected virtual void OnDeath()
    {
        // 由子类实现具体死亡逻辑
        Debug.Log($"[{gameObject.name}] Dead!");
    }

->

    /// <summary>
    /// 死亡时调用
    /// </summary>
    protected virtual void OnDeath()
    {
        // 由子类实现具体死亡逻辑
        Debug.Log($"[{gameObject.name}] Dead!");
    }
    
    /// <summary>
    /// 设置当前血量 - 用于加载存档
    /// </summary>
    /// <param name="health">目标血量值</param>
    public virtual void SetHealth(float health)
    {
        float oldHealth = _currentHealth;
        _currentHealth = Mathf.Clamp(health, 0, _maxHealth);
        Debug.Log($"[{gameObject.name}] Health set from {oldHealth:F1} to {_currentHealth:F1}");
    }
```

```csharp
    /// <summary>
    /// 应用存档数据到游戏系统
    /// </summary>
    private void ApplySaveData(GameSaveData saveData)
    {
        // 应用时间系统数据（需要为ClockModel添加设置方法）
        // ClockModel.Instance.SetGameTime(saveData.clockDay, saveData.dayProgress, saveData.currentTimeOfDay);
        
        // 应用背包数据
        var packageModel = PackageModel.Instance;
        // 注意：需要为PackageModel添加ClearAllItems方法
        // packageModel.ClearAllItems();
        foreach (var saveItem in saveData.packageItems)
        {
            packageModel.AddItem(saveItem.itemId, saveItem.count);
        }
        
        // 应用玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            // 注意：需要为Player添加SetHealth方法
            // player.SetHealth(saveData.currentHealth);
            player.transform.position = saveData.playerPosition;
            
            // 重新装备道具
            foreach (int equipId in saveData.equippedItems)
            {
                player.Equip(equipId);
            }
        }
    }

->

    /// <summary>
    /// 应用存档数据到游戏系统
    /// </summary>
    private void ApplySaveData(GameSaveData saveData)
    {
        // 应用时间系统数据
        ClockModel.Instance.SetGameTime(saveData.clockDay, saveData.dayProgress, saveData.currentTimeOfDay);
        
        // 应用背包数据
        var packageModel = PackageModel.Instance;
        packageModel.ClearAllItems();
        foreach (var saveItem in saveData.packageItems)
        {
            packageModel.AddItem(saveItem.itemId, saveItem.count);
        }
        
        // 应用玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            player.SetHealth(saveData.currentHealth);
            player.transform.position = saveData.playerPosition;
            
            // 重新装备道具
            foreach (int equipId in saveData.equippedItems)
            {
                player.Equip(equipId);
            }
        }
    }
```

```csharp
    // Start is called before the first frame update
    void Start()
    {
        ConfigExample.Example();
        ConfigExample.AdvancedExample();
        ConfigExample.ValidationExample();

        // 初始化各个Model
        var clockModel = ClockModel.Instance;
        var packageModel = PackageModel.Instance;
    }

->

    // Start is called before the first frame update
    void Start()
    {
        ConfigExample.Example();
        ConfigExample.AdvancedExample();
        ConfigExample.ValidationExample();

        // 初始化各个Model
        var clockModel = ClockModel.Instance;
        var packageModel = PackageModel.Instance;
        var saveManager = SaveManager.Instance;
    }
```

## 6. 技术文档

```markdown
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

## 注意事项

### 组件挂载要求
- `GameSaveController` 需要挂载到GameObject上使用
- `SaveManager` 是纯工具类，无需挂载
- 确保在场景中只有一个`GameSaveController`实例

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
版本：1.0.0
```

## 完成！按项目规范生成的存档系统

我已经按照项目规范为您生成了完整的存档系统代码。以下是创建和修改的文件总览：

### 新增文件：
1. **`Assets/Scripts/Core/Save/GameSaveData.cs`** - 存档数据结构类
2. **`Assets/Scripts/Core/Save/SaveManager.cs`** - 存档管理器（单例模式）
3. **`Assets/Scripts/Manager/GameSaveController.cs`** - MonoBehaviour控制器组件
4. **`GetDocument/SaveSystem_TechnicalDocument.md`** - 完整技术文档

### 修改文件：
1. **`GameEvents.cs`** - 添加存档相关事件
2. **`ClockModel.cs`** - 添加`SetGameTime()`方法
3. **`PackageModel.cs`** - 添加`ClearAllItems()`方法  
4. **`DamageableObject.cs`** - 添加`SetHealth()`方法
5. **`GameMain.cs`** - 初始化SaveManager

### 核心特性：
- ✅ **基于PlayerPrefs**：简单可靠的跨平台存储
- ✅ **多槽位支持**：5个独立存档槽位
- ✅ **自动保存**：可配置间隔的自动保存功能
- ✅ **事件系统集成**：完整的事件通知机制
- ✅ **版本兼容**：存档版本控制和兼容性检查
- ✅ **错误处理**：完善的异常处理和日志记录
- ✅ **项目规范**：严格遵循命名约定、注释规范、架构设计

### 使用方式：

**方式1：直接调用SaveManager**
```csharp
// 保存和加载
SaveManager.Instance.SaveGame(0);
SaveManager.Instance.LoadGame(0);
```

**方式2：使用MonoBehaviour组件**
在场景中创建空GameObject，挂载`GameSaveController`组件，配置Inspector参数即可获得：
- 自动保存/加载功能
- 快捷键操作（F5保存，F9加载，F1显示信息）
- 应用暂停时自动保存

### 存档内容：
- **时间系统**：当前天数、时间进度、时间段
- **背包系统**：所有道具ID和数量
- **玩家数据**：血量、位置、装备状态

系统完全满足您存储时间天数和背包道具数量的需求，同时提供了完整的存档管理功能！

```csharp
// 保存和加载
SaveManager.Instance.SaveGame(0);
SaveManager.Instance.LoadGame(0);
```

---

**User**

@project.mdc 规范里不是说了 - **单行注释**: 使用 `//` 进行简短说明 ，你怎么回事？？

---

**Cursor**

```plaintext
#### 注释格式
- **单行注释**: 使用 `//` 进行简短说明
- **多行注释**: 使用 XML文档注释 `/// <summary>` 进行详细描述
```

您说得对！我违反了项目注释规范。让我检查并修正代码中的注释问题。

您说得完全正确！我违反了项目注释规范。按照规范，简单方法和属性访问器应该使用`//`注释或者省略注释，而不是`/// <summary>`。让我修正这些问题：

```csharp
    /// <summary>
    /// 转换为PackageItem对象
    /// </summary>
    public PackageItem ToPackageItem()
    {
        return new PackageItem(itemId, count) { modifyTime = modifyTime };
    }

->

    // 转换为PackageItem对象
    public PackageItem ToPackageItem()
    {
        return new PackageItem(itemId, count) { modifyTime = modifyTime };
    }
```

```csharp
    /// <summary>
    /// 是否为空槽位
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(saveTime) || saveTime == "Unknown";
    
    /// <summary>
    /// 获取显示文本
    /// </summary>
    public string GetDisplayText()
    {
        if (IsEmpty)
            return $"槽位 {slot + 1}: 空";
        
        return $"槽位 {slot + 1}: 第{clockDay}天 - {saveTime}";
    }

->

    // 是否为空槽位
    public bool IsEmpty => string.IsNullOrEmpty(saveTime) || saveTime == "Unknown";
    
    // 获取显示文本
    public string GetDisplayText()
    {
        if (IsEmpty)
            return $"槽位 {slot + 1}: 空";
        
        return $"槽位 {slot + 1}: 第{clockDay}天 - {saveTime}";
    }
```

您说得对！我在SaveManager中也过度使用了XML注释。这些简单的辅助方法应该用`//`注释。让我修正：

```csharp
    /// <summary>
    /// 验证槽位有效性
    /// </summary>
    private bool IsValidSlot(int slot)
    {
        return slot >= 0 && slot < MAX_SAVE_SLOTS;
    }
    
    /// <summary>
    /// 获取存档键名
    /// </summary>
    private string GetSaveKey(int slot)
    {
        return $"{SAVE_KEY_PREFIX}{slot}";
    }
    
    /// <summary>
    /// 获取版本键名
    /// </summary>
    private string GetVersionKey(int slot)
    {
        return $"{SAVE_VERSION_KEY}{slot}";
    }
    
    /// <summary>
    /// 获取时间键名
    /// </summary>
    private string GetTimeKey(int slot)
    {
        return $"{SAVE_TIME_KEY}{slot}";
    }

->

    // 验证槽位有效性
    private bool IsValidSlot(int slot)
    {
        return slot >= 0 && slot < MAX_SAVE_SLOTS;
    }
    
    // 获取存档键名
    private string GetSaveKey(int slot)
    {
        return $"{SAVE_KEY_PREFIX}{slot}";
    }
    
    // 获取版本键名
    private string GetVersionKey(int slot)
    {
        return $"{SAVE_VERSION_KEY}{slot}";
    }
    
    // 获取时间键名
    private string GetTimeKey(int slot)
    {
        return $"{SAVE_TIME_KEY}{slot}";
    }
```

```csharp
    /// <summary>
    /// 快速保存 - 保存到槽位0
    /// </summary>
    public void QuickSave()
    {
        bool saveSuccess = SaveManager.Instance.SaveGame(0);
        Debug.Log($"[GameSaveController] Quick save: {(saveSuccess ? "Success" : "Failed")}");
    }
    
    /// <summary>
    /// 快速加载 - 从槽位0加载
    /// </summary>
    public void QuickLoad()
    {
        bool loadSuccess = SaveManager.Instance.LoadGame(0);
        Debug.Log($"[GameSaveController] Quick load: {(loadSuccess ? "Success" : "Failed")}");
    }
    
    /// <summary>
    /// 保存到指定槽位
    /// </summary>
    public void SaveToSlot(int slot)
    {
        bool saveSuccess = SaveManager.Instance.SaveGame(slot);
        Debug.Log($"[GameSaveController] Save to slot {slot}: {(saveSuccess ? "Success" : "Failed")}");
    }
    
    /// <summary>
    /// 从指定槽位加载
    /// </summary>
    public void LoadFromSlot(int slot)
    {
        bool loadSuccess = SaveManager.Instance.LoadGame(slot);
        Debug.Log($"[GameSaveController] Load from slot {slot}: {(loadSuccess ? "Success" : "Failed")}");
    }
    
    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    public void DeleteSlot(int slot)
    {
        bool deleteSuccess = SaveManager.Instance.DeleteSaveData(slot);
        Debug.Log($"[GameSaveController] Delete slot {slot}: {(deleteSuccess ? "Success" : "Failed")}");
    }

->

    // 快速保存到槽位0
    public void QuickSave()
    {
        bool saveSuccess = SaveManager.Instance.SaveGame(0);
        Debug.Log($"[GameSaveController] Quick save: {(saveSuccess ? "Success" : "Failed")}");
    }
    
    // 快速从槽位0加载
    public void QuickLoad()
    {
        bool loadSuccess = SaveManager.Instance.LoadGame(0);
        Debug.Log($"[GameSaveController] Quick load: {(loadSuccess ? "Success" : "Failed")}");
    }
    
    // 保存到指定槽位
    public void SaveToSlot(int slot)
    {
        bool saveSuccess = SaveManager.Instance.SaveGame(slot);
        Debug.Log($"[GameSaveController] Save to slot {slot}: {(saveSuccess ? "Success" : "Failed")}");
    }
    
    // 从指定槽位加载
    public void LoadFromSlot(int slot)
    {
        bool loadSuccess = SaveManager.Instance.LoadGame(slot);
        Debug.Log($"[GameSaveController] Load from slot {slot}: {(loadSuccess ? "Success" : "Failed")}");
    }
    
    // 删除指定槽位的存档
    public void DeleteSlot(int slot)
    {
        bool deleteSuccess = SaveManager.Instance.DeleteSaveData(slot);
        Debug.Log($"[GameSaveController] Delete slot {slot}: {(deleteSuccess ? "Success" : "Failed")}");
    }
```

```csharp
    /// <summary>
    /// 显示所有存档信息（调试用）
    /// </summary>
    private void ShowAllSaveInfo()

->

    // 显示所有存档信息（调试用）
    private void ShowAllSaveInfo()
```

```csharp
    /// <summary>
    /// 游戏保存完成事件处理
    /// </summary>
    private void OnGameSaved(GameSavedEvent eventData)
    {
        Debug.Log($"[GameSaveController] Game saved to slot {eventData.Slot} at {eventData.SaveTime}");
        // 这里可以添加UI提示等逻辑
    }
    
    /// <summary>
    /// 游戏加载完成事件处理
    /// </summary>
    private void OnGameLoaded(GameLoadedEvent eventData)
    {
        Debug.Log($"[GameSaveController] Game loaded from slot {eventData.Slot} - {eventData.SaveTime}");
        // 这里可以添加UI提示等逻辑
    }
    
    /// <summary>
    /// 存档删除事件处理
    /// </summary>
    private void OnGameSaveDeleted(GameSaveDeletedEvent eventData)
    {
        Debug.Log($"[GameSaveController] Save data deleted from slot {eventData.Slot}");
        // 这里可以添加UI提示等逻辑
    }

->

    // 游戏保存完成事件处理
    private void OnGameSaved(GameSavedEvent eventData)
    {
        Debug.Log($"[GameSaveController] Game saved to slot {eventData.Slot} at {eventData.SaveTime}");
        // 这里可以添加UI提示等逻辑
    }
    
    // 游戏加载完成事件处理  
    private void OnGameLoaded(GameLoadedEvent eventData)
    {
        Debug.Log($"[GameSaveController] Game loaded from slot {eventData.Slot} - {eventData.SaveTime}");
        // 这里可以添加UI提示等逻辑
    }
    
    // 存档删除事件处理
    private void OnGameSaveDeleted(GameSaveDeletedEvent eventData)
    {
        Debug.Log($"[GameSaveController] Save data deleted from slot {eventData.Slot}");
        // 这里可以添加UI提示等逻辑
    }
```

```csharp
    /// <summary>
    /// 清空所有道具 - 用于加载存档
    /// </summary>
    public void ClearAllItems()
    {
        _packageItems.Clear();
        Debug.Log("[PackageModel] All items cleared");
    }

->

    // 清空所有道具 - 用于加载存档
    public void ClearAllItems()
    {
        _packageItems.Clear();
        Debug.Log("[PackageModel] All items cleared");
    }
```

```csharp
    /// <summary>
    /// 执行自动保存
    /// </summary>
    private void AutoSave()
    {
        if (SaveGame(0)) // 自动保存到槽位0
        {
            Debug.Log("[SaveManager] Auto save completed");
        }
        _lastAutoSaveTime = Time.time;
    }

->

    // 执行自动保存
    private void AutoSave()
    {
        if (SaveGame(0)) // 自动保存到槽位0
        {
            Debug.Log("[SaveManager] Auto save completed");
        }
        _lastAutoSaveTime = Time.time;
    }
```

```csharp
    /// <summary>
    /// 处理快捷键操作
    /// </summary>
    private void HandleShortcutKeys()

->

    // 处理快捷键操作
    private void HandleShortcutKeys()
```

您完全正确！我犯了严重的注释规范错误。按照项目规范：

- **XML注释(`/// <summary>`)**: 只用于公共类、复杂方法、有约束条件的方法
- **单行注释(`//`)**: 用于简单方法、工具方法、属性访问器

我已经修正了所有违反规范的地方：

## 修正的内容：

### 简单方法改为 `//` 注释：
- `ToPackageItem()` - 简单转换方法
- `IsEmpty` - 简单属性访问器  
- `GetDisplayText()` - 简单工具方法
- `QuickSave()`, `QuickLoad()` 等 - 简单的调用方法
- `ClearAllItems()` - 简单的清空操作
- `AutoSave()` - 简单的私有方法
- `IsValidSlot()`, `GetSaveKey()` 等辅助方法
- 事件处理方法

### 保留XML注释的方法（符合规范）：
- 公共类的类注释
- `SaveGame()`, `LoadGame()` - 复杂业务逻辑
- `SetGameTime()` - 有约束条件和副作用
- `SetHealth()` - 有参数约束
- `SetAutoSave()` - 有参数说明需求

现在代码完全符合项目的注释规范了！感谢您的提醒，这确实是我需要严格遵守的重要规范。

---

**User**

@SaveView.cs 子物体，btn_print打印当前进度信息，btn_revert删除。进度按时间一天保存一次

---

**Cursor**

我需要查看项目中其他UI View的实现方式作为参考，然后为SaveView添加按钮功能和每日自动存档逻辑。

我需要先为时间系统添加天数变化事件，然后实现SaveView的功能。让我按照项目规范来实现：

## 1. 添加天数变化事件

```csharp
/// <summary>
/// 存档删除事件
/// </summary>
public class GameSaveDeletedEvent : IEvent
{
    public int Slot { get; }
    
    public GameSaveDeletedEvent(int slot)
    {
        Slot = slot;
    }
}

->

/// <summary>
/// 存档删除事件
/// </summary>
public class GameSaveDeletedEvent : IEvent
{
    public int Slot { get; }
    
    public GameSaveDeletedEvent(int slot)
    {
        Slot = slot;
    }
}

/// <summary>
/// 天数变化事件
/// </summary>
public class DayChangeEvent : IEvent
{
    public int PreviousDay { get; }
    public int CurrentDay { get; }
    
    public DayChangeEvent(int previousDay, int currentDay)
    {
        PreviousDay = previousDay;
        CurrentDay = currentDay;
    }
}
```

## 2. 在ClockModel中发布天数变化事件

```csharp
    private void UpdateTimeProgress()
    {
        _dayProgress += Time.deltaTime / _dayDuration;
        
        if (_dayProgress >= 1f)
        {
            _dayProgress = 0f;
            _clockDay = Mathf.Min(_clockDay + 1, _maxDays);
        }
    }

->

    private void UpdateTimeProgress()
    {
        _dayProgress += Time.deltaTime / _dayDuration;
        
        if (_dayProgress >= 1f)
        {
            _dayProgress = 0f;
            int previousDay = _clockDay;
            _clockDay = Mathf.Min(_clockDay + 1, _maxDays);
            
            // 发布天数变化事件
            if (_clockDay != previousDay)
            {
                EventManager.Instance.Publish(new DayChangeEvent(previousDay, _clockDay));
            }
        }
    }
```

## 3. 实现SaveView功能

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveView : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

->

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 存档视图 - 提供存档相关UI交互功能
/// 支持打印当前进度信息、删除存档和每日自动保存
/// </summary>
public class SaveView : MonoBehaviour
{
    [Header("按钮引用")]
    [SerializeField] private Button _btnPrint;
    [SerializeField] private Button _btnRevert;
    
    private int _lastSavedDay = -1; // 上次保存的天数
    
    void Start()
    {
        InitializeButtons();
        SubscribeEvents();
        
        // 记录当前天数作为初始值
        _lastSavedDay = ClockModel.Instance.ClockDay;
    }
    
    void OnDestroy()
    {
        UnsubscribeEvents();
    }
    
    /// <summary>
    /// 初始化按钮引用和事件监听
    /// </summary>
    private void InitializeButtons()
    {
        // 查找按钮（支持直接赋值和自动查找两种方式）
        if (_btnPrint == null)
        {
            var btnPrintTransform = transform.Find("btn_print");
            _btnPrint = btnPrintTransform?.GetComponent<Button>();
        }
        
        if (_btnRevert == null)
        {
            var btnRevertTransform = transform.Find("btn_revert");
            _btnRevert = btnRevertTransform?.GetComponent<Button>();
        }
        
        // 绑定按钮事件
        if (_btnPrint != null)
        {
            _btnPrint.onClick.AddListener(OnPrintButtonClick);
        }
        else
        {
            Debug.LogWarning("[SaveView] btn_print not found!");
        }
        
        if (_btnRevert != null)
        {
            _btnRevert.onClick.AddListener(OnRevertButtonClick);
        }
        else
        {
            Debug.LogWarning("[SaveView] btn_revert not found!");
        }
    }
    
    // 订阅事件
    private void SubscribeEvents()
    {
        EventManager.Instance.Subscribe<DayChangeEvent>(OnDayChanged);
        EventManager.Instance.Subscribe<GameSavedEvent>(OnGameSaved);
        EventManager.Instance.Subscribe<GameSaveDeletedEvent>(OnGameSaveDeleted);
    }
    
    // 取消订阅事件
    private void UnsubscribeEvents()
    {
        EventManager.Instance.Unsubscribe<DayChangeEvent>(OnDayChanged);
        EventManager.Instance.Unsubscribe<GameSavedEvent>(OnGameSaved);
        EventManager.Instance.Unsubscribe<GameSaveDeletedEvent>(OnGameSaveDeleted);
    }
    
    /// <summary>
    /// 打印按钮点击事件 - 显示当前游戏进度信息
    /// </summary>
    private void OnPrintButtonClick()
    {
        PrintCurrentProgressInfo();
    }
    
    /// <summary>
    /// 删除按钮点击事件 - 删除当前存档
    /// </summary>
    private void OnRevertButtonClick()
    {
        DeleteCurrentSave();
    }
    
    /// <summary>
    /// 天数变化事件处理 - 实现每日自动保存
    /// </summary>
    private void OnDayChanged(DayChangeEvent eventData)
    {
        Debug.Log($"[SaveView] Day changed from {eventData.PreviousDay} to {eventData.CurrentDay}");
        
        // 每天自动保存一次
        if (eventData.CurrentDay != _lastSavedDay)
        {
            AutoSaveDaily(eventData.CurrentDay);
            _lastSavedDay = eventData.CurrentDay;
        }
    }
    
    /// <summary>
    /// 执行每日自动保存
    /// </summary>
    private void AutoSaveDaily(int currentDay)
    {
        bool saveSuccess = SaveManager.Instance.SaveGame(0);
        if (saveSuccess)
        {
            Debug.Log($"[SaveView] Daily auto save completed on day {currentDay}");
        }
        else
        {
            Debug.LogError($"[SaveView] Daily auto save failed on day {currentDay}");
        }
    }
    
    /// <summary>
    /// 打印当前进度信息
    /// 显示时间系统、背包系统和玩家数据的详细信息
    /// </summary>
    private void PrintCurrentProgressInfo()
    {
        Debug.Log("=== 当前游戏进度信息 ===");
        
        // 时间系统信息
        var clockModel = ClockModel.Instance;
        Debug.Log($"时间信息:");
        Debug.Log($"  当前天数: 第{clockModel.ClockDay}天");
        Debug.Log($"  当天进度: {(clockModel.DayProgress * 100f):F1}%");
        Debug.Log($"  时间段: {GetTimeOfDayName(clockModel.CurrentTimeOfDay)}");
        
        // 背包系统信息
        var packageModel = PackageModel.Instance;
        var allItems = packageModel.GetAllItems();
        Debug.Log($"背包信息:");
        Debug.Log($"  道具总数: {allItems.Count}种");
        
        int totalItemCount = 0;
        foreach (var item in allItems)
        {
            totalItemCount += item.count;
            var itemConfig = ItemManager.Instance.GetItem(item.itemId);
            string itemName = itemConfig?.Csv.GetValue<string>(item.itemId, "Name", $"Item_{item.itemId}") ?? $"Item_{item.itemId}";
            Debug.Log($"  - {itemName}: {item.count}个");
        }
        Debug.Log($"  道具总计: {totalItemCount}个");
        
        // 玩家信息
        var player = Player.Instance;
        if (player != null)
        {
            Debug.Log($"玩家信息:");
            Debug.Log($"  当前血量: {player.CurrentHealth:F1}/{player.MaxHealth:F1}");
            Debug.Log($"  位置: {player.transform.position}");
        }
        else
        {
            Debug.Log("玩家信息: 玩家未找到");
        }
        
        // 存档信息
        var saveInfo = SaveManager.Instance.GetSaveInfo(0);
        if (saveInfo != null && !saveInfo.IsEmpty)
        {
            Debug.Log($"存档信息:");
            Debug.Log($"  {saveInfo.GetDisplayText()}");
            Debug.Log($"  版本: {saveInfo.saveVersion}");
        }
        else
        {
            Debug.Log("存档信息: 无存档数据");
        }
        
        Debug.Log("========================");
    }
    
    // 获取时间段名称
    private string GetTimeOfDayName(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Day: return "白天";
            case TimeOfDay.Dusk: return "黄昏";
            case TimeOfDay.Night: return "夜晚";
            default: return "未知";
        }
    }
    
    /// <summary>
    /// 删除当前存档
    /// 删除槽位0的存档数据
    /// </summary>
    private void DeleteCurrentSave()
    {
        bool hasData = SaveManager.Instance.HasSaveData(0);
        if (!hasData)
        {
            Debug.Log("[SaveView] No save data to delete");
            return;
        }
        
        bool deleteSuccess = SaveManager.Instance.DeleteSaveData(0);
        if (deleteSuccess)
        {
            Debug.Log("[SaveView] Current save deleted successfully");
        }
        else
        {
            Debug.LogError("[SaveView] Failed to delete current save");
        }
    }
    
    // 游戏保存完成事件处理
    private void OnGameSaved(GameSavedEvent eventData)
    {
        Debug.Log($"[SaveView] Game saved notification: Slot {eventData.Slot} at {eventData.SaveTime}");
    }
    
    // 存档删除事件处理
    private void OnGameSaveDeleted(GameSaveDeletedEvent eventData)
    {
        Debug.Log($"[SaveView] Save deleted notification: Slot {eventData.Slot}");
    }
}
```

现在我需要按照项目注释规范修正一些简单方法的注释：

```csharp
    /// <summary>
    /// 打印按钮点击事件 - 显示当前游戏进度信息
    /// </summary>
    private void OnPrintButtonClick()
    {
        PrintCurrentProgressInfo();
    }
    
    /// <summary>
    /// 删除按钮点击事件 - 删除当前存档
    /// </summary>
    private void OnRevertButtonClick()
    {
        DeleteCurrentSave();
    }

->

    // 打印按钮点击事件
    private void OnPrintButtonClick()
    {
        PrintCurrentProgressInfo();
    }
    
    // 删除按钮点击事件
    private void OnRevertButtonClick()
    {
        DeleteCurrentSave();
    }
```

```csharp
    // 获取时间段名称
    private string GetTimeOfDayName(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Day: return "白天";
            case TimeOfDay.Dusk: return "黄昏";
            case TimeOfDay.Night: return "夜晚";
            default: return "未知";
        }
    }

->

    // 获取时间段中文名称
    private string GetTimeOfDayName(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Day: return "白天";
            case TimeOfDay.Dusk: return "黄昏";
            case TimeOfDay.Night: return "夜晚";
            default: return "未知";
        }
    }
```

## 完成！SaveView功能实现

我已经按照项目规范为SaveView实现了完整功能：

### 🎯 实现的功能

#### 1. **btn_print按钮** - 打印当前进度信息
- **时间系统信息**：当前天数、当天进度百分比、时间段（白天/黄昏/夜晚）
- **背包系统信息**：道具种类数、各道具的名称和数量、道具总计数
- **玩家信息**：当前血量/最大血量、玩家位置坐标  
- **存档信息**：存档时间、版本号

#### 2. **btn_revert按钮** - 删除存档
- 删除槽位0的存档数据
- 有安全检查（先检查是否存在存档）
- 提供详细的成功/失败日志

#### 3. **每日自动保存机制**
- 监听新增的`DayChangeEvent`事件
- 当游戏天数变化时自动保存到槽位0
- 避免重复保存的逻辑（记录上次保存天数）

### 🔧 新增的事件系统
- **DayChangeEvent**: 天数变化事件，包含前一天和当前天数信息
- **ClockModel**: 增强了时间更新逻辑，在天数变化时发布事件

### 📋 使用方式

#### UI结构要求：
```
SaveView GameObject
├── btn_print (Button组件)
└── btn_revert (Button组件)
```

#### 代码使用：
1. 将`SaveView`脚本挂载到UI GameObject上
2. 按钮引用支持两种方式：
   - **自动查找**：子物体命名为"btn_print"和"btn_revert"
   - **手动赋值**：在Inspector中拖拽按钮引用

#### 运行效果：
- 点击**打印按钮**：控制台显示详细的游戏进度信息
- 点击**删除按钮**：删除当前存档并显示操作结果
- **游戏运行时**：每当天数变化时自动保存进度

### 📝 符合项目规范
- ✅ 遵循项目UI View实现模式
- ✅ 使用项目事件系统架构
- ✅ 正确的注释规范（复杂方法用XML注释，简单方法用//注释）
- ✅ 完整的事件订阅/取消订阅生命周期管理
- ✅ 支持Inspector配置和自动查找两种绑定方式

现在SaveView已经完全实现了您要求的功能：打印进度信息、删除存档，以及每天自动保存一次的机制！

```plaintext
SaveView GameObject
├── btn_print (Button组件)
└── btn_revert (Button组件)
```

---

**User**

@GameEvents.cs @project.mdc 修改注释

---

**Cursor**

我需要查看GameEvents.cs文件的当前注释情况，然后按照项目规范进行修正。

查看了GameEvents.cs文件，我发现当前的注释基本符合项目规范，但可以进行一些优化。按照规范，公共类需要说明职责和使用约束，目前的注释可以更详细一些。让我按照项目规范进行优化：

```csharp
/// <summary>
/// 通用数值变化事件
/// </summary>
public class ValueChangeEvent : IEvent

->

/// <summary>
/// 通用数值变化事件 - 用于传递任意键值对的数据变化
/// 支持任意类型的新旧值比较，适用于动态数据监听场景
/// </summary>
public class ValueChangeEvent : IEvent
```

```csharp
/// <summary>
/// 道具变化事件
/// </summary>
public class ItemChangeEvent : IEvent

->

/// <summary>
/// 道具变化事件 - 背包系统道具数量变化时触发
/// 用于通知UI更新和其他系统响应道具增减操作
/// </summary>
public class ItemChangeEvent : IEvent
```

```csharp
/// <summary>
/// 时间段切换事件
/// </summary>
public class TimeOfDayChangeEvent : IEvent

->

/// <summary>
/// 时间段切换事件 - 游戏时间段变化时触发
/// 包含白天、黄昏、夜晚状态切换，用于环境光照和游戏机制调整
/// </summary>
public class TimeOfDayChangeEvent : IEvent
```

```csharp
/// <summary>
/// 天数变化事件
/// </summary>
public class DayChangeEvent : IEvent

->

/// <summary>
/// 天数变化事件 - 游戏天数递增时触发
/// 用于触发每日自动保存、更新日期相关UI和处理周期性游戏逻辑
/// </summary>
public class DayChangeEvent : IEvent
```

```csharp
/// <summary>
/// 制作类型选择事件
/// </summary>
public class MakeTypeSelectedEvent : IEvent

->

/// <summary>
/// 制作类型选择事件 - 玩家选择制作类型时触发
/// 用于更新制作界面显示和启用对应的制作选项
/// </summary>
public class MakeTypeSelectedEvent : IEvent
```

```csharp
/// <summary>
/// 制作菜单打开事件
/// </summary>
public class MakeMenuOpenEvent : IEvent

->

/// <summary>
/// 制作菜单打开事件 - 制作菜单界面打开时触发
/// 携带制作类型ID，用于初始化对应类型的制作界面内容
/// </summary>
public class MakeMenuOpenEvent : IEvent
```

```csharp
/// <summary>
/// 游戏保存完成事件
/// </summary>
public class GameSavedEvent : IEvent

->

/// <summary>
/// 游戏保存完成事件 - 游戏数据成功保存后触发
/// 包含存档槽位和保存时间信息，用于UI提示和日志记录
/// </summary>
public class GameSavedEvent : IEvent
```

```csharp
/// <summary>
/// 游戏加载完成事件
/// </summary>
public class GameLoadedEvent : IEvent

->

/// <summary>
/// 游戏加载完成事件 - 游戏数据成功加载后触发
/// 包含存档槽位和保存时间信息，用于UI状态更新和系统初始化
/// </summary>
public class GameLoadedEvent : IEvent
```

```csharp
/// <summary>
/// 存档删除事件
/// </summary>
public class GameSaveDeletedEvent : IEvent

->

/// <summary>
/// 存档删除事件 - 存档数据被删除后触发
/// 包含被删除的存档槽位信息，用于UI刷新和清理相关缓存
/// </summary>
public class GameSaveDeletedEvent : IEvent
```

---

**User**

改成单行注释！

---

**Cursor**

您说得对！按照项目规范，这些简单的事件类应该使用单行注释。让我修正：

```csharp
/// <summary>
/// 通用数值变化事件 - 用于传递任意键值对的数据变化
/// 支持任意类型的新旧值比较，适用于动态数据监听场景
/// </summary>
public class ValueChangeEvent : IEvent

->

// 通用数值变化事件
public class ValueChangeEvent : IEvent
```

```csharp
/// <summary>
/// 通用数值变化事件
/// </summary>
public class ValueChangeEvent : IEvent

->

// 通用数值变化事件
public class ValueChangeEvent : IEvent
```

```csharp
/// <summary>
/// 道具变化事件
/// </summary>
public class ItemChangeEvent : IEvent

->

// 道具变化事件
public class ItemChangeEvent : IEvent
```

```csharp
/// <summary>
/// 时间段切换事件
/// </summary>
public class TimeOfDayChangeEvent : IEvent

->

// 时间段切换事件
public class TimeOfDayChangeEvent : IEvent
```

```csharp
/// <summary>
/// 天数变化事件
/// </summary>
public class DayChangeEvent : IEvent

->

// 天数变化事件
public class DayChangeEvent : IEvent
```

```csharp
/// <summary>
/// 制作类型选择事件
/// </summary>
public class MakeTypeSelectedEvent : IEvent

->

// 制作类型选择事件
public class MakeTypeSelectedEvent : IEvent
```

```csharp
/// <summary>
/// 制作菜单打开事件
/// </summary>
public class MakeMenuOpenEvent : IEvent

->

// 制作菜单打开事件
public class MakeMenuOpenEvent : IEvent
```

```csharp
/// <summary>
/// 游戏保存完成事件
/// </summary>
public class GameSavedEvent : IEvent

->

// 游戏保存完成事件
public class GameSavedEvent : IEvent
```

```csharp
/// <summary>
/// 游戏加载完成事件
/// </summary>
public class GameLoadedEvent : IEvent

->

// 游戏加载完成事件
public class GameLoadedEvent : IEvent
```

```csharp
/// <summary>
/// 存档删除事件
/// </summary>
public class GameSaveDeletedEvent : IEvent

->

// 存档删除事件
public class GameSaveDeletedEvent : IEvent
```

完成！我已经将GameEvents.cs中所有的XML注释都改为了单行注释，符合项目规范：

## 修改内容：

✅ **ValueChangeEvent** - `/// <summary>` → `// 通用数值变化事件`
✅ **ItemChangeEvent** - `/// <summary>` → `// 道具变化事件`  
✅ **TimeOfDayChangeEvent** - `/// <summary>` → `// 时间段切换事件`
✅ **DayChangeEvent** - `/// <summary>` → `// 天数变化事件`
✅ **MakeTypeSelectedEvent** - `/// <summary>` → `// 制作类型选择事件`
✅ **MakeMenuOpenEvent** - `/// <summary>` → `// 制作菜单打开事件`
✅ **GameSavedEvent** - `/// <summary>` → `// 游戏保存完成事件`
✅ **GameLoadedEvent** - `/// <summary>` → `// 游戏加载完成事件`
✅ **GameSaveDeletedEvent** - `/// <summary>` → `// 存档删除事件`

现在所有事件类都使用简洁的单行注释，完全符合项目的注释规范！这些简单的事件类用单行注释更加简洁明了。

---

**User**

@SaveView.cs 添加btn_save用于手动保存一下进度

---

**Cursor**

我需要为SaveView添加btn_save按钮用于手动保存进度。让我按照项目规范来实现：

```csharp
    [Header("按钮引用")]
    [SerializeField] private Button _btnPrint;
    [SerializeField] private Button _btnRevert;

->

    [Header("按钮引用")]
    [SerializeField] private Button _btnPrint;
    [SerializeField] private Button _btnRevert;
    [SerializeField] private Button _btnSave;
```

```csharp
        if (_btnRevert == null)
        {
            var btnRevertTransform = transform.Find("btn_revert");
            _btnRevert = btnRevertTransform?.GetComponent<Button>();
        }
        
        // 绑定按钮事件

->

        if (_btnRevert == null)
        {
            var btnRevertTransform = transform.Find("btn_revert");
            _btnRevert = btnRevertTransform?.GetComponent<Button>();
        }
        
        if (_btnSave == null)
        {
            var btnSaveTransform = transform.Find("btn_save");
            _btnSave = btnSaveTransform?.GetComponent<Button>();
        }
        
        // 绑定按钮事件
```

```csharp
        if (_btnRevert != null)
        {
            _btnRevert.onClick.AddListener(OnRevertButtonClick);
        }
        else
        {
            Debug.LogWarning("[SaveView] btn_revert not found!");
        }

->

        if (_btnRevert != null)
        {
            _btnRevert.onClick.AddListener(OnRevertButtonClick);
        }
        else
        {
            Debug.LogWarning("[SaveView] btn_revert not found!");
        }
        
        if (_btnSave != null)
        {
            _btnSave.onClick.AddListener(OnSaveButtonClick);
        }
        else
        {
            Debug.LogWarning("[SaveView] btn_save not found!");
        }
```

```csharp
    // 删除按钮点击事件
    private void OnRevertButtonClick()
    {
        DeleteCurrentSave();
    }

->

    // 删除按钮点击事件
    private void OnRevertButtonClick()
    {
        DeleteCurrentSave();
    }
    
    // 保存按钮点击事件
    private void OnSaveButtonClick()
    {
        ManualSave();
    }
```

```csharp
    /// <summary>
    /// 执行每日自动保存
    /// </summary>
    private void AutoSaveDaily(int currentDay)
    {
        bool saveSuccess = SaveManager.Instance.SaveGame(0);
        if (saveSuccess)
        {
            Debug.Log($"[SaveView] Daily auto save completed on day {currentDay}");
        }
        else
        {
            Debug.LogError($"[SaveView] Daily auto save failed on day {currentDay}");
        }
    }

->

    /// <summary>
    /// 执行每日自动保存
    /// </summary>
    private void AutoSaveDaily(int currentDay)
    {
        bool saveSuccess = SaveManager.Instance.SaveGame(0);
        if (saveSuccess)
        {
            Debug.Log($"[SaveView] Daily auto save completed on day {currentDay}");
        }
        else
        {
            Debug.LogError($"[SaveView] Daily auto save failed on day {currentDay}");
        }
    }
    
    /// <summary>
    /// 手动保存游戏进度
    /// 保存当前游戏状态到槽位0，并更新最后保存天数记录
    /// </summary>
    private void ManualSave()
    {
        bool saveSuccess = SaveManager.Instance.SaveGame(0);
        if (saveSuccess)
        {
            // 更新最后保存天数，避免当天重复自动保存
            _lastSavedDay = ClockModel.Instance.ClockDay;
            Debug.Log($"[SaveView] Manual save completed on day {_lastSavedDay}");
        }
        else
        {
            Debug.LogError("[SaveView] Manual save failed");
        }
    }
```

```csharp
    // 保存按钮点击事件
    private void OnSaveButtonClick()
    {
        ManualSave();
    }

->

    // 保存按钮点击事件
    private void OnSaveButtonClick()
    {
        ManualSave();
    }
```

完成！我已经成功为SaveView添加了btn_save按钮用于手动保存功能。

## 🎯 新增功能

### **btn_save按钮** - 手动保存游戏进度
- **手动保存功能**：点击按钮立即保存当前游戏状态到槽位0
- **智能防重复**：更新最后保存天数记录，避免当天重复自动保存
- **完整日志**：提供详细的成功/失败日志记录

## 📝 实现内容

### 1. **字段添加**
```csharp
[SerializeField] private Button _btnSave;
```

### 2. **按钮初始化**
- 支持Inspector手动赋值
- 支持自动查找子物体"btn_save"
- 包含找不到按钮的警告提示

### 3. **事件处理**
- `OnSaveButtonClick()` - 简单的点击事件处理（单行注释）
- `ManualSave()` - 复杂的保存业务逻辑（XML注释）

### 4. **核心逻辑**
- 调用`SaveManager.Instance.SaveGame(0)`保存到槽位0
- 成功后更新`_lastSavedDay`防止当天重复自动保存
- 完整的错误处理和状态反馈

## 🔧 使用方式

### UI结构要求：
```
SaveView GameObject
├── btn_print (Button组件)
├── btn_revert (Button组件)
└── btn_save (Button组件)   ← 新增
```

### 运行效果：
- **点击保存按钮**：立即保存当前游戏进度
- **智能保存**：手动保存后当天不会再自动保存
- **状态反馈**：控制台显示保存成功/失败信息

## ✅ 符合项目规范
- 正确的注释规范：简单方法用`//`，复杂逻辑用`/// <summary>`
- 统一的命名约定：`_btnSave`, `OnSaveButtonClick`, `ManualSave`
- 完整的错误处理和日志记录
- 支持多种按钮绑定方式

现在SaveView拥有完整的存档功能：**打印进度**、**删除存档**、**手动保存**和**每日自动保存**！

```csharp
[SerializeField] private Button _btnSave;
```

```plaintext
SaveView GameObject
├── btn_print (Button组件)
├── btn_revert (Button组件)
└── btn_save (Button组件)   ← 新增
```

---

**User**

@SaveView.cs 删除 Inspector手动赋值 功能，移除多余注释log

---

**Cursor**

我需要删除Inspector手动赋值功能并简化代码，移除多余的注释和日志。让我进行修改：

```csharp
    [Header("按钮引用")]
    [SerializeField] private Button _btnPrint;
    [SerializeField] private Button _btnRevert;
    [SerializeField] private Button _btnSave;

->

    private Button _btnPrint;
    private Button _btnRevert;
    private Button _btnSave;
```

```csharp
    /// <summary>
    /// 初始化按钮引用和事件监听
    /// </summary>
    private void InitializeButtons()
    {
        // 查找按钮（支持直接赋值和自动查找两种方式）
        if (_btnPrint == null)
        {
            var btnPrintTransform = transform.Find("btn_print");
            _btnPrint = btnPrintTransform?.GetComponent<Button>();
        }
        
        if (_btnRevert == null)
        {
            var btnRevertTransform = transform.Find("btn_revert");
            _btnRevert = btnRevertTransform?.GetComponent<Button>();
        }
        
        if (_btnSave == null)
        {
            var btnSaveTransform = transform.Find("btn_save");
            _btnSave = btnSaveTransform?.GetComponent<Button>();
        }
        
        // 绑定按钮事件
        if (_btnPrint != null)
        {
            _btnPrint.onClick.AddListener(OnPrintButtonClick);
        }
        else
        {
            Debug.LogWarning("[SaveView] btn_print not found!");
        }
        
        if (_btnRevert != null)
        {
            _btnRevert.onClick.AddListener(OnRevertButtonClick);
        }
        else
        {
            Debug.LogWarning("[SaveView] btn_revert not found!");
        }
        
        if (_btnSave != null)
        {
            _btnSave.onClick.AddListener(OnSaveButtonClick);
        }
        else
        {
            Debug.LogWarning("[SaveView] btn_save not found!");
        }
    }

->

    // 初始化按钮引用和事件监听
    private void InitializeButtons()
    {
        _btnPrint = transform.Find("btn_print")?.GetComponent<Button>();
        _btnRevert = transform.Find("btn_revert")?.GetComponent<Button>();
        _btnSave = transform.Find("btn_save")?.GetComponent<Button>();
        
        _btnPrint?.onClick.AddListener(OnPrintButtonClick);
        _btnRevert?.onClick.AddListener(OnRevertButtonClick);
        _btnSave?.onClick.AddListener(OnSaveButtonClick);
    }
```

```csharp
    /// <summary>
    /// 天数变化事件处理 - 实现每日自动保存
    /// </summary>
    private void OnDayChanged(DayChangeEvent eventData)
    {
        Debug.Log($"[SaveView] Day changed from {eventData.PreviousDay} to {eventData.CurrentDay}");
        
        // 每天自动保存一次
        if (eventData.CurrentDay != _lastSavedDay)
        {
            AutoSaveDaily(eventData.CurrentDay);
            _lastSavedDay = eventData.CurrentDay;
        }
    }

->

    /// <summary>
    /// 天数变化事件处理 - 实现每日自动保存
    /// </summary>
    private void OnDayChanged(DayChangeEvent eventData)
    {
        // 每天自动保存一次
        if (eventData.CurrentDay != _lastSavedDay)
        {
            AutoSaveDaily(eventData.CurrentDay);
            _lastSavedDay = eventData.CurrentDay;
        }
    }
```

```csharp
    // 游戏保存完成事件处理
    private void OnGameSaved(GameSavedEvent eventData)
    {
        Debug.Log($"[SaveView] Game saved notification: Slot {eventData.Slot} at {eventData.SaveTime}");
    }
    
    // 存档删除事件处理
    private void OnGameSaveDeleted(GameSaveDeletedEvent eventData)
    {
        Debug.Log($"[SaveView] Save deleted notification: Slot {eventData.Slot}");
    }

->

    // 游戏保存完成事件处理
    private void OnGameSaved(GameSavedEvent eventData)
    {
        // 可在此处添加UI提示逻辑
    }
    
    // 存档删除事件处理
    private void OnGameSaveDeleted(GameSaveDeletedEvent eventData)
    {
        // 可在此处添加UI提示逻辑
    }
```

```csharp
        InitializeButtons();
        SubscribeEvents();
        
        // 记录当前天数作为初始值
        _lastSavedDay = ClockModel.Instance.ClockDay;

->

        InitializeButtons();
        SubscribeEvents();
        _lastSavedDay = ClockModel.Instance.ClockDay;
```

完成！我已经按照要求删除了Inspector手动赋值功能并移除了多余的注释和日志。

## 🎯 修改内容

### 1. **移除Inspector手动赋值功能**
- ❌ 删除 `[Header("按钮引用")]`
- ❌ 删除 `[SerializeField]` 标记
- ✅ 按钮字段改为纯private

### 2. **简化按钮初始化逻辑**
- ❌ 删除复杂的null检查逻辑
- ❌ 删除按钮未找到的警告日志
- ✅ 直接使用`transform.Find()`查找
- ✅ 使用`?.`操作符简化事件绑定

### 3. **移除多余的日志输出**
- ❌ 删除天数变化的调试日志
- ❌ 删除事件通知的日志输出
- ❌ 删除多余的注释说明
- ✅ 保留核心功能的操作日志

## 📝 简化后的代码特点

### **InitializeButtons方法** (从34行减少到7行)
```csharp
private void InitializeButtons()
{
    _btnPrint = transform.Find("btn_print")?.GetComponent<Button>();
    _btnRevert = transform.Find("btn_revert")?.GetComponent<Button>();
    _btnSave = transform.Find("btn_save")?.GetComponent<Button>();
    
    _btnPrint?.onClick.AddListener(OnPrintButtonClick);
    _btnRevert?.onClick.AddListener(OnRevertButtonClick);
    _btnSave?.onClick.AddListener(OnSaveButtonClick);
}
```

### **事件处理方法**
- 移除了多余的Debug.Log输出
- 保留了注释作为扩展点
- 代码更加简洁

### **保留的重要日志**
- ✅ 保存操作的成功/失败日志
- ✅ 删除存档的操作日志  
- ✅ 自动保存和手动保存的状态反馈

## 🔧 使用方式

现在SaveView使用更加简洁：
- **纯代码驱动**：不需要在Inspector中配置任何内容
- **自动查找按钮**：按约定命名子物体即可
- **静默运行**：减少不必要的调试信息输出
- **核心功能保留**：所有存档功能完全正常

UI结构要求保持不变：
```
SaveView GameObject
├── btn_print (Button组件)
├── btn_revert (Button组件)  
└── btn_save (Button组件)
```

代码现在更加简洁高效，符合生产环境的要求！

```csharp
private void InitializeButtons()
{
    _btnPrint = transform.Find("btn_print")?.GetComponent<Button>();
    _btnRevert = transform.Find("btn_revert")?.GetComponent<Button>();
    _btnSave = transform.Find("btn_save")?.GetComponent<Button>();
    
    _btnPrint?.onClick.AddListener(OnPrintButtonClick);
    _btnRevert?.onClick.AddListener(OnRevertButtonClick);
    _btnSave?.onClick.AddListener(OnSaveButtonClick);
}
```

```plaintext
SaveView GameObject
├── btn_print (Button组件)
├── btn_revert (Button组件)  
└── btn_save (Button组件)
```

---

**User**

@SaveSystem_TechnicalDocument.md 检查更新文档

---

**Cursor**

我需要检查当前的SaveView实现并更新技术文档。让我先查看SaveView的最新状态：

我需要更新技术文档，添加SaveView组件和DayChangeEvent事件的信息。让我进行更新：

```markdown
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

->

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
```

```markdown
### 事件系统
- `GameSavedEvent` - 游戏保存完成事件
- `GameLoadedEvent` - 游戏加载完成事件  
- `GameSaveDeletedEvent` - 存档删除事件

->

### 事件系统
- `GameSavedEvent` - 游戏保存完成事件
- `GameLoadedEvent` - 游戏加载完成事件  
- `GameSaveDeletedEvent` - 存档删除事件
- `DayChangeEvent` - 天数变化事件（触发每日自动保存）
```

```markdown
void Update()
{
    // 必须调用Update来驱动自动保存
    SaveManager.Instance.Update();
}
```

->

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
```

```markdown
### 组件挂载要求
- `GameSaveController` 需要挂载到GameObject上使用
- `SaveManager` 是纯工具类，无需挂载
- 确保在场景中只有一个`GameSaveController`实例

->

### 组件挂载要求
- `GameSaveController` 需要挂载到GameObject上使用
- `SaveView` 需要挂载到UI GameObject上使用
- `SaveManager` 是纯工具类，无需挂载
- 确保在场景中只有一个`GameSaveController`实例
- `SaveView`可以有多个实例，但通常一个即可满足需求
```

```markdown
### 扩展方法
系统为现有类添加了以下方法：
- `ClockModel.SetGameTime()` - 设置游戏时间
- `PackageModel.ClearAllItems()` - 清空背包
- `DamageableObject.SetHealth()` - 设置血量

->

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
```

```markdown
创建日期：2024-12-19
版本：1.0.0

->

创建日期：2024-12-19
更新日期：2024-12-19
版本：1.1.0
```

## ✅ 技术文档更新完成！

我已经检查并更新了SaveSystem_TechnicalDocument.md，添加了所有最新功能的详细说明：

### 🎯 新增内容

#### 1. **SaveView组件**
- **完整接口说明**：位置、类型、UI结构要求
- **核心功能**：打印进度、删除存档、手动保存、每日自动保存
- **特性说明**：纯代码驱动、自动查找按钮、智能防重复

#### 2. **DayChangeEvent事件**
- 添加到事件系统列表
- 详细说明其触发时机和用途
- 包含前一天和当前天数信息

#### 3. **最佳实践示例**
- **SaveView使用方法**：UI结构创建和功能说明
- **天数变化事件订阅**：完整的代码示例
- **按钮功能说明**：每个按钮的具体作用

#### 4. **组件挂载要求更新**
- 添加SaveView的挂载说明
- 明确多实例使用建议

#### 5. **扩展功能说明**
- **新增事件部分**：DayChangeEvent的详细信息
- **触发位置**：ClockModel.UpdateTimeProgress()
- **应用场景**：每日自动保存和周期性游戏逻辑

### 📋 文档结构优化

```
存档系统技术文档 v1.1.0
├── SaveManager（存档管理器）
├── GameSaveController（存档控制器）
├── SaveView（存档UI视图）        ← 新增
├── 数据结构类
├── 事件系统                     ← 更新
├── 最佳实践                     ← 扩展
├── 注意事项                     ← 更新
└── 其他补充要点                 ← 扩展
```

### 🔧 文档版本信息
- **版本升级**：1.0.0 → 1.1.0
- **更新日期**：2024-12-19
- **新增功能**：SaveView组件、DayChangeEvent事件、每日自动保存

现在技术文档完全反映了当前存档系统的所有功能，包括SaveView的三个按钮、每日自动保存机制和相关事件系统！开发者可以根据文档快速了解和使用完整的存档功能。

```plaintext
存档系统技术文档 v1.1.0
├── SaveManager（存档管理器）
├── GameSaveController（存档控制器）
├── SaveView（存档UI视图）        ← 新增
├── 数据结构类
├── 事件系统                     ← 更新
├── 最佳实践                     ← 扩展
├── 注意事项                     ← 更新
└── 其他补充要点                 ← 扩展
```

---

**User**

生成对应 SaveSystem 的SetDocument文档，遵循markdown格式@/SetDocument 

---

**Cursor**

我需要查看SetDocument文件夹中现有文档的格式和结构，然后为SaveSystem生成对应的SetDocument文档。

```markdown
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
```

## ✅ SaveSystem的SetDocument技术文档已生成完成！

我已经按照SetDocument文件夹的格式规范，为SaveSystem生成了完整的技术指导文档。

### 📋 文档特点

#### **遵循SetDocument格式规范**
- ✅ **文档元信息**：创建日期、版本、文档类型标识
- ✅ **技术指导性质**：面向AI实现的详细指导文档
- ✅ **完整代码示例**：提供具体的实现代码参考

#### **文档结构对比**
```
SetDocument (指导AI实现)          vs    GetDocument (指导开发使用)
├── 核心设计要求                        ├── 简介
├── 架构分层设计                        ├── 详细接口
├── 详细接口定义                        ├── 最佳实践
├── 实现要点(代码示例)                   ├── 注意事项
├── 最佳实践(实现模式)                   └── 补充要点
├── 扩展接口要求
└── 约束条件(规范要求)
```

### 🎯 核心内容

#### **1. 架构设计指导**
```
存档系统架构
├── SaveManager (数据管理层) - 单例模式，纯工具类
├── GameSaveController (控制层) - MonoBehaviour，快捷键管理
├── SaveView (UI交互层) - MonoBehaviour，按钮操作
└── 数据结构层 - GameSaveData, SaveInfo, SavePackageItem
```

#### **2. 技术实现要点**
- **数据收集与应用**：详细的CreateSaveData和ApplySaveData实现
- **PlayerPrefs存储**：键名约定和JSON序列化方案
- **每日自动保存**：DayChangeEvent触发机制

#### **3. 代码实现规范**
- **命名规范**：PascalCase、camelCase+下划线、UPPER_SNAKE_CASE
- **注释规范**：XML注释 vs 单行注释的使用场景
- **错误处理模式**：统一的异常处理和日志输出

#### **4. 扩展接口要求**
明确指出需要为现有系统添加的方法：
- `ClockModel.SetGameTime()`
- `PackageModel.ClearAllItems()`
- `DamageableObject.SetHealth()`

### 📄 文档用途

**SetDocument/SaveSystem_TechnicalDocument.md** 是专门为AI提供的实现指导文档，包含：

- 🔧 **详细的实现要点**：每个功能模块的具体代码实现
- 📝 **完整的代码示例**：可直接参考的代码片段
- 🎯 **明确的约束条件**：命名规范、注释规范、性能要求
- 🏗️ **架构设计原则**：分层设计和接口定义

这样，AI在实现SaveSystem时就有了完整的技术指导文档，确保按照统一的标准和规范进行开发！

```plaintext
SetDocument (指导AI实现)          vs    GetDocument (指导开发使用)
├── 核心设计要求                        ├── 简介
├── 架构分层设计                        ├── 详细接口
├── 详细接口定义                        ├── 最佳实践
├── 实现要点(代码示例)                   ├── 注意事项
├── 最佳实践(实现模式)                   └── 补充要点
├── 扩展接口要求
└── 约束条件(规范要求)
```

```plaintext
存档系统架构
├── SaveManager (数据管理层) - 单例模式，纯工具类
├── GameSaveController (控制层) - MonoBehaviour，快捷键管理
├── SaveView (UI交互层) - MonoBehaviour，按钮操作
└── 数据结构层 - GameSaveData, SaveInfo, SavePackageItem
```

---

**User**

private void PrintCurrentProgressInfo() log打印在一起，不要单独一条一条@SaveView.cs 

---

**Cursor**

```csharp
Debug.Log("=== 当前游戏进度信息 ===");
Debug.Log($"时间信息:");
Debug.Log($"  当前天数: 第{clockModel.ClockDay}天");
Debug.Log($"  当天进度: {(clockModel.DayProgress * 100f):F1}%");
Debug.Log($"  时间段: {GetTimeOfDayName(clockModel.CurrentTimeOfDay)}");
// ... 更多单独的Debug.Log
```

我来修改 `SaveView.cs` 中的 `PrintCurrentProgressInfo()` 方法，将所有日志信息合并在一起打印。

```csharp
    /// <summary>
    /// 打印当前进度信息
    /// 显示时间系统、背包系统和玩家数据的详细信息
    /// </summary>
    private void PrintCurrentProgressInfo()
    {
        Debug.Log("=== 当前游戏进度信息 ===");
        
        // 时间系统信息
        var clockModel = ClockModel.Instance;
        Debug.Log($"时间信息:");
        Debug.Log($"  当前天数: 第{clockModel.ClockDay}天");
        Debug.Log($"  当天进度: {(clockModel.DayProgress * 100f):F1}%");
        Debug.Log($"  时间段: {GetTimeOfDayName(clockModel.CurrentTimeOfDay)}");
        
        // 背包系统信息
        var packageModel = PackageModel.Instance;
        var allItems = packageModel.GetAllItems();
        Debug.Log($"背包信息:");
        Debug.Log($"  道具总数: {allItems.Count}种");
        
        int totalItemCount = 0;
        foreach (var item in allItems)
        {
            totalItemCount += item.count;
            var itemConfig = ItemManager.Instance.GetItem(item.itemId);
            string itemName = itemConfig?.Csv.GetValue<string>(item.itemId, "Name", $"Item_{item.itemId}") ?? $"Item_{item.itemId}";
            Debug.Log($"  - {itemName}: {item.count}个");
        }
        Debug.Log($"  道具总计: {totalItemCount}个");
        
        // 玩家信息
        var player = Player.Instance;
        if (player != null)
        {
            Debug.Log($"玩家信息:");
            Debug.Log($"  当前血量: {player.CurrentHealth:F1}/{player.MaxHealth:F1}");
            Debug.Log($"  位置: {player.transform.position}");
        }
        else
        {
            Debug.Log("玩家信息: 玩家未找到");
        }
        
        // 存档信息
        var saveInfo = SaveManager.Instance.GetSaveInfo(0);
        if (saveInfo != null && !saveInfo.IsEmpty)
        {
            Debug.Log($"存档信息:");
            Debug.Log($"  {saveInfo.GetDisplayText()}");
            Debug.Log($"  版本: {saveInfo.saveVersion}");
        }
        else
        {
            Debug.Log("存档信息: 无存档数据");
        }
        
        Debug.Log("========================");
    }

->

    /// <summary>
    /// 打印当前进度信息
    /// 显示时间系统、背包系统和玩家数据的详细信息
    /// </summary>
    private void PrintCurrentProgressInfo()
    {
        var logBuilder = new System.Text.StringBuilder();
        logBuilder.AppendLine("=== 当前游戏进度信息 ===");
        
        // 时间系统信息
        var clockModel = ClockModel.Instance;
        logBuilder.AppendLine("时间信息:");
        logBuilder.AppendLine($"  当前天数: 第{clockModel.ClockDay}天");
        logBuilder.AppendLine($"  当天进度: {(clockModel.DayProgress * 100f):F1}%");
        logBuilder.AppendLine($"  时间段: {GetTimeOfDayName(clockModel.CurrentTimeOfDay)}");
        
        // 背包系统信息
        var packageModel = PackageModel.Instance;
        var allItems = packageModel.GetAllItems();
        logBuilder.AppendLine("背包信息:");
        logBuilder.AppendLine($"  道具总数: {allItems.Count}种");
        
        int totalItemCount = 0;
        foreach (var item in allItems)
        {
            totalItemCount += item.count;
            var itemConfig = ItemManager.Instance.GetItem(item.itemId);
            string itemName = itemConfig?.Csv.GetValue<string>(item.itemId, "Name", $"Item_{item.itemId}") ?? $"Item_{item.itemId}";
            logBuilder.AppendLine($"  - {itemName}: {item.count}个");
        }
        logBuilder.AppendLine($"  道具总计: {totalItemCount}个");
        
        // 玩家信息
        var player = Player.Instance;
        if (player != null)
        {
            logBuilder.AppendLine("玩家信息:");
            logBuilder.AppendLine($"  当前血量: {player.CurrentHealth:F1}/{player.MaxHealth:F1}");
            logBuilder.AppendLine($"  位置: {player.transform.position}");
        }
        else
        {
            logBuilder.AppendLine("玩家信息: 玩家未找到");
        }
        
        // 存档信息
        var saveInfo = SaveManager.Instance.GetSaveInfo(0);
        if (saveInfo != null && !saveInfo.IsEmpty)
        {
            logBuilder.AppendLine("存档信息:");
            logBuilder.AppendLine($"  {saveInfo.GetDisplayText()}");
            logBuilder.AppendLine($"  版本: {saveInfo.saveVersion}");
        }
        else
        {
            logBuilder.AppendLine("存档信息: 无存档数据");
        }
        
        logBuilder.AppendLine("========================");
        
        // 一次性打印所有信息
        Debug.Log(logBuilder.ToString());
    }
```

## ✅ 修改完成！

我已经将 `SaveView.cs` 中的 `PrintCurrentProgressInfo()` 方法修改为使用 `StringBuilder` 来合并所有日志信息，然后一次性打印。

### 🔄 主要改动

```csharp
// 之前：多条单独的Debug.Log
Debug.Log("=== 当前游戏进度信息 ===");
Debug.Log($"时间信息:");
Debug.Log($"  当前天数: 第{clockModel.ClockDay}天");
// ... 更多单独的日志

// 现在：使用StringBuilder合并后一次性打印
var logBuilder = new System.Text.StringBuilder();
logBuilder.AppendLine("=== 当前游戏进度信息 ===");
logBuilder.AppendLine("时间信息:");
logBuilder.AppendLine($"  当前天数: 第{clockModel.ClockDay}天");
// ... 使用AppendLine构建完整字符串

// 一次性打印所有信息
Debug.Log(logBuilder.ToString());
```

### 📋 修改效果

**之前的效果**：Unity控制台会显示多条分散的日志
```
=== 当前游戏进度信息 ===
时间信息:
  当前天数: 第5天
  当天进度: 75.3%
```

**现在的效果**：Unity控制台只显示一条完整的日志
```
=== 当前游戏进度信息 ===
时间信息:
  当前天数: 第5天
  当天进度: 75.3%
  时间段: 白天
背包信息:
  道具总数: 3种
  - 铁剑: 1个
  - 木盾: 2个
  - 草药: 5个
  道具总计: 8个
玩家信息:
  当前血量: 85.0/100.0
  位置: (10.5, 0.0, 20.3)
存档信息:
  槽位 1: 第5天 - 2024/12/19 15:30:45
  版本: 1
========================
```

现在点击 `btn_print` 按钮时，所有进度信息会在Unity控制台中以一条清晰、完整的日志显示，方便查看和复制！

```csharp
// 之前：多条单独的Debug.Log
Debug.Log("=== 当前游戏进度信息 ===");
Debug.Log($"时间信息:");
Debug.Log($"  当前天数: 第{clockModel.ClockDay}天");
// ... 更多单独的日志

// 现在：使用StringBuilder合并后一次性打印
var logBuilder = new System.Text.StringBuilder();
logBuilder.AppendLine("=== 当前游戏进度信息 ===");
logBuilder.AppendLine("时间信息:");
logBuilder.AppendLine($"  当前天数: 第{clockModel.ClockDay}天");
// ... 使用AppendLine构建完整字符串

// 一次性打印所有信息
Debug.Log(logBuilder.ToString());
```

```plaintext
=== 当前游戏进度信息 ===
时间信息:
  当前天数: 第5天
  当天进度: 75.3%
```

```plaintext
=== 当前游戏进度信息 ===
时间信息:
  当前天数: 第5天
  当天进度: 75.3%
  时间段: 白天
背包信息:
  道具总数: 3种
  - 铁剑: 1个
  - 木盾: 2个
  - 草药: 5个
  道具总计: 8个
玩家信息:
  当前血量: 85.0/100.0
  位置: (10.5, 0.0, 20.3)
存档信息:
  槽位 1: 第5天 - 2024/12/19 15:30:45
  版本: 1
========================
```

