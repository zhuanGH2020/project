# Object系统技术文档

## 简介
Object系统是游戏中所有可交互对象的核心架构，实现了战斗、装备、交互等核心游戏机制。

## 详细接口

### 核心接口

#### IDamageable - 可承受伤害接口
```csharp
public interface IDamageable
{
    float MaxHealth { get; }        // 最大生命值
    float CurrentHealth { get; }    // 当前生命值
    float Defense { get; }          // 防御值
    float TakeDamage(DamageInfo damageInfo); // 承受伤害
}
```

#### IAttacker - 攻击者接口
```csharp
public interface IAttacker
{
    float BaseAttack { get; }       // 基础攻击力
    bool CanAttack { get; }         // 是否可以攻击
    void PerformAttack(IDamageable target); // 执行攻击
}
```

#### IEquipable - 可装备接口
```csharp
public interface IEquipable
{
    float MaxDurability { get; }    // 最大耐久度
    float CurrentDurability { get; } // 当前耐久度
    bool IsEquipped { get; }        // 是否已装备
    bool CanUse { get; }           // 是否可使用
    void OnEquip(IAttacker owner);  // 装备时调用
    void OnUnequip();              // 卸下时调用
    void Use();                    // 使用装备
    float GetAttackBonus();        // 获取攻击加成
    float GetDefenseBonus();       // 获取防御加成
}
```

### 数据结构

#### DamageInfo - 伤害信息
```csharp
public struct DamageInfo
{
    public float Damage;           // 伤害值
    public DamageType Type;        // 伤害类型
    public Vector3 HitPoint;       // 击中点
    public IAttacker Source;       // 伤害来源
    public Vector3 Direction;      // 伤害方向
}
```

#### CooldownTimer - 冷却计时器
```csharp
public class CooldownTimer
{
    public float CooldownTime { get; }     // 冷却时间
    public float RemainingTime { get; }    // 剩余时间
    public bool IsReady { get; }           // 是否就绪
    
    public void StartCooldown();           // 开始冷却
    public void Update();                  // 更新计时器
}
```

### 基础类

#### DamageableObject - 可承伤物体基类
- **挂载**: 需要挂载到GameObject上
- **功能**: 提供生命值、防御值和伤害处理的基础实现
- **配置**: 
  - `_maxHealth`: 最大生命值
  - `_defense`: 防御值

#### CombatEntity - 战斗实体基类
- **挂载**: 需要挂载到GameObject上
- **继承**: DamageableObject
- **功能**: 实现攻击能力和装备管理
- **配置**:
  - `_baseAttack`: 基础攻击力
  - `_attackCooldown`: 攻击冷却时间
  - `_handPoint`: 手部挂载点
  - `_headMesh`/`_bodyMesh`: 头部/身体渲染器

### 角色类

#### Player - 玩家角色
- **挂载**: 需要挂载到Player GameObject上
- **继承**: CombatEntity
- **特性**: 单例模式，WASD移动，装备切换
- **依赖**: NavMeshAgent（可选）

#### Monster - 怪物角色
- **挂载**: 需要挂载到Monster GameObject上
- **继承**: CombatEntity
- **特性**: AI寻路，自动攻击玩家
- **依赖**: NavMeshAgent（必需）
- **配置**:
  - `_detectRange`: 检测范围
  - `_attackRange`: 攻击范围
  - `_moveSpeed`: 移动速度

### 装备系统

#### EquipBase - 装备基类
- **挂载**: 动态挂载到CombatEntity上
- **功能**: 提供耐久度、冷却时间、战斗属性管理
- **数据来源**: 从Equip.csv配置表加载属性

#### HandEquipBase - 手部装备基类
- **继承**: EquipBase
- **功能**: 管理手部武器模型和轨迹线特效
- **特性**: 支持子弹轨迹线显示

#### HeadEquipBase/BodyEquipBase - 头部/身体装备基类
- **继承**: EquipBase
- **功能**: 通过材质替换实现装备外观

### 具体装备

#### 武器类
- **Axe**: 近战武器，物理伤害，有命中特效
- **Uzi**: 自动武器，射线检测，子弹轨迹
- **Shotgun**: 散弹武器，多发子弹，散射轨迹
- **Torch**: 特殊武器，火焰伤害，需要点燃

#### 防具类
- **Armor**: 身体防具，提供防御加成

### 交互物体

#### Tree - 树木
- **挂载**: 需要挂载到Tree GameObject上
- **继承**: DamageableObject
- **特性**: 只受物理伤害影响

#### BerryBush/Grass - 浆果丛/草
- **挂载**: 需要挂载到对应GameObject上
- **继承**: DamageableObject
- **特性**: 生命值为0，无法被破坏

### 特效系统

#### BulletTrail - 子弹轨迹
- **挂载**: 运行时动态创建
- **功能**: 使用LineRenderer显示子弹轨迹线
- **特性**: 自动淡出和销毁

## 最佳实践

### 装备使用示例
```csharp
// 玩家装备武器
player.Equip(30001); // 装备UZI

// 使用手部装备
if (Input.GetKeyDown(KeyCode.Space))
{
    player.UseHandEquip();
}
```

### 怪物AI配置
```csharp
// Monster组件配置
[SerializeField] private float _detectRange = 5f;    // 检测范围
[SerializeField] private float _attackRange = 2f;    // 攻击范围
[SerializeField] private float _moveSpeed = 3.5f;    // 移动速度
```

### 装备数据配置
```csharp
// 装备初始化（从配置表加载）
public void Init(int configId)
{
    var equipConfig = EquipManager.Instance.GetEquip(configId);
    _damage = equipConfig.Csv.GetValue<float>(configId, "Damage");
    _defense = equipConfig.Csv.GetValue<float>(configId, "Defense");
    // ...其他属性
}
```

### 伤害处理示例
```csharp
var damageInfo = new DamageInfo
{
    Damage = GetAttackBonus(),
    Type = DamageType.Physical,
    HitPoint = hitPoint,
    Direction = GetAttackDirection(),
    Source = _owner
};
target.TakeDamage(damageInfo);
```

## 注意事项

### 组件依赖
- Monster必须有NavMeshAgent组件
- CombatEntity需要配置挂载点（HandPoint、HeadMesh、BodyMesh）
- 装备类从配置表动态创建，不要手动添加到场景

### 性能考虑
- Monster使用距离缓存避免重复计算
- BulletTrail自动销毁防止内存泄漏
- 装备模型通过ResourceManager管理，支持引用计数

### 数据驱动
- 所有装备属性从Equip.csv加载
- 支持热更新装备数据
- 配置ID用于区分不同装备类型

### 扩展性
- 新装备类型继承对应基类即可
- 新交互物体继承DamageableObject
- 支持自定义伤害类型和特效 