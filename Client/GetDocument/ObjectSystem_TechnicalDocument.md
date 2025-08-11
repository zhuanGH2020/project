# Object系统技术文档

**创建日期**: 2024年12月  
**版本**: 2.4 

## 简介

Object系统是一个完整的游戏对象交互框架，支持攻击、承伤、装备、采集、建筑等核心游戏机制。系统采用接口驱动设计，支持数据驱动的装备配置，并提供完整的AI行为系统和采集交互体验。

## 系统架构

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
    
    public void StartCooldown();           // 开始冷却
    public void Update();                  // 更新冷却时间
}
```

#### HarvestInfo - 采集信息
```csharp
public struct HarvestInfo
{
    public List<DropItem> drops;                // 掉落物品列表
    public float harvestTime;                   // 采集时间
    public bool destroyAfterHarvest;            // 采集后是否销毁
    public ActionType actionType;               // 特效类型
    public bool requiresTool;                   // 是否需要工具
    public ToolType requiredToolType;           // 需要的工具类型
}
```

#### DropItem - 掉落物品
```csharp
[System.Serializable]
public struct DropItem
{
    public int itemId;           // 物品ID
    public int minCount;         // 最小掉落数量
    public int maxCount;         // 最大掉落数量
    public float dropRate;       // 掉落概率 (0.0-1.0)
    
    public int GetActualDropCount();  // 计算实际掉落数量
}
```

#### ActionType - 动作类型枚举
```csharp
public enum ActionType
{
    None = 0,
    Pull = 1,        // 拔取动作 (草)
    Chop = 2,        // 砍伐动作 (树木)
    Pick = 3,        // 采摘动作 (浆果)
    Mine = 4,        // 挖掘动作 (岩石)
    Collect = 5      // 收集动作 (掉落物)
}
```

## 详细接口

### 基础类

#### ObjectBase - 对象基类（配置系统核心）
**继承**: MonoBehaviour  
**职责**: 所有游戏对象的基类，提供UID、对象类型管理和**配置表查询功能**

**主要属性**:
- `_uid`: 唯一标识符
- `_configId`: **配置表ID**
- `_objectType`: 对象类型

**配置系统核心方法**:
```csharp
// 根据对象类型自动获取对应配置表
public ConfigReader GetConfig()

// 设置配置ID
public void SetConfigId(int configId)
```

**配置表映射规则**:
- 使用枚举名称直接映射：`ObjectType.ToString()`
- `ObjectType.Monster` → `"Monster"` → `Monster.csv`
- `ObjectType.Building` → `"Building"` → `Building.csv`
- `ObjectType.Item` → `"Item"` → `Item.csv`
- `ObjectType.Player` → `"Player"` → `Player.csv`
- `ObjectType.Other` → 不支持配置表

**使用场景**: 所有需要配置表支持的游戏对象的基类

#### DamageableObject - 可承伤物体基类
**继承**: ObjectBase → IDamageable  
**职责**: 所有可承伤物体的基类，提供生命值管理和伤害计算

**主要属性**:
- `_maxHealth`: 最大生命值
- `_defense`: 防御值
- `_currentHealth`: 当前生命值

**核心方法**:
```csharp
public virtual float TakeDamage(DamageInfo damageInfo)
{
    // 计算实际伤害 = 伤害值 - 防御值
    // 扣除生命值并检查死亡
    // 打印调试日志
}

protected virtual void OnDeath()
{
    // 由子类实现具体死亡逻辑
}

public virtual void SetHealth(float health)
{
    // 设置当前血量，用于加载存档
}
```

**使用场景**: Tree、BerryBush、Grass等所有可承伤物体

#### CombatEntity - 战斗实体基类
**继承**: DamageableObject → IAttacker  
**职责**: 所有战斗实体的基类，管理攻击、装备和战斗属性

**主要属性**:
- `_baseAttack`: 基础攻击力
- `_attackCooldown`: 攻击冷却时间
- `_handPoint`: 手部挂载点
- `_headMesh`: 头部渲染器
- `_bodyMesh`: 身体渲染器
- `_equips`: 装备列表

**核心方法**:
```csharp
// 计算总攻击力（基础攻击力 + 装备加成）
public virtual float TotalAttack { get; }

// 计算总防御力（基础防御力 + 装备加成）
public override float Defense { get; }

// 执行攻击
public virtual void PerformAttack(IDamageable target);

// 通过装备ID装备物品（数据驱动）
public virtual void Equip(int equipId);

