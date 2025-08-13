using UnityEngine;

/// <summary>
/// UI路径打印开关状态变化事件
/// </summary>
public class UIPathPrintToggleEvent : IEvent
{
    public bool Enabled { get; }

    public UIPathPrintToggleEvent(bool enabled)
    {
        Enabled = enabled;
    }
}

/// <summary>
/// 时间系统开关状态变化事件
/// </summary>
public class TimeControlToggleEvent : IEvent
{
    public bool Enabled { get; }

    public TimeControlToggleEvent(bool enabled)
    {
        Enabled = enabled;
    }
} 