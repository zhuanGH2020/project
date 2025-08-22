# View开发规范

## 简介
View是Unity UI系统中的视图层组件，负责UI界面的显示、交互处理和用户输入响应。本规范基于项目现有的BaseView架构和实际View实现总结而来，采用标准Unity生命周期和Manager便捷调用模式。

## 基础结构规范

### 类声明和继承
**所有View必须继承BaseView，获得统一的Manager便捷调用能力：**
```csharp
/// <summary>
/// 视图功能描述 - 详细说明视图职责和交互逻辑
/// </summary>
public class ExampleView : BaseView  // ✅ 继承BaseView (MonoBehaviour)
{
    // 类实现
}
```

### 生命周期方法
每个View使用标准Unity生命周期方法：
```csharp
protected override void Start()
{
    base.Start();
    InitializeView();
    SubscribeEvents();
}

protected override void OnDestroy()
{
    base.OnDestroy();
    UnsubscribeEvents();
    // 手动清理资源和协程
}
```

## BaseView便捷API

### Manager调用方法
继承BaseView后，可直接使用以下便捷方法：

```csharp
// 资源加载
protected T LoadResource<T>(string path) where T : UnityEngine.Object

// 配置获取
protected ConfigReader GetConfig(string configName)

// 事件管理
protected void SubscribeEvent<T>(System.Action<T> handler) where T : IEvent
protected void UnsubscribeEvent<T>(System.Action<T> handler) where T : IEvent
protected void PublishEvent<T>(T eventData) where T : IEvent

// UI便捷方法
protected GameObject FindChildByName(string childName)
protected T FindChildComponent<T>(string childName) where T : Component
public void Show() // 显示View
public void Hide() // 隐藏View

// 图片加载（调用ResourceUtils）
protected bool LoadAndSetSprite(Image image, string imagePath, bool isAtlas = true)
```

## 资源管理规范

### 使用ResourceUtils工具类
项目提供ResourceUtils工具类进行资源加载和管理：

```csharp
private void InitializeView()
{
    // ✅ 使用ResourceUtils加载并设置物品图标
    var imgIcon = FindChildComponent<Image>("img_icon");
    ResourceUtils.LoadAndSetItemIcon(imgIcon, itemId);
    
    // ✅ 使用ResourceUtils加载并设置背景图片
    var imgBackground = FindChildComponent<Image>("img_background");
    ResourceUtils.LoadAndSetSprite(imgBackground, "UI/background");
    
    // ✅ 使用BaseView便捷方法加载资源
    var audioClip = LoadResource<AudioClip>("Audio/UI/click");
    var effectPrefab = LoadResource<GameObject>("Prefabs/Effects/ui_glow");
}
```

### 资源释放管理
```csharp
private List<Object> _loadedResources = new List<Object>();

private void LoadResourcesWithCache()
{
    var imgIcon = FindChildComponent<Image>("img_icon");
    // 使用cache参数自动收集加载的资源
    ResourceUtils.LoadAndSetItemIcon(imgIcon, itemId, false, _loadedResources);
}

protected override void OnDestroy()
{
    base.OnDestroy();
    // 手动释放资源
    ResourceUtils.ReleaseResources(_loadedResources);
    UnsubscribeEvents();
}
```

## 组件查找规范

### UI组件声明
- **禁止使用[SerializeField]**，所有UI组件通过代码动态查找（过渡期可暂时保留）
- 使用private字段存储UI组件引用
- 组件字段命名：使用组件类型简写+下划线+功能名称

```csharp
private TextMeshProUGUI txt_title;
private Button btn_close;
private Toggle toggle_option;
private Image img_icon;
private Transform container_list;
private Slider slider_health;
```

### 组件查找模式
在InitializeView()方法中进行组件查找：
```csharp
private void InitializeView()
{
    // 使用BaseView便捷方法查找组件
    txt_title = FindChildComponent<TextMeshProUGUI>("txt_title");
    btn_close = FindChildComponent<Button>("btn_close");
    
    // 或使用传统transform.Find方式
    slider_health = transform.Find("slider_health")?.GetComponent<Slider>();
    
    // 组件未找到时的错误处理
    if (slider_health == null)
    {
        Debug.LogError("[ExampleView] slider_health component not found");
    }
    
    // 设置初始状态
    Hide(); // 或 gameObject.SetActive(false);
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
    // 使用BaseView便捷方法
    SubscribeEvent<ExampleEvent>(OnExampleEvent);
    SubscribeEvent<CloseUIEvent>(OnCloseUI);
    
    // 或使用EventManager直接调用
    EventManager.Instance.Subscribe<ItemChangeEvent>(OnItemChanged);
}

private void UnsubscribeEvents()
{
    // 使用BaseView便捷方法
    UnsubscribeEvent<ExampleEvent>(OnExampleEvent);
    UnsubscribeEvent<CloseUIEvent>(OnCloseUI);
    
    // 或使用EventManager直接调用
    EventManager.Instance.Unsubscribe<ItemChangeEvent>(OnItemChanged);
}
```

