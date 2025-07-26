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
        return GetMakeType() == MakeType.Tool;
    }

    public bool IsWeapon()
    {
        return GetMakeType() == MakeType.Weapon;
    }

    public bool IsClothing()
    {
        return GetMakeType() == MakeType.Clothing;
    }

    public bool IsTech()
    {
        return GetMakeType() == MakeType.Tech;
    }

    public bool IsSurvival()
    {
        return GetMakeType() == MakeType.Survival;
    }

    public bool IsLight()
    {
        return GetMakeType() == MakeType.Light;
    }

    private MakeType GetMakeType()
    {
        switch (Type)
        {
            case ItemType.Weapon: return MakeType.Weapon;
            case ItemType.Armor: return MakeType.Clothing;
            default: return MakeType.None;
        }
    }
}

public class ItemManager
{
    private static ItemManager _instance;
    public static ItemManager Instance => _instance ??= new ItemManager();

    private Dictionary<int, Item> _cache = new Dictionary<int, Item>();
    private ConfigReader _reader;

    /// <summary>
    /// 获取物品 - 先查缓存，没有则查表并缓存
    /// </summary>
    public Item GetItem(int id)
    {
        if (_cache.ContainsKey(id))
            return _cache[id];

        if (_reader == null)
        {
            ConfigManager.Instance.LoadConfig("Item", "Configs/Item");
            _reader = ConfigManager.Instance.GetReader("Item");
        }

        if (_reader == null || !_reader.HasId(id))
            return null;

        ItemType type = (ItemType)_reader.GetValue<int>(id, "Type", 0);
        Item item = new Item(type, _reader);
        _cache[id] = item;

        return item;
    }
}
