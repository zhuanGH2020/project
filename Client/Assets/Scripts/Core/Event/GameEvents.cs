// 通用数值变化事件
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

// 道具变化事件
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

// 时间段切换事件
public class TimeOfDayChangeEvent : IEvent
{
    public TimeOfDay PreviousTime { get; }
    public TimeOfDay CurrentTime { get; }

    public TimeOfDayChangeEvent(TimeOfDay previousTime, TimeOfDay currentTime)
    {
        PreviousTime = previousTime;
        CurrentTime = currentTime;
    }
}

// 天数变化事件
public class DayChangeEvent : IEvent
{
    public int PreviousDay { get; }
    public int CurrentDay { get; }
    
    public DayChangeEvent(int previousDay, int currentDay)
    {
        PreviousDay = previousDay;
        CurrentDay = currentDay;
    }
} 

// 制作类型选择事件
public class MakeTypeSelectedEvent : IEvent
{
    public int TypeId { get; }
    public string TypeName { get; }

    public MakeTypeSelectedEvent(int typeId, string typeName)
    {
        TypeId = typeId;
        TypeName = typeName;
    }
}

// 制作菜单打开事件
public class MakeMenuOpenEvent : IEvent
{
    public int TypeId { get; }

    public MakeMenuOpenEvent(int typeId)
    {
        TypeId = typeId;
    }
}

// 游戏保存完成事件
public class GameSavedEvent : IEvent
{
    public int Slot { get; }
    public string SaveTime { get; }
    
    public GameSavedEvent(int slot, string saveTime)
    {
        Slot = slot;
        SaveTime = saveTime;
    }
}

// 游戏加载完成事件
public class GameLoadedEvent : IEvent
{
    public int Slot { get; }
    public string SaveTime { get; }
    
    public GameLoadedEvent(int slot, string saveTime)
    {
        Slot = slot;
        SaveTime = saveTime;
    }
}

// 存档删除事件
public class GameSaveDeletedEvent : IEvent
{
    public int Slot { get; }
    
    public GameSaveDeletedEvent(int slot)
    {
        Slot = slot;
    }
}

// 点击非UI区域事件
public class ClickOutsideUIEvent : IEvent
{
    public UnityEngine.Vector3 ClickPosition { get; }
    
    public ClickOutsideUIEvent(UnityEngine.Vector3 clickPosition)
    {
        ClickPosition = clickPosition;
    }
}

// 鼠标悬停事件
public class MouseHoverEvent : IEvent
{
    public UnityEngine.GameObject HoveredObject { get; }
    public UnityEngine.Vector3 HoverPosition { get; }
    
    public MouseHoverEvent(UnityEngine.GameObject hoveredObject, UnityEngine.Vector3 hoverPosition)
    {
        HoveredObject = hoveredObject;
        HoverPosition = hoverPosition;
    }
}

// 鼠标离开悬停事件
public class MouseHoverExitEvent : IEvent
{
    public MouseHoverExitEvent()
    {
    }
}