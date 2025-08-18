using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 临时：配置系统示例代码
        ConfigExample.Example();
        ConfigExample.AdvancedExample();
        ConfigExample.ValidationExample();

        InitializeSystems();
    }

    void Update()
    {
        UpdateSystems();
        PeriodicCleanup();
    }
    
    void OnDestroy()
    {
        CleanupSystems();
    }

    /// <summary>
    /// 初始化所有系统 - 按依赖顺序执行
    /// </summary>
    private void InitializeSystems()
    {
        // === 基础系统初始化（无依赖） ===
        _ = ObjectManager.Instance;
        _ = InputManager.Instance;
        _ = ClockModel.Instance;
        _ = PackageModel.Instance;
        _ = MapModel.Instance;
        _ = MapManager.Instance;
        _ = DialogManager.Instance;
        _ = UIManager.Instance;
        
        // === 依赖系统初始化（需要其他系统支持） ===
        InteractionManager.Instance.Initialize();
        CombatInputManager.Instance.Initialize();
        SaveModel.Instance.Initialize();
    }

    /// <summary>
    /// 更新所有系统 - 按优先级顺序执行
    /// </summary>
    private void UpdateSystems()
    {
        // 输入系统 - 最高优先级
        InputManager.Instance.Update();
        
        // 游戏逻辑系统
        ClockModel.Instance.UpdateTime();
        MapManager.Instance.UpdateSpawning();
        InteractionManager.Instance.Update();
        
        // 数据持久化系统
        SaveModel.Instance.Update();
        
        // UI系统 - 最后更新
        DialogManager.Instance.Update();
        UIManager.Instance.Update();
    }

    /// <summary>
    /// 定期清理任务
    /// </summary>
    private void PeriodicCleanup()
    {
        // 每10秒清理一次空引用
        if (Time.time % 10f < Time.deltaTime)
        {
            ObjectManager.Instance.CleanupNullReferences();
        }
    }

    /// <summary>
    /// 清理所有系统 - 按反向依赖顺序执行
    /// </summary>
    private void CleanupSystems()
    {
        // 先清理依赖系统
        SaveModel.Instance.Cleanup();
        CombatInputManager.Instance.Cleanup();
        InteractionManager.Instance.Cleanup();
        
        // 再清理基础系统
        MapManager.Instance.Cleanup();
        DialogManager.Instance.Cleanup();
        UIManager.Instance.Cleanup();
        
        // 最后清理对象管理器
        if (ObjectManager.HasInstance)
        {
            ObjectManager.Instance.ClearAll();
        }
    }
}
