# Object系统技术文档

## 简介

Object系统是游戏对象交互框架，支持攻击、承伤、装备、采集、建筑等核心机制。采用接口驱动设计，支持配置表驱动的装备和怪物AI系统。

## 详细接口

### 核心接口

#### IDamageable - 可承伤接口
```csharp
public interface IDamageable
{
    float MaxHealth { get; }           // 最大生命值
    float CurrentHealth { get; }       // 当前生命值
    float Defense { get; }             // 防御值
    float TakeDamage(DamageInfo damageInfo); // 承受伤害
}
```

#### IAttacker - 攻击者接口
```csharp
public interface IAttacker
{
    float BaseAttack { get; }          // 基础攻击力
    bool CanAttack { get; }            // 是否可以攻击(CD检查)
    void PerformAttack(IDamageable target); // 执行攻击
}
```

#### IEquipable - 可装备接口
```csharp
public interface IEquipable
{
    float MaxDurability { get; }       // 最大耐久度
    float CurrentDurability { get; }   // 当前耐久度
    bool IsEquipped { get; }           // 是否已装备
    bool CanUse { get; }               // 是否可以使用(CD检查)
    void OnEquip(IAttacker owner);     // 装备时调用
    void OnUnequip();                  // 卸下时调用
    void Use();                        // 使用装备
    float GetAttackBonus();            // 获取攻击加成
    float GetDefenseBonus();           // 获取防御加成
}
```

#### IHarvestable - 可采集接口
```csharp
public interface IHarvestable
{
    bool CanHarvest { get; }                    // 是否可采集
    HarvestInfo GetHarvestInfo();               // 获取采集信息
    void OnHarvest(IAttacker harvester);        // 执行采集
}
```

#### IClickable - 可点击交互接口
```csharp
public interface IClickable
{
    bool CanInteract { get; }                   // 是否可交互
    void OnClick(Vector3 clickPosition);        // 处理点击
    float GetInteractionRange();                // 获取交互范围
}
```

### 基础类

#### ObjectBase - 对象基类
```csharp
// 所有游戏对象的基类，提供UID、对象类型管理和配置表查询功能
public class ObjectBase : MonoBehaviour
{
    public int Uid { get; private set; }
    public ObjectType ObjectType { get; private set; }
    public int ConfigId { get; private set; }
    
    // 根据对象类型自动获取对应配置表
    public ConfigReader GetConfig()
    
    // 设置配置ID
    public void SetConfigId(int configId)
    
    // 设置对象类型
    public void SetObjectType(ObjectType type)
}
```

**配置表映射规则**：
- ObjectType.Monster → "Monster.csv"
- ObjectType.Item → "Source.csv"
- ObjectType.Equip → "Equip.csv"
- ObjectType.Building → "Building.csv"

#### DamageableObject - 可承伤物体基类
```csharp
// 继承 ObjectBase，实现 IDamageable
public class DamageableObject : ObjectBase, IDamageable
{
    public float MaxHealth { get; protected set; }
    public float CurrentHealth { get; protected set; }
    public float Defense { get; protected set; }
    
    public virtual float TakeDamage(DamageInfo damageInfo)
    protected virtual void OnDeath()
    public virtual void SetHealth(float health)
}
```

#### CombatEntity - 战斗实体基类
```csharp
// 继承 DamageableObject，实现 IAttacker
public class CombatEntity : DamageableObject, IAttacker
{
    public float BaseAttack { get; protected set; }
    public bool CanAttack => _attackCooldown.IsReady;
    public virtual float TotalAttack { get; }
    
    public virtual void PerformAttack(IDamageable target)
    public virtual void Equip(int equipId)
    protected virtual void UseHandEquip()
}
```

