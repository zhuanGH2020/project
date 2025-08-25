# Building建筑系统技术文档

**版本**: 2.0  
**创建日期**: 2024年12月  
**更新日期**: 2024年12月  

## 简介

Building是游戏中所有建筑物的基类，提供统一的建筑管理、智能交互、寻路导航和配置驱动功能。系统采用面向对象继承设计，支持灵活的建筑扩展和自定义交互逻辑。

## 详细接口

### 核心类结构

#### Building基类
**位置**: `Assets/Scripts/Object/Base/Building.cs`

```csharp
/// <summary>
/// 建筑物基类 - 管理建筑物的数据和行为
/// 挂载到建筑物预制体上，用于管理唯一标识、等级、交互状态等
/// </summary>
public class Building : DamageableObject
{
    // 核心属性
    public int ItemId => _itemId;              // 对应的道具ID
    public Vector2 MapPosition => _mapPosition; // 地图位置
    public int Level => _level;                 // 建筑物等级
    public bool IsConstructed => _isConstructed; // 是否建造完成
    
    // 建筑管理方法
    public void Initialize(int itemId, Vector2 mapPos, int uid = 0)
    public string GetBuildingName()
    public bool UpgradeLevel()
    public void Demolish()
    
    // 交互系统
    public override void OnClick(Vector3 clickPosition)
    protected virtual void OnInteract(Vector3 clickPosition)
}
```

### 继承体系

```
ObjectBase (基类)
    ↓
DamageableObject (可承伤基类)
    ↓
Building (建筑基类) ←── 你在这里
    ↓
├── HarvestableObject (可采集物)
├── CookingPot (烹饪锅)
├── TechStation (科技台)
└── CustomBuilding (自定义建筑)
```

### 核心特性

#### 1. 智能交互系统
- **自动寻路**: 点击建筑时玩家自动寻路到交互范围
- **虚方法重写**: 子类通过重写OnInteract实现自定义交互
- **容错处理**: 移动失败时支持远程交互
- **超时保护**: 10秒超时机制防止卡死

#### 2. 配置驱动管理
- **Item.csv集成**: 从配置表读取建筑属性和血量
- **地图系统集成**: 与MapModel完全集成，支持保存加载
- **UID管理**: 自动生成和管理建筑唯一标识

#### 3. 建筑生命周期
- **初始化**: Initialize()设置基础属性
- **升级系统**: UpgradeLevel()支持建筑升级
- **拆除机制**: Demolish()安全清理建筑数据

## 核心方法详解

### 建筑初始化
```csharp
/// <summary>
/// 初始化建筑物 - 设置ID、位置和UID
/// </summary>
/// <param name="itemId">物品ID</param>
/// <param name="mapPos">地图位置</param>
/// <param name="uid">唯一标识（可选）</param>
public void Initialize(int itemId, Vector2 mapPos, int uid = 0)
{
    _itemId = itemId;
    _mapPosition = mapPos;
    
    // UID管理
    if (uid > 0)
        SetUid(uid);
    else if (Uid == 0)
        SetUid(ResourceUtils.GenerateUid());
        
    _constructTime = Time.time;
    
    // 加载配置并更新GameObject名称
    LoadBuildingConfig();
    UpdateGameObjectName();
}
```

### 智能交互流程
```csharp
/// <summary>
/// 点击处理 - 包含完整的寻路和交互逻辑
/// </summary>
public override void OnClick(Vector3 clickPosition)
{
    if (!CanInteract) return;

    var player = Player.Instance;
    if (player != null)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float interactionRange = GetInteractionRange();

        // 距离检测
        if (distanceToPlayer <= interactionRange)
        {
            OnInteract(clickPosition);  // 直接交互
        }
        else
        {
            // 启动智能寻路
            Vector3 targetPosition = GetInteractionPosition(player.transform.position);
            bool moveStarted = player.MoveToPlayerPosition(targetPosition);
            
            if (moveStarted)
            {
                StartCoroutine(WaitForPlayerAndInteract(clickPosition));
            }
            else
            {
                OnInteract(clickPosition);  // 移动失败，远程交互
            }
        }
    }
}
```

### 交互位置计算
```csharp
/// <summary>
/// 计算合适的交互位置 - 在交互范围内但保持安全距离
/// </summary>
private Vector3 GetInteractionPosition(Vector3 playerPosition)
{
    Vector3 directionToPlayer = (playerPosition - transform.position).normalized;
    float interactionRange = GetInteractionRange();
    
    // 在交互范围内，但保持0.5米安全距离避免重叠
    float stopDistance = Mathf.Max(0.5f, interactionRange - 0.5f);
    return transform.position + directionToPlayer * stopDistance;
}
```

