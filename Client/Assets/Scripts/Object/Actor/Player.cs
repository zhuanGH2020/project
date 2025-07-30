using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 玩家角色
/// </summary>
public class Player : CombatEntity
{
    private static Player _instance;
    public static Player Instance => _instance;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;          // 移动速度

    private Vector3 _moveDirection;     // 移动方向
    private NavMeshAgent _navMeshAgent; // NavMesh代理

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
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected override void Update()
    {
        base.Update();
        HandleInput();
        HandleMovement();
    }

    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        _moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 鼠标左键点击移动
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }

        // 使用手持装备
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseHandEquip();
        }

        // 装备快捷键
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Equip(30001);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Equip(30002);
        }
    }

    /// <summary>
    /// 处理鼠标点击移动
    /// </summary>
    private void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (_navMeshAgent != null)
            {
                _navMeshAgent.SetDestination(hit.point);
            }
        }
    }

    /// <summary>
    /// 处理移动
    /// </summary>
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
    /// 重写攻击实现，玩家只能通过装备造成伤害
    /// </summary>
    public override void PerformAttack(IDamageable target)
    {
        // 玩家不能直接攻击，必须通过装备
    }
} 