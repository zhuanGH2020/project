# 输入系统技术文档

## 概述
输入系统由两个核心组件构成：`InputManager` 和 `InputUtils`，共同提供完整的玩家输入处理解决方案。

- **InputManager**: 统一的输入管理器，负责处理键盘和鼠标输入，通过事件系统与其他组件通信
- **InputUtils**: 静态工具类，提供底层的鼠标点击检测、UI交互判断和射线检测功能

## 架构设计

```
GameMain
├── InputManager (纯C#单例类)
│   ├── 处理WASD移动输入
│   ├── 处理鼠标点击移动
│   ├── 处理装备快捷键
│   └── 使用 InputUtils 进行UI检测
└── InputUtils (静态工具类)
    ├── UI点击检测
    ├── 世界射线检测
    └── 点击信息打印
```

---

# InputManager 使用指南

## 特性
- 统一的输入处理逻辑
- 基于事件的解耦设计
- 可动态启用/禁用输入
- 由 GameMain 统一管理的纯 C# 单例类
- 集成了项目现有的 `InputUtils` 工具类

## 支持的输入类型

### 1. 移动输入 (WASD键)
- **事件**: `OnMoveInput(Vector3 moveDirection)`
- **触发**: 检测到 Horizontal/Vertical 轴输入时
- **参数**: 标准化的移动方向向量

### 2. 鼠标点击移动
- **事件**: `OnMouseClickMove(Vector3 targetPosition)`  
- **触发**: 鼠标左键点击非UI区域时
- **参数**: 世界坐标中的目标位置
- **注意**: 自动过滤UI点击，使用 `InputUtils.IsPointerOverUI()` 检测
- **UI点击**: 点击UI时会自动打印UI路径信息

### 3. 装备使用
- **事件**: `OnUseEquipInput()`
- **触发**: 按下空格键时

### 4. 装备快捷键
- **事件**: `OnEquipShortcutInput(int equipId)`
- **触发**: 按下Q键或E键时
- **参数**: 
  - Q键 → equipId = 30001
  - E键 → equipId = 30002

## 使用方法

### 1. 系统初始化
```csharp
// InputManager 由 GameMain 自动初始化和管理
// 无需手动创建GameObject或添加组件
// GameMain.Start() 中自动调用: var inputManager = InputManager.Instance;
// GameMain.Update() 中自动调用: InputManager.Instance.Update();
```

### 2. 订阅事件
```csharp
private void Start()
{
    if (InputManager.Instance != null)
    {
        InputManager.Instance.OnMoveInput += HandleMoveInput;
        InputManager.Instance.OnMouseClickMove += HandleMouseMove;
        InputManager.Instance.OnUseEquipInput += HandleUseEquip;
        InputManager.Instance.OnEquipShortcutInput += HandleEquipShortcut;
    }
}

private void OnDestroy()
{
    if (InputManager.Instance != null)
    {
        InputManager.Instance.OnMoveInput -= HandleMoveInput;
        InputManager.Instance.OnMouseClickMove -= HandleMouseMove;
        InputManager.Instance.OnUseEquipInput -= HandleUseEquip;
        InputManager.Instance.OnEquipShortcutInput -= HandleEquipShortcut;
    }
}
```

### 3. 控制输入状态
```csharp
// 禁用输入
InputManager.Instance.SetInputEnabled(false);

// 启用输入
InputManager.Instance.SetInputEnabled(true);

// 检查输入状态
bool isEnabled = InputManager.Instance.IsInputEnabled;
```

---

# InputUtils 技术文档

## 简介
InputUtils 是一个静态工具类，为 InputManager 提供底层支持，包含鼠标点击检测、UI交互判断和详细的点击对象信息打印功能。

## 详细接口

### 基础检测方法