### 事件处理方法
```csharp
private void OnExampleEvent(ExampleEvent eventData)
{
    if (eventData == null)
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
    // 使用BaseView便捷方法
    var reader = GetConfig("ConfigTableName");
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
    
    // ✅ 设置列表项图标（使用ResourceUtils）
    var imgIcon = item.transform.Find("img_icon")?.GetComponent<Image>();
    if (imgIcon != null && data.IconId > 0)
    {
        // 使用ResourceUtils工具方法
        ResourceUtils.LoadAndSetItemIcon(imgIcon, data.IconId);
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
    // 使用UIManager关闭（推荐）
    UIManager.Instance.Hide<ExampleView>();
    
    // 或传统方式
    // Hide();
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
    return gameObject.activeInHierarchy;
}
```

#### 传统方式：直接控制GameObject
```csharp
// ⚠️ 传统方式：仍可使用，但推荐迁移到UIManager
public void ShowView()
{
    Show(); // 或 gameObject.SetActive(true);
    RefreshViewContent();
}

public void CloseView()
{
    Hide(); // 或 gameObject.SetActive(false);
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
- UI组件查找失败时应记录错误日志，便于调试

### 2. 状态管理
- 维护视图的当前状态字段
- 提供状态重置和清理方法

### 3. 事件驱动
- 优先使用事件系统进行组件间通信
- 避免直接引用其他View组件

### 4. 数据分离
- View只负责显示和交互，不处理业务逻辑
- 所有数据操作通过Model进行

### 5. 资源管理
- **使用ResourceUtils工具类**：统一的资源加载和管理接口
- **缓存机制**：使用cache参数收集加载的资源，便于后续释放
- **手动资源清理**：在OnDestroy中清理事件订阅、协程和资源

```csharp
private List<Object> _loadedResources = new List<Object>();

protected override void OnDestroy()
{
    base.OnDestroy();
    
    // ✅ 清理事件订阅
    UnsubscribeEvents();
    
    // ✅ 清理协程
    if (_updateCoroutine != null)
    {
        StopCoroutine(_updateCoroutine);
    }
    
    // ✅ 释放加载的资源
    ResourceUtils.ReleaseResources(_loadedResources);
}
```

### 6. UIManager集成（推荐）
- **统一管理**：通过UIManager统一控制UI显示、隐藏、销毁
- **层级控制**：支持UI层级管理，自动处理显示顺序
- **事件驱动**：UI显示/隐藏自动发布事件，便于其他系统响应

```csharp
// ✅ UIManager集成示例
public class ExampleView : BaseView
{
    protected override void Start()
    {
        base.Start();
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

## 参考代码位置

本规范基于以下项目文件总结：
- `Assets/Scripts/UI/Base/BaseView.cs` (View基类，Manager便捷调用)
- `Assets/Scripts/Manager/UIManager.cs` (UI管理器，统一UI控制)
- `Assets/Scripts/Utils/ResourceUtils.cs` (资源工具类，统一资源管理) 
- `Assets/Scripts/UI/Menu/MenuView.cs` (简洁的View示例)
- `Assets/Scripts/UI/PlayerInfo/PlayerInfoView.cs` (标准View示例)
- `Assets/Scripts/UI/Package/PackageView.cs` (复杂View示例)
- `Assets/Scripts/UI/UIDlg/NoticeView.cs` (事件驱动View示例)

开发新的View时，建议：
1. 继承BaseView获得Manager便捷调用能力
2. 使用ResourceUtils进行统一资源管理
3. **优先使用UIManager进行UI显示控制（推荐）**
4. 参考现有View实现业务逻辑
5. 创建对应的UI预制体放置在`Assets/Resources/Prefabs/UI/`

## 迁移指南

### 现有View改造步骤

#### 基础改造（必选）
1. **确认继承关系**：确保继承BaseView而非直接继承MonoBehaviour
2. **使用便捷方法**：将EventManager.Instance调用改为使用BaseView便捷方法
3. **使用ResourceUtils**：将手动资源加载改为ResourceUtils工具类
4. **手动资源管理**：在OnDestroy中清理事件订阅和资源

#### UIManager集成（推荐）
5. **移除SerializeField**：按照项目规范，所有UI组件通过代码查找
6. **创建UI预制体**：将View制作为预制体，放置在`Assets/Resources/Prefabs/UI/`
7. **替换显示控制**：将`gameObject.SetActive()`替换为`UIManager.Instance.Show/Hide<T>()`
8. **更新外部调用**：其他系统通过UIManager显示UI，而非直接操作GameObject

### 改造示例

#### 基础改造示例
```csharp
// ❌ 改造前
public class ItemView : BaseView
{
    private void Start()
    {
        EventManager.Instance.Subscribe<ItemEvent>(OnItemEvent);
        
        // 手动加载资源
        var iconSprite = ResourceManager.Instance.Load<Sprite>("UI/Icons/item_icon");
        imgIcon.sprite = iconSprite;
    }
    
    private void OnDestroy()
    {
        EventManager.Instance.Unsubscribe<ItemEvent>(OnItemEvent);
    }
}

// ✅ 改造后
public class ItemView : BaseView
{
    private List<Object> _loadedResources = new List<Object>();
    
    protected override void Start()
    {
        base.Start();
        SubscribeEvent<ItemEvent>(OnItemEvent); // 使用BaseView便捷方法
        
        // 使用ResourceUtils统一资源管理
        ResourceUtils.LoadAndSetItemIcon(imgIcon, itemId, false, _loadedResources);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeEvent<ItemEvent>(OnItemEvent); // 使用BaseView便捷方法
        ResourceUtils.ReleaseResources(_loadedResources); // 清理资源
    }
}