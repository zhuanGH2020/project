using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 调试视图 - 提供调试相关UI交互功能
/// 支持打印当前进度信息、删除存档、每日自动保存和UI调试功能
/// </summary>
public class DebugView : BaseView
{
    private Button _btnPrint;
    private Button _btnRevert;
    private Button _btnSave;
    private Button _btnGetMat; // 获取材料按钮
    private Button _btnGetCustomItem; // 获取自定义物品按钮
    private Button _btnGetMonster; // 生成怪物按钮
    private Button _btnGetObject; // 生成可采集物体按钮
    
    private Toggle _toggleUIPath; // UI路径打印开关
    private Toggle _toggleUITime; // 时间系统开关
    
    private TMP_InputField _inputItemId; // 物品ID输入框
    
    private int _lastSavedDay = -1; // 上次保存的天数
    
    // UI路径打印开关状态变化事件处理
    private void OnUIPathToggleChanged(bool isOn)
    {
        DebugModel.Instance.SetUIPathPrintEnabled(isOn);
    }
    
    // 时间系统开关状态变化事件处理
    private void OnUITimeToggleChanged(bool isOn)
    {
        DebugModel.Instance.SetTimeEnabled(isOn);
    }
    
    void Start()
    {
        InitializeButtons();
        SubscribeEvents();
        _lastSavedDay = ClockModel.Instance.ClockDay;
    }
    
    void OnDestroy()
    {
        UnsubscribeEvents();
        
        // 取消UI组件监听器订阅
        _toggleUIPath?.onValueChanged.RemoveListener(OnUIPathToggleChanged);
        _toggleUITime?.onValueChanged.RemoveListener(OnUITimeToggleChanged);
    }
    
    // 初始化按钮引用和事件监听
    private void InitializeButtons()
    {
        _btnPrint = transform.Find("btn_print")?.GetComponent<Button>();
        _btnRevert = transform.Find("btn_revert")?.GetComponent<Button>();
        _btnSave = transform.Find("btn_save")?.GetComponent<Button>();
        _btnGetMat = transform.Find("btn_get_mat")?.GetComponent<Button>();
        _btnGetCustomItem = transform.Find("ui_group_get_item/btn_get_custom_item")?.GetComponent<Button>();
        _btnGetMonster = transform.Find("btn_get_monster")?.GetComponent<Button>();
        _btnGetObject = transform.Find("btn_get_object")?.GetComponent<Button>();
        _toggleUIPath = transform.Find("toggle_ui_path")?.GetComponent<Toggle>();
        _toggleUITime = transform.Find("toggle_ui_time")?.GetComponent<Toggle>();
        _inputItemId = transform.Find("ui_group_get_item/input_item_id")?.GetComponent<TMP_InputField>();
        
        _btnPrint?.onClick.AddListener(OnPrintButtonClick);
        _btnRevert?.onClick.AddListener(OnRevertButtonClick);
        _btnSave?.onClick.AddListener(OnSaveButtonClick);
        _btnGetMat?.onClick.AddListener(OnGetMatButtonClick);
        _btnGetCustomItem?.onClick.AddListener(OnGetCustomItemButtonClick);
        _btnGetMonster?.onClick.AddListener(OnGetMonsterButtonClick);
        _btnGetObject?.onClick.AddListener(OnGetObjectButtonClick);
        _toggleUIPath?.onValueChanged.AddListener(OnUIPathToggleChanged);
        _toggleUITime?.onValueChanged.AddListener(OnUITimeToggleChanged);
        
        // 同步Toggle状态和DebugModel状态
        if (_toggleUIPath != null)
        {
            _toggleUIPath.isOn = DebugModel.Instance.IsUIPathPrintEnabled;
        }
        
        if (_toggleUITime != null)
        {
            _toggleUITime.isOn = DebugModel.Instance.IsTimeEnabled;
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
    
    // 保存按钮点击事件
    private void OnSaveButtonClick()
    {
        ManualSave();
    }
    
    // 获取材料按钮点击事件
    private void OnGetMatButtonClick()
    {
        GetMaterials();
    }
    
    // 获取自定义物品按钮点击事件
    private void OnGetCustomItemButtonClick()
    {
        GetCustomItem();
    }
    
    // 生成怪物按钮点击事件
    private void OnGetMonsterButtonClick()
    {
        GetMonster();
    }
    
    // 生成可采集物体按钮点击事件
    private void OnGetObjectButtonClick()
    {
        GetHarvestableObject();
    }
    
    /// <summary>
    /// 天数变化事件处理 - 实现每日自动保存
    /// </summary>
    private void OnDayChanged(DayChangeEvent eventData)
    {
        Debug.Log($"[DebugView] Day changed from {eventData.PreviousDay} to {eventData.CurrentDay}");
        
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
        bool saveSuccess = DebugModel.Instance.ManualSave();
        if (saveSuccess)
        {
            Debug.Log($"[DebugView] Daily auto save completed on day {currentDay}");
        }
        else
        {
            Debug.LogError($"[DebugView] Daily auto save failed on day {currentDay}");
        }
    }
    
    /// <summary>
    /// 手动保存游戏进度
    /// 保存当前游戏状态到槽位0，并更新最后保存天数记录
    /// </summary>
    private void ManualSave()
    {
        bool saveSuccess = DebugModel.Instance.ManualSave();
        if (saveSuccess)
        {
            // 更新最后保存天数，避免当天重复自动保存
            _lastSavedDay = ClockModel.Instance.ClockDay;
            Debug.Log($"[DebugView] Manual save completed on day {_lastSavedDay}");
        }
        else
        {
            Debug.LogError("[DebugView] Manual save failed");
        }
    }
    
    /// <summary>
    /// 获取材料 - 向背包添加多种材料
    /// 通过DebugModel封装业务逻辑
    /// </summary>
    private void GetMaterials()
    {
        DebugModel.Instance.GetMaterials();
    }
    
    /// <summary>
    /// 获取自定义物品 - 根据输入框中的物品ID获得一个物品
    /// </summary>
    private void GetCustomItem()
    {
        if (_inputItemId == null)
        {
            Debug.LogWarning("[DebugView] Item ID input field not found");
            return;
        }
        
        string inputText = _inputItemId.text.Trim();
        if (string.IsNullOrEmpty(inputText))
        {
            Debug.LogWarning("[DebugView] Please enter an item ID");
            return;
        }
        
        if (int.TryParse(inputText, out int itemId))
        {
            DebugModel.Instance.GetCustomItem(itemId);
        }
        else
        {
            Debug.LogWarning($"[DebugView] Invalid item ID format: {inputText}");
        }
    }
    
    /// <summary>
    /// 生成怪物 - 手动生成一个怪物
    /// </summary>
    private void GetMonster()
    {
        MapManager.Instance.ManualSpawnMonster();
    }
    
    /// <summary>
    /// 生成可采集物体 - 手动生成一个灌木丛
    /// </summary>
    private void GetHarvestableObject()
    {
        int itemId = GameSettings.DebugDefaultHarvestableObjectId; // 从GameSettings读取默认ID
        
        // 从Item配置表获取物品信息
        var itemConfig = ConfigManager.Instance.GetReader("Item");
        if (itemConfig == null || !itemConfig.HasKey(itemId))
        {
            Debug.LogError($"[DebugView] Item config not found for ID: {itemId}");
            return;
        }

        // 从配置表读取预制体路径
        string prefabPath = itemConfig.GetValue<string>(itemId, "PrefabPath", "");
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogWarning($"[DebugView] No prefab path found for item {itemId}, creating default object");
            return;
        }

        // 使用ResourceManager加载预制体
        GameObject prefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[DebugView] Prefab not found at path: {prefabPath}, creating default object");
            return;
        }

        // 获取安全的生成位置（在玩家附近）
        Vector3 spawnPosition = GetPlayerNearbyPosition();
        
        // 实例化对象
        GameObject objectInstance = Object.Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // 获取HarvestableObject组件并初始化
        var harvestableComponent = objectInstance.GetComponent<HarvestableObject>();
        if (harvestableComponent != null)
        {
            harvestableComponent.Init(itemId);
            Debug.Log($"[DebugView] Successfully spawned harvestable object {itemId} at {spawnPosition}");
        }
        else
        {
            Debug.LogWarning($"[DebugView] HarvestableObject component not found on prefab, adding it");
            harvestableComponent = objectInstance.AddComponent<HarvestableObject>();
            harvestableComponent.Init(itemId);
        }
    }
    