// 使用手持装备
protected virtual void UseHandEquip();
```

**使用场景**: Player、Monster等所有战斗实体

#### HarvestableObject - 采集物基类
**继承**: DamageableObject → IHarvestable + IClickable  
**职责**: 所有可采集物体的基类，提供采集功能和点击交互

**主要属性**:
- `_drops`: 掉落物品列表
- `_harvestTime`: 采集时间
- `_destroyAfterHarvest`: 采集后是否销毁
- `_actionType`: 动作类型
- `_interactionRange`: 交互范围
- `_requiresTool`: 是否需要工具
- `_requiredToolType`: 需要的工具类型

**核心方法**:
```csharp
public virtual void OnHarvest(IAttacker harvester);
public virtual void OnClick(Vector3 clickPosition);
protected virtual void OnHarvestComplete(IAttacker harvester);
protected virtual void ProcessDrops(IAttacker harvester);
protected virtual void PlayHarvestEffect();
protected virtual void CreateDroppedItem(int itemId, int count);
```

**使用场景**: DirectHarvestable、RepeatableHarvestable、ToolRequiredHarvestable等所有采集物

#### Building - 建筑物基类
**继承**: MonoBehaviour  
**职责**: 管理建筑物的数据和行为，包括唯一标识、等级、交互状态等

**主要属性**:
- `_uid`: 唯一标识符
- `_itemId`: 对应的道具ID
- `_mapPosition`: 地图位置
- `_level`: 建筑物等级
- `_canInteract`: 是否可交互
- `_health`: 当前血量
- `_maxHealth`: 最大血量

**核心方法**:
```csharp
public void Initialize(int itemId, Vector2 mapPos, int uid = 0);
public void SetLevel(int level);
public void SetInteractState(bool canInteract);
public void Demolish();
```

**事件系统**:
```csharp
public event Action<Building> OnDemolished;
public event Action<Building, int> OnLevelChanged;
public event Action<Building, bool> OnInteractStateChanged;
```

**使用场景**: 所有可建造的建筑物体

### 角色类

#### Player - 玩家角色
**继承**: CombatEntity  
**职责**: 玩家特定逻辑，包括输入处理、移动控制和装备快捷键

**主要属性**:
- `_moveSpeed`: 移动速度
- `_instance`: 单例实例

**核心方法**:
```csharp
// 处理输入（WASD移动、空格使用装备、Q/E装备快捷键）
private void HandleInput();

// 处理移动（键盘直接移动，瞬转朝向）
private void HandleMovement();

// 重写攻击实现（玩家只能通过装备造成伤害）
public override void PerformAttack(IDamageable target);

// 移动到指定位置（供InteractionManager调用）
public void MoveToPosition(Vector3 position);
```

**快捷键设置**:
- `WASD`: 移动
- `Space`: 使用手持装备
- `Q`: 装备UZI (ID: 30001)
- `E`: 装备Axe (ID: 30002)

**使用方式**: 挂载到玩家GameObject上，自动实现单例模式

#### Monster - 怪物角色（简化版）
**继承**: CombatEntity  
**职责**: 基础怪物AI，直线追击和攻击

**主要属性**:
- `_detectRange`: 检测范围
- `_attackRange`: 攻击范围
- `_moveSpeed`: 移动速度
- `_rotationSpeed`: 旋转速度

**AI行为**:
```csharp
// 更新AI行为（检测→追击→攻击）
private void UpdateAI();

// 直线追击目标
private void ChaseTarget();

// 面向目标
private void FaceTarget();
```

**使用方式**: 挂载到怪物GameObject上，自动寻找Player实例

#### MonsterAI_Enhanced - 增强版怪物AI（配置驱动）
**继承**: CombatEntity  
**职责**: 完整的怪物AI系统，包含状态机、智能感知、记忆追击等

**核心特性**:
- **配置驱动**: 所有AI参数从Monster.csv配置表读取
- **智能感知**: 基于状态的不同感知策略
- **记忆追击**: 记录玩家最后已知位置，避免因障碍物立即失去目标
- **状态机**: 支持空闲、巡逻、警戒、追击、攻击、眩晕等状态

**初始化方法**:
```csharp
// 必须调用此方法来设置配置ID并加载参数
public void Init(int configId)
{
    SetConfigId(configId);
    LoadConfigValues();
    // ... 其他初始化逻辑
}

// 从配置表加载所有AI参数
private void LoadConfigValues()
{
    var config = GetConfig();
    if (config == null) return;
    
    int configId = ConfigId;
    // 基础战斗参数
    _detectRange = config.GetValue(configId, "DetectionRange", 5f);
    _attackRange = config.GetValue(configId, "AttackRange", 2f);
    _chaseSpeed = config.GetValue(configId, "MoveSpeed", 3.5f);
    _rotationSpeed = config.GetValue(configId, "RotationSpeed", 5f);
    _maxHealth = config.GetValue(configId, "MaxHealth", 100f);
    
    // AI行为参数
    _fieldOfView = config.GetValue(configId, "FieldOfView", 90f);
    _lostTargetTime = config.GetValue(configId, "LostTargetTime", 3f);
    _idleSpeed = config.GetValue(configId, "IdleSpeed", 1f);
    _attackAngle = config.GetValue(configId, "AttackAngle", 45f);
    _patrolRadius = config.GetValue(configId, "PatrolRadius", 8f);
    _patrolWaitTime = config.GetValue(configId, "PatrolWaitTime", 2f);
}
```

**配置参数映射**:
- `DetectionRange` → 检测范围
- `AttackRange` → 攻击范围  
- `MoveSpeed` → 移动速度
- `RotationSpeed` → 旋转速度
- `MaxHealth` → 最大生命值
- `FieldOfView` → 视野角度
- `LostTargetTime` → 失去目标记忆时间
- `IdleSpeed` → 空闲速度
- `AttackAngle` → 攻击角度
- `PatrolRadius` → 巡逻半径
- `PatrolWaitTime` → 巡逻等待时间

**智能感知系统**:
```csharp
// 基于状态的感知策略
private void UpdatePerception()
{
    if (_currentState == MonsterState.Idle || _currentState == MonsterState.Patrol)
    {
        // 空闲/巡逻状态：需要严格的视线检查才能发现目标
        shouldLoseTarget = !CanSeePlayer();
    }
    else if (_currentState == MonsterState.Alert || _currentState == MonsterState.Chase || _currentState == MonsterState.Attack)
    {
        // 警戒/追击/攻击状态：只要在扩大范围内就不会立即失去目标
        // 模拟怪物的记忆能力 - 一旦发现玩家，会记住玩家的大致位置
        shouldLoseTarget = _lastDistanceCheck > _detectRange * 1.5f;
    }
}

