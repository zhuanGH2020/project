# View开发规范

## 简介
View是Unity UI系统中的视图层组件，负责UI界面的显示、交互处理和用户输入响应。本规范基于项目现有的MakeMenuView、MakeView、PackageView等实现总结而来，并采用BaseView统一资源管理架构。

## 基础结构规范

### 类声明和继承
**所有View必须继承BaseView，获得统一的资源管理能力：**
```csharp
/// <summary>
/// 视图功能描述 - 详细说明视图职责和交互逻辑
/// </summary>
public class ExampleView : BaseView  // ✅ 继承BaseView，不是MonoBehaviour
{
    // 类实现
}
```

### 生命周期方法
每个View必须包含以下生命周期方法：
```csharp
void Start()
{
    InitializeView();
    SubscribeEvents();
}

// ✅ 使用OnViewDestroy替代OnDestroy
protected override void OnViewDestroy()
{
    UnsubscribeEvents();
    // 注意：资源会自动释放，无需手动处理
}
```

## 资源管理规范

### BaseView资源管理API
继承BaseView后，可直接使用以下资源管理方法：

```csharp
// 图片资源加载
protected bool LoadAndSetSprite(Image image, string spritePath)
protected bool LoadAndSetSprite(string imagePath, string spritePath)

// 物品图标加载
protected bool LoadAndSetItemIcon(string imagePath, int itemId)

// 通用资源加载
protected T LoadResource<T>(string path) where T : Object
```

### 资源使用示例
```csharp
private void InitializeView()
{
    // ✅ 一行代码设置物品图标，自动管理资源生命周期
    LoadAndSetItemIcon("img_icon", 1000);
    
    // ✅ 一行代码设置背景图片
    LoadAndSetSprite("img_background", "UI/background");
    
    // ✅ 加载其他类型资源
    var audioClip = LoadResource<AudioClip>("Audio/UI/click");
    var effectPrefab = LoadResource<GameObject>("Prefabs/Effects/ui_glow");
}
```

## 组件查找规范

### UI组件声明
- **禁止使用[SerializeField]**，所有UI组件通过代码动态查找
- 使用private字段存储UI组件引用
- 组件字段命名：使用组件类型简写+下划线+功能名称

```csharp
private TextMeshProUGUI txt_title;
private Button btn_close;
private Toggle toggle_option;
private Image img_icon;
private Transform container_list;
```

### 组件查找模式
在InitializeView()方法中进行组件查找：
```csharp
private void InitializeView()
{
    // 查找UI组件（如果未找到则为null，不报错）
    txt_title = transform.Find("txt_title")?.GetComponent<TextMeshProUGUI>();
    btn_close = transform.Find("btn_close")?.GetComponent<Button>();
    
    // 设置初始状态
    SetViewVisible(false);
}
```

### UIList组件查找
对于包含UIList的视图，使用标准查找模式：
```csharp
private UIList GetUIList()
{
    Transform listTransform = transform.Find("list_content") ?? FindChildWithUIList();
    return listTransform?.GetComponent<UIList>() ?? listTransform?.GetComponentInChildren<UIList>();
}

private Transform FindChildWithUIList()
{
    for (int i = 0; i < transform.childCount; i++)
    {
        Transform child = transform.GetChild(i);
        if (child.GetComponent<UIList>() != null) return child;
        
        var childUIList = child.GetComponentInChildren<UIList>();
        if (childUIList != null) return childUIList.transform;
    }
    return null;
}
```

## 事件系统规范

### 事件订阅模式
```csharp
private void SubscribeEvents()
{
    EventManager.Instance.Subscribe<ExampleEvent>(OnExampleEvent);
    EventManager.Instance.Subscribe<CloseUIEvent>(OnCloseUI);
}

private void UnsubscribeEvents()
{
    EventManager.Instance.Unsubscribe<ExampleEvent>(OnExampleEvent);
    EventManager.Instance.Unsubscribe<CloseUIEvent>(OnCloseUI);
}
```

