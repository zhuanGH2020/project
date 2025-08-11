// 配置枚举

// 物品类型
public enum ItemType
{
    None = 0,
    Item = 1,        // 物品
    Building = 2,    // 建筑
    Equip = 3,       // 装备
    Food = 4,        // 食物
}

// 场景对象类型（用于ObjectBase/ObjectManager分类）
public enum ObjectType
{
    Other = 0,
    Player = 1,
    Monster = 2,
    Building = 3,
    Item = 4,
}

// 装备部位
public enum EquipPart
{
    None = 0,
    Head = 1,        // 头
    Body = 2,        // 身体
    Hand = 3,        // 手
}

// 装备类型
public enum EquipType
{
    None = 0,
    Helmet = 1,      // 头盔
    Armor = 2,       // 护甲
    Axe = 3,         // 斧头
    Torch = 4,       // 火把
    Uzi = 5,         // UZI冲锋枪
    Shotgun = 6,     // 散弹枪
}

// 工具类型
public enum ToolType
{
    None = 0,
    Axe = 1,      // 斧头
    Chisel = 2,   // 凿子
    Torch = 3,    // 火把
    Pot = 4,      // 锅
}

// 怪物类型
public enum MonsterType
{
    None = 0,
    Normal = 1,    // 普通怪
    Boss = 2,      // Boss
    Friend = 4,    // 友军
}

// 伙伴类型
public enum PartnerType
{
    None = 0,
    Shooter = 1,    // 豌豆射手
    Sunflower = 2,  // 向日葵
}

// 伤害类型
public enum DamageType
{
    None = 0,
    Physical = 1,
    Fire = 2,
    Ice = 3,
    Poison = 4,
}

// 时间段枚举
public enum TimeOfDay
{
    Day = 0,        // 白天 (0.0-0.5)
    Dusk = 1,       // 黄昏 (0.5-0.75)
    Night = 2,      // 夜晚 (0.75-1.0)
}
