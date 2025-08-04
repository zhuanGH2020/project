using System;
using System.Collections.Generic;
using UnityEngine;

// 存档模型 - 负责存档系统的业务逻辑管理
// 提供存档数据的统一访问接口和自动保存机制
// 遵循项目Model层设计模式，整合了存档管理的所有功能
public class SaveModel
{
    private static SaveModel _instance;
    public static SaveModel Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveModel();
            return _instance;
        }
    }

    private const string SAVE_KEY_PREFIX = "GameSave_";
    private const string SAVE_VERSION_KEY = "SaveVersion_";
    private const string SAVE_TIME_KEY = "SaveTime_";
    private const int CURRENT_SAVE_VERSION = 1;
    private const int MAX_SAVE_SLOTS = 5;
    
    private bool _autoLoadOnStart = true;
    private bool _autoSaveOnQuit = true;
    private bool _enableAutoSave = true;
    private float _autoSaveInterval = 300f; // 5分钟
    private float _lastAutoSaveTime;
    
    private bool _initialized = false;
    
    // 私有构造函数，确保单例模式
    private SaveModel() 
    {
        _lastAutoSaveTime = Time.time;
    }
    
    // 初始化存档模型
    public void Initialize()
    {
        if (_initialized) return;
        
        SetAutoSave(true, _autoSaveInterval);
        
        // 游戏启动时自动加载存档
        if (_autoLoadOnStart && HasSaveData(0))
        {
            bool loadSuccess = LoadGame(0);
            Debug.Log($"[SaveModel] Auto load on start: {(loadSuccess ? "Success" : "Failed")}");
        }
        
        // 订阅存档事件
        EventManager.Instance.Subscribe<GameSavedEvent>(OnGameSaved);
        EventManager.Instance.Subscribe<GameLoadedEvent>(OnGameLoaded);
        EventManager.Instance.Subscribe<GameSaveDeletedEvent>(OnGameSaveDeleted);
        
        _initialized = true;
        Debug.Log("[SaveModel] Initialized successfully");
    }
    
    // 更新方法 - 处理自动保存逻辑
    public void Update()
    {
        if (!_initialized) return;
        
        // 自动保存逻辑
        if (_enableAutoSave && Time.time - _lastAutoSaveTime >= _autoSaveInterval)
        {
            AutoSave();
            _lastAutoSaveTime = Time.time;
        }
    }
    
    // 清理资源
    public void Cleanup()
    {
        if (!_initialized) return;
        
        // 退出时自动保存
        if (_autoSaveOnQuit)
        {
            SaveGame(0);
            //Debug.Log("[SaveModel] Auto save on quit");
        }
        
        // 取消订阅事件
        EventManager.Instance.Unsubscribe<GameSavedEvent>(OnGameSaved);
        EventManager.Instance.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        EventManager.Instance.Unsubscribe<GameSaveDeletedEvent>(OnGameSaveDeleted);
        
        _initialized = false;
    }
    
    /// <summary>
    /// 应用暂停/失焦时的自动保存
    /// </summary>
    public void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && _autoSaveOnQuit)
        {
            SaveGame(0);
            Debug.Log("[SaveModel] Auto save on pause");
        }
    }
    
    // 设置存档配置
    public void SetSaveConfig(bool autoLoadOnStart = true, bool autoSaveOnQuit = true, float autoSaveInterval = 300f)
    {
        _autoLoadOnStart = autoLoadOnStart;
        _autoSaveOnQuit = autoSaveOnQuit;
        _autoSaveInterval = autoSaveInterval;
        
        // 如果已经初始化，更新自动保存设置
        if (_initialized)
        {
            SetAutoSave(true, _autoSaveInterval);
        }
        
        Debug.Log($"[SaveModel] Save config updated - AutoLoad: {autoLoadOnStart}, AutoSave: {autoSaveOnQuit}, Interval: {autoSaveInterval}s");
    }
    
    // ===== 核心存档方法 (从SaveManager迁移) =====
    
    // 保存游戏到指定槽位，slot: 存档槽位 (0-4)，返回: 保存是否成功
    public bool SaveGame(int slot = 0)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError($"[SaveModel] Invalid save slot: {slot}. Must be 0-{MAX_SAVE_SLOTS - 1}");
            return false;
        }
        
        try
        {
            SaveData saveData = CreateSaveData();
            string jsonData = JsonUtility.ToJson(saveData, false);
            
            string saveKey = GetSaveKey(slot);
            PlayerPrefs.SetString(saveKey, jsonData);
            
            PlayerPrefs.SetInt(GetVersionKey(slot), CURRENT_SAVE_VERSION);
            
            PlayerPrefs.SetString(GetTimeKey(slot), saveData.saveTime);
            
            // 立即写入磁盘
            PlayerPrefs.Save();
            
            //Debug.Log($"[SaveModel] Game saved to slot {slot} at {saveData.saveTime}");
            
            EventManager.Instance.Publish(new GameSavedEvent(slot, saveData.saveTime));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveModel] Failed to save game to slot {slot}: {e.Message}");
            return false;
        }
    }
    
    // 从指定槽位加载游戏，slot: 存档槽位 (0-4)，返回: 加载是否成功
    public bool LoadGame(int slot = 0)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError($"[SaveModel] Invalid save slot: {slot}");
            return false;
        }
        
        if (!HasSaveData(slot))
        {
            Debug.LogWarning($"[SaveModel] No save data found in slot {slot}");
            return false;
        }
        
        try
        {
            string saveKey = GetSaveKey(slot);
            string jsonData = PlayerPrefs.GetString(saveKey);
            
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError($"[SaveModel] Save data is empty in slot {slot}");
                return false;
            }
            
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
            if (saveData == null)
            {
                Debug.LogError($"[SaveModel] Failed to parse save data in slot {slot}");
                return false;
            }
            
            // 版本兼容性检查
            int saveVersion = PlayerPrefs.GetInt(GetVersionKey(slot), 1);
            if (saveVersion > CURRENT_SAVE_VERSION)
            {
                Debug.LogWarning($"[SaveModel] Save version {saveVersion} is newer than current {CURRENT_SAVE_VERSION}");
            }
            
            ApplySaveData(saveData);
            Debug.Log($"[SaveModel] Game loaded from slot {slot} - {saveData.saveTime}");
            
            EventManager.Instance.Publish(new GameLoadedEvent(slot, saveData.saveTime));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveModel] Failed to load game from slot {slot}: {e.Message}");
            return false;
        }
    }
    
    // 检查指定槽位是否有存档数据
    public bool HasSaveData(int slot = 0)
    {
        if (!IsValidSlot(slot)) return false;
        return PlayerPrefs.HasKey(GetSaveKey(slot));
    }
    
    // 获取存档信息（不加载游戏）
    public SaveInfo GetSaveInfo(int slot = 0)
    {
        if (!HasSaveData(slot)) return null;
        
        try
        {
            string saveTime = PlayerPrefs.GetString(GetTimeKey(slot), "Unknown");
            int saveVersion = PlayerPrefs.GetInt(GetVersionKey(slot), 1);
            string jsonData = PlayerPrefs.GetString(GetSaveKey(slot));
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
            
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
            Debug.LogError($"[SaveModel] Failed to get save info for slot {slot}: {e.Message}");
            return null;
        }
    }
    
    // 获取所有存档槽位的信息
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
    
    // 删除指定槽位的存档
    public bool DeleteSaveData(int slot = 0)
    {
        if (!IsValidSlot(slot)) return false;
        
        try
        {
            PlayerPrefs.DeleteKey(GetSaveKey(slot));
            PlayerPrefs.DeleteKey(GetVersionKey(slot));
            PlayerPrefs.DeleteKey(GetTimeKey(slot));
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveModel] Save data deleted from slot {slot}");
            
            EventManager.Instance.Publish(new GameSaveDeletedEvent(slot));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveModel] Failed to delete save data from slot {slot}: {e.Message}");
            return false;
        }
    }
    
    // 清空所有存档数据
    public void ClearAllSaveData()
    {
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            DeleteSaveData(i);
        }
        Debug.Log("[SaveModel] All save data cleared");
    }
    
    // 设置自动保存，enabled: 是否启用自动保存，intervalSeconds: 自动保存间隔（秒）
    public void SetAutoSave(bool enabled, float intervalSeconds = 300f)
    {
        _enableAutoSave = enabled;
        _autoSaveInterval = intervalSeconds;
        Debug.Log($"[SaveModel] Auto save: {enabled}, Interval: {intervalSeconds}s");
    }
    
    private void AutoSave()
    {
        if (SaveGame(0)) // 自动保存到槽位0
        {
            Debug.Log("[SaveModel] Auto save completed");
        }
        _lastAutoSaveTime = Time.time;
    }
    
    // 创建当前游戏状态的存档数据
    private SaveData CreateSaveData()
    {
        SaveData saveData = new SaveData();
        
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
            saveData.packageItems.Add(item);
        }
        
        // 保存玩家数据
        var player = Player.Instance;
        if (player != null)
        {
            saveData.currentHealth = player.CurrentHealth;
            saveData.playerPosition = player.transform.position;
            saveData.equippedItems = player.GetEquippedItemIds();
        }
        
        // 保存存档信息
        saveData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        saveData.saveVersion = CURRENT_SAVE_VERSION;
        
        return saveData;
    }
    
    // 应用存档数据到游戏系统
    private void ApplySaveData(SaveData saveData)
    {
        // 应用时间系统数据
        ClockModel.Instance.SetGameTime(saveData.clockDay, saveData.dayProgress, saveData.currentTimeOfDay);
        
        // 应用背包数据
        var packageModel = PackageModel.Instance;
        packageModel.LoadItemsFromSave(saveData.packageItems);
        
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
        
        Debug.Log($"[SaveModel] Save data applied successfully - Day {saveData.clockDay}, Health {saveData.currentHealth}, Items {saveData.packageItems.Count}");
    }
    
    // === 辅助方法 ===
    
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
    
    // 快速保存到槽位0
    public void QuickSave()
    {
        bool saveSuccess = SaveGame(0);
        Debug.Log($"[SaveModel] Quick save: {(saveSuccess ? "Success" : "Failed")}");
    }
    
    // 快速从槽位0加载
    public void QuickLoad()
    {
        bool loadSuccess = LoadGame(0);
        Debug.Log($"[SaveModel] Quick load: {(loadSuccess ? "Success" : "Failed")}");
    }
    
    // 保存到指定槽位
    public void SaveToSlot(int slot)
    {
        bool saveSuccess = SaveGame(slot);
        Debug.Log($"[SaveModel] Save to slot {slot}: {(saveSuccess ? "Success" : "Failed")}");
    }
    
    // 从指定槽位加载
    public void LoadFromSlot(int slot)
    {
        bool loadSuccess = LoadGame(slot);
        Debug.Log($"[SaveModel] Load from slot {slot}: {(loadSuccess ? "Success" : "Failed")}");
    }
    
    // 删除指定槽位的存档
    public void DeleteSlot(int slot)
    {
        bool deleteSuccess = DeleteSaveData(slot);
        Debug.Log($"[SaveModel] Delete slot {slot}: {(deleteSuccess ? "Success" : "Failed")}");
    }
    
    // 显示所有存档信息（调试用）
    private void ShowAllSaveInfo()
    {
        var saveInfos = GetAllSaveInfo();
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
    
    // 游戏保存完成事件处理
    private void OnGameSaved(GameSavedEvent eventData)
    {
        Debug.Log($"[SaveModel] Game saved to slot {eventData.Slot} at {eventData.SaveTime}");
        // 这里可以添加UI提示等逻辑
    }
    
    // 游戏加载完成事件处理  
    private void OnGameLoaded(GameLoadedEvent eventData)
    {
        Debug.Log($"[SaveModel] Game loaded from slot {eventData.Slot} - {eventData.SaveTime}");
        // 这里可以添加UI提示等逻辑
    }
    
    // 存档删除事件处理
    private void OnGameSaveDeleted(GameSaveDeletedEvent eventData)
    {
        Debug.Log($"[SaveModel] Save data deleted from slot {eventData.Slot}");
        // 这里可以添加UI提示等逻辑
    }
} 