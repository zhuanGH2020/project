using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 玩家角色
/// </summary>
public partial class Player : CombatEntity
{
    private static Player _instance;
    public static Player Instance => _instance;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;          // 移动速度

    private Vector3 _moveDirection;     // 移动方向
    private NavMeshAgent _navMeshAgent; // NavMesh代理
    private bool _inBuildingPlacementMode = false; // 是否在建筑放置模式（阻止移动）

    protected override void Awake()
    {
        base.Awake();
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        
        // 设置玩家最大血量
        _maxHealth = GameSettings.PlayerMaxHealth;
        _currentHealth = _maxHealth;
        
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
    }

    // 订阅输入事件
    private void SubscribeToInputEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMoveInput += OnMoveInput;
            InputManager.Instance.OnMouseClickMove += OnMouseClickMove;
            InputManager.Instance.OnUseEquipInput += OnUseEquipInput;
            InputManager.Instance.OnEquipShortcutInput += OnEquipShortcutInput;
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
            InputManager.Instance.OnMouseClickMove -= OnMouseClickMove;
            InputManager.Instance.OnUseEquipInput -= OnUseEquipInput;
            InputManager.Instance.OnEquipShortcutInput -= OnEquipShortcutInput;
        }
        
        // 取消订阅建筑放置模式状态变化事件
        EventManager.Instance.Unsubscribe<BuildingPlacementModeEvent>(OnBuildingPlacementModeChanged);
    }

    // 处理移动输入
    private void OnMoveInput(Vector3 moveDirection)
    {
        _moveDirection = moveDirection;
    }

    // 处理鼠标点击移动
    private void OnMouseClickMove(Vector3 targetPosition)
    {
        // Check if there's an interactable object at the click position first
        if (CheckForInteractableAtPosition(targetPosition))
        {
            // Let InteractionManager handle the interaction
            return;
        }
        
        // No interactable found, use unified movement method
        MoveToPosition(targetPosition);
    }

    /// <summary>
    /// Check if there's an interactable object at the clicked position and trigger interaction
    /// </summary>
    private bool CheckForInteractableAtPosition(Vector3 targetPosition)
    {
        // Use a small sphere to detect nearby interactables
        Collider[] colliders = Physics.OverlapSphere(targetPosition, 1f);
        
        foreach (var collider in colliders)
        {
            var clickable = collider.GetComponent<IClickable>();
            if (clickable != null)
            {
                Debug.Log($"[Player] Found clickable: {collider.name}, CanInteract: {clickable.CanInteract}");
                if (clickable.CanInteract)
                {
                    // Found an interactable, trigger its click event
                    clickable.OnClick(targetPosition);
                    return true;
                }
            }
        }
        
        return false;
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
            if (_navMeshAgent != null)
            {
                _navMeshAgent.ResetPath();
            }
            
            // 直接设置朝向
            transform.rotation = Quaternion.LookRotation(_moveDirection);
            // 移动
            transform.position += _moveDirection * _moveSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 公开方法：移动到指定位置 - 供其他系统调用
    /// </summary>
    public void MoveToPosition(Vector3 targetPosition)
    {
        // 如果在建筑放置模式，不响应移动命令
        if (_inBuildingPlacementMode)
        {
            return;
        }
        
        if (_navMeshAgent != null)
        {
            _navMeshAgent.SetDestination(targetPosition);
        }
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
} 