### 虚方法重写机制
```csharp
/// <summary>
/// 交互逻辑虚方法 - 子类重写实现自定义交互行为
/// </summary>
protected virtual void OnInteract(Vector3 clickPosition)
{
    // 默认实现：发布通用交互事件
    var interactionEvent = new ObjectInteractionEvent(this, clickPosition);
    EventManager.Instance.Publish(interactionEvent);
}
```

## 最佳实践

### 创建标准建筑
```csharp
// 1. 创建建筑GameObject
GameObject buildingGO = Instantiate(buildingPrefab, worldPosition, Quaternion.identity);

// 2. 获取Building组件
Building building = buildingGO.GetComponent<Building>();

// 3. 初始化建筑
building.Initialize(itemId: 20001, mapPos: new Vector2(x, y));

// 4. 注册到地图系统（可选）
MapModel.Instance.AddBuildingData(building.Uid, building.ItemId, building.MapPosition);
```

### 扩展自定义建筑
```csharp
/// <summary>
/// 自定义烹饪锅建筑
/// </summary>
public class CookingPot : Building
{
    [SerializeField] private bool _isCooking = false;
    [SerializeField] private float _cookingProgress = 0f;
    
    // 重写交互逻辑
    protected override void OnInteract(Vector3 clickPosition)
    {
        if (_isCooking)
        {
            CheckCookingProgress();
        }
        else
        {
            StartCookingUI();
        }
    }
    
    private void StartCookingUI()
    {
        // 打开烹饪界面
        EventManager.Instance.Publish(new CookingUIOpenEvent(transform.position));
    }
    
    private void CheckCookingProgress()
    {
        if (_cookingProgress >= 1.0f)
        {
            CompleteCooking();
        }
        else
        {
            ShowCookingProgress();
        }
    }
}
```

### 采集物建筑扩展
```csharp
/// <summary>
/// 可采集建筑物 - 继承Building获得寻路交互
/// </summary>
public class HarvestableObject : Building
{
    private bool _isHarvested;
    private int _requiredWeaponId;
    private bool _anyWeapon;
    
    // 重写交互为采集逻辑
    protected override void OnInteract(Vector3 clickPosition)
    {
        if (!CanInteract || _isHarvested) return;

        if (CanHarvest())
        {
            PerformHarvest();
        }
        else
        {
            ShowHarvestHint();
        }
    }
    
    private void PerformHarvest()
    {
        _isHarvested = true;
        
        // 添加到背包
        var success = PackageModel.Instance.AddItem(ItemId, 1);
        if (success)
        {
            EventManager.Instance.Publish(new NoticeEvent($"获得了 {GetBuildingName()}"));
        }
        
        // 播放特效并销毁
        PlayHarvestEffect();
        Destroy(gameObject, GetDestroyDelay());
    }
}
```

### 调试和测试
```csharp
/// <summary>
/// 调试工具 - 快速创建建筑用于测试
/// </summary>
public class BuildingDebugTools
{
    public static Building CreateTestBuilding(int itemId, Vector3 position)
    {
        // 从配置表获取预制体路径
        var itemConfig = ConfigManager.Instance.GetReader("Item");
        if (!itemConfig.HasKey(itemId))
        {
            Debug.LogError($"Building item {itemId} not found in config");
            return null;
        }
        
        string prefabPath = itemConfig.GetValue<string>(itemId, "PrefabPath", "");
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError($"No prefab path for building {itemId}");
            return null;
        }
        
        // 加载并创建建筑
        GameObject prefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found: {prefabPath}");
            return null;
        }
        
        GameObject buildingGO = Object.Instantiate(prefab, position, Quaternion.identity);
        Building building = buildingGO.GetComponent<Building>();
        
        if (building != null)
        {
            building.Initialize(itemId, new Vector2(position.x, position.z));
            Debug.Log($"Created test building {itemId} at {position}");
        }
        
        return building;
    }
}
```

## 配置集成

### Item.csv配置
Building从Item.csv读取建筑属性：

```csv
Id,Name,Type,MaxStack,IconPath,PrefabPath,Description,MaxHealth,MaxLevel
20001,工作台,Building,1,UI/Icon/Building/Workbench,Prefabs/Buildings/Workbench,基础制作台,200,3
20002,熔炉,Building,1,UI/Icon/Building/Furnace,Prefabs/Buildings/Furnace,冶炼设备,300,5
30005,浆果丛,Building,1,UI/Icon/Plant/Berry_Bush,Prefabs/Plants/pbsc_berry_bush,可采集浆果,50,1
```

