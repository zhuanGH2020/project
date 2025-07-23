using System;
using System.Collections.Generic;
using UnityEngine;

namespace InputSystem
{
    /// <summary>
    /// 输入管理器
    /// 作为整个输入系统的核心协调器，统一管理所有输入组件
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region Events
        /// <summary>
        /// 输入系统初始化完成事件
        /// </summary>
        public static event Action OnInputSystemInitialized;
        
        /// <summary>
        /// 输入提供者状态改变事件
        /// </summary>
        public static event Action<IInputProvider, bool> OnInputProviderStateChanged;
        
        /// <summary>
        /// 移动处理器状态改变事件
        /// </summary>
        public static event Action<IMovementHandler, bool> OnMovementHandlerStateChanged;
        #endregion

        #region Singleton
        private static InputManager instance;
        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<InputManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("InputManager");
                        instance = go.AddComponent<InputManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Serialized Fields
        [Header("=== 核心配置 ===")]
        [SerializeField] private InputConfiguration defaultConfig;
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool dontDestroyOnLoad = true;

        [Header("=== 组件引用 ===")]
        [SerializeField] private KeyboardInputProvider keyboardInputProvider;
        [SerializeField] private TransformMovementHandler movementHandler;

        [Header("=== 系统设置 ===")]
        [SerializeField] private bool enableInputRecording = false;
        [SerializeField] private bool enableDebugVisualization = true;
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private InputSystemMode systemMode = InputSystemMode.Normal;

        [Header("=== 调试选项 ===")]
        [SerializeField] private bool showSystemStatus = true;
        [SerializeField] private bool logInputEvents = false;
        [SerializeField] private bool enableSystemProfiling = false;
        #endregion

        #region Private Fields
        // 组件集合
        private readonly List<IInputProvider> inputProviders = new List<IInputProvider>();
        private readonly List<IMovementHandler> movementHandlers = new List<IMovementHandler>();
        
        // 系统状态
        private bool isInitialized = false;
        private bool systemEnabled = true;
        private InputConfiguration currentConfig;
        
        // 性能监控
        private float lastUpdateTime = 0f;
        private int frameCount = 0;
        private float averageFPS = 0f;
        
        // 输入历史
        private readonly Queue<InputEventData> inputHistory = new Queue<InputEventData>();
        private const int MAX_INPUT_HISTORY = 1000;
        
        // 快捷键系统
        private readonly Dictionary<string, ShortcutAction> shortcuts = new Dictionary<string, ShortcutAction>();
        private readonly List<KeyCode> pressedKeys = new List<KeyCode>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // 单例模式处理
            if (instance == null)
            {
                instance = this;
                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (instance != this)
            {
                Debug.LogWarning("[InputManager] 检测到重复的InputManager实例，销毁当前实例");
                Destroy(gameObject);
                return;
            }

            if (initializeOnAwake)
            {
                Initialize();
            }
        }

        private void Start()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        private void Update()
        {
            if (!isInitialized || !systemEnabled) return;

            UpdatePerformanceMonitoring();
            UpdateShortcutKeys();
            
            if (logInputEvents)
            {
                LogCurrentInputState();
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            SetSystemEnabled(!pauseStatus);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetSystemEnabled(hasFocus);
        }
        #endregion

        #region Initialization
        /// <summary>
        /// 初始化输入系统
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[InputManager] 输入系统已经初始化");
                return;
            }

            Debug.Log("[InputManager] 开始初始化输入系统...");

            try
            {
                // 设置默认配置
                SetConfiguration(defaultConfig);
                
                // 自动发现并注册组件
                DiscoverAndRegisterComponents();
                
                // 初始化子系统
                InitializeSubSystems();
                
                // 建立事件连接
                EstablishEventConnections();
                
                // 初始化快捷键系统
                InitializeShortcutSystem();
                
                // 标记为已初始化
                isInitialized = true;
                
                Debug.Log("[InputManager] 输入系统初始化完成");
                OnInputSystemInitialized?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InputManager] 输入系统初始化失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 自动发现并注册组件
        /// </summary>
        private void DiscoverAndRegisterComponents()
        {
            // 发现输入提供者
            if (keyboardInputProvider == null)
            {
                keyboardInputProvider = GetComponent<KeyboardInputProvider>();
                if (keyboardInputProvider == null)
                {
                    keyboardInputProvider = gameObject.AddComponent<KeyboardInputProvider>();
                }
            }

            // 发现移动处理器
            if (movementHandler == null)
            {
                movementHandler = GetComponent<TransformMovementHandler>();
                if (movementHandler == null)
                {
                    movementHandler = gameObject.AddComponent<TransformMovementHandler>();
                }
            }

            // 注册组件
            RegisterInputProvider(keyboardInputProvider);
            RegisterMovementHandler(movementHandler);
        }

