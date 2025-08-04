# 通用采集系统技术文档

## 简介
通用采集系统提供点击物体→自动寻路→采集入背包的完整交互体验，支持草、树木、浆果等多种采集物，完全兼容现有功能。

## 详细接口

### 核心接口

#### IHarvestable - 可采集接口
```csharp
public interface IHarvestable
{
    bool CanHarvest { get; }                    // 是否可采集
    HarvestInfo GetHarvestInfo();               // 获取采集信息
    void OnHarvest(IAttacker harvester);        // 执行采集
}
```

#### IClickable - 可点击交互接口
```csharp
public interface IClickable
{
    bool CanInteract { get; }                   // 是否可交互
    void OnClick(Vector3 clickPosition);        // 处理点击
    float GetInteractionRange();                // 获取交互范围
}
```

### 数据结构

#### HarvestInfo - 采集信息
```csharp
public struct HarvestInfo
{
    public List<DropItem> drops;                // 掉落物品列表
    public float harvestTime;                   // 采集时间
    public bool destroyAfterHarvest;            // 采集后是否销毁
    public ActionType actionType;               // 特效类型
    public bool requiresTool;                   // 是否需要工具
    public ToolType requiredToolType;           // 需要的工具类型
}
```

#### DropItem - 掉落物品
```csharp
public struct DropItem
{
    public int itemId;           // 物品ID
    public int minCount;         // 最小掉落数量
    public int maxCount;         // 最大掉落数量
    public float dropRate;       // 掉落概率 (0.0-1.0)
    
    public int GetActualDropCount();  // 计算实际掉落数量
}
```

### 基础类

#### HarvestableObject - 采集物基类
```csharp
public abstract class HarvestableObject : DamageableObject, IHarvestable, IClickable
{
    // 配置字段
    [SerializeField] protected List<DropItem> _drops;
    [SerializeField] protected float _harvestTime;
    [SerializeField] protected bool _destroyAfterHarvest;
    [SerializeField] protected ActionType _actionType;
    [SerializeField] protected float _interactionRange;
    
    // 状态字段
    protected bool _isHarvested = false;
    protected bool _isBeingHarvested = false;
    
    // 核心方法
    public virtual void OnHarvest(IAttacker harvester);
    public virtual void OnClick(Vector3 clickPosition);
    protected virtual void OnHarvestComplete(IAttacker harvester);
    protected virtual void ProcessDrops(IAttacker harvester);
    protected virtual void PlayHarvestEffect();
}
```

#### 地面掉落物
```csharp
// 使用 DirectHarvestable 实现掉落物
public class DirectHarvestable : HarvestableObject
{
    public void SetItemId(int itemId);      // 设置物品ID
    public void SetDropCount(int count);    // 设置掉落数量
}
```

### 管理器

#### InteractionManager - 交互管理器
```csharp
public class InteractionManager : MonoBehaviour
{
    public bool IsInteracting { get; }          // 是否正在交互
    public IClickable CurrentTarget { get; }    // 当前目标
    
    private void StartInteraction(IClickable target, Vector3 clickPosition);
    private void PerformInteraction();
    private void CancelCurrentInteraction();
    private void UpdateTargetInteraction();
}
```

## 系统架构

### 交互流程
1. **点击检测**：玩家点击可交互物体
2. **事件发布**：`HarvestableObject.OnClick()` 发布 `ObjectInteractionEvent`
3. **事件处理**：`InteractionManager` 接收事件并开始交互
4. **玩家移动**：玩家自动寻路到目标位置
5. **执行采集**：到达目标后自动执行 `OnHarvest()` 方法
6. **物品处理**：掉落物品进入背包或创建地面掉落物

### 关键组件依赖
```csharp
// GameMain.cs 中的初始化顺序
var inputManager = InputManager.Instance;
var clockModel = ClockModel.Instance;
var packageModel = PackageModel.Instance;

// 初始化交互管理器 - 确保交互系统可用
if (InteractionManager.Instance == null)
{
    var interactionManagerGO = new GameObject("InteractionManager");
    interactionManagerGO.AddComponent<InteractionManager>();
}

var saveModel = SaveModel.Instance;
```

## 最佳实践

### 1. 直接采集物（DirectHarvestable）
**适用于**：草、花等可直接收获的物品

```csharp
// 在Inspector中设置
[SerializeField] private int _itemId = 1001;  // 草的物品ID

// 配置表Source.csv示例
ID,DropItemId,DropCount,DropRate,HarvestTime,DestroyAfterHarvest,InteractionRange,ActionType
1001,2001,1,1.0,0.0,true,2.0,Pull
```

**使用方法**：
1. 点击地图上的草/花
2. 玩家自动寻路到附近
3. 自动采集，物品消失，获得配置表中指定的物品

### 2. 重复采集物（RepeatableHarvestable）
**适用于**：浆果丛、果树等可多次收获的物品

```csharp
// 在Inspector中设置
[SerializeField] private int _itemId = 1002;  // 浆果丛的物品ID
[SerializeField] private GameObject _harvestableVisual;  // 浆果视觉表现

// 配置表Source.csv示例
ID,DropItemId,DropCount,DropRate,MaxHarvestCount,RegrowTime,InteractionRange,ActionType
1002,2002,1,1.0,3,300.0,2.0,Pick
```