// 严格的视线检查（空闲状态使用）
private bool CanSeePlayer()
{
    // 检查距离、视野角度、视线遮挡
    return inRange && inFieldOfView && !isBlocked;
}
```

**记忆追击系统**:
```csharp
// 追击状态的智能目标选择
private void UpdateChaseState()
{
    Vector3 chaseTarget;
    if (CanSeePlayer())
    {
        // 能看到玩家：直接追击玩家当前位置
        chaseTarget = _target.position;
    }
    else
    {
        // 玩家被遮挡：追击最后已知位置
        chaseTarget = _lastKnownPlayerPos;
        
        // 到达最后已知位置后，切换到巡逻状态继续寻找
        if (Vector3.Distance(transform.position, _lastKnownPlayerPos) < 1f)
        {
            ChangeState(MonsterState.Patrol);
            return;
        }
    }
    
    Vector3 direction = (chaseTarget - transform.position).normalized;
    MoveTo(direction, _chaseSpeed);
}
```

**AI状态**:
```csharp
public enum MonsterState
{
    Idle,       // 空闲状态
    Patrol,     // 巡逻状态
    Alert,      // 警戒状态
    Chase,      // 追击状态
    Attack,     // 攻击状态
    Stunned,    // 眩晕状态
    Dead        // 死亡状态
}
```

**状态机行为说明**:
- **Idle（空闲）**: 等待巡逻，使用严格的视线检查发现玩家
- **Patrol（巡逻）**: 在巡逻半径内随机移动，寻找玩家
- **Alert（警戒）**: 发现玩家后的过渡状态，决定追击或攻击
- **Chase（追击）**: 智能追击，记忆玩家最后位置，绕过障碍物
- **Attack（攻击）**: 在攻击范围内对玩家造成伤害
- **Stunned（眩晕）**: 受到控制效果时的无行动状态
- **Dead（死亡）**: 生命值为0时的死亡状态

**核心功能**:
- 智能感知系统（基于状态的不同感知策略）
- 记忆追击系统（记录最后已知位置）
- 视野角度检测和视线遮挡检查
- 状态机管理和流畅的状态转换
- 巡逻路径生成
- 配置表热重载支持

**使用方式**: 
```csharp
// 1. 添加组件
var monster = gameObject.AddComponent<MonsterAI_Enhanced>();

// 2. 使用配置ID初始化
monster.Init(5001); // 初始化为公鸡类型

// 3. AI自动运行，所有参数从配置表加载
```

### 工具类

#### ConfigSystemTest - 配置系统测试
**职责**: 验证配置系统的完整性和性能

**测试功能**:
- Monster配置表加载测试
- ObjectBase配置功能测试
- MonsterAI_Enhanced初始化测试
- 配置查询性能测试

### 采集系统

#### DirectHarvestable - 直接采集物
**继承**: HarvestableObject  
**职责**: 一次性采集物（如草、花、掉落物）

**核心功能**:
- 点击后自动寻路采集
- 采集完成后销毁
- 物品直接进入背包
- 支持配置表驱动

**特有方法**:
```csharp
public void SetItemId(int itemId);      // 设置物品ID
public void SetDropCount(int count);    // 设置掉落数量
```

#### RepeatableHarvestable - 重复采集物
**继承**: HarvestableObject  
**职责**: 可多次收获的采集物（如浆果丛、果树）

**核心功能**:
- 多次采集，每次减少可采集数量
- 全部采集完后开始重新生长
- 支持视觉状态切换
- 自动重新生长计时器

**特有属性**:
```csharp
public int CurrentHarvestCount { get; }     // 当前可采集数量
public int MaxHarvestCount { get; }         // 最大采集数量
public bool IsRegrowing { get; }            // 是否正在重新生长
public float RegrowProgress { get; }        // 重新生长进度
```

#### ToolRequiredHarvestable - 工具采集物
**继承**: HarvestableObject  
**职责**: 需要工具破坏的采集物（如树木、矿石）

**核心功能**:
- 需要先用工具攻击至血量归零
- 死亡后变为可采集状态
- 支持随机掉落数量
- 支持不同状态的视觉切换

**特有属性**:
```csharp
public float HealthPercentage { get; }      // 血量百分比
```

#### CookingPot - 烹饪锅
**继承**: MonoBehaviour → IClickable  
**职责**: 处理玩家与锅的交互，支持烹饪功能

**核心功能**:
- 检测玩家交互范围
- 处理烹饪快捷键输入
- 管理烹饪状态和UI显示

**主要属性**:
```csharp
private float _interactionRange;        // 交互范围
private bool _playerInRange;            // 玩家是否在范围内
```

**使用方式**: 挂载到锅的GameObject上，自动处理交互逻辑

### 装备系统

#### EquipBase - 装备基类
**继承**: MonoBehaviour → IEquipable  
**职责**: 所有装备的基类，管理耐久度、冷却和基础属性

**主要属性**:
- `_equipPart`: 装备部位
- `_configId`: 配置ID
- `_damage`: 攻击力
- `_defense`: 防御力
- `_range`: 攻击范围
- `_maxDurability`: 最大耐久度
- `_durabilityLoss`: 耐久损耗
- `_useCooldown`: 使用冷却

**核心方法**:
```csharp
// 根据配置ID初始化装备（数据驱动）
public virtual void Init(int configId);

