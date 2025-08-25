using UnityEngine;

/// <summary>
/// 调试管理器 - 管理调试相关的状态和业务逻辑
/// 遵循项目管理器架构，统一管理所有调试功能
/// </summary>
public class DebugManager
{
    private static DebugManager _instance;
    public static DebugManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new DebugManager();
            return _instance;
        }
    }

    private bool _enableUIPathPrint = false; // UI路径打印开关

    private DebugManager() { }

    /// <summary>
    /// 设置UI路径打印开关
    /// </summary>
    public void SetUIPathPrintEnabled(bool enabled)
    {
        _enableUIPathPrint = enabled;
        
        // 发布状态变化事件（如果需要通知其他组件）
        EventManager.Instance.Publish(new UIPathPrintToggleEvent(enabled));
    }

    /// <summary>
    /// 获取UI路径打印开关状态
    /// </summary>
    public bool IsUIPathPrintEnabled => _enableUIPathPrint;

    /// <summary>
    /// 设置时间系统开关
    /// </summary>
    public void SetTimeEnabled(bool enabled)
    {
        if (enabled)
        {
            ClockModel.Instance.ResumeTime();
        }
        else
        {
            ClockModel.Instance.PauseTime();
        }
    }

    /// <summary>
    /// 获取时间系统是否启用状态
    /// </summary>
    public bool IsTimeEnabled => !ClockModel.Instance.IsTimePaused;

    /// <summary>
    /// 调试方法：强制设置为夜晚状态来测试夜晚效果
    /// </summary>
    public void ForceSetNightTime()
    {
        ClockModel.Instance.SetGameTime(1, 0.9f, TimeOfDay.Night);
        Debug.Log("[DebugManager] Forced set to Night time for testing");
    }

    /// <summary>
    /// 获取材料 - 业务逻辑封装
    /// </summary>
    public void GetMaterials()
    {
        // 使用GameSettings中的配置数据
        var materials = GameSettings.DebugMaterials;

        int successCount = 0;
        var obtainedItems = new System.Collections.Generic.List<string>();

        foreach (var material in materials)
        {
            int itemId = material.Key;
            int count = material.Value;

            bool addSuccess = PackageModel.Instance.AddItem(itemId, count);
            if (addSuccess)
            {
                var itemConfig = ItemManager.Instance.GetItem(itemId);
                string itemName = itemConfig?.Csv.GetValue<string>(itemId, "Name", $"Item_{itemId}") ?? $"Item_{itemId}";
                obtainedItems.Add($"{itemName} x{count}");
                successCount++;
            }
        }

        // 发布通知事件
        if (obtainedItems.Count > 0)
        {
            string noticeMessage = $"获得材料：{string.Join("、", obtainedItems)}";
            EventManager.Instance.Publish(new NoticeEvent(noticeMessage));
        }
        else
        {
            EventManager.Instance.Publish(new NoticeEvent("材料获取失败"));
        }
    }

    /// <summary>
    /// 获取指定ID的物品一个
    /// </summary>
    public void GetCustomItem(int itemId)
    {
        if (itemId <= 0)
        {
            EventManager.Instance.Publish(new NoticeEvent("请输入有效的物品ID"));
            return;
        }

        bool addSuccess = PackageModel.Instance.AddItem(itemId, 1);
        if (addSuccess)
        {
            var itemConfig = ItemManager.Instance.GetItem(itemId);
            string itemName = itemConfig?.Csv.GetValue<string>(itemId, "Name", $"Item_{itemId}") ?? $"Item_{itemId}";
            string noticeMessage = $"获得物品：{itemName} x1";
            EventManager.Instance.Publish(new NoticeEvent(noticeMessage));
            Debug.Log($"[DebugManager] Successfully obtained item: {itemName} (ID: {itemId})");
        }
        else
        {
            EventManager.Instance.Publish(new NoticeEvent($"获取物品失败 (ID: {itemId})"));
            Debug.LogWarning($"[DebugManager] Failed to obtain item with ID: {itemId}");
        }
    }

    /// <summary>
    /// 手动保存游戏进度
    /// </summary>
    public bool ManualSave()
    {
        return SaveManager.Instance.SaveGame(0);
    }

    /// <summary>
    /// 删除当前存档并重置游戏状态
    /// </summary>
    public bool DeleteCurrentSaveAndReset()
    {
        bool hasData = SaveManager.Instance.HasSaveData(0);
        if (!hasData)
        {
            Debug.Log("[DebugManager] No save data to delete");
            return false;
        }

        bool deleteSuccess = SaveManager.Instance.DeleteSaveData(0);
        if (deleteSuccess)
        {
            // 重置游戏状态
            SaveManager.Instance.ClearCurrentGameData();
            Debug.Log("[DebugManager] Current save deleted and game state reset");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取当前游戏进度信息
    /// </summary>
    public string GetCurrentProgressInfo()
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
        return logBuilder.ToString();
    }

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
} 