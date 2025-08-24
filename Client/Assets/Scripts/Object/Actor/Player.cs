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
    
    // 添加Animator引用
    private Animator _animator;
    
    // 攻击系统相关
    private Transform _attackTarget;           // 当前攻击目标
    private float _lastAttackTime;             // 上次攻击时间
    
    // 玩家状态属性
    private float _currentHunger;             // 当前饥饿值
    private float _currentSanity;             // 当前理智值
    
    // 自动下降定时器
    private const float STATUS_DECREASE_INTERVAL = 5f;  // 状态下降间隔（秒）
    private const float STATUS_DECREASE_AMOUNT = 1f;    // 每次下降的数值
    private float _hungerDecreaseTimer = 0f;            // 饥饿值下降计时器
    private float _sanityDecreaseTimer = 0f;            // 理智值下降计时器
    
    // 长按攻击相关
    private bool _isContinuousAttack = false;  // 是否处于连续攻击状态

    // 重写DamageableObject的抽象属性
    public override float MaxHealth => GameSettings.PlayerMaxHealth;
    public override float Defense => base.Defense;
    public override bool CanInteract => _currentHealth > 0;
    public override float GetInteractionRange() => 2f;
    
    // 玩家状态属性
    public float MaxHunger => GameSettings.PlayerMaxHunger;
    public float CurrentHunger => _currentHunger;
    public float MaxSanity => GameSettings.PlayerMaxSanity;
    public float CurrentSanity => _currentSanity;

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
        
        // 获取Animator组件
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("[Player] 未找到Animator组件！");
        }
        
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
        
        // 初始化玩家状态属性
        _currentHunger = MaxHunger;
        _currentSanity = MaxSanity;
        
        // 初始化定时器
        _hungerDecreaseTimer = 0f;
        _sanityDecreaseTimer = 0f;
        
        // 订阅输入事件
        SubscribeToInputEvents();
        SetObjectType(ObjectType.Player);
    }



    private void OnDestroy()
    {
        if (_instance != null && _instance == this)
        {
            _instance = null;
            // 取消订阅输入事件
            UnsubscribeFromInputEvents();
        }
    }

    protected override void Update()
    {
        base.Update();
        HandleMouseRotation();
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
    }

    // 处理移动输入
    private void OnMoveInput(Vector3 moveDirection)
    {
        _moveDirection = moveDirection;
        
        // 设置Animator参数
        UpdateAnimatorMovementParams();
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

    // ========== 鼠标朝向系统 ==========

    /// <summary>
    /// 处理鼠标朝向 - 寻路时不朝向鼠标，而是朝向移动方向
    /// </summary>
    private void HandleMouseRotation()
    {
        // 如果正在寻路中，不朝向鼠标，而是朝向移动方向
        if (IsMovingToTarget())
        {
            HandleMovementRotation();
            return;
        }
        
        // 不在寻路时，正常朝向鼠标
        if (Camera.main == null) return;
        
        // 获取鼠标屏幕位置
        Vector3 mousePosition = Input.mousePosition;
        
        // 创建从相机到鼠标的射线
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        
        // 计算射线与玩家所在水平面的交点（Y轴固定）
        float playerY = transform.position.y;
        float rayDirectionY = ray.direction.y;
        
        // 如果射线方向与水平面平行，使用距离计算
        if (Mathf.Abs(rayDirectionY) < 0.001f)
        {
            float distanceToPlayer = Vector3.Distance(Camera.main.transform.position, transform.position);
            Vector3 mouseWorldPos = ray.GetPoint(distanceToPlayer);
            mouseWorldPos.y = playerY;
            HandleRotationToPosition(mouseWorldPos);
        }
        else
        {
            // 计算射线与水平面的交点
            float t = (playerY - ray.origin.y) / rayDirectionY;
            if (t > 0) // 确保交点在相机前方
            {
                Vector3 mouseWorldPos = ray.GetPoint(t);
                HandleRotationToPosition(mouseWorldPos);
            }
        }
    }
    
    /// <summary>
    /// 处理移动朝向 - 朝向移动方向
    /// </summary>
    private void HandleMovementRotation()
    {
        if (_navMeshAgent == null || !_navMeshAgent.hasPath) return;
        
        // 获取移动方向
        Vector3 moveDirection = _navMeshAgent.velocity.normalized;
        if (moveDirection != Vector3.zero)
        {
            // 只考虑水平方向
            moveDirection.y = 0;
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                float rotationSpeed = 15f; // 移动时旋转速度稍慢，更自然
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
    
    /// <summary>
    /// 检查是否正在寻路移动
    /// </summary>
    private bool IsMovingToTarget()
    {
        if (_navMeshAgent == null) return false;
        
        // 检查是否有路径且正在移动
        return _navMeshAgent.hasPath && 
               _navMeshAgent.remainingDistance > 0.1f && 
               _navMeshAgent.velocity.magnitude > 0.1f;
    }
    
    /// <summary>
    /// 处理朝向指定位置的旋转逻辑
    /// </summary>
    private void HandleRotationToPosition(Vector3 targetPosition)
    {
        // 计算朝向目标的方向（忽略Y轴）
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        directionToTarget.y = 0;
        
        // 只有当方向不为零时才进行旋转
        if (directionToTarget != Vector3.zero)
        {
            // 计算目标旋转
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            // 平滑旋转朝向目标（提高旋转速度确保响应及时）
            float rotationSpeed = 20f; // 进一步提高旋转速度
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    // 处理移动
    private void HandleMovement()
    {
        if (_moveDirection != Vector3.zero)
        {
            // 停止NavMesh移动，使用键盘移动
            StopMovement();
            
            // 获取当前速度（优先使用NavMeshAgent的speed，否则使用_moveSpeed）
            float currentSpeed = (_navMeshAgent != null) ? _navMeshAgent.speed : _moveSpeed;
            
            // 移动
            transform.position += _moveDirection * currentSpeed * Time.deltaTime;
        }
        
        // 统一更新动画参数（包括停止移动的情况）
        UpdateAnimatorMovementParams();
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
        bool result = base.MoveToPosition(targetPosition);
        
        return result;
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
    /// 设置饥饿值
    /// </summary>
    public void SetHunger(float hunger)
    {
        float previousHunger = _currentHunger;
        _currentHunger = Mathf.Clamp(hunger, 0, MaxHunger);
        
        // 如果饥饿值发生了变化，触发饥饿变化事件
        if (Mathf.Abs(_currentHunger - previousHunger) > 0.001f)
        {
            TriggerHungerChangeEvent(previousHunger, _currentHunger);
        }
    }

    /// <summary>
    /// 设置理智值
    /// </summary>
    public void SetSanity(float sanity)
    {
        float previousSanity = _currentSanity;
        _currentSanity = Mathf.Clamp(sanity, 0, MaxSanity);
        
        // 如果理智值发生了变化，触发理智变化事件
        if (Mathf.Abs(_currentSanity - previousSanity) > 0.001f)
        {
            TriggerSanityChangeEvent(previousSanity, _currentSanity);
        }
    }

    /// <summary>
    /// 触发饥饿值变化事件
    /// </summary>
    private void TriggerHungerChangeEvent(float previousHunger, float currentHunger)
    {
        var hungerChangeEvent = new PlayerHungerChangeEvent(previousHunger, currentHunger);
        EventManager.Instance.Publish(hungerChangeEvent);
    }

    /// <summary>
    /// 触发理智值变化事件
    /// </summary>
    private void TriggerSanityChangeEvent(float previousSanity, float currentSanity)
    {
        var sanityChangeEvent = new PlayerSanityChangeEvent(previousSanity, currentSanity);
        EventManager.Instance.Publish(sanityChangeEvent);
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

    /// <summary>
    /// 更新玩家状态 - 处理饥饿和理智的自动下降
    /// 需要外部定期调用（如GameMain.Update）
    /// </summary>
    public void UpdateStatus()
    {
        // 检查时间是否暂停或玩家已死亡
        if (ClockModel.Instance.IsTimePaused || CurrentHealth <= 0)
            return;

        // 更新饥饿值计时器
        _hungerDecreaseTimer += Time.deltaTime;
        if (_hungerDecreaseTimer >= STATUS_DECREASE_INTERVAL)
        {
            _hungerDecreaseTimer = 0f;
            float newHunger = Mathf.Max(0, _currentHunger - STATUS_DECREASE_AMOUNT);
            SetHunger(newHunger);
        }

        // 更新理智值计时器
        _sanityDecreaseTimer += Time.deltaTime;
        if (_sanityDecreaseTimer >= STATUS_DECREASE_INTERVAL)
        {
            _sanityDecreaseTimer = 0f;
            float newSanity = Mathf.Max(0, _currentSanity - STATUS_DECREASE_AMOUNT);
            SetSanity(newSanity);
        }
    }

    // ========== 攻击系统方法 ==========
    
    // ========== 攻击系统重构 ==========

    /// <summary>
    /// 处理攻击点击事件 - 重构为基于鼠标方向的攻击
    /// </summary>
    private void OnAttackClick(Vector3 clickPosition)
    {
        // 直接向鼠标方向攻击
        TryAttackInMouseDirection(clickPosition);
    }
    
    /// <summary>
    /// 向鼠标方向攻击 - 新的攻击方法
    /// </summary>
    private void TryAttackInMouseDirection(Vector3 mousePosition)
    {
        // 检查是否有武器
        var handEquip = GetCurrentHandEquip();
        if (handEquip == null)
        {
            return;
        }
        
        // 检查武器状态
        if (!handEquip.CanUse)
        {
            return;
        }
        
        // 使用装备攻击（朝向已经在HandleMouseRotation中处理）
        handEquip.Use();
        _lastAttackTime = Time.time;
        
        Debug.Log($"[Player] 向鼠标方向攻击: {mousePosition}");
    }
    
    /// <summary>
    /// 获取当前手部装备
    /// </summary>
    private EquipBase GetCurrentHandEquip()
    {
        return _equips.Find(equip => equip.EquipPart == EquipPart.Hand);
    }
    
    // ========== 长按连续攻击系统 ==========
    
    /// <summary>
    /// 处理长按攻击事件
    /// </summary>
    private void OnAttackHold(bool isHolding)
    {
        if (isHolding)
        {
            // 检查是否有可用武器
            var handEquip = GetCurrentHandEquip();
            _isContinuousAttack = handEquip != null;
        }
        else
        {
            _isContinuousAttack = false;
        }
    }
    
    /// <summary>
    /// 处理连续攻击逻辑
    /// </summary>
    private void HandleContinuousAttack()
    {
        if (!_isContinuousAttack) return;
        
        // 直接尝试攻击，装备CD会自动控制频率
        TryAttackInMouseDirection(Input.mousePosition);
    }

    // 更新Animator移动参数
    private void UpdateAnimatorMovementParams()
    {
        if (_animator == null) return;
        
        // 如果正在寻路移动，设置动画参数为前进
        if (IsMovingToTarget())
        {
            _animator.SetFloat("MoveX", 0f);
            _animator.SetFloat("MoveY", 1f);
            return;
        }
        
        // 键盘移动时，根据实际移动方向设置动画参数
        if (_moveDirection != Vector3.zero)
        {
            // 获取世界坐标的移动方向
            Vector3 worldMoveDirection = _moveDirection.normalized;
            
            // 将世界坐标的移动方向转换为角色本地坐标
            Vector3 localMoveDirection = transform.InverseTransformDirection(worldMoveDirection);
            
            // 设置Animator参数
            // MoveX: 左右移动 (-1左, 0中, 1右)
            // MoveY: 前后移动 (-1后, 0中, 1前)
            _animator.SetFloat("MoveX", localMoveDirection.x);
            _animator.SetFloat("MoveY", localMoveDirection.z);
        }
        else
        {
            // 停止移动时，将动画参数设为0
            _animator.SetFloat("MoveX", 0f);
            _animator.SetFloat("MoveY", 0f);
        }
    }

    /// <summary>
    /// 重写停止移动方法，确保动画参数正确设置
    /// </summary>
    public override void StopMovement()
    {
        base.StopMovement();
        
        // 停止移动时，将动画参数设为0
        if (_animator != null)
        {
            _animator.SetFloat("MoveX", 0f);
            _animator.SetFloat("MoveY", 0f);
        }
    }

} 