using UnityEngine;

/// <summary>
/// 怪物AI - 带状态机和智能感知系统
/// </summary>
public class Monster : CombatEntity
{
    private float _detectRange;            // 检测范围 - 从配置表读取
    private float _fieldOfView;            // 视野角度 - 从配置表读取
    private float _lostTargetTime;         // 失去目标记忆时间 - 从配置表读取
    private LayerMask _obstacleLayer = 1;  // 障碍物层

    private float _idleSpeed;              // 空闲速度 - 从配置表读取
    private float _chaseSpeed;             // 追击速度 - 从配置表读取
    private float _rotationSpeed;          // 旋转速度 - 从配置表读取

    private float _attackRange;            // 攻击范围 - 从配置表读取
    private float _attackAngle;            // 攻击角度 - 从配置表读取

    private float _patrolRadius;           // 巡逻半径 - 从配置表读取
    private float _patrolWaitTime;         // 巡逻等待时间 - 从配置表读取

    [Header("对话设置")]
    [SerializeField] private float _dialogRange = 3f;    // 对话范围
    [SerializeField] private int[] _availableDialogIds = { 1, 2, 3, 4 }; // 可用对话ID列表

    // 对话相关变量
    private float _lastDialogTime;      // 上次对话时间
    private float _dialogCooldown = 5f; // 对话冷却时间（秒）
    private int _currentDialogId = -1;  // 当前对话框ID

    protected override void Awake()
    {
        base.Awake();
        _obstacleLayer = LayerMask.GetMask("Obstacle");
        SetObjectType(ObjectType.Monster);
        
        Init(5001);
    }

    /// <summary>
    /// 初始化怪物 - 设置配置ID并加载配置参数
    /// </summary>
    /// <param name="configId">怪物配置ID</param>
    public void Init(int configId)
    {
        SetConfigId(configId);
        LoadConfigValues();
        
        _player = FindObjectOfType<Player>();
        _spawnPoint = transform.position;
        _lastPosition = transform.position;
        _currentSpeed = _idleSpeed;
        ChangeState(MonsterState.Idle);
    }

    /// <summary>
    /// 从配置表加载数值参数
    /// </summary>
    private void LoadConfigValues()
    {
        var config = GetConfig();
        if (config == null)
        {
            Debug.LogError("无法获取Monster配置表");
            return;
        }

        int configId = ConfigId;
        
        // 从Monster.csv配置表读取基础战斗参数
        _detectRange = config.GetValue(configId, "DetectionRange", 5f);
        _attackRange = config.GetValue(configId, "AttackRange", 2f);
        _chaseSpeed = config.GetValue(configId, "MoveSpeed", 3.5f);
        _rotationSpeed = config.GetValue(configId, "RotationSpeed", 5f);
        
        // 读取感知参数
        _fieldOfView = config.GetValue(configId, "FieldOfView", 90f);
        _lostTargetTime = config.GetValue(configId, "LostTargetTime", 3f);
        
        // 读取移动参数
        _idleSpeed = config.GetValue(configId, "IdleSpeed", 1f);
        
        // 读取战斗参数
        _attackAngle = config.GetValue(configId, "AttackAngle", 45f);
        
        // 读取AI巡逻参数
        _patrolRadius = config.GetValue(configId, "PatrolRadius", 8f);
        _patrolWaitTime = config.GetValue(configId, "PatrolWaitTime", 2f);
        
        // 设置基础属性
        _maxHealth = config.GetValue(configId, "MaxHealth", 100f);
        _currentHealth = _maxHealth;
    }

    /// <summary>
    /// 重写承受伤害方法 - 被攻击时立即锁定攻击者
    /// </summary>
    public override float TakeDamage(DamageInfo damageInfo)
    {
        // 调用基类方法处理伤害
        float actualDamage = base.TakeDamage(damageInfo);
        
        // 如果还活着且攻击者是玩家，立即锁定
        if (_currentHealth > 0 && damageInfo.Source is Player)
        {
            OnAttackedByPlayer(damageInfo);
        }
        
        return actualDamage;
    }

