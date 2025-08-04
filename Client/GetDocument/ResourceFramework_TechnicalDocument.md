# Resource框架技术文档

## 简介

基于Unity Resources系统的轻量级资源管理工具，提供资源加载、缓存和自动回收功能。

## 详细接口

### ResourceManager

**核心类**：`ResourceManager`（单例模式）
- **用途**：统一管理游戏中所有资源的加载、缓存和释放

#### 主要接口

```csharp
// 单例访问
public static ResourceManager Instance { get; }

// 加载资源（泛型方法）
public T Load<T>(string path) where T : UnityEngine.Object

// 释放资源
public void Release(UnityEngine.Object obj)

// 清理未使用的资源
public void UnloadUnusedAssets()
```

### ResourceCache

**核心类**：`ResourceCache`
- **用途**：管理单个资源的引用计数和生命周期

#### 主要属性和方法

```csharp
// 缓存的资源对象
public UnityEngine.Object Resource { get; private set; }

// 资源路径
public string Path { get; private set; }

// 增加引用计数
public void Retain()

// 减少引用计数，返回是否可以释放
public bool Release()
```

## 最佳实践

### 1. 基础资源加载

```csharp
public class WeaponLoader : MonoBehaviour
{
    private GameObject _weaponPrefab;
    private GameObject _weaponInstance;
    
    private void Start()
    {
        // 加载武器预制体
        _weaponPrefab = ResourceManager.Instance.Load<GameObject>("Prefabs/Equips/pbsc_equip_axe");
        if (_weaponPrefab != null)
        {
            _weaponInstance = Instantiate(_weaponPrefab);
        }
    }
    
    private void OnDestroy()
    {
        // 销毁实例
        if (_weaponInstance != null)
        {
            Destroy(_weaponInstance);
        }
        
        // 释放资源引用
        if (_weaponPrefab != null)
        {
            ResourceManager.Instance.Release(_weaponPrefab);
            _weaponPrefab = null;
        }
    }
}
```

### 2. 多种资源类型加载

```csharp
public class ResourceLoader : MonoBehaviour
{
    private AudioClip _audioClip;
    private Texture2D _texture;
    private Material _material;
    
    private void LoadResources()
    {
        // 加载音频
        _audioClip = ResourceManager.Instance.Load<AudioClip>("Audio/BGM/main_theme");
        
        // 加载贴图
        _texture = ResourceManager.Instance.Load<Texture2D>("Texture/UI/button_normal");
        
        // 加载材质
        _material = ResourceManager.Instance.Load<Material>("Material/pbsc_ground");
    }
    
    private void ReleaseResources()
    {
        // 按加载顺序释放
        if (_audioClip != null)
        {
            ResourceManager.Instance.Release(_audioClip);
            _audioClip = null;
        }
        
        if (_texture != null)
        {
            ResourceManager.Instance.Release(_texture);
            _texture = null;
        }
        
        if (_material != null)
        {
            ResourceManager.Instance.Release(_material);
            _material = null;
        }
    }
}
```

### 3. 批量资源管理

```csharp
public class InventoryManager : MonoBehaviour
{
    private List<GameObject> _itemPrefabs = new List<GameObject>();
    
    private void LoadAllItems()
    {
        string[] itemPaths = {
            "Prefabs/Items/wood",
            "Prefabs/Items/stone", 
            "Prefabs/Items/iron_ingot"
        };
        
        foreach (string path in itemPaths)
        {
            GameObject item = ResourceManager.Instance.Load<GameObject>(path);
            if (item != null)
            {
                _itemPrefabs.Add(item);
            }
        }
    }
    
    private void OnDestroy()
    {
        // 批量释放资源
        foreach (GameObject item in _itemPrefabs)
        {
            if (item != null)
            {
                ResourceManager.Instance.Release(item);
            }
        }
        _itemPrefabs.Clear();
    }
}
```

## 注意事项

### 1. 资源路径规范
- 路径相对于`Assets/Resources/`目录
- 不包含文件扩展名
- 使用正斜杠分隔路径：`"Prefabs/Equips/pbsc_equip_axe"`

### 2. 生命周期管理
- 每个`Load()`调用必须对应一个`Release()`调用
- 在`OnDestroy()`中释放所有加载的资源
- 先销毁实例化对象，再释放原始资源

### 3. 性能优化
- 资源会自动缓存，重复加载同一路径不会重复IO
- 使用`UnloadUnusedAssets()`清理长期未使用的资源
- 避免频繁加载/释放，合理规划资源生命周期

### 4. 错误处理
- 加载失败时返回`null`，务必检查返回值
- 释放`null`对象是安全的，内部已做空检查
- 查看Console日志了解资源加载/卸载状态 