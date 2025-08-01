using UnityEngine;

/// <summary>
/// 调试管理器 - 管理游戏调试相关的设置和功能
/// </summary>
public class DebugManager
{
    private static DebugManager _instance;
    public static DebugManager Instance => _instance ??= new DebugManager();

    private bool _enableUIPathPrint = true; // UI路径打印开关

    private DebugManager() { }

    /// <summary>
    /// 设置UI路径打印开关状态
    /// </summary>
    public void SetUIPathPrintEnabled(bool enabled)
    {
        _enableUIPathPrint = enabled;
    }

    /// <summary>
    /// 获取UI路径打印开关状态
    /// </summary>
    public bool IsUIPathPrintEnabled => _enableUIPathPrint;
} 