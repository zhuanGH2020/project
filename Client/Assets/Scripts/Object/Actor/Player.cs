using UnityEngine;

/// <summary>
/// 玩家角色
/// </summary>
public partial class Player : CombatEntity
{
    private static Player _instance;
    public static Player Instance => _instance;

    private Vector3 _moveDirection;     // 移动方向
    private bool _inBuildingPlacementMode = false; // 是否在建筑放置模式（阻止移动）
    
    // 攻击系统相关
    private Transform _attackTarget;           // 当前攻击目标
    private bool _isContinuousAttack = false;  // 是否连续攻击模式
    private float _lastAttackTime;             // 上次攻击时间
    
    // 对话ID分组常量 - 每个类型包含多条随机消息
    private readonly int[] _noWeaponDialogIds = { 100, 101, 102, 103 };        // 没有武器
    private readonly int[] _weaponDamagedDialogIds = { 110, 111, 112 };        // 武器损坏
    private readonly int[] _weaponCooldownDialogIds = { 120, 121, 122 };       // 武器冷却
    private readonly int[] _distanceTooFarDialogIds = { 130, 131, 132, 133 };  // 距离太远

    // 重写DamageableObject的抽象属性
    public override float MaxHealth => GameSettings.PlayerMaxHealth;
    public override float Defense => base.Defense;
    public override bool CanInteract => _currentHealth > 0;
    public override float GetInteractionRange() => 2f;

    // 重写OnClick方法实现玩家点击逻辑
    public override void OnClick(Vector3 clickPosition)
    {
        Debug.Log("player click");
    }

