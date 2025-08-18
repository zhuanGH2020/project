using UnityEngine;

/// <summary>
/// 战斗输入管理器 - 专门处理攻击相关的输入逻辑
/// 职责：监听攻击输入事件、判断攻击目标、发布战斗事件
/// </summary>
public class CombatInputManager
{
    private static CombatInputManager _instance;
    public static CombatInputManager Instance => _instance ??= new CombatInputManager();

    private bool _initialized;
    
    // 攻击事件
    public System.Action<Monster, Vector3> OnMonsterAttack; // 攻击怪物事件
    public System.Action<bool> OnContinuousAttack; // 连续攻击事件

    private CombatInputManager() { }

    /// <summary>
    /// 初始化战斗输入管理器
    /// </summary>
    public void Initialize()
    {
        if (_initialized) return;
        
        SubscribeToInputEvents();
        _initialized = true;
        
        Debug.Log("[CombatInputManager] Initialized");
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Cleanup()
    {
        UnsubscribeFromInputEvents();
        _initialized = false;
        
        Debug.Log("[CombatInputManager] Cleaned up");
    }

    /// <summary>
    /// 订阅输入管理器的事件
    /// </summary>
    private void SubscribeToInputEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnAttackClick += HandleAttackClick;
            InputManager.Instance.OnAttackHold += HandleAttackHold;
        }
    }

    /// <summary>
    /// 取消订阅输入事件
    /// </summary>
    private void UnsubscribeFromInputEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnAttackClick -= HandleAttackClick;
            InputManager.Instance.OnAttackHold -= HandleAttackHold;
        }
    }

    /// <summary>
    /// 处理攻击点击事件 - 判断是否点击了可攻击的怪物
    /// </summary>
    private void HandleAttackClick(Vector3 clickPosition)
    {
        if (!_initialized) return;

        // 检查点击位置是否有可攻击目标
        var monster = GetAttackTarget(clickPosition);
        if (monster != null)
        {
            OnMonsterAttack?.Invoke(monster, clickPosition);
            Debug.Log($"[CombatInputManager] Attack target: {monster.name}");
        }
    }

    /// <summary>
    /// 处理攻击长按事件
    /// </summary>
    private void HandleAttackHold(bool isHolding)
    {
        if (!_initialized) return;

        OnContinuousAttack?.Invoke(isHolding);
        Debug.Log($"[CombatInputManager] Continuous attack: {isHolding}");
    }

    /// <summary>
    /// 获取点击位置的攻击目标
    /// </summary>
    private Monster GetAttackTarget(Vector3 clickPosition)
    {
        if (InputUtils.GetMouseWorldHit(out RaycastHit hit))
        {
            // 检查是否点击了怪物
            var monster = hit.collider.GetComponent<Monster>();
            if (monster != null && monster.CurrentHealth > 0)
            {
                return monster;
            }
        }
        return null;
    }

    /// <summary>
    /// 检查是否正在处理战斗输入
    /// </summary>
    public bool IsInitialized => _initialized;
} 