// 应用装备效果
protected virtual void ApplyEquipEffect();

// 移除装备效果
protected virtual void RemoveEquipEffect();

// 使用装备（消耗耐久度，开始冷却）
public virtual void Use();
```

#### HandEquipBase - 手部装备基类
**继承**: EquipBase  
**职责**: 手持装备的基类，管理模型加载和攻击逻辑

**主要功能**:
- 自动加载和挂载装备模型
- 提供攻击点位置和方向
- 支持子弹轨迹效果
- 处理攻击命中逻辑

**核心方法**:
```csharp
// 显示子弹轨迹
protected virtual void ShowBulletTrail(Vector3 startPoint, Vector3 endPoint);

// 处理攻击命中
protected virtual void HandleHit(IDamageable target, Vector3 hitPoint);
```

#### HeadEquipBase - 头部装备基类
**继承**: EquipBase  
**职责**: 头部装备的基类，管理材质替换

**主要功能**:
- 自动加载和替换头部材质
- 支持视觉效果的装备

#### BodyEquipBase - 身体装备基类
**继承**: EquipBase  
**职责**: 身体装备的基类，管理材质替换

**主要功能**:
- 自动加载和替换身体材质
- 支持视觉效果的装备

### 具体装备类

#### Axe - 斧头
**继承**: HandEquipBase  
**职责**: 近战武器，可砍树和攻击怪物

**特殊功能**:
- 近战攻击范围
- 攻击命中特效
- 物理伤害类型

#### Uzi - 冲锋枪
**继承**: HandEquipBase  
**职责**: 远程武器，可攻击怪物

**特殊功能**:
- 远程攻击
- 子弹轨迹效果
- 快速连射

#### Shotgun - 散弹枪
**继承**: HandEquipBase  
**职责**: 远程武器，散射攻击

**特殊功能**:
- 多颗子弹散射
- 子弹轨迹效果
- 高伤害

#### Torch - 火把
**继承**: HandEquipBase  
**职责**: 照明和燃烧武器

**特殊功能**:
- 可点燃/熄灭状态
- 燃烧伤害
- 照明效果

#### Armor - 护甲
**继承**: BodyEquipBase  
**职责**: 防御装备，提供防御加成

**特殊功能**:
- 高防御值
- 身体材质替换
- 保护效果

### 特效系统

#### BulletTrail - 子弹轨迹
**职责**: 管理子弹轨迹的视觉效果

**核心方法**:
```csharp
// 创建轨迹效果
public static void CreateTrail(Vector3 startPoint, Vector3 endPoint, Material material);
```

**实现原理**:
- 使用LineRenderer组件
- 自动销毁机制
- 材质支持

**文档支持**:
- 包含详细的README_BulletTrail.md文档
- 说明集成方法和最佳实践

### 管理器

#### InteractionManager - 交互管理器
**职责**: 统一管理所有点击交互行为，处理点击→寻路→交互的完整流程

**核心属性**:
```csharp
public bool IsInteracting { get; }          // 是否正在交互
public IClickable CurrentTarget { get; }    // 当前目标
```

**核心方法**:
```csharp
private void StartInteraction(IClickable target, Vector3 clickPosition);
private void PerformInteraction();
private void CancelCurrentInteraction();
private void UpdateTargetInteraction();
```

**交互流程**:
1. 监听 `ObjectInteractionEvent` 事件
2. 接收事件并开始交互
3. 玩家自动寻路到目标位置
4. 到达目标后自动执行相应操作
5. 物品进入背包或创建地面掉落物

**事件集成**:
- 订阅 `ObjectInteractionEvent` 处理交互请求
- 支持 `NavMeshAgent` 寻路（可选）
- 自动取消无效交互

## 最佳实践

### 1. 配置驱动开发

#### ObjectBase配置系统
```csharp
// 1. 设置对象类型和配置ID
var monster = gameObject.AddComponent<MonsterAI_Enhanced>();
monster.SetObjectType(ObjectType.Monster);
monster.SetConfigId(5001);

// 2. 获取配置表并查询值
var config = monster.GetConfig();
if (config != null)
{
    string name = config.GetValue(5001, "Name", "Unknown");
    float health = config.GetValue(5001, "MaxHealth", 100f);
    // 执行更复杂的配置查询逻辑
}
```

#### 怪物AI配置驱动
```csharp
// 标准初始化流程
var monster = gameObject.AddComponent<MonsterAI_Enhanced>();
monster.Init(5001); // 自动设置配置ID并加载所有参数

// 配置会自动加载以下参数:
// - DetectionRange: 检测范围
// - AttackRange: 攻击范围  
// - MoveSpeed: 移动速度
// - RotationSpeed: 旋转速度
// - MaxHealth: 最大生命值
// - FieldOfView: 视野角度
// - LostTargetTime: 失去目标记忆时间
// - IdleSpeed: 空闲速度
// - AttackAngle: 攻击角度
// - PatrolRadius: 巡逻半径
// - PatrolWaitTime: 巡逻等待时间
// - Name: 怪物名称
```

#### AI行为调优
```csharp
// 通过配置表调整不同类型怪物的AI行为

// 公鸡 - 警觉但容易放弃追击
5001,公鸡,Normal,50,5,5,2,3.5,5,90,3,1,45,8,2

// 母鸡 - 更持久的追击和更大的感知范围  
5002,母鸡,Normal,200,10,6,2.5,3,4,120,4,0.8,60,10,3

// 鸡王 - 极强的感知和记忆能力
5003,鸡王,Boss,30,5,8,3,4,6,150,5,1.2,90,15,1

