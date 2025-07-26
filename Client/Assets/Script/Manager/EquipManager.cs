using System.Collections.Generic;
using UnityEngine;

public class Equip
{
    public EquipPart Type { get; private set; }
    public ConfigReader Csv { get; private set; }

    public Equip(EquipPart type, ConfigReader csv)
    {
        Type = type;
        Csv = csv;
    }

    public bool IsHead()
    {
        return Type == EquipPart.Head;
    }

    public bool IsBody()
    {
        return Type == EquipPart.Body;
    }

    public bool IsHand()
    {
        return Type == EquipPart.Hand;
    }
}

public class EquipManager
{
    private static EquipManager _instance;
    public static EquipManager Instance => _instance ??= new EquipManager();

    private Dictionary<int, Equip> _cache = new Dictionary<int, Equip>();
    private ConfigReader _reader;

    /// <summary>
    /// 获取装备 - 先查缓存，没有则查表并缓存
    /// </summary>
    public Equip GetEquip(int id)
    {
        if (_cache.ContainsKey(id))
            return _cache[id];

        if (_reader == null)
        {
            ConfigManager.Instance.LoadConfig("Equip", "Configs/Equip");
            _reader = ConfigManager.Instance.GetReader("Equip");
        }

        if (_reader == null || !_reader.HasId(id))
            return null;

        EquipPart type = (EquipPart)_reader.GetValue<int>(id, "Type", 0);
        Equip equip = new Equip(type, _reader);
        _cache[id] = equip;

        return equip;
    }
} 