### 事件处理方法
```csharp
private void OnExampleEvent(ExampleEvent eventData)
{
    if (eventData == null || !ValidateEventData(eventData))
    {
        Debug.LogWarning("Invalid event data received");
        return;
    }
    
    // 处理事件逻辑
    ProcessEventData(eventData);
}
```

## 数据交互规范

### Model数据获取
```csharp
private void LoadViewData()
{
    var data = ExampleModel.Instance.GetViewData();
    if (data == null)
    {
        Debug.LogError("Failed to load view data");
        return;
    }
    
    UpdateViewWithData(data);
}
```

### 配置数据读取
```csharp
private void LoadConfigData()
{
    var reader = ConfigManager.Instance.GetReader("ConfigTableName");
    if (reader == null)
    {
        Debug.LogError("Failed to get config reader");
        return;
    }
    
    // 使用配置数据
    ProcessConfigData(reader);
}
```

## UI列表管理规范

### 列表初始化
```csharp
private void InitializeList()
{
    UIList uiList = GetUIList();
    if (uiList == null)
    {
        Debug.LogError("UIList component not found");
        return;
    }
    
    uiList.RemoveAll();
    CreateListItems(uiList);
}
```

### 列表项创建
```csharp
private void CreateListItems(UIList uiList)
{
    var dataList = GetListData();
    
    foreach (var data in dataList)
    {
        GameObject item = uiList.AddListItem();
        if (item != null)
        {
            SetupListItem(item, data);
        }
    }
}

private void SetupListItem(GameObject item, DataType data)
{
    // 设置文本
    var txtName = item.transform.Find("txt_name")?.GetComponent<TextMeshProUGUI>();
    if (txtName != null)
    {
        txtName.text = data.Name;
    }
    
    // ✅ 设置列表项图标（使用ResourceUtils或手动处理）
    var imgIcon = item.transform.Find("img_icon")?.GetComponent<Image>();
    if (imgIcon != null && data.IconId > 0)
    {
        // 方式1：使用ResourceUtils（不自动管理，适用于列表项）
        ResourceUtils.LoadAndSetItemIcon(imgIcon, data.IconId);
        
        // 方式2：使用BaseView的LoadResource，手动设置图标
        // var iconPath = GetItemIconPath(data.IconId);
        // var sprite = LoadResource<Sprite>(iconPath);
        // if (sprite != null) imgIcon.sprite = sprite;
    }
    
    // 设置按钮交互
    var button = item.GetComponent<Button>();
    if (button != null)
    {
        button.onClick.AddListener(() => OnItemClick(data.Id));
    }
}
```

## 交互处理规范

### 按钮交互
```csharp
private void SetupButtonInteractions()
{
    if (btn_close != null)
    {
        btn_close.onClick.AddListener(OnCloseClick);
    }
}

private void OnCloseClick()
{
    CloseView();
}
```

### Toggle交互
```csharp
private void SetupToggleInteractions()
{
    if (toggle_option != null)
    {
        toggle_option.onValueChanged.AddListener(OnToggleChanged);
    }
}

private void OnToggleChanged(bool isOn)
{
    // 处理Toggle状态变化
    HandleToggleState(isOn);
}
```

## 视图状态管理

### 显示/隐藏控制

#### 推荐方式：使用UIManager
```csharp
// ✅ 推荐：通过UIManager统一管理UI显示
public void ShowView()
{
    UIManager.Instance.Show<YourView>();
    RefreshViewContent();
}

public void CloseView()
{
    UIManager.Instance.Hide<YourView>();
    CleanupViewState();
}

// ✅ 检查UI状态
public bool IsViewVisible()
{
    return UIManager.Instance.IsVisible<YourView>();
}
```

#### 传统方式：直接控制GameObject（兼容现有代码）
```csharp
// ⚠️ 传统方式：仍可使用，但推荐迁移到UIManager
private void SetViewVisible(bool visible)
{
    gameObject.SetActive(visible);
}

public void ShowView()
{
    SetViewVisible(true);
    RefreshViewContent();
}

public void CloseView()
{
    SetViewVisible(false);
    CleanupViewState();
}
```

