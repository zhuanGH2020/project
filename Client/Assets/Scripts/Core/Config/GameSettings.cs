using UnityEngine;

// 游戏静态配置 - 直接通过 GameSettings.MapSpawnInterval 访问
public static class GameSettings
{
    #region Map配置
    // 地图怪物生成间隔（秒）
    public const float MapSpawnInterval = 5f;

    // 默认怪物生成位置
    public static readonly Vector3 MapDefaultSpawnPosition = Vector3.zero;

    // 默认可生成的怪物ID列表
    public static readonly int[] MapDefaultMonsterIds = { 5001 };

    // 最小生成间隔限制（秒）
    public const float MapMinSpawnInterval = 0.1f;
    #endregion

    #region Package配置
    // 背包最大格子数量
    public const int PackageMaxSlots = 9;
    #endregion

    #region Clock配置
    // 游戏中一天的持续时间（秒）
    public const float ClockDayDuration = 60f;

    // 游戏最大天数
    public const int ClockMaxDays = 60;

    // 白天环境光强度
    public const float ClockDayAmbientIntensity = 1.0f;

    // 黄昏环境光强度
    public const float ClockDuskAmbientIntensity = 0.6f;

    // 夜晚环境光强度
    public const float ClockNightAmbientIntensity = 0.3f;

    // 白天时间占比 (0.0-0.5)
    public const float ClockDayTimeRatio = 0.5f;

    // 黄昏时间占比 (0.5-0.75)
    public const float ClockDuskTimeRatio = 0.75f;
    #endregion
} 