    /// <summary>
    /// 获取玩家附近的安全位置
    /// </summary>
    private Vector3 GetPlayerNearbyPosition()
    {
        var player = Player.Instance;
        if (player != null)
        {
            // 在玩家前方2-4米的随机位置生成
            Vector3 playerPos = player.transform.position;
            Vector3 playerForward = player.transform.forward;
            float distance = Random.Range(2f, 4f);
            Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            return playerPos + playerForward * distance + randomOffset;
        }
        else
        {
            // 如果没有玩家，在默认位置生成
            return new Vector3(0, 0, 0);
        }
    }
    
    /// <summary>
    /// 打印当前进度信息
    /// 通过DebugModel获取格式化的进度信息
    /// </summary>
    private void PrintCurrentProgressInfo()
    {
        string progressInfo = DebugModel.Instance.GetCurrentProgressInfo();
        Debug.Log(progressInfo);
    }
    

    
    /// <summary>
    /// 删除当前存档
    /// 删除槽位0的存档数据，并重置游戏状态到初始状态
    /// </summary>
    private void DeleteCurrentSave()
    {
        bool deleteSuccess = DebugModel.Instance.DeleteCurrentSaveAndReset();
        if (deleteSuccess)
        {
            // 更新最后保存天数
            _lastSavedDay = 1;
            Debug.Log("[DebugView] Current save deleted and game state reset successfully");
        }
        else
        {
            Debug.LogError("[DebugView] Failed to delete current save or no save data to delete");
        }
    }
    
    // 游戏保存完成事件处理
    private void OnGameSaved(GameSavedEvent eventData)
    {
        //Debug.Log($"[DebugView] Game saved notification: Slot {eventData.Slot} at {eventData.SaveTime}");
    }
    
    // 存档删除事件处理
    private void OnGameSaveDeleted(GameSaveDeletedEvent eventData)
    {
        Debug.Log($"[DebugView] Save deleted notification: Slot {eventData.Slot}");
    }
}
