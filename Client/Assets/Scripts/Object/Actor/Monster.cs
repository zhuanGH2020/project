using UnityEngine;

/// <summary>
/// 怪物AI基类 - 带状态机和智能感知系统
/// </summary>
public class Monster : CombatEntity
{
    protected float _detectRange;            // 检测范围 - 从配置表读取
    protected float _fieldOfView;            // 视野角度 - 从配置表读取
    protected float _lostTargetTime;         // 失去目标记忆时间 - 从配置表读取
    protected LayerMask _obstacleLayer = 1;  // 障碍物层

    protected float _idleSpeed;              // 空闲速度 - 从配置表读取
    protected float _chaseSpeed;             // 追击速度 - 从配置表读取
    protected float _rotationSpeed;          // 旋转速度 - 从配置表读取

    protected float _attackRange;            // 攻击范围 - 从配置表读取
    protected float _attackAngle;            // 攻击角度 - 从配置表读取

    protected float _patrolRadius;           // 巡逻半径 - 从配置表读取
    protected float _patrolWaitTime;         // 巡逻等待时间 - 从配置表读取

    private float _maxHealthValue = 100f;    // 最大生命值

    // 实现DamageableObject的抽象属性
    public override float MaxHealth => _maxHealthValue;
    public override bool CanInteract => CurrentHealth > 0;
    public override float GetInteractionRange() => 2f;

    protected override void Awake()
    {
        base.Awake();
        _obstacleLayer = LayerMask.GetMask("Obstacle");
        SetObjectType(ObjectType.Monster);
    }

    public void Init(int configId)
    {
        SetConfigId(configId);
        LoadConfigValues();
        
        _player = FindObjectOfType<Player>();
        _spawnPoint = transform.position;
        _lastPosition = transform.position;
        ChangeState(MonsterState.Idle);
    }

    private void LoadConfigValues()
    {
        var config = GetConfig();
        if (config == null)
        {
            return;
        }

        int configId = ConfigId;
        
        _detectRange = config.GetValue(configId, "DetectionRange", 5f);
        _attackRange = config.GetValue(configId, "AttackRange", 2f);
        _chaseSpeed = config.GetValue(configId, "MoveSpeed", 3.5f);
        _rotationSpeed = config.GetValue(configId, "RotationSpeed", 5f);
        _fieldOfView = config.GetValue(configId, "FieldOfView", 90f);
        _lostTargetTime = config.GetValue(configId, "LostTargetTime", 3f);
        _idleSpeed = config.GetValue(configId, "IdleSpeed", 1f);
        _attackAngle = config.GetValue(configId, "AttackAngle", 45f);
        _patrolRadius = config.GetValue(configId, "PatrolRadius", 8f);
        _patrolWaitTime = config.GetValue(configId, "PatrolWaitTime", 2f);
        _maxHealthValue = config.GetValue(configId, "MaxHealth", 100f);
        _dialogRange = config.GetValue(configId, "DialogRange", 3f);
        _availableDialogIds = config.GetValue(configId, "DialogIds", new int[] { 1, 2, 3, 4 });
        _moveSpeed = _idleSpeed;
    }

    public override float TakeDamage(DamageInfo damageInfo)
    {
        float actualDamage = base.TakeDamage(damageInfo);
        
        if (CurrentHealth > 0 && damageInfo.Source is Player)
        {
            OnAttackedByPlayer(damageInfo);
        }
        
        return actualDamage;
    }