### 状态字段管理
```csharp
private int _currentSelectedId = -1;
private bool _isViewInitialized = false;
private const string DEFAULT_TITLE = "默认标题";
```

## 性能优化规范

### 字符串操作优化
```csharp
// 使用常量避免字符串重复分配
private const string TITLE_PREFIX = "标题 - ";

private void UpdateTitle(string content)
{
    if (txt_title != null && !string.IsNullOrEmpty(content))
    {
        txt_title.text = TITLE_PREFIX + content;
    }
}
```

### 空值检查
```csharp
// 组件查找后进行空值检查
if (txt_display != null)
{
    txt_display.text = displayText;
}
```

## 命名规范

### 字段命名
- UI组件：`组件类型缩写_功能名称`，如`txt_title`、`btn_close`
- 私有字段：camelCase，可选下划线前缀，如`_currentState`
- 常量：UPPER_SNAKE_CASE，如`DEFAULT_TITLE`

### 方法命名
- 公共方法：PascalCase，如`ShowView()`、`CloseView()`
- 私有方法：PascalCase，如`InitializeView()`、`SetupListItem()`
- 事件处理：以`On`开头，如`OnCloseClick()`、`OnItemSelected()`

### 事件处理方法
- 格式：`On` + `事件源` + `动作`，如`OnCloseClick`、`OnItemToggle`

## 代码组织规范

### 方法顺序
1. Unity生命周期方法（Start、OnDestroy等）
2. 初始化方法（InitializeView、SubscribeEvents等）
3. 事件处理方法（OnXXXEvent）
4. UI交互方法（OnXXXClick）
5. 数据处理方法
6. 工具方法

### 注释规范
- 复杂方法使用XML文档注释
- 简单方法使用单行注释
- 关键业务逻辑必须添加注释

```csharp
/// <summary>
/// 根据制作类型ID加载对应的菜单内容
/// 清理现有内容后动态创建新的制作配方UI元素
/// </summary>
private void LoadMenuContent(int typeId)
{
    // 实现逻辑
}

// 设置菜单的可见性
private void SetMenuVisible(bool visible)
{
    gameObject.SetActive(visible);
}
```

## 最佳实践

### 1. 容错处理
- 所有外部数据获取都要进行空值检查
- UI组件查找失败时不应报错，只记录警告

### 2. 状态管理
- 维护视图的当前状态字段
- 提供状态重置和清理方法

### 3. 事件驱动
- 优先使用事件系统进行组件间通信
- 避免直接引用其他View组件

### 4. 数据分离
- View只负责显示和交互，不处理业务逻辑
- 所有数据操作通过Model进行

### 5. 资源管理（BaseView架构）
- **自动资源释放**：继承BaseView后，所有通过LoadResource加载的资源会在View销毁时自动释放
- **使用BaseView API**：优先使用LoadAndSetSprite、LoadAndSetItemIcon等便捷方法
- **避免手动资源管理**：不要在OnViewDestroy中手动释放资源，BaseView会自动处理
- **事件订阅清理**：在OnViewDestroy中清理事件订阅和协程等非资源对象

```csharp
protected override void OnViewDestroy()
{
    // ✅ 只处理事件和协程清理
    UnsubscribeEvents();
    if (_updateCoroutine != null)
    {
        StopCoroutine(_updateCoroutine);
    }
    
    // ❌ 不要手动释放资源，BaseView自动处理
    // ResourceManager.Instance.Release(sprite); // 错误做法
}
```

### 6. UIManager集成（推荐）
- **统一管理**：通过UIManager统一控制UI显示、隐藏、销毁
- **层级控制**：支持UI层级管理，自动处理显示顺序
- **事件驱动**：UI显示/隐藏自动发布事件，便于其他系统响应
- **全局控制**：支持一键隐藏/显示所有UI

