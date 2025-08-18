using UnityEngine;
using System.Collections;

/// <summary>
/// 烹饪锅组件 - 处理玩家与锅的交互
/// 需要挂载到锅的GameObject上
/// </summary>
public class CookingPot : Building
{
    private float _interactionRange = 3f;  // 交互范围    


    protected override void Awake()
    {
        base.Awake();
        // 确保对象类型正确设置为Building（继承自Building已设置，这里可选）
        SetObjectType(ObjectType.Building);
    }

    private void Start()
    {
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

    // 范围检查由InteractionManager统一处理，CookingPot不需要自己检查

    /// <summary>
    /// 重写点击处理 - 触发交互事件让InteractionManager处理寻路
    /// </summary>
    public override void OnClick(Vector3 clickPosition)
    {
        if (!CanInteract) return;

        // 触发交互事件，让InteractionManager处理寻路
        EventManager.Instance.Publish(new ObjectInteractionEvent(this, clickPosition));
    }

    /// <summary>
    /// 处理按键输入（E键对应30002装备ID）
    /// 由InteractionManager处理范围检查，这里检查玩家是否在交互范围内
    /// </summary>
    private void OnEquipShortcutInput(int equipId)
    {
        if (equipId == 30002) // E键对应30002
        {
            // 检查玩家是否在交互范围内
            var player = Player.Instance;
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= _interactionRange)
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
        }
    }

    /// <summary>
    /// 重写交互逻辑 - 打开烹饪界面
    /// 此方法由InteractionManager在玩家到达后调用，或玩家在范围内时直接调用
    /// </summary>
    public override void OnInteract(Vector3 clickPosition)
    {
        base.OnInteract(clickPosition);
        OpenCookingUI();
    }

    /// <summary>
    /// 玩家进入烹饪锅交互范围时调用
    /// </summary>
    public override void OnEnterInteractionRange()
    {
        base.OnEnterInteractionRange();
        
        // 发布进入交互范围事件，用于显示UI提示
        EventManager.Instance.Publish(new CookingInteractionEvent(true, this));
    }

    /// <summary>
    /// 玩家离开烹饪锅交互范围时调用
    /// </summary>
    public override void OnLeaveInteractionRange()
    {
        base.OnLeaveInteractionRange();
        
        // 发布离开交互范围事件，用于隐藏UI提示
        EventManager.Instance.Publish(new CookingInteractionEvent(false, this));
        
        // 关闭烹饪界面
        CookingModel.Instance.CloseCookingUI();
    }

    /// <summary>
    /// 打开烹饪界面
    /// </summary>
    private void OpenCookingUI()
    {
        CookingModel.Instance.OpenCookingUI(transform.position);
    }

    public override float GetInteractionRange()
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