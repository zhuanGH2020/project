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
/// 怪物类型
/// </summary>
public enum MonsterType
{
    None = 0,
    Normal = 1,    // 普通僵尸
    Boss = 2,      // Boss僵尸
    Poison = 3,    // 毒性僵尸
    Tank = 4,      // 坦克僵尸
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