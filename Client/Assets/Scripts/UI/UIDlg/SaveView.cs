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
    private Button _btnPrint;
    private Button _btnRevert;
    private Button _btnSave;
    
    private int _lastSavedDay = -1; // 上次保存的天数
    
    void Start()
    {
        InitializeButtons();
        SubscribeEvents();
        _lastSavedDay = ClockModel.Instance.ClockDay;
    }
    
    void OnDestroy()
    {
        UnsubscribeEvents();
    }
    
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