    protected override void Awake()
    {
        base.Awake();
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // 从NavMeshAgent获取移动速度（如果存在的话）
        if (_navMeshAgent != null)
        {
            _moveSpeed = _navMeshAgent.speed;
        }
        else
        {
            // 如果没有NavMeshAgent，使用默认速度
            _moveSpeed = 5f;
        }
        
        // 玩家血量通过MaxHealth属性管理，_currentHealth在基类Awake中初始化
        
        // 订阅输入事件
        SubscribeToInputEvents();
        SetObjectType(ObjectType.Player);
    }



    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            // 取消订阅输入事件
            UnsubscribeFromInputEvents();
        }
    }

    protected override void Update()
    {
        base.Update();
        HandleMovement();
        HandleContinuousAttack();
    }

    // 订阅输入事件
    private void SubscribeToInputEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMoveInput += OnMoveInput;
            InputManager.Instance.OnUseEquipInput += OnUseEquipInput;
            InputManager.Instance.OnEquipShortcutInput += OnEquipShortcutInput;
            InputManager.Instance.OnAttackClick += OnAttackClick;
            InputManager.Instance.OnAttackHold += OnAttackHold;
        }
        
        // 订阅建筑放置模式状态变化事件
        EventManager.Instance.Subscribe<BuildingPlacementModeEvent>(OnBuildingPlacementModeChanged);

    }

    // 取消订阅输入事件
    private void UnsubscribeFromInputEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMoveInput -= OnMoveInput;
            InputManager.Instance.OnUseEquipInput -= OnUseEquipInput;
            InputManager.Instance.OnEquipShortcutInput -= OnEquipShortcutInput;
            InputManager.Instance.OnAttackClick -= OnAttackClick;
            InputManager.Instance.OnAttackHold -= OnAttackHold;
        }
        
        // 取消订阅建筑放置模式状态变化事件
        EventManager.Instance.Unsubscribe<BuildingPlacementModeEvent>(OnBuildingPlacementModeChanged);
        
        // 清理对话框
        ClearDialog();
    }

    // 处理移动输入
    private void OnMoveInput(Vector3 moveDirection)
    {
        _moveDirection = moveDirection;
    }





    // 处理使用装备输入
    private void OnUseEquipInput()
    {
        UseHandEquip();
    }

    // 处理装备快捷键输入
    private void OnEquipShortcutInput(int equipId)
    {
        Equip(equipId);
    }
    
    // 处理建筑放置模式状态变化
    private void OnBuildingPlacementModeChanged(BuildingPlacementModeEvent e)
    {
        _inBuildingPlacementMode = e.IsInPlacementMode;

    }

    // 处理移动
    private void HandleMovement()
    {
        if (_moveDirection != Vector3.zero)
        {
            // 停止NavMesh移动，使用键盘移动
            StopMovement();
            
            // 直接设置朝向
            transform.rotation = Quaternion.LookRotation(_moveDirection);
            
            // 获取当前速度（优先使用NavMeshAgent的speed，否则使用_moveSpeed）
            float currentSpeed = (_navMeshAgent != null) ? _navMeshAgent.speed : _moveSpeed;
            
            // 移动
            transform.position += _moveDirection * currentSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Player专用移动方法，增加建筑放置模式检查
    /// </summary>
    public bool MoveToPlayerPosition(Vector3 targetPosition)
    {
        // 如果在建筑放置模式，不响应移动命令
        if (_inBuildingPlacementMode)
        {
            return false;
        }
        
        // 调用基类的统一移动接口
        return base.MoveToPosition(targetPosition);
        }

    /// <summary>
    /// 重写移动接口，增加建筑放置模式检查
    /// </summary>
    public override bool MoveToPosition(Vector3 targetPosition)
    {
        return MoveToPlayerPosition(targetPosition);
    }

    /// <summary>
    /// 重写攻击实现，玩家只能通过装备造成伤害
    /// </summary>
    public override void PerformAttack(IDamageable target)
    {
        // 玩家不能直接攻击，必须通过装备
    }

    /// <summary>
    /// 重写受伤方法，添加血量变化事件触发
    /// </summary>
    public override float TakeDamage(DamageInfo damageInfo)
    {
        float previousHealth = CurrentHealth;
        float actualDamage = base.TakeDamage(damageInfo);
        
        // 如果血量发生了变化，触发血量变化事件
        if (Mathf.Abs(CurrentHealth - previousHealth) > 0.001f)
        {
            TriggerHealthChangeEvent(previousHealth, CurrentHealth);
        }
        
        return actualDamage;
    }

    /// <summary>
    /// 重写设置血量方法，添加血量变化事件触发
    /// </summary>
    public override void SetHealth(float health)
    {
        float previousHealth = CurrentHealth;
        base.SetHealth(health);
        
        // 如果血量发生了变化，触发血量变化事件
        if (Mathf.Abs(CurrentHealth - previousHealth) > 0.001f)
        {
            TriggerHealthChangeEvent(previousHealth, CurrentHealth);
        }
    }

    /// <summary>
    /// 触发血量变化事件
    /// </summary>
    private void TriggerHealthChangeEvent(float previousHealth, float currentHealth)
    {
        var healthChangeEvent = new PlayerHealthChangeEvent(previousHealth, currentHealth);
        EventManager.Instance.Publish(healthChangeEvent);
    }

    /// <summary>
    /// 重写死亡方法，玩家死亡时显示MenuView
    /// </summary>
    protected override void OnDeath()
    {
        base.OnDeath();
        
        // 暂停时间系统
        DebugModel.Instance.SetTimeEnabled(false);
        DebugModel.Instance.ManualSave();
        // 显示死亡菜单界面
        UIManager.Instance.Show<MenuView>(UILayer.System);
    }

    // ========== 攻击系统方法 ==========
    
    /// <summary>
    /// 处理攻击点击事件
    /// </summary>
    private void OnAttackClick(Vector3 clickPosition)
    {
        // 检测点击位置的怪物
        if (InputUtils.GetMouseWorldHit(out RaycastHit hit))
        {
            var monster = hit.collider.GetComponent<Monster>();
            if (monster != null && monster.CurrentHealth > 0)
            {
                SetAttackTarget(monster.transform, clickPosition);
            }
        }
    }
    
    /// <summary>
    /// 处理长按攻击事件
    /// </summary>
    private void OnAttackHold(bool isHolding)
    {
        bool wasInContinuousAttack = _isContinuousAttack;
        _isContinuousAttack = isHolding;
        
        if (isHolding)
        {
            Debug.Log("[Player] 开始连续攻击模式");
        }
        else if (wasInContinuousAttack)
        {
            // 只有在确实处于连续攻击状态时才输出结束日志
            Debug.Log("[Player] 结束连续攻击模式");
        }
    }
    
    /// <summary>
    /// 设置攻击目标
    /// </summary>
    private void SetAttackTarget(Transform target, Vector3 clickPosition)
    {
        _attackTarget = target;
        
        // 首先检查是否有武器
        var handEquip = GetCurrentHandEquip();
        if (handEquip == null)
        {
            ShowRandomDialogMessage(_noWeaponDialogIds);
            return;
        }
        
        // 检查武器状态
        if (!handEquip.CanUse)
        {
            // 武器正在冷却中或耐久为0
            if (handEquip.CurrentDurability <= 0)
            {
                ShowRandomDialogMessage(_weaponDamagedDialogIds);
            }
            else
            {
                ShowRandomDialogMessage(_weaponCooldownDialogIds);
            }
            return;
        }
        
        // 检查攻击范围
        if (CheckAttackRange(target))
        {
            // 在攻击范围内，直接攻击
            TryAttackTarget();
        }
        else
        {
            // 不在攻击范围内，显示提示
            ShowDistanceWarning();
        }
    }
    
    /// <summary>
    /// 检查攻击范围
    /// </summary>
    private bool CheckAttackRange(Transform target)
    {
        if (target == null) return false;
        
        // 获取当前手部装备的攻击范围
        var handEquip = GetCurrentHandEquip();
        float attackRange = handEquip != null ? handEquip.Range : 2f; // 默认范围2米
        
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= attackRange;
    }
    
    /// <summary>
    /// 获取当前手部装备
    /// </summary>
    private EquipBase GetCurrentHandEquip()
    {
        return _equips.Find(equip => equip.EquipPart == EquipPart.Hand);
    }
    
    /// <summary>
    /// 尝试攻击目标
    /// </summary>
    private void TryAttackTarget()
    {
        if (_attackTarget == null) return;
        
        var handEquip = GetCurrentHandEquip();
        if (handEquip == null || !handEquip.CanUse) return; // 武器检查已在SetAttackTarget中完成
        
        // 面向目标
        Vector3 direction = (_attackTarget.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        
        // 使用装备攻击
        handEquip.Use();
        _lastAttackTime = Time.time;
        
        Debug.Log($"[Player] 攻击目标: {_attackTarget.name}");
    }
    
    /// <summary>
    /// 处理连续攻击
    /// </summary>
    private void HandleContinuousAttack()
    {
        if (!_isContinuousAttack || _attackTarget == null) return;
        
        // 检查目标是否还活着
        var monster = _attackTarget.GetComponent<Monster>();
        if (monster == null || monster.CurrentHealth <= 0)
        {
            _attackTarget = null;
            _isContinuousAttack = false;
            return;
        }
        
        // 检查武器状态（优先级最高）
        var handEquip = GetCurrentHandEquip();
        if (handEquip == null)
        {
            ShowRandomDialogMessage(_noWeaponDialogIds);
            _isContinuousAttack = false;
            return;
        }
        
        if (!handEquip.CanUse)
        {
            // 武器不可用时不显示提示，只是暂停连续攻击等待冷却
            return;
        }
        
        // 检查攻击范围
        if (CheckAttackRange(_attackTarget))
        {
            TryAttackTarget();
        }
        else
        {
            // 目标超出范围，停止连续攻击并提示
            ShowDistanceWarning();
            _isContinuousAttack = false;
        }
    }
    
    /// <summary>
    /// 显示距离警告
    /// </summary>
    private void ShowDistanceWarning()
    {
        Debug.Log("[Player] 距离太远了，需要靠近目标！");
        ShowRandomDialogMessage(_distanceTooFarDialogIds);
    }


} 