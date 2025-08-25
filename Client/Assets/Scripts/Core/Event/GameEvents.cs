using System.Collections.Generic;
using UnityEngine;
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

// 背包道具选中状态变化事件
public class PackageItemSelectedEvent : IEvent
{
    public PackageItem SelectedItem { get; }
    public bool IsSelected { get; }

    public PackageItemSelectedEvent(PackageItem selectedItem, bool isSelected)
    {
        SelectedItem = selectedItem;
        IsSelected = isSelected;
    }
}

// 背包刷新事件 - 用于所有需要刷新背包UI的场景
public class PackageRefreshEvent : IEvent
{
    public int ItemCount { get; }

    public PackageRefreshEvent(int itemCount)
    {
        ItemCount = itemCount;
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

// 制作菜单关闭事件
public class MakeMenuCloseEvent : IEvent
{
    public MakeMenuCloseEvent()
    {
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

// 通知显示事件
public class NoticeEvent : IEvent
{
    public string Message { get; }
    
    public NoticeEvent(string message)
    {
        Message = message;
    }
}

// 制作详情视图打开事件
public class MakeDetailOpenEvent : IEvent
{
    public int ItemId { get; }
    public UnityEngine.Vector2 UIPosition { get; }
    
    public MakeDetailOpenEvent(int itemId, UnityEngine.Vector2 uiPosition)
    {
        ItemId = itemId;
        UIPosition = uiPosition;
    }
}

// 制作详情视图关闭事件
public class MakeDetailCloseEvent : IEvent
{
    public bool WithDelay { get; }
    
    public MakeDetailCloseEvent(bool withDelay = true)
    {
        WithDelay = withDelay;
    }
}

/// <summary>
/// 物体交互事件 - 玩家点击可交互物体时触发
/// </summary>
public class ObjectInteractionEvent : IEvent
{
    public IClickable Target { get; private set; }
    public UnityEngine.Vector3 ClickPosition { get; private set; }

    public ObjectInteractionEvent(IClickable target, UnityEngine.Vector3 clickPosition)
    {
        Target = target;
        ClickPosition = clickPosition;
    }
}

// 地图数据添加事件
public class MapDataAddedEvent : IEvent
{
    public MapData MapData { get; }

    public MapDataAddedEvent(MapData mapData)
    {
        MapData = mapData;
    }
}

// 地图数据删除事件
public class MapDataRemovedEvent : IEvent
{
    public MapData MapData { get; }

    public MapDataRemovedEvent(MapData mapData)
    {
        MapData = mapData;
    }
}

// 地图数据选中状态变化事件
public class MapDataSelectedEvent : IEvent
{
    public MapData MapData { get; }
    public bool IsSelected { get; }

    public MapDataSelectedEvent(MapData mapData, bool isSelected)
    {
        MapData = mapData;
        IsSelected = isSelected;
    }
}

// 建筑物待放置事件
public class BuildingPendingPlaceEvent : IEvent
{
    public int BuildingId { get; }

    public BuildingPendingPlaceEvent(int buildingId)
    {
        BuildingId = buildingId;
    }
}

// 建筑物放置完成事件
public class BuildingPlacedEvent : IEvent
{
    public int BuildingId { get; }
    public float PosX { get; }
    public float PosY { get; }

    public BuildingPlacedEvent(int buildingId, float posX, float posY)
    {
        BuildingId = buildingId;
        PosX = posX;
        PosY = posY;
    }
}

// 建筑放置模式状态变化事件
public class BuildingPlacementModeEvent : IEvent
{
    public bool IsInPlacementMode { get; }
    public int BuildingId { get; }

    public BuildingPlacementModeEvent(bool isInPlacementMode, int buildingId = -1)
    {
        IsInPlacementMode = isInPlacementMode;
        BuildingId = buildingId;
    }
}

/// <summary>
/// 烹饪交互事件 - 玩家进入/离开锅的交互范围
/// </summary>
public class CookingInteractionEvent : IEvent
{
    public bool InRange { get; private set; }
    public CookingPot Pot { get; private set; }
    
    public CookingInteractionEvent(bool inRange, CookingPot pot)
    {
        InRange = inRange;
        Pot = pot;
    }
}

/// <summary>
/// 烹饪界面打开事件
/// </summary>
public class CookingUIOpenEvent : IEvent
{
    public Vector3 PotWorldPosition { get; }
    
    public CookingUIOpenEvent(Vector3 potWorldPosition) 
    { 
        PotWorldPosition = potWorldPosition;
    }
}


/// <summary>
/// 烹饪界面关闭事件
/// </summary>
public class CookingUICloseEvent : IEvent
{
    public CookingUICloseEvent() { }
}

/// <summary>
/// 烹饪槽位更新事件
/// </summary>
public class CookingSlotUpdateEvent : IEvent
{
    public int SlotIndex { get; private set; }
    public int ItemId { get; private set; }
    public int Count { get; private set; }
    
    public CookingSlotUpdateEvent(int slotIndex, int itemId, int count)
    {
        SlotIndex = slotIndex;
        ItemId = itemId;
        Count = count;
    }
}

/// <summary>
/// 烹饪成功事件
/// </summary>
public class CookingSuccessEvent : IEvent
{
    public int ResultItemId { get; private set; }
    public int ResultCount { get; private set; }
    
    public CookingSuccessEvent(int resultItemId, int resultCount)
    {
        ResultItemId = resultItemId;
        ResultCount = resultCount;
    }
}

/// <summary>
/// 科技台交互事件 - 玩家进入/离开科技台的交互范围
/// </summary>
public class TechTableInteractionEvent : IEvent
{
    public bool InRange { get; private set; }
    public TechTable TechTable { get; private set; }
    
    public TechTableInteractionEvent(bool inRange, TechTable techTable)
    {
        InRange = inRange;
        TechTable = techTable;
    }
}

/// <summary>
/// 科技台材料添加事件
/// </summary>
public class TechTableMaterialAddedEvent : IEvent
{
    public int ItemId { get; private set; }
    public int Count { get; private set; }
    public string MaterialName { get; private set; }
    
    public TechTableMaterialAddedEvent(int itemId, int count, string materialName)
    {
        ItemId = itemId;
        Count = count;
        MaterialName = materialName;
    }
}

/// <summary>
/// 科技台升级完成事件
/// </summary>
public class TechTableUpgradeCompleteEvent : IEvent
{
    public TechTable TechTable { get; private set; }
    
    public TechTableUpgradeCompleteEvent(TechTable techTable)
    {
        TechTable = techTable;
    }
}

/// <summary>
/// 科技台等级提升事件
/// </summary>
public class TechTableLevelUpEvent : IEvent
{
    public int OldLevel { get; private set; }
    public int NewLevel { get; private set; }
    
    public TechTableLevelUpEvent(int oldLevel, int newLevel)
    {
        OldLevel = oldLevel;
        NewLevel = newLevel;
    }
}

// 事件系统仅用于真正需要的一对多通知场景
// View-Model的一对一交互直接使用方法调用，更加简洁高效



/// <summary>
/// 怪物生成事件
/// </summary>
public class MonsterSpawnedEvent : IEvent
{
    public UnityEngine.GameObject MonsterInstance { get; private set; }
    public UnityEngine.Vector3 SpawnPosition { get; private set; }
    
    public MonsterSpawnedEvent(UnityEngine.GameObject monsterInstance, UnityEngine.Vector3 spawnPosition)
    {
        MonsterInstance = monsterInstance;
        SpawnPosition = spawnPosition;
    }
}

/// <summary>
/// 玩家血量变化事件
/// </summary>
public class PlayerHealthChangeEvent : IEvent
{
    public float PreviousHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    
    public PlayerHealthChangeEvent(float previousHealth, float currentHealth)
    {
        PreviousHealth = previousHealth;
        CurrentHealth = currentHealth;
    }
}

/// <summary>
/// 玩家饥饿值变化事件
/// </summary>
public class PlayerHungerChangeEvent : IEvent
{
    public float PreviousHunger { get; private set; }
    public float CurrentHunger { get; private set; }
    
    public PlayerHungerChangeEvent(float previousHunger, float currentHunger)
    {
        PreviousHunger = previousHunger;
        CurrentHunger = currentHunger;
    }
}

/// <summary>
/// 玩家理智值变化事件
/// </summary>
public class PlayerSanityChangeEvent : IEvent
{
    public float PreviousSanity { get; private set; }
    public float CurrentSanity { get; private set; }
    
    public PlayerSanityChangeEvent(float previousSanity, float currentSanity)
    {
        PreviousSanity = previousSanity;
        CurrentSanity = currentSanity;
    }
}