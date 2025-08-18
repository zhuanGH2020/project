# GameMain 技术文档

## 简介

GameMain 是项目的核心系统编排者，负责统一管理所有游戏系统的生命周期、初始化顺序和更新驱动。

**核心功能：**
- 系统初始化管理：按依赖关系有序初始化所有Manager和Model
- 生命周期控制：统一驱动系统更新和清理
- 跨场景持久化：确保核心系统在场景切换时保持
- 内存管理：定期清理无效对象引用

**组件类型：** MonoBehaviour（需要挂载到GameObject上使用）

## 详细接口

### 核心生命周期方法

#### `void Awake()`
设置GameMain的持久化属性
```csharp
void Awake()
{
    DontDestroyOnLoad(gameObject); // 跨场景保持
}
```

#### `void Start()`
启动系统初始化流程
```csharp
void Start()
{
    // 临时：配置系统示例代码
    ConfigExample.Example();
    ConfigExample.AdvancedExample();
    ConfigExample.ValidationExample();

    InitializeSystems();
}
```

#### `void Update()`
驱动所有系统更新
```csharp
void Update()
{
    UpdateSystems();     // 系统更新
    PeriodicCleanup();   // 定期清理
}
```

#### `void OnDestroy()`
清理所有系统资源
```csharp
void OnDestroy()
{
    CleanupSystems();
}
```

### 系统管理方法

#### `private void InitializeSystems()`
按依赖顺序初始化所有系统

**初始化顺序：**
```csharp
// === 基础系统（无依赖） ===
_ = ObjectManager.Instance;    // 对象注册管理器
_ = InputManager.Instance;     // 输入管理器
_ = ClockModel.Instance;       // 时间模型
_ = PackageModel.Instance;     // 背包模型
_ = MapModel.Instance;         // 地图模型
_ = MapManager.Instance;       // 地图管理器
_ = DialogManager.Instance;    // 对话管理器
_ = UIManager.Instance;        // UI管理器

// === 依赖系统（需要其他系统支持） ===
InteractionManager.Instance.Initialize(); // 交互管理器（需要EventManager）
SaveModel.Instance.Initialize();          // 存档模型（需要其他Model）
```

#### `private void UpdateSystems()`
按优先级顺序更新系统

**更新优先级：**
```csharp
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
```

#### `private void PeriodicCleanup()`
定期清理任务
```csharp
// 每10秒清理一次空引用
if (Time.time % 10f < Time.deltaTime)
{
    ObjectManager.Instance.CleanupNullReferences();
}
```

#### `private void CleanupSystems()`
按反向依赖顺序清理系统

**清理顺序：**
```csharp
// 先清理依赖系统
SaveModel.Instance.Cleanup();
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
```

## 最佳实践

### 使用方式

#### 1. GameObject 设置
```csharp
// 在Unity编辑器中：
// 1. 创建空的GameObject，命名为"GameMain"
// 2. 添加GameMain脚本组件
// 3. 确保该GameObject在场景层级中位置合适
```

#### 2. 场景配置
```csharp
// 确保GameMain在游戏启动场景中存在
// GameMain会自动处理跨场景持久化
// 不需要在每个场景都放置GameMain
```

### 系统集成规范

#### 1. 添加新的Manager系统
```csharp
// 在InitializeSystems()中添加：

// 如果是基础系统（无依赖）：
private void InitializeSystems()
{
    // === 基础系统初始化（无依赖） ===
    _ = NewManager.Instance; // 添加到这里
    
    // === 依赖系统初始化 ===
    // ...
}

// 如果是依赖系统（需要其他系统）：
private void InitializeSystems()
{
    // === 基础系统初始化 ===
    // ...
    
    // === 依赖系统初始化（需要其他系统支持） ===
    NewManager.Instance.Initialize(); // 添加到这里
}
```

#### 2. 添加系统更新
```csharp
private void UpdateSystems()
{
    // 根据系统类型添加到合适位置：
    // 输入系统 - 最高优先级
    // 游戏逻辑系统
    // 数据持久化系统  
    // UI系统 - 最后更新
    
    NewManager.Instance.Update(); // 添加到合适位置
}
```

#### 3. 添加系统清理
```csharp
private void CleanupSystems()
{
    // 按反向依赖顺序添加：
    NewManager.Instance.Cleanup(); // 根据依赖关系添加到合适位置
}
```

### 代码风格规范

#### 1. 统一的初始化方式
```csharp
// ✅ 基础系统：使用丢弃变量
_ = ObjectManager.Instance;

// ✅ 依赖系统：显式调用Initialize
InteractionManager.Instance.Initialize();

// ❌ 避免：声明但不使用的变量
var manager = SomeManager.Instance; // 不推荐
```

#### 2. 系统分组注释
```csharp
// === 基础系统初始化（无依赖） ===
// 明确标注系统类型和依赖关系

// === 依赖系统初始化（需要其他系统支持） ===
// 说明为什么需要在基础系统之后初始化
```

## 注意事项

### 初始化顺序
- **严格按依赖关系初始化**：基础系统 → 依赖系统
- **不要随意调整顺序**：可能导致空引用或初始化失败
- **新增系统时确认依赖**：确定是否需要其他系统先初始化

### 更新性能
- **避免在Update中添加耗时操作**：所有Manager.Update()应该高效执行
- **使用定时器控制频率**：参考PeriodicCleanup的实现方式
- **监控系统更新开销**：通过Profiler检查各系统性能

### 清理完整性
- **确保所有系统都有清理逻辑**：避免内存泄漏
- **按反向依赖顺序清理**：防止清理时的空引用问题
- **处理HasInstance检查**：如ObjectManager.HasInstance

### 跨场景考虑
- **GameMain是持久化对象**：会跨场景保持存在
- **避免重复初始化**：使用_initialized标志防止重复
- **场景特定逻辑分离**：不要在GameMain中写场景特定代码

## 其他关键点

### 系统架构设计
GameMain采用**Application Root模式**，作为整个应用程序的根组织者：
- **统一入口**：所有系统通过GameMain统一管理
- **依赖注入**：控制系统间的依赖关系
- **生命周期控制**：统一管理系统的创建、更新、销毁

### 扩展建议
- **配置系统集成**：可以将系统初始化参数通过配置表管理
- **启动性能优化**：可以实现异步初始化避免启动卡顿
- **错误处理增强**：添加系统初始化失败的处理逻辑
- **调试支持**：添加系统状态监控和调试信息

### 与其他系统的关系
- **ConfigManager**：提供配置数据支持
- **EventManager**：提供系统间通信
- **ObjectManager**：提供对象注册和查询
- **SaveModel**：提供数据持久化
- **所有Manager类**：由GameMain统一管理生命周期

这种设计确保了系统的有序启动、高效运行和完整清理，是整个项目架构的核心支柱。 