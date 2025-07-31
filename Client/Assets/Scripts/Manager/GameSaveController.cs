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
    
    // 处理快捷键操作
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
    
    // 显示所有存档信息（调试用）
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
} 