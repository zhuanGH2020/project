using UnityEngine;
using System;

// 输入管理器 - 统一处理玩家键盘和鼠标输入
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
    public event Action<Vector3> OnMouseClickMove;             // 低优先级移动（Player使用）
    
    // 按键输入事件
    public event Action OnUseEquipInput;
    public event Action<int> OnEquipShortcutInput;

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
            HandleLeftClick();
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

        // 点击了非UI区域，获取世界坐标
        Vector3 mouseWorldPos = Vector3.zero;
        if (InputUtils.GetMouseWorldHit(out RaycastHit hit))
        {
            mouseWorldPos = hit.point;
        }
        else
        {
            // 即使没有碰撞到物体，也计算世界坐标
            mouseWorldPos = Camera.main ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
        }

        // 优先级事件处理系统 - 从源头控制时序问题
        // 1. 先处理高优先级事件（TouchView建筑放置、UI交互等）
        bool eventConsumed = false;
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
        
        // 2. 只有高优先级事件未被消费时，才处理低优先级事件（Player移动等）
        if (!eventConsumed)
        {
            OnLeftClickLowPriority?.Invoke(mouseWorldPos);
            OnMouseClickMove?.Invoke(mouseWorldPos); // Player移动事件
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