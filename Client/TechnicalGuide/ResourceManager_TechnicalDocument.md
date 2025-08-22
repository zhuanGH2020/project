# ResourceManager 技术文档

## 简介
ResourceManager是一个基于Unity Resources系统的轻量级资源管理工具，主要用于管理小型项目中的资源加载和释放，通过引用计数机制实现资源的自动回收。

## 详细接口

```csharp
// 获取单例实例
public static ResourceManager Instance { get; }

// 加载资源
public T Load<T>(string path) where T : UnityEngine.Object

// 释放资源
public void Release(UnityEngine.Object obj)

// 清理未使用的资源
public void UnloadUnusedAssets()
```

## 最佳实践

### 1. 资源加载示例
```csharp
// 加载预制体
GameObject prefab = ResourceManager.Instance.Load<GameObject>("Prefabs/MyPrefab");
if (prefab != null)
{
    GameObject instance = Instantiate(prefab);
}

// 加载其他类型资源
Material material = ResourceManager.Instance.Load<Material>("Materials/MyMaterial");
AudioClip audio = ResourceManager.Instance.Load<AudioClip>("Audio/MySound");
```

### 2. 资源释放示例
```csharp
public class ResourceUser : MonoBehaviour
{
    private GameObject _prefab;
    
    private void Start()
    {
        _prefab = ResourceManager.Instance.Load<GameObject>("Prefabs/MyPrefab");
    }
    
    private void OnDestroy()
    {
        // 在不需要时释放资源
        if (_prefab != null)
        {
            ResourceManager.Instance.Release(_prefab);
            _prefab = null;
        }
    }
}
```

## 注意事项

1. **资源路径**
   - 所有资源必须放在Resources文件夹下
   - 路径参数不需要包含"Resources/"前缀
   - 不需要包含文件扩展名

2. **内存管理**
   - GameObject类型资源会依赖Unity自动回收
   - 如需立即释放内存，可调用UnloadUnusedAssets方法
   - 确保在对象销毁时调用Release方法

3. **性能考虑**
   - 适用于小型项目（资源总量建议控制在100MB以内）
   - 不支持热更新
   - 所有资源都会打包进安装包 