#### `IsPointerOverUI()`
```csharp
public static bool IsPointerOverUI()
```
- **功能**: 检测鼠标是否点击在UI上
- **返回值**: bool - 是否点击UI
- **说明**: 基于EventSystem检测，无EventSystem时返回false并警告

#### `GetMouseWorldHit(out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = -1)`
```csharp
public static bool GetMouseWorldHit(out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = -1)
```
- **功能**: 获取鼠标在世界空间的射线检测结果
- **参数**: 
  - `hit`: 射线检测结果
  - `maxDistance`: 最大检测距离，默认无限远
  - `layerMask`: 检测层级，默认检测所有层
- **返回值**: bool - 是否检测到物体

#### `GetUIRaycastResults()`
```csharp
public static List<RaycastResult> GetUIRaycastResults()
```
- **功能**: 获取UI射线检测结果列表
- **返回值**: List<RaycastResult> - UI检测结果列表
- **说明**: 使用缓存列表避免GC分配

#### `GetGameObjectPath(GameObject obj)`
```csharp
public static string GetGameObjectPath(GameObject obj)
```
- **功能**: 获取GameObject的完整层级路径
- **参数**: `obj` - 目标GameObject
- **返回值**: string - 完整的层级路径（如："Canvas/MainPanel/Button"）

### 信息打印方法

#### `PrintClickedUIObjects()`
```csharp
public static void PrintClickedUIObjects()
```
- **功能**: 打印点击的UI对象详细信息
- **输出**: 单行日志包含所有UI对象信息

#### `PrintClickedUIPath()`
```csharp
public static void PrintClickedUIPath()
```
- **功能**: 打印点击的UI对象路径信息
- **输出**: 单行日志包含所有UI对象的完整路径
- **格式**: `=== UI路径检测 - 共检测到 N 个UI对象 === | [UI-0] Path: Canvas/Panel/Button`

#### `PrintClickedWorldObject(RaycastHit hit)`
```csharp
public static void PrintClickedWorldObject(RaycastHit hit)
```
- **功能**: 打印点击的世界GameObject详细信息
- **参数**: `hit` - 射线检测结果
- **输出**: 单行日志包含GameObject完整信息

### 综合处理方法

#### `HandleSafeMouseClick(System.Action onClickWorld)`
```csharp
public static void HandleSafeMouseClick(System.Action onClickWorld)
```
- **功能**: 执行安全的鼠标点击检测，只有在不点击UI时才执行游戏世界回调
- **参数**: `onClickWorld` - 点击游戏世界时的回调
- **行为**: 点击UI时打印UI信息，点击世界时执行回调

#### `HandleWorldClick(System.Action<RaycastHit> onHitWorld, System.Action onClickEmpty = null, int layerMask = -1)`
```csharp
public static void HandleWorldClick(System.Action<RaycastHit> onHitWorld, System.Action onClickEmpty = null, int layerMask = -1)
```
- **功能**: 综合的世界点击处理方法，自动判断UI/世界点击并打印相应信息
- **参数**: 
  - `onHitWorld`: 击中世界对象时的回调
  - `onClickEmpty`: 点击空白区域时的回调
  - `layerMask`: 检测层级
- **行为**: 自动处理UI点击、世界点击、空白点击三种情况

#### `AnalyzeClick()`
```csharp
public static bool AnalyzeClick()
```
- **功能**: 完整的点击分析方法，执行详细的点击检测并打印所有相关信息
- **返回值**: bool - 是否点击了UI（true为UI，false为世界或空白）

### 管理方法

#### `ClearCachedReferences()`
```csharp
public static void ClearCachedReferences()
```
- **功能**: 清理缓存的摄像机引用
- **说明**: 在场景切换时调用以避免空引用

---

# 系统集成和最佳实践

## InputManager 与 InputUtils 的配合