        /// <summary>
        /// 初始化子系统
        /// </summary>
        private void InitializeSubSystems()
        {
            // 初始化所有输入提供者
            foreach (var provider in inputProviders)
            {
                provider.Initialize();
            }

            // 初始化所有移动处理器
            foreach (var handler in movementHandlers)
            {
                handler.Initialize();
            }
        }

        /// <summary>
        /// 建立事件连接
        /// </summary>
        private void EstablishEventConnections()
        {
            foreach (var provider in inputProviders)
            {
                // 连接移动输入事件
                provider.OnMovementInput += HandleMovementInput;
                provider.OnInputStart += HandleInputStart;
                provider.OnInputEnd += HandleInputEnd;
            }
        }

        /// <summary>
        /// 初始化快捷键系统
        /// </summary>
        private void InitializeShortcutSystem()
        {
            if (currentConfig != null && currentConfig.enableComboKeys)
            {
                RegisterDefaultShortcuts();
            }
        }
        #endregion

        #region Component Registration
        /// <summary>
        /// 注册输入提供者
        /// </summary>
        public void RegisterInputProvider(IInputProvider provider)
        {
            if (provider != null && !inputProviders.Contains(provider))
            {
                inputProviders.Add(provider);
                
                if (currentConfig != null && provider is KeyboardInputProvider keyboardProvider)
                {
                    keyboardProvider.SetConfiguration(currentConfig);
                }
                
                Debug.Log($"[InputManager] 注册输入提供者: {provider.GetType().Name}");
                OnInputProviderStateChanged?.Invoke(provider, true);
            }
        }

        /// <summary>
        /// 注销输入提供者
        /// </summary>
        public void UnregisterInputProvider(IInputProvider provider)
        {
            if (provider != null && inputProviders.Contains(provider))
            {
                inputProviders.Remove(provider);
                
                // 断开事件连接
                provider.OnMovementInput -= HandleMovementInput;
                provider.OnInputStart -= HandleInputStart;
                provider.OnInputEnd -= HandleInputEnd;
                
                Debug.Log($"[InputManager] 注销输入提供者: {provider.GetType().Name}");
                OnInputProviderStateChanged?.Invoke(provider, false);
            }
        }

        /// <summary>
        /// 注册移动处理器
        /// </summary>
        public void RegisterMovementHandler(IMovementHandler handler)
        {
            if (handler != null && !movementHandlers.Contains(handler))
            {
                movementHandlers.Add(handler);
                
                if (currentConfig != null && handler is TransformMovementHandler transformHandler)
                {
                    transformHandler.SetConfiguration(currentConfig);
                }
                
                Debug.Log($"[InputManager] 注册移动处理器: {handler.GetType().Name}");
                OnMovementHandlerStateChanged?.Invoke(handler, true);
            }
        }