#### HarvestableObject - 可采集物体
**位置**: `Assets/Scripts/Object/Item/HarvestableObject.cs`
```csharp
// 继承 Building，可采集物体基类
public class HarvestableObject : Building
{
    public void Init(int itemId)           // 初始化采集物
    
    // 重写Building的交互逻辑 - 实现采集功能
    protected override void OnInteract(Vector3 clickPosition)
    
    // 采集逻辑
    private bool CanHarvest()              // 检查是否可采集
    private void PerformHarvest()          // 执行采集
    
    // 配置支持
    public bool IsHarvested { get; }       // 是否已被采集
}
```

**核心特性**：
- **继承Building寻路**：使用Building的OnClick寻路逻辑
- **背包友好**：采集后直接添加到玩家背包，不掉落地面
- **配置驱动**：从Drop.csv读取武器要求和掉落配置
- **工具支持**：支持专用工具、任意武器或直接采集模式
- **UI反馈**：提供获得物品提示和背包满提示

### 角色类

#### Player - 玩家角色
```csharp
// 继承 CombatEntity，玩家特定逻辑
public class Player : CombatEntity
{
    public static Player Instance { get; private set; }
    
    // 处理输入（WASD移动、空格使用装备、Q/E装备快捷键）
    private void HandleInput()
    
    // 处理移动（键盘直接移动，瞬转朝向）
    private void HandleMovement()
    
    // 移动到指定位置（供InteractionManager调用）
    public void MoveToPosition(Vector3 position)
}
```

#### 怪物AI继承结构

新的怪物AI系统采用继承结构，Monster为基类，Zombie为特殊子类：

```csharp
CombatEntity
    ↓
Monster (基类)
    ↓  
Zombie (特殊子类)
```

##### Monster - 怪物AI基类
**位置**: `Assets/Scripts/Object/Actor/Monster.cs`
```csharp
// 继承 CombatEntity，配置驱动的基础AI系统
public class Monster : CombatEntity
{
    public void Init(int configId)      // 设置配置ID并加载参数
    private void LoadConfigValues()     // 从配置表加载AI参数
    
    // 基础AI属性访问（供子类使用）
    protected float _detectRange;       // 检测范围
    protected float _attackRange;       // 攻击范围
    protected float _chaseSpeed;        // 追击速度
    protected float _attackAngle;       // 攻击角度
    
    // 移动方法（供子类使用）
    protected void MoveTo(Vector3 direction, float speed)                   // 正常移动（带避障）
    protected void MoveToIgnoreObstacles(Vector3 direction, float speed)    // 无障碍移动（僵尸专用）
    protected void FaceTarget(Vector3 targetPosition)                       // 面向目标
}
```

**核心特性**：
- **智能感知系统**：结合视野角度、检测范围和视线遮挡检测
- **记忆追击系统**：记住玩家最后已知位置，即使失去视线也能继续追击
- **状态机AI**：Idle → Patrol → Alert → Chase → Attack 的完整状态流转
- **被攻击响应**：玩家攻击怪物时立即锁定玩家，无论攻击方向
- **防卡顿系统**：自动检测并处理卡在障碍物的情况
- **智能巡逻**：带避障的巡逻点生成，避免在障碍物前反复移动
- **对话系统**：支持随机对话显示，可配置对话范围和冷却时间
- **双重移动模式**：提供正常避障移动和无障碍移动两种模式

##### Zombie - 特殊僵尸AI
**位置**: `Assets/Scripts/Object/Actor/Zombie.cs`
```csharp
// 继承 Monster，具备特殊AI能力的僵尸系统
public class Zombie : Monster
{
    // 僵尸核心AI方法
    private void UpdateTarget()         // 智能目标选择
    private Transform GetPlantInPath() // 检测路径上的Plant
    private void MoveTowardsTarget()    // 无障碍移动
    private Vector3 CalculateZombieAvoidance() // 僵尸间距保持
}
```

**特殊能力**：
1. **无障碍移动**：不受墙壁、树木等障碍物影响，可直接穿越
2. **智能优先级攻击**：
   - 玩家在攻击范围内 → 立即攻击玩家（最高优先级）
   - 前进路线上有Plant → 攻击路径上的Plant 
   - 科技台存在 → 攻击科技台
   - 玩家不在攻击范围内 → 追击玩家
