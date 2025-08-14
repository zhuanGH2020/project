# UIManager技术文档

*最后更新：2024年12月 - v1.0版本：UI管理系统核心功能*

## 简介

UI管理器，统一管理所有UI界面的生命周期、显示隐藏和层级控制，与BaseView架构深度集成，提供简洁高效的UI管理能力。

## 详细接口

### UIManager（单例管理器）

**核心类**：`UIManager`（单例类）
- **用途**：管理所有UI界面的显示、隐藏、销毁和层级
- **特点**：与BaseView架构集成，支持事件驱动，由GameMain管理

#### 访问方式
```csharp
// 单例访问
UIManager.Instance
```

#### UI生命周期管理
```csharp
/// <summary>
/// 显示UI界面
/// </summary>
/// <typeparam name="T">UI界面类型，必须继承BaseView</typeparam>
/// <param name="layer">UI显示层级</param>
/// <returns>是否成功显示</returns>
public bool Show<T>(UILayer layer = UILayer.Normal) where T : BaseView

/// <summary>
/// 隐藏UI界面
/// </summary>
public bool Hide<T>() where T : BaseView

/// <summary>
/// 销毁UI界面
/// </summary>
public bool Destroy<T>() where T : BaseView

/// <summary>
/// 检查UI界面是否可见
/// </summary>
public bool IsVisible<T>() where T : BaseView

/// <summary>
/// 获取UI界面实例
/// </summary>
public T Get<T>() where T : BaseView
```

#### 全局UI控制
```csharp
/// <summary>
/// 隐藏所有UI界面 - 通过UICanvas的CanvasGroup控制整个UI系统
/// </summary>
public void HideAll()

/// <summary>
/// 显示所有UI界面 - 恢复整个UI系统的显示和交互
/// </summary>
public void ShowAll()
```

#### UI层级枚举
```csharp
public enum UILayer
{
    Background = 0,  // 背景层
    Normal = 100,    // 普通UI层
    Popup = 200,     // 弹窗层
    System = 400     // 系统层
}
```

#### GameMain集成
```csharp
/// <summary>
/// 更新方法 - 供GameMain调用
/// </summary>
public void Update()

/// <summary>
/// 清理方法 - 供GameMain调用
/// </summary>
public void Cleanup()
```

## 最佳实践

### 1. 基础使用

```csharp
// 显示普通UI界面
UIManager.Instance.Show<PackageView>();

// 显示系统层UI界面
UIManager.Instance.Show<SettingsView>(UILayer.System);

// 隐藏UI界面
UIManager.Instance.Hide<PackageView>();

// 检查UI状态
if (UIManager.Instance.IsVisible<PackageView>())
{
    // 处理UI可见逻辑
    var packageView = UIManager.Instance.Get<PackageView>();
    // 操作UI实例
}
```

### 2. UI预制体放置

UI预制体需要放置在正确的路径：
```
Assets/Resources/Prefabs/UI/
    ├── PackageView.prefab
    ├── InventoryView.prefab
    ├── SettingsView.prefab
    └── ...
```

### 3. 模态效果实现

不需要代码管理模态逻辑，在UI预制体中直接添加阻挡背景：
```csharp
// 在UI预制体中添加一个全屏Image组件
// 设置为半透明黑色：Color(0, 0, 0, 0.5f)
// 自动阻挡背景点击，无需编程管理
```

### 4. 全局UI控制

```csharp
// 游戏暂停时隐藏所有UI
UIManager.Instance.HideAll();

// 游戏恢复时显示所有UI
UIManager.Instance.ShowAll();
```

### 5. 事件驱动通信

UIManager会自动发布UI事件：
```csharp
// 订阅UI显示事件
EventManager.Instance.Subscribe<UIShowEvent>(OnUIShow);
EventManager.Instance.Subscribe<UIHideEvent>(OnUIHide);

private void OnUIShow(UIShowEvent eventData)
{
    Debug.Log($"UI显示：{eventData.ViewType.Name}");
}

private void OnUIHide(UIHideEvent eventData)
{
    Debug.Log($"UI隐藏：{eventData.ViewType.Name}");
}
```

### 6. UI销毁管理

```csharp
// 手动销毁不需要的UI（释放内存）
UIManager.Instance.Destroy<PackageView>();

// 或在游戏结束时自动清理所有UI
// GameMain.OnDestroy()会自动调用UIManager.Instance.Cleanup()
```

## 注意事项

### 1. UI预制体要求
- 所有UI类必须继承`BaseView`
- UI预制体需要放置在`Assets/Resources/Prefabs/UI/`目录
- 预制体名称必须与类名一致

### 2. UI层级管理
- 层级值越大，显示越靠前
- 系统层UI会显示在最上方
- 只有带Canvas组件的UI才支持层级设置

### 3. 性能考虑
- `Hide()`只是隐藏UI，不释放内存
- `Destroy()`会完全销毁UI，释放内存
- `HideAll()`通过CanvasGroup控制，性能更好

### 4. 事件处理
- UI显示/隐藏会自动发布对应事件
- ESC键会自动发布`BackKeyEvent`事件
- 其他组件可以订阅这些事件进行响应

## 系统集成

### GameMain集成
```csharp
void Start()
{
    // 初始化UI管理器
    var uiManager = UIManager.Instance;
}

void Update()
{
    // 驱动UI管理器
    UIManager.Instance.Update();
}

void OnDestroy()
{
    // 清理UI管理器
    UIManager.Instance.Cleanup();
}
```

### BaseView架构集成
UIManager与现有BaseView架构完美集成：
- 自动利用BaseView的资源管理能力
- 支持BaseView的生命周期方法
- 兼容现有的所有View组件

### View系统整体集成
UIManager是View系统生态的核心管理器：
- **UIManager**：统一UI显示控制和生命周期管理
- **BaseView**：提供资源管理和View基础功能
- **ViewUtils**：简化道具UI设置，减少重复代码
- **ResourceUtils**：提供工具方法和配置访问
- **EventManager**：支持UI事件的自动发布和订阅

```csharp
// 完整的系统集成示例
public class PlayerInventoryController : MonoBehaviour
{
    private void Start()
    {
        // 订阅UI事件
        EventManager.Instance.Subscribe<UIShowEvent>(OnUIShow);
    }
    
    private void OpenInventory()
    {
        // UIManager控制显示
        UIManager.Instance.Show<InventoryView>(UILayer.Popup);
    }
    
    private void OnUIShow(UIShowEvent eventData)
    {
        if (eventData.ViewType == typeof(InventoryView))
        {
            Debug.Log("背包界面已打开");
        }
    }
}
```

## 参考代码位置

本技术文档基于以下项目文件：
- `Assets/Scripts/Manager/UIManager.cs` (UI管理器核心实现)
- `Assets/Scripts/UI/Base/BaseView.cs` (View基类，自动集成)
- `Assets/Scripts/GameMain.cs` (系统集成示例)
- `Assets/Scripts/Utils/ViewUtils.cs` (View工具类，配合使用)
- `Assets/Scripts/Utils/ResourceUtils.cs` (资源工具类，配合使用)

开发新UI时，建议：
1. 继承BaseView创建View类
2. 创建对应的UI预制体，放置在`Assets/Resources/Prefabs/UI/`
3. 使用UIManager进行显示控制
4. 结合ViewUtils简化道具UI设置
5. 订阅UI事件实现系统响应 