// 关键参数说明:
// - LostTargetTime: 越大越持久追击
// - FieldOfView: 越大越容易发现玩家  
// - DetectionRange: 基础感知范围
// - PatrolRadius: 巡逻搜索范围
```

#### 智能感知机制
```csharp
// 怪物会根据当前状态使用不同的感知策略:

// 1. 初次发现(Idle/Patrol状态):
// - 需要严格的视线检查(距离+角度+无遮挡)
// - 保证怪物不会隔墙发现玩家

// 2. 追击状态(Chase/Attack状态):  
// - 只检查距离(1.5倍检测范围)
// - 记忆玩家最后位置，绕过障碍物追击
// - 到达最后位置后切换到巡逻状态继续搜索

// 3. 记忆时间控制:
// - LostTargetTime控制失去目标后的记忆保持时间
// - 时间越长，怪物越难甩掉
```

### 2. 装备系统使用

#### 数据驱动装备
```csharp
// 通过装备ID装备物品
player.Equip(30001); // 装备UZI
player.Equip(30002); // 装备Axe
```

#### 装备配置表
```csv
ID,Name,EquipType,Type,Damage,Defense,Durability,DurabilityLoss,UseCooldown,Range,ModelPath,MaterialPath
30001,UZI,Uzi,Hand,25,0,100,1,0.1,10,Prefabs/Weapons/Uzi,Null
30002,Axe,Axe,Hand,50,0,200,2,1.0,2,Prefabs/Weapons/Axe,Null
```

### 3. 伤害系统使用

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

### 4. AI系统使用

#### 基础AI（Monster）
```csharp
// 自动检测和追击玩家
// 在攻击范围内自动攻击
// 无需额外配置
```

#### 增强AI（MonsterAI_Enhanced）
```csharp
// 配置AI参数
[SerializeField] private float _detectRange = 8f;
[SerializeField] private float _fieldOfView = 120f;
[SerializeField] private float _patrolRadius = 5f;

// 自动状态机管理
// 支持调试可视化
```

### 5. 采集系统使用

#### 直接采集物（DirectHarvestable）
**适用于**：草、花等可直接收获的物品

```csharp
// 在Inspector中设置
[SerializeField] private int _itemId = 1001;  // 草的物品ID

// 配置表Source.csv示例
ID,DropItemId,DropCount,DropRate,HarvestTime,DestroyAfterHarvest,InteractionRange,ActionType
1001,2001,1,1.0,0.0,true,2.0,Pull
```

**使用方法**：
1. 点击地图上的草/花
2. 玩家自动寻路到附近
3. 自动采集，物品消失，获得配置表中指定的物品

#### 重复采集物（RepeatableHarvestable）
**适用于**：浆果丛、果树等可多次收获的物品

```csharp
// 在Inspector中设置
[SerializeField] private int _itemId = 1002;  // 浆果丛的物品ID
[SerializeField] private GameObject _harvestableVisual;  // 浆果视觉表现

// 配置表Source.csv示例
ID,DropItemId,DropCount,DropRate,MaxHarvestCount,RegrowTime,InteractionRange,ActionType
1002,2002,1,1.0,3,300.0,2.0,Pick
```

**使用方法**：
1. 点击有浆果的浆果丛
2. 玩家寻路过去采集浆果
3. 浆果数量减少，全部采集完后开始重新生长
4. 一段时间后浆果重新长出，可再次采集

#### 工具采集物（ToolRequiredHarvestable）
**适用于**：树木、矿石等需要工具破坏的物品

```csharp
// 在Inspector中设置
[SerializeField] private int _itemId = 1003;  // 树木的物品ID
[SerializeField] private GameObject _normalState;    // 正常状态模型
[SerializeField] private GameObject _harvestedState; // 倒下状态模型

// 配置表Source.csv示例
ID,MaxHealth,DropItemId,MinDropCount,MaxDropCount,DropRate,RequiresTool,RequiredToolType,InteractionRange,ActionType
1003,100.0,2003,2,4,1.0,true,Axe,3.0,Chop
```

**使用方法**：
1. 使用斧头攻击树木直到倒下（血量归零）
2. 点击倒下的树
3. 玩家寻路过去采集木头
4. 根据配置表随机掉落2-4个木头

#### 地面掉落物拾取
```csharp
// 创建掉落物
GameObject droppedItemGO = new GameObject("DroppedItem");
var directHarvestable = droppedItemGO.AddComponent<DirectHarvestable>();
directHarvestable.SetItemId(itemId);
directHarvestable.SetDropCount(count);
```

**使用方法**：
1. 点击地面的掉落物
2. 玩家寻路过去拾取
3. 物品进入背包，掉落物消失

### 6. 建筑系统使用

#### 创建建筑物
```csharp
// 创建建筑物GameObject
GameObject buildingGO = new GameObject("Building");
var building = buildingGO.AddComponent<Building>();

// 初始化建筑物
building.Initialize(itemId, mapPosition, uniqueId);

// 监听建筑物事件
building.OnDemolished += OnBuildingDemolished;
building.OnLevelChanged += OnBuildingLevelChanged;
```

#### 交互物体使用

#### 创建可承伤物体
```csharp
public class CustomObject : DamageableObject
{
    protected override void OnDeath()
    {
        // 实现自定义死亡逻辑
        PlayDeathEffect();
        DropLoot();
        Destroy(gameObject);
    }
}
```

#### 创建自定义采集物
```csharp
public class CustomHarvestable : HarvestableObject
{
    protected override void Awake()
    {
        base.Awake();
        
        // 配置自定义的掉落和行为
        _drops.Add(new DropItem(customItemId, minCount, maxCount, dropRate));
        _interactionRange = customRange;
    }
    
