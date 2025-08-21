using UnityEngine;
using System;

/// <summary>
/// 输入管理器 - 统一处理玩家键盘和鼠标输入，专注于输入捕获和事件分发
/// 职责：检测输入、UI碰撞检测、优先级事件分发，不包含具体业务逻辑
/// </summary>
public class InputManager
{
    private static InputManager _instance;
    public static InputManager Instance => _instance ??= new InputManager();

    private bool _enableInput = true; // 是否启用输入

    // 移动输入事件
    public event Action<Vector3> OnMoveInput;
    
    // 优先级鼠标事件系统 - 保证处理顺序，不依赖订阅顺序
    public event Func<Vector3, bool> OnLeftClickHighPriority;  // 高优先级（UI、建筑放置）- 返回true消费事件
    public event Action<Vector3> OnLeftClickLowPriority;       // 低优先级（普通交互）
    public event Action OnRightClick;                          // 右键点击
    
    // 攻击输入事件
    public event Action<Vector3> OnAttackClick;                // 攻击点击事件（点击怪物攻击）
    
    // 长按攻击事件
    public event Action<bool> OnAttackHold;                    // 长按攻击事件（true=按下, false=抬起）
    
    // 按键输入事件
    public Action OnUseEquipInput;
    public Action<int> OnEquipShortcutInput;
    
    // 长按检测相关
    private bool _isHoldingAttack = false;
    private float _holdStartTime = 0f;
    private bool _hasTriggeredHold = false; // 是否已经触发过长按事件
    private const float HOLD_THRESHOLD = 0.15f; // 长按阈值：0.15秒

    private InputManager() { }

    // 供GameMain调用的更新方法
    public void Update()
    {
        if (!_enableInput) return;
        
        HandleMovementInput();
        HandleMouseInput();
        HandleKeyboardInput();
    }

    // 处理移动输入（WASD键）
    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        OnMoveInput?.Invoke(moveDirection);
    }

    // 处理鼠标输入
    private void HandleMouseInput()
    {
        // 鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 开始长按检测
            _isHoldingAttack = true;
            _hasTriggeredHold = false;
            _holdStartTime = Time.time;
            
            // 获取鼠标世界坐标
            Vector3 mouseWorldPos = Camera.main ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
            
            // 处理点击事件（保持原有逻辑）
            HandleLeftClick(mouseWorldPos);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // 结束长按检测
            if (_isHoldingAttack || _hasTriggeredHold)
            {
                _isHoldingAttack = false;
                _hasTriggeredHold = false;
                OnAttackHold?.Invoke(false); // 发布长按结束事件
            }
        }
        else if (Input.GetMouseButton(0) && _isHoldingAttack && !_hasTriggeredHold)
        {
            // 检查是否达到长按阈值
            if (Time.time - _holdStartTime >= HOLD_THRESHOLD)
            {
                // 发布长按开始事件（只发布一次）
                OnAttackHold?.Invoke(true);
                _hasTriggeredHold = true; // 标记已触发长按事件
            }
        }
        
        // 鼠标右键点击处理
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
        
        // 处理鼠标悬停
        HandleMouseHover();
    }

    // 处理鼠标左键点击
    private void HandleLeftClick(Vector3 mouseWorldPos)
    {
        // 使用 InputUtils 检测是否点击UI
        if (InputUtils.IsPointerOverUI())
        {
                    // 检查调试模型的开关状态，决定是否打印UI路径信息
        if (DebugModel.Instance.IsUIPathPrintEnabled)
        {
            InputUtils.PrintClickedUIPath();
        }
            return;
        }

        // 点击了非UI区域，获取世界坐标和碰撞对象
        bool eventConsumed = false;
        
        if (InputUtils.GetMouseWorldHit(out RaycastHit hit))
        {
            // 1. 先处理高优先级事件（TouchView建筑放置、UI交互等）
            if (OnLeftClickHighPriority != null)
            {
                foreach (Func<Vector3, bool> handler in OnLeftClickHighPriority.GetInvocationList())
                {
                    if (handler(mouseWorldPos))
                    {
                        eventConsumed = true;
                        break; // 高优先级事件被消费，停止所有后续处理
                    }
                }
            }
            
            // 2. 如果高优先级事件未消费，检查是否点击了可交互对象
            if (!eventConsumed)
            {
                var clickable = hit.collider.GetComponent<IClickable>();
                if (clickable != null && clickable.CanInteract)
                {
                    // 直接让被点击的对象处理交互
                    clickable.OnClick(hit.point);
                    eventConsumed = true; // 交互事件被消费，不再传递给移动
                }
            }
        }
        else
        {
            // 即使没有碰撞到物体，也计算世界坐标用于移动
            // mouseWorldPos = Camera.main ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero; // This line is now redundant
            
            // 处理高优先级事件
            if (OnLeftClickHighPriority != null)
            {
                foreach (Func<Vector3, bool> handler in OnLeftClickHighPriority.GetInvocationList())
                {
                    if (handler(mouseWorldPos))
                    {
                        eventConsumed = true;
                        break;
                    }
                }
            }
        }
        
        // 3. 只有在事件未被消费时，才处理低优先级事件（攻击等）
        if (!eventConsumed)
        {
            OnLeftClickLowPriority?.Invoke(mouseWorldPos);
            OnAttackClick?.Invoke(mouseWorldPos); // 攻击事件（由具体系统判断是否为攻击）
        }

        // 发布点击非UI区域事件
        EventManager.Instance.Publish(new ClickOutsideUIEvent(mouseWorldPos));
    }

    // 处理鼠标右键点击
    private void HandleRightClick()
    {
        // 发布通用右键点击事件
        OnRightClick?.Invoke();
        
        // 注意：业务逻辑已移除，符合单一职责原则
        // 如需处理右键取消选中物品，请在相关业务组件中订阅OnRightClick事件
    }

    // 处理键盘按键输入
    private void HandleKeyboardInput()
    {
        // 使用装备 - 空格键
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnUseEquipInput?.Invoke();
        }
    }

    // 启用或禁用输入
    public void SetInputEnabled(bool enable)
    {
        _enableInput = enable;
    }

    // 获取当前输入状态
    public bool IsInputEnabled => _enableInput;
    
    // 处理鼠标悬停检测
    private void HandleMouseHover()
    {
        // 如果鼠标在UI上，发布离开悬停事件
        // if (InputUtils.IsPointerOverUI())
        // {
        //     EventManager.Instance.Publish(new MouseHoverExitEvent());
        //     return;
        // }
        
        // 射线检测鼠标悬停的对象
        if (InputUtils.GetMouseWorldHit(out RaycastHit hit))
        {
            // 发布悬停事件
            EventManager.Instance.Publish(new MouseHoverEvent(hit.collider.gameObject, hit.point));
        }
        else
        {
            // 没有检测到对象，发布离开悬停事件
            EventManager.Instance.Publish(new MouseHoverExitEvent());
        }
    }
} 