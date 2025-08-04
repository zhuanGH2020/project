# BaseView技术文档

*最后更新：2024年 - 新增PNG纹理支持，统一图片加载API*

## 简介

View资源管理解决方案，包含ResourceUtils（纯工具类）和BaseView（基类）两个核心组件，通过继承方式提供资源管理功能，实现零重复代码和自动生命周期管理。支持图集和PNG纹理两种加载模式。

## 详细接口

### ResourceUtils（静态工具类）

**核心类**：`ResourceUtils`（静态类）
- **用途**：提供纯功能性的资源加载方法，无状态设计

#### 基础资源加载
```csharp
// 泛型资源加载
public static T LoadResource<T>(string path) where T : Object

// 常用类型快捷方法
public static Sprite LoadSprite(string path)
public static GameObject LoadGameObject(string path) 
public static AudioClip LoadAudioClip(string path)
```

#### 图片设置工具
```csharp
// 加载并设置图片（支持图集和PNG纹理）
public static bool LoadAndSetSprite(Image image, string imagePath, bool isAtlas = true, List<Object> cache = null)

// 从PNG纹理文件创建Sprite
public static Sprite LoadSpriteFromTexture(string path)

// 从配置加载物品图标（默认使用PNG模式）
public static bool LoadAndSetItemIcon(Image image, int itemId, bool isAtlas = false, List<Object> cache = null)
```

#### 资源释放工具
```csharp
// 释放资源列表
public static void ReleaseResources(List<Object> resources)

// 释放单个资源
public static void ReleaseResource(Object resource)
```

#### 配置工具
```csharp
// 获取物品图标路径
public static string GetItemIconPath(int itemId)

// 获取物品名称
public static string GetItemName(int itemId)
```

### BaseView（基类）

**核心类**：`BaseView`（抽象基类）
- **用途**：为所有View提供统一的资源管理功能
- **特点**：继承即获得资源管理，零重复代码

#### 继承方式
```csharp
// 所有View继承BaseView
public class YourView : BaseView
{
    // 自动获得资源管理功能
}
```

#### 核心API（protected方法）
```csharp
// 图片加载（支持图集和PNG纹理）
protected bool LoadAndSetSprite(Image image, string imagePath, bool isAtlas = true)
protected bool LoadAndSetSprite(string imagePath, string spritePath, bool isAtlas = true)

// 物品图标加载（默认使用PNG模式）
protected bool LoadAndSetItemIcon(string imagePath, int itemId, bool isAtlas = false)

// 通用资源加载
protected T LoadResource<T>(string path) where T : Object
```

#### 资源管理
```csharp
// 手动释放特定资源
protected void ReleaseResource(Object resource)

// 获取已加载资源数量
protected int LoadedResourceCount { get; }

// 子类重写此方法进行额外清理
protected virtual void OnViewDestroy()
```

## 最佳实践

### 1. 最简单用法

```csharp
public class SimpleItemView : BaseView
{
    private void Start()
    {
        // 一行代码设置物品图标（PNG纹理模式）
        LoadAndSetItemIcon("img_icon", 1000);
        
        // 设置背景图片（图集模式）
        LoadAndSetSprite("img_background", "UI/item_background");
        
        // 设置PNG图片（纹理模式）
        LoadAndSetSprite("img_photo", "UI/Photos/screenshot.png", false);
    }
    
    // 不需要OnDestroy，BaseView自动释放资源
}
```

### 2. 需要额外清理的View

```csharp
public class AdvancedItemView : BaseView
{
    private Coroutine _updateCoroutine;
    
    private void Start()
    {
        LoadAndSetItemIcon("img_icon", 1000);
        _updateCoroutine = StartCoroutine(UpdateLoop());
    }
    
    private System.Collections.IEnumerator UpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
        }
    }
    
    protected override void OnViewDestroy()
    {
        // 停止协程等额外清理工作
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
        }
        // 注意：资源会自动释放，无需手动处理
    }
}
```

### 3. 与现有View配合使用

```csharp
// 可以逐步改造现有View，将MonoBehaviour改为BaseView
public class YourExistingView : BaseView  // 原来继承MonoBehaviour
{
    private void Start()
    {
        // 原有逻辑保持不变
        InitializeUI();
        
        // 新增：使用BaseView的资源管理功能
        LoadAndSetItemIcon("img_icon", 1000);
    }
    
    private void InitializeUI()
    {
        // 原有的UI初始化代码
    }
    
    // 原有的OnDestroy可以删除或改为OnViewDestroy
}

## 注意事项

### 1. 架构选择
- **BaseView继承**：推荐方式，零重复代码，最简洁的API
- **ResourceUtils静态方法**：适用于非View类或特殊需求

### 2. 继承规范
- **所有View继承BaseView**：获得自动资源管理功能
- **使用protected方法**：子类直接调用资源加载方法
- **重写OnViewDestroy**：仅在需要额外清理时重写

### 3. 资源路径规范
- **图片路径**：相对于`Assets/Resources/`，如`"Icons/Items/wood"`
- **UI组件路径**：相对于当前Transform，如`"img_icon"`
- **配置依赖**：自动从Item.csv读取IconPath字段
- **扩展名处理**：方法内部自动移除`.png`等扩展名

### 4. 图集vs纹理模式选择
- **图集模式**（`isAtlas = true`）：适用于Unity中已设置为Sprite类型的资源
- **纹理模式**（`isAtlas = false`）：适用于PNG/JPG等图片文件，运行时创建Sprite
- **物品图标**：默认使用纹理模式，因为通常来自配置的PNG文件路径
- **UI图片**：默认使用图集模式，因为UI资源通常制作为Sprite

### 5. 性能优化
- BaseView按实例管理资源，避免全局状态
- 资源在ResourceManager层面自动缓存
- OnDestroy时统一释放，避免内存泄漏

### 6. 设计对比

```csharp
// ❌ 原来的写法：复杂且易出错
public class ItemView : MonoBehaviour
{
    private List<Object> _resources = new List<Object>();

    private void Start() {
        var config = ConfigManager.Instance.GetReader("Item");
        string iconPath = config?.GetValue<string>(1000, "IconPath", "") ?? "";
        
        // 手动处理扩展名
        string pathWithoutExtension = System.IO.Path.ChangeExtension(iconPath, null);
        
        // 手动加载纹理并创建Sprite
        var texture = ResourceManager.Instance.Load<Texture2D>(pathWithoutExtension);
        if (texture != null) {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            var img = transform.Find("img_icon").GetComponent<Image>();
            img.sprite = sprite;
            _resources.Add(texture);
        }
    }

    private void OnDestroy() {
        foreach (var resource in _resources) {
            ResourceManager.Instance.Release(resource);
        }
    }
}

// ✅ 现在的写法：简洁且安全
public class ItemView : BaseView
{
    private void Start() {
        LoadAndSetItemIcon("img_icon", 1000); // 一行代码，自动处理PNG纹理
    }
    
    // 自动处理资源释放
}
```

## 其他要点

- **代码参考位置**：
  - `Assets/Scripts/Utils/ResourceUtils.cs`（工具类）
  - `Assets/Scripts/UI/Base/BaseView.cs`（基类）
  - `Assets/Scripts/UI/Package/PackageView.cs`（实际使用示例）
- **遵循项目规范**：命名约定、注释规范、架构设计原则
- **向后兼容**：不影响现有ResourceManager的使用
- **继承设计**：符合OOP设计原则，is-a关系清晰 