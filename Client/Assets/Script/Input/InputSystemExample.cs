using UnityEngine;
using InputSystem;

/// <summary>
/// 输入系统使用示例
/// 展示如何正确配置和使用完整的GameObject输入控制系统
/// </summary>
public class InputSystemExample : MonoBehaviour
{
    #region Serialized Fields
    [Header("=== 配置文件 ===")]
    [SerializeField] private InputConfiguration inputConfig;
    
    [Header("=== 目标对象 ===")]
    [SerializeField] private Transform controlledObject;
    [SerializeField] private Camera followCamera;
    
    [Header("=== 示例设置 ===")]
    [SerializeField] private bool enableAutoSetup = true;
    [SerializeField] private bool showExampleUI = true;
    [SerializeField] private bool logSystemEvents = false;
    #endregion

    #region Private Fields
    // 系统组件
    private InputManager inputManager;
    private KeyboardInputProvider keyboardProvider;
    private TransformMovementHandler movementHandler;
    private InputRecorder recorder;
    private InputDebugVisualizer visualizer;
    
    // 示例状态
    private bool systemInitialized = false;
    private float exampleStartTime;
    
    // UI相关
    private bool showUI = true;
    private Rect uiRect = new Rect(10, 10, 300, 300);
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        exampleStartTime = Time.time;
        