    /// <summary>
    /// 被玩家攻击时的处理
    /// </summary>
    private void OnAttackedByPlayer(DamageInfo damageInfo)
    {
        // 立即锁定玩家
        _target = _player.transform;
        _lastKnownPlayerPos = _player.transform.position;
        _lostTargetTimer = 0f;
        
        // 根据当前状态进行状态切换
        switch (_currentState)
        {
            case MonsterState.Idle:
            case MonsterState.Patrol:
                // 空闲或巡逻状态：被偷袭，立即进入警戒
                ChangeState(MonsterState.Alert);
                break;
                
            case MonsterState.Alert:
                // 警戒状态：已经发现玩家，被攻击后更加警觉
                // 如果在攻击范围内，立即反击
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
                // 追击或攻击状态：已经在战斗中，更新目标信息即可
                // 被攻击会重置失去目标计时器，让怪物更难甩掉
                break;
                
            case MonsterState.Stunned:
                // 眩晕状态：记录攻击者，但不能立即反应
                // 眩晕结束后会优先攻击这个目标
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
    private float _currentSpeed;            // 当前移动速度
    private Vector3 _lastPosition;          // 上一帧位置（用于卡住检测）
    private float _stuckTimer;              // 卡住计时器
    private float _lastStateChangeTime;     // 上次状态改变的时间（防止频繁切换）


    // 公共属性
    public MonsterState CurrentState => _currentState;

    protected override void Update()
    {
        base.Update();
        
        // 只有在初始化完成后才运行AI
        if (ConfigId > 0)
        {
            UpdateAI();
            UpdateDialog();
        }
    }

    /// <summary>
    /// AI主更新循环
    /// </summary>
    private void UpdateAI()
    {
        if (_player == null) return;

        // 检查是否卡住（所有移动状态）
        CheckStuckCondition();
        
        // 更新感知
        UpdatePerception();
        
        // 状态机更新
        UpdateStateMachine();
        
        // 更新位置记录
        _lastPosition = transform.position;
    }

    /// <summary>
    /// 更新感知系统
    /// </summary>
    private void UpdatePerception()
    {
        if (_player == null) return;

        _lastDistanceCheck = Vector3.Distance(transform.position, _player.transform.position);

        if (_currentState == MonsterState.Idle || _currentState == MonsterState.Patrol)
        {
            // 空闲和巡逻状态：只能通过严格的视线检查发现目标
            if (_target == null && CanSeePlayer())
            {
                _target = _player.transform;
                _lastKnownPlayerPos = _player.transform.position;
                _lostTargetTimer = 0f;
            }
        }
        else if (_currentState == MonsterState.Alert || _currentState == MonsterState.Chase || _currentState == MonsterState.Attack)
        {
            // 警戒、追击、攻击状态：使用记忆能力，不会因为暂时遮挡立即失去目标
            if (_target != null)
            {
                // 更新最后已知位置（如果能看到玩家）
                if (CanSeePlayer())
                {
                    _lastKnownPlayerPos = _player.transform.position;
                    _lostTargetTimer = 0f;
                }
                else
                {
                    // 看不到玩家，开始计时
                    _lostTargetTimer += Time.deltaTime;
                }

                // 距离太远或失去目标时间太长，放弃目标
                if (_lastDistanceCheck > _detectRange * 1.5f || _lostTargetTimer >= _lostTargetTime)
                {
                    _target = null;
                    _lostTargetTimer = 0f;
                    // 丢失目标后开始巡逻寻找
                    ChangeState(MonsterState.Patrol);
                }
            }
        }
    }

    /// <summary>
    /// 检查是否能看到玩家（严格的视线检查）
    /// </summary>
    /// <returns>是否能看到玩家</returns>
    private bool CanSeePlayer()
    {
        if (_player == null) return false;

        // 检查是否在检测范围内
        bool inRange = _lastDistanceCheck <= _detectRange;
        if (!inRange) return false;

        // 检查视野角度
        Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angleToPlayer > _fieldOfView * 0.5f) return false;

        // 检查视线遮挡
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayStart, directionToPlayer, _lastDistanceCheck, _obstacleLayer))
        {
            return false;
        }

        return true;
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

