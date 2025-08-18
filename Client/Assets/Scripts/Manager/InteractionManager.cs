using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 交互管理器 - 专门处理对象交互的完整流程（寻路→到达→执行交互）
/// 职责：监听交互事件、管理玩家移动到目标、执行交互逻辑
/// 纯 C# 类，由 GameMain 统一管理生命周期和更新
/// </summary>
public class InteractionManager
{
    private static InteractionManager _instance;
    public static InteractionManager Instance => _instance ??= new InteractionManager();

    private const float InteractionCheckInterval = 0.1f; // 交互检测间隔
    private IClickable _currentTarget;              // 当前目标交互物体
    private Vector3 _targetPosition;                // 目标位置
    private bool _isMovingToTarget;                 // 是否正在移动到目标
    private float _lastInteractionCheck;            // 上次交互检测时间
    private bool _initialized;
    
    // 范围追踪相关
    private IClickable _nearbyTarget;               // 当前范围内的目标
    private bool _wasInRange;                       // 上一帧是否在范围内

    private InteractionManager() { }

    public void Initialize()
    {
        if (_initialized) return;
        
        SubscribeToEvents();
        _initialized = true;
        
        Debug.Log("[InteractionManager] Initialized");
    }

    public void Cleanup()
    {
        UnsubscribeFromEvents();
        CancelCurrentInteraction();
        
        // 清理范围追踪状态
        _nearbyTarget = null;
        _wasInRange = false;
        
        _initialized = false;
        
        Debug.Log("[InteractionManager] Cleaned up");
    }

    public void Update()
    {
        if (!_initialized) return;
        UpdateTargetInteraction();
        UpdateNearbyInteraction();
    }

    private void SubscribeToEvents()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Subscribe<ObjectInteractionEvent>(OnObjectInteraction);
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<ObjectInteractionEvent>(OnObjectInteraction);
        }
    }

    private void OnObjectInteraction(ObjectInteractionEvent eventData)
    {
        StartInteraction(eventData.Target, eventData.ClickPosition);
    }

    private void StartInteraction(IClickable target, Vector3 clickPosition)
    {
        if (target == null || !target.CanInteract) return;

        var player = Player.Instance;
        if (player == null) return;

        _currentTarget = target;
        _targetPosition = clickPosition;
        _isMovingToTarget = true;

        Vector3 targetObject = target.GetInteractionRange() > 0 ? 
            GetInteractionPosition(clickPosition, target) : clickPosition;

        MovePlayerToPosition(targetObject);

        Debug.Log($"[InteractionManager] Started interaction with {((MonoBehaviour)target).name}");
    }

    private Vector3 GetInteractionPosition(Vector3 targetPosition, IClickable target)
    {
        var player = Player.Instance;
        if (player == null) return targetPosition;

        Vector3 playerPos = player.transform.position;
        Vector3 direction = (targetPosition - playerPos).normalized;
        float interactionRange = Mathf.Max(1f, target.GetInteractionRange() - 0.5f);
        
        return targetPosition - direction * interactionRange;
    }

    private void MovePlayerToPosition(Vector3 position)
    {
        var player = Player.Instance;
        if (player == null) return;

        player.MoveToPosition(position);
    }

    private void UpdateTargetInteraction()
    {
        if (!_isMovingToTarget || _currentTarget == null) return;

        if (Time.time - _lastInteractionCheck < InteractionCheckInterval) return;
        _lastInteractionCheck = Time.time;

        var player = Player.Instance;
        if (player == null)
        {
            CancelCurrentInteraction();
            return;
        }

        if (!_currentTarget.CanInteract)
        {
            CancelCurrentInteraction();
            return;
        }

        float distanceToTarget = Vector3.Distance(player.transform.position, _targetPosition);
        float interactionRange = _currentTarget.GetInteractionRange();

        if (distanceToTarget <= interactionRange)
        {
            PerformInteraction();
        }
        else
        {
            var navAgent = player.GetComponent<NavMeshAgent>();
            if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance < 0.1f)
            {
                if (distanceToTarget <= interactionRange * 2f)
                {
                    PerformInteraction();
                }
                else
                {
                    Debug.Log("[InteractionManager] Player couldn't reach target, canceling interaction");
                    CancelCurrentInteraction();
                }
            }
        }
    }

    private void PerformInteraction()
    {
        if (_currentTarget == null) return;

        var player = Player.Instance;
        if (player == null) return;

        // 如果是Building类型，直接调用OnInteract执行交互逻辑
        if (_currentTarget is Building building)
        {
            if (building.CanInteract) // 确保仍可交互
            {
                building.OnInteract(_targetPosition);
            }
        }
        else if (_currentTarget is IClickable clickable)
        {
            clickable.OnClick(_targetPosition);
        }

        CancelCurrentInteraction();
    }

    private void CancelCurrentInteraction()
    {
        _currentTarget = null;
        _isMovingToTarget = false;
        _targetPosition = Vector3.zero;
    }

    /// <summary>
    /// 检查玩家附近的交互对象，处理进入/离开交互范围的逻辑
    /// </summary>
    private void UpdateNearbyInteraction()
    {
        var player = Player.Instance;
        if (player == null) return;

        IClickable nearestTarget = null;
        float nearestDistance = float.MaxValue;

        // 查找附近需要范围提示的Building对象
        var colliders = Physics.OverlapSphere(player.transform.position, 5f); // 5米检测范围
        foreach (var collider in colliders)
        {
            var clickable = collider.GetComponent<IClickable>();
            if (clickable != null && clickable != _currentTarget) // 不检查正在交互的目标
            {
                // 只处理Building类型的范围提示
                if (clickable is Building building)
                {
                    float distance = Vector3.Distance(player.transform.position, collider.transform.position);
                    float interactionRange = clickable.GetInteractionRange();
                    
                    if (distance <= interactionRange && distance < nearestDistance)
                    {
                        nearestTarget = clickable;
                        nearestDistance = distance;
                    }
                }
            }
        }

        // 检查范围状态变化
        bool currentInRange = nearestTarget != null;
        bool targetChanged = nearestTarget != _nearbyTarget;

        if (currentInRange != _wasInRange || targetChanged)
        {
            // 处理离开之前目标的范围
            if (_nearbyTarget is Building oldBuilding && (!currentInRange || targetChanged))
            {
                oldBuilding.OnLeaveInteractionRange();
            }

            // 处理进入新目标的范围
            if (nearestTarget is Building newBuilding && currentInRange)
            {
                newBuilding.OnEnterInteractionRange();
            }

            _nearbyTarget = nearestTarget;
            _wasInRange = currentInRange;
        }
    }

    public bool IsInteracting => _isMovingToTarget && _currentTarget != null;

    public IClickable CurrentTarget => _currentTarget;
} 