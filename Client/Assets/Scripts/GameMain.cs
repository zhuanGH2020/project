using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    private static readonly GameMain m_Instance = new GameMain();
    public static GameMain Instance { get { return m_Instance; } }

    // Start is called before the first frame update
    void Start()
    {
        ConfigExample.Example();
        ConfigExample.AdvancedExample();
        ConfigExample.ValidationExample();

        // 初始化对象管理器 - 确保在其他系统之前
        _ = ObjectManager.Instance; // Trigger lazy initialization
        Debug.Log("[GameMain] ObjectManager initialized");

        // 初始化各个Model - 按依赖顺序初始化
        var inputManager = InputManager.Instance;
        var clockModel = ClockModel.Instance;
        var packageModel = PackageModel.Instance;
        var mapModel = MapModel.Instance;
        
        // 初始化交互管理器 - 确保交互系统可用
        if (InteractionManager.Instance == null)
        {
            var interactionManagerGO = new GameObject("InteractionManager");
            interactionManagerGO.AddComponent<InteractionManager>();
        }
        
        // 初始化地图管理器 - 确保地图生成系统可用
        var mapManager = MapManager.Instance;
        
        // 初始化对话管理器 - 确保对话系统可用
        var dialogManager = DialogManager.Instance;
        
        // 初始化存档模型 - 确保在所有数据Model之后
        var saveModel = SaveModel.Instance;
        saveModel.Initialize();
        
        Debug.Log("[GameMain] All systems initialized");
    }

    // Update is called once per frame
    void Update()
    {
        // 驱动需要更新的Model
        InputManager.Instance.Update();
        ClockModel.Instance.UpdateTime();
        MapManager.Instance.UpdateSpawning();
        
        // 驱动存档模型（处理自动保存）
        SaveModel.Instance.Update();
        
        // 驱动对话管理器
        DialogManager.Instance.Update();
        
        // 定期清理ObjectManager中的空引用（每10秒一次）
        if (Time.time % 10f < Time.deltaTime)
        {
            ObjectManager.Instance.CleanupNullReferences();
        }
    }
    
    void OnDestroy()
    {
        SaveModel.Instance.Cleanup();
        
        // 清理地图管理器
        MapManager.Instance.Cleanup();
        
        // 清理对话管理器
        DialogManager.Instance.Cleanup();
        
        // 清理对象管理器
        if (ObjectManager.HasInstance)
        {
            ObjectManager.Instance.ClearAll();
        }
    }
}