3. **Plant路径检测**：使用锥形检测前进路线上的伙伴
4. **僵尸间距保持**：自动避开其他僵尸，防止穿模和聚集
5. **配置继承**：完全使用Monster基类配置，无独立参数

**使用Monster基类配置**：
- `_detectRange` - 检测范围和Plant检测范围
- `_attackRange` - 攻击范围和僵尸避让距离  
- `_attackAngle` - Plant检测角度
- `_chaseSpeed` - 移动速度

#### Partner - 伙伴角色
**位置**: `Assets/Scripts/Object/Actor/Partner.cs`
```csharp
// 继承 CombatEntity，伙伴AI系统
public class Partner : CombatEntity
{
    public void Init(int configId)      // 配置驱动初始化
    public PartnerType PartnerType { get; }
    
    // 跟随和战斗AI
    private void UpdateFollowAI()
    private void UpdateCombatAI()
}
```

#### Building - 建筑基类
**位置**: `Assets/Scripts/Object/Base/Building.cs`
```csharp
// 继承 DamageableObject，建筑物基类
public class Building : DamageableObject
{
    public void Initialize(int itemId, Vector2 mapPos, int uid = 0)  // 初始化建筑
    
    // 点击处理 - 包含寻路逻辑
    public override void OnClick(Vector3 clickPosition)
    
    // 交互逻辑 - 子类可重写实现自定义交互行为
    protected virtual void OnInteract(Vector3 clickPosition)
    
    // 建筑管理
    public string GetBuildingName()      // 获取建筑名称
    public bool UpgradeLevel()           // 升级建筑
    public void Demolish()               // 拆除建筑
}
```

**核心特性**：
- **智能寻路**：点击建筑时玩家自动寻路到交互范围内
- **虚方法重写**：子类通过重写OnInteract实现自定义交互逻辑
- **配置驱动**：从Item.csv读取建筑属性和血量
- **地图集成**：与MapModel完全集成，支持保存和加载

#### CookingPot - 烹饪锅
**位置**: `Assets/Scripts/Object/Building/CookingPot.cs`
```csharp
// 继承 Building，烹饪功能建筑
public class CookingPot : Building
{
    public void StartCooking(int recipeId)
    public bool IsCooking { get; }
    public float CookingProgress { get; }
}
```

### 装备系统

#### EquipBase - 装备基类
```csharp
// 所有装备的基类，管理耐久度、冷却和基础属性
public class EquipBase : MonoBehaviour, IEquipable
{
    // 根据配置ID初始化装备（数据驱动）
    public virtual void Init(int configId)
    
    // 使用装备（消耗耐久度，开始冷却）
    public virtual void Use()
}
```

#### HandEquipBase - 手部装备基类
```csharp
// 继承 EquipBase，手持装备的基类
public class HandEquipBase : EquipBase
{
    // 显示子弹轨迹
    protected virtual void ShowBulletTrail(Vector3 startPoint, Vector3 endPoint)
    
    // 处理攻击命中
    protected virtual void HandleHit(IDamageable target, Vector3 hitPoint)
}
```

### 建筑交互系统

#### 智能寻路机制
Building.OnClick提供统一的交互体验：
1. **距离检测**：自动检测玩家与建筑距离
2. **智能寻路**：距离超出范围时自动寻路到附近
3. **交互触发**：到达范围后自动调用OnInteract
4. **容错处理**：移动失败或超时后仍可远程交互

#### 虚方法重写模式
```csharp
// 基类提供寻路框架
public class Building : DamageableObject
{
    protected virtual void OnInteract(Vector3 clickPosition)
    {
        // 默认发布通用交互事件
        var interactionEvent = new ObjectInteractionEvent(this, clickPosition);
        EventManager.Instance.Publish(interactionEvent);
    }
}

// 子类重写实现具体交互逻辑
public class HarvestableObject : Building
{
    protected override void OnInteract(Vector3 clickPosition)
    {
        // 实现采集逻辑
        if (CanHarvest()) PerformHarvest();
    }
}
```

