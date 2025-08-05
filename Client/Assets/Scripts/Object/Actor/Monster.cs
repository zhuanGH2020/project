using UnityEngine;

/// <summary>
/// 怪物角色 - 直线追击AI
/// </summary>
public class Monster : CombatEntity
{
    [Header("AI设置")]
    [SerializeField] private float _detectRange = 5f;    // 检测范围
    [SerializeField] private float _attackRange = 2f;    // 攻击范围
    [SerializeField] private float _moveSpeed = 3.5f;    // 移动速度
    [SerializeField] private float _rotationSpeed = 5f;  // 旋转速度
    
    [Header("对话设置")]
    [SerializeField] private float _dialogRange = 3f;    // 对话范围
    [SerializeField] private int[] _availableDialogIds = { 1, 2, 3, 4 }; // 可用对话ID列表

    private Transform _target;          // 当前目标
    private Player _player;             // 玩家引用
    private float _lastDistanceCheck;   // 距离计算缓存
    
    // 对话相关变量
    private float _lastDialogTime;      // 上次对话时间
    private float _dialogCooldown = 5f; // 对话冷却时间（秒）
    private int _currentDialogId = -1;  // 当前对话框ID

    protected override void Awake()
    {
        base.Awake();
        _player = FindObjectOfType<Player>();
    }

    protected override void Update()
    {
        base.Update();
        UpdateAI();
        UpdateDialog();
    }

    /// <summary>
    /// 更新AI行为，优化距离计算
    /// </summary>
    private void UpdateAI()
    {
        if (_player == null) return;

        // 缓存距离计算，避免重复调用Vector3.Distance
        _lastDistanceCheck = Vector3.Distance(transform.position, _player.transform.position);

        // 在检测范围内
        if (_lastDistanceCheck <= _detectRange)
        {
            _target = _player.transform;

            // 在攻击范围内
            if (_lastDistanceCheck <= _attackRange)
            {
                HandleAttackBehavior();
            }
            // 不在攻击范围内，追击目标
            else
            {
                HandleChaseBehavior();
            }
        }
        // 超出检测范围，失去目标
        else
        {
            HandleIdleBehavior();
        }
    }

    /// <summary>
    /// 处理在攻击范围内的攻击行为
    /// </summary>
    private void HandleAttackBehavior()
    {
        // 如果可以攻击则攻击
        if (CanAttack)
        {
            // 攻击前面向目标
            FaceTarget();
            PerformAttack(_player);
        }
    }

    /// <summary>
    /// 处理检测到目标但不在攻击范围内的追击行为
    /// </summary>
    private void HandleChaseBehavior()
    {
        ChaseTarget();
    }

    /// <summary>
    /// 处理未检测到目标时的空闲行为
    /// </summary>
    private void HandleIdleBehavior()
    {
        _target = null;
    }

    /// <summary>
    /// 直线追击目标
    /// </summary>
    private void ChaseTarget()
    {
        if (_target == null) return;

        Vector3 direction = (_target.position - transform.position).normalized;
        direction.y = 0; // 保持在水平面上

        // 移动到目标位置
        transform.position += direction * _moveSpeed * Time.deltaTime;
        
        // 旋转面向目标
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
        }
    }

    /// <summary>
    /// 面向目标准备攻击
    /// </summary>
    private void FaceTarget()
    {
        if (_target == null) return;

        Vector3 direction = (_target.position - transform.position).normalized;
        direction.y = 0; // 保持在水平面上
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
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
        
        // 停止所有移动
        _target = null;
        
        // 清理对话框
        if (_currentDialogId != -1 && DialogManager.Instance != null)
        {
            DialogManager.Instance.DestroyDialog(_currentDialogId);
            _currentDialogId = -1;
        }
        
        Debug.Log($"[Monster] {gameObject.name} died");
        
        // 怪物死亡逻辑 - 动画、掉落物品等
        Destroy(gameObject, 1f); // 延迟销毁以播放死亡动画
    }

    #region 数值验证
    protected override void OnValidate()
    {
        base.OnValidate();

        // 确保检测范围大于攻击范围
        if (_attackRange > _detectRange)
        {
            _attackRange = _detectRange;
        }
        
        // 确保对话范围合理
        if (_dialogRange <= 0)
        {
            _dialogRange = 3f;
        }
    }
    #endregion
} 