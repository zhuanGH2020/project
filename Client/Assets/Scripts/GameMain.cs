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

        // 初始化各个Model - 按依赖顺序初始化
        var clockModel = ClockModel.Instance;
        var packageModel = PackageModel.Instance;
        
        // 初始化存档模型 - 确保在所有数据Model之后
        var saveModel = SaveModel.Instance;
        saveModel.Initialize();
        
        Debug.Log("[GameMain] All systems initialized");
    }

    // Update is called once per frame
    void Update()
    {
        // 驱动需要更新的Model
        ClockModel.Instance.UpdateTime();
        
        // 驱动存档模型（处理自动保存）
        SaveModel.Instance.Update();
    }
    
    void OnDestroy()
    {
        SaveModel.Instance.Cleanup();
    }
}
