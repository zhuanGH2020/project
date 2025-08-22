using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public ItemType Type { get; private set; }
    public ConfigReader Csv { get; private set; }

    public Item(ItemType type, ConfigReader csv)
    {
        Type = type;
        Csv = csv;
    }

    public bool IsItem()
    {
        return Type == ItemType.Item;
    }

    public bool IsBuilding()
    {
        return Type == ItemType.Building;
    }

    public bool IsEquip()
    {
        return Type == ItemType.Equip;
    }

    /// <summary>
    /// 检查是否为装备类型（包括武器、护甲等）
    /// 注意：已从Weapon改为Equip，统一使用装备系统
    /// </summary>
    public bool IsWeapon()
    {
        return Type == ItemType.Equip;
    }

    public bool IsClothing()
    {
        return Type == ItemType.Equip;
    }

    public bool IsFood()
    {
        return Type == ItemType.Food;
    }
}

public class ItemManager
{
    private static ItemManager _instance;
    public static ItemManager Instance => _instance ??= new ItemManager();

    private Dictionary<int, Item> _cache = new Dictionary<int, Item>();
    private ConfigReader _reader;

    /// <summary>
    /// 查找物品 - 先查缓存，没有则查表并缓存
    /// </summary>
    public Item GetItem(int id)
    {
        if (_cache.ContainsKey(id))
            return _cache[id];

        if (_reader == null)
        {
            _reader = ConfigManager.Instance.GetReader("Item");
        }

        if (_reader == null || !_reader.HasKey(id))
            return null;

        ItemType type = _reader.GetValue<ItemType>(id, "Type", ItemType.None);
        Item item = new Item(type, _reader);
        _cache[id] = item;

        return item;
    }
}
