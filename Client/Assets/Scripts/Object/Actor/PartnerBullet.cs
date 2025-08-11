using UnityEngine;

/// <summary>
/// 伙伴子弹 - Partner攻击时发射的投射物
/// 移动一定距离后自动消失，碰到敌人时造成伤害并消失
/// 统一检测所有CombatEntity类型的敌人（Monster、MonsterAI_Enhanced等）
/// </summary>
public class PartnerBullet : ObjectBase
{
    [Header("子弹设置")]
    [SerializeField] private float _speed = 15f;              // 子弹飞行速度
    [SerializeField] private float _maxDistance = 10f;        // 最大飞行距离
    [SerializeField] private float _damage = 25f;             // 子弹伤害
    [SerializeField] private LayerMask _enemyLayer = -1;      // 敌人层级
    
    private Vector3 _direction;                               // 飞行方向
    private Vector3 _startPosition;                           // 起始位置
    private float _traveledDistance;                          // 已飞行距离
    private IAttacker _source;                                // 伤害来源
    private bool _isDestroyed;                                // 是否已销毁标记

    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Other);
    }

    /// <summary>
    /// 初始化子弹
    /// </summary>
    /// <param name="startPos">起始位置</param>
    /// <param name="direction">飞行方向</param>
    /// <param name="damage">伤害值</param>
    /// <param name="source">伤害来源</param>
    /// <param name="speed">飞行速度</param>
    /// <param name="maxDistance">最大距离</param>
    /// <param name="bulletConfigId">子弹配置ID（可选，用于从配置表读取额外参数）</param>
    public void Initialize(Vector3 startPos, Vector3 direction, float damage, IAttacker source, 
                          float speed = 15f, float maxDistance = 10f, int bulletConfigId = 0)
    {
        _startPosition = startPos;
        _direction = direction.normalized;
        _damage = damage;
        _source = source;
        _speed = speed;
        _maxDistance = maxDistance;
        _traveledDistance = 0f;
        _isDestroyed = false;
        
        // 如果提供了子弹配置ID，尝试从配置表读取额外参数
        if (bulletConfigId > 0)
        {
            LoadBulletConfigValues(bulletConfigId);
        }
        
        transform.position = startPos;
        
        // 面向飞行方向
        if (_direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(_direction);
        }
        
        Debug.Log($"[PartnerBullet] 初始化子弹 - 伤害:{_damage}, 速度:{_speed}, 最大距离:{_maxDistance}");
    }
    
    /// <summary>
    /// 从配置表加载子弹参数（可选功能）
    /// </summary>
    private void LoadBulletConfigValues(int bulletConfigId)
    {
        var config = ConfigManager.Instance.GetReader("Bullet");
        if (config == null || !config.HasKey(bulletConfigId))
        {
            Debug.LogWarning($"[PartnerBullet] 子弹配置ID {bulletConfigId} 不存在，使用默认参数");
            return;
        }
        
        // 可以从Bullet.csv配置表读取子弹特殊效果参数
        // 例如：爆炸范围、特效路径、音效等
        // 这里暂时保留接口，后续可扩展
        Debug.Log($"[PartnerBullet] 从配置表加载子弹参数: {bulletConfigId}");
    }

    private void Update()
    {
        if (_isDestroyed) return;
        
        // 移动子弹
        MoveBullet();
        
        // 检查碰撞
        CheckCollision();
        
        // 检查是否超出最大距离
        CheckMaxDistance();
    }

    /// <summary>
    /// 移动子弹
    /// </summary>
    private void MoveBullet()
    {
        float moveDistance = _speed * Time.deltaTime;
        transform.position += _direction * moveDistance;
        _traveledDistance += moveDistance;
    }

    /// <summary>
    /// 检查碰撞
    /// </summary>
    private void CheckCollision()
    {
        // 使用球形检测避免高速穿透
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.2f, _enemyLayer);
        
        foreach (var hitCollider in hitColliders)
        {
            // 检查是否是CombatEntity类型的敌人（统一处理Monster和MonsterAI_Enhanced）
            var combatEntity = hitCollider.GetComponent<CombatEntity>();
            if (combatEntity != null && combatEntity.CurrentHealth > 0 && !IsPartner(hitCollider))
            {
                // 确认是敌人类型（ObjectType.Monster）
                var objectBase = hitCollider.GetComponent<ObjectBase>();
                if (objectBase != null && objectBase.ObjectType == ObjectType.Monster)
                {
                    // 造成伤害
                    DealDamage(combatEntity, hitCollider.transform.position);
                    return;
                }
            }
            
            // 检查是否是其他可承伤对象（但不是伙伴自己）
            var damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null && !IsPartner(hitCollider))
            {
                DealDamage(damageable, hitCollider.transform.position);
                return;
            }
        }
        
        // 检查是否撞到障碍物
        if (Physics.CheckSphere(transform.position, 0.1f, LayerMask.GetMask("Obstacle")))
        {
            Debug.Log("[PartnerBullet] 撞到障碍物");
            DestroyBullet();
        }
    }

    /// <summary>
    /// 检查是否是伙伴对象（避免误伤）
    /// </summary>
    private bool IsPartner(Collider collider)
    {
        var partner = collider.GetComponent<Partner>();
        return partner != null;
    }

    /// <summary>
    /// 对目标造成伤害
    /// </summary>
    private void DealDamage(IDamageable target, Vector3 hitPoint)
    {
        if (_isDestroyed) return;
        
        // 创建伤害信息
        var damageInfo = new DamageInfo
        {
            Damage = _damage,
            Type = DamageType.Physical,
            HitPoint = hitPoint,
            Direction = _direction,
            Source = _source
        };
        
        // 造成伤害
        float actualDamage = target.TakeDamage(damageInfo);
        
        Debug.Log($"[PartnerBullet] 击中目标，造成 {actualDamage} 伤害");
        
        // 子弹消失
        DestroyBullet();
    }

    /// <summary>
    /// 检查是否超出最大距离
    /// </summary>
    private void CheckMaxDistance()
    {
        if (_traveledDistance >= _maxDistance)
        {
            Debug.Log($"[PartnerBullet] 达到最大距离 {_maxDistance}，子弹消失");
            DestroyBullet();
        }
    }

    /// <summary>
    /// 销毁子弹
    /// </summary>
    private void DestroyBullet()
    {
        if (_isDestroyed) return;
        
        _isDestroyed = true;
        
        // 可以在这里添加爆炸特效或其他效果
        // TODO: 添加子弹消失特效
        
        Destroy(gameObject);
    }

    /// <summary>
    /// 获取子弹信息（调试用）
    /// </summary>
    public string GetBulletInfo()
    {
        return $"子弹信息:\n" +
               $"伤害: {_damage}\n" +
               $"速度: {_speed}\n" +
               $"已飞行: {_traveledDistance:F1}/{_maxDistance:F1}";
    }

    // 调试用的Gizmos
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // 绘制子弹检测范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        
        // 绘制飞行轨迹
        Gizmos.color = Color.yellow;
        if (_direction != Vector3.zero)
        {
            Gizmos.DrawRay(transform.position, _direction * 2f);
        }
        
        // 绘制剩余飞行距离
        if (_maxDistance > 0 && _traveledDistance < _maxDistance)
        {
            Gizmos.color = Color.green;
            float remainingDistance = _maxDistance - _traveledDistance;
            Gizmos.DrawRay(transform.position, _direction * remainingDistance);
        }
    }
} 