using UnityEngine;

/// <summary>
/// 背包事件管理器
/// 参考实现：Assets/Script/input/InputController.cs 的MonoBehaviour设计模式
/// 遵循项目Manager模式和Unity特定规范
/// </summary>
public class InventoryEventManager : MonoBehaviour
{
    #region 序列化字段
    
    [Header("背包事件")]
    [SerializeField, Tooltip("物品获得时触发的事件")]
    public InventorySystemEvents.ItemObtainedEvent onItemObtained;
    
    [SerializeField, Tooltip("物品消耗时触发的事件")]
    public InventorySystemEvents.ItemConsumedEvent onItemConsumed;
    
    [SerializeField, Tooltip("物品丢弃时触发的事件")]
    public InventorySystemEvents.ItemDroppedEvent onItemDropped;
    
    [SerializeField, Tooltip("背包已满时触发的事件")]
    public InventorySystemEvents.InventoryFullEvent onInventoryFull;
    
    #endregion
    
    #region 单例模式
    
    public static InventoryEventManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    #endregion
    
    #region 事件触发方法
    
    /// <summary>
    /// 触发物品获得事件
    /// </summary>
    /// <param name="itemData">获得的物品数据</param>
    /// <param name="quantity">获得数量</param>
    public void TriggerItemObtained(ItemData itemData, int quantity)
    {
        if (itemData == null || quantity <= 0)
            return;
            
        onItemObtained?.Invoke(itemData, quantity);
        
        if (Application.isEditor)
            Debug.Log($"[InventoryEvent] 获得物品: {itemData.GetDisplayName()} x{quantity}");
    }
    
    /// <summary>
    /// 触发物品消耗事件
    /// </summary>
    /// <param name="itemData">消耗的物品数据</param>
    /// <param name="quantity">消耗数量</param>
    public void TriggerItemConsumed(ItemData itemData, int quantity)
    {
        if (itemData == null || quantity <= 0)
            return;
            
        onItemConsumed?.Invoke(itemData, quantity);
        
        if (Application.isEditor)
            Debug.Log($"[InventoryEvent] 消耗物品: {itemData.GetDisplayName()} x{quantity}");
    }
    
    /// <summary>
    /// 触发物品丢弃事件
    /// </summary>
    /// <param name="itemData">丢弃的物品数据</param>
    /// <param name="quantity">丢弃数量</param>
    public void TriggerItemDropped(ItemData itemData, int quantity)
    {
        if (itemData == null || quantity <= 0)
            return;
            
        onItemDropped?.Invoke(itemData, quantity);
        
        if (Application.isEditor)
            Debug.Log($"[InventoryEvent] 丢弃物品: {itemData.GetDisplayName()} x{quantity}");
    }
    
    /// <summary>
    /// 触发背包已满事件
    /// </summary>
    /// <param name="itemData">无法添加的物品数据</param>
    public void TriggerInventoryFull(ItemData itemData)
    {
        if (itemData == null)
            return;
            
        onInventoryFull?.Invoke(itemData);
        
        if (Application.isEditor)
            Debug.LogWarning($"[InventoryEvent] 背包已满，无法添加: {itemData.GetDisplayName()}");
    }
    
    #endregion
} 