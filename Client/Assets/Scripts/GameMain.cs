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

        // 初始化各个Model
        var clockModel = ClockModel.Instance;
        var packageModel = PackageModel.Instance;
        var saveManager = SaveManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        // 驱动需要更新的Model
        ClockModel.Instance.UpdateTime();
    }
}
