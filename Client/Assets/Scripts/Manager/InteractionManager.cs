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
        _initialized = false;
        
        Debug.Log("[InteractionManager] Cleaned up");
    }

    public void Update()
    {
        if (!_initialized) return;
        UpdateTargetInteraction();
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

        if (_currentTarget is IClickable clickable)
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

    public bool IsInteracting => _isMovingToTarget && _currentTarget != null;

    public IClickable CurrentTarget => _currentTarget;
} 