    protected override void OnHarvestComplete(IAttacker harvester)
    {
        // 自定义采集完成逻辑
        DoCustomBehavior();
    }
}
```

### 7. 装备开发

#### 创建新装备
```csharp
public class CustomWeapon : HandEquipBase
{
    protected override void PlayAttackEffect()
    {
        // 实现自定义攻击特效
        ShowBulletTrail(GetAttackPoint(), GetAttackPoint() + GetAttackDirection() * Range);
    }
}
```

### 8. 配置表管理最佳实践

#### 预加载配置表
```csharp
private void Start()
{
    // 预加载常用配置表
    ConfigManager.Instance.LoadConfig("Monster");
    ConfigManager.Instance.LoadConfig("Equip");
    ConfigManager.Instance.LoadConfig("Source");
}
```

#### 配置验证
```csharp
public void SpawnMonster(int configId)
{
    // 验证配置是否存在
    var config = ConfigManager.Instance.GetReader("Monster");
    if (config == null || !config.HasKey(configId))
    {
        Debug.LogError($"Monster config {configId} not found!");
        return;
    }
    
    // 安全地生成怪物
    // ...
}
```

#### 性能优化
```csharp
// 缓存配置读取器
private ConfigReader _monsterConfig;

private void Awake()
{
    _monsterConfig = ConfigManager.Instance.GetReader("Monster");
}

private void UseConfig()
{
    // 直接使用缓存的读取器，避免重复查找
    string name = _monsterConfig.GetValue(configId, "Name", "Unknown");
}
```

## 注意事项

### 1. 配置系统最佳实践
- **必须调用Init方法**: MonsterAI_Enhanced必须调用`Init(configId)`来正确初始化
- **配置ID验证**: 生成前验证配置ID是否存在于配置表中
- **配置表预加载**: 在游戏启动时预加载常用配置表以提升性能
- **错误处理**: 配置不存在时提供合理的默认值和错误提示
- **性能考虑**: 缓存`ConfigReader`实例，避免重复查找

### 2. 性能优化
- 避免在Update中进行频繁的GetComponent调用
- 使用缓存的距离计算结果
- 合理设置AI检测频率
- 交互检测使用合理的更新频率（默认0.1秒）
- 掉落物自动超时清理（默认300秒）
- 使用对象池管理频繁创建的掉落物
- **配置查询优化**: 缓存ConfigReader实例，避免重复加载

### 3. 内存管理
- 装备模型使用ResourceManager加载和释放
- 及时销毁不需要的特效对象
- 避免内存泄漏
- 视觉组件按需激活/停用，减少渲染开销
- **配置表内存**: 合理使用ConfigManager.ClearConfig()清理不需要的配置

### 4. 配置管理
- 所有装备属性通过CSV配置表管理
- 采集物配置通过Source.csv管理
- **怪物配置通过Monster.csv管理**
- 确保配置表路径正确
- 验证配置数据的有效性
- 配置表在启动时一次性加载，运行时高效查询
- **配置表格式**: 必须包含列名、类型定义、中文说明和数据行

### 5. 调试支持
- 使用Debug.Log输出关键信息
- 启用Gizmos可视化调试
- 合理设置日志级别
- **配置测试**: 使用ConfigSystemTest验证配置系统功能

### 6. 扩展性
- 遵循接口设计原则
- 使用虚方法支持重写
- 保持类的单一职责
- **配置扩展**: 通过ObjectType轻松添加新的配置表类型

### 7. 兼容性保证
- 所有现有的 `Pull()`, `Harvest()` 等方法都保持可用
- 现有的预制体和场景无需修改，自动获得新功能
- 原有的特效和动画系统完全兼容
- **配置兼容**: ObjectBase配置系统向下兼容所有现有对象

### 8. 系统集成
- InteractionManager会在GameMain中自动创建
- 采集物需要添加Collider组件用于射线检测
- 掉落物会自动添加SphereCollider
- 重新生长计时器仅在需要时更新
- **配置集成**: 配置表自动按需加载，无需手动管理

## 文件组织

```
Assets/Scripts/Object/
├── Interface/           # 核心接口
│   ├── IDamageable.cs
│   ├── IAttacker.cs
│   ├── IEquipable.cs
│   └── IHarvestable.cs  # IClickable也在此文件中
├── Data/               # 数据结构
│   ├── DamageInfo.cs
│   ├── CooldownTimer.cs
│   └── HarvestData.cs  # 包含HarvestInfo、DropItem、ActionType
├── Base/               # 基础类
│   ├── ObjectBase.cs   # 配置系统核心基类
│   ├── DamageableObject.cs
│   ├── CombatEntity.cs
│   ├── HarvestableObject.cs
│   └── Building.cs     # 建筑物基类
├── Actor/              # 角色类
│   ├── Player.cs
│   ├── Monster.cs
│   └── MonsterAI_Enhanced.cs  # 配置驱动AI
├── Item/               # 采集和交互物品
│   ├── DirectHarvestable.cs
│   ├── RepeatableHarvestable.cs
│   ├── ToolRequiredHarvestable.cs
│   └── CookingPot.cs   # 烹饪锅
├── Equip/              # 装备系统
│   ├── Base/
│   │   ├── EquipBase.cs
│   │   ├── HandEquipBase.cs
│   │   ├── HeadEquipBase.cs
│   │   └── BodyEquipBase.cs
│   ├── Hand/
│   │   ├── Axe.cs
│   │   ├── Uzi.cs
│   │   ├── Shotgun.cs
│   │   └── Torch.cs
│   ├── Body/
│   │   └── Armor.cs
│   └── Head/            # 头部装备目录（当前为空）
├── Effect/             # 特效系统
│   ├── BulletTrail.cs
│   └── README_BulletTrail.md
└── Test/               # 测试和示例
    └── ConfigSystemTest.cs  # 配置系统测试

