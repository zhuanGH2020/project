using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 交互管理器 - 处理点击→寻路→交互的完整流程
/// 管理玩家与可交互物体的交互逻辑
/// </summary>
public class InteractionManager : MonoBehaviour
{
    private static InteractionManager _instance;
    public static InteractionManager Instance => _instance;

    [Header("Interaction Settings")]
    [SerializeField] private LayerMask _interactableLayerMask = -1;  // 可交互物体的层级
    [SerializeField] private float _interactionCheckInterval = 0.1f; // 交互检测间隔

    private IClickable _currentTarget;              // 当前目标交互物体
    private Vector3 _targetPosition;                // 目标位置
    private bool _isMovingToTarget;                 // 是否正在移动到目标
    private float _lastInteractionCheck;            // 上次交互检测时间

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // Subscribe to events
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            UnsubscribeFromEvents();
        }
    }

    private void Update()
    {
        // Remove direct mouse handling - let Player handle it
        // HandleMouseInteraction();
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

    /// <summary>
    /// Handle mouse clicks on interactable objects
    /// </summary>
    private void HandleMouseInteraction()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // Skip if clicking on UI
        if (InputUtils.IsPointerOverUI()) return;

        // Raycast to find interactable objects
        if (InputUtils.GetMouseWorldHit(out RaycastHit hit, Mathf.Infinity, _interactableLayerMask))
        {
            var clickable = hit.collider.GetComponent<IClickable>();
            if (clickable != null && clickable.CanInteract)
            {
                // Trigger click interaction
                clickable.OnClick(hit.point);
                return;
            }
        }

        // If no interactable hit, cancel current interaction
        CancelCurrentInteraction();
    }

    /// <summary>
    /// Handle object interaction events
    /// </summary>
    private void OnObjectInteraction(ObjectInteractionEvent eventData)
    {
        StartInteraction(eventData.Target, eventData.ClickPosition);
    }

    /// <summary>
    /// Start interaction with target object
    /// </summary>
    private void StartInteraction(IClickable target, Vector3 clickPosition)
    {
        if (target == null || !target.CanInteract) return;

        var player = Player.Instance;
        if (player == null) return;

        // Set new target
        _currentTarget = target;
        _targetPosition = clickPosition;
        _isMovingToTarget = true;

        // Calculate interaction position (closer to target for better interaction)
        Vector3 targetObject = target.GetInteractionRange() > 0 ? 
            GetInteractionPosition(clickPosition, target) : clickPosition;

        // Move player to interaction position
        MovePlayerToPosition(targetObject);

        Debug.Log($"[InteractionManager] Started interaction with {((MonoBehaviour)target).name}");
    }

    /// <summary>
    /// Calculate optimal interaction position near the target
    /// </summary>
    private Vector3 GetInteractionPosition(Vector3 targetPosition, IClickable target)
    {
        var player = Player.Instance;
        if (player == null) return targetPosition;

        Vector3 playerPos = player.transform.position;
        Vector3 direction = (targetPosition - playerPos).normalized;
        float interactionRange = Mathf.Max(1f, target.GetInteractionRange() - 0.5f);
        
        return targetPosition - direction * interactionRange;
    }

    /// <summary>
    /// Move player to specified position using Player's public method
    /// </summary>
    private void MovePlayerToPosition(Vector3 position)
    {
        var player = Player.Instance;
        if (player == null) return;

        // Use player's public movement method
        player.MoveToPosition(position);
    }

    /// <summary>
    /// Update current target interaction - check if player reached target
    /// </summary>
    private void UpdateTargetInteraction()
    {
        if (!_isMovingToTarget || _currentTarget == null) return;

        // Throttle check frequency
        if (Time.time - _lastInteractionCheck < _interactionCheckInterval) return;
        _lastInteractionCheck = Time.time;

        var player = Player.Instance;
        if (player == null)
        {
            CancelCurrentInteraction();
            return;
        }

        // Check if target is still valid
        if (!_currentTarget.CanInteract)
        {
            CancelCurrentInteraction();
            return;
        }

        // Check if player is close enough to interact
        float distanceToTarget = Vector3.Distance(player.transform.position, _targetPosition);
        float interactionRange = _currentTarget.GetInteractionRange();

        if (distanceToTarget <= interactionRange)
        {
            // Player reached target, perform interaction
            PerformInteraction();
        }
        else
        {
            // Check if player has stopped moving (stuck or path blocked)
            var navAgent = player.GetComponent<NavMeshAgent>();
            if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance < 0.1f)
            {
                // Player stopped but not in range - try direct interaction if close enough
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

    /// <summary>
    /// Perform the actual interaction with the target
    /// </summary>
    private void PerformInteraction()
    {
        if (_currentTarget == null) return;

        var player = Player.Instance;
        if (player == null) return;

        // Check if target is harvestable
        if (_currentTarget is IHarvestable harvestable && harvestable.CanHarvest)
        {
            harvestable.OnHarvest(player);
            Debug.Log($"[InteractionManager] Harvested {((MonoBehaviour)_currentTarget).name}");
        }

        // Clear current interaction
        CancelCurrentInteraction();
    }

    /// <summary>
    /// Cancel current interaction
    /// </summary>
    private void CancelCurrentInteraction()
    {
        _currentTarget = null;
        _isMovingToTarget = false;
        _targetPosition = Vector3.zero;
    }

    /// <summary>
    /// Check if player is currently moving to an interaction target
    /// </summary>
    public bool IsInteracting => _isMovingToTarget && _currentTarget != null;

    /// <summary>
    /// Get current interaction target
    /// </summary>
    public IClickable CurrentTarget => _currentTarget;
} 