# 掉落系统技术文档

## 简介

掉落系统提供统一的物品掉落功能，支持所有可损坏对象（怪物、建筑、采集物等）在死亡或被破坏时掉落物品。系统通过 `IDroppable` 接口和 `DamageableObject` 基类实现，支持掉落概率、数量范围和优先背包添加等功能。

## 详细接口

### IDroppable 掉落接口
**位置**: `Assets/Scripts/Object/Interface/IDroppable.cs`

```csharp
public interface IDroppable
{
    List<DropItem> GetDropItems();              // 获取掉落物品列表
    void ProcessDrops(IAttacker killer);        // 处理掉落逻辑
    void CreateDroppedItem(int itemId, int count); // 创建掉落物品到世界
    bool IsDropEnabled { get; }                 // 是否启用掉落功能
}
```

### DamageableObject 掉落功能
**位置**: `Assets/Scripts/Object/Base/DamageableObject.cs`
**组件类型**: MonoBehaviour基类，需要继承使用

**掉落控制**:
```csharp
public virtual void EnableDrop()                // 启用掉落功能
public virtual void DisableDrop()               // 禁用掉落功能
public virtual bool IsDropEnabled               // 检查掉落是否启用
```

**掉落配置**:
```csharp
public virtual void AddDrop(DropItem dropItem)  // 添加掉落物品
public virtual void AddDrop(int itemId, int minCount, int maxCount, float dropRate = 1.0f) // 简化添加
public virtual void ClearDrops()                // 清除所有掉落物品
public virtual List<DropItem> GetDropItems()    // 获取掉落列表
```

**掉落处理**:
```csharp
public virtual void ProcessDrops(IAttacker killer)         // 处理掉落逻辑
public virtual void CreateDroppedItem(int itemId, int count) // 创建掉落物
```

### DropItem 掉落数据结构
**位置**: `Assets/Scripts/Object/Data/HarvestData.cs`

```csharp
[System.Serializable]
public struct DropItem
{
    public int itemId;          // 物品ID
    public int minCount;        // 最小掉落数量
    public int maxCount;        // 最大掉落数量
    public float dropRate;      // 掉落概率(0.0-1.0)
    
    public int GetActualDropCount(); // 计算实际掉落数量
}
```

### DropManager 掉落管理器
**位置**: `Assets/Scripts/Manager/DropManager.cs`
**组件类型**: 工具类，静态方法调用

```csharp
// 批量设置掉落
public static void SetupDrops(IDroppable droppable, params DropItem[] drops)

// 设置单个掉落
public static void SetupSingleDrop(IDroppable droppable, int itemId, int minCount, int maxCount, float dropRate)

// 检查是否有掉落
public static bool HasDrops(IDroppable droppable)
```

### 死亡处理流程
```csharp
protected virtual void OnDeath()
{
    // 1. 处理死亡掉落（如果启用）
    if (IsDropEnabled && _lastAttacker != null)
    {
        ProcessDrops(_lastAttacker);
    }
    
    // 2. 调用子类自定义死亡逻辑
    OnDeathCustom();
}

protected virtual void OnDeathCustom()
{
    // 子类重写实现死亡逻辑
}
```

## 最佳实践

### 1. Monster 怪物掉落设置
**挂载脚本**: `Monster.cs` 继承自 `DamageableObject`

```csharp
public class Monster : CombatEntity
{
    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Monster);
        
        // 添加掉落物品
        AddDrop(10001, 1, 2, 0.8f);  // 80%几率掉落1-2个物品10001
        AddDrop(10002, 1, 1, 0.3f);  // 30%几率掉落1个物品10002
        
        // 启用掉落功能
        EnableDrop();
    }
}
```

### 2. 自定义掉落物对象
**挂载脚本**: 继承 `DamageableObject` 的自定义脚本

```csharp
public class DestructibleCrate : DamageableObject
{
    public override float MaxHealth => 50f;
    public override float Defense => 0f;
    public override bool CanInteract => CurrentHealth > 0;
    public override float GetInteractionRange() => 3f;
    
    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Building);
        
        // 设置掉落物品
        AddDrop(20001, 1, 3, 0.7f);  // 木材
        AddDrop(20002, 1, 1, 0.3f);  // 稀有材料
        
        EnableDrop(); // 启用掉落功能
    }
    
    protected override void OnDeathCustom()
    {
        // 播放破坏特效
        PlayDestroyEffect();
        
        // 延迟销毁GameObject
        Destroy(gameObject, 1f);
    }
}
```

### 3. HarvestableObject 采集物掉落
**挂载脚本**: `HarvestableObject.cs` 继承自 `Building`

```csharp
public class HarvestableObject : Building
{
    protected override void Awake()
    {
        base.Awake();
        SetObjectType(ObjectType.Item);
        
        // 从配置表加载掉落数据
        LoadDropConfig();
    }
    
    private void LoadDropConfig()
    {
        var dropReader = ConfigManager.Instance.GetReader("Drop");
        
        // 读取掉落配置
        for (int i = 1; i <= 5; i++)
        {
            int itemId = dropReader.GetValue(configId, $"DropItemId{i}", 0);
            int count = dropReader.GetValue(configId, $"DropCount{i}", 1);
            float chance = dropReader.GetValue(configId, $"DropChance{i}", 1.0f);
            
            if (itemId > 0)
            {
                AddDrop(itemId, count, count, chance);
            }
        }
        
        EnableDrop();
    }
}
```

