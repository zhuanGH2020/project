using UnityEngine;

/// <summary>
/// 怪物角色
/// </summary>
public class Monster : CombatEntity
{
    [Header("AI Settings")]
    [SerializeField] private float _detectRange = 5f;    // 检测范围
    [SerializeField] private float _attackRange = 2f;    // 攻击范围
    [SerializeField] private float _moveSpeed = 3f;      // 移动速度

    private Transform _target;    // 当前目标
    private Player _player;       // 玩家引用

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
    /// 更新AI行为
    /// </summary>
    private void UpdateAI()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        // 在检测范围内
        if (distanceToPlayer <= _detectRange)
        {
            _target = _player.transform;

            // 在攻击范围内
            if (distanceToPlayer <= _attackRange)
            {
                // 可以攻击则攻击
                if (CanAttack)
                {
                    PerformAttack(_player);
                }
            }
            // 不在攻击范围则追击
            else
            {
                ChaseTarget();
            }
        }
        // 超出检测范围，失去目标
        else
        {
            _target = null;
        }
    }

    /// <summary>
    /// 追击目标
    /// </summary>
    private void ChaseTarget()
    {
        if (_target == null) return;

        // 简单的移动逻辑，实际项目中可能需要寻路系统
        Vector3 direction = (_target.position - transform.position).normalized;
        transform.position += direction * _moveSpeed * Time.deltaTime;
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        // 怪物死亡逻辑，比如播放动画、掉落物品等
        Destroy(gameObject);
    }
} 