        // 到达巡逻点，生成新的巡逻目标继续巡逻
        float distanceToTarget = Vector3.Distance(transform.position, _patrolTarget);
        if (distanceToTarget < 0.5f)
        {
            GeneratePatrolTarget();
        }
    }

    /// <summary>
    /// 警戒状态
    /// </summary>
    private void UpdateAlertState()
    {
        if (_target == null)
        {
            ChangeState(MonsterState.Patrol);
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
            ChangeState(MonsterState.Patrol);
            return;
        }

        if (_lastDistanceCheck <= _attackRange)
        {
            ChangeState(MonsterState.Attack);
            return;
        }

        // 决定追击目标：如果能看到玩家就追玩家，否则追最后已知位置
        Vector3 chaseTarget;
        if (CanSeePlayer())
        {
            chaseTarget = _target.position;
        }
        else
        {
            chaseTarget = _lastKnownPlayerPos;
            
            // 如果已经到达最后已知位置，开始巡逻寻找玩家
            float distanceToLastKnown = Vector3.Distance(transform.position, _lastKnownPlayerPos);
            if (distanceToLastKnown < 1f)
            {
                ChangeState(MonsterState.Patrol);
                return;
            }
        }

        // 追击目标位置
        Vector3 direction = (chaseTarget - transform.position).normalized;
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
    /// 检查卡住状态
    /// </summary>
    private void CheckStuckCondition()
    {
        // 只在移动状态下检查卡住
        if (_currentState != MonsterState.Patrol && _currentState != MonsterState.Chase)
        {
            _stuckTimer = 0f;
            return;
        }

        // 检查移动距离
        float moveDistance = Vector3.Distance(transform.position, _lastPosition);
        
        if (moveDistance < 0.05f) // 移动距离极小
        {
            _stuckTimer += Time.deltaTime;
        }
        else
        {
            _stuckTimer = 0f;
        }

        // 如果卡住超过2秒，采取解卡行动
        if (_stuckTimer >= 2f)
        {
            HandleStuckSituation();
            _stuckTimer = 0f;
        }
    }

    /// <summary>
    /// 处理卡住情况
    /// </summary>
    private void HandleStuckSituation()
    {
        if (_currentState == MonsterState.Patrol)
        {
            // 巡逻状态：重新生成巡逻目标
            GeneratePatrolTarget();
        }
        else if (_currentState == MonsterState.Chase)
        {
            // 追击状态：切换到巡逻状态重新寻路
            ChangeState(MonsterState.Patrol);
        }
    }



    /// <summary>
    /// 生成巡逻目标点 - 智能避障版本
    /// </summary>
    private void GeneratePatrolTarget()
    {
        int maxAttempts = 12; // 增加尝试次数
        
        for (int i = 0; i < maxAttempts; i++)
        {
            // 生成随机方向的巡逻点
            Vector3 randomDirection = Random.insideUnitSphere * _patrolRadius;
            randomDirection.y = 0;
            Vector3 candidateTarget = _spawnPoint + randomDirection;
            
            // 检查路径是否被障碍物阻挡
            Vector3 directionToTarget = (candidateTarget - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, candidateTarget);
            
            // 从当前位置向候选目标点发射射线检查障碍物
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToTarget, distanceToTarget, _obstacleLayer))
            {
                _patrolTarget = candidateTarget;
                return;
            }
        }
        
        // 如果随机生成失败，尝试使用预定义的安全方向
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
        
        // 最后的备用方案：就地转圈
        _patrolTarget = transform.position + transform.right * 1f;
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
    /// 更新对话逻辑
    /// </summary>
    private void UpdateDialog()
    {
        if (_player == null || DialogManager.Instance == null) return;

        // 检查对话冷却时间
        if (Time.time - _lastDialogTime < _dialogCooldown) return;

        // 检查玩家是否在对话范围内
        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        if (distanceToPlayer <= _dialogRange)
        {
            // 玩家在对话范围内，创建随机对话
            TriggerRandomDialog();
        }
    }

    /// <summary>
    /// 触发随机对话
    /// </summary>
    private void TriggerRandomDialog()
    {
        if (_availableDialogIds == null || _availableDialogIds.Length == 0)
        {
            Debug.LogWarning($"[Monster] {gameObject.name} 没有可用的对话ID");
            return;
        }

        // 销毁当前对话框（如果存在）
        if (_currentDialogId != -1)
        {
            DialogManager.Instance.DestroyDialog(_currentDialogId);
            _currentDialogId = -1;
        }

        // 创建新的随机对话
        Vector3 dialogOffset = new Vector3(0, 2.5f, 0); // 在怪物头顶显示
        _currentDialogId = DialogManager.Instance.CreateRandomDialog(
            transform,
            _availableDialogIds,
            dialogOffset,
            3f // 3秒持续时间
        );

        if (_currentDialogId != -1)
        {
            _lastDialogTime = Time.time;
            Debug.Log($"[Monster] {gameObject.name} 说话了，对话ID: {_currentDialogId}");
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

        // 清理对话框
        if (_currentDialogId != -1 && DialogManager.Instance != null)
        {
            DialogManager.Instance.DestroyDialog(_currentDialogId);
            _currentDialogId = -1;
        }

        // 怪物死亡处理
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
        // 只有在运行时且配置已加载时才显示Gizmos
        if (!Application.isPlaying || ConfigId == 0) return;
        
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
        
        // 绘制巡逻目标点
        if (_currentState == MonsterState.Patrol && _patrolTarget != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_patrolTarget, 0.5f);
            Gizmos.DrawLine(transform.position, _patrolTarget);
        }

        // 绘制最后已知玩家位置
        if (_lastKnownPlayerPos != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_lastKnownPlayerPos, 0.3f);
            Gizmos.DrawLine(transform.position, _lastKnownPlayerPos);
        }

        // 显示卡住状态
        if (_stuckTimer > 1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
        }
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