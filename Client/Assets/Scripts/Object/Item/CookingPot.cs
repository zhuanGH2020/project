using UnityEngine;

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
    
    // 重写CanInteract属性，结合Building的交互状态和玩家范围检测
    public new bool CanInteract => base.CanInteract && _playerInRange;

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
            OpenCookingUI();
        }
    }

    /// <summary>
    /// 点击锅也可以打开烹饪界面
    /// </summary>
    public void OnClick(Vector3 clickPosition)
    {
        if (_playerInRange)
        {
            OpenCookingUI();
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