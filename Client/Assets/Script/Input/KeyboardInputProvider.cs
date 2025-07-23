using System;
using System.Collections;
using UnityEngine;

namespace InputSystem
{
    /// <summary>
    /// 键盘输入提供者
    /// 负责捕获键盘输入并转换为标准化的输入事件
    /// </summary>
    public class KeyboardInputProvider : MonoBehaviour, IInputProvider
    {
        #region Events
        public event Action<Vector2> OnMovementInput;
        public event Action OnInputStart;
        public event Action OnInputEnd;
        #endregion

        #region Properties
        public bool IsEnabled { get; set; } = true;
        public Vector2 CurrentInput { get; private set; }
        #endregion

        #region Private Fields
        [SerializeField] private InputConfiguration config;
        
        // 输入状态缓存
        private Vector2 rawInput = Vector2.zero;
        private Vector2 smoothedInput = Vector2.zero;
        private Vector2 inputVelocity = Vector2.zero;
        
        // 键盘状态缓存
        private bool[] keyStates = new bool[4]; // W, S, A, D
        private bool[] previousKeyStates = new bool[4];
        
        // 输入状态
        private bool wasInputActive = false;
        
        // 性能优化
        private float lastUpdateTime = 0f;
        private float updateInterval = 0f;
        private WaitForFixedUpdate waitForFixedUpdate;
        
        // 对象池
        private static readonly ObjectPool<Vector2Wrapper> vector2Pool = 
            new ObjectPool<Vector2Wrapper>(() => new Vector2Wrapper(), 10);
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            waitForFixedUpdate = new WaitForFixedUpdate();
            Initialize();
        }

