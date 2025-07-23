# Unity GameObject 输入控制系统

一个完整的工业级Unity输入控制系统，基于事件驱动架构设计，提供高性能、可扩展的输入处理解决方案。

## 系统特点

- ✅ **事件驱动架构** - 完全解耦的组件设计
- ✅ **高性能优化** - 对象池、输入缓存、帧率控制
- ✅ **完全可配置** - ScriptableObject配置系统
- ✅ **输入录制回放** - 完整的录制和回放功能
- ✅ **实时调试可视化** - 详细的输入状态可视化
- ✅ **多种移动模式** - Transform、Rigidbody、CharacterController
- ✅ **扩展性强** - 易于添加新的输入源和处理器
- ✅ **生产就绪** - 完整的错误处理和性能监控

## 系统架构

```
InputManager (核心管理器)
├── IInputProvider (输入提供者接口)
│   └── KeyboardInputProvider (键盘输入)
├── IMovementHandler (移动处理器接口)  
│   └── TransformMovementHandler (Transform移动)
├── InputRecorder (录制回放)
├── InputDebugVisualizer (调试可视化)
└── InputConfiguration (配置文件)
```

## 快速开始

### 1. 创建配置文件

```csharp
// 在Project窗口右键 -> Create -> Input System -> Input Configuration
// 或使用代码创建
var config = ScriptableObject.CreateInstance<InputConfiguration>();
AssetDatabase.CreateAsset(config, "Assets/MyInputConfig.asset");
```

### 2. 基础设置

```csharp
public class MyController : MonoBehaviour
{
    [SerializeField] private InputConfiguration inputConfig;
    
    private void Start()
    {
        // 方法1: 使用InputSystemExample自动设置
        var example = gameObject.AddComponent<InputSystemExample>();
        example.inputConfig = inputConfig;
        
        // 方法2: 手动设置
        SetupInputSystemManually();
    }
    
    private void SetupInputSystemManually()
    {
        // 获取或创建InputManager
        var inputManager = InputManager.Instance;
        inputManager.SetConfiguration(inputConfig);
        
        // 创建输入提供者
        var keyboardProvider = gameObject.AddComponent<KeyboardInputProvider>();
        keyboardProvider.SetConfiguration(inputConfig);
        
        // 创建移动处理器
        var movementHandler = gameObject.AddComponent<TransformMovementHandler>();
        movementHandler.SetConfiguration(inputConfig);
        
        // 注册组件
        inputManager.RegisterInputProvider(keyboardProvider);
        inputManager.RegisterMovementHandler(movementHandler);
    }
}
```

### 3. 使用示例脚本

```csharp
// 直接使用InputSystemExample组件
var example = gameObject.AddComponent<InputSystemExample>();
example.inputConfig = myInputConfig;
example.controlledObject = playerTransform;
```

## 控制说明

### 基础控制
- **W** - 向前移动
- **S** - 向后移动  
- **A** - 向左移动
- **D** - 向右移动

### 系统控制
- **Ctrl+R** - 重置输入系统

### 调试功能
通过代码或UI面板控制：
- 切换输入可视化
- 开始/停止录制
- 播放录制文件
- 暂停/恢复系统
- 清空输入历史

## 配置系统

### InputConfiguration 配置参数

```csharp
[CreateAssetMenu(fileName = "InputConfig", menuName = "Input System/Input Configuration")]
public class InputConfiguration : ScriptableObject
{
    [Header("键位设置")]
    public KeyCode forwardKey = KeyCode.W;    // 前进键
    public KeyCode backKey = KeyCode.S;       // 后退键
    public KeyCode leftKey = KeyCode.A;       // 左移键
    public KeyCode rightKey = KeyCode.D;      // 右移键
    
    [Header("移动参数")]
    public float moveSpeed = 5f;              // 基础移动速度
    public float acceleration = 10f;          // 加速度
    public float deceleration = 15f;          // 减速度
    
    [Header("输入处理")]
    public float inputDeadZone = 0.1f;        // 输入死区
    public float inputSmoothTime = 0.1f;      // 输入平滑时间
    public float inputSensitivity = 1f;       // 输入灵敏度
    public bool enable8DirectionalInput = true; // 8方向输入
    
    [Header("调试选项")]
    public bool enableInputVisualization = true;   // 输入可视化
    public bool enableInputRecording = false;      // 输入录制
    public bool enableDetailedDebugInfo = false;   // 详细调试信息
}
```