    private void OnAttackedByPlayer(DamageInfo damageInfo)
    {
        _target = _player.transform;
        _lastKnownPlayerPos = _player.transform.position;
        _lostTargetTimer = 0f;
        
        switch (_currentState)
        {
            case MonsterState.Idle:
            case MonsterState.Patrol:
                ChangeState(MonsterState.Alert);
                break;
                
            case MonsterState.Alert:
                if (_lastDistanceCheck <= _attackRange)
                {
                    ChangeState(MonsterState.Attack);
                }
                else
                {
                    ChangeState(MonsterState.Chase);
                }
                break;
                
            case MonsterState.Chase:
            case MonsterState.Attack:
                break;
                
            case MonsterState.Stunned:
                break;
        }
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
    private Vector3 _lastKnownPlayerPos;    // 玩家最后已知位置
    private float _lostTargetTimer;         // 失去目标计时器
    private float _patrolTimer;             // 巡逻计时器
    private float _lastDistanceCheck;       // 距离缓存

    private Vector3 _lastPosition;          // 上一帧位置（用于卡住检测）
    private float _stuckTimer;              // 卡住计时器

    // 公共属性
    public MonsterState CurrentState => _currentState;

    protected override void Update()
    {
        base.Update();
        
        if (ConfigId > 0)
        {
            UpdateAI();
            UpdateDialog();
        }
    }

    private void UpdateAI()
    {
        if (_player == null) return;

        CheckStuckCondition();
        UpdatePerception();
        UpdateStateMachine();
        _lastPosition = transform.position;
    }

    private void UpdatePerception()
    {
        if (_player == null) return;

        _lastDistanceCheck = Vector3.Distance(transform.position, _player.transform.position);

        if (_currentState == MonsterState.Idle || _currentState == MonsterState.Patrol)
        {
            if (_target == null && CanSeePlayer())
            {
                _target = _player.transform;
                _lastKnownPlayerPos = _player.transform.position;
                _lostTargetTimer = 0f;
            }
        }
        else if (_currentState == MonsterState.Alert || _currentState == MonsterState.Chase || _currentState == MonsterState.Attack)
        {
            if (_target != null)
            {
                if (CanSeePlayer())
                {
                    _lastKnownPlayerPos = _player.transform.position;
                    _lostTargetTimer = 0f;
                }
                else
                {
                    _lostTargetTimer += Time.deltaTime;
                }

                if (_lastDistanceCheck > _detectRange * 1.5f || _lostTargetTimer >= _lostTargetTime)
                {
                    _target = null;
                    _lostTargetTimer = 0f;
                    ChangeState(MonsterState.Patrol);
                }
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (_player == null) return false;

        bool inRange = _lastDistanceCheck <= _detectRange;
        if (!inRange) return false;

        Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angleToPlayer > _fieldOfView * 0.5f) return false;

        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayStart, directionToPlayer, _lastDistanceCheck, _obstacleLayer))
        {
            return false;
        }

        return true;
    }

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

    private void UpdatePatrolState()
    {
        if (_target != null)
        {
            ChangeState(MonsterState.Alert);
            return;
        }

        Vector3 direction = (_patrolTarget - transform.position).normalized;
        MoveTo(direction, _idleSpeed);

        float distanceToTarget = Vector3.Distance(transform.position, _patrolTarget);
        if (distanceToTarget < 0.5f)
        {
            GeneratePatrolTarget();
        }
    }

    private void UpdateAlertState()
    {
        if (_target == null)
        {
            ChangeState(MonsterState.Patrol);
            return;
        }

        FaceTarget();

        if (_lastDistanceCheck <= _attackRange)
        {
            ChangeState(MonsterState.Attack);
        }
        else
        {
            ChangeState(MonsterState.Chase);
        }
    }

    private void UpdateChaseState()
    {
        if (_target == null)
        {
            ChangeState(MonsterState.Patrol);
            return;
        }

        if (_lastDistanceCheck <= _attackRange)
        {
            ChangeState(MonsterState.Attack);
            return;
        }

        Vector3 chaseTarget;
        if (CanSeePlayer())
        {
            chaseTarget = _target.position;
        }
        else
        {
            chaseTarget = _lastKnownPlayerPos;
            
            float distanceToLastKnown = Vector3.Distance(transform.position, _lastKnownPlayerPos);
            if (distanceToLastKnown < 1f)
            {
                ChangeState(MonsterState.Patrol);
                return;
            }
        }

        Vector3 direction = (chaseTarget - transform.position).normalized;
        MoveTo(direction, _chaseSpeed);
    }

