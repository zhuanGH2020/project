using UnityEngine;

/// <summary>
/// 僵尸AI - 特殊AI系统
/// </summary>
public class Zombie : Monster
{
    // 私有变量
    private Transform _currentTarget;           // 当前目标
    private Transform _techTable;               // 科技台对象
    private float _lastTargetSearchTime;        // 上次搜索目标时间
    private float _lastPlantCheckTime;          // 上次Plant检测时间

    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Monster);
        
        Debug.Log("[Zombie] 僵尸Awake完成，等待外部调用Init");
    }

    protected override void Update()
    {
        base.Update();
        
        if (ConfigId > 0)
        {
            UpdateZombieAI();
        }
    }

    /// <summary>
    /// 僵尸AI主循环
    /// </summary>
    private void UpdateZombieAI()
    {
        // 每0.3秒更新目标
        if (Time.time - _lastTargetSearchTime > 0.3f)
        {
            UpdateTarget();
            _lastTargetSearchTime = Time.time;
        }

        // 移动和攻击
        if (_currentTarget != null)
        {
            MoveTowardsTarget();
            TryAttackTarget();
        }
    }

    /// <summary>
    /// 更新目标 - 按优先级选择
    /// </summary>
    private void UpdateTarget()
    {
        var player = FindObjectOfType<Player>();
        
        // 1. 玩家在攻击范围内 - 最高优先级
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= _attackRange)
            {
                _currentTarget = player.transform;
                return;
            }
        }

        // 2. 检查前进路线上的Plant
        var plantInPath = GetPlantInPath();
        if (plantInPath != null)
        {
            _currentTarget = plantInPath;
            return;
        }

        // 3. 查找科技台
        if (_techTable == null)
        {
            FindTechTable();
        }

        // 4. 设置目标：科技台 > 玩家
        if (_techTable != null)
        {
            _currentTarget = _techTable;
        }
        else if (player != null)
        {
            _currentTarget = player.transform;
        }
        else
        {
            _currentTarget = null;
        }
    }

    /// <summary>
    /// 查找科技台
    /// </summary>
    private void FindTechTable()
    {
        if (ObjectManager.Instance != null)
        {
            var techTables = ObjectManager.Instance.FindAllByType<ObjectBase>(ObjectType.TechTable);
            foreach (var techTable in techTables)
            {
                _techTable = techTable.transform;
                Debug.Log("[Zombie] 发现科技台");
                break;
            }
        }
    }

    /// <summary>
    /// 获取前进路线上的Plant（作为阻挡目标）
    /// </summary>
    private Transform GetPlantInPath()
    {
        if (_currentTarget == null) return null;

        // 限制检测频率
        if (Time.time - _lastPlantCheckTime < 0.2f)
        {
            return null;
        }
        _lastPlantCheckTime = Time.time;

        var plants = FindObjectsOfType<Plant>();
        Vector3 directionToTarget = (_currentTarget.position - transform.position).normalized;
        
        Transform closestPlant = null;
        float closestDistance = float.MaxValue;

        foreach (var plant in plants)
        {
            if (plant.CurrentHealth <= 0) continue;

            Vector3 directionToPlant = (plant.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(directionToTarget, directionToPlant);
            float distance = Vector3.Distance(transform.position, plant.transform.position);

            // 在前进方向的锥形范围内且距离合理
            // 使用Monster基类配置：攻击角度作为检测角度，检测范围作为Plant检测范围
            if (angle <= _attackAngle * 0.5f && distance <= _detectRange && distance < closestDistance)
            {
                closestPlant = plant.transform;
                closestDistance = distance;
            }
        }

        return closestPlant;
    }

    /// <summary>
    /// 朝目标移动（无障碍）
    /// </summary>
    private void MoveTowardsTarget()
    {
        if (_currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, _currentTarget.position);
        
        // 不在攻击范围内则移动
        if (distance > _attackRange)
        {
            Vector3 direction = GetMovementDirection();
            MoveToIgnoreObstacles(direction, _chaseSpeed);
        }
    }

    /// <summary>
    /// 获取移动方向（考虑僵尸间距）
    /// </summary>
    private Vector3 GetMovementDirection()
    {
        Vector3 baseDirection = (_currentTarget.position - transform.position).normalized;
        
        // 检测附近僵尸并计算避让力
        Vector3 avoidanceForce = CalculateZombieAvoidance();
        
        // 混合基础方向和避让方向
        if (avoidanceForce != Vector3.zero)
        {
            // 基础方向权重75%，避让方向权重25%
            Vector3 finalDirection = (baseDirection * 0.75f + avoidanceForce * 0.25f).normalized;
            return finalDirection;
        }
        
        return baseDirection;
    }

    /// <summary>
    /// 计算僵尸避让力
    /// </summary>
    private Vector3 CalculateZombieAvoidance()
    {
        var nearbyZombies = FindObjectsOfType<Zombie>();
        Vector3 totalAvoidance = Vector3.zero;
        int avoidanceCount = 0;

        foreach (var otherZombie in nearbyZombies)
        {
            if (otherZombie == this || otherZombie.CurrentHealth <= 0) continue;

            float distance = Vector3.Distance(transform.position, otherZombie.transform.position);
            
            if (distance < _attackRange)
            {
                // 计算远离其他僵尸的方向
                Vector3 awayDirection = (transform.position - otherZombie.transform.position).normalized;
                float strength = (_attackRange - distance) / _attackRange;
                totalAvoidance += awayDirection * strength;
                avoidanceCount++;
            }
        }

        return avoidanceCount > 0 ? totalAvoidance.normalized : Vector3.zero;
    }

    /// <summary>
    /// 尝试攻击目标
    /// </summary>
    private void TryAttackTarget()
    {
        if (_currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, _currentTarget.position);
        
        if (distance <= _attackRange && CanAttack)
        {
            FaceTarget(_currentTarget.position);
            
            // 根据目标类型执行攻击
            var damageable = _currentTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                PerformAttack(damageable);
            }
        }
    }

    /// <summary>
    /// 被攻击时立即锁定攻击者
    /// </summary>
    public override float TakeDamage(DamageInfo damageInfo)
    {
        float actualDamage = base.TakeDamage(damageInfo);
        
        if (CurrentHealth > 0 && damageInfo.Source is Player player)
        {
            // 被玩家攻击后立即锁定玩家
            _currentTarget = player.transform;
        }
        
        return actualDamage;
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    protected override void OnDeath()
    {
        base.OnDeath();
        Debug.Log("[Zombie] 僵尸死亡");
    }

    // 调试用Gizmos
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || ConfigId == 0) return;
        
        // 当前目标连线
        if (_currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _currentTarget.position);
            Gizmos.DrawWireSphere(_currentTarget.position, 0.5f);
        }
        
        // 检测范围
        Gizmos.color = Color.yellow;
        DrawWireCircle(transform.position, _detectRange);
        
        // 攻击范围
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, _attackRange);
        
        // Plant检测锥形
        if (_currentTarget != null)
        {
            Gizmos.color = Color.green;
            Vector3 directionToTarget = (_currentTarget.position - transform.position).normalized;
            Vector3 leftBoundary = Quaternion.Euler(0, -_attackAngle * 0.5f, 0) * directionToTarget * _detectRange;
            Vector3 rightBoundary = Quaternion.Euler(0, _attackAngle * 0.5f, 0) * directionToTarget * _detectRange;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
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