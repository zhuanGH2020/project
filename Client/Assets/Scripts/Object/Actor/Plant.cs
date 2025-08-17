using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 植物系统 - 防御植物
/// 参考Monster的架构，简化实现
/// </summary>
public class Plant : CombatEntity
{
    private float _attackRange = 10f;        // 攻击范围 - 从配置表读取
    private float _attackDamage = 25f;       // 攻击伤害 - 从配置表读取
    private float _attackSpeed = 2f;         // 攻击速度 - 从配置表读取
    private int _cost = 50;                  // 建造成本 - 从配置表读取
    
    private float _maxHealthValue = 100f;    // 最大生命值
    
    private PlantType _plantType;
    private List<CombatEntity> _enemiesInRange = new List<CombatEntity>();
    private CombatEntity _currentTarget;
    private ObjectState _objectState;

    // 实现DamageableObject的抽象属性
    public override float MaxHealth => _maxHealthValue;
    public override bool CanInteract => CurrentHealth > 0;
    public override float GetInteractionRange() => 2f;

    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Building); // 植物归类为建筑物
        _objectState = GetOrAddComponent<ObjectState>();

        Init(70001);
    }

    public void Init(int configId)
    {
        SetConfigId(configId);
        LoadConfigValues();
        
        var idleState = GetOrAddComponent<StateIdle>();
        _objectState.StartState(idleState);
    }

    private void LoadConfigValues()
    {
        var config = GetPlantConfig();
        if (config == null)
        {
            return;
        }

        int configId = ConfigId;
        
        _attackRange = config.GetValue(configId, "AttackRange", 10f);
        _attackDamage = config.GetValue(configId, "AttackDamage", 25f);
        _attackSpeed = config.GetValue(configId, "AttackSpeed", 2f);
        _cost = config.GetValue(configId, "Cost", 50);
        _maxHealthValue = config.GetValue(configId, "MaxHealth", 100f);
        _plantType = config.GetValue<PlantType>(configId, "Type", PlantType.None);
        _attackCooldown = config.GetValue(configId, "CooldownTime", 2f);
        _attackTimer = new CooldownTimer(_attackCooldown);
    }

    private ConfigReader GetPlantConfig()
    {
        return ConfigManager.Instance.GetReader("Plant");
    }

    protected override void Update()
    {
        base.Update();
        
        if (ConfigId > 0)
        {
            UpdatePlantAI();
        }
    }

    private void UpdatePlantAI()
    {
        UpdateEnemyDetection();
        UpdateStateMachine();
    }

    private void UpdateEnemyDetection()
    {
        _enemiesInRange.Clear();
        
        var allMonsterObjects = ObjectManager.Instance.FindAllByType(ObjectType.Monster);
        
        foreach (var monsterObj in allMonsterObjects)
        {
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

    private void SelectTarget()
    {
        if (_enemiesInRange.Count == 0)
        {
            _currentTarget = null;
            return;
        }
        
        if (_currentTarget != null && _enemiesInRange.Contains(_currentTarget))
        {
            return;
        }
        
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

    private void UpdateStateMachine()
    {
        if (_currentTarget != null && !_objectState.IsWorking)
        {
            var attackState = GetOrAddComponent<StateAttack>();
            _objectState.StartState(attackState);
        }
        else if (_currentTarget == null && _objectState.IsWorking)
        {
            var idleState = GetOrAddComponent<StateIdle>();
            _objectState.StartState(idleState);
        }
        
        if (_currentTarget != null && _objectState.IsWorking)
        {
            UpdateAttackBehavior();
        }
    }

    private void UpdateAttackBehavior()
    {
        if (_currentTarget == null || _currentTarget.CurrentHealth <= 0)
        {
            return;
        }
        
        FaceTarget();
        
        if (CanAttack)
        {
            PerformAttack(_currentTarget);
        }
    }

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

    private void PerformAttack(CombatEntity target)
    {
        if (target == null) return;
        
        _attackTimer.StartCooldown();
        
        if (_plantType == PlantType.Shooter)
        {
            PlayAttackEffect();
        }
        else
        {
            var damageInfo = new DamageInfo
            {
                Damage = _attackDamage,
                Type = DamageType.Physical,
                HitPoint = target.transform.position,
                Direction = (target.transform.position - transform.position).normalized,
                Source = this
            };
            
            target.TakeDamage(damageInfo);
            PlayAttackEffect();
        }
    }

    private void PlayAttackEffect()
    {
        switch (_plantType)
        {
            case PlantType.Shooter:
                CreateProjectile();
                break;
            case PlantType.Sunflower:
                break;
        }
    }

    private void CalculateShootingParams(out Vector3 shootPoint, out Vector3 shootDirection)
    {
        shootPoint = transform.position + Vector3.up * 0.5f + transform.forward * 0.3f;
        Vector3 targetPosition = _currentTarget.transform.position;
        targetPosition.y = shootPoint.y;
        shootDirection = (targetPosition - shootPoint).normalized;
    }

    private void CreateProjectile()
    {
        if (_currentTarget == null) return;
        
        var config = GetPlantConfig();
        if (config == null)
        {
            return;
        }
        
        string bulletPath = config.GetValue<string>(ConfigId, "BulletPath", "");
        if (string.IsNullOrEmpty(bulletPath))
        {
            return;
        }
        
        GameObject bulletPrefab = ResourceManager.Instance.Load<GameObject>(bulletPath);
        if (bulletPrefab == null)
        {
            return;
        }
        
        CalculateShootingParams(out Vector3 shootPoint, out Vector3 shootDirection);
        GameObject bulletGO = Object.Instantiate(bulletPrefab, shootPoint, Quaternion.LookRotation(shootDirection));
        
        var bullet = bulletGO.GetComponent<PlantBullet>();
        if (bullet == null)
        {
            Object.Destroy(bulletGO);
            return;
        }
        
        float bulletSpeed = _attackSpeed;
        float bulletMaxDistance = _attackRange;
        
        bullet.Initialize(
            shootPoint, 
            shootDirection, 
            _attackDamage,
            this, 
            bulletSpeed,
            bulletMaxDistance
        );
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        
        var deadState = GetOrAddComponent<StateDead>();
        _objectState.StartState(deadState);
        
        Object.Destroy(gameObject, 1f);
    }

    public string GetPlantInfo()
    {
        var config = GetPlantConfig();
        string plantName = config?.GetValue<string>(ConfigId, "Name", "未知植物") ?? "未知植物";
        string currentState = _objectState.IsWorking ? "工作中" : "空闲";
        
        return $"植物: {plantName}\n" +
               $"类型: {_plantType}\n" +
               $"血量: {CurrentHealth:F0}/{MaxHealth:F0}\n" +
               $"攻击力: {_attackDamage:F0}\n" +
               $"攻击范围: {_attackRange:F1}\n" +
               $"状态: {currentState}";
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || ConfigId == 0) return;
        
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, _attackRange);
        
        if (_currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _currentTarget.transform.position);
            Gizmos.DrawWireSphere(_currentTarget.transform.position, 1f);
        }
    }

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