        private void Start()
        {
            if (config != null)
            {
                updateInterval = 1f / config.inputUpdateRate;
            }
            
            StartCoroutine(InputUpdateCoroutine());
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void OnValidate()
        {
            if (config != null && !config.ValidateConfiguration())
            {
                Debug.LogWarning($"[KeyboardInputProvider] 配置验证失败: {gameObject.name}");
            }
        }
        #endregion

        #region IInputProvider Implementation
        public void Initialize()
        {
            if (config == null)
            {
                Debug.LogError("[KeyboardInputProvider] InputConfiguration未设置!");
                return;
            }

            // 初始化状态数组
            keyStates = new bool[4];
            previousKeyStates = new bool[4];
            
            // 重置输入状态
            CurrentInput = Vector2.zero;
            rawInput = Vector2.zero;
            smoothedInput = Vector2.zero;
            inputVelocity = Vector2.zero;
            
            Debug.Log("[KeyboardInputProvider] 初始化完成");
        }

        public void Cleanup()
        {
            OnMovementInput = null;
            OnInputStart = null;
            OnInputEnd = null;
            
            StopAllCoroutines();
        }
        #endregion

        #region Input Processing
        private IEnumerator InputUpdateCoroutine()
        {
            while (true)
            {
                if (IsEnabled && config != null)
                {
                    ProcessKeyboardInput();
                    ProcessInputSmoothing();
                    FireInputEvents();
                }
                
                yield return waitForFixedUpdate;
            }
        }

        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void ProcessKeyboardInput()
        {
            // 缓存当前帧的键盘状态
            CacheKeyboardStates();
            
            // 计算基础移动输入
            rawInput = CalculateRawMovementInput();
            
            // 应用死区处理
            rawInput = ApplyDeadZone(rawInput);
            
            // 标准化8方向输入
            if (config.enable8DirectionalInput)
            {
                rawInput = NormalizeEightDirectionalInput(rawInput);
            }
        }

        /// <summary>
        /// 缓存键盘状态以减少Input.GetKey调用
        /// </summary>
        private void CacheKeyboardStates()
        {
            // 保存上一帧状态
            System.Array.Copy(keyStates, previousKeyStates, keyStates.Length);
            
            // 更新当前状态
            keyStates[0] = Input.GetKey(config.forwardKey);  // W
            keyStates[1] = Input.GetKey(config.backKey);     // S
            keyStates[2] = Input.GetKey(config.leftKey);     // A
            keyStates[3] = Input.GetKey(config.rightKey);    // D
        }

        /// <summary>
        /// 计算原始移动输入
        /// </summary>
        private Vector2 CalculateRawMovementInput()
        {
            Vector2 input = Vector2.zero;
            
            // 垂直输入 (W/S)
            if (keyStates[0]) input.y += 1f;      // W - Forward
            if (keyStates[1]) input.y -= 1f;      // S - Back
            
            // 水平输入 (A/D)
            if (keyStates[2]) input.x -= 1f;      // A - Left
            if (keyStates[3]) input.x += 1f;      // D - Right
            
            // 应用输入灵敏度
            input *= config.inputSensitivity;
            
            return input;
        }

        /// <summary>
        /// 应用死区处理
        /// </summary>
        private Vector2 ApplyDeadZone(Vector2 input)
        {
            if (input.magnitude < config.inputDeadZone)
            {
                return Vector2.zero;
            }
            return input;
        }

        /// <summary>
        /// 标准化8方向输入
        /// </summary>
        private Vector2 NormalizeEightDirectionalInput(Vector2 input)
        {
            if (input.magnitude > 1f)
            {
                return input.normalized;
            }
            return input;
        }

        /// <summary>
        /// 处理输入平滑
        /// </summary>
        private void ProcessInputSmoothing()
        {
            if (config.inputSmoothTime > 0f)
            {
                smoothedInput = Vector2.SmoothDamp(
                    smoothedInput, 
                    rawInput, 
                    ref inputVelocity, 
                    config.inputSmoothTime,
                    Mathf.Infinity,
                    Time.fixedDeltaTime
                );
            }
            else
            {
                smoothedInput = rawInput;
            }
            
            CurrentInput = smoothedInput;
        }

        /// <summary>
        /// 触发输入事件
        /// </summary>
        private void FireInputEvents()
        {
            bool isCurrentlyActive = CurrentInput.magnitude > 0.01f;
            
            // 输入开始事件
            if (isCurrentlyActive && !wasInputActive)
            {
                OnInputStart?.Invoke();
            }
            
            // 输入结束事件
            if (!isCurrentlyActive && wasInputActive)
            {
                OnInputEnd?.Invoke();
            }
            
            // 移动输入事件
            if (isCurrentlyActive)
            {
                OnMovementInput?.Invoke(CurrentInput);
            }
            
            wasInputActive = isCurrentlyActive;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 设置输入配置
        /// </summary>
        public void SetConfiguration(InputConfiguration newConfig)
        {
            config = newConfig;
            if (config != null)
            {
                updateInterval = 1f / config.inputUpdateRate;
                Initialize();
            }
        }

        /// <summary>
        /// 检测按键按下
        /// </summary>
        public bool GetKeyDown(KeyCode keyCode)
        {
            return Input.GetKeyDown(keyCode);
        }

        /// <summary>
        /// 检测按键抬起
        /// </summary>
        public bool GetKeyUp(KeyCode keyCode)
        {
            return Input.GetKeyUp(keyCode);
        }
        #endregion

        #region Debugging
        private void OnGUI()
        {
            if (config != null && config.enableDetailedDebugInfo)
            {
                DrawDebugInfo();
            }
        }

        private void DrawDebugInfo()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label("=== 键盘输入调试信息 ===");
            GUILayout.Label($"原始输入: {rawInput}");
            GUILayout.Label($"平滑输入: {smoothedInput}");
            GUILayout.Label($"当前输入: {CurrentInput}");
            GUILayout.Label($"输入活跃: {wasInputActive}");
            GUILayout.EndArea();
        }
        #endregion
    }

    #region Helper Classes
    /// <summary>
    /// Vector2包装器，用于对象池
    /// </summary>
    public class Vector2Wrapper
    {
        public Vector2 value;
        
        public void Reset()
        {
            value = Vector2.zero;
        }
    }

    /// <summary>
    /// 简单对象池实现
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private readonly System.Collections.Generic.Queue<T> pool;
        private readonly System.Func<T> createFunc;

        public ObjectPool(System.Func<T> createFunc, int initialSize = 10)
        {
            this.createFunc = createFunc;
            pool = new System.Collections.Generic.Queue<T>();
            
            for (int i = 0; i < initialSize; i++)
            {
                pool.Enqueue(createFunc());
            }
        }

        public T Get()
        {
            return pool.Count > 0 ? pool.Dequeue() : createFunc();
        }

        public void Return(T item)
        {
            pool.Enqueue(item);
        }
    }
    #endregion
} 