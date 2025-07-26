/// <summary>
/// 物品选择事件
/// </summary>
public class ItemSelectEvent : IEvent
{
    public int ItemId { get; }
    public string ItemName { get; }

    public ItemSelectEvent(int itemId, string itemName)
    {
        ItemId = itemId;
        ItemName = itemName;
    }
}

/// <summary>
/// 通用数值变化事件
/// </summary>
public class ValueChangeEvent : IEvent
{
    public string Key { get; }
    public object OldValue { get; }
    public object NewValue { get; }

    public ValueChangeEvent(string key, object oldValue, object newValue)
    {
        Key = key;
        OldValue = oldValue;
        NewValue = newValue;
    }
} 