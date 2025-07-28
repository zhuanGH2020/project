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

/// <summary>
/// 道具变化事件
/// </summary>
public class ItemChangeEvent : IEvent
{
    public int ItemId { get; }
    public int Count { get; }
    public bool IsAdd { get; }

    public ItemChangeEvent(int itemId, int count, bool isAdd)
    {
        ItemId = itemId;
        Count = count;
        IsAdd = isAdd;
    }
} 