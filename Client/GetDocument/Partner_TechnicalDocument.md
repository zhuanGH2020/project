# Partner 伙伴系统技术文档

## 简介
Partner伙伴系统是防御植物系统，支持多种伙伴类型（射手、向日葵等），具备完整的敌人检测、攻击逻辑和子弹发射功能。系统从配置表驱动，支持预制体加载和参数自动配置。

## 详细接口

### Partner 主类
**位置**: `Assets/Scripts/Object/Actor/Partner.cs`

#### 核心方法
```csharp
// 初始化伙伴 - 设置配置ID并加载配置参数
public void Init(int configId)

// 获取伙伴信息（调试用）
public string GetPartnerInfo()
```

#### 配置参数
```csharp
[Header("伙伴设置")]
[SerializeField] private float _attackRange = 10f;        // 攻击范围 - 从配置表读取
[SerializeField] private float _attackDamage = 25f;       // 攻击伤害 - 从配置表读取
[SerializeField] private float _attackSpeed = 2f;        // 攻击速度 - 从配置表读取
[SerializeField] private int _cost = 50;                 // 建造成本 - 从配置表读取
```

#### 敌人检测数据结构
```csharp
private PartnerType _partnerType;                         // 伙伴类型
private List<CombatEntity> _enemiesInRange = new List<CombatEntity>();  // 范围内敌人
private CombatEntity _currentTarget;                      // 当前攻击目标
private ObjectState _objectState;                         // 状态机
```

### PartnerBullet 子弹类
**位置**: `Assets/Scripts/Object/Actor/PartnerBullet.cs`

#### 核心方法
```csharp
// 初始化子弹参数
public void Initialize(Vector3 startPos, Vector3 direction, float damage, IAttacker source, 
                      float speed = 15f, float maxDistance = 10f, int bulletConfigId = 0)

// 获取子弹信息（调试用）
public string GetBulletInfo()
```

#### 配置参数
```csharp
[Header("子弹设置")]
[SerializeField] private float _speed = 15f;              // 子弹飞行速度
[SerializeField] private float _maxDistance = 10f;        // 最大飞行距离
[SerializeField] private float _damage = 25f;             // 子弹伤害
[SerializeField] private LayerMask _enemyLayer = -1;      // 敌人层级
```

### 配置表集成
**配置文件**: `Assets/Resources/Configs/Partner.csv`

#### 字段说明
```csv
Id,Name,Type,BulletPath,AttackDamage,AttackSpeed,AttackRange,CooldownTime
70001,豌豆射手,Shooter,Prefabs/Partners/pb_partner_bullet,25,2,15,0.5
```

- `BulletPath`: 子弹预制体路径（相对于Resources文件夹）
- `AttackDamage`: 攻击伤害（直接作为子弹伤害）
- `AttackSpeed`: 攻击速度（影响子弹发射频率和飞行速度）
- `AttackRange`: 攻击范围（决定敌人检测和子弹飞行距离）
- `CooldownTime`: 冷却时间（控制攻击间隔）

## 最佳实践

### 统一敌人检测系统
```csharp
private void UpdateEnemyDetection()
{
    _enemiesInRange.Clear();
    
    // 获取所有怪物类型的对象（包括Monster和MonsterAI_Enhanced）
    var allMonsterObjects = ObjectManager.Instance.FindAllByType(ObjectType.Monster);
    
    foreach (var monsterObj in allMonsterObjects)
    {
        // 检查是否是CombatEntity（战斗实体）
        var combatEntity = monsterObj as CombatEntity;
        if (combatEntity != null && combatEntity.CurrentHealth > 0)
        {
            float distance = Vector3.Distance(transform.position, combatEntity.transform.position);
            if (distance <= _attackRange)
            {
                _enemiesInRange.Add(combatEntity);
            }
        }
    }
    
    SelectTarget();
}
```

