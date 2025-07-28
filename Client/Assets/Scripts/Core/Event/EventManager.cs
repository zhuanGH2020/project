using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件管理器 - 统一事件架构核心
/// </summary>
public class EventManager
{
    private static EventManager _instance;
    public static EventManager Instance => _instance ??= new EventManager();

    private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new Dictionary<Type, List<Delegate>>();

    /// <summary>
    /// 订阅事件
    /// </summary>
    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        Type eventType = typeof(T);
        if (!_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType] = new List<Delegate>();
        }
        _eventHandlers[eventType].Add(handler);
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    public void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        Type eventType = typeof(T);
        if (_eventHandlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
            if (handlers.Count == 0)
            {
                _eventHandlers.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// 发布事件
    /// </summary>
    public void Publish<T>(T eventData) where T : IEvent
    {
        Type eventType = typeof(T);
        if (!_eventHandlers.TryGetValue(eventType, out var handlers)) return;

        for (int i = handlers.Count - 1; i >= 0; i--)
        {
            try
            {
                ((Action<T>)handlers[i])?.Invoke(eventData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Event handler error: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 清空所有事件订阅
    /// </summary>
    public void Clear()
    {
        _eventHandlers.Clear();
    }
} 