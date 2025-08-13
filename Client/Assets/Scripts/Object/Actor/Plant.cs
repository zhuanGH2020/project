using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 植物系统 - 防御植物
/// 参考Monster的架构，简化实现
/// </summary>
public class Plant : CombatEntity
{
    [Header("植物设置")]
    [SerializeField] private float _attackRange = 10f;        // 攻击范围 - 从配置表读取
    [SerializeField] private float _attackDamage = 25f;       // 攻击伤害 - 从配置表读取
    [SerializeField] private float _attackSpeed = 2f;        // 攻击速度 - 从配置表读取
    [SerializeField] private int _cost = 50;                 // 建造成本 - 从配置表读取
    
    private PlantType _plantType;
    private List<CombatEntity> _enemiesInRange = new List<CombatEntity>();
    private CombatEntity _currentTarget;
    private ObjectState _objectState;

    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Building); // 植物归类为建筑物
        _objectState = GetOrAddComponent<ObjectState>();

        Init(70001);
    }

    /// <summary>
    /// 初始化植物 - 设置配置ID并加载配置参数
    /// </summary>
    /// <param name="configId">植物配置ID</param>
    public void Init(int configId)
    {
        SetConfigId(configId);
        LoadConfigValues();
        
        // 启动空闲状态
        var idleState = GetOrAddComponent<StateIdle>();
        _objectState.StartState(idleState);
    }

    /// <summary>
    /// 从配置表加载数值参数
    /// </summary>
    private void LoadConfigValues()
    {
        var config = GetPlantConfig();
        if (config == null)
        {
            Debug.LogError("无法获取Plant配置表");
            return;
        }

        int configId = ConfigId;
        
        // 从Plant.csv配置表读取参数
        _attackRange = config.GetValue(configId, "AttackRange", 10f);
        _attackDamage = config.GetValue(configId, "AttackDamage", 25f);
        _attackSpeed = config.GetValue(configId, "AttackSpeed", 2f);
        _cost = config.GetValue(configId, "Cost", 50);
        
        // 设置基础属性
        _maxHealth = config.GetValue(configId, "MaxHealth", 100f);
        _currentHealth = _maxHealth;
        
        // 获取植物类型
        _plantType = config.GetValue<PlantType>(configId, "Type", PlantType.None);
        
        // 设置攻击冷却时间 - 从配置表读取CooldownTime
        _attackCooldown = config.GetValue(configId, "CooldownTime", 2f);
        _attackTimer = new CooldownTimer(_attackCooldown);
    }

    /// <summary>
    /// 获取植物配置表
    /// </summary>
    private ConfigReader GetPlantConfig()
    {
        return ConfigManager.Instance.GetReader("Plant");
    }

    protected override void Update()
    {
        base.Update();
        
        // 只有在初始化完成后才运行AI
        if (ConfigId > 0)
        {
            UpdatePlantAI();
        }
    }

    /// <summary>
    /// 植物AI主更新循环
    /// </summary>
    private void UpdatePlantAI()
    {
        // 更新敌人检测
        UpdateEnemyDetection();
        
        // 状态机更新
        UpdateStateMachine();
    }

    /// <summary>
    /// 更新敌人检测
    /// </summary>
    private void UpdateEnemyDetection()
    {
        _enemiesInRange.Clear();
        
        // 获取所有怪物类型的对象（Monster等）
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
        
        // 选择最近的敌人作为目标
        SelectTarget();
    }

    /// <summary>
    /// 选择攻击目标
    /// </summary>
    private void SelectTarget()
    {
        if (_enemiesInRange.Count == 0)
        {
            _currentTarget = null;
            return;
        }
        
        // 如果当前目标仍然有效，继续攻击
        if (_currentTarget != null && _enemiesInRange.Contains(_currentTarget))
        {
            return;
        }
        
        // 选择最近的敌人
        CombatEntity closestEnemy = null;
        float closestDistance = float.MaxValue;
        
        foreach (var enemy in _enemiesInRange)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        
        _currentTarget = closestEnemy;
    }

    /// <summary>
    /// 状态机更新
    /// </summary>
    private void UpdateStateMachine()
    {
        // 检查是否需要切换状态
        if (_currentTarget != null && !_objectState.IsWorking)
        {
            // 发现目标，切换到攻击状态
            var attackState = GetOrAddComponent<StateAttack>();
            _objectState.StartState(attackState);
        }
        else if (_currentTarget == null && _objectState.IsWorking)
        {
            // 失去目标，切换到空闲状态
            var idleState = GetOrAddComponent<StateIdle>();
            _objectState.StartState(idleState);
        }
        
        // 如果在攻击状态且有目标，执行攻击逻辑
        if (_currentTarget != null && _objectState.IsWorking)
        {
            UpdateAttackBehavior();
        }
    }

    /// <summary>
    /// 攻击行为更新
    /// </summary>
    private void UpdateAttackBehavior()
    {
        if (_currentTarget == null || _currentTarget.CurrentHealth <= 0)
        {
            return;
        }
        
        // 面向目标
        FaceTarget();
        
        // 攻击
        if (CanAttack)
        {
            PerformAttack(_currentTarget);
        }
    }

    /// <summary>
    /// 面向目标
    /// </summary>
    private void FaceTarget()
    {
        if (_currentTarget == null) return;
        
        Vector3 direction = (_currentTarget.transform.position - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    private void PerformAttack(CombatEntity target)
    {
        if (target == null) return;
        
        // 重置攻击计时器
        _attackTimer.StartCooldown();
        
        // 根据植物类型执行不同的攻击方式
        if (_plantType == PlantType.Shooter)
        {
            // 射手类型：发射子弹，由子弹造成伤害
            PlayAttackEffect();
            Debug.Log($"[Plant] {gameObject.name} 向 {target.name} 发射子弹");
        }
        else
        {
            // 其他类型：直接造成伤害（近战攻击）
            var damageInfo = new DamageInfo
            {
                Damage = _attackDamage,
                Type = DamageType.Physical,
                HitPoint = target.transform.position,
                Direction = (target.transform.position - transform.position).normalized,
                Source = this
            };
            
            // 对目标造成伤害
            target.TakeDamage(damageInfo);
            
            // 播放攻击效果
            PlayAttackEffect();
            
            Debug.Log($"[Plant] {gameObject.name} 攻击 {target.name}，造成 {_attackDamage} 伤害");
        }
    }

    /// <summary>
    /// 播放攻击效果
    /// </summary>
    private void PlayAttackEffect()
    {
        // 这里可以根据不同的植物类型播放不同的特效
        switch (_plantType)
        {
            case PlantType.Shooter:
                // 豌豆射手发射豌豆效果
                CreateProjectile();
                break;
            case PlantType.Sunflower:
                // 向日葵不攻击，但可以有其他效果
                break;
        }
    }

    /// <summary>
    /// 计算发射位置和方向
    /// </summary>
    /// <param name="shootPoint">发射位置</param>
    /// <param name="shootDirection">射击方向</param>
    private void CalculateShootingParams(out Vector3 shootPoint, out Vector3 shootDirection)
    {
        // 计算发射位置（植物前方稍高位置）
        shootPoint = transform.position + Vector3.up * 0.5f + transform.forward * 0.3f;
        
        // 计算射击方向（水平方向，y轴保持与发射点一致）
        Vector3 targetPosition = _currentTarget.transform.position;
        targetPosition.y = shootPoint.y; // 保持与发射点相同的y轴高度
        shootDirection = (targetPosition - shootPoint).normalized;
    }

    /// <summary>
    /// 创建投射物 - 发射子弹攻击目标
    /// </summary>
    private void CreateProjectile()
    {
        if (_currentTarget == null) return;
        
        // 从配置表获取子弹预制体路径
        var config = GetPlantConfig();
        if (config == null)
        {
            Debug.LogError("[Plant] 无法获取Plant配置表");
            return;
        }
        
        string bulletPath = config.GetValue<string>(ConfigId, "BulletPath", "");
        if (string.IsNullOrEmpty(bulletPath))
        {
            Debug.LogError($"[Plant] 配置ID {ConfigId} 没有设置BulletPath，无法发射子弹");
            return;
        }
        
        // 使用ResourceManager加载子弹预制体
        GameObject bulletPrefab = ResourceManager.Instance.Load<GameObject>(bulletPath);
        if (bulletPrefab == null)
        {
            Debug.LogError($"[Plant] 无法加载子弹预制体: {bulletPath}，无法发射子弹");
            return;
        }
        
        // 计算发射位置和方向
        CalculateShootingParams(out Vector3 shootPoint, out Vector3 shootDirection);
        
        // 实例化子弹预制体
        GameObject bulletGO = Object.Instantiate(bulletPrefab, shootPoint, Quaternion.LookRotation(shootDirection));
        
        // 获取PlantBullet组件并验证
        var bullet = bulletGO.GetComponent<PlantBullet>();
        if (bullet == null)
        {
            Debug.LogError($"[Plant] 子弹预制体缺少PlantBullet组件: {bulletPath}");
            Object.Destroy(bulletGO);
            return;
        }
        
        float bulletSpeed = _attackSpeed;
        float bulletMaxDistance = _attackRange;
        
        // 初始化子弹
        bullet.Initialize(
            shootPoint, 
            shootDirection, 
            _attackDamage,      // 使用Plant的攻击伤害
            this, 
            bulletSpeed,        // 根据攻击速度计算的子弹速度
            bulletMaxDistance   // 基于攻击范围的飞行距离
        );
        
        Debug.Log($"[Plant] {gameObject.name} 发射子弹攻击 {_currentTarget.name} (预制体: {bulletPath})");
    }


    /// <summary>
    /// 处理植物死亡
    /// </summary>
    protected override void OnDeath()
    {
        base.OnDeath();
        
        // 切换到死亡状态
        var deadState = GetOrAddComponent<StateDead>();
        _objectState.StartState(deadState);
        
        // 植物死亡处理
        Debug.Log($"[Plant] {gameObject.name} 被摧毁");
        Object.Destroy(gameObject, 1f);
    }

    /// <summary>
    /// 获取植物信息
    /// </summary>
    public string GetPlantInfo()
    {
        var config = GetPlantConfig();
        string plantName = config?.GetValue<string>(ConfigId, "Name", "未知植物") ?? "未知植物";
        string currentState = _objectState.IsWorking ? "工作中" : "空闲";
        
        return $"植物: {plantName}\n" +
               $"类型: {_plantType}\n" +
               $"血量: {_currentHealth:F0}/{_maxHealth:F0}\n" +
               $"攻击力: {_attackDamage:F0}\n" +
               $"攻击范围: {_attackRange:F1}\n" +
               $"状态: {currentState}";
    }

    // 调试用的Gizmos
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || ConfigId == 0) return;
        
        // 攻击范围
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, _attackRange);
        
        // 当前目标
        if (_currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _currentTarget.transform.position);
            Gizmos.DrawWireSphere(_currentTarget.transform.position, 1f);
        }
    }

    /// <summary>
    /// 绘制线框圆形
    /// </summary>
    private void DrawWireCircle(Vector3 center, float radius)
    {
        const int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + Vector3.forward * radius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
} 