    private void UpdateAttackState()
    {
        if (_target == null || _lastDistanceCheck > _attackRange)
        {
            ChangeState(MonsterState.Chase);
            return;
        }

        FaceTarget();
        
        if (CanAttack)
        {
            Vector3 directionToTarget = (_target.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            
            if (angleToTarget <= _attackAngle * 0.5f)
            {
                PerformAttack(_player);
            }
        }
    }

    private void UpdateStunnedState()
    {
        // 眩晕状态下不做任何行动，由外部控制状态切换
    }

    private void ChangeState(MonsterState newState)
    {
        if (_currentState == newState) return;

        OnExitState(_currentState);
        MonsterState oldState = _currentState;
        _currentState = newState;
        OnEnterState(newState);
    }

    private void OnEnterState(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Idle:
                _patrolTimer = 0f;
                break;
            case MonsterState.Patrol:
                GeneratePatrolTarget();
                break;
        }
    }

    private void OnExitState(MonsterState state)
    {
        // 状态退出时的清理工作
    }

    protected void MoveTo(Vector3 direction, float speed)
    {
        direction.y = 0;
        if (direction == Vector3.zero) return;

        _moveSpeed = speed;
        SetMoveSpeed(speed);

        Vector3 targetPosition = transform.position + direction.normalized * 2f;

        if (HasNavMeshAgent && base.MoveToPosition(targetPosition))
        {
            return;
        }

        MoveToDirectly(direction, speed);
    }

    protected void MoveToDirectly(Vector3 direction, float speed)
    {
        direction.y = 0;
        if (direction == Vector3.zero) return;

        Vector3 desiredMove = direction * speed * Time.deltaTime;
        Vector3 newPosition = transform.position + desiredMove;
        
        if (CanMoveTo(newPosition))
        {
            transform.position = newPosition;
        }
        else
        {
            Vector3 avoidanceDirection = GetAvoidanceDirection(direction);
            if (avoidanceDirection != Vector3.zero)
            {
                Vector3 avoidanceMove = avoidanceDirection * speed * 0.7f * Time.deltaTime;
                Vector3 avoidancePosition = transform.position + avoidanceMove;
                
                if (CanMoveTo(avoidancePosition))
                {
                    transform.position = avoidancePosition;
                }
            }
        }
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }

    protected bool MoveToTargetPosition(Vector3 targetPosition)
    {
        return base.MoveToPosition(targetPosition);
    }

    private bool CanMoveTo(Vector3 position)
    {
        float checkRadius = 0.8f;
        
        if (Physics.CheckSphere(position, checkRadius, _obstacleLayer))
        {
            return false;
        }
        
        Collider[] monsters = Physics.OverlapSphere(position, checkRadius);
        foreach (var collider in monsters)
        {
            if (collider.transform == transform) continue;
            
            var otherMonster = collider.GetComponent<Monster>();
            if (otherMonster != null)
            {
                return false;
            }
        }
        
        return true;
    }

    private Vector3 GetAvoidanceDirection(Vector3 originalDirection)
    {
        Vector3 rightAvoidance = Vector3.Cross(Vector3.up, originalDirection).normalized;
        Vector3 leftAvoidance = -rightAvoidance;
        
        Vector3[] avoidanceOptions = {
            (originalDirection + rightAvoidance).normalized,
            (originalDirection + leftAvoidance).normalized,
            rightAvoidance,
            leftAvoidance,
            -originalDirection * 0.3f
        };
        
        foreach (var option in avoidanceOptions)
        {
            Vector3 testPosition = transform.position + option * 0.5f;
            if (CanMoveTo(testPosition))
            {
                return option;
            }
        }
        
        return Vector3.zero;
    }

    protected void FaceTarget()
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