        /// <summary>
        /// 注销移动处理器
        /// </summary>
        public void UnregisterMovementHandler(IMovementHandler handler)
        {
            if (handler != null && movementHandlers.Contains(handler))
            {
                movementHandlers.Remove(handler);
                
                Debug.Log($"[InputManager] 注销移动处理器: {handler.GetType().Name}");
                OnMovementHandlerStateChanged?.Invoke(handler, false);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 处理移动输入事件
        /// </summary>
        private void HandleMovementInput(Vector2 inputVector)
        {
            if (!systemEnabled) return;

            // 分发到所有移动处理器
            foreach (var handler in movementHandlers)
            {
                handler.HandleMovement(inputVector);
            }

            // 记录输入历史
            RecordInputEvent(new InputEventData
            {
                eventType = InputEventType.Movement,
                inputVector = inputVector,
                timestamp = Time.time
            });

            if (logInputEvents)
            {
                Debug.Log($"[InputManager] 移动输入: {inputVector}");
            }
        }

        /// <summary>
        /// 处理输入开始事件
        /// </summary>
        private void HandleInputStart()
        {
            if (!systemEnabled) return;

            RecordInputEvent(new InputEventData
            {
                eventType = InputEventType.InputStart,
                timestamp = Time.time
            });

            if (logInputEvents)
            {
                Debug.Log("[InputManager] 输入开始");
            }
        }

        /// <summary>
        /// 处理输入结束事件
        /// </summary>
        private void HandleInputEnd()
        {
            if (!systemEnabled) return;

            RecordInputEvent(new InputEventData
            {
                eventType = InputEventType.InputEnd,
                timestamp = Time.time
            });

            if (logInputEvents)
            {
                Debug.Log("[InputManager] 输入结束");
            }
        }
        #endregion

        #region Configuration Management
        /// <summary>
        /// 设置输入配置
        /// </summary>
        public void SetConfiguration(InputConfiguration config)
        {
            if (config == null)
            {
                Debug.LogError("[InputManager] 输入配置不能为空");
                return;
            }

            if (!config.ValidateConfiguration())
            {
                Debug.LogError("[InputManager] 输入配置验证失败");
                return;
            }

            currentConfig = config;

            // 更新所有组件的配置
            UpdateComponentConfigurations();

            Debug.Log("[InputManager] 输入配置已更新");
        }

        /// <summary>
        /// 更新组件配置
        /// </summary>
        private void UpdateComponentConfigurations()
        {
            // 更新输入提供者配置
            foreach (var provider in inputProviders)
            {
                if (provider is KeyboardInputProvider keyboardProvider)
                {
                    keyboardProvider.SetConfiguration(currentConfig);
                }
            }

            // 更新移动处理器配置
            foreach (var handler in movementHandlers)
            {
                if (handler is TransformMovementHandler transformHandler)
                {
                    transformHandler.SetConfiguration(currentConfig);
                }
            }
        }
        #endregion

        #region System Control
        /// <summary>
        /// 设置系统启用状态
        /// </summary>
        public void SetSystemEnabled(bool enabled)
        {
            systemEnabled = enabled;

            // 更新所有输入提供者状态
            foreach (var provider in inputProviders)
            {
                provider.IsEnabled = enabled;
            }

            // 更新所有移动处理器状态
            foreach (var handler in movementHandlers)
            {
                handler.SetMovementEnabled(enabled);
            }

            Debug.Log($"[InputManager] 输入系统 {(enabled ? "启用" : "禁用")}");
        }

        /// <summary>
        /// 暂停输入系统
        /// </summary>
        public void PauseSystem()
        {
            SetSystemEnabled(false);
        }

        /// <summary>
        /// 恢复输入系统
        /// </summary>
        public void ResumeSystem()
        {
            SetSystemEnabled(true);
        }

        /// <summary>
        /// 重置输入系统
        /// </summary>
        public void ResetSystem()
        {
            // 停止所有移动
            foreach (var handler in movementHandlers)
            {
                if (handler is TransformMovementHandler transformHandler)
                {
                    transformHandler.StopMovement();
                }
            }

            // 清空输入历史
            inputHistory.Clear();

            // 重新初始化
            if (isInitialized)
            {
                Initialize();
            }

            Debug.Log("[InputManager] 输入系统已重置");
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 记录输入事件
        /// </summary>
        private void RecordInputEvent(InputEventData eventData)
        {
            inputHistory.Enqueue(eventData);
            
            // 限制历史记录数量
            while (inputHistory.Count > MAX_INPUT_HISTORY)
            {
                inputHistory.Dequeue();
            }
        }

        /// <summary>
        /// 更新性能监控
        /// </summary>
        private void UpdatePerformanceMonitoring()
        {
            if (!enablePerformanceMonitoring) return;

            frameCount++;
            float deltaTime = Time.time - lastUpdateTime;
            
            if (deltaTime >= 1f)
            {
                averageFPS = frameCount / deltaTime;
                frameCount = 0;
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// 更新快捷键系统
        /// </summary>
        private void UpdateShortcutKeys()
        {
            if (currentConfig == null || !currentConfig.enableComboKeys) return;

            // 检测快捷键
            foreach (var shortcut in shortcuts.Values)
            {
                if (IsShortcutPressed(shortcut))
                {
                    shortcut.action?.Invoke();
                }
            }
        }

        /// <summary>
        /// 检查快捷键是否被按下
        /// </summary>
        private bool IsShortcutPressed(ShortcutAction shortcut)
        {
            bool primaryPressed = Input.GetKeyDown(shortcut.primaryKey);
            bool modifierPressed = shortcut.modifierKey == KeyCode.None || Input.GetKey(shortcut.modifierKey);
            
            return primaryPressed && modifierPressed;
        }

        /// <summary>
        /// 注册默认快捷键
        /// </summary>
        private void RegisterDefaultShortcuts()
        {
            // 仅注册系统重置快捷键
            RegisterShortcut("ResetSystem", KeyCode.R, KeyCode.LeftControl, ResetSystem);
        }

        /// <summary>
        /// 记录当前输入状态
        /// </summary>
        private void LogCurrentInputState()
        {
            if (keyboardInputProvider != null)
            {
                Vector2 input = keyboardInputProvider.CurrentInput;
                if (input.magnitude > 0.01f)
                {
                    Debug.Log($"[InputManager] 当前输入: {input}, 幅度: {input.magnitude:F3}");
                }
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void Cleanup()
        {
            // 清理事件订阅
            OnInputSystemInitialized = null;
            OnInputProviderStateChanged = null;
            OnMovementHandlerStateChanged = null;

            // 清理组件
            foreach (var provider in inputProviders)
            {
                provider?.Cleanup();
            }

            foreach (var handler in movementHandlers)
            {
                handler?.Cleanup();
            }

            // 清理其他资源
            inputHistory.Clear();
            shortcuts.Clear();
            pressedKeys.Clear();
        }
        #endregion

        #region Public API
        /// <summary>
        /// 注册快捷键
        /// </summary>
        public void RegisterShortcut(string name, KeyCode primaryKey, KeyCode modifierKey, Action action)
        {
            shortcuts[name] = new ShortcutAction
            {
                name = name,
                primaryKey = primaryKey,
                modifierKey = modifierKey,
                action = action
            };
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        public void UnregisterShortcut(string name)
        {
            shortcuts.Remove(name);
        }

        /// <summary>
        /// 获取系统状态
        /// </summary>
        public InputSystemStatus GetSystemStatus()
        {
            return new InputSystemStatus
            {
                isInitialized = this.isInitialized,
                systemEnabled = this.systemEnabled,
                inputProviderCount = inputProviders.Count,
                movementHandlerCount = movementHandlers.Count,
                averageFPS = this.averageFPS,
                inputHistoryCount = inputHistory.Count,
                currentConfig = this.currentConfig
            };
        }
        #endregion

        #region Debugging
        private void OnGUI()
        {
            if (showSystemStatus && currentConfig != null && currentConfig.enableDetailedDebugInfo)
            {
                DrawSystemStatus();
            }
        }

        private void DrawSystemStatus()
        {
            GUILayout.BeginArea(new Rect(10, 220, 300, 150));
            GUILayout.Label("=== 输入系统状态 ===");
            GUILayout.Label($"系统初始化: {isInitialized}");
            GUILayout.Label($"系统启用: {systemEnabled}");
            GUILayout.Label($"输入提供者: {inputProviders.Count}");
            GUILayout.Label($"移动处理器: {movementHandlers.Count}");
            GUILayout.Label($"平均FPS: {averageFPS:F1}");
            GUILayout.Label($"输入历史: {inputHistory.Count}");
            GUILayout.Label($"系统模式: {systemMode}");
            GUILayout.EndArea();
        }
        #endregion
    }

    #region Data Structures
    /// <summary>
    /// 输入事件数据
    /// </summary>
    public struct InputEventData
    {
        public InputEventType eventType;
        public Vector2 inputVector;
        public float timestamp;
    }

    /// <summary>
    /// 输入事件类型
    /// </summary>
    public enum InputEventType
    {
        Movement,
        InputStart,
        InputEnd
    }

    /// <summary>
    /// 快捷键动作
    /// </summary>
    public class ShortcutAction
    {
        public string name;
        public KeyCode primaryKey;
        public KeyCode modifierKey;
        public Action action;
    }

    /// <summary>
    /// 输入系统模式
    /// </summary>
    public enum InputSystemMode
    {
        Normal,    // 正常模式
        Recording, // 录制模式
        Playback,  // 回放模式
        Debug      // 调试模式
    }

    /// <summary>
    /// 输入系统状态
    /// </summary>
    public struct InputSystemStatus
    {
        public bool isInitialized;
        public bool systemEnabled;
        public int inputProviderCount;
        public int movementHandlerCount;
        public float averageFPS;
        public int inputHistoryCount;
        public InputConfiguration currentConfig;
    }
    #endregion
} 