### 采集系统

#### 配置驱动采集
HarvestableObject使用Drop.csv配置采集需求：
- **RequiredEquipId**: 专用装备ID（如斧头砍树）
- **AnyEquip**: 是否需要任意装备（如攻击怪物）
- **DropItemId1-5**: 最多5种掉落物品
- **DropCount/Chance**: 每种物品的数量和概率

#### 背包集成
采集后物品直接进入背包：
```csharp
var success = PackageModel.Instance.AddItem(ItemId, 1);
if (success)
{
    EventManager.Instance.Publish(new NoticeEvent($"获得了 {GetBuildingName()}"));
}
else
{
    EventManager.Instance.Publish(new NoticeEvent("背包已满！"));
}
```

### 数据结构

#### DamageInfo - 伤害信息
```csharp
public struct DamageInfo
{
    public float Damage;               // 伤害值
    public DamageType Type;            // 伤害类型
    public Vector3 HitPoint;           // 击中点
    public IAttacker Source;           // 伤害来源
    public Vector3 Direction;          // 伤害方向
}
```

#### CooldownTimer - 冷却计时器
```csharp
public class CooldownTimer
{
    public float CooldownTime { get; }     // 冷却时间
    public float RemainingTime { get; }    // 剩余冷却时间
    public bool IsReady => RemainingTime <= 0; // 是否已冷却完成
    
    public void StartCooldown()           // 开始冷却
    public void Update()                  // 更新冷却时间
}
```

## 文件组织

### 核心脚本结构
```
Scripts/Object/
├── Base/                     # 基础类
│   ├── ObjectBase.cs
│   ├── DamageableObject.cs
│   ├── CombatEntity.cs
│   └── HarvestableObject.cs
├── Actor/                    # 角色类
│   ├── Player.cs
│   ├── Monster.cs            # 怪物基类（标准AI）
│   ├── Zombie.cs             # 僵尸（特殊AI）
│   └── Partner.cs
├── Building/                 # 建筑类  
│   ├── Building.cs
│   └── CookingPot.cs
├── Equip/                    # 装备系统
│   ├── Base/                 # 装备基类
│   ├── Hand/                 # 手部装备
│   ├── Head/                 # 头部装备
│   └── Body/                 # 身体装备
├── Interactive/              # 采集物
│   ├── DirectHarvestable.cs
│   └── RepeatableHarvestable.cs
├── Item/                     # 掉落物
├── Manager/                  # 管理器
│   └── InteractionManager.cs
├── Effect/                   # 特效
│   └── BulletTrail.cs
└── Data/                     # 数据结构
    ├── DamageInfo.cs
    └── CooldownTimer.cs
```

## 最佳实践

### 怪物AI使用

#### 标准怪物初始化
```csharp
// 基础怪物 - 标准AI行为
var monster = gameObject.AddComponent<Monster>();
monster.Init(5001); // 使用Monster.csv配置

// Monster.csv 配置示例
Id,Name,DetectionRange,AttackRange,MoveSpeed,FieldOfView,LostTargetTime
5001,公鸡,5,2,3.5,90,3
5002,母鸡,6,2.5,3,120,4
5003,鸡王,8,3,4,150,5
```

#### 特殊僵尸初始化
```csharp
// 僵尸 - 特殊AI能力
var zombie = gameObject.AddComponent<Zombie>();
zombie.Init(5000); // 外部调用Init，使用僵尸专用配置

// Monster.csv 僵尸配置示例
Id,Name,DetectionRange,AttackRange,MoveSpeed,AttackAngle
5000,僵尸,8,2,5,60
```

#### 僵尸AI调优
```csharp
// 僵尸特殊行为优先级
// 1. 玩家在攻击范围内 → 立即攻击玩家
// 2. 路径上有Plant → 攻击Plant
// 3. 科技台存在 → 攻击科技台
// 4. 追击玩家
```

### 怪物类型选择指南

