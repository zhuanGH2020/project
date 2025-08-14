using UnityEngine;

// 游戏静态配置 - 直接通过 GameSettings.MapSpawnInterval 访问
public static class GameSettings
{
    #region Map配置
    // 地图怪物生成间隔（秒）
    public const float MapSpawnInterval = 30f;

    // 默认怪物生成位置
    public static readonly Vector3 MapDefaultSpawnPosition = Vector3.zero;

    // 默认可生成的怪物ID列表
    public static readonly int[] MapDefaultMonsterIds = { 5000 };
    //public static readonly int[] MapDefaultMonsterIds = {  };

    // 最小生成间隔限制（秒）
    public const float MapMinSpawnInterval = 0.1f;
    #endregion

    #region Package配置
    // 背包最大格子数量
    public const int PackageMaxSlots = 9;
    #endregion

    #region Player配置
    // 玩家最大血量
    public const float PlayerMaxHealth = 100f;
    #endregion

    #region Clock配置
    // 游戏中一天的持续时间（秒）
    public const float ClockDayDuration = 60f;

    // 游戏最大天数
    public const int ClockMaxDays = 60;

    // 时间段比例配置
    public const float ClockDayTimeRatio = 0.5f;        // 白天时间占比
    public const float ClockDuskTimeRatio = 0.25f;       // 黄昏时间占比
    public const float ClockNightTimeRatio = 0.25f;      // 夜晚时间占比

    // 主光源颜色配置
    public static readonly Color ClockDayMainLightColor = Color.white;                  // 白天主光源颜色
    public static readonly Color ClockDuskMainLightColor = new Color(1f, 0.8f, 0.6f);  // 黄昏主光源颜色
    public static readonly Color ClockNightMainLightColor = new Color(0.6f, 0.7f, 1f); // 夜晚主光源颜色

    // 主光源旋转配置
    public static readonly Vector3 ClockDayMainLightRotation = new Vector3(56f, 197f, -72f);    // 白天主光源旋转角度
    public static readonly Vector3 ClockDuskMainLightRotation = new Vector3(35f, 336f, 81f);    // 黄昏主光源旋转角度
    public static readonly Vector3 ClockNightMainLightRotation = new Vector3(-39f, 173f, -77f); // 夜晚主光源旋转角度

    // 主光源旋转过渡时间配置
    public const float ClockNightToDayRotationTime = 5f;     // 夜晚到白天旋转时间（秒）
    public const float ClockDayToDuskRotationTime = 2f;      // 白天到黄昏旋转时间（秒）
    public const float ClockDuskToNightRotationTime = 4f;    // 黄昏到夜晚旋转时间（秒）
    #endregion
} 