**使用方法**：
1. 点击有浆果的浆果丛
2. 玩家寻路过去采集浆果
3. 浆果数量减少，全部采集完后开始重新生长
4. 一段时间后浆果重新长出，可再次采集

### 3. 工具采集物（ToolRequiredHarvestable）
**适用于**：树木、矿石等需要工具破坏的物品

```csharp
// 在Inspector中设置
[SerializeField] private int _itemId = 1003;  // 树木的物品ID
[SerializeField] private GameObject _normalState;    // 正常状态模型
[SerializeField] private GameObject _harvestedState; // 倒下状态模型

// 配置表Source.csv示例
ID,MaxHealth,DropItemId,MinDropCount,MaxDropCount,DropRate,RequiresTool,RequiredToolType,InteractionRange,ActionType
1003,100.0,2003,2,4,1.0,true,Axe,3.0,Chop,0.5
```

**使用方法**：
1. 使用斧头攻击树木直到倒下（血量归零）
2. 点击倒下的树
3. 玩家寻路过去采集木头
4. 根据配置表随机掉落2-4个木头

### 4. 地面掉落物拾取
```csharp
// 创建掉落物
GameObject droppedItemGO = new GameObject("DroppedItem");
var directHarvestable = droppedItemGO.AddComponent<DirectHarvestable>();
directHarvestable.SetItemId(itemId);
directHarvestable.SetDropCount(count);
```

**使用方法**：
1. 点击地面的掉落物
2. 玩家寻路过去拾取
3. 物品进入背包，掉落物消失

### 5. 自定义采集物
```csharp
public class CustomHarvestable : HarvestableObject
{
    protected override void Awake()
    {
        base.Awake();
        
        // 配置自定义的掉落和行为
        _drops.Add(new DropItem(customItemId, minCount, maxCount, dropRate));
        _interactionRange = customRange;
    }
    
    protected override void OnHarvestComplete(IAttacker harvester)
    {
        // 自定义采集完成逻辑
        DoCustomBehavior();
    }
}
```

## 注意事项

### 兼容性保证
- 所有现有的 `Pull()`, `Harvest()` 等方法都保持可用
- 现有的预制体和场景无需修改，自动获得新功能
- 原有的特效和动画系统完全兼容

### 性能优化
- 交互检测使用合理的更新频率（默认0.1秒）
- 掉落物自动超时清理（默认300秒）
- 使用对象池管理频繁创建的掉落物

### 配置要求
- 采集物需要添加Collider组件用于射线检测
- 掉落物会自动添加SphereCollider
- InteractionManager会在GameMain中自动创建

### 事件系统集成
```csharp
// 监听采集事件
EventManager.Instance.Subscribe<ObjectInteractionEvent>(OnObjectInteraction);

// 监听物品变化事件
EventManager.Instance.Subscribe<ItemChangeEvent>(OnItemChanged);
```

## 配置表结构

### Source.csv 配置表字段说明
```csv
ID,DropItemId,DropCount,DropRate,HarvestTime,DestroyAfterHarvest,InteractionRange,ActionType,MaxHealth,MinDropCount,MaxDropCount,RequiresTool,RequiredToolType,MaxHarvestCount,RegrowTime
```

**字段说明**：
- `ID`: 物品ID（必须）
- `DropItemId`: 掉落的物品ID，默认为当前ID
- `DropCount`: 掉落数量（DirectHarvestable, RepeatableHarvestable）
- `MinDropCount/MaxDropCount`: 掉落数量范围（ToolRequiredHarvestable）
- `DropRate`: 掉落概率（0.0-1.0）
- `HarvestTime`: 采集时间（秒）
- `DestroyAfterHarvest`: 采集后是否销毁
- `InteractionRange`: 交互范围
- `ActionType`: 动作类型（Pull, Pick, Chop, Mine等）
- `MaxHealth`: 最大血量（ToolRequiredHarvestable）
- `RequiresTool`: 是否需要工具
- `RequiredToolType`: 需要的工具类型（Axe, Chisel等）
- `MaxHarvestCount`: 最大采集次数（RepeatableHarvestable）
- `RegrowTime`: 重新生长时间（RepeatableHarvestable）

## 扩展指南

### 添加新的采集物
1. **选择合适的脚本类型**：
   - 一次性采集 → `DirectHarvestable`
   - 可重复采集 → `RepeatableHarvestable`  
   - 需要工具 → `ToolRequiredHarvestable`

2. **配置GameObject**：
   - 添加对应的脚本组件
   - 设置 `_itemId` 为配置表中的ID
   - 配置必要的视觉组件（如_harvestableVisual）

3. **更新配置表**：
   - 在 `Source.csv` 中添加对应ID的配置
   - 设置掉落物品、数量、概率等参数

### 转换现有采集物
- **Grass** → 使用 `DirectHarvestable`，设置 `_itemId = 1001`
- **BerryBush** → 使用 `RepeatableHarvestable`，设置 `_itemId = 1002`
- **Tree** → 使用 `ToolRequiredHarvestable`，设置 `_itemId = 1003`

### 性能优化
- 配置表在启动时一次性加载，运行时高效查询
- 视觉组件按需激活/停用，减少渲染开销
- 重新生长计时器仅在需要时更新