## 核心组件详解

### 1. InputManager - 输入管理器

整个系统的核心协调器，负责管理所有输入组件。

```csharp
// 获取单例实例
var inputManager = InputManager.Instance;

// 设置配置
inputManager.SetConfiguration(config);

// 注册组件
inputManager.RegisterInputProvider(provider);
inputManager.RegisterMovementHandler(handler);

// 系统控制
inputManager.PauseSystem();     // 暂停
inputManager.ResumeSystem();    // 恢复
inputManager.ResetSystem();     // 重置

// 获取系统状态
var status = inputManager.GetSystemStatus();
```

### 2. KeyboardInputProvider - 键盘输入提供者

处理键盘输入并转换为标准化事件。

```csharp
public class KeyboardInputProvider : MonoBehaviour, IInputProvider
{
    // 事件
    public event Action<Vector2> OnMovementInput;
    public event Action OnInputStart;
    public event Action OnInputEnd;
    
    // 属性
    public bool IsEnabled { get; set; }
    public Vector2 CurrentInput { get; }
    
    // 方法
    public void SetConfiguration(InputConfiguration config);
    public bool GetKeyDown(KeyCode keyCode);
    public bool GetKeyUp(KeyCode keyCode);
}
```

### 3. TransformMovementHandler - 移动处理器

将输入转换为实际的GameObject移动。

```csharp
public class TransformMovementHandler : MonoBehaviour, IMovementHandler
{
    // 移动模式
    public enum MovementMode
    {
        Transform,           // 直接修改Transform
        Rigidbody,          // 使用Rigidbody物理
        CharacterController // 使用CharacterController
    }
    
    // 方法
    public void HandleMovement(Vector2 inputVector);
    public void SetMovementEnabled(bool enabled);
    public void StopMovement();
    public void TeleportTo(Vector3 position);
    public MovementStats GetMovementStats();
}
```

### 4. InputRecorder - 录制回放系统

```csharp
// 开始录制
recorder.StartRecording();

// 停止录制
var recording = recorder.StopRecording();

// 保存录制
recorder.SaveRecording(recording, "MyRecording");

// 加载录制
var loadedRecording = recorder.LoadRecording("MyRecording");

// 开始回放
recorder.StartPlayback(loadedRecording);

// 控制回放
recorder.SetPlaybackSpeed(2f);        // 2倍速
recorder.SeekPlayback(0.5f);          // 跳转到50%位置
recorder.StopPlayback();              // 停止回放

// 获取录制统计
var stats = recorder.GetRecorderStats();
```

### 5. InputDebugVisualizer - 调试可视化

```csharp
// 切换可视化
visualizer.ToggleVisualization();

// 设置显示参数
visualizer.SetDisplayPosition(new Vector2(100, 100));
visualizer.SetDisplaySize(new Vector2(300, 300));

// 清空历史
visualizer.ClearInputHistory();

// 启用/禁用可视化
visualizer.SetVisualizationEnabled(true);
```

## 扩展系统

### 创建自定义输入提供者

```csharp
public class GamepadInputProvider : MonoBehaviour, IInputProvider
{
    public event Action<Vector2> OnMovementInput;
    public event Action OnInputStart; 
    public event Action OnInputEnd;
    
    public bool IsEnabled { get; set; } = true;
    public Vector2 CurrentInput { get; private set; }
    
    public void Initialize()
    {
        // 初始化手柄输入
    }
    
    public void Cleanup()
    {
        // 清理资源
    }
    
    private void Update()
    {
        if (!IsEnabled) return;
        
        // 读取手柄输入
        Vector2 leftStick = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
        
        CurrentInput = leftStick;
        OnMovementInput?.Invoke(leftStick);
    }
}
```