### 配置表驱动的子弹创建
```csharp
private void CreateProjectile()
{
    if (_currentTarget == null) return;
    
    // 从配置表获取子弹预制体路径
    var config = GetPartnerConfig();
    string bulletPath = config.GetValue<string>(ConfigId, "BulletPath", "");
    
    // 使用ResourceManager加载子弹预制体
    GameObject bulletPrefab = ResourceManager.Instance.Load<GameObject>(bulletPath);
    GameObject bulletGO = GameObject.Instantiate(bulletPrefab, shootPoint, rotation);
    
         // 使用Partner的攻击参数作为子弹参数
     float bulletSpeed = _attackSpeed;                    // 直接使用攻击速度
     float bulletMaxDistance = _attackRange;              // 直接使用攻击范围
    
    // 初始化子弹
    var bullet = bulletGO.GetComponent<PartnerBullet>();
    bullet.Initialize(shootPoint, shootDirection, _attackDamage, this, bulletSpeed, bulletMaxDistance);
}
```

### 统一碰撞检测（子弹）
```csharp
private void CheckCollision()
{
    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.2f, _enemyLayer);
    
    foreach (var hitCollider in hitColliders)
    {
        // 统一检测CombatEntity类型的敌人（Monster、MonsterAI_Enhanced等）
        var combatEntity = hitCollider.GetComponent<CombatEntity>();
        if (combatEntity != null && combatEntity.CurrentHealth > 0 && !IsPartner(hitCollider))
        {
            // 确认是敌人类型（ObjectType.Monster）
            var objectBase = hitCollider.GetComponent<ObjectBase>();
            if (objectBase != null && objectBase.ObjectType == ObjectType.Monster)
            {
                DealDamage(combatEntity, hitCollider.transform.position);
                return;
            }
        }
    }
}
```

### 参数计算规则
- **子弹伤害** = Partner的AttackDamage（直接使用）
- **子弹速度** = Partner的AttackSpeed（直接使用）
- **飞行距离** = Partner的AttackRange（直接使用）
- **发射间隔** = Partner的CooldownTime（从配置表读取，单位：秒）

## 注意事项

### 配置要求
1. **预制体路径**: 射手类型Partner必须在配置表中设置BulletPath字段
2. **组件要求**: 子弹预制体必须包含PartnerBullet组件
3. **路径规范**: 预制体路径必须相对于Resources文件夹

### 敌人检测要求
1. **类继承**: 敌人必须继承自CombatEntity
2. **对象类型**: 必须设置ObjectType.Monster
3. **碰撞体**: 需要Collider组件用于物理检测

### 支持的敌人类型
1. **Monster** - 基础怪物AI，继承自CombatEntity
2. **MonsterAI_Enhanced** - 增强版怪物AI，继承自CombatEntity
3. **自定义CombatEntity** - 任何继承自CombatEntity且设置ObjectType.Monster的类

### 性能优化
- 使用CombatEntity统一类型减少类型转换开销
- 通过ObjectManager统一管理避免FindObjectsOfType
- 球形碰撞检测避免高速子弹穿透问题

## 其他需要补充的关键点

### 伙伴类型支持
```csharp
public enum PartnerType
{
    None = 0,
    Shooter = 1,    // 豌豆射手 - 发射子弹攻击
    Sunflower = 2,  // 向日葵 - 生产资源（不攻击）
}
```

### 状态机集成
- **StateIdle** - 空闲状态，等待敌人
- **StateAttack** - 攻击状态，发射子弹
- **StateDead** - 死亡状态，播放死亡动画

### 容错机制
- 预制体加载失败时自动创建默认子弹
- 支持多种敌人类型的兼容检测
- 防止误伤友方单位的安全检查

### 调试功能
- **Gizmos可视化**: 攻击范围、当前目标连线等
- **调试信息**: GetPartnerInfo()方法提供实时状态
- **完整日志**: 攻击、子弹发射、碰撞等事件记录

### 扩展建议
1. **特效系统**: 可在攻击和子弹消失时添加特效
2. **音效集成**: 添加发射和击中音效
3. **对象池优化**: 大量子弹时使用ObjectPoolManager
4. **AI增强**: 支持目标优先级、预测射击等

### 相关文件
- `Assets/Scripts/Object/Actor/Partner.cs` - 伙伴主要逻辑
- `Assets/Scripts/Object/Actor/PartnerBullet.cs` - 子弹系统
- `Assets/Resources/Configs/Partner.csv` - 配置表
- `Assets/Scripts/Object/Data/DamageInfo.cs` - 伤害信息结构
- `Assets/Scripts/Object/Base/CombatEntity.cs` - 战斗实体基类 