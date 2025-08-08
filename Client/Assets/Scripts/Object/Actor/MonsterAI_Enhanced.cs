using UnityEngine;

/// <summary>
/// 增强版怪物AI - 带状态机和优化功能
/// </summary>
public class MonsterAI_Enhanced : CombatEntity
{
    [Header("感知设置")]
    [SerializeField] private float _detectRange = 5f;        // 检测范围
    [SerializeField] private float _fieldOfView = 90f;       // 视野角度
    [SerializeField] private float _lostTargetTime = 3f;     // 失去目标记忆时间
    [SerializeField] private LayerMask _obstacleLayer = 1;   // 障碍物层

    [Header("移动设置")]
    [SerializeField] private float _idleSpeed = 1f;          // 空闲速度
    [SerializeField] private float _chaseSpeed = 3.5f;       // 追击速度
    [SerializeField] private float _rotationSpeed = 5f;      // 旋转速度

    [Header("战斗设置")]
    [SerializeField] private float _attackRange = 2f;        // 攻击范围
    [SerializeField] private float _attackAngle = 45f;       // 攻击角度

    [Header("AI设置")]
    [SerializeField] private float _patrolRadius = 8f;       // 巡逻半径
    [SerializeField] private float _patrolWaitTime = 2f;     // 巡逻等待时间

    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Monster);
        _player = FindObjectOfType<Player>();
        _spawnPoint = transform.position;
        _currentSpeed = _idleSpeed;
        ChangeState(MonsterState.Idle);
    }

    // 状态枚举
    public enum MonsterState
    {
        Idle,           // 空闲
        Patrol,         // 巡逻
        Alert,          // 警戒
        Chase,          // 追击
        Attack,         // 攻击
        Stunned,        // 眩晕
        Dead            // 死亡
    }

    // 私有变量
    private MonsterState _currentState = MonsterState.Idle;
    private Transform _target;
    private Player _player;
    private Vector3 _spawnPoint;            // 出生点
    private Vector3 _patrolTarget;          // 巡逻目标点
    private float _lostTargetTimer;         // 失去目标计时器
    private float _patrolTimer;             // 巡逻计时器
    private float _lastDistanceCheck;       // 距离缓存
    private float _currentSpeed;            // 当前移动速度

    // 公共属性
    public MonsterState CurrentState => _currentState;

    protected override void Update()
    {
        base.Update();
        UpdateAI();
    }

    /// <summary>
    /// AI主更新循环
    /// </summary>
    private void UpdateAI()
    {
        if (_player == null) return;

        // 更新感知
        UpdatePerception();
        
        // 状态机更新
        UpdateStateMachine();
    }

    /// <summary>
    /// 更新感知系统
    /// </summary>
    private void UpdatePerception()
    {
        if (_player == null) return;

        _lastDistanceCheck = Vector3.Distance(transform.position, _player.transform.position);

        // 检查是否在检测范围内
        bool inRange = _lastDistanceCheck <= _detectRange;
        bool inSight = false;

        if (inRange)
        {
            // 检查视野角度
            Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angleToPlayer <= _fieldOfView * 0.5f)
            {
                // 检查视线遮挡
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, _lastDistanceCheck, _obstacleLayer))
                {
                    inSight = true;
                }
            }
        }

        // 更新目标
        if (inSight)
        {
            _target = _player.transform;
            _lostTargetTimer = 0f;
        }
        else if (_target != null)
        {
            _lostTargetTimer += Time.deltaTime;
            if (_lostTargetTimer >= _lostTargetTime)
            {
                _target = null;
            }
        }
    }

    /// <summary>
    /// 状态机更新
    /// </summary>
    private void UpdateStateMachine()
    {
        switch (_currentState)
        {
            case MonsterState.Idle:
                UpdateIdleState();
                break;
            case MonsterState.Patrol:
                UpdatePatrolState();
                break;
            case MonsterState.Alert:
                UpdateAlertState();
                break;
            case MonsterState.Chase:
                UpdateChaseState();
                break;
            case MonsterState.Attack:
                UpdateAttackState();
                break;
            case MonsterState.Stunned:
                UpdateStunnedState();
                break;
        }
    }

    /// <summary>
    /// 空闲状态
    /// </summary>
    private void UpdateIdleState()
    {
        if (_target != null)
        {
            ChangeState(MonsterState.Alert);
            return;
        }

        _patrolTimer += Time.deltaTime;
        if (_patrolTimer >= _patrolWaitTime)
        {
            ChangeState(MonsterState.Patrol);
        }
    }

    /// <summary>
    /// 巡逻状态
    /// </summary>
    private void UpdatePatrolState()
    {
        if (_target != null)
        {
            ChangeState(MonsterState.Alert);
            return;
        }

        // 移动到巡逻点
        Vector3 direction = (_patrolTarget - transform.position).normalized;
        MoveTo(direction, _idleSpeed);

        // 到达巡逻点
        if (Vector3.Distance(transform.position, _patrolTarget) < 0.5f)
        {
            ChangeState(MonsterState.Idle);
        }
    }

    /// <summary>
    /// 警戒状态
    /// </summary>
    private void UpdateAlertState()
    {
        if (_target == null)
        {
            ChangeState(MonsterState.Idle);
            return;
        }

        // 面向目标
        FaceTarget();

        // 进入追击或攻击范围
        if (_lastDistanceCheck <= _attackRange)
        {
            ChangeState(MonsterState.Attack);
        }
        else
        {
            ChangeState(MonsterState.Chase);
        }
    }

    /// <summary>
    /// 追击状态
    /// </summary>
    private void UpdateChaseState()
    {
        if (_target == null)
        {
            ChangeState(MonsterState.Alert);
            return;
        }

        if (_lastDistanceCheck <= _attackRange)
        {
            ChangeState(MonsterState.Attack);
            return;
        }

        // 追击目标
        Vector3 direction = (_target.position - transform.position).normalized;
        MoveTo(direction, _chaseSpeed);
    }

    /// <summary>
    /// 攻击状态
    /// </summary>
    private void UpdateAttackState()
    {
        if (_target == null || _lastDistanceCheck > _attackRange)
        {
            ChangeState(MonsterState.Chase);
            return;
        }

        // 面向目标并攻击
        FaceTarget();
        
        if (CanAttack)
        {
            // 检查攻击角度
            Vector3 directionToTarget = (_target.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            
            if (angleToTarget <= _attackAngle * 0.5f)
            {
                PerformAttack(_player);
            }
        }
    }

    /// <summary>
    /// 眩晕状态
    /// </summary>
    private void UpdateStunnedState()
    {
        // 眩晕状态下不做任何行动，由外部控制状态切换
    }

    /// <summary>
    /// 改变状态
    /// </summary>
    private void ChangeState(MonsterState newState)
    {
        if (_currentState == newState) return;

        // 退出当前状态
        OnExitState(_currentState);
        
        // 切换状态
        MonsterState oldState = _currentState;
        _currentState = newState;
        
        // 进入新状态
        OnEnterState(newState);
        
        Debug.Log($"[Monster] State changed: {oldState} -> {newState}");
    }

    /// <summary>
    /// 进入状态处理
    /// </summary>
    private void OnEnterState(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Idle:
                _patrolTimer = 0f;
                _currentSpeed = _idleSpeed;
                break;
            case MonsterState.Patrol:
                GeneratePatrolTarget();
                break;
            case MonsterState.Chase:
                _currentSpeed = _chaseSpeed;
                break;
        }
    }

    /// <summary>
    /// 退出状态处理
    /// </summary>
    private void OnExitState(MonsterState state)
    {
        // 状态退出时的清理工作
    }

    /// <summary>
    /// 移动到指定方向
    /// </summary>
    private void MoveTo(Vector3 direction, float speed)
    {
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            // 移动
            transform.position += direction * speed * Time.deltaTime;
            
            // 旋转
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
        }
    }

    /// <summary>
    /// 面向目标
    /// </summary>
    private void FaceTarget()
    {
        if (_target == null) return;

        Vector3 direction = (_target.position - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
        }
    }

    /// <summary>
    /// 生成巡逻目标点
    /// </summary>
    private void GeneratePatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * _patrolRadius;
        randomDirection.y = 0;
        _patrolTarget = _spawnPoint + randomDirection;
    }

    /// <summary>
    /// 外部眩晕接口
    /// </summary>
    public void Stun(float duration)
    {
        ChangeState(MonsterState.Stunned);
        Invoke(nameof(RecoverFromStun), duration);
    }

    /// <summary>
    /// 从眩晕中恢复
    /// </summary>
    private void RecoverFromStun()
    {
        if (_currentState == MonsterState.Stunned)
        {
            ChangeState(_target != null ? MonsterState.Alert : MonsterState.Idle);
        }
    }

    /// <summary>
    /// 处理怪物死亡
    /// </summary>
    protected override void OnDeath()
    {
        base.OnDeath();
        ChangeState(MonsterState.Dead);
        _target = null;
        Debug.Log($"[Monster] {gameObject.name} died");
        Destroy(gameObject, 1f);
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        
        if (_attackRange > _detectRange)
        {
            _attackRange = _detectRange;
        }
    }

    // 调试用的Gizmos
    private void OnDrawGizmosSelected()
    {
        // 检测范围
        Gizmos.color = Color.yellow;
        DrawWireCircle(transform.position, _detectRange);
        
        // 攻击范围
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, _attackRange);
        
        // 视野角度
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -_fieldOfView * 0.5f, 0) * transform.forward * _detectRange;
        Vector3 rightBoundary = Quaternion.Euler(0, _fieldOfView * 0.5f, 0) * transform.forward * _detectRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        // 巡逻范围
        Gizmos.color = Color.green;
        DrawWireCircle(_spawnPoint, _patrolRadius);
    }

    /// <summary>
    /// 绘制线框圆形（Unity Gizmos没有DrawWireCircle，自己实现）
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