### 创建自定义移动处理器

```csharp
public class RigidbodyMovementHandler : MonoBehaviour, IMovementHandler
{
    private Rigidbody rb;
    private InputConfiguration config;
    
    public bool IsMoving { get; private set; }
    public Vector3 CurrentVelocity => rb.velocity;
    
    public void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public void HandleMovement(Vector2 inputVector)
    {
        Vector3 force = new Vector3(inputVector.x, 0, inputVector.y) * config.moveSpeed;
        rb.AddForce(force, ForceMode.Force);
        
        IsMoving = inputVector.magnitude > 0.01f;
    }
    
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
    }
    
    public void Cleanup()
    {
        // 清理资源
    }
}
```

## 性能优化建议

### 1. 对象池使用

```csharp
// 系统内置对象池
private static readonly ObjectPool<Vector2Wrapper> vector2Pool = 
    new ObjectPool<Vector2Wrapper>(() => new Vector2Wrapper(), 10);

// 使用方式
var wrapper = vector2Pool.Get();
wrapper.value = inputVector;
// 使用完毕后归还
vector2Pool.Return(wrapper);
```

### 2. 输入采样率控制

```csharp
// 在InputConfiguration中设置
public int inputUpdateRate = 60;  // 60fps采样率

// 或在代码中动态调整
inputProvider.SetConfiguration(configWithLowerRate);
```

### 3. 输入缓存优化

```csharp
// 启用输入缓存
config.enableInputCaching = true;
config.maxInputHistoryCount = 50;  // 限制历史数量
```

## 调试和测试

### 1. 启用调试信息

```csharp
// 在InputConfiguration中启用
config.enableDetailedDebugInfo = true;
config.enableInputVisualization = true;
```

### 2. 系统验证

```csharp
// 使用InputSystemExample的验证功能
[ContextMenu("Validate System Setup")]
private void ValidateSystemSetup()
{
    // 检查配置文件
    // 检查组件引用
    // 检查系统状态
}
```

### 3. 获取系统状态

```csharp
// 获取输入管理器状态
var managerStatus = inputManager.GetSystemStatus();
Debug.Log($"系统启用: {managerStatus.systemEnabled}");
Debug.Log($"平均FPS: {managerStatus.averageFPS}");

// 获取移动处理器状态
var movementStats = movementHandler.GetMovementStats();
Debug.Log($"当前速度: {movementStats.currentSpeed}");
Debug.Log($"是否移动: {movementStats.isMoving}");

// 获取录制器状态
var recorderStats = recorder.GetRecorderStats();
Debug.Log($"录制状态: {recorderStats.isRecording}");
Debug.Log($"回放进度: {recorderStats.playbackProgress}");
```

## 常见问题解答

### Q: 如何添加新的输入键位？

A: 在InputConfiguration中添加新的KeyCode字段，然后在KeyboardInputProvider中添加相应的处理逻辑。

```csharp
// 在InputConfiguration中
public KeyCode jumpKey = KeyCode.Space;

// 在KeyboardInputProvider中的ProcessKeyboardInput方法中
if (Input.GetKeyDown(config.jumpKey))
{
    OnJumpInput?.Invoke();  // 需要先定义这个事件
}
```

### Q: 如何支持手柄输入？

A: 创建一个实现IInputProvider接口的GamepadInputProvider类，处理手柄输入逻辑，然后注册到InputManager中。

### Q: 如何优化性能？

A: 
1. 降低输入采样率（`inputUpdateRate`）
2. 启用输入缓存（`enableInputCaching`）
3. 使用内置对象池减少GC
4. 限制历史记录数量（`maxInputHistoryCount`）
5. 在不需要时禁用调试功能