#### 使用Monster基类的场景
- ✅ **标准AI行为**：巡逻、追击、攻击等正常怪物行为
- ✅ **受障碍物影响**：需要绕过墙壁、树木等障碍物
- ✅ **平衡游戏性**：玩家可以利用地形优势

#### 使用Zombie子类的场景
- ✅ **特殊敌人**：BOSS级或特殊事件中的敌人
- ✅ **无障碍追击**：需要穿越地形直接追击玩家
- ✅ **智能索敌**：优先攻击重要目标（科技台、伙伴）
- ✅ **高威胁性**：营造紧张感的特殊敌人

### 被攻击响应机制
```csharp
// Monster和Zombie都会自动响应玩家攻击
// 无需额外代码，装备系统会自动设置DamageInfo.Source

// 基础怪物响应
// 1. 立即锁定攻击者
// 2. 根据当前状态智能切换AI状态
// 3. 重置失去目标计时器

// 僵尸特殊响应
// 1. 立即将玩家设为当前目标
// 2. 覆盖其他目标优先级
// 3. 无障碍直接追击
```

### 装备系统使用
```csharp
// 通过装备ID装备物品
player.Equip(30001); // 装备UZI
player.Equip(30002); // 装备Axe

// Equip.csv 配置示例
ID,Name,EquipType,Damage,Defense,Durability,UseCooldown,Range
30001,UZI,Uzi,25,0,100,0.1,10
30002,Axe,Axe,50,0,200,1.0,2
```

### 采集系统使用

#### 直接采集物（DirectHarvestable）
```csharp
// 适用于：草、花等可直接收获的物品
[SerializeField] private int _itemId = 1001;

// Source.csv 配置示例
ID,DropItemId,DropCount,DropRate,HarvestTime,DestroyAfterHarvest
1001,2001,1,1.0,0.0,true
```

#### 重复采集物（RepeatableHarvestable）
```csharp
// 适用于：浆果丛、果树等可多次收获的物品
// Source.csv 配置示例
ID,DropItemId,DropCount,MaxHarvestCount,RegrowTime
1002,2002,1,3,300.0
```

### 伤害系统使用

#### 造成伤害
```csharp
var damageInfo = new DamageInfo
{
    Damage = 25f,
    Type = DamageType.Physical,
    HitPoint = target.transform.position,
    Direction = transform.forward,
    Source = this
};

float actualDamage = target.TakeDamage(damageInfo);
```

#### 承受伤害
```csharp
public override float TakeDamage(DamageInfo damageInfo)
{
    float actualDamage = Mathf.Max(0, damageInfo.Damage - Defense);
    _currentHealth -= actualDamage;
    
    if (_currentHealth <= 0)
    {
        OnDeath();
    }
    
    return actualDamage;
}
```

### 对象管理系统

#### 对象注册和查询
```csharp
// 在子类的 Awake 中设置对象类型
protected override void Awake()
{ 
    base.Awake(); 
    SetObjectType(ObjectType.Player); 
}

// 使用对象管理器查询
var player = ObjectManager.Instance.FindAllByType<Player>(ObjectType.Player);
var monsters = ObjectManager.Instance.FindAllByType<Monster>(ObjectType.Monster);
var harvestables = ObjectManager.Instance.FindAllByType<HarvestableObject>(ObjectType.Item);
```

## 注意事项

### 配置系统
- Monster和Zombie都必须调用`Init(configId)`来正确初始化
- Zombie完全使用Monster.csv配置，无独立配置参数
- 配置ID验证：生成前验证配置ID是否存在于配置表中
- 配置表预加载：在游戏启动时预加载常用配置表以提升性能

### 性能优化
- 避免在Update中进行频繁的GetComponent调用
- 僵尸AI每0.3秒更新目标，每0.2秒检测Plant
- 合理设置AI检测频率
- 交互检测使用合理的更新频率（默认0.1秒）
- 掉落物自动超时清理（默认300秒）