Assets/Scripts/Manager/  # 管理器
├── InteractionManager.cs
├── ItemManager.cs
└── ...其他管理器

Assets/Scripts/Core/    # 核心系统
├── Enums.cs           # 包含ToolType、DamageType、ObjectType等枚举
├── Config/            # 配置系统
│   ├── ConfigManager.cs
│   ├── ConfigReader.cs
│   └── ConfigDefinition.cs
└── ...其他核心文件
```

## 配置表结构

### Monster.csv 怪物配置表（新增）
```csv
Id,Name,Type,MaxHealth,RespawnTime,PrefabPath,IconPath,DropId,DropCount,DropChance,DetectionRange,AttackRange,MoveSpeed,RotationSpeed,DialogIds,DialogRange,FieldOfView,LostTargetTime,IdleSpeed,AttackAngle,PatrolRadius,PatrolWaitTime
5001,公鸡,Normal,50,5,Prefabs/Monsters/zombie,Icons/Monsters/zombie,4004,1,0.5,5,2,3.5,5,1|2,10,90,3,1,45,8,2
5002,母鸡,Normal,200,10,Prefabs/Monsters/tank_zombie,Icons/Monsters/tank_zombie,4004,2,0.8,6,2.5,3,4,3|4,10,120,4,0.8,60,10,3
5003,鸡王,Boss,30,5,Prefabs/Monsters/poison_zombie,Icons/Monsters/poison_zombie,4004,1,0.3,8,3,4,6,1|2|3|4,15,150,5,1.2,90,15,1
```

**字段说明**：
- `Id`: 怪物ID（主键）
- `Name`: 怪物名称
- `Type`: 怪物类型（Normal/Boss/Friend）
- `MaxHealth`: 最大生命值
- `DetectionRange`: 检测范围（用于MonsterAI_Enhanced）
- `AttackRange`: 攻击范围（用于MonsterAI_Enhanced）
- `MoveSpeed`: 移动速度（用于MonsterAI_Enhanced）
- `RotationSpeed`: 旋转速度（用于MonsterAI_Enhanced）
- `FieldOfView`: 视野角度（用于MonsterAI_Enhanced）
- `LostTargetTime`: 失去目标记忆时间（用于MonsterAI_Enhanced）
- `IdleSpeed`: 空闲速度（用于MonsterAI_Enhanced）
- `AttackAngle`: 攻击角度（用于MonsterAI_Enhanced）
- `PatrolRadius`: 巡逻半径（用于MonsterAI_Enhanced）
- `PatrolWaitTime`: 巡逻等待时间（用于MonsterAI_Enhanced）
- `RespawnTime`: 重生时间
- `PrefabPath`: 预制体路径
- `DropId`: 掉落物品ID
- `DropCount`: 掉落数量
- `DropChance`: 掉落概率

### Equip.csv 装备配置表
```csv
ID,Name,EquipType,Type,Damage,Defense,Durability,DurabilityLoss,UseCooldown,Range,ModelPath,MaterialPath
30001,UZI,Uzi,Hand,25,0,100,1,0.1,10,Prefabs/Weapons/Uzi,Null
30002,Axe,Axe,Hand,50,0,200,2,1.0,2,Prefabs/Weapons/Axe,Null
```

### Source.csv 采集物配置表
```csv
ID,DropItemId,DropCount,DropRate,HarvestTime,DestroyAfterHarvest,InteractionRange,ActionType,MaxHealth,MinDropCount,MaxDropCount,RequiresTool,RequiredToolType,MaxHarvestCount,RegrowTime
1001,2001,1,1.0,0.0,true,2.0,Pull,,,,,,,
1002,2002,1,1.0,1.0,false,2.0,Pick,,,,,,3,300.0
1003,2003,,1.0,0.5,true,3.0,Chop,100.0,2,4,true,Axe,,
```

**字段说明**：
- `ID`: 物品ID（必须）
- `DropItemId`: 掉落的物品ID，默认为当前ID
- `DropCount`: 掉落数量（DirectHarvestable, RepeatableHarvestable）
- `MinDropCount/MaxDropCount`: 掉落数量范围（ToolRequiredHarvestable）
- `DropRate`: 掉落概率（0.0-1.0）
- `HarvestTime`: 采集时间（秒）
- `DestroyAfterHarvest`: 采集后是否销毁
- `InteractionRange`: 交互范围
- `ActionType`: 动作类型（Pull, Pick, Chop, Mine等）
- `MaxHealth`: 最大血量（ToolRequiredHarvestable）
- `RequiresTool`: 是否需要工具
- `RequiredToolType`: 需要的工具类型（Axe, Chisel等）
- `MaxHarvestCount`: 最大采集次数（RepeatableHarvestable）
- `RegrowTime`: 重新生长时间（RepeatableHarvestable）

## 事件系统集成

```csharp
// 监听采集事件
EventManager.Instance.Subscribe<ObjectInteractionEvent>(OnObjectInteraction);

