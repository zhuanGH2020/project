using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventSystemEnums;

/// <summary>
/// 背包事件管理器
/// 参考实现：Assets/Script/input/InputController.cs 的MonoBehaviour设计模式
/// 遵循项目Manager模式和Unity特定规范
/// </summary>
public class InventoryEventManager : MonoBehaviour
{
    #region 序列化字段
    
    [Header("背包事件设置")]
    [SerializeField, Range(10, 200)] private int maxEventQueueSize = 100;
    [SerializeField] private bool enableEventLogging = true;
    [SerializeField] private EventLogLevel logLevel = EventLogLevel.Info;
    
    [Header("背包事件")]
    [Tooltip("物品获得时触发的事件")]
    public InventorySystemEvents.ItemObtainedEvent onItemObtained;
    
    [Tooltip("物品消耗时触发的事件")]
    public InventorySystemEvents.ItemConsumedEvent onItemConsumed;
    
    [Tooltip("物品丢弃时触发的事件")]
    public InventorySystemEvents.ItemDroppedEvent onItemDropped;
    
    [Tooltip("背包容量变化时触发的事件")]
    public InventorySystemEvents.InventoryCapacityChangedEvent onInventoryCapacityChanged;
    
    [Tooltip("背包已满时触发的事件")]
    public InventorySystemEvents.InventoryFullEvent onInventoryFull;
    
    [Tooltip("物品堆叠变化时触发的事件")]
    public InventorySystemEvents.ItemStackChangedEvent onItemStackChanged;
    
    [Tooltip("背包重量变化时触发的事件")]
    public InventorySystemEvents.InventoryWeightChangedEvent onInventoryWeightChanged;
    
    [Tooltip("背包超重时触发的事件")]
    public InventorySystemEvents.InventoryOverweightEvent onInventoryOverweight;
    
    [Tooltip("物品移动时触发的事件")]
    public InventorySystemEvents.ItemMovedEvent onItemMoved;
    
    [Tooltip("物品分割时触发的事件")]
    public InventorySystemEvents.ItemSplitEvent onItemSplit;
    
    [Tooltip("背包排序时触发的事件")]
    public InventorySystemEvents.InventorySortedEvent onInventorySorted;
    
    [Tooltip("背包清空时触发的事件")]
    public InventorySystemEvents.InventoryClearedEvent onInventoryCleared;
    
    [Tooltip("背包槽位锁定时触发的事件")]
    public InventorySystemEvents.InventorySlotLockedEvent onInventorySlotLocked;
    
    [Tooltip("背包操作失败时触发的事件")]
    public InventorySystemEvents.InventoryOperationFailedEvent onInventoryOperationFailed;
    
    [Tooltip("背包状态保存时触发的事件")]
    public InventorySystemEvents.InventoryStateSavedEvent onInventoryStateSaved;
    
    [Tooltip("背包状态加载时触发的事件")]
    public InventorySystemEvents.InventoryStateLoadedEvent onInventoryStateLoaded;
    
    #endregion
    
    #region 私有字段
    
    // 缓存Transform组件提升性能（遵循项目性能优化规范）
    private Transform _cachedTransform;
    
    // 事件队列管理
    private Queue<System.Action> _eventQueue;
    private bool _isProcessingEvents;
    
    // 事件统计（用于调试和性能监控）
    private Dictionary<string, int> _eventStatistics;
    private Dictionary<string, float> _eventExecutionTimes;
    
    // 单例引用（如果需要）
    public static InventoryEventManager Instance { get; private set; }
    
    #endregion
    
    #region 公共属性
    
    /// <summary>
    /// 获取当前事件队列大小
    /// </summary>
    public int CurrentEventQueueSize => _eventQueue?.Count ?? 0;
    
    /// <summary>
    /// 获取事件统计信息
    /// </summary>
    public Dictionary<string, int> EventStatistics => new Dictionary<string, int>(_eventStatistics);
    
    /// <summary>
    /// 获取是否启用事件日志
    /// </summary>
    public bool IsEventLoggingEnabled => enableEventLogging;
    
