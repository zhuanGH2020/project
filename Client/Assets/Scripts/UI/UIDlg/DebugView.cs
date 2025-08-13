using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    
    private Toggle _toggleUIPath; // UI路径打印开关
    private Toggle _toggleUITime; // 时间系统开关
    
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
        _toggleUIPath = transform.Find("toggle_ui_path")?.GetComponent<Toggle>();
        _toggleUITime = transform.Find("toggle_ui_time")?.GetComponent<Toggle>();
        
        _btnPrint?.onClick.AddListener(OnPrintButtonClick);
        _btnRevert?.onClick.AddListener(OnRevertButtonClick);
        _btnSave?.onClick.AddListener(OnSaveButtonClick);
        _btnGetMat?.onClick.AddListener(OnGetMatButtonClick);
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