### 4. 使用 DropManager 批量设置
```csharp
// 为任何掉落对象批量设置掉落
var droppableObject = GetComponent<IDroppable>();
DropManager.SetupDrops(droppableObject, 
    new DropItem(10001, 1, 2, 0.8f),  // 普通物品
    new DropItem(10002, 1, 1, 0.3f),  // 稀有物品
    new DropItem(10003, 2, 5, 0.5f)   // 材料
);

// 快速设置单个掉落
DropManager.SetupSingleDrop(droppableObject, 15001, 1, 3, 0.9f);

// 检查对象是否设置了掉落
if (DropManager.HasDrops(droppableObject))
{
    Debug.Log("此对象被破坏时会掉落物品");
}
```

### 5. Monster 扩展方法示例
```csharp
public static class MonsterDropExtensions
{
    // 设置标准掉落（根据等级）
    public static void SetStandardDrops(this Monster monster, int level)
    {
        // 基础掉落
        monster.AddDrop(16001, 1, 2, 0.6f); // 经验宝石
        
        // 根据等级设置额外掉落
        if (level >= 5)
        {
            monster.AddDrop(15001, 1, 1, 0.3f); // 高级材料
        }
        
        monster.EnableDrop();
    }
    
    // 设置BOSS掉落
    public static void SetBossDrops(this Monster monster)
    {
        monster.AddDrop(18001, 1, 1, 1.0f);   // BOSS武器
        monster.AddDrop(16001, 3, 5, 1.0f);   // 大量经验
        monster.AddDrop(17001, 1, 2, 0.8f);   // 稀有装备
        
        monster.EnableDrop();
    }
}
```

### 6. 掉落处理完整流程
```csharp
public virtual void ProcessDrops(IAttacker killer)
{
    foreach (var drop in _drops)
    {
        // 计算实际掉落数量（包含概率计算）
        int actualCount = drop.GetActualDropCount();
        if (actualCount <= 0) continue;
        
        // 优先尝试添加到玩家背包
        if (TryAddToPlayerBag(killer, drop.itemId, actualCount))
        {
            ShowItemGainHint(drop.itemId, actualCount);
        }
        else
        {
            // 背包已满，创建掉落物到世界
            CreateDroppedItem(drop.itemId, actualCount);
        }
    }
}

private bool TryAddToPlayerBag(IAttacker killer, int itemId, int count)
{
    if (killer != null && killer == Player.Instance)
    {
        return PackageModel.Instance.AddItem(itemId, count);
    }
    return false;
}
```

## 注意事项

### 使用要点
1. **默认状态**: 掉落功能默认关闭，必须调用 `EnableDrop()` 启用
2. **继承要求**: 使用掉落功能的对象必须继承自 `DamageableObject`
3. **攻击者记录**: 系统自动记录最后攻击者 `_lastAttacker` 用于掉落处理
4. **背包优先**: 优先尝试将掉落物添加到玩家背包，失败后才创建世界掉落物
5. **概率计算**: `dropRate` 为 0.0-1.0 范围，1.0 表示 100% 掉落

### 配置表集成
**Drop.csv 格式示例**:
```csv
Id,Name,DropItemId1,DropCount1,DropChance1,DropItemId2,DropCount2,DropChance2
30001,树,13001,3,0.8,11001,1,0.3
30005,浆果丛,12001,2,1.0,,,,
5000,僵尸,16001,1,0.3,15001,1,0.5
```

### 性能考虑
- 掉落处理只在对象死亡时执行，不会影响运行时性能
- 建议使用对象池管理掉落物 GameObject，避免频繁创建销毁
- 大量掉落时可考虑批量处理和延迟创建

### 调试支持
```csharp
// 调试：显示对象的所有掉落设置
var drops = GetDropItems();
foreach (var drop in drops)
{
    Debug.Log($"掉落物: {drop.itemId}, 数量: {drop.minCount}-{drop.maxCount}, 概率: {drop.dropRate:P}");
}
```

## 系统架构

### 继承关系
```
IDroppable (接口)
    ↑
DamageableObject (基类实现)
    ↑
├─Monster (怪物) - 挂载到怪物GameObject
├─Building (建筑) - 挂载到建筑GameObject
│   ↑
│   └─HarvestableObject (采集物) - 挂载到采集物GameObject
└─自定义掉落对象 - 继承DamageableObject
```

### 掉落流程
```
TakeDamage() → 记录攻击者 (_lastAttacker)
    ↓
CurrentHealth <= 0
    ↓
OnDeath() → IsDropEnabled? → ProcessDrops()
    ↓              ↓              ↓
OnDeathCustom()   跳过掉落      计算掉落 → 尝试添加到背包
    ↓                              ↓
子类自定义逻辑                    失败 → CreateDroppedItem()
```

### 工具类支持
- **DropManager**: 静态工具类，提供掉落设置的便捷方法
- **MonsterDropExtensions**: 扩展方法，为Monster提供标准掉落设置 