    /// <summary>
    /// 获取当前日志级别
    /// </summary>
    public EventLogLevel CurrentLogLevel => logLevel;
    
    #endregion
    
    #region Unity生命周期
    
    /// <summary>
    /// 初始化背包事件管理器
    /// </summary>
    void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEventManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 启动时进行组件缓存和事件初始化
    /// </summary>
    void Start()
    {
        CacheComponents();
        InitializeEvents();
    }
    
    /// <summary>
    /// 每帧处理事件队列
    /// </summary>
    void Update()
    {
        ProcessEventQueue();
    }
    
    void OnDestroy()
    {
        // 清理单例引用
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    #endregion
    
    #region 初始化方法
    
    /// <summary>
    /// 初始化事件管理器核心组件
    /// </summary>
    private void InitializeEventManager()
    {
        _eventQueue = new Queue<System.Action>();
        _eventStatistics = new Dictionary<string, int>();
        _eventExecutionTimes = new Dictionary<string, float>();
        _isProcessingEvents = false;
        
        LogEventMessage(EventLogLevel.Info, "EventManager", "背包事件管理器初始化完成");
    }
    
    /// <summary>
    /// 缓存组件引用提升性能
    /// 遵循项目性能优化规范
    /// </summary>
    private void CacheComponents()
    {
        _cachedTransform = transform;
    }
    
    /// <summary>
    /// 初始化所有UnityEvent事件
    /// </summary>
    private void InitializeEvents()
    {
        if (onItemObtained == null)
            onItemObtained = new InventorySystemEvents.ItemObtainedEvent();
            
        if (onItemConsumed == null)
            onItemConsumed = new InventorySystemEvents.ItemConsumedEvent();
            
        if (onItemDropped == null)
            onItemDropped = new InventorySystemEvents.ItemDroppedEvent();
            
        if (onInventoryCapacityChanged == null)
            onInventoryCapacityChanged = new InventorySystemEvents.InventoryCapacityChangedEvent();
            
        if (onInventoryFull == null)
            onInventoryFull = new InventorySystemEvents.InventoryFullEvent();
            
        if (onItemStackChanged == null)
            onItemStackChanged = new InventorySystemEvents.ItemStackChangedEvent();
            
        if (onInventoryWeightChanged == null)
            onInventoryWeightChanged = new InventorySystemEvents.InventoryWeightChangedEvent();
            
        if (onInventoryOverweight == null)
            onInventoryOverweight = new InventorySystemEvents.InventoryOverweightEvent();
            
        if (onItemMoved == null)
            onItemMoved = new InventorySystemEvents.ItemMovedEvent();
            
        if (onItemSplit == null)
            onItemSplit = new InventorySystemEvents.ItemSplitEvent();
            
        if (onInventorySorted == null)
            onInventorySorted = new InventorySystemEvents.InventorySortedEvent();
            
        if (onInventoryCleared == null)
            onInventoryCleared = new InventorySystemEvents.InventoryClearedEvent();
            
        if (onInventorySlotLocked == null)
            onInventorySlotLocked = new InventorySystemEvents.InventorySlotLockedEvent();
            
        if (onInventoryOperationFailed == null)
            onInventoryOperationFailed = new InventorySystemEvents.InventoryOperationFailedEvent();
            
        if (onInventoryStateSaved == null)
            onInventoryStateSaved = new InventorySystemEvents.InventoryStateSavedEvent();
            
        if (onInventoryStateLoaded == null)
            onInventoryStateLoaded = new InventorySystemEvents.InventoryStateLoadedEvent();
            
        LogEventMessage(EventLogLevel.Info, "Initialize", "所有背包事件初始化完成");
    }
    
    #endregion
    
    #region 公共事件触发方法
    
    /// <summary>
    /// 触发物品获得事件
    /// 使用安全的事件调用方式
    /// </summary>
    /// <param name="itemData">获得的物品数据</param>
    /// <param name="quantity">获得数量</param>
    public void TriggerItemObtained(ItemData itemData, int quantity)
    {
        if (ValidateItemData(itemData, "ItemObtained") && ValidateQuantity(quantity, "ItemObtained"))
        {
            RecordEventStatistic("ItemObtained");
            LogEventMessage(EventLogLevel.Info, "ItemObtained", $"获得物品: {itemData.GetDisplayName()} x{quantity}");
            
            SafeInvokeEvent(() => onItemObtained?.Invoke(itemData, quantity), "ItemObtained");
        }
    }
    
    /// <summary>
    /// 触发物品消耗事件
    /// </summary>
    /// <param name="itemData">消耗的物品数据</param>
    /// <param name="quantity">消耗数量</param>
    public void TriggerItemConsumed(ItemData itemData, int quantity)
    {
        if (ValidateItemData(itemData, "ItemConsumed") && ValidateQuantity(quantity, "ItemConsumed"))
        {
            RecordEventStatistic("ItemConsumed");
            LogEventMessage(EventLogLevel.Info, "ItemConsumed", $"消耗物品: {itemData.GetDisplayName()} x{quantity}");
            
            SafeInvokeEvent(() => onItemConsumed?.Invoke(itemData, quantity), "ItemConsumed");
        }
    }
    
    /// <summary>
    /// 触发物品丢弃事件
    /// </summary>
    /// <param name="itemData">丢弃的物品数据</param>
    /// <param name="quantity">丢弃数量</param>
    /// <param name="dropPosition">丢弃位置</param>
    public void TriggerItemDropped(ItemData itemData, int quantity, Vector3 dropPosition)
    {
        if (ValidateItemData(itemData, "ItemDropped") && ValidateQuantity(quantity, "ItemDropped"))
        {
            RecordEventStatistic("ItemDropped");
            LogEventMessage(EventLogLevel.Info, "ItemDropped", 
                $"丢弃物品: {itemData.GetDisplayName()} x{quantity} 在位置 {dropPosition}");
            
            SafeInvokeEvent(() => onItemDropped?.Invoke(itemData, quantity, dropPosition), "ItemDropped");
        }
    }
    
    /// <summary>
    /// 触发背包容量变化事件
    /// </summary>
    /// <param name="oldCapacity">原容量</param>
    /// <param name="newCapacity">新容量</param>
    public void TriggerInventoryCapacityChanged(int oldCapacity, int newCapacity)
    {
        RecordEventStatistic("InventoryCapacityChanged");
        LogEventMessage(EventLogLevel.Info, "InventoryCapacityChanged", $"背包容量变化: {oldCapacity} -> {newCapacity}");
        
        SafeInvokeEvent(() => onInventoryCapacityChanged?.Invoke(oldCapacity, newCapacity), "InventoryCapacityChanged");
    }
    
    /// <summary>
    /// 触发背包已满事件
    /// </summary>
    /// <param name="attemptedItem">尝试添加的物品</param>
    public void TriggerInventoryFull(ItemData attemptedItem)
    {
        RecordEventStatistic("InventoryFull");
        string itemName = attemptedItem?.GetDisplayName() ?? "未知物品";
        LogEventMessage(EventLogLevel.Warning, "InventoryFull", $"背包已满，无法添加物品: {itemName}");
        
        SafeInvokeEvent(() => onInventoryFull?.Invoke(attemptedItem), "InventoryFull");
    }
    
    /// <summary>
    /// 触发物品堆叠变化事件
    /// </summary>
    /// <param name="itemData">物品数据</param>
    /// <param name="oldQuantity">原数量</param>
    /// <param name="newQuantity">新数量</param>
    public void TriggerItemStackChanged(ItemData itemData, int oldQuantity, int newQuantity)
    {
        if (ValidateItemData(itemData, "ItemStackChanged"))
        {
            RecordEventStatistic("ItemStackChanged");
            LogEventMessage(EventLogLevel.Debug, "ItemStackChanged", 
                $"物品堆叠变化: {itemData.GetDisplayName()} {oldQuantity} -> {newQuantity}");
            
            SafeInvokeEvent(() => onItemStackChanged?.Invoke(itemData, oldQuantity, newQuantity), "ItemStackChanged");
        }
    }
    
    /// <summary>
    /// 触发背包重量变化事件
    /// </summary>
    /// <param name="oldWeight">原重量</param>
    /// <param name="newWeight">新重量</param>
    /// <param name="maxWeight">最大重量</param>
    public void TriggerInventoryWeightChanged(float oldWeight, float newWeight, float maxWeight)
    {
        RecordEventStatistic("InventoryWeightChanged");
        LogEventMessage(EventLogLevel.Debug, "InventoryWeightChanged", 
            $"背包重量变化: {oldWeight:F1} -> {newWeight:F1} (最大: {maxWeight:F1})");
        
        SafeInvokeEvent(() => onInventoryWeightChanged?.Invoke(oldWeight, newWeight, maxWeight), "InventoryWeightChanged");
    }
    
    /// <summary>
    /// 触发背包超重事件
    /// </summary>
    /// <param name="currentWeight">当前重量</param>
    /// <param name="maxWeight">最大重量</param>
    public void TriggerInventoryOverweight(float currentWeight, float maxWeight)
    {
        RecordEventStatistic("InventoryOverweight");
        LogEventMessage(EventLogLevel.Warning, "InventoryOverweight", 
            $"背包超重: {currentWeight:F1}/{maxWeight:F1}");
        
        SafeInvokeEvent(() => onInventoryOverweight?.Invoke(currentWeight, maxWeight), "InventoryOverweight");
    }
    
    /// <summary>
    /// 触发物品移动事件
    /// </summary>
    /// <param name="itemData">移动的物品</param>
    /// <param name="fromSlot">源槽位</param>
    /// <param name="toSlot">目标槽位</param>
    public void TriggerItemMoved(ItemData itemData, int fromSlot, int toSlot)
    {
        if (ValidateItemData(itemData, "ItemMoved"))
        {
            RecordEventStatistic("ItemMoved");
            LogEventMessage(EventLogLevel.Debug, "ItemMoved", 
                $"物品移动: {itemData.GetDisplayName()} 从槽位{fromSlot} -> 槽位{toSlot}");
            
            SafeInvokeEvent(() => onItemMoved?.Invoke(itemData, fromSlot, toSlot), "ItemMoved");
        }
    }
    
    /// <summary>
    /// 触发物品分割事件
    /// </summary>
    /// <param name="itemData">分割的物品</param>
    /// <param name="originalQuantity">原数量</param>
    /// <param name="splitQuantity">分割数量</param>
    public void TriggerItemSplit(ItemData itemData, int originalQuantity, int splitQuantity)
    {
        if (ValidateItemData(itemData, "ItemSplit"))
        {
            RecordEventStatistic("ItemSplit");
            LogEventMessage(EventLogLevel.Debug, "ItemSplit", 
                $"物品分割: {itemData.GetDisplayName()} {originalQuantity} 分割出 {splitQuantity}");
            
            SafeInvokeEvent(() => onItemSplit?.Invoke(itemData, originalQuantity, splitQuantity), "ItemSplit");
        }
    }
    
    /// <summary>
    /// 触发背包排序事件
    /// </summary>
    /// <param name="operationType">操作类型</param>
    public void TriggerInventorySorted(InventoryOperationType operationType)
    {
        RecordEventStatistic("InventorySorted");
        LogEventMessage(EventLogLevel.Info, "InventorySorted", $"背包排序: {operationType}");
        
        SafeInvokeEvent(() => onInventorySorted?.Invoke(operationType), "InventorySorted");
    }
    
    /// <summary>
    /// 触发背包操作失败事件
    /// </summary>
    /// <param name="operationType">操作类型</param>
    /// <param name="reason">失败原因</param>
    public void TriggerInventoryOperationFailed(InventoryOperationType operationType, string reason)
    {
        RecordEventStatistic("InventoryOperationFailed");
        LogEventMessage(EventLogLevel.Error, "InventoryOperationFailed", 
            $"背包操作失败: {operationType} - {reason}");
        
        SafeInvokeEvent(() => onInventoryOperationFailed?.Invoke(operationType, reason), "InventoryOperationFailed");
    }
    
    /// <summary>
    /// 将事件添加到队列中延迟执行
    /// 用于处理高频事件，避免性能问题
    /// </summary>
    /// <param name="eventAction">要执行的事件动作</param>
    /// <param name="priority">事件优先级</param>
    public void QueueEvent(System.Action eventAction, EventPriority priority = EventPriority.Normal)
    {
        if (_eventQueue.Count < maxEventQueueSize)
        {
            _eventQueue.Enqueue(eventAction);
        }
        else
        {
            LogEventMessage(EventLogLevel.Warning, "QueueEvent", "事件队列已满，丢弃事件");
        }
    }
    
    #endregion
    
    #region 私有辅助方法
    
    /// <summary>
    /// 验证物品数据有效性
    /// 实现错误处理机制
    /// </summary>
    /// <param name="itemData">要验证的物品数据</param>
    /// <param name="eventName">事件名称</param>
    /// <returns>验证是否通过</returns>
    private bool ValidateItemData(ItemData itemData, string eventName)
    {
        if (itemData == null)
        {
            LogEventMessage(EventLogLevel.Error, eventName, "错误: 物品数据为空");
            return false;
        }
        
        var (isValid, errorMessage) = itemData.ValidateData();
        if (!isValid)
        {
            LogEventMessage(EventLogLevel.Error, eventName, $"物品数据验证失败: {errorMessage}");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 验证数量有效性
    /// </summary>
    /// <param name="quantity">数量</param>
    /// <param name="eventName">事件名称</param>
    /// <returns>验证是否通过</returns>
    private bool ValidateQuantity(int quantity, string eventName)
    {
        if (quantity <= 0)
        {
            LogEventMessage(EventLogLevel.Error, eventName, $"错误: 无效的数量 {quantity}");
            return false;
        }
        return true;
    }
    
    /// <summary>
    /// 记录事件统计信息
    /// 用于性能监控和调试
    /// </summary>
    /// <param name="eventName">事件名称</param>
    private void RecordEventStatistic(string eventName)
    {
        if (_eventStatistics.ContainsKey(eventName))
        {
            _eventStatistics[eventName]++;
        }
        else
        {
            _eventStatistics[eventName] = 1;
        }
    }
    
    /// <summary>
    /// 安全地调用事件
    /// 提供异常处理
    /// </summary>
    /// <param name="eventAction">事件动作</param>
    /// <param name="eventName">事件名称</param>
    private void SafeInvokeEvent(System.Action eventAction, string eventName)
    {
        try
        {
            float startTime = Time.realtimeSinceStartup;
            eventAction?.Invoke();
            float executionTime = (Time.realtimeSinceStartup - startTime) * 1000f; // 转换为毫秒
            
            // 记录执行时间
            if (_eventExecutionTimes.ContainsKey(eventName))
            {
                _eventExecutionTimes[eventName] += executionTime;
            }
            else
            {
                _eventExecutionTimes[eventName] = executionTime;
            }
            
            // 如果执行时间过长，发出警告
            if (executionTime > 5f) // 超过5毫秒
            {
                LogEventMessage(EventLogLevel.Warning, eventName, $"事件处理耗时过长: {executionTime:F2}ms");
            }
        }
        catch (System.Exception exception)
        {
            LogEventMessage(EventLogLevel.Error, eventName, $"事件处理发生异常: {exception.Message}");
        }
    }
    
    /// <summary>
    /// 处理事件队列
    /// 避免在单帧内处理过多事件影响性能
    /// </summary>
    private void ProcessEventQueue()
    {
        if (_isProcessingEvents || _eventQueue.Count == 0)
            return;
            
        _isProcessingEvents = true;
        
        try
        {
            // 每帧最多处理5个事件，避免性能问题
            const int MAX_EVENTS_PER_FRAME = 5;
            int processedEvents = 0;
            
            while (_eventQueue.Count > 0 && processedEvents < MAX_EVENTS_PER_FRAME)
            {
                System.Action eventAction = _eventQueue.Dequeue();
                eventAction?.Invoke();
                processedEvents++;
            }
        }
        catch (System.Exception exception)
        {
            LogEventMessage(EventLogLevel.Error, "ProcessEventQueue", $"处理事件队列时发生错误: {exception.Message}");
        }
        finally
        {
            _isProcessingEvents = false;
        }
    }
    
    /// <summary>
    /// 记录事件日志
    /// 支持不同日志级别和开关控制
    /// </summary>
    /// <param name="level">日志级别</param>
    /// <param name="eventName">事件名称</param>
    /// <param name="message">日志消息</param>
    private void LogEventMessage(EventLogLevel level, string eventName, string message)
    {
        if (!enableEventLogging || level > logLevel)
            return;
            
        string prefix = "[InventoryEventManager]";
        string fullMessage = $"{prefix} {eventName}: {message}";
        
        switch (level)
        {
            case EventLogLevel.Error:
                Debug.LogError(fullMessage);
                break;
            case EventLogLevel.Warning:
                Debug.LogWarning(fullMessage);
                break;
            case EventLogLevel.Info:
            case EventLogLevel.Debug:
            case EventLogLevel.Verbose:
                Debug.Log(fullMessage);
                break;
        }
    }
    
    #endregion
    
    #region 公共工具方法
    
    /// <summary>
    /// 清除事件统计数据
    /// 用于性能测试和调试
    /// </summary>
    public void ClearEventStatistics()
    {
        _eventStatistics.Clear();
        _eventExecutionTimes.Clear();
        LogEventMessage(EventLogLevel.Info, "ClearStatistics", "事件统计数据已清除");
    }
    
    /// <summary>
    /// 设置事件日志开关
    /// </summary>
    /// <param name="enabled">是否启用日志</param>
    public void SetEventLogging(bool enabled)
    {
        enableEventLogging = enabled;
        LogEventMessage(EventLogLevel.Info, "SetEventLogging", $"事件日志已{(enabled ? "启用" : "禁用")}");
    }
    
    /// <summary>
    /// 设置日志级别
    /// </summary>
    /// <param name="level">新的日志级别</param>
    public void SetLogLevel(EventLogLevel level)
    {
        logLevel = level;
        LogEventMessage(EventLogLevel.Info, "SetLogLevel", $"日志级别已设置为: {level}");
    }
    
    /// <summary>
    /// 获取性能报告
    /// </summary>
    /// <returns>性能报告字符串</returns>
    public string GetPerformanceReport()
    {
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("=== 背包事件管理器性能报告 ===");
        report.AppendLine($"事件队列大小: {CurrentEventQueueSize}/{maxEventQueueSize}");
        report.AppendLine();
        
        report.AppendLine("事件触发统计:");
        foreach (var kvp in _eventStatistics)
        {
            float totalTime = _eventExecutionTimes.ContainsKey(kvp.Key) ? _eventExecutionTimes[kvp.Key] : 0f;
            float averageTime = kvp.Value > 0 ? totalTime / kvp.Value : 0f;
            report.AppendLine($"  {kvp.Key}: {kvp.Value}次, 总耗时: {totalTime:F2}ms, 平均: {averageTime:F4}ms");
        }
        
        return report.ToString();
    }
    
    /// <summary>
    /// 强制处理所有队列中的事件
    /// 用于场景切换或游戏退出时
    /// </summary>
    public void FlushEventQueue()
    {
        if (_eventQueue == null)
            return;
            
        LogEventMessage(EventLogLevel.Info, "FlushEventQueue", $"开始处理剩余的{_eventQueue.Count}个事件");
        
        while (_eventQueue.Count > 0)
        {
            try
            {
                System.Action eventAction = _eventQueue.Dequeue();
                eventAction?.Invoke();
            }
            catch (System.Exception exception)
            {
                LogEventMessage(EventLogLevel.Error, "FlushEventQueue", $"处理事件时发生异常: {exception.Message}");
            }
        }
        
        LogEventMessage(EventLogLevel.Info, "FlushEventQueue", "事件队列清空完成");
    }
    
    #endregion
} 