    protected void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
        }
    }

    protected void MoveToIgnoreObstacles(Vector3 direction, float speed)
    {
        direction.y = 0;
        if (direction == Vector3.zero) return;

        Vector3 desiredMove = direction * speed * Time.deltaTime;
        Vector3 newPosition = transform.position + desiredMove;
        
        if (CanMoveToIgnoreObstacles(newPosition))
        {
            transform.position = newPosition;
        }
        else
        {
            Vector3 avoidanceDirection = GetAvoidanceDirection(direction);
            if (avoidanceDirection != Vector3.zero)
            {
                Vector3 avoidanceMove = avoidanceDirection * speed * 0.8f * Time.deltaTime;
                Vector3 avoidancePosition = transform.position + avoidanceMove;
                
                if (CanMoveToIgnoreObstacles(avoidancePosition))
                {
                    transform.position = avoidancePosition;
                }
            }
        }
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }

    private bool CanMoveToIgnoreObstacles(Vector3 position)
    {
        float checkRadius = 0.8f;
        
        Collider[] monsters = Physics.OverlapSphere(position, checkRadius);
        foreach (var collider in monsters)
        {
            if (collider.transform == transform) continue;
            
            var otherMonster = collider.GetComponent<Monster>();
            if (otherMonster != null)
            {
                return false;
            }
        }
        
        return true;
    }

    private void CheckStuckCondition()
    {
        if (_currentState != MonsterState.Patrol && _currentState != MonsterState.Chase)
        {
            _stuckTimer = 0f;
            return;
        }

        float moveDistance = Vector3.Distance(transform.position, _lastPosition);
        
        if (moveDistance < 0.05f)
        {
            _stuckTimer += Time.deltaTime;
        }
        else
        {
            _stuckTimer = 0f;
        }

        if (_stuckTimer >= 2f)
        {
            HandleStuckSituation();
            _stuckTimer = 0f;
        }
    }

    private void HandleStuckSituation()
    {
        if (_currentState == MonsterState.Patrol)
        {
            GeneratePatrolTarget();
        }
        else if (_currentState == MonsterState.Chase)
        {
            ChangeState(MonsterState.Patrol);
        }
    }

    private void GeneratePatrolTarget()
    {
        int maxAttempts = 12;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * _patrolRadius;
            randomDirection.y = 0;
            Vector3 candidateTarget = _spawnPoint + randomDirection;
            
            Vector3 directionToTarget = (candidateTarget - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, candidateTarget);
            
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToTarget, distanceToTarget, _obstacleLayer))
            {
                _patrolTarget = candidateTarget;
                return;
            }
        }
        
        Vector3[] safeDirections = {
            transform.right * 3f,
            -transform.right * 3f,
            transform.forward * 3f,
            -transform.forward * 3f,
            (transform.right + transform.forward).normalized * 3f,
            (-transform.right + transform.forward).normalized * 3f,
            (transform.right - transform.forward).normalized * 3f,
            (-transform.right - transform.forward).normalized * 3f
        };
        
        foreach (Vector3 direction in safeDirections)
        {
            Vector3 candidateTarget = transform.position + direction;
            Vector3 directionToTarget = direction.normalized;
            
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToTarget, direction.magnitude, _obstacleLayer))
            {
                _patrolTarget = candidateTarget;
                return;
            }
        }
        
        _patrolTarget = transform.position + transform.right * 1f;
    }

    public void Stun(float duration)
    {
        ChangeState(MonsterState.Stunned);
        Invoke(nameof(RecoverFromStun), duration);
    }

    private void RecoverFromStun()
    {
        if (_currentState == MonsterState.Stunned)
        {
            ChangeState(_target != null ? MonsterState.Alert : MonsterState.Idle);
        }
    }

    private void UpdateDialog()
    {
        if (_player == null || DialogManager.Instance == null) return;

        if (!CanTriggerDialog()) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        if (distanceToPlayer <= _dialogRange)
        {
            TriggerRandomDialog();
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        ChangeState(MonsterState.Dead);
        _target = null;

        ClearDialog();

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

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || ConfigId == 0) return;
        
        Gizmos.color = Color.yellow;
        DrawWireCircle(transform.position, _detectRange);
        
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, _attackRange);
        
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -_fieldOfView * 0.5f, 0) * transform.forward * _detectRange;
        Vector3 rightBoundary = Quaternion.Euler(0, _fieldOfView * 0.5f, 0) * transform.forward * _detectRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        Gizmos.color = Color.green;
        DrawWireCircle(_spawnPoint, _patrolRadius);
        
        if (_currentState == MonsterState.Patrol && _patrolTarget != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_patrolTarget, 0.5f);
            Gizmos.DrawLine(transform.position, _patrolTarget);
        }

        if (_lastKnownPlayerPos != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_lastKnownPlayerPos, 0.3f);
            Gizmos.DrawLine(transform.position, _lastKnownPlayerPos);
        }

        if (_stuckTimer > 1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
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