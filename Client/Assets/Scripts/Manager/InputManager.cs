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
    
    // 鼠标点击移动事件
    public event Action<Vector3> OnMouseClickMove;
    
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
        // 鼠标左键点击移动
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClickMove();
        }
    }

    // 处理鼠标点击移动
    private void HandleMouseClickMove()
    {
        // 使用 InputUtils 检测是否点击UI
        if (InputUtils.IsPointerOverUI())
        {
            // 检查调试管理器的开关状态，决定是否打印UI路径信息
            if (DebugManager.Instance.IsUIPathPrintEnabled)
            {
                InputUtils.PrintClickedUIPath();
            }
            return;
        }

        // 点击了非UI区域，发布事件通知其他组件
        Vector3 mouseWorldPos = Vector3.zero;
        if (InputUtils.GetMouseWorldHit(out RaycastHit hit))
        {
            mouseWorldPos = hit.point;
            OnMouseClickMove?.Invoke(hit.point);
        }
        else
        {
            // 即使没有碰撞到物体，也要发布点击外部UI事件
            mouseWorldPos = Camera.main ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
        }

        // 发布点击非UI区域事件
        EventManager.Instance.Publish(new ClickOutsideUIEvent(mouseWorldPos));
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
} 