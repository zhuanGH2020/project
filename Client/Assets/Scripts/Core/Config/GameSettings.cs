using UnityEngine;

// 游戏静态配置 - 直接通过 GameSettings.MapSpawnInterval 访问
public static class GameSettings
{
    #region Map配置
    // 地图怪物生成间隔（秒）
    public const float MapSpawnInterval = 1000000f;

    // 默认怪物生成位置（XZ坐标，Y坐标通过射线检测地面计算）
    public static readonly Vector2 MapDefaultSpawnPosition = Vector2.zero;

    // 默认可生成的怪物ID列表
    public static readonly int[] MapDefaultMonsterIds = { 5001 };
    //public static readonly int[] MapDefaultMonsterIds = {  };

    // 最小生成间隔限制（秒）
    public const float MapMinSpawnInterval = 0.1f;
    
    // 安全生成位置配置
    public const float MapSafeSpawnCheckRadius = 1.5f;     // 冲突检测半径（米）
    public const float MapSafeSpawnSearchRadius = 8f;      // 搜索安全位置的范围（米）
    public const int MapSafeSpawnMaxAttempts = 15;         // 最大搜索尝试次数
    #endregion

    #region Package配置
    // 背包最大格子数量
    public const int PackageMaxSlots = 9;
    #endregion

    #region Player配置
    // 玩家最大血量
    public const float PlayerMaxHealth = 100f;
    
    // 玩家最大饥饿值
    public const float PlayerMaxHunger = 100f;
    
    // 玩家最大理智值
    public const float PlayerMaxSanity = 100f;
    #endregion

    #region Clock配置
    // 游戏中一天的持续时间（秒）
    public const float ClockDayDuration = 60f;

    // 游戏最大天数
    public const int ClockMaxDays = 60;

    // 是否启用灯光效果（为false时关闭所有灯光变化，包括初始化和状态改变效果）
    public static readonly bool ClockEnableLightingEffects = false;

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

    #region Debug配置
    // 调试模式获取材料的物品列表 - 物品ID和数量的键值对
    public static readonly System.Collections.Generic.Dictionary<int, int> DebugMaterials = 
        new System.Collections.Generic.Dictionary<int, int>
        {
            { 13001, 1 },  // 木头 - 基础材料，用于制作各种物品
            { 14001, 1 },  // 石头 - 基础材料，用于制作工具和建筑
            { 14002, 1 },  // 燧石 - 基础材料，用于制作武器和护甲
            { 14003, 1 },  // 金块 - 基础材料，用于制作工具
            { 15002, 1 },  // 布料 - 基础材料，用于制作轻型护甲
        };

    // 调试模式默认生成的可采集物体ID
    public const int DebugDefaultHarvestableObjectId = 30005; // 浆果丛
    #endregion
} 