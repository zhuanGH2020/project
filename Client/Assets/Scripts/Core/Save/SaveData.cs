using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档数据 - 存储所有需要持久化的游戏进度
/// 包含时间系统、背包系统、玩家数据等核心游戏状态
/// </summary>
[System.Serializable]
public class SaveData
{   
    [Header("时间系统")]
    public int clockDay = 1;                    // 当前天数
    public float dayProgress = 0f;              // 当天进度 (0.0-1.0)
    public TimeOfDay currentTimeOfDay = TimeOfDay.Day; // 当前时间段
    
    [Header("背包系统")]
    public List<PackageItem> packageItems = new List<PackageItem>(); // 背包道具列表
    
    [Header("玩家数据")]
    public float currentHealth = 100f;          // 当前血量
    public Vector3 playerPosition = Vector3.zero; // 玩家位置
    public List<int> equippedItems = new List<int>(); // 已装备的道具ID列表
    
    [Header("建筑系统")]
    public List<MapData> buildingData = new List<MapData>(); // 建筑物数据列表
    
    [Header("存档信息")]
    public string saveTime;                     // 存档时间
    public int saveVersion = 1;                 // 存档版本号
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
    public int buildingCount;     // 建筑数量
    
    // 是否为空槽位
    public bool IsEmpty => string.IsNullOrEmpty(saveTime) || saveTime == "Unknown";
    
    // 获取显示文本
    public string GetDisplayText()
    {
        if (IsEmpty)
            return $"槽位 {slot + 1}: 空";
        
        return $"槽位 {slot + 1}: 第{clockDay}天 - {saveTime}";
    }
} 