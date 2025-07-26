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

    public bool IsTool()
    {
        return Type == ItemType.Tool;
    }

    public bool IsWeapon()
    {
        return Type == ItemType.Equip;
    }

    public bool IsClothing()
    {
        return Type == ItemType.Equip;
    }

    public bool IsTech()
    {
        return Type == ItemType.Tech;
    }

    public bool IsSurvival()
    {
        return Type == ItemType.Survival;
    }

    public bool IsLight()
    {
        return Type == ItemType.Light;
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

        ItemType type = (ItemType)_reader.GetValue<int>(id, "Type", 0);
        Item item = new Item(type, _reader);
        _cache[id] = item;

        return item;
    }
}