```csharp
// ✅ UIManager集成示例
public class ExampleView : BaseView
{
    void Start()
    {
        InitializeView();
        SubscribeEvents();
        // 不需要手动设置显示状态，由UIManager控制
    }
    
    private void OnCloseButtonClick()
    {
        // 通过UIManager隐藏自己
        UIManager.Instance.Hide<ExampleView>();
    }
}

// ✅ 在其他系统中显示UI
public class SomeController : MonoBehaviour
{
    private void ShowInventory()
    {
        UIManager.Instance.Show<InventoryView>(UILayer.Popup);
    }
    
    private void HideAllUI()
    {
        UIManager.Instance.HideAll(); // 游戏暂停时隐藏所有UI
    }
}
```

### 7. 架构优势
- **统一继承**：所有View继承BaseView，架构统一
- **资源安全**：自动资源管理，避免内存泄漏
- **代码简洁**：一行代码完成资源加载和UI设置
- **易于维护**：标准化的生命周期管理
- **集中管理**：UIManager提供统一的UI控制入口

## 参考代码位置

本规范基于以下项目文件总结：
- `Assets/Scripts/UI/Base/BaseView.cs` (View基类，资源管理核心)
- `Assets/Scripts/UI/Base/BaseViewExample.cs` (BaseView使用示例)
- `Assets/Scripts/Manager/UIManager.cs` (UI管理器，统一UI控制) 🔥**新增**
- `Assets/Scripts/UI/Make/MakeMenuView.cs` (完整的复杂View示例)
- `Assets/Scripts/UI/Make/MakeView.cs` (简单的列表View示例)  
- `Assets/Scripts/UI/Package/PackageView.cs` (数据驱动View示例)

开发新的View时，建议：
1. 继承BaseView而不是MonoBehaviour
2. 参考BaseViewExample.cs学习资源管理用法
3. **使用UIManager进行UI显示控制（推荐）** 🔥**新增**
4. 参考现有View实现业务逻辑
5. 创建对应的UI预制体放置在`Assets/Resources/Prefabs/UI/`

## 迁移指南

### 现有View改造步骤

#### BaseView迁移（必选）
1. **修改继承关系**：`MonoBehaviour` → `BaseView`
2. **修改生命周期**：`OnDestroy()` → `OnViewDestroy()`
3. **使用资源管理API**：将手动资源加载改为BaseView的便捷方法
4. **移除手动资源释放代码**：BaseView自动处理

#### UIManager集成（推荐）
5. **移除SerializeField**：按照项目规范，所有UI组件通过代码查找
6. **创建UI预制体**：将View制作为预制体，放置在`Assets/Resources/Prefabs/UI/`
7. **替换显示控制**：将`gameObject.SetActive()`替换为`UIManager.Instance.Show/Hide<T>()`
8. **更新外部调用**：其他系统通过UIManager显示UI，而非直接操作GameObject

### 改造示例

#### BaseView迁移示例
```csharp
// ❌ 改造前
public class ItemView : MonoBehaviour
{
    private void OnDestroy()
    {
        // 手动资源管理代码
        foreach (var sprite in loadedSprites)
        {
            ResourceManager.Instance.Release(sprite);
        }
    }
}

// ✅ 改造后
public class ItemView : BaseView
{
    private void Start()
    {
        // 使用BaseView API，自动资源管理
        LoadAndSetItemIcon("img_icon", 1000);
    }
    
    protected override void OnViewDestroy()
    {
        // 只处理事件清理，资源自动释放
        UnsubscribeEvents();
    }
}
```

#### UIManager集成迁移示例
```csharp
// ❌ 传统方式
public class InventoryController : MonoBehaviour
{
    [SerializeField] private GameObject inventoryView;
    
    private void OpenInventory()
    {
        inventoryView.SetActive(true);
    }
    
    private void CloseInventory()
    {
        inventoryView.SetActive(false);
    }
}

// ✅ UIManager方式
public class InventoryController : MonoBehaviour
{
    private void OpenInventory()
    {
        UIManager.Instance.Show<InventoryView>(UILayer.Popup);
    }
    
    private void CloseInventory()
    {
        UIManager.Instance.Hide<InventoryView>();
    }
    
    private void PauseGame()
    {
        // 游戏暂停时隐藏所有UI
        UIManager.Instance.HideAll();
    }
}
``` 