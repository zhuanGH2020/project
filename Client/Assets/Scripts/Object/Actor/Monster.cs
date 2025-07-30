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

    private Transform _target;          // 当前目标
    private Player _player;             // 玩家引用
    private float _lastDistanceCheck;   // 距离计算缓存

    protected override void Awake()
    {
        base.Awake();
        _player = FindObjectOfType<Player>();
    }

    protected override void Update()
    {
        base.Update();
        UpdateAI();
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
    /// 处理怪物死亡
    /// </summary>
    protected override void OnDeath()
    {
        base.OnDeath();
        
        // 停止所有移动
        _target = null;
        
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
    }
    #endregion
} 