### Q: 如何处理多个玩家？

A: 为每个玩家创建独立的输入组件组合，使用不同的配置文件。

```csharp
// 玩家1 - 键盘控制
var player1Manager = player1Object.AddComponent<InputManager>();
var player1Provider = player1Object.AddComponent<KeyboardInputProvider>();
var player1Handler = player1Object.AddComponent<TransformMovementHandler>();

// 玩家2 - 手柄控制
var player2Manager = player2Object.AddComponent<InputManager>();
var player2Provider = player2Object.AddComponent<GamepadInputProvider>();
var player2Handler = player2Object.AddComponent<TransformMovementHandler>();
```

### Q: 如何在运行时动态改变键位？

A: 修改配置文件中的键位设置，然后重新应用配置。

```csharp
// 修改键位
config.forwardKey = KeyCode.UpArrow;
config.backKey = KeyCode.DownArrow;
config.leftKey = KeyCode.LeftArrow;
config.rightKey = KeyCode.RightArrow;

// 重新应用配置
inputManager.SetConfiguration(config);
```

## API 参考

### InputManager 主要方法
- `Initialize()` - 初始化系统
- `SetConfiguration(InputConfiguration)` - 设置配置
- `RegisterInputProvider(IInputProvider)` - 注册输入提供者
- `RegisterMovementHandler(IMovementHandler)` - 注册移动处理器
- `PauseSystem()` / `ResumeSystem()` - 暂停/恢复系统
- `ResetSystem()` - 重置系统
- `GetSystemStatus()` - 获取系统状态

### KeyboardInputProvider 主要方法
- `SetConfiguration(InputConfiguration)` - 设置配置
- `GetKeyDown(KeyCode)` / `GetKeyUp(KeyCode)` - 检测按键

### TransformMovementHandler 主要方法
- `HandleMovement(Vector2)` - 处理移动输入
- `SetMovementEnabled(bool)` - 启用/禁用移动
- `StopMovement()` - 停止移动
- `TeleportTo(Vector3)` - 瞬移到位置
- `GetMovementStats()` - 获取移动统计

### InputRecorder 主要方法
- `StartRecording()` / `StopRecording()` - 开始/停止录制
- `SaveRecording(InputRecording, string)` - 保存录制
- `LoadRecording(string)` - 加载录制
- `StartPlayback(InputRecording)` / `StopPlayback()` - 开始/停止回放
- `SetPlaybackSpeed(float)` - 设置回放速度

## 最佳实践

1. **始终使用配置文件** - 避免硬编码输入设置
2. **合理使用事件** - 避免在Update中轮询状态
3. **及时清理资源** - 在OnDestroy中调用Cleanup方法
4. **使用对象池** - 减少GC压力
5. **性能监控** - 启用性能监控功能，及时发现问题
6. **模块化设计** - 保持组件间的松耦合
7. **测试录制回放** - 使用录制功能进行自动化测试
8. **适当的采样率** - 根据游戏需求调整输入更新频率

## 系统限制

- 单例InputManager设计，每个场景建议只有一个实例
- 输入历史记录有数量限制（可配置）
- 录制文件大小取决于录制时长和采样率
- 调试可视化功能会消耗额外性能

## 版本历史

- **v1.0.0** - 初始版本，包含基础输入处理功能
- **v1.1.0** - 添加录制回放系统
- **v1.2.0** - 添加调试可视化功能
- **v1.3.0** - 性能优化和错误处理改进
- **v1.4.0** - 简化系统，移除冗余功能，专注核心输入处理

## 技术支持

如有问题，请查看：
1. 控制台错误信息和警告
2. 系统状态面板显示的信息
3. 调试可视化器的实时数据
4. 示例场景和代码实现
5. 使用 `ValidateSystemSetup()` 检查配置

---

**注意**: 这是一个专注于核心功能的生产级输入系统。系统经过简化，移除了不必要的复杂性，专注于提供稳定、高性能的输入处理能力。 