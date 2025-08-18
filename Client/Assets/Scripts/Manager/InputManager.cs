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
    public event Action<bool> OnAttackHold;                    // 攻击长按事件（开始/结束连续攻击）
    
    // 按键输入事件
    public event Action OnUseEquipInput;
    public event Action<int> OnEquipShortcutInput;

    // 长按攻击检测相关
    private bool _isHoldingAttack = false;
    private float _holdStartTime = 0f;
    private const float _holdThreshold = 0.3f; // 长按阈值（秒）

    private InputManager() { }

    // 供GameMain调用的更新方法
    public void Update()
    {
        if (!_enableInput) return;
        
        HandleMovementInput();
        HandleMouseInput();
        HandleKeyboardInput();
        HandleAttackHold();
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
            HandleLeftClick();
            
            // 开始长按检测
            _isHoldingAttack = true;
            _holdStartTime = Time.time;
        }
        
        // 鼠标左键抬起
        if (Input.GetMouseButtonUp(0))
        {
            // 结束长按检测
            if (_isHoldingAttack)
            {
                _isHoldingAttack = false;
                OnAttackHold?.Invoke(false); // 结束连续攻击
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
    private void HandleLeftClick()
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
        Vector3 mouseWorldPos = Vector3.zero;
        bool eventConsumed = false;
        
        if (InputUtils.GetMouseWorldHit(out RaycastHit hit))
        {
            mouseWorldPos = hit.point;
            
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
            mouseWorldPos = Camera.main ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
            
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

        // 装备快捷键 - Q键
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnEquipShortcutInput?.Invoke(30001);
        }
        // 装备快捷键 - E键
        else if (Input.GetKeyDown(KeyCode.E))
        {
            OnEquipShortcutInput?.Invoke(30002);
        }
    }

    // 启用或禁用输入
    public void SetInputEnabled(bool enable)
    {
        _enableInput = enable;
    }

    // 获取当前输入状态
    public bool IsInputEnabled => _enableInput;
    
    // 处理长按攻击检测
    private void HandleAttackHold()
    {
        if (_isHoldingAttack && Time.time - _holdStartTime >= _holdThreshold)
        {
            // 达到长按阈值，开始连续攻击
            OnAttackHold?.Invoke(true);
            _isHoldingAttack = false; // 避免重复触发
        }
    }
    

    
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