### 配置加载逻辑
```csharp
private void LoadBuildingConfig()
{
    var reader = ConfigManager.Instance.GetReader("Item");
    if (reader != null)
    {
        _maxHealthValue = reader.GetValue<float>(_itemId, "MaxHealth", 100f);
        // 可以添加更多配置读取
    }
}
```

### 地图数据集成
```csharp
// Building与MapModel的集成
public class MapModel
{
    public void AddBuildingData(int uid, int itemId, Vector2 position)
    {
        var mapData = new MapData
        {
            uid = uid,
            itemId = itemId,
            posX = position.x,
            posY = position.y,
            type = ObjectType.Building
        };
        
        _mapDataList.Add(mapData);
        // 触发地图更新事件
        EventManager.Instance.Publish(new MapDataAddedEvent(mapData));
    }
}
```

## 系统集成

### 与其他系统的协作

#### 1. 对象管理系统
```csharp
// 自动注册到ObjectManager
protected override void Awake()
{
    base.Awake();
    SetObjectType(ObjectType.Building);
    // ObjectBase.Awake()会自动注册到ObjectManager
}
```

#### 2. 事件系统集成
```csharp
// Building生命周期事件
public event Action<Building> OnDemolished;
public event Action<Building, int> OnLevelChanged;
public event Action<Building, bool> OnInteractStateChanged;

// 事件触发示例
public bool UpgradeLevel()
{
    int oldLevel = _level;
    _level++;
    OnLevelChanged?.Invoke(this, oldLevel);
    return true;
}
```

#### 3. 保存系统集成
```csharp
// Building数据会自动被SaveManager收集和保存
// 通过MapModel.MapDataList进行序列化
```

#### 4. 资源管理集成
```csharp
// 预制体通过ResourceManager加载
GameObject prefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
```

## 注意事项

### 性能优化
1. **协程管理**: `WaitForPlayerAndInteract`协程在对象销毁时自动清理
2. **配置缓存**: 配置数据在Initialize时一次性加载并缓存
3. **距离计算**: 合理使用距离检测，避免频繁计算
4. **事件订阅**: 确保在OnDestroy中正确取消事件订阅

### 内存管理
```csharp
// Building的正确销毁流程
public void Demolish()
{
    // 1. 触发拆除事件
    OnDemolished?.Invoke(this);
    
    // 2. 从地图数据中移除
    MapModel.Instance.RemoveBuildingByUid(Uid);
    
    // 3. 销毁GameObject（会自动触发ObjectManager反注册）
    Destroy(gameObject);
}
```

### 线程安全
- Building主要在主线程操作，避免在其他线程直接访问
- 协程和Update方法都在主线程执行，线程安全

### 错误处理
```csharp
// 健壮的初始化检查
public void Initialize(int itemId, Vector2 mapPos, int uid = 0)
{
    if (itemId <= 0)
    {
        Debug.LogError("[Building] Invalid itemId: " + itemId);
        return;
    }
    
    var itemConfig = ConfigManager.Instance.GetReader("Item");
    if (itemConfig == null || !itemConfig.HasKey(itemId))
    {
        Debug.LogError($"[Building] Item config not found for ID: {itemId}");
        return;
    }
    
    // ... 正常初始化逻辑
}
```

## 其他需要补充的关键点

### 扩展性设计
- **虚方法模式**: 通过重写OnInteract实现不同建筑的交互逻辑
- **事件驱动**: 支持外部系统监听建筑状态变化
- **配置驱动**: 新建筑类型只需添加配置表条目
- **组件化**: 可以添加额外的MonoBehaviour组件扩展功能

### 调试支持
```csharp
// Inspector中显示的调试信息
[Header("Debug Info")]
[SerializeField] private string _debugInfo;

private void OnValidate()
{
    if (Application.isPlaying)
    {
        _debugInfo = $"ID:{ItemId} UID:{Uid} Level:{Level} Health:{CurrentHealth}/{MaxHealth}";
    }
}
```

### 版本兼容
- **向后兼容**: 新版本Building仍支持旧的IClickable接口
- **配置兼容**: 新增配置字段提供默认值，不影响现有建筑
- **API稳定**: 核心公共方法保持稳定，内部实现可以优化

---

创建日期：2024-12-19  
更新日期：2024-12-19  
版本：2.0.0 