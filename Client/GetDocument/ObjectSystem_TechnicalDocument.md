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

#### HarvestableObject - 采集物基类
```csharp
// 继承 DamageableObject，实现 IHarvestable + IClickable
public class HarvestableObject : DamageableObject, IHarvestable, IClickable
{
    public virtual void OnHarvest(IAttacker harvester)
    public virtual void OnClick(Vector3 clickPosition)
    protected virtual void OnHarvestComplete(IAttacker harvester)
    protected virtual void ProcessDrops(IAttacker harvester)
}
```

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

#### MonsterAI_Enhanced - 增强版怪物AI
```csharp
// 继承 CombatEntity，配置驱动的完整AI系统
public class MonsterAI_Enhanced : CombatEntity
{
    // 必须调用此方法来设置配置ID并加载参数
    public void Init(int configId)
    
    // 从配置表加载所有AI参数
    private void LoadConfigValues()
}
```

**配置参数**：
- DetectionRange - 检测范围
- AttackRange - 攻击范围  
- MoveSpeed - 移动速度
- RotationSpeed - 旋转速度
- MaxHealth - 最大生命值
- FieldOfView - 视野角度
- LostTargetTime - 失去目标记忆时间
- IdleSpeed - 空闲速度
- AttackAngle - 攻击角度
- PatrolRadius - 巡逻半径
- PatrolWaitTime - 巡逻等待时间

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

### 采集系统

#### DirectHarvestable - 直接采集物
```csharp
// 继承 HarvestableObject，一次性采集物（如草、花、掉落物）
public class DirectHarvestable : HarvestableObject
{
    public void SetItemId(int itemId)      // 设置物品ID
    public void SetDropCount(int count)    // 设置掉落数量
}
```

#### RepeatableHarvestable - 重复采集物
```csharp
// 继承 HarvestableObject，可多次收获的采集物（如浆果丛、果树）
public class RepeatableHarvestable : HarvestableObject
{
    public int CurrentHarvestCount { get; }     // 当前可采集数量
    public int MaxHarvestCount { get; }         // 最大采集数量
    public bool IsRegrowing { get; }            // 是否正在重新生长
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

## 最佳实践

### 配置驱动开发

#### 怪物AI配置驱动
```csharp
// 标准初始化流程
var monster = gameObject.AddComponent<MonsterAI_Enhanced>();
monster.Init(5001); // 自动设置配置ID并加载所有参数

// Monster.csv 配置示例
Id,Name,DetectionRange,AttackRange,MoveSpeed,FieldOfView,LostTargetTime
5001,公鸡,5,2,3.5,90,3
5002,母鸡,6,2.5,3,120,4
5003,鸡王,8,3,4,150,5
```

#### 装备系统使用
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
// 在Inspector中设置
[SerializeField] private int _itemId = 1001;  // 草的物品ID

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
var monsters = ObjectManager.Instance.FindAllByType<MonsterAI_Enhanced>(ObjectType.Monster);
var harvestables = ObjectManager.Instance.FindAllByType<HarvestableObject>(ObjectType.Item);
```

## 注意事项

### 配置系统
- MonsterAI_Enhanced必须调用`Init(configId)`来正确初始化
- 配置ID验证：生成前验证配置ID是否存在于配置表中
- 配置表预加载：在游戏启动时预加载常用配置表以提升性能
- 错误处理：配置不存在时提供合理的默认值和错误提示

### 性能优化
- 避免在Update中进行频繁的GetComponent调用
- 使用缓存的距离计算结果
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

## 配置表结构

### Monster.csv - 怪物配置表
```csv
Id,Name,Type,MaxHealth,DetectionRange,AttackRange,MoveSpeed,RotationSpeed,FieldOfView,LostTargetTime,PatrolRadius
5001,公鸡,Normal,50,5,2,3.5,5,90,3,8
5002,母鸡,Normal,200,6,2.5,3,4,120,4,10
5003,鸡王,Boss,300,8,3,4,6,150,5,15
```

### Equip.csv - 装备配置表
```csv
ID,Name,EquipType,Type,Damage,Defense,Durability,DurabilityLoss,UseCooldown,Range,ModelPath
30001,UZI,Uzi,Hand,25,0,100,1,0.1,10,Prefabs/Weapons/Uzi
30002,Axe,Axe,Hand,50,0,200,2,1.0,2,Prefabs/Weapons/Axe
```

### Source.csv - 采集物配置表
```csv
ID,DropItemId,DropCount,DropRate,HarvestTime,DestroyAfterHarvest,InteractionRange,ActionType
1001,2001,1,1.0,0.0,true,2.0,Pull
1002,2002,1,1.0,1.0,false,2.0,Pick
1003,2003,2,1.0,0.5,true,3.0,Chop
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
1. 更新Monster.csv配置表：添加新的怪物ID和属性配置
2. 使用MonsterAI_Enhanced：`monster.Init(newMonsterId)`
3. 自定义怪物行为（可选）：继承MonsterAI_Enhanced，重写特定的AI状态逻辑

#### 添加新的采集物
1. 选择合适的脚本类型：DirectHarvestable/RepeatableHarvestable
2. 配置GameObject：添加对应的脚本组件，设置_itemId
3. 更新配置表：在Source.csv中添加对应ID的配置

#### 添加新的装备
1. 继承对应的装备基类：HandEquipBase/HeadEquipBase/BodyEquipBase
2. 实现特定的装备逻辑：攻击效果、特殊能力等
3. 更新Equip.csv配置表：添加装备的属性配置
