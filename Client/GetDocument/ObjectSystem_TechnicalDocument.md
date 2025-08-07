# Object系统技术文档

**创建日期**: 2024年12月  
**版本**: 2.0  

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

#### DamageableObject - 可承伤物体基类
**继承**: MonoBehaviour → IDamageable  
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

#### MonsterAI_Enhanced - 增强版怪物AI
**继承**: CombatEntity  
**职责**: 完整的怪物AI系统，包含状态机、视野检测、巡逻等

**主要属性**:
- `_detectRange`: 检测范围
- `_attackRange`: 攻击范围
- `_fieldOfView`: 视野角度
- `_patrolRadius`: 巡逻半径
- `_spawnPoint`: 出生点

**AI状态**:
```csharp
public enum MonsterState
{
    Idle,       // 空闲状态
    Patrol,     // 巡逻状态
    Chase,      // 追击状态
    Attack,     // 攻击状态
    Return      // 返回状态
}
```

**核心功能**:
- 视野角度检测
- 状态机管理
- 巡逻路径生成
- 调试可视化

**使用方式**: 挂载到怪物GameObject上，提供完整的AI行为

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

### 1. 装备系统使用

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

### 2. 伤害系统使用

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

### 3. AI系统使用

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

### 4. 采集系统使用

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

### 5. 建筑系统使用

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

### 6. 装备开发

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

## 注意事项

### 1. 性能优化
- 避免在Update中进行频繁的GetComponent调用
- 使用缓存的距离计算结果
- 合理设置AI检测频率
- 交互检测使用合理的更新频率（默认0.1秒）
- 掉落物自动超时清理（默认300秒）
- 使用对象池管理频繁创建的掉落物

### 2. 内存管理
- 装备模型使用ResourceManager加载和释放
- 及时销毁不需要的特效对象
- 避免内存泄漏
- 视觉组件按需激活/停用，减少渲染开销

### 3. 配置管理
- 所有装备属性通过CSV配置表管理
- 采集物配置通过Source.csv管理
- 确保配置表路径正确
- 验证配置数据的有效性
- 配置表在启动时一次性加载，运行时高效查询

### 4. 调试支持
- 使用Debug.Log输出关键信息
- 启用Gizmos可视化调试
- 合理设置日志级别

### 5. 扩展性
- 遵循接口设计原则
- 使用虚方法支持重写
- 保持类的单一职责

### 6. 兼容性保证
- 所有现有的 `Pull()`, `Harvest()` 等方法都保持可用
- 现有的预制体和场景无需修改，自动获得新功能
- 原有的特效和动画系统完全兼容

### 7. 系统集成
- InteractionManager会在GameMain中自动创建
- 采集物需要添加Collider组件用于射线检测
- 掉落物会自动添加SphereCollider
- 重新生长计时器仅在需要时更新

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
│   ├── DamageableObject.cs
│   ├── CombatEntity.cs
│   ├── HarvestableObject.cs
│   └── Building.cs     # 建筑物基类
├── Actor/              # 角色类
│   ├── Player.cs
│   ├── Monster.cs
│   └── MonsterAI_Enhanced.cs
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
└── Effect/             # 特效系统
    ├── BulletTrail.cs
    └── README_BulletTrail.md

Assets/Scripts/Manager/  # 管理器
├── InteractionManager.cs
├── ItemManager.cs
└── ...其他管理器

Assets/Scripts/Core/    # 核心系统
├── Enums.cs           # 包含ToolType、DamageType等枚举
└── ...其他核心文件
```

## 配置表结构

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

### 1. 网络同步支持
- 系统设计支持网络同步扩展
- 关键数据使用可序列化结构
- 预留网络事件接口

### 2. 数据持久化
- 装备耐久度可保存
- 角色状态可序列化
- 采集物重新生长状态可持久化
- 建筑物状态可持久化
- 支持存档系统

### 3. 音效系统集成
- 攻击音效支持
- 装备使用音效
- 采集动作音效
- 建筑音效
- 环境音效集成

### 4. 动画系统集成
- 攻击动画支持
- 装备切换动画
- 采集动作动画
- 建筑动画
- 死亡动画系统

### 5. 粒子系统集成
- 攻击特效
- 装备特效
- 采集特效
- 建筑特效
- 环境特效

### 6. 扩展指南

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