        if (enableAutoSetup)
        {
            SetupInputSystem();
        }
    }

    private void Start()
    {
        if (enableAutoSetup && !systemInitialized)
        {
            InitializeExample();
        }
        
        // 订阅系统事件
        SubscribeToSystemEvents();
    }

    private void Update()
    {
        // 更新跟随摄像机
        UpdateFollowCamera();
    }

    private void OnGUI()
    {
        if (showExampleUI && showUI)
        {
            DrawExampleUI();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromSystemEvents();
    }
    #endregion

    #region Setup and Initialization
    /// <summary>
    /// 设置输入系统
    /// </summary>
    [ContextMenu("Setup Input System")]
    public void SetupInputSystem()
    {
        Debug.Log("[InputSystemExample] 开始设置输入系统...");

        try
        {
            // 创建或获取InputManager
            SetupInputManager();
            
            // 设置输入提供者
            SetupInputProvider();
            
            // 设置移动处理器
            SetupMovementHandler();
            
            // 设置录制器
            SetupRecorder();
            
            // 设置调试可视化器
            SetupVisualizer();
            
            // 配置系统
            ConfigureSystem();
            
            systemInitialized = true;
            Debug.Log("[InputSystemExample] 输入系统设置完成!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InputSystemExample] 设置输入系统失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 设置输入管理器
    /// </summary>
    private void SetupInputManager()
    {
        // 使用单例获取或创建InputManager
        inputManager = InputManager.Instance;
        
        // 如果InputManager不在当前GameObject上，将其移动过来
        if (inputManager.gameObject != gameObject)
        {
            // 创建新的InputManager组件在当前对象上
            var localInputManager = GetComponent<InputManager>();
            if (localInputManager == null)
            {
                localInputManager = gameObject.AddComponent<InputManager>();
            }
            inputManager = localInputManager;
        }
    }

    /// <summary>
    /// 设置输入提供者
    /// </summary>
    private void SetupInputProvider()
    {
        keyboardProvider = GetComponent<KeyboardInputProvider>();
        if (keyboardProvider == null)
        {
            keyboardProvider = gameObject.AddComponent<KeyboardInputProvider>();
        }
        
        // 设置配置
        if (inputConfig != null)
        {
            keyboardProvider.SetConfiguration(inputConfig);
        }
        
        Debug.Log("[InputSystemExample] 键盘输入提供者设置完成");
    }

    /// <summary>
    /// 设置移动处理器
    /// </summary>
    private void SetupMovementHandler()
    {
        // 确定控制目标
        Transform targetTransform = controlledObject != null ? controlledObject : transform;
        
        // 在目标对象上设置移动处理器
        GameObject targetObject = targetTransform.gameObject;
        movementHandler = targetObject.GetComponent<TransformMovementHandler>();
        
        if (movementHandler == null)
        {
            movementHandler = targetObject.AddComponent<TransformMovementHandler>();
        }
        
        // 设置配置
        if (inputConfig != null)
        {
            movementHandler.SetConfiguration(inputConfig);
        }
        
        Debug.Log($"[InputSystemExample] 移动处理器设置完成，目标: {targetObject.name}");
    }

    /// <summary>
    /// 设置录制器
    /// </summary>
    private void SetupRecorder()
    {
        recorder = GetComponent<InputRecorder>();
        if (recorder == null)
        {
            recorder = gameObject.AddComponent<InputRecorder>();
        }
        
        // 设置配置
        if (inputConfig != null)
        {
            recorder.SetConfiguration(inputConfig);
        }
        
        Debug.Log("[InputSystemExample] 输入录制器设置完成");
    }

    /// <summary>
    /// 设置调试可视化器
    /// </summary>
    private void SetupVisualizer()
    {
        visualizer = GetComponent<InputDebugVisualizer>();
        if (visualizer == null)
        {
            visualizer = gameObject.AddComponent<InputDebugVisualizer>();
        }
        
        // 设置配置
        if (inputConfig != null)
        {
            visualizer.SetConfiguration(inputConfig);
        }
        
        Debug.Log("[InputSystemExample] 调试可视化器设置完成");
    }

    /// <summary>
    /// 配置系统
    /// </summary>
    private void ConfigureSystem()
    {
        if (inputManager != null && inputConfig != null)
        {
            inputManager.SetConfiguration(inputConfig);
        }
        
        // 注册组件到管理器
        if (inputManager != null)
        {
            inputManager.RegisterInputProvider(keyboardProvider);
            inputManager.RegisterMovementHandler(movementHandler);
        }
    }

    /// <summary>
    /// 初始化示例
    /// </summary>
    private void InitializeExample()
    {
        // 确保控制对象存在
        if (controlledObject == null)
        {
            controlledObject = transform;
        }
        
        // 设置摄像机跟随
        if (followCamera == null)
        {
            followCamera = Camera.main;
        }
        
        Debug.Log("[InputSystemExample] 示例初始化完成");
    }
    #endregion

    #region Event Handling
    /// <summary>
    /// 订阅系统事件
    /// </summary>
    private void SubscribeToSystemEvents()
    {
        if (logSystemEvents)
        {
            // 订阅InputManager事件
            InputManager.OnInputSystemInitialized += OnInputSystemInitialized;
            InputManager.OnInputProviderStateChanged += OnInputProviderStateChanged;
            InputManager.OnMovementHandlerStateChanged += OnMovementHandlerStateChanged;
            
            // 订阅录制器事件
            if (recorder != null)
            {
                recorder.OnRecordingStarted += OnRecordingStarted;
                recorder.OnRecordingStopped += OnRecordingStopped;
                recorder.OnPlaybackStarted += OnPlaybackStarted;
                recorder.OnPlaybackEnded += OnPlaybackEnded;
            }
        }
    }

    /// <summary>
    /// 取消订阅系统事件
    /// </summary>
    private void UnsubscribeFromSystemEvents()
    {
        if (logSystemEvents)
        {
            InputManager.OnInputSystemInitialized -= OnInputSystemInitialized;
            InputManager.OnInputProviderStateChanged -= OnInputProviderStateChanged;
            InputManager.OnMovementHandlerStateChanged -= OnMovementHandlerStateChanged;
            
            if (recorder != null)
            {
                recorder.OnRecordingStarted -= OnRecordingStarted;
                recorder.OnRecordingStopped -= OnRecordingStopped;
                recorder.OnPlaybackStarted -= OnPlaybackStarted;
                recorder.OnPlaybackEnded -= OnPlaybackEnded;
            }
        }
    }

    // 事件处理方法
    private void OnInputSystemInitialized()
    {
        Debug.Log("[InputSystemExample] 输入系统初始化完成");
    }

    private void OnInputProviderStateChanged(IInputProvider provider, bool enabled)
    {
        Debug.Log($"[InputSystemExample] 输入提供者状态改变: {provider.GetType().Name} - {(enabled ? "启用" : "禁用")}");
    }

    private void OnMovementHandlerStateChanged(IMovementHandler handler, bool enabled)
    {
        Debug.Log($"[InputSystemExample] 移动处理器状态改变: {handler.GetType().Name} - {(enabled ? "启用" : "禁用")}");
    }

    private void OnRecordingStarted()
    {
        Debug.Log("[InputSystemExample] 录制开始");
    }

    private void OnRecordingStopped(InputRecording recording)
    {
        Debug.Log($"[InputSystemExample] 录制结束: {recording.name}");
    }

    private void OnPlaybackStarted(InputRecording recording)
    {
        Debug.Log($"[InputSystemExample] 回放开始: {recording.name}");
    }

    private void OnPlaybackEnded()
    {
        Debug.Log("[InputSystemExample] 回放结束");
    }
    #endregion

    #region Camera Update
    /// <summary>
    /// 更新跟随摄像机
    /// </summary>
    private void UpdateFollowCamera()
    {
        if (followCamera != null && controlledObject != null)
        {
            Vector3 targetPosition = controlledObject.position + Vector3.back * 10f + Vector3.up * 5f;
            followCamera.transform.position = Vector3.Lerp(
                followCamera.transform.position, 
                targetPosition, 
                Time.deltaTime * 2f
            );
            
            followCamera.transform.LookAt(controlledObject);
        }
    }
    #endregion

    #region UI Drawing
    /// <summary>
    /// 绘制示例UI
    /// </summary>
    private void DrawExampleUI()
    {
        uiRect = GUILayout.Window(0, uiRect, DrawWindow, "输入系统示例控制面板");
    }

    /// <summary>
    /// 绘制窗口内容
    /// </summary>
    private void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical();

        // 系统状态
        DrawSystemStatus();
        
        GUILayout.Space(10);
        
        // 控制按钮
        DrawControlButtons();
        
        GUILayout.Space(10);
        
        // 系统信息
        DrawSystemInfo();

        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }

    /// <summary>
    /// 绘制系统状态
    /// </summary>
    private void DrawSystemStatus()
    {
        GUILayout.Label("=== 系统状态 ===", GUI.skin.box);
        
        if (inputManager != null)
        {
            var status = inputManager.GetSystemStatus();
            GUILayout.Label($"系统初始化: {status.isInitialized}");
            GUILayout.Label($"系统启用: {status.systemEnabled}");
            GUILayout.Label($"输入提供者: {status.inputProviderCount}");
            GUILayout.Label($"移动处理器: {status.movementHandlerCount}");
            GUILayout.Label($"平均FPS: {status.averageFPS:F1}");
        }
        else
        {
            GUILayout.Label("输入管理器未初始化", GUI.skin.box);
        }
    }

    /// <summary>
    /// 绘制控制按钮
    /// </summary>
    private void DrawControlButtons()
    {
        GUILayout.Label("=== 系统控制 ===", GUI.skin.box);
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("重置系统"))
        {
            ResetSystem();
        }
        
        if (GUILayout.Button("暂停系统"))
        {
            PauseSystem();
        }
        
        if (GUILayout.Button("恢复系统"))
        {
            ResumeSystem();
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("切换可视化"))
        {
            ToggleVisualization();
        }
        
        if (GUILayout.Button("清空历史"))
        {
            ClearInputHistory();
        }
        
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 绘制系统信息
    /// </summary>
    private void DrawSystemInfo()
    {
        GUILayout.Label("=== 控制说明 ===", GUI.skin.box);
        GUILayout.Label("WASD - 移动控制");
        GUILayout.Label("Ctrl+R - 重置系统");
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 重置系统
    /// </summary>
    public void ResetSystem()
    {
        if (inputManager != null)
        {
            inputManager.ResetSystem();
        }
        Debug.Log("[InputSystemExample] 系统已重置");
    }

    /// <summary>
    /// 暂停系统
    /// </summary>
    public void PauseSystem()
    {
        if (inputManager != null)
        {
            inputManager.PauseSystem();
        }
        Debug.Log("[InputSystemExample] 系统已暂停");
    }

    /// <summary>
    /// 恢复系统
    /// </summary>
    public void ResumeSystem()
    {
        if (inputManager != null)
        {
            inputManager.ResumeSystem();
        }
        Debug.Log("[InputSystemExample] 系统已恢复");
    }

    /// <summary>
    /// 切换可视化
    /// </summary>
    public void ToggleVisualization()
    {
        if (visualizer != null)
        {
            visualizer.ToggleVisualization();
        }
    }

    /// <summary>
    /// 清空输入历史
    /// </summary>
    public void ClearInputHistory()
    {
        if (visualizer != null)
        {
            visualizer.ClearInputHistory();
        }
    }

    /// <summary>
    /// 获取系统组件（用于外部访问）
    /// </summary>
    public T GetSystemComponent<T>() where T : Component
    {
        if (typeof(T) == typeof(InputManager)) return inputManager as T;
        if (typeof(T) == typeof(KeyboardInputProvider)) return keyboardProvider as T;
        if (typeof(T) == typeof(TransformMovementHandler)) return movementHandler as T;
        if (typeof(T) == typeof(InputRecorder)) return recorder as T;
        if (typeof(T) == typeof(InputDebugVisualizer)) return visualizer as T;
        
        return null;
    }
    #endregion

    #region Editor Utilities
#if UNITY_EDITOR
    [ContextMenu("Create Input Configuration")]
    private void CreateInputConfiguration()
    {
        if (inputConfig == null)
        {
            inputConfig = ScriptableObject.CreateInstance<InputConfiguration>();
            UnityEditor.AssetDatabase.CreateAsset(inputConfig, "Assets/DefaultInputConfiguration.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("[InputSystemExample] 输入配置文件已创建");
        }
    }

    [ContextMenu("Validate System Setup")]
    private void ValidateSystemSetup()
    {
        Debug.Log("=== 输入系统验证 ===");
        
        if (inputConfig == null)
        {
            Debug.LogWarning("输入配置文件未设置");
        }
        else
        {
            Debug.Log($"输入配置: {inputConfig.name}");
        }
        
        if (controlledObject == null)
        {
            Debug.LogWarning("控制对象未设置，将使用当前对象");
        }
        
        if (inputManager == null)
        {
            Debug.LogWarning("输入管理器未初始化");
        }
        else
        {
            Debug.Log("输入管理器: OK");
        }
        
        Debug.Log("验证完成");
    }
#endif
    #endregion
} 