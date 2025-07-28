/// <summary>
/// 配置枚举
/// </summary>

/// <summary>
/// 物品类型
/// </summary>
public enum ItemType
{
    None = 0,
    Tool = 1,        // 工具
    Light = 2,       // 光源
    Survival = 3,    // 生存
    Tech = 4,        // 科技
    Equip = 5,       // 装备
}

/// <summary>
/// 装备部位
/// </summary>
public enum EquipPart
{
    None = 0,
    Head = 1,        // 头
    Body = 2,        // 身体
    Hand = 3,        // 手
}

/// <summary>
/// 工具类型
/// </summary>
public enum ToolType
{
    None = 0,
    Axe = 1,      // 斧头
    Chisel = 2,   // 凿子
    Torch = 3,    // 火把
    Pot = 4,      // 锅
}

/// <summary>
/// 怪物类型
/// </summary>
public enum MonsterType
{
    None = 0,
    Normal = 1,    // 普通怪
    Boss = 2,      // Boss
    Friend = 4,    // 友军
}

/// <summary>
/// 伙伴类型
/// </summary>
public enum PartnerType
{
    None = 0,
    Peashooter = 1,    // 豌豆射手
    IceShooter = 2,    // 冰豌豆射手
    FireShooter = 3,   // 火豌豆射手
    LightShooter = 4,  // 光明射手
    DarkShooter = 5,   // 黑暗射手
}

/// <summary>
/// 伤害类型
/// </summary>
public enum DamageType
{
    None = 0,
    Physical = 1,
    Fire = 2,
    Ice = 3,
    Poison = 4,
}