// 监听物品变化事件
EventManager.Instance.Subscribe<ItemChangeEvent>(OnItemChanged);

// 监听攻击事件
EventManager.Instance.Subscribe<AttackEvent>(OnAttack);

// 监听装备事件
EventManager.Instance.Subscribe<EquipChangeEvent>(OnEquipChange);
```

### ObjectInteractionEvent - 对象交互事件
```csharp
public class ObjectInteractionEvent : IGameEvent
{
    public IClickable Target { get; }           // 交互目标
    public Vector3 ClickPosition { get; }       // 点击位置
}
```

## 其他需要补充的关键点

### 1. 配置系统高级特性
- **热重载支持**: 运行时修改配置表立即生效
- **类型安全**: 强类型配置值获取，避免类型错误
- **性能优化**: 配置表一次加载，多次查询
- **错误恢复**: 配置缺失时的优雅降级机制

### 2. 网络同步支持
- 系统设计支持网络同步扩展
- 关键数据使用可序列化结构
- 预留网络事件接口
- **配置同步**: 支持服务器端配置下发

### 3. 数据持久化
- 装备耐久度可保存
- 角色状态可序列化
- 采集物重新生长状态可持久化
- 建筑物状态可持久化
- 支持存档系统
- **配置版本**: 配置表版本控制和迁移支持

### 4. 音效系统集成
- 攻击音效支持
- 装备使用音效
- 采集动作音效
- 建筑音效
- 环境音效集成
- **配置音效**: 音效配置表支持

### 5. 动画系统集成
- 攻击动画支持
- 装备切换动画
- 采集动作动画
- 建筑动画
- 死亡动画系统
- **配置动画**: 动画配置表支持

### 6. 粒子系统集成
- 攻击特效
- 装备特效
- 采集特效
- 建筑特效
- 环境特效
- **配置特效**: 特效配置表支持

### 7. 扩展指南

#### 添加新的采集物
1. **选择合适的脚本类型**：
   - 一次性采集 → `DirectHarvestable`
   - 可重复采集 → `RepeatableHarvestable`  
   - 需要工具 → `ToolRequiredHarvestable`

2. **配置GameObject**：
   - 添加对应的脚本组件
   - 设置 `_itemId` 为配置表中的ID
   - 配置必要的视觉组件（如_harvestableVisual）

3. **更新配置表**：
   - 在 `Source.csv` 中添加对应ID的配置
   - 设置掉落物品、数量、概率等参数

#### 添加新的建筑物
1. **创建建筑物类**：
   - 继承 `Building` 基类或直接使用
   - 实现特定的建筑物逻辑

2. **配置建筑物**：
   - 设置建筑物的基本属性
   - 配置交互行为和升级逻辑

3. **事件处理**：
   - 订阅建筑物相关事件
   - 实现自定义的建筑物行为

#### 添加新的怪物类型
1. **更新Monster.csv配置表**：
   - 添加新的怪物ID和属性配置
   - 设置AI参数（检测范围、攻击范围等）

2. **使用MonsterAI_Enhanced**：
   ```csharp
   var monster = gameObject.AddComponent<MonsterAI_Enhanced>();
   monster.Init(newMonsterId); // 使用新的配置ID
   ```

3. **自定义怪物行为**（可选）：
   - 继承MonsterAI_Enhanced
   - 重写特定的AI状态逻辑

#### 添加新的配置表类型
1. **扩展ObjectType枚举**：
   ```csharp
   public enum ObjectType
   {
       // ... 现有类型
       NewType = 5,  // 添加新类型
   }
   ```

2. **无需修改ObjectBase代码**：
   - 系统使用`ObjectType.ToString()`自动映射
   - 新的`ObjectType.NewType`会自动映射到`"NewType"`
   - 对应配置文件：`NewType.csv`

3. **创建对应的配置表文件**：
   - 在 `Assets/Resources/Configs/` 目录下创建 `NewTypeConfig.csv`
   - 遵循配置表格式规范

---

## 版本历史

### 版本 2.4 (2024年12月)
**MonsterAI追击系统重大优化**:
- ✅ **智能感知系统**: 基于状态的不同感知策略，空闲状态需要严格视线检查，追击状态具有记忆能力
- ✅ **记忆追击机制**: 记录玩家最后已知位置，避免因障碍物立即失去目标
- ✅ **状态感知差异化**: 追击状态使用1.5倍检测范围，提升追击持续性
- ✅ **智能路径规划**: 到达最后已知位置后自动切换巡逻状态继续搜索
- ✅ **调试日志清理**: 移除所有调试日志，提升运行性能

### 版本 2.3 (2024年12月)
**配置系统完善**:
- ✅ ObjectBase配置功能实现
- ✅ MonsterAI_Enhanced全面配置驱动
- ✅ Monster.csv配置表扩展（包含所有AI参数）
- ✅ MonsterSpawner示例脚本移除

### 版本 2.2 (2024年12月)
**系统整合优化**:
- ✅ 采集系统与对象系统合并
- ✅ 技术文档统一和更新
- ✅ 代码规范统一（中文注释）

### 版本 2.1 (2024年12月)
**装备系统增强**:
- ✅ 配置驱动装备系统
- ✅ 子弹轨迹系统
- ✅ 装备耐久和冷却机制

### 版本 2.0 (2024年12月)
**初始版本**:
- ✅ 核心接口设计（IDamageable, IAttacker, IEquipable）
- ✅ 基础类架构（ObjectBase, CombatEntity等）
- ✅ 基础AI系统和装备系统

---