### 内存管理
- 装备模型使用ResourceManager加载和释放
- 及时销毁不需要的特效对象
- 避免内存泄漏
- 视觉组件按需激活/停用，减少渲染开销

### 系统集成
- InteractionManager会在GameMain中自动创建
- 采集物需要添加Collider组件用于射线检测
- 掉落物会自动添加SphereCollider
- 重新生长计时器仅在需要时更新

### 僵尸AI设计要点
- **无障碍移动**：使用`MoveToIgnoreObstacles`而非普通`MoveTo`
- **配置继承**：所有参数来自Monster.csv，保持统一性
- **目标优先级**：玩家在攻击范围 > Plant路径 > 科技台 > 玩家追击
- **间距保持**：使用攻击范围作为避让距离，防止僵尸聚集
- **外部初始化**：必须由外部代码调用`Init(configId)`

## 配置表结构

### Monster.csv - 怪物配置表
```csv
Id,Name,Type,MaxHealth,DetectionRange,AttackRange,MoveSpeed,RotationSpeed,FieldOfView,LostTargetTime,IdleSpeed,AttackAngle,PatrolRadius,PatrolWaitTime,DialogRange,DialogIds
```

### Equip.csv - 装备配置表
```csv
ID,Name,EquipType,Type,Damage,Defense,Durability,DurabilityLoss,UseCooldown,Range,ModelPath
```

### Source.csv - 采集物配置表
```csv
ID,DropItemId,DropCount,DropRate,HarvestTime,DestroyAfterHarvest,InteractionRange,ActionType
```

## 其他需要补充的关键点

### 事件系统集成
```csharp
// 监听采集事件
EventManager.Instance.Subscribe<ObjectInteractionEvent>(OnObjectInteraction);

// 监听物品变化事件
EventManager.Instance.Subscribe<ItemChangeEvent>(OnItemChanged);

// 监听攻击事件
EventManager.Instance.Subscribe<AttackEvent>(OnAttack);
```

### 扩展指南

#### 添加新的怪物类型
```csharp
// 方式1：直接使用基类Monster（推荐）
var newMonster = gameObject.AddComponent<Monster>();
newMonster.Init(newMonsterId); // 更新Monster.csv配置表

// 方式2：继承Monster创建新类型
public class Boss : Monster
{
    // 重写特定AI行为
    protected override void UpdateChaseState() { ... }
}
```

#### 添加新的采集物
1. 选择合适的脚本类型：DirectHarvestable/RepeatableHarvestable
2. 配置GameObject：添加对应的脚本组件，设置_itemId
3. 更新配置表：在Source.csv中添加对应ID的配置

#### 添加新的装备
1. 继承对应的装备基类：HandEquipBase/HeadEquipBase/BodyEquipBase
2. 实现特定的装备逻辑：攻击效果、特殊能力等
3. 更新Equip.csv配置表：添加装备的属性配置

### 调试和可视化

#### Monster基类调试
- **黄色圆**：检测范围
- **红色圆**：攻击范围
- **蓝色扇形**：视野角度
- **绿色圆**：巡逻范围
- **青色点**：巡逻目标点
- **洋红点**：玩家最后已知位置

#### Zombie特殊调试
- **红色线**：当前目标连线
- **黄色圆**：检测范围（用于Plant检测）
- **红色圆**：攻击范围（也是避让距离）
- **绿色锥形**：Plant检测区域

## 版本历史

### v2.5
- **重大重构**：Monster-Zombie继承架构
- **僵尸特殊AI**：无障碍移动、智能优先级攻击、Plant路径检测
- **配置统一**：Zombie使用Monster.csv配置，无独立参数
- **双重移动模式**：Monster基类提供正常和无障碍两种移动方式
- **性能优化**：僵尸AI更新频率和检测间隔优化
- **调试增强**：完善的Gizmos可视化调试功能

### v2.4 
- 添加智能怪物AI优先级索敌系统
- 实现Monster与NormalMonster双AI系统
- 添加配置表驱动的怪物系统
- 完善装备系统和采集系统
 