### 1. InputManager 中的使用
```csharp
// InputManager 在处理鼠标点击时使用 InputUtils
private void HandleMouseClickMove()
{
    // 使用 InputUtils 检测是否点击UI
    if (InputUtils.IsPointerOverUI())
    {
        // 打印UI路径信息
        InputUtils.PrintClickedUIPath();
        return;
    }

    // 使用 InputUtils 获取世界点击位置
    if (InputUtils.GetMouseWorldHit(out RaycastHit hit))
    {
        OnMouseClickMove?.Invoke(hit.point);
    }
}
```

### 2. Player.cs 的改动
原来的 `Player.cs` 中的输入处理逻辑已被移除，改为订阅 `InputManager` 的事件：

- `HandleInput()` 方法已删除
- 添加了 `SubscribeToInputEvents()` 和 `UnsubscribeFromInputEvents()` 方法
- 各种输入处理改为事件回调：
  - `OnMoveInput()` - 处理移动输入
  - `OnMouseClickMove()` - 处理鼠标点击移动
  - `OnUseEquipInput()` - 处理装备使用
  - `OnEquipShortcutInput()` - 处理装备快捷键

## 使用场景示例

### 1. 基础UI点击检测
```csharp
// 简单判断是否点击UI
if (InputUtils.IsPointerOverUI())
{
    return; // 点击了UI，不执行游戏逻辑
}
```

### 2. 安全的世界点击处理
```csharp
// 在Update中使用安全点击检测
private void Update()
{
    InputUtils.HandleSafeMouseClick(() => {
        // 只有点击游戏世界时才执行的逻辑
        Debug.Log("点击了游戏世界！");
    });
}
```

### 3. 完整的点击分析
```csharp
// 获取详细点击信息用于调试
private void Update()
{
    InputUtils.HandleWorldClick(
        onHitWorld: (hit) => {
            // 处理击中的世界对象
            HandleWorldObject(hit.collider.gameObject);
        },
        onClickEmpty: () => {
            // 处理点击空白区域
            Debug.Log("点击了空白区域");
        }
    );
}
```

### 4. 场景切换管理
```csharp
// 在场景切换时清理缓存
private void OnLevelWasLoaded(int level)
{
    InputUtils.ClearCachedReferences();
}
```

## 注意事项

### 使用约束
- **InputManager**: 纯C#单例类，由GameMain统一管理，无需手动创建
- **InputUtils**: 静态工具类，无需实例化，直接调用方法
- **EventSystem依赖**: UI检测功能依赖场景中的EventSystem组件  
- **主摄像机要求**: 世界射线检测需要场景中存在Camera.main

### 性能考虑
- **摄像机缓存**: 自动缓存主摄像机引用，提升性能
- **列表复用**: UI检测使用静态列表避免GC分配
- **StringBuilder优化**: 日志输出使用StringBuilder减少字符串分配
- **统一更新**: InputManager由GameMain统一驱动，避免多个Update循环

### 调试功能
- **详细日志**: 提供丰富的点击信息用于调试
- **UI路径打印**: 自动显示UI对象的完整层级路径
- **单行输出**: 所有日志信息合并为单行，保持控制台整洁
- **组件信息**: 自动显示点击对象的所有组件信息

## 参考的项目代码
- **EventManager.cs** (Assets/Scripts/Core/Event/EventManager.cs) - 单例模式设计
- **ClockModel.cs, SaveModel.cs** - Model类设计模式
- **GameMain.cs** (Assets/Scripts/GameMain.cs) - 统一系统管理

## 系统优势
1. **统一架构**: InputManager与其他Model类保持一致的设计模式
2. **解耦设计**: 输入逻辑与具体业务逻辑分离
3. **事件驱动**: 基于事件的松耦合通信机制
4. **复用性强**: 其他类可以订阅相同的输入事件
5. **易于维护**: 输入逻辑集中管理，易于修改和扩展
6. **调试友好**: 提供丰富的调试信息和UI路径打印功能
7. **性能优化**: 缓存机制和对象复用减少GC压力

*版本: 2.0 - 整合版* 