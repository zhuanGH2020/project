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
    
    // 执行自动保存
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
} 