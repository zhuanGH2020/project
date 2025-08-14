using UnityEngine;
using System.Collections;

/// <summary>
/// 烹饪锅组件 - 处理玩家与锅的交互
/// 需要挂载到锅的GameObject上
/// </summary>
public class CookingPot : Building, IClickable
{
    [Header("Cooking Settings")]
    [SerializeField] private float _interactionRange = 3f;  // 交互范围
    
    private bool _playerInRange = false;
    private Player _player;
    
    // 重写CanInteract属性，允许远距离点击触发寻路
    // 只要基础交互条件满足就可以点击，具体的范围检查在OnClick中处理
    public new bool CanInteract => base.CanInteract;

    protected override void Awake()
    {
        base.Awake();
        // 确保对象类型正确设置为Building（继承自Building已设置，这里可选）
        SetObjectType(ObjectType.Building);
    }

    private void Start()
    {
        _player = Player.Instance;
        
        // 订阅输入事件
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnEquipShortcutInput += OnEquipShortcutInput;
        }
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnEquipShortcutInput -= OnEquipShortcutInput;
        }
    }

    private void Update()
    {
        CheckPlayerRange();
    }

    /// <summary>
    /// 检查玩家是否在交互范围内
    /// </summary>
    private void CheckPlayerRange()
    {
        if (_player == null) return;

        float distance = Vector3.Distance(transform.position, _player.transform.position);
        bool wasInRange = _playerInRange;
        _playerInRange = distance <= _interactionRange;

        // 范围状态改变时的处理
        if (_playerInRange != wasInRange)
        {
            if (_playerInRange)
            {
                // 发布进入交互范围事件
                EventManager.Instance.Publish(new CookingInteractionEvent(true, this));
            }
            else
            {
                // 发布离开交互范围事件
                EventManager.Instance.Publish(new CookingInteractionEvent(false, this));
                
                // 关闭烹饪界面
                CookingModel.Instance.CloseCookingUI();
            }
        }
    }

    /// <summary>
    /// 处理按键输入（E键对应30002装备ID）
    /// </summary>
    private void OnEquipShortcutInput(int equipId)
    {
        if (equipId == 30002 && _playerInRange) // E键对应30002
        {
            // 如果烹饪UI已经打开，则关闭它；否则打开
            if (CookingModel.Instance.IsUIOpen)
            {
                CookingModel.Instance.CloseCookingUI();
            }
            else
            {
                OpenCookingUI();
            }
        }
    }

    /// <summary>
    /// 点击锅时的处理逻辑
    /// - 如果玩家在范围内：直接打开烹饪界面
    /// - 如果玩家不在范围内：触发寻路，走到锅附近再打开烹饪界面
    /// </summary>
    public void OnClick(Vector3 clickPosition)
    {
        if (_playerInRange)
        {
            // 玩家在范围内，直接打开UI
            OpenCookingUI();
        }
        else
        {
            // 玩家不在范围内，触发寻路
            StartMoveToInteract(clickPosition);
        }
    }

    /// <summary>
    /// 开始移动到锅并交互
    /// </summary>
    private void StartMoveToInteract(Vector3 clickPosition)
    {
        var player = Player.Instance;
        if (player == null) return;

        // 计算交互位置（在锅的交互范围边缘）
        Vector3 interactionPosition = GetOptimalInteractionPosition(player.transform.position);
        
        // 使用Player的移动方法
        player.MoveToPosition(interactionPosition);
        
        // 开始监听到达事件
        StartCoroutine(MonitorPlayerArrival(interactionPosition));
        
        Debug.Log($"[CookingPot] 开始寻路到烹饪锅，目标位置: {interactionPosition}");
    }

    /// <summary>
    /// 计算最佳交互位置
    /// </summary>
    private Vector3 GetOptimalInteractionPosition(Vector3 playerPosition)
    {
        Vector3 potPosition = transform.position;
        Vector3 direction = (playerPosition - potPosition).normalized;
        
        // 在交互范围边缘放置目标点（留一点缓冲）
        float targetDistance = Mathf.Max(1f, _interactionRange - 0.5f);
        return potPosition + direction * targetDistance;
    }

    /// <summary>
    /// 监控玩家是否到达目标位置
    /// </summary>
    private System.Collections.IEnumerator MonitorPlayerArrival(Vector3 targetPosition)
    {
        var player = Player.Instance;
        if (player == null) yield break;

        var navAgent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
        
        // 监控玩家移动，直到到达目标或停止移动
        while (player != null)
        {
            float distanceToTarget = Vector3.Distance(player.transform.position, transform.position);
            
            // 检查是否已经在交互范围内
            if (distanceToTarget <= _interactionRange)
            {
                // 玩家到达交互范围，打开烹饪界面
                OpenCookingUI();
                Debug.Log("[CookingPot] 玩家到达目标位置，打开烹饪界面");
                yield break;
            }
            
            // 检查玩家是否停止移动（路径被阻挡或到达了NavMesh能到达的最近位置）
            if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance < 0.1f)
            {
                // 如果玩家停止移动且距离较近，也尝试打开UI
                if (distanceToTarget <= _interactionRange * 2f)
                {
                    OpenCookingUI();
                    Debug.Log("[CookingPot] 玩家停止移动但较接近目标，打开烹饪界面");
                    yield break;
                }
                else
                {
                    Debug.Log("[CookingPot] 玩家无法到达目标位置，取消交互");
                    yield break;
                }
            }
            
            yield return new WaitForSeconds(0.1f); // 每0.1秒检查一次
        }
    }

    /// <summary>
    /// 打开烹饪界面
    /// </summary>
    private void OpenCookingUI()
    {
        CookingModel.Instance.OpenCookingUI(transform.position);
    }

    public float GetInteractionRange()
    {
        return _interactionRange;
    }

    private void OnDrawGizmosSelected()
    {
        // 在Scene视图中显示交互范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
    }
}